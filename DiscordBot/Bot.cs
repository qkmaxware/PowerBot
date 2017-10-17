using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;
using Discord.Commands;

namespace DiscordBot {
    class Bot {

        private DiscordSocketClient client;
        private BotConfig config;

        public Bot(BotConfig config) {
            client = new DiscordSocketClient(new DiscordSocketConfig {

            });

            client.Log += Log;
            this.config = config; 
        }

        private Task Log(LogMessage message) {
            switch (message.Severity) {
                case LogSeverity.Critical:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
            }

            Console.WriteLine($"{DateTime.Now,-19}, [{message.Severity,8}] {message.Source} {message.Message}");
            Console.ResetColor();

            return Task.Delay(0);
        }

        private async Task Read(SocketMessage msg) {
            SocketUserMessage message = msg as SocketUserMessage;
            if (message == null)
                return;

            //Don't respond to myself or other bots
            if (message.Author.Id == client.CurrentUser.Id || message.Author.IsBot)
                return;

            //Check if the message is a command...if so do it
            if (message.Content.StartsWith(config.commandPrefix)) {
                await message.Channel.SendMessageAsync(message.Content);
            }
        }

        private async Task InitBot() {
            client.MessageReceived += Read;
        }

        public async Task Connect() {
            await InitBot();

            //Login and connect
            await client.LoginAsync(TokenType.Bot, config.authentication.token);
            await client.StartAsync();

            //Wait indefinitely
            await Task.Delay(-1);
        }

    }
}
