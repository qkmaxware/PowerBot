using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;
using System.Text.RegularExpressions;

namespace DiscordBot.Bot {
    class Bot {

        private DiscordSocketClient client;
        private BotConfig config;
        private CommandMatrix commands = new CommandMatrix();

        private static Regex firstWordOnly = new Regex("^.+\\s");

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
                try {
                    string m = message.Content.Substring(config.commandPrefix.Length);
                    string first = firstWordOnly.Match(m).Value.Trim();
                    if (first.Length != 0) {
                        //Define context variables (sender ect)
                        Modules.ICommandContext ctx = null;

                        string[] parts = first.Split('.');
                        string mod = null;
                        string func = parts[0];
                        if (parts.Length > 1) {
                            mod = func;
                            func = string.Join(".", parts.Skip(1));
                        }
                        List<KeyValuePair<Modules.IModule, Modules.ICommand>> cmds;
                        commands.GetCommand(out cmds, Modules.xtype.Register(func));

                        if (cmds == null || cmds.Count == 0) {
                            //No command
                        }

                        if (cmds.Count > 1) {
                            //Multiple commands, Match module
                            if (mod != null) {
                                Modules.ICommand c = null;
                                foreach (KeyValuePair<Modules.IModule, Modules.ICommand> p in cmds) {
                                    if (p.Key.GetModuleName().Value == mod) {
                                        c = p.Value;
                                        break;
                                    }
                                }
                                if (c != null) {
                                    //Found command, run it
                                    c.Run(ctx);
                                }
                                else {
                                    //No command in that module
                                }
                            }
                            else {
                                //No module provided...too many alternatives
                            }
                        }

                        if (cmds.Count == 1) {
                            //One Command, call it
                            cmds[0].Value.Run(ctx);
                        }
                    }
                }
                catch (Exception e) {
                    //Something unexpected occurred
                }
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

        private void ReplyOnChannel(ISocketMessageChannel channel, string content) {
            channel.SendMessageAsync(content);
        }

        private void SendMessageToUser(ulong userid, string content) {
            client.GetUser(userid).SendMessageAsync(content);
        }

    }
}
