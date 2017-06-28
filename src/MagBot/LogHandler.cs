using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;

namespace MagBot
{
    public static class LogHandler
    {
        public static async Task Log(LogMessage message)
        {
            await LogInternal(message);
        }

        public static async Task Log(string messageText, LogSeverity severity = LogSeverity.Info, string source = "MagBot", Exception exception = null)
        {
            LogMessage message = new LogMessage(severity, source, messageText, exception);
            await LogInternal(message);
        }

        private static Task LogInternal(LogMessage message)
        {
            ConsoleColor color = new ConsoleColor();
            switch (message.Severity)
            {
                case LogSeverity.Info:
                    color = ConsoleColor.White;
                    break;
                case LogSeverity.Warning:
                    color = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Error:
                    color = ConsoleColor.Red;
                    break;
                case LogSeverity.Critical:
                    color = ConsoleColor.DarkRed;
                    break;
                default:
                    color = ConsoleColor.White;
                    break;
            }
            if (message.Source == "Command")
            {
                color = ConsoleColor.Cyan;
            }
            Console.ForegroundColor = color;

            string output = $"[{DateTime.Now}][{message.Severity}][{message.Source}]: {message.Message}";
            if (message.Exception != null)
            {
                output += $" Error: {message.Exception.Message}";
            }

            Console.WriteLine(output);
            Console.ForegroundColor = ConsoleColor.White;
            return Task.CompletedTask;
        }
    }
}
