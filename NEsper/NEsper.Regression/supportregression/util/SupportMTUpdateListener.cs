///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.threading;

using NUnit.Framework;

namespace com.espertech.esper.supportregression.util
{
    public class SupportMTUpdateListener
    {
        private readonly string _id;
        private readonly IList<EventBean[]> _newDataList;
        private readonly IList<EventBean[]> _oldDataList;
        private EventBean[] _lastNewData;
        private EventBean[] _lastOldData;
        private bool _isInvoked;
        private readonly ILockable _oLock;

        public SupportMTUpdateListener(string name)
        {
            _id = name;
            _oLock = SupportContainer.Instance.LockManager()
                .CreateLock(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            _newDataList = new List<EventBean[]>();
            _oldDataList = new List<EventBean[]>();
        }

        public SupportMTUpdateListener()
        {
            _id = string.Empty;
            _oLock = SupportContainer.Instance.LockManager()
                .CreateLock(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            _newDataList = new List<EventBean[]>();
            _oldDataList = new List<EventBean[]>();
        }
    
        public void Update(Object sender, UpdateEventArgs e)
        {
            var oldData = e.OldEvents; 
            var newData = e.NewEvents;

            using(_oLock.Acquire()) {
                _oldDataList.Add(oldData);
                _newDataList.Add(newData);
                _lastNewData = newData;
                _lastOldData = oldData;
                _isInvoked = true;
            }
        }
    
        public void Reset()
        {
            using(_oLock.Acquire()) {
                _oldDataList.Clear();
                _newDataList.Clear();
                _lastNewData = null;
                _lastOldData = null;
                _isInvoked = false;
            }
        }
    
        public EventBean[] GetLastNewData()
        {
            return _lastNewData;
        }
    
        public EventBean[] GetAndResetLastNewData()
        {
            using(_oLock.Acquire()) {
                EventBean[] lastNew = _lastNewData;
                Reset();
                return lastNew;
            }
        }
    
        public EventBean AssertOneGetNewAndReset()
        {
            using(_oLock.Acquire()) {
                Assert.IsTrue(_isInvoked);

                Assert.AreEqual(1, _newDataList.Count);
                Assert.AreEqual(1, _oldDataList.Count);

                Assert.AreEqual(1, _lastNewData.Length);
                Assert.IsNull(_lastOldData);

                EventBean lastNew = _lastNewData[0];
                Reset();
                return lastNew;
            }
        }
    
        public EventBean AssertOneGetOldAndReset()
        {
            using(_oLock.Acquire()) {
                Assert.IsTrue(_isInvoked);

                Assert.AreEqual(1, _newDataList.Count);
                Assert.AreEqual(1, _oldDataList.Count);

                Assert.AreEqual(1, _lastOldData.Length);
                Assert.IsNull(_lastNewData);

                EventBean lastNew = _lastOldData[0];
                Reset();
                return lastNew;
            }
        }

        public EventBean[] LastOldData {
            get { return _lastOldData; }
        }

        public IList<EventBean[]> NewDataList {
            get { return _newDataList; }
        }

        public IList<EventBean[]> NewDataListCopy {
            get {
                using (_oLock.Acquire()) {
                    return new List<EventBean[]>(_newDataList);
                }
            }
        }

        public IList<EventBean[]> OldDataList {
            get { return _oldDataList; }
        }

        public bool IsInvoked {
            get { return _isInvoked; }
        }

        public bool GetAndClearIsInvoked()
        {
            using(_oLock.Acquire()) {
                bool invoked = _isInvoked;
                _isInvoked = false;
                return invoked;
            }
        }
    
        public EventBean[] GetNewDataListFlattened()
        {
            using(_oLock.Acquire()) {
                return Flatten(_newDataList);
            }
        }
    
        public EventBean[] GetOldDataListFlattened()
        {
            using (_oLock.Acquire()) {
                return Flatten(_oldDataList);
            }
        }
    
        private static EventBean[] Flatten(IEnumerable<EventBean[]> list)
        {
            int count = 0;
            foreach (EventBean[] events in list)
            {
                if (events != null)
                {
                    count += events.Length;
                }
            }
    
            EventBean[] array = new EventBean[count];
            count = 0;
            foreach (EventBean[] events in list)
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
