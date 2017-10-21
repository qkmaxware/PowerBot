using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ModuleCore.Modules;
using System.Timers;

namespace DiscordBot.Bot {
    class Scheduler {

        private List<IScheduledTask> tasks = new List<IScheduledTask>();
        private List<Timer> task_timers = new List<Timer>();

        public void Register(IScheduledTask task) {
            if (tasks.Contains(task))
                return;
            //Add task
            tasks.Add(task);
            //Add Timer
            ScheduleTimer(task);
        }

        void ScheduleTimer(IScheduledTask task) {
            DateTime now = DateTime.Now;
            DateTime fireAt = now.Add(task.GetRepeatTime());

            double ticktime = task.GetRepeatTime().TotalMilliseconds;
            Timer t = new Timer(ticktime);
            t.Elapsed += new ElapsedEventHandler((object sender, ElapsedEventArgs e) => {
                task.DoTask();
            });
            t.Start();
            task_timers.Add(t);
        }

        void Kill() {
            foreach (Timer t in this.task_timers) {
                t.Stop();
            }
        }

    }
}
