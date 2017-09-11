using System;
using System.Collections.Generic;
using System.Linq;
using Server.BuildConfig;
using Server.Commands;
using Microsoft.Extensions.Logging;

namespace Server.Runtime {
	public class BuildProcess {

		public event Action<BuildTask> TaskStarted;
		public event Action<BuildTask> TaskDone;
		public event Action            BuildStarted; 
		public event Action            BuildDone;
		
		public string          Name  { get; }
		public List<BuildTask> Tasks { get; }

		public bool IsStarted {
			get { return Tasks.Any(task => task.IsStarted); }
		}
		
		public bool IsDone {
			get { return Tasks.All(task => task.IsDone); }
		}
		
		public bool IsSuccess {
			get { return IsDone && !IsAborted && Tasks.All(task => task.IsSuccess); }
		}

		public BuildTask CurrentTask { get; private set; }
		public DateTime  StartTime   { get; private set; }
		public DateTime  EndTime     { get; private set; }
		public TimeSpan  WorkTime    { get; private set; }
		public bool      IsAborted   { get; private set; }
		public bool      Silent      { get; private set; }


		ILogger _logger;

		public BuildProcess(LoggerFactory loggerFactory, Build build) {
			_logger = loggerFactory.CreateLogger<BuildProcess>();
			Name  = build.Name;
			Tasks = build.Nodes.Select(node => new BuildTask(loggerFactory, node)).ToList();
		}

		BuildTask FindTask(BuildNode node) {
			return Tasks.FirstOrDefault(task => task.Node == node);
		}

		public void StartBuild(DateTime time) {
			_logger.LogDebug($"BuildProcess.StartBuild: {time}");
			StartTime = time;
			BuildStarted?.Invoke();
		}
		
		public void StartTask(BuildNode node) {
			_logger.LogDebug($"BuildProcess.StartTask: Node: \"{node.Name}\"");
			var task = FindTask(node);
			if (task != null) {
				_logger.LogDebug($"BuildProcess.StartTask: Task: {task.GetHashCode()}");
				CurrentTask = task;
				task.Start();
				TaskStarted?.Invoke(task);
			}
		}

		public void DoneTask(DateTime time, CommandResult result) {
			_logger.LogDebug($"BuildProcess.DoneTask: {time}");
			var task = CurrentTask;
			if (task == null) {
				return;
			}
			_logger.LogDebug($"BuildProcess.DoneTask: Task: {task.GetHashCode()}");
			task.Done(result.IsSuccess, result.Message, result.Result);
			Silent = result.Silent;
			TaskDone?.Invoke(task);
			CurrentTask = null;
			if (IsDone || IsAborted) {
				DoneBuild(time);
			}
		}

		public void Abort(DateTime time) {
			_logger.LogDebug($"BuildProcess.Abort: {time}");
			IsAborted = true;
			if (CurrentTask == null) {
				DoneBuild(time);
			}
		}

		void DoneBuild(DateTime time) {
			_logger.LogDebug($"BuildProcess.DoneBuild: {time}, isAborted: {IsAborted}");
			EndTime = time;
			WorkTime = EndTime - StartTime;
			BuildDone?.Invoke();
		}
	}
}