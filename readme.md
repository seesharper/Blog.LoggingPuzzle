# Solving the logging puzzle

As a framework or library developer, we are of often faced with a need to log what is going on inside our library. We can then either choose to adapt one of the many logging libraries out there such as Log4Net, NLog or maybe even the new kid on the block, Microsoft.Extensions.Logging.

The problem with this approach is that we force a dependency upon the users of our library. If we for instance decide to use Log4Net, the host must confirm to this and this that can get problematic really fast as they might be using another logging framework or even worse, another version of Log4Net

> Different versions of Log4Net is extra troublesome since they have a tendency to change the public key token between versions, making assembly redirects a real pain in the …

## Let the host decide

As a library developer we should not denote a logging framework, but we should provide a way for the actual logging framework to plug into our library.

Let's assume that we want to provide logging capabilities for traditional five logging levels 

 * Debug
* Info
* Warning
* Error
* Fatal

The following code represents a super simple logging abstraction that we can simply copy into our library.

```C#
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
```

> Note: Extension methods are internal since they will only be call inside our library.



## Let's integrate 

Okay, so how do we use this?

We have this class that requires logging and the class is being consumed by someone (the host) that has landed on Microsoft.Extensions.Logging as the logging framework. 

We are simply going to expose the `LoggingFactory` delegate in the constructor of our class

```C#
public class Foo
{
    private readonly Logger _logger;

    public Foo(LogFactory logFactory)
    {
        _logger = logFactory.CreateLogger<Foo>();
        _logger.Info("This is an info message");        
    }
}
```



Then on the call site we simply redirect into 

```C#
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
```



Run the app (`dotnet run`) and watch as our log messages are being forwarded to the console.

```shell
info: logging_puzzle.Foo[0]
      This is an info message
```











