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

namespace com.espertech.esper.supportregression.multithread
{
    public class UpdateDispatchViewOrderEnforcingModel : UpdateDispatchViewModel
    {
        private readonly DispatchListener _dispatchListener;
        private readonly DispatchService _dispatchService;
        private readonly IThreadLocal<bool> _isDispatchWaiting = new FastThreadLocal<bool>(() => false);
        private readonly IThreadLocal<LinkedList<int[]>> _received = new FastThreadLocal<LinkedList<int[]>>(
            () => new LinkedList<int[]>());

        private DispatchFuture _currentFuture;

        public UpdateDispatchViewOrderEnforcingModel(DispatchService dispatchService, DispatchListener dispatchListener)
        {
            _currentFuture = new DispatchFuture(); // use a completed future as a start
            _dispatchService = dispatchService;
            _dispatchListener = dispatchListener;
        }

        public void Add(int[] payload)
        {
            _received.GetOrCreate().AddLast(payload);
            if (!_isDispatchWaiting.GetOrCreate())
            {
                DispatchFuture nextFuture;
                lock (this)
                {
                    nextFuture = new DispatchFuture(this, _currentFuture);
                    _currentFuture.Later = nextFuture;
                    _currentFuture = nextFuture;
                }

                _dispatchService.AddExternal(nextFuture);
                _isDispatchWaiting.Value = true;
            }
        }

        public void Execute()
        {
            // flatten
            var payloads = _received.GetOrCreate();
            var result = new int[payloads.Count][];

            var count = 0;
            foreach (var entry in payloads)
            {
                result[count++] = entry;
            }

            _isDispatchWaiting.Value = false;
            payloads.Clear();
            _dispatchListener.Dispatched(result);
        }
    }
} // end of namespace