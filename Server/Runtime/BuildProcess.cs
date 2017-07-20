using System;
using System.Collections.Generic;
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
			get { return IsDone && Tasks.All(task => task.IsSuccess); }
		}
		
		public BuildProcess(Build build) {
			Name  = build.Name;
			Tasks = build.Nodes.Select(node => new BuildTask(node)).ToList();
		}

		BuildTask FindTask(BuildNode node) {
			return Tasks.FirstOrDefault(task => task.Node == node);
		}
		
		public void StartTask(BuildNode node) {
			var task = FindTask(node);
			if (task != null) {
				if (!IsStarted) {
					BuildStarted?.Invoke();
				}
				task.Start();
				TaskStarted?.Invoke(task);
			}
		}

		public void DoneTask(BuildNode node, bool isSuccess, string message) {
			var task = FindTask(node);
			if (task != null) {
				task.Done(isSuccess, message);
				TaskDone?.Invoke(task);
				if (IsDone) {
					BuildDone?.Invoke();
				}
			}
		}

		public void Abort() {
			BuildDone?.Invoke();
		}
	}
}