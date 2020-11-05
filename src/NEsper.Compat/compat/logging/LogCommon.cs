using System;

namespace com.espertech.esper.compat.logging
{
    public class LogCommon : ILog
    {
        private readonly Common.Logging.ILog _log;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogCommon"/> class.
        /// </summary>
        /// <param name="log">The log.</param>
        public LogCommon(Common.Logging.ILog log)
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
            if (IsDebugEnabled) {
                _log.Debug(message);
            }
        }

        public void Debug(string messageFormat, params object[] args)
        {
            if (IsDebugEnabled) {
                _log.DebugFormat(messageFormat, args);
            }
        }

        public void Debug(string message, Exception e)
        {
            if (IsDebugEnabled) {
                _log.Debug(message, e);
            }
        }

        public void Info(string message)
        {
            if (IsInfoEnabled) {
                _log.Info(message);
            }
        }

        public void Info(string messageFormat, params object[] args)
        {
            if (IsInfoEnabled) {
                _log.InfoFormat(messageFormat, args);
            }
        }

        public void Info(string message, Exception e)
        {
            if (IsInfoEnabled) {
                _log.Info(message, e);
            }
        }

        public void Warn(string message)
        {
            if (IsWarnEnabled) {
                _log.Warn(message);
            }
        }

        public void Warn(string messageFormat, params object[] args)
        {
            if (IsWarnEnabled) {
                _log.WarnFormat(messageFormat, args);
            }
        }

        public void Warn(string message, Exception e)
        {
            if (IsWarnEnabled) {
                _log.Warn(message, e);
            }
        }

        public void Error(string message)
        {
            if (IsErrorEnabled) {
                _log.Error(message);
            }
        }

        public void Error(string messageFormat, params object[] args)
        {
            if (IsErrorEnabled) {
                _log.ErrorFormat(messageFormat, args);
            }
        }

        public void Error(string message, Exception e)
        {
            if (IsErrorEnabled) {
                _log.Error(message, e);
            }
        }

        public void Fatal(string message)
        {
            if (IsFatalEnabled) {
                _log.Fatal(message);
            }
        }

        public void Fatal(string messageFormat, params object[] args)
        {
            if (IsFatalEnabled) {
                _log.FatalFormat(messageFormat, args);
            }
        }

        public void Fatal(string message, Exception e)
        {
            if (IsFatalEnabled) {
                _log.Fatal(message, e);
            }
        }
    }
}
