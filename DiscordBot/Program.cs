using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }

        /// <summary>
        /// Run the bot
        /// </summary>
        static void Run() {
            //Load the configs
            BotConfig conf = BotConfig.Deserialize(configRef);

            //Create the bot
            Bot bot = new Bot();

            //Listen asynchronously
            bot.Connect(conf.authentification.token).GetAwaiter().GetResult();
        }
    }
}
