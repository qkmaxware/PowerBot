using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ModuleCore.Modules;

namespace DiscordBot.Bot {
    class CommandMatrix {

        private Dictionary<string, IModule> modules = new Dictionary<string, IModule>();
        private HashSet<IModule> disabledModules = new HashSet<IModule>();
        private Dictionary<xtype, List<KeyValuePair<IModule, ICommand>>> commands = new Dictionary<xtype, List<KeyValuePair<IModule, ICommand>>>();

        public void Install(IModule mod) {
            string modid = mod.GetModuleName().Value + "#" + mod.GetUid();
            if (modules.ContainsKey(modid))
                return;
            modules[modid] = (mod);
            foreach (ICommand c in mod.GetCommands()) {
                if (commands.ContainsKey(c.GetName())) {
                    bool add = true;
                    foreach (KeyValuePair<IModule, ICommand> pair in commands[c.GetName()]) {
                        if (pair.Value == c) {
                            add = false;
                            break;
                        }
                    }
                    if(add)
                        commands[c.GetName()].Add(new KeyValuePair<IModule, ICommand>(mod, c));
                }
                else {
                    commands[c.GetName()] = new List<KeyValuePair<IModule, ICommand>>();
                    commands[c.GetName()].Add(new KeyValuePair<IModule, ICommand>(mod, c));
                }
            }
        }

        public string[] getInstalled() {
            string[] ids = new string[this.modules.Count];
            int i = 0;
            foreach (string name in this.modules.Keys) {
                ids[i++] = name;
            }
            return ids;
        }

        public void Enable(IModule mod, bool enable) {
            string modid = mod.GetModuleName().Value + "#" + mod.GetUid();
            if (modules.ContainsKey(modid)) {
                if (enable)
                    this.disabledModules.Remove(mod);
                else
                    this.disabledModules.Add(mod);
            }
        }

        public void GetCommand(out List<KeyValuePair<IModule, ICommand>> cmds, xtype xtype) {
            if (!commands.ContainsKey(xtype)) {
                //Command does not exist in any module
                cmds = null;
                return;
            }

            List<KeyValuePair<IModule, ICommand>> cmd = commands[xtype];
            List<KeyValuePair<IModule, ICommand>> allowed = new List<KeyValuePair<IModule, ICommand>>();
            if (cmd.Count > 0) {
                foreach(KeyValuePair<IModule, ICommand> p in cmd) {
                    if (!this.disabledModules.Contains(p.Key))
                        allowed.Add(p);
                }
                //Multiple options and no module specified
                cmds = allowed;
                return;
            }

            //Match command with module
            KeyValuePair<IModule, ICommand> r = commands[xtype][0];

            if (!this.disabledModules.Contains(r.Key))
                allowed.Add(r);

            cmds = allowed;
        }

    }
}
