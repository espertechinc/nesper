///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat;
using com.espertech.esper.runtime.client;

namespace com.espertech.esper.regressionlib.support.util
{
    public class SupportCountListener : UpdateListener
    {
        private readonly AtomicLong countNew = new AtomicLong();
        private readonly AtomicLong countOld = new AtomicLong();

        public long CountNew => countNew.Get();

        public long CountOld => countOld.Get();

        public void Update(
            object sender,
            UpdateEventArgs eventArgs)
        {
            var newEvents = eventArgs.NewEvents;
            if (newEvents != null) {
                countNew.IncrementAndGet(newEvents.Length);
            }

            var oldEvents = eventArgs.OldEvents;
            if (oldEvents != null) {
                countOld.IncrementAndGet(oldEvents.Length);
            }
        }
    }
} // end of namespace