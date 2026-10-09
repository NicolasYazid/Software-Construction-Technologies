using System;

using Microsoft.Extensions.Logging;

namespace GinRummy.Client.Logging
{
    // The client has no console, so its log goes to a text file in the local application data of the player.
    // Only the abstractions package is referenced because it adds no other dependency to .NET Framework 4.8.
    public class FileLoggerFactory : ILoggerFactory
    {
        private readonly string _logFilePath;
        private readonly LogLevel _minimumLevel;

        public FileLoggerFactory(string logFilePath, LogLevel minimumLevel)
        {
            _logFilePath = logFilePath;
            _minimumLevel = minimumLevel;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger(categoryName, _logFilePath, _minimumLevel);
        }

        public void AddProvider(ILoggerProvider provider)
        {
            throw new NotSupportedException("The file logger factory writes only to its own file and accepts no other provider.");
        }

        // Every entry opens and closes the file, so the factory holds no resource to release.
        public void Dispose()
        {
        }
    }
}
