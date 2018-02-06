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
    /// FastThreadStore is a variation of the FastThreadLocal, but it lacks a factory
    /// for object creation.  While there are plenty of cases where this makes sense,
    /// we actually did this to work around an issue in .NET 3.5 SP1.
    /// </summary>
    /// <typeparam name="T"></typeparam>

    [Serializable]
    public class FastThreadStore<T>
        where T : class
    {
        private static long _typeInstanceId = 0;

        private static readonly Queue<int> IndexReclaim = new Queue<int>();

        [NonSerialized]
        private int _instanceId = 0;

        /// <summary>
        /// Gets the instance id ... if you really must know.
        /// </summary>
        /// <value>The instance id.</value>
        public int InstanceId
        {
            get
            {
                int temp;
                while(( temp = Interlocked.CompareExchange(ref _instanceId, -1, 0) ) <= 0) {
                    if ( temp == 0 ) {
                        Interlocked.Exchange(ref _instanceId, temp = AllocateIndex());
                        return temp;
                    }

                    Thread.Sleep(0);
                }

                return temp;
            }
        }

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
        /// Initializes the <see cref="FastThreadStore&lt;T&gt;"/> class.
        /// </summary>
        static FastThreadStore()
        {
            ThreadDataList = new LinkedList<WeakReference<StaticData>>();
            ThreadDataListLock = new SlimReaderWriterLock(60000);
        }

        [ThreadStatic]
        private static StaticData _threadData;

        private static StaticData GetThreadData()
        {
            StaticData lThreadData = _threadData;
            if (lThreadData == null)
            {
                _threadData = lThreadData = new StaticData();
                using (ThreadDataListLock.AcquireWriteLock())
                {
                    ThreadDataList.AddLast(new WeakReference<StaticData>(_threadData));
                }
            }

            return lThreadData; //._table;
        }

        private static StaticData GetThreadData(int index)
        {
            StaticData lThreadData = _threadData;
            if (lThreadData == null)
            {
                _threadData = lThreadData = new StaticData();
                using (ThreadDataListLock.AcquireWriteLock())
                {
                    ThreadDataList.AddLast(new WeakReference<StaticData>(_threadData));
                }
            }

            T[] lTable = lThreadData.Table;
            if (index >= lThreadData.Count)
            {
                Rebalance(lThreadData, index, lTable);
            }

            return lThreadData;
        }

        private static void Rebalance(StaticData lThreadData, int index, T[] lTable)
        {
            var tempTable = new T[index + 100 - index%100];
            Array.Copy(lTable, tempTable, lTable.Length);
            lThreadData.Table = lTable = tempTable;
            lThreadData.Count = lTable.Length;
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
                int lInstanceId = InstanceId;
                T[] lThreadData = GetThreadData().Table;
                T value = lThreadData.Length > lInstanceId
                              ? lThreadData[lInstanceId]
                              : default(T);

                return value;
            }

            set
            {
                int lInstanceId = InstanceId;
                T[] lThreadData = GetThreadData(lInstanceId).Table;
                lThreadData[lInstanceId] = value;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            int lInstance = InstanceId;

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
                                threadData.Table[lInstance] = null;
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

            lock (IndexReclaim)
            {
                IndexReclaim.Enqueue(lInstance);
            }
        }
        
        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="FastThreadStore&lt;T&gt;"/> is reclaimed by garbage collection.
        /// </summary>
        ~FastThreadStore()
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

            return (int) Interlocked.Increment(ref _typeInstanceId);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FastThreadStore&lt;T&gt;"/> class.
        /// </summary>
        public FastThreadStore()
        {
            _instanceId = AllocateIndex();
        }
    }
}
