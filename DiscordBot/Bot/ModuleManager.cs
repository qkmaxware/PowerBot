using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ModuleCore.Modules;

namespace DiscordBot.Bot {
    class ModuleManager {

        public class CommandInfo{
            public ICommand cmd;
            public IModule mod;

            public override bool Equals(object obj) {
                return cmd == obj;
            }
            public override int GetHashCode() {
                return cmd.GetHashCode();
            }
        }

        //All mods available
        private HashSet<IModule> available_mods = new HashSet<IModule> ();

        //Installed (enabled mods)
        private HashSet<IModule> installed_mods = new HashSet<IModule>();
        private Dictionary<xtype, List<CommandInfo>> installed_cmds = new Dictionary<xtype, List<CommandInfo>>();

        /// <summary>
        /// Add a mod to be managed
        /// </summary>
        /// <param name="mod"></param>
        /// <param name="autoinstall"></param>
        /// <returns></returns>
        public bool AddModule(IModule mod, bool autoinstall = false) {
            if (available_mods.Contains(mod))
                return false;
            available_mods.Add(mod);

            if (autoinstall)
                EnableMod(mod);

            return true;
        }

        /// <summary>
        /// Uninstall and remove a mod from management
        /// </summary>
        /// <param name="mod"></param>
        /// <returns></returns>
        public bool RemoveModule(IModule mod) {
            if (!available_mods.Contains(mod))
                return false;
            DisableMod(mod);
            available_mods.Remove(mod);
            return true;
        }

        /// <summary>
        /// Enable or disable a mod
        /// </summary>
        /// <param name="mod"></param>
        /// <param name="enable"></param>
        public void EnableMod(IModule mod, bool enable) {
            if (enable)
                EnableMod(mod);
            else
                DisableMod(mod);
        }

        /// <summary>
        /// Enable (install) an available mod
        /// </summary>
        /// <param name="mod"></param>
        public void EnableMod(IModule mod) {
            //Not added
            if (!available_mods.Contains(mod))
                return;

            //Already installed
            if (installed_mods.Contains(mod))
                return;

            installed_mods.Add(mod);

            //Add commands to installed list
            ICommand[] cmds = mod.GetCommands();
            foreach (ICommand cmd in cmds) {
                List<CommandInfo> ls;
                if (installed_cmds.ContainsKey(cmd.GetName())) {
                    ls = installed_cmds[cmd.GetName()];
                }
                else {
                    ls = new List<CommandInfo>();
                    installed_cmds[cmd.GetName()] = ls;
                }

                CommandInfo info = new CommandInfo();
                info.cmd = cmd;
                info.mod = mod;
                ls.Add(info);
            }

            //Add anything else we need
        }

        /// <summary>
        /// Disable (uninstall) an available mod
        /// </summary>
        /// <param name="mod"></param>
        public void DisableMod(IModule mod) {
            //Not added
            if (!available_mods.Contains(mod))
                return;

            //Not installed, can't uninstall
            if (!installed_mods.Contains(mod))
                return;

            installed_mods.Remove(mod);

            //Remove commands from installed list
            ICommand[] cmds = mod.GetCommands();
            foreach (ICommand cmd in cmds) {
                List<CommandInfo> ls;
                if (installed_cmds.ContainsKey(cmd.GetName())) {
                    ls = installed_cmds[cmd.GetName()];
                }
                else {
                    ls = new List<CommandInfo>();
                    installed_cmds[cmd.GetName()] = ls;
                }

                CommandInfo info = new CommandInfo();
                info.cmd = cmd;
                info.mod = mod;

                ls.Remove(info);
            }

            //Remove anything else we need to

        }

        /// <summary>
        /// List of all available mods
        /// </summary>
        /// <returns></returns>
        public IModule[] GetAvailableMods() {
            return this.available_mods.ToArray();
        }

        /// <summary>
        /// List of all installed (enabled)
        /// </summary>
        /// <returns></returns>
        public IModule[] GetInstalledMods() {
            return this.installed_mods.ToArray();
        }

        /// <summary>
        /// List all commands for a particular xtype
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public List<CommandInfo> GetCommandsForXtype(xtype type) {
            if (this.installed_cmds.ContainsKey(type)) {
                return this.installed_cmds[type];
            }
            else {
                return new List<CommandInfo>();
            }
        }

    }
}
