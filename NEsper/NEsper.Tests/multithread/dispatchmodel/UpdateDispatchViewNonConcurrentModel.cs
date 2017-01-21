///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System.Collections.Generic;

using com.espertech.esper.compat.threading;
using com.espertech.esper.dispatch;


namespace com.espertech.esper.multithread.dispatchmodel
{
    public class UpdateDispatchViewNonConcurrentModel : UpdateDispatchViewModel, Dispatchable
    {
        private DispatchService dispatchService;
        private DispatchListener dispatchListener;
    
        internal class ThreadLocalData
        {
            public bool IsDispatchWaiting;
            public LinkedList<int[]> Received;

            internal ThreadLocalData()
            {
                IsDispatchWaiting = false;
                Received = new LinkedList<int[]>();
            }
        }

        private readonly IThreadLocal<ThreadLocalData> threadLocalData =
            ThreadLocalManager.Create<ThreadLocalData>(() => new ThreadLocalData());
    
        /// <summary>
        /// Gets the local data.
        /// </summary>
        /// <value>The local data.</value>
        private ThreadLocalData LocalData
        {
            get { return threadLocalData.GetOrCreate(); }
        }

        public UpdateDispatchViewNonConcurrentModel(DispatchService dispatchService, DispatchListener dispatchListener)
        {
            this.dispatchService = dispatchService;
            this.dispatchListener = dispatchListener;
        }
    
        public void Add(int[] payload)
        {
            var local = LocalData;

            local.Received.AddLast(payload);
            if (!local.IsDispatchWaiting)
            {
                dispatchService.AddExternal(this);
                local.IsDispatchWaiting = true;
            }
        }
    
        public void Execute()
        {
            var local = LocalData;

            // flatten
            LinkedList<int[]> payloads = local.Received;
            int[][] result = new int[payloads.Count][];
    
            int count = 0;
            foreach (int[] entry in payloads)
            {
                result[count++] = entry;
            }
    
            local.IsDispatchWaiting = false;
            payloads.Clear();
            dispatchListener.Dispatched(result);
        }
    }
}
