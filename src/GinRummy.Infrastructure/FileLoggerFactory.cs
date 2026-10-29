using System;

using Microsoft.Extensions.Logging;

namespace GinRummy.Infrastructure
{
    /// <summary>
    /// Creates the loggers of the server, all writing to one text file. Only the abstractions
    /// package is referenced, the same one the client uses, because it adds no other dependency
    /// to .NET Framework 4.8.
    /// </summary>
    public class FileLoggerFactory : ILoggerFactory
    {
        private readonly string _logFilePath;
        private readonly LogLevel _minimumLevel;

        /// <summary>
        /// Builds the factory for the given file and minimum level.
        /// </summary>
        /// <param name="logFilePath">Full path of the log file.</param>
        /// <param name="minimumLevel">Lowest level that is written.</param>
        public FileLoggerFactory(string logFilePath, LogLevel minimumLevel)
        {
            _logFilePath = logFilePath ?? throw new ArgumentNullException(nameof(logFilePath));
            _minimumLevel = minimumLevel;
        }

        /// <summary>
        /// Creates the logger of one category.
        /// </summary>
        /// <param name="categoryName">Name written next to every entry.</param>
        /// <returns>A logger that writes to the server's log file.</returns>
        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger(categoryName, _logFilePath, _minimumLevel);
        }

        /// <summary>
        /// Not supported: the factory writes only to its own file.
        /// </summary>
        /// <param name="provider">Ignored.</param>
        /// <exception cref="NotSupportedException">Always thrown.</exception>
        public void AddProvider(ILoggerProvider provider)
        {
            throw new NotSupportedException("The file logger factory writes only to its own file and accepts no other provider.");
        }

        /// <summary>
        /// Every entry opens and closes the file, so the factory holds no resource to release.
        /// </summary>
        public void Dispose()
        {
        }
    }
}
