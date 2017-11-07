using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Discord;
using Discord.Audio;
using Discord.WebSocket;
using System.Text.RegularExpressions;

namespace DiscordBot.Bot {
    class Bot {

        private DiscordSocketClient client;
        private BotConfig config;
        public readonly ModuleManager modManager = new ModuleManager();

        private static Regex firstWordOnly = new Regex("^.+?(?:\\s|$)");
        private Logger logger;

        public Bot(BotConfig config, Logger logger) {
            DiscordSocketConfig socketConfig = new DiscordSocketConfig {
            };
            
            client = new DiscordSocketClient(socketConfig);

            this.logger = logger;
            this.config = config;

            //Logging
            client.Log += logger.Log;

            //Connection
            client.Disconnected += OnDisconnected;
            client.Connected += OnConnected;
            client.Ready += OnReady;

            //Message reading
            client.MessageReceived += Read;
        }

        private Task OnReady(){
            return Task.Delay(0);
        }

        private Task OnConnected(){
            return Task.Delay(0);
        }

        private Task OnDisconnected(Exception exp){
            return Task.Delay(0);
        }

        private async Task JoinAudioChannel(IVoiceChannel channel) {
            if (channel == null)
                return;

            IAudioClient voip = await channel.ConnectAsync();
        }

        private StreamDefinition CreateStream() {
            StreamDefinition def = new StreamDefinition();

            return def;
        }

        private async Task SendStreamAsync(IAudioClient client, StreamDefinition stream) {

        }

        private async Task DisconnectAudioChannel(IAudioClient voip) {
            await voip.StopAsync();
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
                    string remainder = firstWordOnly.Replace(m, "").Trim();
                    //For $event-ls this is wrong, it returns an empty cmd and the cmd on remainder

                    if (first.Length != 0) {
                        //Define context variables (sender ect)
                        //SocketGuild guild = client.Guilds.FirstOrDefault(g => g.Name == message.Author.);
                        //IVoiceChannel vc = guild.VoiceChannels.FirstOrDefault(t => t.Name == voiceChannelName);
                        DefaultCommandContext ctx = new DefaultCommandContext(msg, remainder);

                        //Get the mod.name
                        string[] parts = first.Split('.');
                        string mod = null;
                        string func = parts[0];
                        if (parts.Length > 1) {
                            mod = func;
                            func = string.Join(".", parts.Skip(1));
                        }
                        
                        ModuleCore.Modules.xtype function = ModuleCore.Modules.xtype.Register(func);
                        List<ModuleManager.CommandInfo> cmds = modManager.GetCommandsForXtype(function);

                        //No command
                        if (cmds == null || cmds.Count == 0) {
                            ctx.ReplyToChannel("No command found in any modules that matches ["+message.Content+"]");
                        }

                        //Obtain the command
                        ModuleCore.Modules.ICommand cmd = null;
                        if (cmds.Count > 1) {
                            //Multiple commands, Match module
                            if (mod != null) {
                                ModuleCore.Modules.ICommand c = null;
                                foreach (ModuleManager.CommandInfo p in cmds) {
                                    if (p.mod.GetModuleName().Value == mod) {
                                        c = p.cmd;
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
                            cmd = cmds[0].cmd;
                        }

                        //Run the command
                        if (cmd != null)
                            cmd.Run(ctx);
                    }
                }
                catch (Exception e) {
                    //Something unexpected occurred
                    logger.Log("Bot",e.ToString());
                }
            }
        }

        public async Task Connect(CancellationTokenSource token) {

            //Login and connect
            await client.SetGameAsync("Nothing...");
            await client.LoginAsync(TokenType.Bot, config.authentication.token);
            await client.StartAsync();

            //Wait indefinitely until cancelled
            while (!token.Token.IsCancellationRequested) { }

        }

    }
}
