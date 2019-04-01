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
using com.espertech.esper.epl.join.exec.@base;
using com.espertech.esper.epl.join.table;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.events;

using NUnit.Framework;

namespace com.espertech.esper.epl.join.exec
{
    [TestFixture]
    public class TestTableLookupExecNode 
    {
        private TableLookupExecNode _exec;
        private PropertyIndexedEventTable _index;
    
        [SetUp]
        public void SetUp()
        {
            var eventTypeIndex = SupportEventTypeFactory.CreateBeanType(typeof(SupportBean));
            var factory = new PropertyIndexedEventTableFactory(0, eventTypeIndex, new String[] {"TheString"}, false, null);
            _index = (PropertyIndexedEventTable) factory.MakeEventTables(null, null)[0];
    
            var eventTypeKeyGen = SupportEventTypeFactory.CreateBeanType(typeof(SupportMarketDataBean));
    
            _exec = new TableLookupExecNode(1, new IndexedTableLookupStrategy(eventTypeKeyGen, new String[] {"Symbol"}, _index));
        }
    
        [Test]
        public void TestFlow()
        {
            var indexEvents = SupportEventBeanFactory.MakeEvents(new String[] {"a1", "a2"});
            _index.Add(indexEvents, null);
    
            var lookupEvents = SupportEventBeanFactory.MakeMarketDataEvents(new String[] {"a2", "a3"});
    
            var result = new List<EventBean[]>();
            var prefill = new EventBean[] {lookupEvents[0], null};
            _exec.Process(lookupEvents[0], prefill, result, null);
    
            // Test lookup found 1 row
            Assert.AreEqual(1, result.Count);
            var events = result.FirstOrDefault();
            Assert.AreSame(indexEvents[1], events[1]);
            Assert.AreSame(lookupEvents[0], events[0]);
    
            // Test lookup found no rows
            result.Clear();
            _exec.Process(lookupEvents[1], prefill, result, null);
            Assert.AreEqual(0, result.Count);
        }
    }
}
