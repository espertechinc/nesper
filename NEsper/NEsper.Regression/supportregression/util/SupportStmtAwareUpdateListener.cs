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
using com.espertech.esper.collection;

using NUnit.Framework;

namespace com.espertech.esper.supportregression.util
{
    public class SupportStmtAwareUpdateListener
    {
        private readonly Guid _id = Guid.NewGuid();
        private readonly IList<EPStatement> _statementList;
        private readonly IList<EPServiceProvider> _svcProviderList;
        private readonly IList<EventBean[]> _newDataList;
        private readonly IList<EventBean[]> _oldDataList;
        private EventBean[] _lastNewData;
        private EventBean[] _lastOldData;
        private bool _isInvoked;
    
        public SupportStmtAwareUpdateListener()
        {
            _newDataList = new List<EventBean[]>();
            _oldDataList = new List<EventBean[]>();
            _statementList = new List<EPStatement>();
            _svcProviderList = new List<EPServiceProvider>();
        }
    
        public void Update(Object sender, UpdateEventArgs e)
        {
            _statementList.Add(e.Statement);
            _svcProviderList.Add(e.ServiceProvider);
    
            this._oldDataList.Add(e.OldEvents);
            this._newDataList.Add(e.NewEvents);
    
            this._lastNewData = e.NewEvents;
            this._lastOldData = e.OldEvents;
    
            _isInvoked = true;
        }
    
        public void Reset()
        {
            _statementList.Clear();
            _svcProviderList.Clear();
            this._oldDataList.Clear();
            this._newDataList.Clear();
            this._lastNewData = null;
            this._lastOldData = null;
            _isInvoked = false;
        }
    
        public EventBean[] GetLastNewData()
        {
            return _lastNewData;
        }
    
        public EventBean[] GetAndResetLastNewData()
        {
            EventBean[] lastNew = _lastNewData;
            Reset();
            return lastNew;
        }

        public IList<EPStatement> StatementList
        {
            get { return _statementList; }
        }

        public IList<EPServiceProvider> SvcProviderList
        {
            get { return _svcProviderList; }
        }

        public EventBean AssertOneGetNewAndReset()
        {
            Assert.IsTrue(_isInvoked);
    
            Assert.AreEqual(1, _newDataList.Count);
            Assert.AreEqual(1, _oldDataList.Count);
    
            Assert.AreEqual(1, _lastNewData.Length);
            Assert.IsNull(_lastOldData);
    
            EventBean lastNew = _lastNewData[0];
            Reset();
            return lastNew;
        }
    
        public EventBean AssertOneGetOldAndReset()
        {
            Assert.IsTrue(_isInvoked);
    
            Assert.AreEqual(1, _newDataList.Count);
            Assert.AreEqual(1, _oldDataList.Count);
    
            Assert.AreEqual(1, _lastOldData.Length);
            Assert.IsNull(_lastNewData);
    
            EventBean lastNew = _lastOldData[0];
            Reset();
            return lastNew;
        }

        public EventBean[] LastOldData {
            get { return _lastOldData; }
        }

        public IList<EventBean[]> NewDataList {
            get { return _newDataList; }
        }

        public IList<EventBean[]> OldDataList {
            get { return _oldDataList; }
        }

        public bool IsInvoked
        {
            get { return _isInvoked; }
        }

        public bool GetAndClearIsInvoked()
        {
            bool invoked = _isInvoked;
            _isInvoked = false;
            return invoked;
        }
    
        public void SetLastNewData(EventBean[] lastNewData)
        {
            this._lastNewData = lastNewData;
        }
    
        public void SetLastOldData(EventBean[] lastOldData)
        {
            this._lastOldData = lastOldData;
        }
    
        public EventBean[] GetNewDataListFlattened()
        {
            return Flatten(_newDataList);
        }
    
        private EventBean[] Flatten(IList<EventBean[]> list)
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
    
        public void AssertUnderlyingAndReset(object[] expectedUnderlyingNew, object[] expectedUnderlyingOld)
        {
            Assert.AreEqual(1, NewDataList.Count);
            Assert.AreEqual(1, OldDataList.Count);
    
            EventBean[] newEvents = GetLastNewData();
            EventBean[] oldEvents = LastOldData;
    
            if (expectedUnderlyingNew != null)
            {
                Assert.AreEqual(expectedUnderlyingNew.Length, newEvents.Length);
                for (int i = 0; i < expectedUnderlyingNew.Length; i++)
                {
                    Assert.AreSame(expectedUnderlyingNew[i], newEvents[i].Underlying);
                }
            }
            else
            {
                Assert.IsNull(newEvents);
            }
    
            if (expectedUnderlyingOld != null)
            {
                Assert.AreEqual(expectedUnderlyingOld.Length, oldEvents.Length);
                for (int i = 0; i < expectedUnderlyingOld.Length; i++)
                {
                    Assert.AreSame(expectedUnderlyingOld[i], oldEvents[i].Underlying);
                }
            }
            else
            {
                Assert.IsNull(oldEvents);
            }
    
            Reset();
        }
    
        public void AssertFieldEqualsAndReset(String fieldName, object[] expectedNew, object[] expectedOld)
        {
            Assert.AreEqual(1, NewDataList.Count);
            Assert.AreEqual(1, OldDataList.Count);
    
            EventBean[] newEvents = GetLastNewData();
            EventBean[] oldEvents = LastOldData;
    
            if (expectedNew != null)
            {
                Assert.AreEqual(expectedNew.Length, newEvents.Length);
                for (int i = 0; i < expectedNew.Length; i++)
                {
                    Object result = newEvents[i].Get(fieldName);
                    Assert.AreEqual(expectedNew[i], result);
                }
            }
            else
            {
                Assert.IsNull(newEvents);
            }
    
            if (expectedOld != null)
            {
                Assert.AreEqual(expectedOld.Length, oldEvents.Length);
                for (int i = 0; i < expectedOld.Length; i++)
                {
                    Assert.AreEqual(expectedOld[i], oldEvents[i].Get(fieldName));
                }
            }
            else
            {
                Assert.IsNull(oldEvents);
            }
    
            Reset();
        }
    
        public UniformPair<EventBean[]> GetDataListsFlattened()
        {
            return new UniformPair<EventBean[]>(Flatten(_newDataList), Flatten(_oldDataList));
        }
    }
}
