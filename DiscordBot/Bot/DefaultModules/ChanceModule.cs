using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ModuleCore.Modules;

namespace DiscordBot.Bot.DefaultModules
{
    class ChanceModule : IModule
    {

        private string author = "Colin";
        private string description = "Contains random number generation commands.";
        private xtype type = xtype.Register("Chance-Mod");
        private long uid = "Chance-Mod".GetHashCode();

        private Random rng = new Random();

        private ICommand[] cmds;

        public ChanceModule() {

            cmds = new ICommand[] {
                new Command("coin", (ctx) => {
                    int v = rng.Next(2);
                    string res;
                    if(v == 0)
                        res = "Heads";
                    else
                        res = "Tails";

                    ctx.ReplyToChannel(ctx.GetSenderMention() + " The coin landed on " + res);
                }),
                new Command("d4", (ctx) => {
                    int v = rng.Next(4) + 1;

                    ctx.ReplyToChannel(ctx.GetSenderMention() + " " + v);
                }),
                new Command("d6", (ctx) => {
                    int v = rng.Next(6) + 1;

                    ctx.ReplyToChannel(ctx.GetSenderMention() + " " + v);
                }),
                new Command("d8", (ctx) => {
                    int v = rng.Next(8) + 1;

                    ctx.ReplyToChannel(ctx.GetSenderMention() + " " + v);
                }),
                new Command("d10", (ctx) => {
                    int v = rng.Next(10) + 1;

                    ctx.ReplyToChannel(ctx.GetSenderMention() + " " + v);
                }),
                new Command("d12", (ctx) => {
                    int v = rng.Next(12) + 1;

                    ctx.ReplyToChannel(ctx.GetSenderMention() + " " + v);
                }),
                new Command("d20", (ctx) => {
                    int v = rng.Next(20) + 1;

                    ctx.ReplyToChannel(ctx.GetSenderMention() + " " + v);
                }),
                new Command("rand", (ctx) => {
                    int i = 0;
                    bool success = int.TryParse(ctx.GetMessage(), out i);
                    if(!success) {
                        ctx.ReplyToChannel(ctx.GetSenderMention() + " Invalid numberic parameter");
                    }
                    else {
                        int v = rng.Next(i) + 1;
                        ctx.ReplyToChannel(ctx.GetSenderMention() + " " + v);
                    }
                })
            };

        }

        public string GetAuthor()
        {
            return author;
        }

        public ICommand[] GetCommands()
        {
            return cmds;
        }

        public string GetDescription()
        {
            return description;
        }

        public xtype GetModuleName()
        {
            return type;
        }

        public IScheduledTask[] GetScheduledEvents()
        {
            return new IScheduledTask[0];
        }

        public long GetUid()
        {
            return uid;
        }
    }
}
