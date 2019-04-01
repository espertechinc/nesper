///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.supportunit.events;

using NUnit.Framework;

namespace com.espertech.esper.epl.join.exec
{
    [TestFixture]
    public class TestTableOuterLookupExecNode 
    {
        private TableOuterLookupExecNode _exec;
        private UnindexedEventTable _index;
    
        [SetUp]
        public void SetUp()
        {
            _index = new UnindexedEventTableImpl(0);
            _exec = new TableOuterLookupExecNode(1, new FullTableScanLookupStrategy(_index));
        }
    
        [Test]
        public void TestFlow()
        {
            var lookupEvents = SupportEventBeanFactory.MakeMarketDataEvents(new String[] {"a2"});
            var result = new List<EventBean[]>();
            var prefill = new EventBean[] {lookupEvents[0], null};
    
            // Test lookup on empty index, expect 1 row
            _exec.Process(lookupEvents[0], prefill, result, null);
            Assert.AreEqual(1, result.Count);
            var events = result.FirstOrDefault();
            Assert.IsNull(events[1]);
            Assert.AreSame(lookupEvents[0], events[0]);
            result.Clear();
    
            // Test lookup on filled index, expect row2
            var indexEvents = SupportEventBeanFactory.MakeEvents(new String[] {"a1", "a2"});
            _index.Add(indexEvents, null);
            _exec.Process(lookupEvents[0], prefill, result, null);
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
