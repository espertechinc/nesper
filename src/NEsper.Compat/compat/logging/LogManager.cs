using System;

namespace com.espertech.esper.compat.logging
{
    public class LogManager
    {
        /// <summary>
        /// Gets or sets the factory that produces a logger given a type.
        /// </summary>
        public static Func<Type, ILog> FactoryLoggerFromType { get; set; }

        /// <summary>
        /// Gets or sets the factory that produces a logger given a name.
        /// </summary>
        public static Func<String, ILog> FactoryLoggerFromName { get; set; }

        /// <summary>
        /// Initializes the <see cref="LogManager" /> class.
        /// </summary>
        static LogManager()
        {
            FactoryLoggerFromName = name => new LogCommon(Common.Logging.LogManager.GetLogger(name));
            FactoryLoggerFromType = type => new LogCommon(Common.Logging.LogManager.GetLogger(type));
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
