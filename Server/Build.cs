using System;
using System.Collections.Generic;
using System.Linq;

namespace Server {
	public class Build {

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
			get { return IsDone && Tasks.All(task => task.IsSuccess); }
		}
		
		public Build(BuildConfig config) {
			Name  = config.Name;
			Tasks = config.Tasks.Select(taskName => new BuildTask(taskName)).ToList();
		}

		BuildTask FindTask(string taskName) {
			return Tasks.FirstOrDefault(task => task.Name == taskName);
		}
		
		public void StartTask(string taskName) {
			var task = FindTask(taskName);
			if (task != null) {
				if (!IsStarted) {
					BuildStarted?.Invoke();
				}
				task.Start();
				TaskStarted?.Invoke(task);
			}
		}

		public void DoneTask(string taskName, bool isSuccess, string message = "") {
			var task = FindTask(taskName);
			if (task != null) {
				task.Done(isSuccess, message);
				TaskDone?.Invoke(task);
				if (IsDone) {
					BuildDone?.Invoke();
				}
			}
		}
	}
}