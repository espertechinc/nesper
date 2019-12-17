///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.compat.concurrency
{
    /// <summary>
    /// TBD: Replace with .NET Task Model
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IFuture<T>
    {
        /// <summary>
        /// Waits for execution of the future to complete up to the allotted amount of time.
        /// </summary>
        /// <param name="timeOut"></param>
        bool Wait(TimeSpan timeOut);
        
        /// <summary>
        /// Gets a value indicating whether this instance has value.
        /// </summary>
        /// <value><c>true</c> if this instance has value; otherwise, <c>false</c>.</value>
        bool HasValue { get; }

        /// <summary>
        /// Gets the value.  If a value is not available this method throw a InvalidOperationException.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        T Get();

        /// <summary>
        /// Gets the value. If a value is not available before the timeout expires,
        /// a TimeoutException will be thrown.
        /// </summary>
        /// <param name="timeOut">The time out.</param>
        /// <returns></returns>
        T GetValue(TimeSpan timeOut);

        T GetValue(int units, TimeUnit timeUnit);

        /// <summary>
        /// Gets the result value from the execution.
        /// </summary>
        /// <returns></returns>
        T GetValueOrDefault();

        /// <summary>
        /// Attempts to cancel the future execution.
        /// </summary>
        /// <param name="force">if set to <c>true</c> [force].</param>
        /// <returns></returns>
        bool Cancel(bool force);
    }

    /// <summary>
    /// Default implementation of a future
    /// </summary>
    public interface IFuture : IFuture<object>
    {
    }
}