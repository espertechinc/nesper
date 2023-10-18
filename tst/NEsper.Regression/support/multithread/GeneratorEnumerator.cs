///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;

namespace com.espertech.esper.regressionlib.support.multithread
{
    public class GeneratorEnumerator : IEnumerator<object>
    {
        public static readonly GeneratorEnumeratorCallback DEFAULT_SUPPORTEBEAN_CB = numEvent =>
            new SupportBean(Convert.ToString(numEvent), numEvent);

        private readonly GeneratorEnumeratorCallback callback;
        private readonly int maxNumEvents;

        private object nextValue;
        private int numEvents;

        public GeneratorEnumerator(
            int maxNumEvents,
            GeneratorEnumeratorCallback callback)
        {
            this.maxNumEvents = maxNumEvents;
            this.callback = callback;
        }

        public GeneratorEnumerator(int maxNumEvents)
        {
            this.maxNumEvents = maxNumEvents;
            callback = DEFAULT_SUPPORTEBEAN_CB;
            nextValue = null;
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            if (numEvents < maxNumEvents) {
                nextValue = callback.Invoke(numEvents);
                numEvents++;
                return true;
            }

            return false;
        }

        public object Current {
            get {
                if (numEvents > maxNumEvents) {
                    throw new InvalidOperationException();
                }

                return nextValue;
            }
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }
    }
} // end of namespace