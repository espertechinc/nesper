///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.compat.concurrency
{
    internal class FutureImpl<T> : FutureBase,
        IFuture<T>
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private T _value;

        /// <summary>
        ///     Initializes a new instance of the <see cref="FutureImpl{T}" /> class.
        /// </summary>
        public FutureImpl()
        {
            HasValue = false;
            _value = default(T);
        }

        /// <summary>
        ///     Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public T Value {
            get {
                if (!HasValue) {
                    throw new InvalidOperationException();
                }

                return _value;
            }
            set {
                _value = value;
                HasValue = true;
            }
        }

        /// <summary>
        ///     Gets or sets the callable.
        /// </summary>
        /// <value>The callable.</value>
        internal Func<T> Callable { get; set; }

        /// <summary>
        ///     Gets a value indicating whether this instance has value.
        /// </summary>
        /// <value><c>true</c> if this instance has value; otherwise, <c>false</c>.</value>
        public bool HasValue { get; private set; }

        /// <summary>
        /// Gets the value.  If a value is not available this method throw a InvalidOperationException.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public T Get()
        {
            return Value;
        }

        /// <summary>
        ///     Gets the value. If a value is not available before the timeout expires,
        ///     a TimeoutException will be thrown.
        /// </summary>
        /// <param name="timeOut">The time out.</param>
        /// <returns></returns>
        public T GetValue(TimeSpan timeOut)
        {
            var timeCur = PerformanceObserver.MilliTime;
            var timeEnd = timeCur + timeOut.TotalMilliseconds;

            for (var ii = 0; !HasValue; ii++) {
                timeCur = PerformanceObserver.MilliTime;
                if (timeCur > timeEnd) {
                    throw new TimeoutException();
                }

                SlimLock.SmartWait(ii);
            }

            return Value;
        }

        public T GetValue(
            int units,
            TimeUnit timeUnit)
        {
            return GetValue(TimeUnitHelper.ToTimeSpan(units, timeUnit));
        }

        /// <summary>
        ///     Gets the result value from the execution.
        /// </summary>
        /// <returns></returns>
        public T GetValueOrDefault()
        {
            if (!HasValue) {
                return default(T);
            }

            return Value;
        }

        /// <summary>
        ///     Invokes this instance.
        /// </summary>
        protected override void InvokeImpl()
        {
            Value = Callable.Invoke();
            if (Log.IsInfoEnabled) {
                Log.Info("Invoke - Completed with return value of {0}", Value);
            }
        }
    }
}