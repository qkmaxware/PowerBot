using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace DiscordBot.Bot {
    class DefaultCommandContext : Modules.ICommandContext {
        private string msg;
        private SocketUser sender;
        private ISocketMessageChannel channel;

        public DefaultCommandContext(SocketUser sender, ISocketMessageChannel channel, string msg) {
            this.msg = msg;
            this.sender = sender;
            this.channel = channel;
        }

        public string GetMessage() {
            return msg;
        }

        public void ReplyToChannel(string msg) {
            channel.SendMessageAsync(msg);
        }

        public void ReplyToUser(string msg) {
            sender.GetOrCreateDMChannelAsync().GetAwaiter().GetResult().SendMessageAsync(msg);
        }
    }
}
