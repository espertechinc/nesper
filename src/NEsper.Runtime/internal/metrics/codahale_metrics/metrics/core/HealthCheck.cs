///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text;

namespace com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.core
{
    /// <summary>
    ///     A health check for a component of your application.
    /// </summary>
    public abstract class HealthCheck
    {
        /// <summary>
        ///     Create a new <seealso cref="HealthCheck" /> instance with the given name.
        /// </summary>
        /// <param name="name">
        ///     the name of the health check (and, ideally, the name of the underlyingcomponent the health check tests)
        /// </param>
        protected HealthCheck(string name)
        {
            Name = name;
        }

        /// <summary>
        ///     Returns the health check's name.
        /// </summary>
        /// <returns>the health check's name</returns>
        public string Name { get; }

        /// <summary>
        ///     Perform a check of the application component.
        /// </summary>
        /// <returns>
        ///     if the component is healthy, a healthy <seealso cref="Result" />; otherwise, an unhealthy<seealso cref="Result" />
        ///     with a descriptive error message or exception
        /// </returns>
        /// <throws>
        ///     Exception if there is an unhandled error during the health check; this will result ina failed health check
        /// </throws>
        protected abstract Result Check();

        /// <summary>
        ///     Executes the health check, catching and handling any exceptions raised by {@link #check()}.
        /// </summary>
        /// <returns>
        ///     if the component is healthy, a healthy <seealso cref="Result" />; otherwise, an unhealthy<seealso cref="Result" />
        ///     with a descriptive error message or exception
        /// </returns>
        public Result Execute()
        {
            try
            {
                return Check();
            }
            catch (Exception e)
            {
                return Result.Unhealthy(e);
            }
        }

        /// <summary>
        ///     The result of a <seealso cref="HealthCheck" /> being run. It can be healthy (with an optional message)
        ///     or unhealthy (with either an error message or a thrown exception).
        /// </summary>
        public class Result
        {
            private const int PRIME = 31;
            private static readonly Result HEALTHY = new Result(true, null, null);

            private Result(
                bool isHealthy,
                string message,
                Exception error)
            {
                IsHealthy = isHealthy;
                Message = message;
                Error = error;
            }

            /// <summary>
            ///     Returns {@code true} if the result indicates the component is healthy; {@code false}
            ///     otherwise.
            /// </summary>
            /// <returns>{@code true} if the result indicates the component is healthy</returns>
            public bool IsHealthy { get; }

            /// <summary>
            ///     Returns any additional message for the result, or {@code null} if the result has no
            ///     message.
            /// </summary>
            /// <returns>any additional message for the result, or {@code null}</returns>
            public string Message { get; }

            /// <summary>
            ///     Returns any exception for the result, or {@code null} if the result has no exception.
            /// </summary>
            /// <returns>any exception for the result, or {@code null}</returns>
            public Exception Error { get; }

            /// <summary>
            ///     Returns a healthy <seealso cref="Result" /> with no additional message.
            /// </summary>
            /// <returns>a healthy <seealso cref="Result" /> with no additional message</returns>
            public static Result Healthy()
            {
                return HEALTHY;
            }

            /// <summary>
            ///     Returns a healthy <seealso cref="Result" /> with an additional message.
            /// </summary>
            /// <param name="message">an informative message</param>
            /// <returns>a healthy <seealso cref="Result" /> with an additional message</returns>
            public static Result Healthy(string message)
            {
                return new Result(true, message, null);
            }

            /// <summary>
            ///     Returns a healthy <seealso cref="Result" /> with a formatted message.
            ///     <para />
            ///     Message formatting follows the same rules as
            ///     {@link String#format(String, Object...)}.
            /// </summary>
            /// <param name="message">a message format</param>
            /// <param name="args">the arguments apply to the message format</param>
            /// <returns>a healthy <seealso cref="Result" /> with an additional message</returns>
            /// <unknown>@see String#format(String, Object...)</unknown>
            public static Result Healthy(
                string message,
                params object[] args)
            {
                return Healthy(string.Format(message, args));
            }

            /// <summary>
            ///     Returns an unhealthy <seealso cref="Result" /> with the given message.
            /// </summary>
            /// <param name="message">an informative message describing how the health check failed</param>
            /// <returns>an unhealthy <seealso cref="Result" /> with the given message</returns>
            public static Result Unhealthy(string message)
            {
                return new Result(false, message, null);
            }

            /// <summary>
            ///     Returns an unhealthy <seealso cref="Result" /> with a formatted message.
            ///     <para />
            ///     Message formatting follows the same rules as
            ///     {@link String#format(String, Object...)}.
            /// </summary>
            /// <param name="message">a message format</param>
            /// <param name="args">the arguments apply to the message format</param>
            /// <returns>an unhealthy <seealso cref="Result" /> with an additional message</returns>
            /// <unknown>@see String#format(String, Object...)</unknown>
            public static Result Unhealthy(
                string message,
                params object[] args)
            {
                return Unhealthy(string.Format(message, args));
            }

            /// <summary>
            ///     Returns an unhealthy <seealso cref="Result" /> with the given error.
            /// </summary>
            /// <param name="error">an exception thrown during the health check</param>
            /// <returns>an unhealthy <seealso cref="Result" /> with the given error</returns>
            public static Result Unhealthy(Exception error)
            {
                return new Result(false, error.Message, error);
            }

            protected bool Equals(Result other)
            {
                return IsHealthy == other.IsHealthy
                       && string.Equals(Message, other.Message)
                       && Equals(Error, other.Error);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }

                if (ReferenceEquals(this, obj))
                {
                    return true;
                }

                if (obj.GetType() != GetType())
                {
                    return false;
                }

                return Equals((Result) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = IsHealthy.GetHashCode();
                    hashCode = (hashCode * 397) ^ (Message != null ? Message.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (Error != null ? Error.GetHashCode() : 0);
                    return hashCode;
                }
            }

            public override string ToString()
            {
                var builder = new StringBuilder("Result{isHealthy=");
                builder.Append(IsHealthy);
                if (Message != null)
                {
                    builder.Append(", message=").Append(Message);
                }

                if (Error != null)
                {
                    builder.Append(", error=").Append(Error);
                }

                builder.Append('}');
                return builder.ToString();
            }
        }
    }
} // end of namespace