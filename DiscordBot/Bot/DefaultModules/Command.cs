using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ModuleCore.Modules;

namespace DiscordBot.Bot.DefaultModules
{
    public class Command : ICommand
    {
        private xtype name;
        private string desc;
        Action<ICommandContext> action;
        public Command(string name, Action<ICommandContext> action, string desc = "")
        {
            this.name = xtype.Register(name);
            this.action = action;
            this.desc = desc;
        }

        public string GetHelp()
        {
            return desc;
        }

        public xtype GetName()
        {
            return name;
        }

        public void Run(ICommandContext ctx)
        {
            action.Invoke(ctx);
        }
    }
}
