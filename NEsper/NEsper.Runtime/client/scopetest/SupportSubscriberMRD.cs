///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.runtime.client.scopetest
{
    /// <summary>EPSubscriber for multi-row delivery that retains the events it receives for use in assertions. </summary>
    public class SupportSubscriberMRD
    {
        private readonly List<object[][]> _insertStreamList = new List<object[][]>();
        private readonly List<object[][]> _removeStreamList = new List<object[][]>();
        private bool _isInvoked;

        /// <summary>
        /// Receive multi-row subscriber data through this methods.
        /// </summary>
        /// <param name="insertStream">new data</param>
        /// <param name="removeStream">removed data</param>
        public void Update(object[][] insertStream, object[][] removeStream)
        {
            lock (this)
            {
                _isInvoked = true;
                _insertStreamList.Add(insertStream);
                _removeStreamList.Add(insertStream);
            }
        }

        /// <summary>
        /// Returns all insert-stream events received so far.
        /// <para/>
        /// The list contains an item for each delivery. Each item contains a row with the event and each event is itself a tuple (object array).
        /// </summary>
        /// <value>list of Object array-array</value>
        public IList<object[][]> InsertStreamList
        {
            get { return _insertStreamList; }
        }

        /// <summary>
        /// Returns all removed-stream events received so far.
        /// <para/>
        /// The list contains an item for each delivery. Each item contains a row with the event and each event is itself a tuple (object array).
        /// </summary>
        /// <value>list of Object array-array</value>
        public IList<object[][]> RemoveStreamList
        {
            get { return _removeStreamList; }
        }

        /// <summary>
        /// Reset subscriber, clearing all associated state.
        /// </summary>
        public void Reset()
        {
            lock (this)
            {
                _isInvoked = false;
                _insertStreamList.Clear();
                _removeStreamList.Clear();
            }
        }

        /// <summary>
        /// Returns true if the subscriber was invoked at least once.
        /// </summary>
        /// <returns>invoked flag</returns>
        public bool IsInvoked()
        {
            return _isInvoked;
        }

        /// <summary>
        /// Returns true if the subscriber  was invoked at least once and clears the invocation flag.
        /// </summary>
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
    }
}