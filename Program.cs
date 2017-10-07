using System;
using Microsoft.Extensions.Logging;
namespace logging_puzzle
{
    class Program
    {
        static void Main(string[] args)
        {           
            var loggerFactory = new LoggerFactory()
                .AddConsole();                           
            var foo = new Foo(type =>
            {
                var logger = loggerFactory.CreateLogger(type);
                return ((level, message, exception) =>
                {
                    if (level == LogLevel.Info)
                    {
                        logger.LogInformation(message);
                    }
                    // optonally map other levels 
                });
            });
            Console.ReadKey();
        }
    }
    public class Foo
    {
        private readonly Logger _logger;

        public Foo(LogFactory logFactory)
        {
            _logger = logFactory.CreateLogger<Foo>();
            _logger.Info("This is an info message");        
        }
    }

    public delegate Logger LogFactory(Type type);

    public delegate void Logger(LogLevel level, string message, Exception exception = null);

    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Fatal
    }

    internal static class LogExtensions
    {
        public static Logger CreateLogger<T>(this LogFactory logFactory) => logFactory(typeof(T));

        public static void Info(this Logger logger, string message) => logger(LogLevel.Info, message);

        public static void Warning(this Logger logger, string message) => logger(LogLevel.Warning, message);

        public static void Error(this Logger logger, string message, Exception exception = null) => logger(LogLevel.Error, message, exception);

        public static void Fatal(this Logger logger, string message, Exception exception = null) => logger(LogLevel.Fatal, message, exception);
    }
}
