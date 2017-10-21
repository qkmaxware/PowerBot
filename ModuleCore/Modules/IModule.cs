using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleCore.Modules {
    public interface IModule {

        xtype GetModuleName();
        long GetUid();
        string GetAuthor();
        string GetDescription();

        IScheduledTask[] GetScheduledEvents();
        ICommand[] GetCommands();

    }
}
