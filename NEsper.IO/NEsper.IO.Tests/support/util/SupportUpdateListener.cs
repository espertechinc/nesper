///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

namespace com.espertech.esperio.support.util
{
    public class SupportUpdateListener
    {
        private readonly IList<EventBean[]> _newDataList;
        private readonly IList<EventBean[]> _oldDataList;
        private EventBean[] _lastNewData;
        private EventBean[] _lastOldData;
        private bool _isInvoked;
    
        public SupportUpdateListener()
        {
            _newDataList = new List<EventBean[]>();
            _oldDataList = new List<EventBean[]>();
        }

        public void Update(Object sender, UpdateEventArgs e)
        {
            var oldData = e.OldEvents;
            var newData = e.NewEvents;

            this._oldDataList.Add(oldData);
            this._newDataList.Add(newData);
    
            this._lastNewData = newData;
            this._lastOldData = oldData;
    
            _isInvoked = true;
        }
    
        public void Reset()
        {
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
            var lastNew = _lastNewData;
            Reset();
            return lastNew;
        }
    
        public EventBean AssertOneGetNewAndReset()
        {
            Assert.IsTrue(_isInvoked);
    
            Assert.AreEqual(1, _newDataList.Count);
            Assert.AreEqual(1, _oldDataList.Count);
    
            Assert.AreEqual(1, _lastNewData.Length);
            Assert.IsNull(_lastOldData);
    
            var lastNew = _lastNewData[0];
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
    
            var lastNew = _lastOldData[0];
            Reset();
            return lastNew;
        }
    
        public EventBean[] GetLastOldData()
        {
            return _lastOldData;
        }
    
        public IList<EventBean[]> GetNewDataList()
        {
            return _newDataList;
        }
    
        public IList<EventBean[]> GetOldDataList()
        {
            return _oldDataList;
        }
    
        public bool IsInvoked()
        {
            return _isInvoked;
        }
    
        public bool GetAndClearIsInvoked()
        {
            var invoked = _isInvoked;
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
    }
}
