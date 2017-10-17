using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Web.Script.Serialization;
using System.IO;

namespace DiscordBot {
    
    class BotConfig {

        public class Authentification {
            public string token;
        }

        private static JavaScriptSerializer serializer = new JavaScriptSerializer();

        public static void Serialize(string location, BotConfig config) {
            string json = serializer.Serialize(config);
            File.WriteAllLines(location, new string[] { json });
        }

        public static BotConfig Deserialize(string location) {
            if (!File.Exists(location)) {
                BotConfig c = new BotConfig();
                Serialize(location, c);
                throw new Exception("No config file exists. A default config file has been created for you. Please edit this file and fill in the required information before attempting to load this configuration again.");
            }
            string[] contents = File.ReadAllLines(location);
            string json = string.Join("\n", contents);
            BotConfig conf = serializer.Deserialize<BotConfig>(json);
            return conf;
        }

        //----------------------------------------------------------------------------

        /// <summary>
        /// Bot authentification settings
        /// </summary>
        public Authentification authentification = new Authentification();

    }
}
