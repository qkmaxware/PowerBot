using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;

namespace DiscordBot{
    class Logger {

        public Task Log(LogMessage message) {
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

        public void Log(string source, string msg) {
            Log(new LogMessage(LogSeverity.Info, source, msg));
        }

    }
}
