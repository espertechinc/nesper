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
using com.espertech.esper.epl.join.exec.@base;
using com.espertech.esper.epl.join.table;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.events;

using NUnit.Framework;

namespace com.espertech.esper.epl.join.exec
{
    [TestFixture]
    public class TestIndexedTableLookupStrategy 
    {
        private EventType _eventType;
        private IndexedTableLookupStrategy _lookupStrategy;
        private PropertyIndexedEventTable _propertyMapEventIndex;
    
        [SetUp]
        public void SetUp()
        {
            _eventType = SupportEventTypeFactory.CreateBeanType(typeof(SupportBean));

            PropertyIndexedEventTableFactory factory = new PropertyIndexedEventTableFactory(0, _eventType, new String[] { "TheString", "IntPrimitive" }, false, null);
            _propertyMapEventIndex = (PropertyIndexedEventTable) factory.MakeEventTables(null, null)[0];
            _lookupStrategy = new IndexedTableLookupStrategy(_eventType, new String[] {"TheString", "IntPrimitive"}, _propertyMapEventIndex);
    
            _propertyMapEventIndex.Add(new EventBean[] {SupportEventBeanFactory.CreateObject(new SupportBean("a", 1))}, null);
        }
    
        [Test]
        public void TestLookup()
        {
            ICollection<EventBean> events = _lookupStrategy.Lookup(SupportEventBeanFactory.CreateObject(new SupportBean("a", 1)), null, null);
    
            Assert.AreEqual(1, events.Count);
        }
    
        [Test]
        public void TestInvalid()
        {
            try
            {
                new IndexedTableLookupStrategy(_eventType, new String[] { "TheString", "xxx" }, _propertyMapEventIndex);
                Assert.Fail();
            }
            catch (ArgumentException)
            {
                // expected
            }
        }
    }
}
