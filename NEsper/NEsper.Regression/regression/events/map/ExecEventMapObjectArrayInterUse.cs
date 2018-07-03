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
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.events.map
{
    using Map = IDictionary<string, object>;

    public class ExecEventMapObjectArrayInterUse : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionObjectArrayWithMap(epService);
            RunAssertionMapWithObjectArray(epService);
        }
    
        // test ObjectArray event with Map, Map[], MapType and MapType[] properties
        private void RunAssertionObjectArrayWithMap(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("MapType", Collections.SingletonDataMap("im", typeof(string)));
            epService.EPAdministrator.Configuration.AddEventType("OAType", "p0,p1,p2,p3".Split(','), new object[]{typeof(string), "MapType", "MapType[]", Collections.SingletonDataMap("om", typeof(string))});
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select p0 as c0, p1.im as c1, p2[0].im as c2, p3.om as c3 from OAType");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            epService.EPRuntime.SendEvent(
                new object[] {
                    "E1",
                    Collections.SingletonMap("im", "IM1"),
                    new Map[] { Collections.SingletonDataMap("im", "IM2") },
                    Collections.SingletonDataMap("om", "OM1")
                }, "OAType");
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), 
                "c0,c1,c2,c3".Split(','), 
                new object[]{"E1", "IM1", "IM2", "OM1"});
    
            epService.EPAdministrator.DestroyAllStatements();
    
            // test inserting from array to map
            epService.EPAdministrator.CreateEPL("insert into MapType(im) select p0 from OAType").Events += listener.Update;
            epService.EPRuntime.SendEvent(new object[]{"E1", null, null, null}, "OAType");
            Assert.IsTrue(listener.AssertOneGetNew() is MappedEventBean);
            Assert.AreEqual("E1", listener.AssertOneGetNewAndReset().Get("im"));
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MapType", false);
            epService.EPAdministrator.Configuration.RemoveEventType("OAType", false);
        }
    
        // test Map event with ObjectArrayType and ObjectArrayType[] properties
        private void RunAssertionMapWithObjectArray(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("OAType", "p0,p1".Split(','), new object[]{typeof(string), typeof(int?)});
            var def = new Dictionary<string, Object>();
            def.Put("oa1", "OAType");
            def.Put("oa2", "OAType[]");
            epService.EPAdministrator.Configuration.AddEventType("MapType", def);
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select oa1.p0 as c0, oa1.p1 as c1, oa2[0].p0 as c2, oa2[1].p1 as c3 from MapType");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var data = new Dictionary<string, Object>();
            data.Put("oa1", new object[]{"A", 100});
            data.Put("oa2", new object[][]{new object[] {"B", 200}, new object[] {"C", 300}});
            epService.EPRuntime.SendEvent(data, "MapType");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "c0,c1,c2,c3".Split(','), new object[]{"A", 100, "B", 300});
    
            epService.EPAdministrator.DestroyAllStatements();
    
            // test inserting from map to array
            epService.EPAdministrator.CreateEPL("insert into OAType select 'a' as p0, 1 as p1 from MapType").Events += listener.Update;
            epService.EPRuntime.SendEvent(data, "MapType");
            Assert.IsTrue(listener.AssertOneGetNew() is ObjectArrayBackedEventBean);
            Assert.AreEqual("a", listener.AssertOneGetNew().Get("p0"));
            Assert.AreEqual(1, listener.AssertOneGetNewAndReset().Get("p1"));
        }
    }
} // end of namespace
