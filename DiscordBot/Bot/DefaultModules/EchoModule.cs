using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ModuleCore.Modules;

namespace DiscordBot.Bot.DefaultModules {
    class EchoModule : IModule {

        private class EchoCommand : ICommand {
            private xtype name = xtype.Register("echo");

            public string GetHelp() {
                return "[Package Echo-Mod] $echo <expression> - echoes your expression right back at you";
            }

            public xtype GetName() {
                return name;
            }

            public void Run(ICommandContext ctx) {
                ctx.ReplyToChannel(ctx.GetMessage());
            }
        }

        private string author = "Colin";
        private string desc = "Module containing only one command to echo back input";
        private xtype name = xtype.Register("Echo-Mod");
        private ICommand[] cmds = new ICommand[] {
            new EchoCommand()
        };
        private long uid = "Echo-Module".GetHashCode();

        public string GetAuthor() {
            return author;
        }

        public ICommand[] GetCommands() {
            return cmds;
        }

        public string GetDescription() {
            return desc;
        }

        public xtype GetModuleName() {
            return name;
        }

        public long GetUid() {
            return uid;
        }
    }
}
