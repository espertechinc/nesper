///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.compat;

namespace com.espertech.esper.runtime.client.scopetest
{
    /// <summary>
    ///     Update listener that retains the events it receives for use in assertions.
    /// </summary>
    public class SupportUpdateListener
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        public SupportUpdateListener()
        {
            NewDataList = new List<EventBean[]>();
            OldDataList = new List<EventBean[]>();
        }

        /// <summary>
        ///     Gets or sets a public tag for the listener.  This is useful for tracking the listener
        ///     when more than one listener might be in use.
        /// </summary>
        /// <value>The tag.</value>
        public string Tag { get; set; }

        /// <summary>Returns the last array of events (insert stream) that were received. </summary>
        /// <value>
        ///     insert stream events or null if either a null value was received or when no events have been received since the
        ///     last
        ///     reset
        /// </value>
        public EventBean[] LastNewData { get; set; }

        /// <summary>Returns the last array of remove-stream events that were received. </summary>
        /// <value>
        ///     remove stream events or null if either a null value was received or when no events have been received since the
        ///     last
        ///     reset
        /// </value>
        public EventBean[] LastOldData { get; set; }

        /// <summary>Get a list of all insert-stream event arrays received. </summary>
        /// <value>list of event arrays</value>
        public List<EventBean[]> NewDataList { get; }

        /// <summary>Get a list of all remove-stream event arrays received. </summary>
        /// <value>list of event arrays</value>
        public List<EventBean[]> OldDataList { get; }

        /// <summary>Returns true if the listener was invoked at least once. </summary>
        /// <value>invoked flag</value>
        public bool IsInvoked { get; private set; }

        /// <summary>
        ///     For multiple listeners, return the invoked flags and reset each listener
        /// </summary>
        public static bool[] GetInvokedFlagsAndReset(SupportUpdateListener[] listeners)
        {
            var invoked = new bool[listeners.Length];
            for (var i = 0; i < listeners.Length; i++) {
                invoked[i] = listeners[i].IsInvokedAndReset();
            }

            return invoked;
        }

        /// <summary>Wait for the listener invocation for up to the given number of milliseconds. </summary>
        /// <param name="msecWait">to wait</param>
        /// <throws>RuntimeException when no results were received</throws>
        public void WaitForInvocation(long msecWait)
        {
            var startTime = DateTimeHelper.CurrentTimeMillis;
            while (true) {
                if (DateTimeHelper.CurrentTimeMillis - startTime > msecWait) {
                    throw new EPException("No result received");
                }

                if (IsInvoked) {
                    return;
                }

                try {
                    Thread.Sleep(50);
                }
                catch (ThreadInterruptedException) {
                    return;
                }
                catch (ThreadAbortException) {
                    return;
                }
            }
        }

        /// <summary>
        ///     Wait for the listener invocation for up to the given number of milliseconds.
        /// </summary>
        /// <param name="msecWait">to wait</param>
        /// <param name="numberOfNewEvents">in any number of separate invocations required before returning</param>
        /// <throws>RuntimeException when no results or insufficient number of events were received</throws>
        public void WaitForInvocation(
            long msecWait,
            int numberOfNewEvents)
        {
            var startTime = DateTimeHelper.CurrentTimeMillis;
            while (true) {
                if (DateTimeHelper.CurrentTimeMillis - startTime > msecWait) {
                    throw new EPException(
                        "No events or less then the number of expected events received, expected " + numberOfNewEvents + " received " +
                        GetNewDataListFlattened().Length);
                }

                var events = GetNewDataListFlattened();
                if (events.Length >= numberOfNewEvents) {
                    return;
                }

                try {
                    Thread.Sleep(50);
                }
                catch (ThreadInterruptedException) {
                    return;
                }
            }
        }

        public void update(
            EventBean[] newData,
            EventBean[] oldData,
            EPStatement statement,
            EPRuntime runtime)
        {
            OldDataList.Add(oldData);
            NewDataList.Add(newData);

            LastNewData = newData;
            LastOldData = oldData;

            IsInvoked = true;
        }

        /// <summary>Reset listener, clearing all associated state. </summary>
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

        /// <summary>Returns the last array of events (insert stream) that were received and resets the listener. </summary>
        /// <returns>
        ///     insert stream events or null if either a null value was received or when no events have been received since
        ///     the last reset
        /// </returns>
        public EventBean[] GetAndResetLastNewData()
        {
            lock (this) {
                var lastNew = LastNewData;
                Reset();
                return lastNew;
            }
        }

        /// <summary>Returns the last array of events (insert stream) that were received and resets the listener. </summary>
        /// <returns>
        ///     insert stream events or null if either a null value was received or when no events have been received since
        ///     the last reset
        /// </returns>
        public EventBean[] GetAndResetLastOldData()
        {
            lock (this) {
                var lastOld = LastOldData;
                Reset();
                return lastOld;
            }
        }

        /// <summary>
        ///     Asserts that exactly one insert stream event was received and no remove stream events, resets the listener
        ///     clearing all state and returns the received event.
        /// </summary>
        /// <returns>single insert-stream event</returns>
        public EventBean AssertOneGetNewAndReset()
        {
            lock (this) {
                ScopeTestHelper.AssertTrue("Listener invocation not received but expected", IsInvoked);

                ScopeTestHelper.AssertEquals("Mismatch in the number of invocations", 1, NewDataList.Count);
                ScopeTestHelper.AssertEquals("Mismatch in the number of invocations", 1, OldDataList.Count);

                if (LastNewData == null) {
                    ScopeTestHelper.Fail("No new-data events received");
                }

                ScopeTestHelper.AssertEquals("Mismatch in the number of new-data events", 1, LastNewData.Length);
                ScopeTestHelper.AssertNull("No old-data events are expected but some were received", LastOldData);

                var lastNew = LastNewData[0];
                Reset();
                return lastNew;
            }
        }

        /// <summary>
        ///     Asserts that exactly one remove stream event was received and no insert stream events, resets the listener
        ///     clearing all state and returns the received event.
        /// </summary>
        /// <returns>single remove-stream event</returns>
        public EventBean AssertOneGetOldAndReset()
        {
            lock (this) {
                ScopeTestHelper.AssertTrue("Listener invocation not received but expected", IsInvoked);

                ScopeTestHelper.AssertEquals("Mismatch in the number of invocations", 1, NewDataList.Count);
                ScopeTestHelper.AssertEquals("Mismatch in the number of invocations", 1, OldDataList.Count);

                if (LastOldData == null) {
                    ScopeTestHelper.Fail("No old-data events received");
                }

                ScopeTestHelper.AssertEquals("Mismatch in the number of old-data events", 1, LastOldData.Length);
                ScopeTestHelper.AssertNull("Expected no new-data events", LastNewData);

                var lastNew = LastOldData[0];
                Reset();
                return lastNew;
            }
        }

        /// <summary>
        ///     Asserts that exactly one insert stream event and exactly one remove stream event was received, resets the
        ///     listener clearing all state and returns the received events as a pair.
        /// </summary>
        /// <returns>pair of insert-stream and remove-stream events</returns>
        public UniformPair<EventBean> AssertPairGetIRAndReset()
        {
            lock (this) {
                ScopeTestHelper.AssertTrue("Listener invocation not received but expected", IsInvoked);

                ScopeTestHelper.AssertEquals("Mismatch in the number of invocations", 1, NewDataList.Count);
                ScopeTestHelper.AssertEquals("Mismatch in the number of invocations", 1, OldDataList.Count);

                if (LastNewData == null) {
                    ScopeTestHelper.Fail("No new-data events received");
                }

                if (LastOldData == null) {
                    ScopeTestHelper.Fail("No old-data events received");
                }

                ScopeTestHelper.AssertEquals("Mismatch in the number of new-data events", 1, LastNewData.Length);
                ScopeTestHelper.AssertEquals("Mismatch in the number of old-data events", 1, LastOldData.Length);

                var lastNew = LastNewData[0];
                var lastOld = LastOldData[0];
                Reset();
                return new UniformPair<EventBean>(lastNew, lastOld);
            }
        }

        /// <summary>
        ///     Asserts that exactly one insert stream event was received not checking remove stream data, and returns the
        ///     received event.
        /// </summary>
        /// <returns>single insert-stream event</returns>
        public EventBean AssertOneGetNew()
        {
            lock (this) {
                ScopeTestHelper.AssertTrue("Listener invocation not received but expected", IsInvoked);

                ScopeTestHelper.AssertEquals("Mismatch in the number of invocations", 1, NewDataList.Count);
                ScopeTestHelper.AssertEquals("Mismatch in the number of invocations", 1, OldDataList.Count);

                if (LastNewData == null) {
                    ScopeTestHelper.Fail("No new-data events received");
                }

                ScopeTestHelper.AssertEquals("Mismatch in the number of new-data events", 1, LastNewData.Length);
                return LastNewData[0];
            }
        }

        /// <summary>
        ///     Asserts that exactly one remove stream event was received not checking insert stream data, and returns the
        ///     received event.
        /// </summary>
        /// <returns>single remove-stream event</returns>
        public EventBean AssertOneGetOld()
        {
            lock (this) {
                ScopeTestHelper.AssertTrue("Listener invocation not received but expected", IsInvoked);

                ScopeTestHelper.AssertEquals("Mismatch in the number of invocations", 1, NewDataList.Count);
                ScopeTestHelper.AssertEquals("Mismatch in the number of invocations", 1, OldDataList.Count);

                if (LastOldData == null) {
                    ScopeTestHelper.Fail("No old-data events received");
                }

                ScopeTestHelper.AssertEquals("Mismatch in the number of old-data events", 1, LastOldData.Length);
                return LastOldData[0];
            }
        }

        /// <summary>Returns true if the listener was invoked at least once and clears the invocation flag. </summary>
        /// <returns>invoked flag</returns>
        public bool GetAndClearIsInvoked()
        {
            lock (this) {
                var invoked = IsInvoked;
                IsInvoked = false;
                return invoked;
            }
        }

        /// <summary>Returns true if the listener was invoked at least once and clears the invocation flag. </summary>
        /// <returns>invoked flag</returns>
        public bool IsInvokedAndReset()
        {
            lock (this) {
                var invoked = IsInvoked;
                Reset();
                return invoked;
            }
        }

        /// <summary>Returns an event array that represents all insert-stream events received so far. </summary>
        /// <returns>event array</returns>
        public EventBean[] GetNewDataListFlattened()
        {
            lock (this) {
                return Flatten(NewDataList);
            }
        }

        /// <summary>Returns an event array that represents all remove-stream events received so far. </summary>
        /// <returns>event array</returns>
        public EventBean[] GetOldDataListFlattened()
        {
            lock (this) {
                return Flatten(OldDataList);
            }
        }

        private static EventBean[] Flatten(IEnumerable<EventBean[]> list)
        {
            return list.Where(events => events != null).SelectMany(events => events).ToArray();
        }

        /// <summary>
        ///     Returns a pair of insert and remove stream event arrays considering the last invocation only, asserting that
        ///     only a single invocation occured, and resetting the listener.
        /// </summary>
        /// <returns>
        ///     pair of event arrays, the first in the pair is the insert stream data, the second in the pair is the remove
        ///     stream data
        /// </returns>
        public UniformPair<EventBean[]> AssertInvokedAndReset()
        {
            lock (this) {
                ScopeTestHelper.AssertTrue("Listener invocation not received but expected", IsInvoked);
                ScopeTestHelper.AssertEquals("Received more then one invocation", 1, NewDataList.Count);
                ScopeTestHelper.AssertEquals("Received more then one invocation", 1, OldDataList.Count);
                var newEvents = LastNewData;
                var oldEvents = LastOldData;
                Reset();
                return new UniformPair<EventBean[]>(newEvents, oldEvents);
            }
        }

        /// <summary>Returns a pair of insert and remove stream event arrays considering the all invocations. </summary>
        /// <returns>
        ///     pair of event arrays, the first in the pair is the insert stream data, the second in the pair is the remove
        ///     stream data
        /// </returns>
        public UniformPair<EventBean[]> GetDataListsFlattened()
        {
            lock (this) {
                return new UniformPair<EventBean[]>(Flatten(NewDataList), Flatten(OldDataList));
            }
        }

        /// <summary>
        ///     Returns a pair of insert and remove stream event arrays considering the all invocations, and resets the
        ///     listener.
        /// </summary>
        /// <returns>
        ///     pair of event arrays, the first in the pair is the insert stream data, the second in the pair is the remove
        ///     stream data
        /// </returns>
        public UniformPair<EventBean[]> GetAndResetDataListsFlattened()
        {
            lock (this) {
                var pair = GetDataListsFlattened();
                Reset();
                return pair;
            }
        }

        /// <summary>
        ///     Produce an array of listeners
        /// </summary>
        /// <param name="size">The size.</param>
        /// <returns></returns>
        public static SupportUpdateListener[] MakeListeners(int size)
        {
            var listeners = new SupportUpdateListener[size];
            for (var i = 0; i < listeners.Length; i++) {
                listeners[i] = new SupportUpdateListener();
            }

            return listeners;
        }
    }

#if REVISIT
    public static class SupportUpdateListenerExtensions
    {
        public static EPStatement AddListener(
            this EPStatement statement,
            SupportUpdateListener listener)
        {
            statement.Events += listener.Update;
            return statement;
        }
    }
#endif
}