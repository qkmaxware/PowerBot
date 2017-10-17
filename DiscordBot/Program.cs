using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SimpleWebServer;
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
            //Create default JSON
            BotConfig conf = new BotConfig();
            BotConfig.Serialize(configRef, conf);

            FlatFileDatabase.Database db = Database.Create("db.json");
            FlatFileDatabase.QueryCompiler compiler = new QueryCompiler();
            Console.WriteLine(string.Join(",",compiler.Tokenize("SELECT * FROM users WHERE uid > 7")));

            SimpleWebServer.WebServer web = new SimpleWebServer.WebServer();
            SimpleWebServer.WebPage index = new SimpleWebServer.WebPage("Testing 1,2,3");
            web.AddPage("index", index);

            web.Start();
            Console.WriteLine("A Simple Web Server. Press esc to stop.");
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
            //Load the configs
            BotConfig config = BotConfig.Deserialize(configRef);

            //Create the bot
            Bot bot = new Bot(config);

            //Listen asynchronously
            bot.Connect().GetAwaiter().GetResult();
        }
    }
}
