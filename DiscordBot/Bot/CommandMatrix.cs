using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DiscordBot.Bot.Modules;

namespace DiscordBot.Bot {
    class CommandMatrix {

        private HashSet<Modules.IModule> modules = new HashSet<Modules.IModule>();
        private HashSet<Modules.IModule> disabledModules = new HashSet<Modules.IModule>();
        private Dictionary<xtype, List<KeyValuePair<IModule, Modules.ICommand>>> commands = new Dictionary<xtype, List<KeyValuePair<IModule, Modules.ICommand>>>();

        public void Install(IModule mod) {
            if (modules.Contains(mod))
                return;
            modules.Add(mod);
            foreach (ICommand c in mod.GetCommands()) {
                if (commands.ContainsKey(c.GetName())) {
                    commands[c.GetName()].Add(new KeyValuePair<IModule, ICommand>(mod, c));
                }
                else {
                    commands[c.GetName()] = new List<KeyValuePair<IModule, ICommand>>();
                    commands[c.GetName()].Add(new KeyValuePair<IModule, ICommand>(mod, c));
                }
            }
        }

        public void Enable(IModule mod, bool enable) {
            if (modules.Contains(mod)) {
                if (enable)
                    this.disabledModules.Remove(mod);
                else
                    this.disabledModules.Add(mod);
            }
        }

        public void GetCommand(out List<KeyValuePair<IModule, Modules.ICommand>> cmds, xtype xtype) {
            if (!commands.ContainsKey(xtype)) {
                //Command does not exist in any module
                cmds = null;
                return;
            }

            List<KeyValuePair<IModule, Modules.ICommand>> cmd = commands[xtype];
            List<KeyValuePair<IModule, Modules.ICommand>> allowed = new List<KeyValuePair<IModule, ICommand>>();
            if (cmd.Count > 0) {
                foreach(KeyValuePair<IModule, Modules.ICommand> p in cmd) {
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
