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
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.events;
using com.espertech.esper.events.arr;
using com.espertech.esper.events.map;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.events
{
    using Map = IDictionary<string, object>;

    [TestFixture]
    public class TestMapObjectArrayInterUse 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            _listener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }
    
        // test ObjectArray event with Map, Map[], MapType and MapType[] properties
        [Test]
        public void TestObjectArrayWithMap()
        {
            _epService.EPAdministrator.Configuration.AddEventType("MapType", Collections.SingletonDataMap("im", typeof(String)));
            _epService.EPAdministrator.Configuration.AddEventType("OAType", "p0,p1,p2,p3".Split(','), new Object[] {typeof(String), "MapType", "MapType[]", Collections.SingletonDataMap("om", typeof(String))});
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select p0 as c0, p1.im as c1, p2[0].im as c2, p3.om as c3 from OAType");
            stmt.Events += _listener.Update;
            
            _epService.EPRuntime.SendEvent(new Object[] {"E1", Collections.SingletonDataMap("im", "IM1"), new Map[] {Collections.SingletonDataMap("im", "IM2")}, Collections.SingletonDataMap("om", "OM1")}, "OAType");
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "c0,c1,c2,c3".Split(','), new Object[]{"E1", "IM1", "IM2", "OM1"});
    
            _epService.EPAdministrator.DestroyAllStatements();
            
            // test inserting from array to map
            _epService.EPAdministrator.CreateEPL("insert into MapType(im) select p0 from OAType").Events += _listener.Update;
            _epService.EPRuntime.SendEvent(new Object[]{"E1", null, null, null}, "OAType");
            Assert.That(_listener.AssertOneGetNew(), Is.InstanceOf<MappedEventBean>());
            Assert.AreEqual("E1", _listener.AssertOneGetNew().Get("im"));
        }
    
        // test Map event with ObjectArrayType and ObjectArrayType[] properties
        [Test]
        public void TestMapWithObjectArray()
        {
            _epService.EPAdministrator.Configuration.AddEventType("OAType", "p0,p1".Split(','), new Object[] {typeof(String), typeof(int)});
            IDictionary<String, Object> def = new Dictionary<String, Object>();
            def["oa1"] = "OAType";
            def["oa2"] = "OAType[]";
            _epService.EPAdministrator.Configuration.AddEventType("MapType", def);
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select oa1.p0 as c0, oa1.p1 as c1, oa2[0].p0 as c2, oa2[1].p1 as c3 from MapType");
            stmt.Events += _listener.Update;
            
            IDictionary<String, Object> data = new Dictionary<String, Object>();
            data["oa1"] = new Object[] {"A", 100};
            data["oa2"] = new Object[][] { new Object[] {"B", 200}, new Object[] {"C", 300} };
            _epService.EPRuntime.SendEvent(data, "MapType");
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "c0,c1,c2,c3".Split(','), new Object[] {"A", 100, "B", 300});
    
            _epService.EPAdministrator.DestroyAllStatements();
            
            // test inserting from map to array
            _epService.EPAdministrator.CreateEPL("insert into OAType select 'a' as p0, 1 as p1 from MapType").Events += _listener.Update;
            _epService.EPRuntime.SendEvent(data, "MapType");
            Assert.That(_listener.AssertOneGetNew(), Is.InstanceOf<ObjectArrayBackedEventBean>());
            Assert.AreEqual("a", _listener.AssertOneGetNew().Get("p0"));
            Assert.AreEqual(1, _listener.AssertOneGetNew().Get("p1"));
        }
    }
}
