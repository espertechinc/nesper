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
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.events
{
    [TestFixture]
    public class TestMappedIndexedPropertyExpression
    {

        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;

        [SetUp]
        public void SetUp()
        {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            _listener = new SupportUpdateListener();
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddEventType("SupportBeanComplexProps", typeof(SupportBeanComplexProps));
        }

        [TearDown]
        public void TearDown()
        {
            _listener = null;
        }

        [Test]
        public void TestBeanMapWrap()
        {
            // test bean-type
            String eplBeans = "select " +
                              "Mapped(TheString) as val0, " +
                              "Indexed(IntPrimitive) as val1 " +
                              "from SupportBeanComplexProps.std:lastevent(), SupportBean sb unidirectional";
            RunAssertionBean(eplBeans);

            // test bean-type prefixed
            String eplBeansPrefixed = "select " +
                                      "sbcp.Mapped(TheString) as val0, " +
                                      "sbcp.Indexed(IntPrimitive) as val1 " +
                                      "from SupportBeanComplexProps.std:lastevent() sbcp, SupportBean sb unidirectional";
            RunAssertionBean(eplBeansPrefixed);

            // test wrap
            _epService.EPAdministrator.CreateEPL(
                "insert into SecondStream select 'a' as val0, * from SupportBeanComplexProps");

            String eplWrap = "select " +
                             "Mapped(TheString) as val0," +
                             "Indexed(IntPrimitive) as val1 " +
                             "from SecondStream .std:lastevent(), SupportBean unidirectional";
            RunAssertionBean(eplWrap);

            String eplWrapPrefixed = "select " +
                                     "sbcp.Mapped(TheString) as val0," +
                                     "sbcp.Indexed(IntPrimitive) as val1 " +
                                     "from SecondStream .std:lastevent() sbcp, SupportBean unidirectional";
            RunAssertionBean(eplWrapPrefixed);

            // test Map-type
            IDictionary<String, Object> def = new Dictionary<String, Object>();
            def["Mapped"] = new Dictionary<string, object>();
            def["Indexed"] = typeof (int[]);
            _epService.EPAdministrator.Configuration.AddEventType("MapEvent", def);

            String eplMap = "select " +
                            "Mapped(TheString) as val0," +
                            "Indexed(IntPrimitive) as val1 " +
                            "from MapEvent.std:lastevent(), SupportBean unidirectional";
            RunAssertionMap(eplMap);

            String eplMapPrefixed = "select " +
                                    "sbcp.Mapped(TheString) as val0," +
                                    "sbcp.Indexed(IntPrimitive) as val1 " +
                                    "from MapEvent.std:lastevent() sbcp, SupportBean unidirectional";
            RunAssertionMap(eplMapPrefixed);


            // test insert-int
            var defType = new Dictionary<String, Object>();

            defType["name"] = typeof (string);
            defType["value"] = typeof(string);
            defType["properties"] = typeof(IDictionary<string, object>);
            _epService.EPAdministrator.Configuration.AddEventType("InputEvent", defType);
            _epService.EPAdministrator.CreateEPL("select name,value,properties(name) = value as ok from InputEvent").Events += _listener.Update;

            _listener.Reset();
            _epService.EPRuntime.SendEvent(
                MakeMapEvent("name", "value1", Collections.SingletonDataMap("name", "xxxx")), "InputEvent");
            Assert.IsFalse(_listener.AssertOneGetNewAndReset().Get("ok").AsBoolean());

            _epService.EPRuntime.SendEvent(
                MakeMapEvent("name", "value1", Collections.SingletonDataMap("name", "value1")), "InputEvent");
            Assert.IsTrue(_listener.AssertOneGetNewAndReset().Get("ok").AsBoolean());

            // test Object-array-type
            _epService.EPAdministrator.Configuration.AddEventType(
                "ObjectArrayEvent", 
                new String[]
                {
                    "mapped", "indexed"
                },
                new Object[]
                {
                    new Dictionary<string, object>(), typeof (int[])
                });
            String eplObjectArray = "select " + "mapped(TheString) as val0,"
                                    + "indexed(IntPrimitive) as val1 "
                                    + "from ObjectArrayEvent.std:lastevent(), SupportBean unidirectional";

            RunAssertionObjectArray(eplObjectArray);

            String eplObjectArrayPrefixed = "select "
                                            + "sbcp.mapped(TheString) as val0,"
                                            + "sbcp.indexed(IntPrimitive) as val1 "
                                            + "from ObjectArrayEvent.std:lastevent() sbcp, SupportBean unidirectional";

            RunAssertionObjectArray(eplObjectArrayPrefixed);
        }

        private void RunAssertionMap(String epl)
        {
            EPStatement stmtMap = _epService.EPAdministrator.CreateEPL(epl);
            stmtMap.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(MakeMapEvent(), "MapEvent");
            _epService.EPRuntime.SendEvent(new SupportBean("keyOne", 1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "val0,val1".Split(','), new Object[] { "valueOne", 2 });
            stmtMap.Dispose();
        }

        private void RunAssertionObjectArray(String epl)
        {
            EPStatement stmtObjectArray = _epService.EPAdministrator.CreateEPL(epl);

            stmtObjectArray.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(
                new Object[]
                { 
                    Collections.SingletonDataMap("keyOne", "valueOne"), 
                    new int[]{ 1, 2 }
                }, 
                "ObjectArrayEvent");
            _epService.EPRuntime.SendEvent(new SupportBean("keyOne", 1));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(),
                "val0,val1".Split(','), 
                new Object[] { "valueOne", 2 }
                );
            stmtObjectArray.Dispose();
        }

        private void RunAssertionBean(String epl)
        {
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(SupportBeanComplexProps.MakeDefaultBean());
            _epService.EPRuntime.SendEvent(new SupportBean("keyOne", 1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "val0,val1".Split(','), new Object[] { "valueOne", 2 });
            stmt.Dispose();
        }

        private IDictionary<String, Object> MakeMapEvent()
        {
            IDictionary<String, Object> map = new Dictionary<String, Object>();
            map["Mapped"] = Collections.SingletonDataMap("keyOne", "valueOne");
            map["Indexed"] = new int[] { 1, 2 };
            return map;
        }

        private IDictionary<String, Object> MakeMapEvent(String name, String value, IDictionary<String, Object> properties)
        {
            IDictionary<String, Object> map = new Dictionary<String, Object>();

            map["name"] = name;
            map["value"] = value;
            map["properties"] = properties;
            return map;
        }
    }
}
