///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Threading;

namespace com.espertech.esper.compat.threading
{
    /// <summary>
    /// IThreadLocal implementation that uses the native support
    /// in the CLR (i.e. the LocalDataStoreSlot).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SystemThreadLocal<T> : IThreadLocal<T>
        where T : class
    {
        /// <summary>
        /// Local data storage slot
        /// </summary>
        private readonly LocalDataStoreSlot m_dataStoreSlot;

        /// <summary>
        /// Factory delegate for construction of data on miss.
        /// </summary>

        private readonly FactoryDelegate<T> m_dataFactory;

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public T Value
        {
            get { return (T) Thread.GetData(m_dataStoreSlot); }
            set { Thread.SetData(m_dataStoreSlot, value); }
        }

        /// <summary>
        /// Gets the data or creates it if not found.
        /// </summary>
        /// <returns></returns>
        public T GetOrCreate()
        {
            T value;

            value = (T)Thread.GetData(m_dataStoreSlot);
            if ( value == null )
            {
                value = m_dataFactory();
                Thread.SetData( m_dataStoreSlot, value );
            }

            return value;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemThreadLocal&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="factory">The factory used to create values when not found.</param>
        public SystemThreadLocal( FactoryDelegate<T> factory )
        {
            m_dataStoreSlot = Thread.AllocateDataSlot();
            m_dataFactory = factory;
        }
    }

    /// <summary>
    /// Creates system thread local objects.
    /// </summary>
    public class SystemThreadLocalFactory : ThreadLocalFactory
    {
        #region ThreadLocalFactory Members

        /// <summary>
        /// Create a thread local object of the specified type param.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="factory"></param>
        /// <returns></returns>
        public IThreadLocal<T> CreateThreadLocal<T>(FactoryDelegate<T> factory) where T : class
        {
            return new SystemThreadLocal<T>(factory);
        }

        #endregion
    }
}
