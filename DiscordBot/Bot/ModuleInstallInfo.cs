using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ModuleCore.Modules;

namespace DiscordBot.Bot {
    public struct ModuleInstallInfo {
        public IModule module;
        public bool enabled;
        public string name;
        public long uid;

        public string identity {
            get {
                return name + "#" + uid;
            }
        }
    }
}
