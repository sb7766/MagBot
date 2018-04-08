using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MagBot.DatabaseContexts;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Collections.Concurrent;

namespace MagBot.Services
{
    public class LogService
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly IConfiguration _config;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _discordLogger;
        private readonly ILogger _commandsLogger;
        private readonly ILogger _databaseLogger;

        public LogService(DiscordSocketClient discord, CommandService commands, IConfiguration config, ILoggerFactory loggerFactory)
        {
            _discord = discord;
            _commands = commands;
            _config = config;

            _loggerFactory = ConfigureLogging(loggerFactory);
            _discordLogger = _loggerFactory.CreateLogger("discord");
            _commandsLogger = _loggerFactory.CreateLogger("commands");
            _databaseLogger = _loggerFactory.CreateLogger("database");


            _discord.Log += LogDiscord;
            _commands.Log += LogCommand;
        }

        private ILoggerFactory ConfigureLogging(ILoggerFactory factory)
        {
            factory.AddConsole(_config.GetSection("Logging"));
            factory.AddProvider(new FileLoggerProvider(new FileLoggerConfig
            {
                LogLevel = LogLevel.Error,
                FilePath = Environment.CurrentDirectory + @"\errorlog.txt"
            }));
            if (_config["logDebug"] == "true")
            factory.AddProvider(new FileLoggerProvider(new FileLoggerConfig
            {
                LogLevel = LogLevel.Debug,
                FilePath = Environment.CurrentDirectory + @"\debuglog.txt"
            }));

            return factory;
        }

        private Task LogDiscord(LogMessage message)
        {
            _discordLogger.Log(
                LogLevelFromSeverity(message.Severity),
                0,
                message,
                message.Exception,
                (_1, _2) => message.ToString(prependTimestamp: false));
            return Task.CompletedTask;
        }

        private Task LogCommand(LogMessage message)
        {
            // Return an error message for async commands
            if (message.Exception is CommandException command)
            {
                // Don't risk blocking the logging task by awaiting a message send; ratelimits!?
                var _ = command.Context.Channel.SendMessageAsync($"Error: {command.Message}");
            }

            _commandsLogger.Log(
                LogLevelFromSeverity(message.Severity),
                0,
                message,
                message.Exception,
                (_1, _2) => message.ToString(prependTimestamp: false));
            return Task.CompletedTask;
        }

        private static LogLevel LogLevelFromSeverity(LogSeverity severity)
            => (LogLevel)(Math.Abs((int)severity - 5));

    }

    public class FileLoggerConfig
    {
        public LogLevel LogLevel { get; set; } = LogLevel.Warning;
        public int EventId { get; set; } = 0;
        public string FilePath { get; set; } = Environment.CurrentDirectory + @"\logfile.txt";
    }

    public class FileLogger : ILogger
    {
        private readonly string _name;
        private readonly FileLoggerConfig _config;

        public FileLogger(string name, FileLoggerConfig config)
        {
            _name = name;
            _config = config;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= _config.LogLevel;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            if (_config.EventId == 0 || _config.EventId == eventId.Id)
            {
                using (StreamWriter writer = new StreamWriter(_config.FilePath, true))
                {
                    writer.WriteLine($"{logLevel.ToString()}: {_name}[{eventId.Id}] at {DateTime.Now}\n{formatter(state, exception)}\n");
                }
            }
        }
    }

    public class FileLoggerProvider : ILoggerProvider
    {
        private readonly FileLoggerConfig _config;
        private readonly ConcurrentDictionary<string, FileLogger> _loggers = new ConcurrentDictionary<string, FileLogger>();

        public FileLoggerProvider(FileLoggerConfig config)
        {
            _config = config;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, name => new FileLogger(name, _config));
        }

        public void Dispose()
        {
            _loggers.Clear();
        }
    }
}