using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DiscordBot.Bot;
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
            BotConfig config = BotConfig.Deserialize("configRef");

            //Create the Database
            FlatFileDatabase.Database db = Database.Create("db.json");

            //Create the bot
            Bot.Bot bot = new Bot.Bot(config);

            //Initialize the webserver
            SimpleWebServer.WebServer web = new SimpleWebServer.WebServer();
            SimpleWebServer.WebPage index = new SimpleWebServer.WebPage("Testing 1,2,3");
            web.AddPage("index", index);

            web.Start();

            //Listen asynchronously
            Task t = bot.Connect(); //.GetAwaiter().GetResult();

            //Wait until told otherwise
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

        }
    }
}
