///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Threading;

namespace com.espertech.esper.compat.threading
{
    public sealed class SlimThreadLocal<T> : IThreadLocal<T>
        where T : class
    {
        private IDictionary<Thread, T> _threadTable;
        private readonly SlimLock _wLock;
        private readonly Func<T> _valueFactory;

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public T Value
        {
            get
            {
                T value;
                _threadTable.TryGetValue(Thread.CurrentThread, out value);
                return value;
            }

            set
            {
                _wLock.Enter();
                try
                {
                    var tempTable = new Dictionary<Thread, T>(_threadTable);
                    tempTable[Thread.CurrentThread] = value;
                    _threadTable = tempTable;
                }
                finally
                {
                    _wLock.Release();
                }
            }
        }

        /// <summary>
        /// Gets the data or creates it if not found.
        /// </summary>
        /// <returns></returns>
        public T GetOrCreate()
        {
            T value;
            if (_threadTable.TryGetValue(Thread.CurrentThread, out value))
            {
                return value;
            }

            _wLock.Enter();
            try
            {
                var tempTable = new Dictionary<Thread, T>(_threadTable);
                tempTable[Thread.CurrentThread] = value = _valueFactory.Invoke();
                _threadTable = tempTable;
                return value;
            }
            finally
            {
                _wLock.Release();
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SlimThreadLocal{T}"/> class.
        /// </summary>
        /// <param name="factory">The factory.</param>
        public SlimThreadLocal(Func<T> factory)
        {
            _threadTable = new Dictionary<Thread, T>(new ThreadEq());
            _valueFactory = factory;
            _wLock = new SlimLock();
        }

        internal class ThreadEq : IEqualityComparer<Thread>
        {
            public bool Equals(Thread x, Thread y)
            {
                return x == y;
            }

            public int GetHashCode(Thread obj)
            {
                return obj.ManagedThreadId;
            }
        }
    }

    /// <summary>
    /// Creates slim thread local objects.
    /// </summary>
    public class SlimThreadLocalFactory : IThreadLocalFactory
    {
        #region ThreadLocalFactory Members

        /// <summary>
        /// Create a thread local object of the specified type param.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="factory"></param>
        /// <returns></returns>
        public IThreadLocal<T> CreateThreadLocal<T>(Func<T> factory) where T : class
        {
            return new SlimThreadLocal<T>(factory);
        }

        #endregion
    }
}
