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
        private static Logger logger;

        public Bot(BotConfig config, Logger logger) {
            client = new DiscordSocketClient(new DiscordSocketConfig {

            });

            client.Log += logger.Log;
            this.config = config; 
        }

        public void ReInstall(Modules.IModule mod) {
            commands.Install(mod);
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
                    string remainder = firstWordOnly.Replace(m, "");
                    if (first.Length != 0) {
                        //Define context variables (sender ect)
                        DefaultCommandContext ctx = new DefaultCommandContext(msg.Author, msg.Channel, remainder);
                        
                        //Get the mod.name
                        string[] parts = first.Split('.');
                        string mod = null;
                        string func = parts[0];
                        if (parts.Length > 1) {
                            mod = func;
                            func = string.Join(".", parts.Skip(1));
                        }
                        List<KeyValuePair<Modules.IModule, Modules.ICommand>> cmds;
                        commands.GetCommand(out cmds, Modules.xtype.Register(func));

                        //No command
                        if (cmds == null || cmds.Count == 0) {
                            ctx.ReplyToChannel("No command found in any modules that matches ["+message.Content+"]");
                        }

                        //Obtain the command
                        Modules.ICommand cmd = null;
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
                                    cmd = c;
                                }
                                else {
                                    //No command in that module
                                    ctx.ReplyToChannel("That command does not exist in module [" + mod + "]");
                                }
                            }
                            else {
                                //No module provided...too many alternatives
                                ctx.ReplyToChannel("There are multiple commands that match ["+func+"] in in modules ["+string.Join(", ", cmds)+"]");
                            }
                        }

                        if (cmds.Count == 1) {
                            //One Command, call it
                            cmd = cmds[0].Value;
                        }

                        //Run the command
                        if (cmd != null)
                            cmd.Run(ctx);
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
