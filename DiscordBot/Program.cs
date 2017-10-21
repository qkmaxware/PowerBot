using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            Console.WriteLine("PowerBot 2016. Press esc at any point to halt program.");
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
            bot.ReInstall(echo);

            //Initialize the webserver
            SimpleWebServer.WebServer web = new SimpleWebServer.WebServer(logger, "http://localhost", 8081);
            TemplateWebPage index = new TemplateWebPage("Web/index.html");
            index.SetReplacementMethod((string val) => {
                if (val == "modules") {
                    string[] mods = bot.GetInstalledMods();
                    string[] replace = new string[mods.Length];
                    for (int i = 0; i < mods.Length; i++) {
                        string[] split = mods[i].Split('#');
                        string s = "{name:\"" + split[0] + "\",uid:" + split[1] + "}";
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
                    return "{error:true, message:\"No function supplied\"}";

                return "{\"message\":\"Call recieved\"}";
            });
            web.AddPage("index", index);
            web.AddPage("rest", api);

            web.Start();

            //Listen asynchronously
            Task t = bot.Connect(); //.GetAwaiter().GetResult();

            //Wait until told otherwise
            while (true) {
                if (Console.ReadKey().Key == ConsoleKey.Escape)
                    break;
            }
            web.Stop();

        }

        /// <summary>
        /// Run the bot
        /// </summary>
        static void Run() {

        }
    }
}
