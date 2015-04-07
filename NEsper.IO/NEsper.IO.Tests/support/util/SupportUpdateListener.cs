///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;

using NUnit.Framework;

namespace com.espertech.esperio.support.util
{
    public class SupportUpdateListener
    {
        private readonly IList<EventBean[]> newDataList;
        private readonly IList<EventBean[]> oldDataList;
        private EventBean[] lastNewData;
        private EventBean[] lastOldData;
        private bool isInvoked;
    
        public SupportUpdateListener()
        {
            newDataList = new List<EventBean[]>();
            oldDataList = new List<EventBean[]>();
        }

        public void Update(Object sender, UpdateEventArgs e)
        {
            var oldData = e.OldEvents;
            var newData = e.NewEvents;

            this.oldDataList.Add(oldData);
            this.newDataList.Add(newData);
    
            this.lastNewData = newData;
            this.lastOldData = oldData;
    
            isInvoked = true;
        }
    
        public void Reset()
        {
            this.oldDataList.Clear();
            this.newDataList.Clear();
            this.lastNewData = null;
            this.lastOldData = null;
            isInvoked = false;
        }
    
        public EventBean[] GetLastNewData()
        {
            return lastNewData;
        }
    
        public EventBean[] GetAndResetLastNewData()
        {
            EventBean[] lastNew = lastNewData;
            Reset();
            return lastNew;
        }
    
        public EventBean AssertOneGetNewAndReset()
        {
            Assert.IsTrue(isInvoked);
    
            Assert.AreEqual(1, newDataList.Count);
            Assert.AreEqual(1, oldDataList.Count);
    
            Assert.AreEqual(1, lastNewData.Length);
            Assert.IsNull(lastOldData);
    
            EventBean lastNew = lastNewData[0];
            Reset();
            return lastNew;
        }
    
        public EventBean AssertOneGetOldAndReset()
        {
            Assert.IsTrue(isInvoked);
    
            Assert.AreEqual(1, newDataList.Count);
            Assert.AreEqual(1, oldDataList.Count);
    
            Assert.AreEqual(1, lastOldData.Length);
            Assert.IsNull(lastNewData);
    
            EventBean lastNew = lastOldData[0];
            Reset();
            return lastNew;
        }
    
        public EventBean[] GetLastOldData()
        {
            return lastOldData;
        }
    
        public IList<EventBean[]> GetNewDataList()
        {
            return newDataList;
        }
    
        public IList<EventBean[]> GetOldDataList()
        {
            return oldDataList;
        }
    
        public bool IsInvoked()
        {
            return isInvoked;
        }
    
        public bool GetAndClearIsInvoked()
        {
            bool invoked = isInvoked;
            isInvoked = false;
            return invoked;
        }
    
        public void SetLastNewData(EventBean[] lastNewData)
        {
            this.lastNewData = lastNewData;
        }
    
        public void SetLastOldData(EventBean[] lastOldData)
        {
            this.lastOldData = lastOldData;
        }
    }
}
