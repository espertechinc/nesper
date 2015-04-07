using System;

namespace com.espertech.esper.compat.logging
{
    public class LogManager
    {
        /// <summary>
        /// Gets a logger instance for the given type.
        /// </summary>
        /// <param name="t">The t.</param>
        /// <returns></returns>
        public static ILog GetLogger(Type t)
        {
            return new LogCommon(Common.Logging.LogManager.GetLogger(t));
        }

        /// <summary>
        /// Gets the logger instance for the given name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static ILog GetLogger(string name)
        {
            return new LogCommon(Common.Logging.LogManager.GetLogger(name));
        }
    }
}
