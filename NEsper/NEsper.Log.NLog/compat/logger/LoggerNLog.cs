///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat.logging;

namespace com.espertech.esper.compat.logger
{
    using LogManager = com.espertech.esper.compat.logging.LogManager;

    public class LoggerNLog : ILog
    {
        private readonly NLog.ILogger _log;

        /// <summary>
        /// Registers this logger.
        /// </summary>
        public static void Register()
        {
            LogManager.FactoryLoggerFromType = type =>
                new LoggerNLog(NLog.LogManager.GetLogger(type.FullName));
            LogManager.FactoryLoggerFromName = name =>
                new LoggerNLog(NLog.LogManager.GetLogger(name));
        }

        /// <summary>
        /// A simple configuration for "basic" setups - mostly testing.
        /// </summary>
        public static void BasicConfig()
        {
            var config = new NLog.Config.LoggingConfiguration();
            var console = new NLog.Targets.ConsoleTarget();
            config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, console);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggerNLog"/> class.
        /// </summary>
        /// <param name="log">The log.</param>
        public LoggerNLog(NLog.ILogger log)
        {
            _log = log;
            ChangeConfiguration();
        }

        /// <summary>
        /// Changes the configuration.
        /// </summary>
        private void ChangeConfiguration()
        {
            IsDebugEnabled = _log.IsDebugEnabled;
            IsInfoEnabled = _log.IsInfoEnabled;
            IsWarnEnabled = _log.IsWarnEnabled;
            IsErrorEnabled = _log.IsErrorEnabled;
            IsFatalEnabled = _log.IsFatalEnabled;
        }

        public bool IsDebugEnabled { get; set; }

        public bool IsInfoEnabled { get; set; }

        public bool IsWarnEnabled { get; set; }

        public bool IsErrorEnabled { get; set; }

        public bool IsFatalEnabled { get; set; }

        public void Debug(string message)
        {
            if (IsDebugEnabled)
            {
                _log.Debug(message);
            }
        }

        public void Debug(string messageFormat, params object[] args)
        {
            if (IsDebugEnabled)
            {
                _log.Debug(messageFormat, args);
            }
        }

        public void Debug(string message, Exception e)
        {
            if (IsDebugEnabled)
            {
                _log.Debug(e, message);
            }
        }

        public void Info(string message)
        {
            if (IsInfoEnabled)
            {
                _log.Info(message);
            }
        }

        public void Info(string messageFormat, params object[] args)
        {
            if (IsInfoEnabled)
            {
                _log.Info(messageFormat, args);
            }
        }

        public void Info(string message, Exception e)
        {
            if (IsInfoEnabled)
            {
                _log.Info(e, message);
            }
        }

        public void Warn(string message)
        {
            if (IsWarnEnabled)
            {
                _log.Warn(message);
            }
        }

        public void Warn(string messageFormat, params object[] args)
        {
            if (IsWarnEnabled)
            {
                _log.Warn(messageFormat, args);
            }
        }

        public void Warn(string message, Exception e)
        {
            if (IsWarnEnabled)
            {
                _log.Warn(e, message);
            }
        }

        public void Error(string message)
        {
            if (IsErrorEnabled)
            {
                _log.Error(message);
            }
        }

        public void Error(string messageFormat, params object[] args)
        {
            if (IsErrorEnabled)
            {
                _log.Error(messageFormat, args);
            }
        }

        public void Error(string message, Exception e)
        {
            if (IsErrorEnabled)
            {
                _log.Error(e, message);
            }
        }

        public void Fatal(string message)
        {
            if (IsFatalEnabled)
            {
                _log.Fatal(message);
            }
        }

        public void Fatal(string messageFormat, params object[] args)
        {
            if (IsFatalEnabled)
            {
                _log.Fatal(messageFormat, args);
            }
        }

        public void Fatal(string message, Exception e)
        {
            if (IsFatalEnabled)
            {
                _log.Fatal(e, message);
            }
        }

    }
}
