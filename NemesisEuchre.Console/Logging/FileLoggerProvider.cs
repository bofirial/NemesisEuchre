using System.Diagnostics.CodeAnalysis;
using System.Globalization;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NemesisEuchre.Console.Logging;

[ProviderAlias("File")]
public sealed class FileLoggerProvider : ILoggerProvider
{
    private readonly StreamWriter _writer;
    private readonly object _writeLock = new();

    public FileLoggerProvider(string filePath)
    {
        string? directory = Path.GetDirectoryName(filePath);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        _writer = new StreamWriter(filePath, append: true) { AutoFlush = true };
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new FileLogger(categoryName, _writer, _writeLock);
    }

    public void Dispose()
    {
        _writer.Flush();
        _writer.Dispose();
    }
}

#pragma warning disable SA1204

[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Extension method helper tightly coupled to FileLoggerProvider")]
public static class FileLoggerExtensions
{
    public static ILoggingBuilder AddFile(this ILoggingBuilder builder, string filePath)
    {
        builder.Services.AddSingleton<ILoggerProvider>(new FileLoggerProvider(filePath));
        return builder;
    }
}

#pragma warning disable SA1201, SA1202

[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Internal logger implementation tightly coupled to FileLoggerProvider")]
internal sealed class FileLogger(string categoryName, StreamWriter writer, object writeLock) : ILogger
{
    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        string level = logLevel switch
        {
            LogLevel.Trace => "TRCE",
            LogLevel.Debug => "DBUG",
            LogLevel.Information => "INFO",
            LogLevel.Warning => "WARN",
            LogLevel.Error => "EROR",
            LogLevel.Critical => "CRIT",
            LogLevel.None => "NONE",
            _ => "NONE",
        };
        string message = formatter(state, exception);

        lock (writeLock)
        {
            writer.WriteLine($"{timestamp} [{level}] {categoryName}: {message}");

            if (exception is not null)
            {
                writer.WriteLine(exception.ToString());
            }
        }
    }
}
