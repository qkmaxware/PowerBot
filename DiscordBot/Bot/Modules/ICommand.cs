using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Bot.Modules {
    interface ICommand {
        xtype GetName();
        string GetHelp();
        void Run(ICommandContext ctx);
    }
}
