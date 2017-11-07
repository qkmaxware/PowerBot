using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DiscordBot.Bot;
using SimpleWebServer.SpecialWebPages;
using FlatFileDatabase;

namespace DiscordBot {
    class Program {
        public enum Mode {
            Debug, Release
        }

        public static Mode mode = Mode.Debug;
        public static string configRef = "config.json";

        static void Main(string[] args) {

            if (mode == Mode.Debug) {
                Debug();
            }

            else if (mode == Mode.Release) {
                Run();
            }

            Console.WriteLine("\n\n//------------------------------------------");
            Console.WriteLine("This program has exited. If this was unexpected, please review the log above. Type exit to quit.");
            while (true) {
                string s = Console.ReadLine();
                if (s.Trim().ToLower() == "exit") {
                    break;
                }
            }
        }

        /// <summary>
        /// Run bot components in debug mode
        /// </summary>
        static void Debug() {
            //Print header
            Console.WriteLine("----------------------------------------");
            Console.WriteLine("PowerBot 2017. Press esc at any point to halt program.");
            Console.WriteLine("----------------------------------------\n");

            //Create default JSON
            BotConfig config = BotConfig.Deserialize(configRef);

            //Create the Database
            FlatFileDatabase.Database db = Database.Create("db.json");

            //Create the logger
            Logger logger = new Logger();

            //Create the bot
            Bot.Bot bot = new Bot.Bot(config, logger);

            //Install the mods
            //Default mods first
            Bot.DefaultModules.EchoModule echo = new Bot.DefaultModules.EchoModule();
            bot.modManager.AddModule(echo, true);
            //Bot.DefaultModules.TaskModule taskmgr = new Bot.DefaultModules.TaskModule();
            //bot.modManager.AddModule(taskmgr, true);
            Bot.DefaultModules.ChanceModule chance = new Bot.DefaultModules.ChanceModule();
            bot.modManager.AddModule(chance, true);

            //Initialize the webserver and web-pages
            SimpleWebServer.WebServer web = new SimpleWebServer.WebServer(logger, "http://localhost", config.networkPort);
            TemplateWebPage index = new TemplateWebPage("Web/index.html");
            index.SetReplacementMethod((string val) => {
                if (val == "modules") {
                    ModuleCore.Modules.IModule[] mods = bot.modManager.GetAvailableMods();
                    ModuleCore.Modules.IModule[] installed = bot.modManager.GetInstalledMods();
                    string[] replace = new string[mods.Length];
                    for (int i = 0; i < mods.Length; i++) {
                        string s = "{name:\"" 
                            + mods[i].GetModuleName() 
                            + "\",uid:" + mods[i].GetUid() 
                            + ",enabled:"+(installed.Contains(mods[i]) ? "true" : "false")+"}";
                        replace[i] = s;
                    }
                    return string.Join(",", replace);
                }
                else {
                    return string.Empty;
                }
            });
            ApiCallPage api = new ApiCallPage((args) => {
                if (!args.ContainsKey("func"))
                    return "{\"error\":true, \"message\":\"No function supplied\"}";

                string op = args["func"];
                switch (op) {
                    case "toggleMods":
                        ModuleCore.Modules.IModule[] mods = bot.modManager.GetAvailableMods();
                        bool success = true;
                        foreach (ModuleCore.Modules.IModule info in mods) {
                                string modid = info.GetModuleName().Value + "#" + info.GetUid();
                                string enabler = args[modid];
                                bool isEnabled;
                                bool parsed = bool.TryParse(enabler, out isEnabled);
                                if (parsed) {
                                    bot.modManager.EnableMod(info, isEnabled);
                                }
                                else {
                                    success = false;
                                }
                            }
                            return "{\"error\":"+!success+", \"message\":\""+(success ? "All mods have been updated." : "An error occured, some mods may not have been updated.")+"\"}";
                        case "getModDesc":
                            mods = bot.modManager.GetAvailableMods();
                            string mod = args["mod"].Trim();
                            foreach (ModuleCore.Modules.IModule info in mods) {
                                string modid = info.GetModuleName().Value + "#" + info.GetUid();
                                if (mod == modid) {
                                    return "{\"error\":" + false + ", \"message\":\""+ info.GetDescription() + "\"}";
                                }
                            }
                            return "{\"error\":" + true + ", \"message\":\"" + "that module does not exist" + "\"}";
                        default:
                            return "{\"error\":true, \"message\":\"This function does not exist.\"}";
                    }
            });
            web.AddPage("index", index);
            web.AddPage("rest", api);

            web.Start();

            //Listen asynchronously
            CancellationTokenSource cancel = new CancellationTokenSource();
            Task t = bot.Connect(cancel); //.GetAwaiter().GetResult();

            //Wait until told otherwise
            while (true) {
                if (Console.ReadKey().Key == ConsoleKey.Escape)
                    break;
            }

            //Cancel everything
            web.Stop();
            cancel.Cancel();

        }

        /// <summary>
        /// Run the bot
        /// </summary>
        static void Run() {

        }
    }
}
