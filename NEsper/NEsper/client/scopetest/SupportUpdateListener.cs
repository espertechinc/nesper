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

using com.espertech.esper.collection;
using com.espertech.esper.compat;

namespace com.espertech.esper.client.scopetest
{
    /// <summary>
    /// Update listener that retains the events it receives for use in assertions.
    /// </summary>
    public class SupportUpdateListener
    {
        private readonly List<EventBean[]> _newDataList;
        private readonly List<EventBean[]> _oldDataList;
        private EventBean[] _lastNewData;
        private EventBean[] _lastOldData;
        private bool _isInvoked;

        /// <summary>
        /// Gets or sets a public tag for the listener.  This is useful for tracking the listener
        /// when more than one listener might be in use.
        /// </summary>
        /// <value>The tag.</value>
        public string Tag { get; set; }

        /// <summary>
        /// Ctor.
        /// </summary>
        public SupportUpdateListener()
        {
            _newDataList = new List<EventBean[]>();
            _oldDataList = new List<EventBean[]>();
        }

        /// <summary>
        /// For multiple listeners, return the invoked flags and reset each listener
        /// </summary>
 
        public static bool[] GetInvokedFlagsAndReset(SupportUpdateListener[] listeners)
        {
            bool[] invoked = new bool[listeners.Length];
            for (int i = 0; i < listeners.Length; i++)
            {
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
            while (true)
            {
                if ((DateTimeHelper.CurrentTimeMillis - startTime) > msecWait)
                {
                    throw new EPException("No result received");
                }
                if (_isInvoked)
                {
                    return;
                }
                try
                {
                    Thread.Sleep(50);
                }
                catch (ThreadInterruptedException)
                {
                    return;
                }
                catch (ThreadAbortException)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Wait for the listener invocation for up to the given number of milliseconds.
        /// </summary>
        /// <param name="msecWait">to wait</param>
        /// <param name="numberOfNewEvents">in any number of separate invocations required before returning</param>
        /// <throws>RuntimeException when no results or insufficient number of events were received</throws>
        public void WaitForInvocation(long msecWait, int numberOfNewEvents)
        {
            var startTime = DateTimeHelper.CurrentTimeMillis;
            while (true)
            {
                if ((DateTimeHelper.CurrentTimeMillis - startTime) > msecWait)
                {
                    throw new EPException("No events or less then the number of expected events received, expected " + numberOfNewEvents + " received " + GetNewDataListFlattened().Length);
                }

                var events = GetNewDataListFlattened();
                if (events.Length >= numberOfNewEvents)
                {
                    return;
                }

                try
                {
                    Thread.Sleep(50);
                }
                catch (ThreadInterruptedException)
                {
                    return;
                }
            }
        }

        public void Update(object sender, UpdateEventArgs e)
        {
            lock (this)
            {
                Update(e.NewEvents, e.OldEvents);
            }
        }

        public void Update(EventBean[] newData, EventBean[] oldData)
        {
            lock (this)
            {
                _oldDataList.Add(oldData);
                _newDataList.Add(newData);

                _lastNewData = newData;
                _lastOldData = oldData;

                _isInvoked = true;
            }
        }

        /// <summary>Reset listener, clearing all associated state. </summary>
        public void Reset()
        {
            lock (this)
            {
                _oldDataList.Clear();
                _newDataList.Clear();
                _lastNewData = null;
                _lastOldData = null;
                _isInvoked = false;
            }
        }

        /// <summary>Returns the last array of events (insert stream) that were received. </summary>
        /// <value>insert stream events or null if either a null value was received or when no events have been received since the last
        ///   reset</value>
        public EventBean[] LastNewData
        {
            get { return _lastNewData; }
            set { _lastNewData = value; }
        }

        /// <summary>Returns the last array of remove-stream events that were received. </summary>
        /// <value>remove stream events or null if either a null value was received or when no events have been received since the last
        ///   reset</value>
        public EventBean[] LastOldData
        {
            get { return _lastOldData; }
            set { _lastOldData = value; }
        }

        /// <summary>Returns the last array of events (insert stream) that were received and resets the listener. </summary>
        /// <returns>insert stream events or null if either a null value was received or when no events have been received since the last reset</returns>
        public EventBean[] GetAndResetLastNewData()
        {
            lock (this)
            {
                EventBean[] lastNew = _lastNewData;
                Reset();
                return lastNew;
            }
        }

        /// <summary>Returns the last array of events (insert stream) that were received and resets the listener. </summary>
        /// <returns>insert stream events or null if either a null value was received or when no events have been received since the last reset</returns>
        public EventBean[] GetAndResetLastOldData()
        {
            lock (this)
            {
                EventBean[] lastOld = _lastOldData;
                Reset();
                return lastOld;
            }
        }

        /// <summary>Asserts that exactly one insert stream event was received and no remove stream events, resets the listener clearing all state and returns the received event. </summary>
        /// <returns>single insert-stream event</returns>
        public EventBean AssertOneGetNewAndReset()
        {
            lock (this)
            {
                ScopeTestHelper.AssertTrue("Listener invocation not received but expected", _isInvoked);

                ScopeTestHelper.AssertEquals("Mismatch in the number of invocations", 1, _newDataList.Count);
                ScopeTestHelper.AssertEquals("Mismatch in the number of invocations", 1, _oldDataList.Count);

                if (_lastNewData == null)
                {
                    ScopeTestHelper.Fail("No new-data events received");
                }
                ScopeTestHelper.AssertEquals("Mismatch in the number of new-data events", 1, _lastNewData.Length);
                ScopeTestHelper.AssertNull("No old-data events are expected but some were received", _lastOldData);

                EventBean lastNew = _lastNewData[0];
                Reset();
                return lastNew;
            }
        }

        /// <summary>Asserts that exactly one remove stream event was received and no insert stream events, resets the listener clearing all state and returns the received event. </summary>
        /// <returns>single remove-stream event</returns>
        public EventBean AssertOneGetOldAndReset()
        {
            lock (this)
            {
                ScopeTestHelper.AssertTrue("Listener invocation not received but expected", _isInvoked);

                ScopeTestHelper.AssertEquals("Mismatch in the number of invocations", 1, _newDataList.Count);
                ScopeTestHelper.AssertEquals("Mismatch in the number of invocations", 1, _oldDataList.Count);

                if (_lastOldData == null)
                {
                    ScopeTestHelper.Fail("No old-data events received");
                }
                ScopeTestHelper.AssertEquals("Mismatch in the number of old-data events", 1, _lastOldData.Length);
                ScopeTestHelper.AssertNull("Expected no new-data events", _lastNewData);

                EventBean lastNew = _lastOldData[0];
                Reset();
                return lastNew;
            }
        }

        /// <summary>Asserts that exactly one insert stream event and exactly one remove stream event was received, resets the listener clearing all state and returns the received events as a pair. </summary>
        /// <returns>pair of insert-stream and remove-stream events</returns>
        public UniformPair<EventBean> AssertPairGetIRAndReset()
        {
            lock (this)
            {
                ScopeTestHelper.AssertTrue("Listener invocation not received but expected", _isInvoked);

                ScopeTestHelper.AssertEquals("Mismatch in the number of invocations", 1, _newDataList.Count);
                ScopeTestHelper.AssertEquals("Mismatch in the number of invocations", 1, _oldDataList.Count);

                if (_lastNewData == null)
                {
                    ScopeTestHelper.Fail("No new-data events received");
                }
                if (_lastOldData == null)
                {
                    ScopeTestHelper.Fail("No old-data events received");
                }
                ScopeTestHelper.AssertEquals("Mismatch in the number of new-data events", 1, _lastNewData.Length);
                ScopeTestHelper.AssertEquals("Mismatch in the number of old-data events", 1, _lastOldData.Length);

                EventBean lastNew = _lastNewData[0];
                EventBean lastOld = _lastOldData[0];
                Reset();
                return new UniformPair<EventBean>(lastNew, lastOld);
            }
        }

        /// <summary>Asserts that exactly one insert stream event was received not checking remove stream data, and returns the received event. </summary>
        /// <returns>single insert-stream event</returns>
        public EventBean AssertOneGetNew()
        {
            lock (this)
            {
                ScopeTestHelper.AssertTrue("Listener invocation not received but expected", _isInvoked);

                ScopeTestHelper.AssertEquals("Mismatch in the number of invocations", 1, _newDataList.Count);
                ScopeTestHelper.AssertEquals("Mismatch in the number of invocations", 1, _oldDataList.Count);

                if (_lastNewData == null)
                {
                    ScopeTestHelper.Fail("No new-data events received");
                }
                ScopeTestHelper.AssertEquals("Mismatch in the number of new-data events", 1, _lastNewData.Length);
                return _lastNewData[0];
            }
        }

        /// <summary>Asserts that exactly one remove stream event was received not checking insert stream data, and returns the received event. </summary>
        /// <returns>single remove-stream event</returns>
        public EventBean AssertOneGetOld()
        {
            lock (this)
            {
                ScopeTestHelper.AssertTrue("Listener invocation not received but expected", _isInvoked);

                ScopeTestHelper.AssertEquals("Mismatch in the number of invocations", 1, _newDataList.Count);
                ScopeTestHelper.AssertEquals("Mismatch in the number of invocations", 1, _oldDataList.Count);

                if (_lastOldData == null)
                {
                    ScopeTestHelper.Fail("No old-data events received");
                }
                ScopeTestHelper.AssertEquals("Mismatch in the number of old-data events", 1, _lastOldData.Length);
                return _lastOldData[0];
            }
        }

        /// <summary>Get a list of all insert-stream event arrays received. </summary>
        /// <value>list of event arrays</value>
        public List<EventBean[]> NewDataList
        {
            get { return _newDataList; }
        }

        /// <summary>Get a list of all remove-stream event arrays received. </summary>
        /// <value>list of event arrays</value>
        public List<EventBean[]> OldDataList
        {
            get { return _oldDataList; }
        }

        /// <summary>Returns true if the listener was invoked at least once. </summary>
        /// <value>invoked flag</value>
        public bool IsInvoked
        {
            get { return _isInvoked; }
        }

        /// <summary>Returns true if the listener was invoked at least once and clears the invocation flag. </summary>
        /// <returns>invoked flag</returns>
        public bool GetAndClearIsInvoked()
        {
            lock (this)
            {
                bool invoked = _isInvoked;
                _isInvoked = false;
                return invoked;
            }
        }

        /// <summary>Returns true if the listener was invoked at least once and clears the invocation flag. </summary>
        /// <returns>invoked flag</returns>
        public bool IsInvokedAndReset()
        {
            lock (this)
            {
                var invoked = _isInvoked;
                Reset();
                return invoked;
            }
        }

        /// <summary>Returns an event array that represents all insert-stream events received so far. </summary>
        /// <returns>event array</returns>
        public EventBean[] GetNewDataListFlattened()
        {
            lock (this)
            {
                return Flatten(_newDataList);
            }
        }

        /// <summary>Returns an event array that represents all remove-stream events received so far. </summary>
        /// <returns>event array</returns>
        public EventBean[] GetOldDataListFlattened()
        {
            lock (this)
            {
                return Flatten(_oldDataList);
            }
        }

        private static EventBean[] Flatten(IEnumerable<EventBean[]> list)
        {
            return list.Where(events => events != null).SelectMany(events => events).ToArray();
        }

        /// <summary>Returns a pair of insert and remove stream event arrays considering the last invocation only, asserting that only a single invocation occured, and resetting the listener. </summary>
        /// <returns>pair of event arrays, the first in the pair is the insert stream data, the second in the pair is the remove stream data</returns>
        public UniformPair<EventBean[]> AssertInvokedAndReset()
        {
            lock (this)
            {
                ScopeTestHelper.AssertTrue("Listener invocation not received but expected", _isInvoked);
                ScopeTestHelper.AssertEquals("Received more then one invocation", 1, NewDataList.Count);
                ScopeTestHelper.AssertEquals("Received more then one invocation", 1, OldDataList.Count);
                EventBean[] newEvents = LastNewData;
                EventBean[] oldEvents = LastOldData;
                Reset();
                return new UniformPair<EventBean[]>(newEvents, oldEvents);
            }
        }

        /// <summary>Returns a pair of insert and remove stream event arrays considering the all invocations. </summary>
        /// <returns>pair of event arrays, the first in the pair is the insert stream data, the second in the pair is the remove stream data</returns>
        public UniformPair<EventBean[]> GetDataListsFlattened()
        {
            lock (this)
            {
                return new UniformPair<EventBean[]>(Flatten(_newDataList), Flatten(_oldDataList));
            }
        }

        /// <summary>Returns a pair of insert and remove stream event arrays considering the all invocations, and resets the listener. </summary>
        /// <returns>pair of event arrays, the first in the pair is the insert stream data, the second in the pair is the remove stream data</returns>
        public UniformPair<EventBean[]> GetAndResetDataListsFlattened()
        {
            lock (this)
            {
                UniformPair<EventBean[]> pair = GetDataListsFlattened();
                Reset();
                return pair;
            }
        }

        /// <summary>
        /// Produce an array of listeners
        /// </summary>
        /// <param name="size">The size.</param>
        /// <returns></returns>
        public static SupportUpdateListener[] MakeListeners(int size)
        {
            SupportUpdateListener[] listeners = new SupportUpdateListener[size];
            for (int i = 0; i < listeners.Length; i++)
            {
                listeners[i] = new SupportUpdateListener();
            }
            return listeners;
        }
    }

    public static class SupportUpdateListenerExtensions
    {
        public static EPStatement AddListener(this EPStatement statement, SupportUpdateListener listener)
        {
            statement.Events += listener.Update;
            return statement;
        }
    }
}
