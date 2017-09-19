using System;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using Server.BuildConfig;
using Server.Runtime;
using System.Text;
using System.Linq;
using Server.Services.Stats;

namespace Server.Services {
	public class StatService : IService {
		public string ContainerPath { get; private set; }

		ILogger _logger;

		BuildServer   _server;
		BuildProcess  _process;
		StatContainer _container;

		public StatService(string containerPath, LoggerFactory loggerFactory) {
			ContainerPath = containerPath;
			_logger = loggerFactory.CreateLogger<StatService>();
		}

		public bool TryInit(BuildServer server, Project project) {
			_server = server;
			_server.OnInitBuild += OnInitBuild;
			_server.AddCommand(null, "stats", "show stats about all builds", OnStatsRequested);
			LoadContainer();
			_logger.LogDebug($"Container: {_container.Builds.Count} builds");
			return true;
		}

		XmlSerializer CreateSerializer() {
			return new XmlSerializer(typeof(StatContainer));
		}

		void LoadContainer() {
			if ( !File.Exists(ContainerPath) ) {
				_container = new StatContainer();
				_logger.LogDebug($"Not found stat file at '{ContainerPath}', initialize as empty.");
				return;
			}
			var serializer = CreateSerializer();
			using ( var stream = new FileStream(ContainerPath, FileMode.OpenOrCreate) ) {
				try {
					_container = serializer.Deserialize(stream) as StatContainer;
				} catch (Exception e) {
					_logger.LogError($"Can't load stat file from '{ContainerPath}': \"{e}\"");
					_container = new StatContainer();
				}
			}
		}

		void SaveContainer() {
			_logger.LogDebug("SaveContainer");
			var serializer = CreateSerializer();
			using ( var stream = new FileStream(ContainerPath, FileMode.OpenOrCreate) ) {
				try {
					serializer.Serialize(stream, _container);
				} catch ( Exception e ) {
					_logger.LogError($"Can't save stat file to '{ContainerPath}': \"{e}\"");
				}
			}
		}

		private void OnInitBuild(BuildProcess process) {
			_logger.LogDebug("OnInitBuild");
			_process = process;
			process.BuildDone += OnBuildDone;
		}

		private void OnBuildDone() {
			if ( _process != null ) {
				_logger.LogDebug($"OnBuildDone: {_process.Name}: {_process.IsSuccess}");
				_process.BuildDone -= OnBuildDone;
				if ( _process.IsSuccess ) {
					AddBuildStat(_process);
					SaveContainer();
				}
			}
		}

		void AddBuildStat(BuildProcess process) {
			var stat = new BuildStat(process.Name, process.StartTime, process.WorkTime);
			foreach ( var task in process.Tasks ) {
				stat.Tasks.Add(new TaskStat(task.Node.Name, task.StartTime, task.EndTime - task.StartTime));
			}
			_container.Builds.Add(stat);
		}

		void OnStatsRequested(RequestArgs args) {
			_logger.LogDebug($"OnStatsRequested ({args.Count})");
			var sb = new StringBuilder();
			string buildName = args.Count > 0 ? args[0] : null;
			sb.Append(buildName == null ? "Stats:\n" : $"Stats ({buildName}):\n");
			var table = new StatTable();
			FormatHeader(table);
			AppendBuildStats(_container.Builds, buildName, table, buildName != null);
			table.Append(sb);
			_server.RaiseCommonMessage(sb.ToString());
		}

		void FormatHeader(StatTable table) {
			table.AddNewRow("BUILD", "COUNT", "MIN", "MAX", "AVG", "LAST");
		}

		void AppendBuildStats(List<BuildStat> stats, string name, StatTable table, bool withTaskDetails) {
			if ( string.IsNullOrEmpty(name) ) {
				var builds = _server.FindBuilds();
				foreach ( var buildName in builds.Keys ) {
					var statsByName = stats.FindAll(s => IsSameName(s, buildName));
					_logger.LogDebug($"Stats for {buildName}: {statsByName.Count}");
					AppendBuildStats(statsByName, buildName, table, withTaskDetails);
				}
				return;
			}
			if ( stats.Count > 0 ) {
				var statsByName = stats.FindAll(s => IsSameName(s, name));
				AppendCommonBuildStats(statsByName, name, table);
				if ( withTaskDetails ) {
					AppendBuildTaskStats(statsByName, table);
				}
			}
		}

		void AppendCommonInfo<T>(List<T> stats, string name, StatTable table) where T : ICommonStat {
			table.AddNewRow(name);
			table.AddToRow(stats.Count.ToString());
			if ( stats.Count > 1 ) {
				var history = new List<T>(stats);
				history.Reverse();
				history = history.Skip(1).ToList();
				var min = history.Min(s => s.Duration.TotalSeconds);
				var max = history.Max(s => s.Duration.TotalSeconds);
				var avg = history.Average(s => s.Duration.TotalSeconds);
				table.AddToRow(Utils.FormatSeconds(min), Utils.FormatSeconds(max), Utils.FormatSeconds(avg));
			} else {
				table.AddToRow("", "", "");
			}
			var last = stats.Last().Duration.TotalSeconds;
			table.AddToRow(Utils.FormatSeconds(last));
		}

		void AppendCommonBuildStats(List<BuildStat> stats, string name, StatTable table) {
			AppendCommonInfo(stats, name, table);
		}

		void AppendBuildTaskStats(List<BuildStat> stats, StatTable table) {
			table.FillNewRow("---");
			var tasks = CollectTaskStatsByName(stats);
			foreach (var taskList in tasks ) {
				var taskName = taskList.First().Name;
				AppendCommonInfo(taskList, taskName, table);
			}
		}

		List<List<TaskStat>> CollectTaskStatsByName(List<BuildStat> stats) {
			var lastBuild = stats.Last();
			var tasks = new List<List<TaskStat>>();
			var taskMap = new Dictionary<string, List<TaskStat>>();
			foreach ( var build in stats ) {
				foreach ( var task in build.Tasks ) {
					if ( lastBuild.Tasks.Find(t => t.Name == task.Name) == null ) {
						continue;
					}
					List<TaskStat> taskStats;
					if ( !taskMap.TryGetValue(task.Name, out taskStats) ) {
						taskStats = new List<TaskStat>();
						tasks.Add(taskStats);
						taskMap.Add(task.Name, taskStats);
					}
					taskStats.Add(task);
				}
			}
			return tasks;
		}

		static bool IsSameName(BuildStat stat, string name) {
			return stat.Name == name;
		}

		List<BuildStat> FindBuildsByName(string name) {
			return _container.Builds.FindAll(b => IsSameName(b, name));
		}

		public bool HasStatistics(string buildName) {
			return FindBuildsByName(buildName).Count > 0;
		}

		public DateTime FindEstimateEndTime(string buildName, DateTime startTime) {
			var buildStats = FindBuildsByName(buildName);
			if ( buildStats.Count > 0 ) {
				var avgDuration = buildStats.Average(b => b.Duration.TotalSeconds);
				return startTime.AddSeconds(avgDuration);
			}
			return DateTime.MinValue;
		}
	}
}
