using System;
using System.Globalization;
using System.IO;

using Microsoft.Extensions.Logging;

namespace GinRummy.Infrastructure
{
    /// <summary>
    /// Writes the entries of one category to the server's log file. Every category shares the
    /// same file, so the entries are written one at a time.
    /// </summary>
    public class FileLogger : ILogger
    {
        private const string EntryFormat = "{0:yyyy-MM-dd HH:mm:ss.fff} [{1}] {2}: {3}";

        // Every category writes to the same file, so the lock keeps entries from different threads from mixing.
        private static readonly object _fileLock = new object();

        private readonly string _categoryName;
        private readonly string _logFilePath;
        private readonly LogLevel _minimumLevel;

        /// <summary>
        /// Builds a logger for one category.
        /// </summary>
        /// <param name="categoryName">Name written next to every entry, usually the class name.</param>
        /// <param name="logFilePath">Full path of the log file.</param>
        /// <param name="minimumLevel">Lowest level that is written.</param>
        public FileLogger(string categoryName, string logFilePath, LogLevel minimumLevel)
        {
            _categoryName = categoryName ?? throw new ArgumentNullException(nameof(categoryName));
            _logFilePath = logFilePath ?? throw new ArgumentNullException(nameof(logFilePath));
            _minimumLevel = minimumLevel;
        }

        /// <summary>
        /// Scopes are not written to the file, so the caller receives no scope to dispose of.
        /// </summary>
        /// <typeparam name="TState">Type of the scope state.</typeparam>
        /// <param name="state">Scope state; ignored.</param>
        /// <returns>Always null.</returns>
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        /// <summary>
        /// Tells whether entries of the given level are written.
        /// </summary>
        /// <param name="logLevel">Level to check.</param>
        /// <returns>True when the level is at or above the minimum level.</returns>
        public bool IsEnabled(LogLevel logLevel)
        {
            return (logLevel != LogLevel.None) && (logLevel >= _minimumLevel);
        }

        /// <summary>
        /// Writes one entry, with the exception and its stack trace when there is one. The time
        /// is written in UTC so entries from different machines can be compared.
        /// </summary>
        /// <typeparam name="TState">Type of the entry state.</typeparam>
        /// <param name="logLevel">Level of the entry.</param>
        /// <param name="eventId">Identifier of the event; not written.</param>
        /// <param name="state">State the formatter turns into the message.</param>
        /// <param name="exception">Exception related to the entry, or null.</param>
        /// <param name="formatter">Function that builds the message.</param>
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
                // The entry is dropped because the server account has no permission to write in the log folder.
            }
        }
    }
}
