///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using com.espertech.esper.compat.collections;

namespace com.espertech.esper.supportregression.multithread
{
    public class DispatchProducer
    {
        private readonly UpdateDispatchViewModel _dispatchProcessor;
        private readonly LinkedHashMap<int, int[]> _payloads = new LinkedHashMap<int, int[]>();
        private int _currentCount;

        public DispatchProducer(UpdateDispatchViewModel dispatchProcessor)
        {
            _dispatchProcessor = dispatchProcessor;
        }

        public int Next()
        {
            lock (this) {
                _currentCount++;

                var payload = new[] {_currentCount, 0};
                _payloads.Put(_currentCount, payload);

                _dispatchProcessor.Add(payload);

                return _currentCount;
            }
        }

        public LinkedHashMap<int, int[]> GetPayloads()
        {
            return _payloads;
        }
    }
}
