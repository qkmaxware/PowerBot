using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord.WebSocket;
using Discord.Audio;

namespace DiscordBot.Bot {
    class DefaultCommandContext : ModuleCore.Modules.ICommandContext {
        private string msg;
        private SocketMessage socket;

        public DefaultCommandContext(SocketMessage socket, string msg) {
            this.msg = msg;
            this.socket = socket;
        }

        public string GetMessage() {
            return msg;
        }

        public string GetSenderName() {
            return this.socket.Author.Username;
        }

        public string GetSenderId() {
            return ""+this.socket.Author.Id;
        }

        public string GetSenderMention() {
            return this.socket.Author.Mention;
        }

        public void ReplyToChannel(string msg) {
            this.socket.Channel.SendMessageAsync(msg);
        }

        public void ReplyToUser(string msg) {
            this.socket.Author.GetOrCreateDMChannelAsync().GetAwaiter().GetResult().SendMessageAsync(msg);
        }
    }
}
