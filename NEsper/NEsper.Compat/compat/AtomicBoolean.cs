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
    public class AtomicBoolean
    {
        private long _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="AtomicBoolean"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public AtomicBoolean(bool value = false)
        {
            _value = value ? 1 : 0;
        }

        /// <summary>
        /// Gets this instance.
        /// </summary>
        /// <returns></returns>
        public bool Get()
        {
            return Interlocked.Read(ref _value) == 1;
        }

        /// <summary>
        /// Sets the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        public void Set(bool value)
        {
            Interlocked.Exchange(ref _value, value ? 1 : 0);
        }
    }
}
