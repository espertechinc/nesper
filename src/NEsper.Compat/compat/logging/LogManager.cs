using System;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace com.espertech.esper.compat.logging
{
    public class LogManager
    {
        private static Microsoft.Extensions.Logging.ILoggerFactory _loggerFactory = NullLoggerFactory.Instance;

        /// <summary>
        /// Gets or sets the factory that produces a logger given a type.
        /// </summary>
        public static Func<Type, ILog> FactoryLoggerFromType { get; set; }

        /// <summary>
        /// Gets or sets the factory that produces a logger given a name.
        /// </summary>
        public static Func<String, ILog> FactoryLoggerFromName { get; set; }

        /// <summary>
        /// Sets the <see cref="Microsoft.Extensions.Logging.ILoggerFactory"/> used by the default factories.
        /// Call this at application startup to wire in a real logging backend.
        /// </summary>
        public static void SetLoggerFactory(Microsoft.Extensions.Logging.ILoggerFactory factory)
        {
            _loggerFactory = factory ?? NullLoggerFactory.Instance;
        }

        /// <summary>
        /// Initializes the <see cref="LogManager" /> class.
        /// </summary>
        static LogManager()
        {
            FactoryLoggerFromName = name => new LogMEL(_loggerFactory.CreateLogger(name));
            FactoryLoggerFromType = type => new LogMEL(_loggerFactory.CreateLogger(type.FullName));
        }

        /// <summary>
        /// Gets a logger instance for the given type.
        /// </summary>
        /// <param name="t">The t.</param>
        /// <returns></returns>
        public static ILog GetLogger(Type t)
        {
            return FactoryLoggerFromType?.Invoke(t);
        }

        /// <summary>
        /// Gets the logger instance for the given name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static ILog GetLogger(string name)
        {
            return FactoryLoggerFromName?.Invoke(name);
        }
    }
}
