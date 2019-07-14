///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.support.util
{
    public class SupportMTUpdateListener : UpdateListener
    {
        public SupportMTUpdateListener()
        {
            NewDataList = new List<EventBean[]>();
            OldDataList = new List<EventBean[]>();
        }

        public EventBean[] LastNewData { get; private set; }

        public EventBean[] LastOldData { get; private set; }

        public IList<EventBean[]> NewDataList { get; }

        public IList<EventBean[]> OldDataList { get; }

        public bool IsInvoked { get; private set; }

        public void Update(
            object sender,
            UpdateEventArgs eventArgs)
        {
            var newData = eventArgs.NewEvents;
            var oldData = eventArgs.OldEvents;

            lock (this) {
                OldDataList.Add(oldData);
                NewDataList.Add(newData);

                LastNewData = newData;
                LastOldData = oldData;

                IsInvoked = true;
            }
        }

        public void Reset()
        {
            lock (this) {
                OldDataList.Clear();
                NewDataList.Clear();
                LastNewData = null;
                LastOldData = null;
                IsInvoked = false;
            }
        }

        public EventBean[] GetAndResetLastNewData()
        {
            lock (this) {
                var lastNew = LastNewData;
                Reset();
                return lastNew;
            }
        }

        public EventBean AssertOneGetNewAndReset()
        {
            lock (this) {
                Assert.IsTrue(IsInvoked);

                Assert.AreEqual(1, NewDataList.Count);
                Assert.AreEqual(1, OldDataList.Count);

                Assert.AreEqual(1, LastNewData.Length);
                Assert.IsNull(LastOldData);

                var lastNew = LastNewData[0];
                Reset();
                return lastNew;
            }
        }

        public EventBean AssertOneGetOldAndReset()
        {
            lock (this) {
                Assert.IsTrue(IsInvoked);

                Assert.AreEqual(1, NewDataList.Count);
                Assert.AreEqual(1, OldDataList.Count);

                Assert.AreEqual(1, LastOldData.Length);
                Assert.IsNull(LastNewData);

                var lastNew = LastOldData[0];
                Reset();
                return lastNew;
            }
        }

        public IList<EventBean[]> GetNewDataListCopy()
        {
            lock (this) {
                return new List<EventBean[]>(NewDataList);
            }
        }

        public bool GetAndClearIsInvoked()
        {
            lock (this) {
                var invoked = IsInvoked;
                IsInvoked = false;
                return invoked;
            }
        }

        public EventBean[] GetNewDataListFlattened()
        {
            lock (this) {
                return Flatten(NewDataList);
            }
        }

        public EventBean[] GetOldDataListFlattened()
        {
            lock (this) {
                return Flatten(OldDataList);
            }
        }

        private EventBean[] Flatten(IList<EventBean[]> list)
        {
            var count = 0;
            foreach (var events in list) {
                if (events != null) {
                    count += events.Length;
                }
            }

            var array = new EventBean[count];
            count = 0;
            foreach (var events in list) {
                if (events != null) {
                    for (var i = 0; i < events.Length; i++) {
                        array[count++] = events[i];
                    }
                }
            }

            return array;
        }
    }
} // end of namespace