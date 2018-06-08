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
    /// <summary>
    /// IThreadLocal provides the engine with a way to store information that
    /// is local to the instance and a the thread.  While the CLR provides the
    /// ThreadStatic attribute, it can only be applied to static variables;
    /// some usage patterns in esper (such as statement-specific thread-specific
    /// processing data) require that data be associated by instance and thread.
    /// The CLR provides a solution to this known as LocalDataStoreSlot.  It
    /// has been documented that this method is slower than its ThreadStatic
    /// counterpart, but it allows for instance-based allocation.
    /// <para/>
    /// During recent testing it was determined that the LocalDataStoreSlot was
    /// using an amount of time that seemed a bit excessive.  We took some
    /// snapshots of performance under the profiler.  Using that information we
    /// retooled the class to provide tight and fast access to thread-local
    /// instance-specific data.  The class is pretty tightly wound and takes a
    /// few liberties in understanding how esper uses it.  A ThreadStatic
    /// variable is initialized for the IThreadLocal.  This item is 'thread-local'
    /// and contains an array of 'instance-specific' data.  Indexing is done
    /// when the IThreadLocal item is created.  Under esper this results in roughly
    /// one 'index' per statement.  Changes to this model resulted in good cost
    /// savings in the retrieval and acquisition of local data.
    /// </summary>
    /// <typeparam name="T"></typeparam>

    public sealed class FastThreadLocal<T> : IThreadLocal<T>
    {
        // Technique    Config      Cycles      time-ms     avg time-us
        // IThreadLocal	Release	    1183734	    6200.5	    5.238085583
        // IThreadLocal	Release	    1224525	    5126.6	    4.186602968
        // IThreadLocal	Release	    1153012	    5935.3	    5.147648073
        // Hashtable	Debug	    1185562	    3848.1	    3.245802413
        // List<T>	    Debug	    996737	    1678	    1.683493238
        // Array	    Debug	    924738	    1032	    1.115991773
        // Array	    Debug	    1179226	    1328.4	    1.126501621
        // Array	    Release	    1224513	    1296.4	    1.058706604

        private static long _typeInstanceId = 0;

        private static readonly Queue<int> IndexReclaim = new Queue<int>();

        private readonly int _instanceId;

        /// <summary>
        /// Gets the instance id ... if you really must know.
        /// </summary>
        /// <value>The instance id.</value>
        public int InstanceId => _instanceId;

        internal class StaticData
        {
            internal T[] Table;
            internal int Count;
            internal T   Last;
            internal int LastIndx;

            internal StaticData()
            {
                Table = new T[100];
                Count = Table.Length;
                Last = default(T);
                LastIndx = -1;
            }
        }

        /// <summary>
        /// List of weak reference data.  This list is allocated when the
        /// class is instantiated and keeps track of data that is allocated
        /// regardless of thread.  Minimal locks should be used to ensure
        /// that normal IThreadLocal activity is not placed in the crossfire
        /// of this structure.
        /// </summary>
        private static readonly LinkedList<WeakReference<StaticData>> ThreadDataList;

        /// <summary>
        /// Lock for the _threadDataList
        /// </summary>
        private static readonly IReaderWriterLock ThreadDataListLock;

        /// <summary>
        /// Initializes the <see cref="FastThreadLocal&lt;T&gt;"/> class.
        /// </summary>
        static FastThreadLocal()
        {
            ThreadDataList = new LinkedList<WeakReference<StaticData>>();
            ThreadDataListLock = new SlimReaderWriterLock(60000);
        }

        [ThreadStatic]
        private static StaticData _threadData;

        /// <summary>
        /// Factory delegate for construction of data on miss.
        /// </summary>

        private readonly Func<T> _dataFactory;

        private static StaticData GetThreadData()
        {
            var lThreadData = _threadData;
            if (lThreadData != null)
            {
                return lThreadData;
            }

            _threadData = lThreadData = new StaticData();
            using (ThreadDataListLock.AcquireWriteLock())
            {
                ThreadDataList.AddLast(new WeakReference<StaticData>(_threadData));
            }

            return lThreadData; //_table;
        }

        private static StaticData GetThreadData(int index)
        {
            var lThreadData = _threadData;
            if (lThreadData == null)
            {
                _threadData = lThreadData = new StaticData();
                using (ThreadDataListLock.AcquireWriteLock())
                {
                    ThreadDataList.AddLast(new WeakReference<StaticData>(_threadData));
                }
            }

            if (index >= lThreadData.Count)
            {
                Rebalance(lThreadData, index);
            }

            return lThreadData;
        }

        private static StaticData CreateThreadData(int index)
        {
            StaticData lThreadData = _threadData = new StaticData();
            using (ThreadDataListLock.AcquireWriteLock())
            {
                ThreadDataList.AddLast(new WeakReference<StaticData>(_threadData));
            }

            //T[] lTable = lThreadData.Table;
            if (index >= lThreadData.Count)
            {
                Rebalance(lThreadData, index);
            }

            return lThreadData;
        }

        private static void Rebalance(StaticData lThreadData, int index)
        {
            var lTable = lThreadData.Table;
            var tempTable = new T[index + 100 - index%100];
            Array.Copy(lTable, tempTable, lTable.Length);
            lThreadData.Table = tempTable;
            lThreadData.Count = tempTable.Length;
            //return lTable;
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public T Value
        {
            get
            {
                int lInstanceId = _instanceId;
                T[] lThreadData = GetThreadData().Table;
                T value = lThreadData.Length > lInstanceId
                              ? lThreadData[lInstanceId]
                              : default(T);

                return value;
            }

            set
            {
                var lInstanceId = _instanceId;
                var lThreadData = GetThreadData(lInstanceId).Table;
                lThreadData[lInstanceId] = value;
            }
        }

        /// <summary>
        /// Gets the data or creates it if not found.
        /// </summary>
        /// <returns></returns>
        public T GetOrCreate() // MSIL Length - 0031 bytes
        {
            var sThreadData = _threadData;
            if (sThreadData != null) {
                if (sThreadData.LastIndx == _instanceId) {
                    return sThreadData.Last;
                }
            } else {
                sThreadData = CreateThreadData(_instanceId);
            }

            return ReturnRest(sThreadData);
        }

        private T ReturnRest(StaticData sThreadData)
        {
            if (sThreadData.Count <= _instanceId) {
                Rebalance(sThreadData, _instanceId);
            }

            var value = sThreadData.Table[_instanceId];
            if (value == null) {
                value = sThreadData.Table[_instanceId] = _dataFactory();
            }

            sThreadData.LastIndx = _instanceId;
            sThreadData.Last = value;
            return value;
        }

        /// <summary>
        /// Clears all threads
        /// </summary>
        public void ClearAll()
        {
            int lInstance = _instanceId;

            using (ThreadDataListLock.AcquireReadLock())
            {
                LinkedList<WeakReference<StaticData>>.Enumerator threadDataEnum =
                    ThreadDataList.GetEnumerator();
                while (threadDataEnum.MoveNext())
                {
                    WeakReference<StaticData> threadDataRef = threadDataEnum.Current;
                    if (threadDataRef.IsAlive)
                    {
                        StaticData threadData = threadDataRef.Target;
                        if (threadData != null)
                        {
                            if (threadData.Count > lInstance)
                            {
                                threadData.Table[lInstance] = default(T);
                            }

                            continue;
                        }
                    }

                    // Anything making it to this point indicates that the thread
                    // has probably terminated and we are still keeping it's static
                    // data weakly referenced in the threadDataList.  We can safely
                    // remove it, but it needs to be done with a writerLock.
                }
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            ClearAll();

            lock (IndexReclaim)
            {
                IndexReclaim.Enqueue(_instanceId);
            }
        }

        #region ISerializable Members
#if false
        public void GetObjectData(SerializationInfo INFO, StreamingContext context)
        {
            INFO.AddValue("m_dataFactory", m_dataFactory, typeof(Func<T>));
        }
#endif
        #endregion

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="FastThreadLocal&lt;T&gt;"/> is reclaimed by garbage collection.
        /// </summary>
        ~FastThreadLocal()
        {
            Dispose();
        }

        /// <summary>
        /// Allocates a usable index.  This method looks in the indexReclaim
        /// first to determine if there is a slot that has been released.  If so,
        /// it is reclaimed.  If no space is available, a new index is allocated.
        /// This can lead to growth of the static data table.
        /// </summary>
        /// <returns></returns>
        private static int AllocateIndex()
        {
            if (IndexReclaim.Count != 0)
            {
                lock (IndexReclaim)
                {
                    if (IndexReclaim.Count != 0)
                    {
                        return IndexReclaim.Dequeue() ;
                    }
                }
            }

            // indexes in the above range have already been allocated to the
            // table space, so it not necessary to worry about them... however,
            // down here we may be seeing table growth.

            GetThreadData();

            var index = (int) Interlocked.Increment(ref _typeInstanceId);
            if (index >= _threadData.Table.Length)
            {
                var tempTable = new T[_threadData.Table.Length << 1];
                Array.Copy(_threadData.Table, 0, tempTable, 0, _threadData.Table.Length);
                _threadData.Table = tempTable;
            }

            return index;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FastThreadLocal&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="factory">The factory.</param>
        public FastThreadLocal( Func<T> factory )
        {
            _instanceId = AllocateIndex();
            _dataFactory = factory;
        }

#if false
        public FastThreadLocal(SerializationInfo information, StreamingContext context)
        {
            m_instanceId = AllocateIndex();
            m_dataFactory = (Func<T>) information.GetValue("m_dataFactory", typeof (Func<T>));
        }
#endif
    }

    /// <summary>
    /// Creates fast thread local objects.
    /// </summary>
    public class FastThreadLocalFactory : IThreadLocalFactory
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
            return new FastThreadLocal<T>(factory);
        }

        #endregion
    }
}
