using System;
using System.Globalization;
using System.IO;

using Microsoft.Extensions.Logging;

namespace GinRummy.Client.Logging
{
    public class FileLogger : ILogger
    {
        private const string EntryFormat = "{0:yyyy-MM-dd HH:mm:ss.fff} [{1}] {2}: {3}";

        // Every category writes to the same file, so the lock keeps two entries from different threads from mixing.
        private static readonly object _fileLock = new object();

        private readonly string _categoryName;
        private readonly string _logFilePath;
        private readonly LogLevel _minimumLevel;

        public FileLogger(string categoryName, string logFilePath, LogLevel minimumLevel)
        {
            _categoryName = categoryName;
            _logFilePath = logFilePath;
            _minimumLevel = minimumLevel;
        }

        // Scopes are not written to the file, so the caller receives no scope to dispose of.
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return (logLevel != LogLevel.None) && (logLevel >= _minimumLevel);
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            string entry = string.Format(
                CultureInfo.InvariantCulture,
                EntryFormat,
                DateTime.UtcNow,
                logLevel,
                _categoryName,
                formatter(state, exception));
            if (exception != null)
            {
                entry = entry + Environment.NewLine + exception;
            }

            AppendEntry(entry);
        }

        // A failure to write the log must not interrupt the operation being logged, and there is no other log to report it to.
        private void AppendEntry(string entry)
        {
            try
            {
                lock (_fileLock)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(_logFilePath));
                    File.AppendAllText(_logFilePath, entry + Environment.NewLine);
                }
            }
            catch (IOException)
            {
                // The entry is dropped because the file is locked by another process or the disk is not available.
            }
            catch (UnauthorizedAccessException)
            {
                // The entry is dropped because the player has no permission to write in the log folder.
            }
        }
    }
}
