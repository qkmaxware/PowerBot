using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleCore.Modules {
    public interface ICommand {
        xtype GetName();
        string GetHelp();
        void Run(ICommandContext ctx);
    }
}
