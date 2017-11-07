using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleCore.Modules {
    public interface IScheduledTask {
        DateTime GetNextScheduledTime();
        void DoTask();
    }
}
