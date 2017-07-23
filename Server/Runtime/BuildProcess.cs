using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Server.BuildConfig;

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
		public TimeSpan  WorkTime    { get; private set; }
		public bool      IsAborted   { get; private set; }

		DateTime  _startTime;
		DateTime  _endTime;
		
		public BuildProcess(Build build) {
			Name  = build.Name;
			Tasks = build.Nodes.Select(node => new BuildTask(node)).ToList();
		}

		BuildTask FindTask(BuildNode node) {
			return Tasks.FirstOrDefault(task => task.Node == node);
		}

		public void StartBuild(DateTime time) {
			Debug.WriteLine($"BuildProcess.StartBuild: {time}");
			_startTime = time;
			BuildStarted?.Invoke();
		}
		
		public void StartTask(BuildNode node) {
			Debug.WriteLine($"BuildProcess.StartTask: Node: {node.Name}");
			var task = FindTask(node);
			if (task != null) {
				Debug.WriteLine($"BuildProcess.StartTask: Task: {task.GetHashCode()}");
				CurrentTask = task;
				task.Start();
				TaskStarted?.Invoke(task);
			}
		}

		public void DoneTask(DateTime time, bool isSuccess, string message, string result) {
			Debug.WriteLine($"BuildProcess.DoneTask: {time}");
			var task = CurrentTask;
			if (task != null) {
				Debug.WriteLine($"BuildProcess.DoneTask: Task: {task.GetHashCode()}");
				task.Done(isSuccess, message, result);
				TaskDone?.Invoke(task);
				CurrentTask = null;
				if (IsDone || IsAborted) {
					DoneBuild(time);
				}
			}
		}

		public void Abort(DateTime time) {
			Debug.WriteLine($"BuildProcess.Abort: {time}");
			IsAborted = true;
			if (CurrentTask == null) {
				DoneBuild(time);
			}
		}

		void DoneBuild(DateTime time) {
			Debug.WriteLine($"BuildProcess.DoneBuild: {time}, isAborted: {IsAborted}");
			_endTime = time;
			WorkTime = _endTime - _startTime;
			BuildDone?.Invoke();
		}
	}
}