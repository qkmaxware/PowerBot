using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ModuleCore.Modules;
using FlatFileDatabase;

namespace DiscordBot.Bot.DefaultModules {
    class TaskModule : IModule {

        private class TimedChecker : IScheduledTask {
            private Database db;
            public TimedChecker(Database db) {
                this.db = db;
            }

            public void DoTask() {
                //long future = DateTimeOffset.UtcNow.ToUniversalTime().UtcTicks + new TimeSpan(0, 15, 0).Ticks;
                //Query check = Query.Compile("SELECT * FROM tasks WHERE time < "+future);
                //Table table = db.Query(check);
            }

            public TimeSpan GetNextTime() {
                //5 min repeat
                return new TimeSpan(0, 5, 0);
            }
        }

        private string author = "Colin";
        private string description = "";
        private xtype name = xtype.Register("Task-Manager");
        private long uid = "Task-Manager".GetHashCode();
        private Database db;

        private ICommand[] cmds;

        private IScheduledTask[] tasks = new IScheduledTask[] {

        };

        public TaskModule() {
            db = Database.Create("DB_taskmgr.json");
            if (db.IsEmpty()) {
                //First time setup
                Query c = Query.Compile("CREATE TABLE tasks (uid, creation, time, event)");
                db.Query(c);
            }

            cmds = new ICommand[] {
                new Command("event-add", (ctx) => {
                    string uid = ctx.GetSenderId();
                    long now = DateTimeOffset.UtcNow.ToUniversalTime().UtcTicks;
                    string scheduledTime = "Na";
                    Query add = Query.Compile("INSERT INTO tasks (uid, creation, time, event) VALUES '"+uid+"', "+now+", '"+scheduledTime+"', '"+ctx.GetMessage()+"'");
                    db.Query(add);
                    ctx.ReplyToChannel(ctx.GetSenderMention()+" your task has be scheduled");
                }),
                new Command("event-rm", (ctx) => {

                }),
                new Command("event-ls", (ctx) => {
                    string uid = ctx.GetSenderId();
                    Query select = Query.Compile("SELECT time, event FROM tasks WHERE uid = '"+uid+"'");
                    Table t = db.Query(select);
                    string send = ctx.GetSenderMention()+" your tasks include: \n";
                    foreach (Table.Row row in t.rows) {
                        send += row.values["time"]+": "+row.values["event"]+",\n";
                    }
                    ctx.ReplyToChannel(send);
                })
            };
        }

        public string GetAuthor() {
            return author;
        }

        public ICommand[] GetCommands() {
            return cmds;
        }

        public string GetDescription() {
            return description;
        }

        public xtype GetModuleName() {
            return name;
        }

        public IScheduledTask[] GetScheduledEvents() {
            return tasks;
        }

        public long GetUid() {
            return this.uid;
        }
    }
}
