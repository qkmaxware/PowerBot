using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Bot.Modules {
    interface IModule {

        xtype GetModuleName();
        long GetUid();
        string GetAuthor();
        string GetDescription();
        ICommand[] GetCommands();

    }
}
