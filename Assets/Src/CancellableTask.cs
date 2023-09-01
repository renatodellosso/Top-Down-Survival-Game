using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Src
{
    public class CancellableTask : Task
    {

        public class ActiveTaskEntry : IEquatable<ActiveTaskEntry>
        {
            public Task task;
            public CancellationTokenSource cancellationTokenSource;

            public ActiveTaskEntry(Task task, CancellationTokenSource cancellationTokenSource)
            {
                this.task = task;
                this.cancellationTokenSource = cancellationTokenSource;
            }

            public bool Equals(ActiveTaskEntry other)
            {
                return task == other.task;
            }
        }

        public static readonly List<ActiveTaskEntry> activeTasks = new();

        CancellationTokenSource cancellationTokenSource;

        public string name;

        public CancellableTask(string name, Action<CancellationToken> action, CancellationTokenSource cancellationTokenSource)
            : base(()=>action(cancellationTokenSource.Token), cancellationTokenSource.Token)
        {
            this.name = name;
            this.cancellationTokenSource = cancellationTokenSource;
        }

        public static new CancellableTask Run(Action action)
        {
            return Run("UnnamedTask-" + Guid.NewGuid().ToString(), (ct) => action());
        }

        public static CancellableTask Run(string name, Action<CancellationToken> action)
        {
            Utils.Log("Starting cancellable task...");

            CancellationTokenSource cancellationTokenSource = new();
            CancellableTask task = new(name, action, cancellationTokenSource);

            activeTasks.Add(new(task, cancellationTokenSource));

            task.Start();

            Utils.Log("Task started.");

            return task;
        }

        public void Cancel()
        {
            cancellationTokenSource.Cancel();
            activeTasks.Remove(new(this, cancellationTokenSource));
        }

        public static void CancelAllTasks()
        {
            Utils.Log("Cancelling all active tasks...");
            for(int i = 0; i < activeTasks.Count; i++)
            {
                ActiveTaskEntry entry = activeTasks[i];

                if (entry.task is CancellableTask cancellableTask)
                    Utils.Log($"Cancelling {cancellableTask.name}...");

                entry.cancellationTokenSource.Cancel();
            }
        }

        public new Task ContinueWith(Action<Task> action)
        {
            Task task = ContinueWith(action, cancellationTokenSource.Token);
            activeTasks.Add(new(task, cancellationTokenSource));
            return task;
        }
    }
}
