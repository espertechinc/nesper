using System;

using Microsoft.Extensions.Logging;

namespace com.espertech.esper.compat.logging
{
    public class LogMEL : ILog
    {
        private readonly Microsoft.Extensions.Logging.ILogger _log;

        public LogMEL(Microsoft.Extensions.Logging.ILogger log)
        {
            _log = log;
            IsDebugEnabled = log.IsEnabled(LogLevel.Debug);
            IsInfoEnabled = log.IsEnabled(LogLevel.Information);
            IsWarnEnabled = log.IsEnabled(LogLevel.Warning);
            IsErrorEnabled = log.IsEnabled(LogLevel.Error);
            IsFatalEnabled = log.IsEnabled(LogLevel.Critical);
        }

        public bool IsDebugEnabled { get; set; }
        public bool IsInfoEnabled { get; set; }
        public bool IsWarnEnabled { get; set; }
        public bool IsErrorEnabled { get; set; }
        public bool IsFatalEnabled { get; set; }

        public void Debug(string message)
        {
            if (IsDebugEnabled) {
                _log.LogDebug(message);
            }
        }

        public void Debug(string messageFormat, params object[] args)
        {
            if (IsDebugEnabled) {
                _log.LogDebug(messageFormat, args);
            }
        }

        public void Debug(string message, Exception e)
        {
            if (IsDebugEnabled) {
                _log.LogDebug(e, message);
            }
        }

        public void Info(string message)
        {
            if (IsInfoEnabled) {
                _log.LogInformation(message);
            }
        }

        public void Info(string messageFormat, params object[] args)
        {
            if (IsInfoEnabled) {
                _log.LogInformation(messageFormat, args);
            }
        }

        public void Info(string message, Exception e)
        {
            if (IsInfoEnabled) {
                _log.LogInformation(e, message);
            }
        }

        public void Warn(string message)
        {
            if (IsWarnEnabled) {
                _log.LogWarning(message);
            }
        }

        public void Warn(string messageFormat, params object[] args)
        {
            if (IsWarnEnabled) {
                _log.LogWarning(messageFormat, args);
            }
        }

        public void Warn(string message, Exception e)
        {
            if (IsWarnEnabled) {
                _log.LogWarning(e, message);
            }
        }

        public void Error(string message)
        {
            if (IsErrorEnabled) {
                _log.LogError(message);
            }
        }

        public void Error(string messageFormat, params object[] args)
        {
            if (IsErrorEnabled) {
                _log.LogError(messageFormat, args);
            }
        }

        public void Error(string message, Exception e)
        {
            if (IsErrorEnabled) {
                _log.LogError(e, message);
            }
        }

        public void Fatal(string message)
        {
            if (IsFatalEnabled) {
                _log.LogCritical(message);
            }
        }

        public void Fatal(string messageFormat, params object[] args)
        {
            if (IsFatalEnabled) {
                _log.LogCritical(messageFormat, args);
            }
        }

        public void Fatal(string message, Exception e)
        {
            if (IsFatalEnabled) {
                _log.LogCritical(e, message);
            }
        }
    }
}
