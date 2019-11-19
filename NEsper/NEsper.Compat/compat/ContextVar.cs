///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

namespace com.espertech.esper.compat
{
    /// <summary>
    /// Provides a stack-like object that can be used to maintain the state of a
    /// thread-local value.  Unlike a pure threadstatic variable a ContextVar can
    /// have multiple values that can be stacked.
    /// </summary>
    /// <typeparam name="T"></typeparam>

    public class ContextVar<T> : IDisposable
    {
        [ThreadStatic] private static T current;

        /// <summary>
        /// Gets the current value associated with the context.
        /// </summary>
        /// <value>The current.</value>
        public static T Current
        {
            get { return current; }
        }

        private readonly T previous;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextVar&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public ContextVar(T value)
        {
            previous = current;
            current = value;
        }

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            current = previous;
        }

        #endregion
    }
}
