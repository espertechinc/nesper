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
    public class AtomicLong
    {
        private long _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="AtomicLong"/> class.
        /// </summary>
        public AtomicLong()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AtomicLong"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public AtomicLong(long value)
        {
            _value = value;
        }

        /// <summary>
        /// Gets this instance.
        /// </summary>
        /// <returns></returns>
        public long Get()
        {
            return Interlocked.Read(ref _value);
        }

        /// <summary>
        /// Sets the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        public void Set(long value)
        {
            Interlocked.Exchange(ref _value, value);
        }

        /// <summary>
        /// Increments and returns the previous value.
        /// </summary>
        /// <returns></returns>
        public long GetAndIncrement()
        {
            while (true)
            {
                long value = _value;
                if (Interlocked.CompareExchange(ref _value, value + 1, value) == value)
                    return value;
            }
        }

        /// <summary>
        /// Increments and returns the value.
        /// </summary>
        /// <returns></returns>
        public long IncrementAndGet()
        {
            return Interlocked.Increment(ref _value);
        }

        /// <summary>
        /// Increments and returns the value.
        /// </summary>
        /// <returns></returns>
        public long IncrementAndGet(long value)
        {
            return Interlocked.Add(ref _value, value);
        }

        /// <summary>
        /// Decrements and returns the value.
        /// </summary>
        /// <returns></returns>
        public long DecrementAndGet()
        {
            return Interlocked.Decrement(ref _value);
        }

        /// <summary>
        /// Increments and returns the value.
        /// </summary>
        /// <returns></returns>
        public long DecrementAndGet(long value)
        {
            return Interlocked.Add(ref _value, -value);
        }
    }
}
