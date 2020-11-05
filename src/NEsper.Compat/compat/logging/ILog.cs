using System;

namespace com.espertech.esper.compat.logging
{
    public interface ILog
    {
        bool IsDebugEnabled { get; }
        bool IsInfoEnabled { get; }
        bool IsWarnEnabled { get; }
        bool IsErrorEnabled { get; }
        bool IsFatalEnabled { get; }

        void Debug(string message);
        void Debug(string messageFormat, params object[] args);
        void Debug(string message, Exception e);

        void Info(string message);
        void Info(string messageFormat, params object[] args);
        void Info(string message, Exception e);

        void Warn(string message);
        void Warn(string messageFormat, params object[] args);
        void Warn(string message, Exception e);

        void Error(string message);
        void Error(string messageFormat, params object[] args);
        void Error(string message, Exception e);

        void Fatal(string message);
        void Fatal(string messageFormat, params object[] args);
        void Fatal(string message, Exception e);
    }
}
