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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.events.bean
{
    using Map = IDictionary<string, object>;

    public class ExecEventBeanMappedIndexedPropertyExpression : RegressionExecution {
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBeanComplexProps));
            var listener = new SupportUpdateListener();
    
            // test bean-type
            string eplBeans = "select " +
                    "Mapped(TheString) as val0, " +
                    "Indexed(IntPrimitive) as val1 " +
                    "from SupportBeanComplexProps#lastevent, SupportBean sb unidirectional";
            RunAssertionBean(epService, listener, eplBeans);
    
            // test bean-type prefixed
            string eplBeansPrefixed = "select " +
                    "sbcp.Mapped(TheString) as val0, " +
                    "sbcp.Indexed(IntPrimitive) as val1 " +
                    "from SupportBeanComplexProps#lastevent sbcp, SupportBean sb unidirectional";
            RunAssertionBean(epService, listener, eplBeansPrefixed);
    
            // test wrap
            epService.EPAdministrator.CreateEPL("insert into SecondStream select 'a' as val0, * from SupportBeanComplexProps");
    
            string eplWrap = "select " +
                    "Mapped(TheString) as val0," +
                    "Indexed(IntPrimitive) as val1 " +
                    "from SecondStream #lastevent, SupportBean unidirectional";
            RunAssertionBean(epService, listener, eplWrap);
    
            string eplWrapPrefixed = "select " +
                    "sbcp.Mapped(TheString) as val0," +
                    "sbcp.Indexed(IntPrimitive) as val1 " +
                    "from SecondStream #lastevent sbcp, SupportBean unidirectional";
            RunAssertionBean(epService, listener, eplWrapPrefixed);
    
            // test Map-type
            var def = new Dictionary<string, Object>();
            def.Put("mapped", new Dictionary<string, object>());
            def.Put("indexed", typeof(int[]));
            epService.EPAdministrator.Configuration.AddEventType("MapEvent", def);
    
            string eplMap = "select " +
                    "mapped(TheString) as val0," +
                    "indexed(IntPrimitive) as val1 " +
                    "from MapEvent#lastevent, SupportBean unidirectional";
            RunAssertionMap(epService, listener, eplMap);
    
            string eplMapPrefixed = "select " +
                    "sbcp.mapped(TheString) as val0," +
                    "sbcp.indexed(IntPrimitive) as val1 " +
                    "from MapEvent#lastevent sbcp, SupportBean unidirectional";
            RunAssertionMap(epService, listener, eplMapPrefixed);
    
            // test insert-int
            var defType = new Dictionary<string, Object>();
            defType.Put("name", typeof(string));
            defType.Put("value", typeof(string));
            defType.Put("properties", typeof(Map));
            epService.EPAdministrator.Configuration.AddEventType("InputEvent", defType);
            epService.EPAdministrator.CreateEPL("select name,value,properties(name) = value as ok from InputEvent").Events += listener.Update;
    
            listener.Reset();
            epService.EPRuntime.SendEvent(MakeMapEvent("name", "value1", Collections.SingletonDataMap("name", "xxxx")), "InputEvent");
            Assert.IsFalse((bool?) listener.AssertOneGetNewAndReset().Get("ok"));
    
            epService.EPRuntime.SendEvent(MakeMapEvent("name", "value1", Collections.SingletonDataMap("name", "value1")), "InputEvent");
            Assert.IsTrue((bool?) listener.AssertOneGetNewAndReset().Get("ok"));
    
            // test Object-array-type
            epService.EPAdministrator.Configuration.AddEventType("ObjectArrayEvent", new string[]{"mapped", "indexed"}, new object[]{new Dictionary<string, object>(), typeof(int[])});
            string eplObjectArray = "select " +
                    "mapped(TheString) as val0," +
                    "indexed(IntPrimitive) as val1 " +
                    "from ObjectArrayEvent#lastevent, SupportBean unidirectional";
            RunAssertionObjectArray(epService, listener, eplObjectArray);
    
            string eplObjectArrayPrefixed = "select " +
                    "sbcp.mapped(TheString) as val0," +
                    "sbcp.indexed(IntPrimitive) as val1 " +
                    "from ObjectArrayEvent#lastevent sbcp, SupportBean unidirectional";
            RunAssertionObjectArray(epService, listener, eplObjectArrayPrefixed);
        }
    
        private void RunAssertionMap(EPServiceProvider epService, SupportUpdateListener listener, string epl) {
            EPStatement stmtMap = epService.EPAdministrator.CreateEPL(epl);
            stmtMap.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(MakeMapEvent(), "MapEvent");
            epService.EPRuntime.SendEvent(new SupportBean("keyOne", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "val0,val1".Split(','), new object[]{"valueOne", 2});
            stmtMap.Dispose();
        }
    
        private void RunAssertionObjectArray(EPServiceProvider epService, SupportUpdateListener listener, string epl) {
            EPStatement stmtObjectArray = epService.EPAdministrator.CreateEPL(epl);
            stmtObjectArray.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new object[]{Collections.SingletonMap("keyOne", "valueOne"), new int[]{1, 2}}, "ObjectArrayEvent");
            epService.EPRuntime.SendEvent(new SupportBean("keyOne", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "val0,val1".Split(','), new object[]{"valueOne", 2});
            stmtObjectArray.Dispose();
        }
    
        private void RunAssertionBean(EPServiceProvider epService, SupportUpdateListener listener, string epl) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(SupportBeanComplexProps.MakeDefaultBean());
            epService.EPRuntime.SendEvent(new SupportBean("keyOne", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "val0,val1".Split(','), new object[]{"valueOne", 2});
            stmt.Dispose();
        }
    
        private IDictionary<string, Object> MakeMapEvent() {
            var map = new Dictionary<string, Object>();
            map.Put("mapped", Collections.SingletonMap("keyOne", "valueOne"));
            map.Put("indexed", new int[]{1, 2});
            return map;
        }
    
        private IDictionary<string, Object> MakeMapEvent(string name, string value, Map properties) {
            var map = new Dictionary<string, Object>();
            map.Put("name", name);
            map.Put("value", value);
            map.Put("properties", properties);
            return map;
        }
    }
} // end of namespace
