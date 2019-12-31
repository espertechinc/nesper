///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.collection;

namespace com.espertech.esper.runtime.client.scopetest
{
    /// <summary>
    /// EPSubscriber for that retains the events it receives for use in assertions.
    /// </summary>
    public class SupportSubscriber
    {
        private readonly IList<object[]> _newDataList;
        private readonly IList<object[]> _oldDataList;
        private object[] _lastNewData;
        private object[] _lastOldData;
        private bool _isInvoked;

        /// <summary>Ctor. </summary>
        public SupportSubscriber()
        {
            _newDataList = new List<object[]>();
            _oldDataList = new List<object[]>();
        }

        /// <summary>Receive events. </summary>
        /// <param name="newData">insert stream</param>
        /// <param name="oldData">remove stream</param>
        public void Update(object[] newData, object[] oldData)
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

        /// <summary>
        /// Reset subscriber, clearing all associated state.
        /// </summary>
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
        /// <value>insert stream events or null if either a null value was received or when no events have been received since the last reset</value>
        public object[] LastNewData
        {
            get { return _lastNewData; }
        }

        /// <summary>Returns the last array of events (insert stream) that were received and resets the subscriber. </summary>
        /// <returns>insert stream events or null if either a null value was received or when no events have been received since the last reset</returns>
        public object[] GetAndResetLastNewData()
        {
            lock (this)
            {
                object[] lastNew = _lastNewData;
                Reset();
                return lastNew;
            }
        }

        /// <summary>Asserts that exactly one insert stream event was received and no remove stream events, resets the listener clearing all state and returns the received event. </summary>
        /// <returns>single insert-stream event</returns>
        public object AssertOneGetNewAndReset()
        {
            lock (this)
            {
                ScopeTestHelper.AssertTrue("EPSubscriber invocation not received but expected", _isInvoked);

                ScopeTestHelper.AssertEquals("Mismatch in the number of invocations", 1, _newDataList.Count);
                ScopeTestHelper.AssertEquals("Mismatch in the number of invocations", 1, _oldDataList.Count);

                if (_lastNewData == null)
                {
                    ScopeTestHelper.Fail("No new-data events received");
                }

                ScopeTestHelper.AssertEquals("Mismatch in the number of new-data events", 1, _lastNewData.Length);
                ScopeTestHelper.AssertNull("No old-data events are expected but some were received", _lastOldData);

                object lastNew = _lastNewData[0];
                Reset();
                return lastNew;
            }
        }

        /// <summary>Asserts that exactly one remove stream event was received and no insert stream events, resets the listener clearing all state and returns the received event. </summary>
        /// <returns>single remove-stream event</returns>
        public object AssertOneGetOldAndReset()
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

            object lastNew = _lastOldData[0];
            Reset();
            return lastNew;
        }

        /// <summary>Returns the last array of remove-stream events that were received. </summary>
        /// <value>remove stream events or null if either a null value was received or when no events have been received since the last reset</value>
        public object[] LastOldData
        {
            get { return _lastOldData; }
        }

        /// <summary>Get a list of all insert-stream event arrays received. </summary>
        /// <value>list of event arrays</value>
        public IList<object[]> NewDataList
        {
            get { return _newDataList; }
        }

        /// <summary>Get a list of all remove-stream event arrays received. </summary>
        /// <value>list of event arrays</value>
        public IList<object[]> OldDataList
        {
            get { return _oldDataList; }
        }

        /// <summary>Returns true if the subscriber was invoked at least once. </summary>
        /// <value>invoked flag</value>
        public bool IsInvoked
        {
            get { return _isInvoked; }
        }

        /// <summary>Returns true if the subscriber was invoked at least once and clears the invocation flag. </summary>
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

        /// <summary>Returns an event array that represents all insert-stream events received so far. </summary>
        /// <value>event array</value>
        public object[] NewDataListFlattened {
            get {
                lock (this) {
                    return Flatten(_newDataList);
                }
            }
        }

        /// <summary>Returns an event array that represents all remove-stream events received so far. </summary>
        /// <value>event array</value>
        public object[] OldDataListFlattened {
            get {
                lock (this) {
                    return Flatten(_oldDataList);
                }
            }
        }

        /// <summary>Returns a pair of insert and remove stream event arrays considering the all invocations. </summary>
        /// <value>pair of event arrays, the first in the pair is the insert stream data, the second in the pair is the remove stream data</value>
        public UniformPair<object[]> DataListsFlattened {
            get {
                lock (this) {
                    return new UniformPair<object[]>(Flatten(_newDataList), Flatten(_oldDataList));
                }
            }
        }

        private static object[] Flatten(IEnumerable<object[]> list)
        {
            int count = list
                .Where(events => events != null)
                .Sum(events => events.Length);

            var array = new object[count];
            count = 0;
            foreach (object[] events in list)
            {
                if (events != null)
                {
                    for (int i = 0; i < events.Length; i++)
                    {
                        array[count++] = events[i];
                    }
                }
            }
            return array;
        }
    }
}