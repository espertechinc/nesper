///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Threading;

namespace com.espertech.esper.compat
{
    public class Atomic<T> where T : class
    {
        private T _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="Atomic&lt;T&gt;"/> class.
        /// </summary>
        public Atomic()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Atomic&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public Atomic(T value)
        {
            _value = value;
        }

        /// <summary>
        /// Gets this instance.
        /// </summary>
        /// <returns></returns>
        public T Get()
        {
            return _value;
        }

        /// <summary>
        /// Sets the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        public void Set(T value)
        {
            Interlocked.Exchange(ref _value, value);
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        public T Value
        {
            get { return _value; }
        }

    }
}
