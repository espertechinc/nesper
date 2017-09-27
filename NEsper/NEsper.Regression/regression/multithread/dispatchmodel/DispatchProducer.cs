///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using com.espertech.esper.compat.collections;

namespace com.espertech.esper.regression.multithread.dispatchmodel
{
    public class DispatchProducer
    {
        private readonly UpdateDispatchViewModel dispatchProcessor;
        private readonly LinkedHashMap<int, int[]> payloads = new LinkedHashMap<int, int[]>();
        private int currentCount;

        public DispatchProducer(UpdateDispatchViewModel dispatchProcessor)
        {
            this.dispatchProcessor = dispatchProcessor;
        }

        public int Next()
        {
            lock (this) {
                currentCount++;

                var payload = new[] {currentCount, 0};
                payloads.Put(currentCount, payload);

                dispatchProcessor.Add(payload);

                return currentCount;
            }
        }

        public LinkedHashMap<int, int[]> GetPayloads()
        {
            return payloads;
        }
    }
}
