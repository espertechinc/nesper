///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.join.exec.@base;
using com.espertech.esper.epl.join.table;
using com.espertech.esper.support.events;

using NUnit.Framework;




namespace com.espertech.esper.epl.join.exec
{
    [TestFixture]
    public class TestTableOuterLookupExecNode 
    {
        private TableOuterLookupExecNode exec;
        private UnindexedEventTable index;
    
        [SetUp]
        public void SetUp()
        {
            index = new UnindexedEventTable(0);
            exec = new TableOuterLookupExecNode(1, new FullTableScanLookupStrategy(index));
        }
    
        [Test]
        public void TestFlow()
        {
            EventBean[] lookupEvents = SupportEventBeanFactory.MakeMarketDataEvents(new String[] {"a2"});
            List<EventBean[]> result = new List<EventBean[]>();
            EventBean[] prefill = new EventBean[] {lookupEvents[0], null};
    
            // Test lookup on empty index, expect 1 row
            exec.Process(lookupEvents[0], prefill, result, null);
            Assert.AreEqual(1, result.Count);
            EventBean[] events = result.FirstOrDefault();
            Assert.IsNull(events[1]);
            Assert.AreSame(lookupEvents[0], events[0]);
            result.Clear();
    
            // Test lookup on filled index, expect row2
            EventBean[] indexEvents = SupportEventBeanFactory.MakeEvents(new String[] {"a1", "a2"});
            index.Add(indexEvents);
            exec.Process(lookupEvents[0], prefill, result, null);
            Assert.AreEqual(2, result.Count);
    
            IEnumerator<EventBean[]> it = result.GetEnumerator();
    
            events = it.Advance();
            Assert.AreSame(lookupEvents[0], events[0]);
            Assert.IsTrue((indexEvents[0] == events[1]) || (indexEvents[1] == events[1]));

            events = it.Advance();
            Assert.AreSame(lookupEvents[0], events[0]);
            Assert.IsTrue((indexEvents[0] == events[1]) || (indexEvents[1] == events[1]));
        }
    }
}
