using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ModuleCore.Collections;
using ModuleCore.Modules;
using System.Timers;
using System.Collections.Concurrent;

namespace DiscordBot.Bot {
    class Scheduler {

        private class ScheduledTask: Comparer<ScheduledTask> {
            public IScheduledTask task;

            public override int Compare(ScheduledTask x, ScheduledTask y){
                return 0;
            }
        }

        private List<IScheduledTask> tasks = new List<IScheduledTask>();
        private Heap<ScheduledTask> queue;

        public Scheduler() {
            queue = new Heap<ScheduledTask>(new ScheduledTask());
        }

        public void AddTask(IScheduledTask task) {
            tasks.Add(task);
        }

        public void RemoveTask(IScheduledTask task){
            tasks.Remove(task);
        }

        private void CheckTaskAndRun() {
            DateTime time = DateTime.Now;

            //No tasks, quit
            if (queue.IsEmpty)
                return;

            //Do I have a task...if not quit
            ScheduledTask scheduled = queue.First;

            DateTime next = scheduled.task.GetNextScheduledTime();

            //I am past my due date...do the task
            if (time >= next) {
                queue.PopFirst();
                scheduled.task.DoTask();
            }
        }

    }
}
