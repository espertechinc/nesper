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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.events
{
    [TestFixture]
	public class TestBeanMappedIndexedPropertyExpression
    {
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp() {
	        _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);
	        }
	        _listener = new SupportUpdateListener();
	        _epService.EPAdministrator.Configuration.AddEventType("SupportBean", typeof(SupportBean));
	        _epService.EPAdministrator.Configuration.AddEventType("SupportBeanComplexProps", typeof(SupportBeanComplexProps));
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.EndTest();
	        }
	        _listener = null;
	    }

        [Test]
	    public void TestBeanMapWrap() {
            // test bean-type
            string eplBeans = "select " +
	                          "Mapped(theString) as val0, " +
	                          "Indexed(intPrimitive) as val1 " +
	                          "from SupportBeanComplexProps#lastevent, SupportBean sb unidirectional";
	        RunAssertionBean(eplBeans);

	        // test bean-type prefixed
	        string eplBeansPrefixed = "select " +
	                                  "sbcp.Mapped(theString) as val0, " +
	                                  "sbcp.Indexed(intPrimitive) as val1 " +
	                                  "from SupportBeanComplexProps#lastevent sbcp, SupportBean sb unidirectional";
	        RunAssertionBean(eplBeansPrefixed);

	        // test wrap
	        _epService.EPAdministrator.CreateEPL("insert into SecondStream select 'a' as val0, * from SupportBeanComplexProps");

	        string eplWrap = "select " +
                             "Mapped(theString) as val0," +
                             "Indexed(intPrimitive) as val1 " +
	                         "from SecondStream #lastevent, SupportBean unidirectional";
	        RunAssertionBean(eplWrap);

	        string eplWrapPrefixed = "select " +
                                     "sbcp.Mapped(theString) as val0," +
                                     "sbcp.Indexed(intPrimitive) as val1 " +
	                                 "from SecondStream #lastevent sbcp, SupportBean unidirectional";
	        RunAssertionBean(eplWrapPrefixed);

            // test Map-type
            IDictionary<string, object> def = new Dictionary<string, object>();
	        def.Put("mapped", new Dictionary<string, object>());
	        def.Put("indexed", typeof(int[]));
	        _epService.EPAdministrator.Configuration.AddEventType("MapEvent", def);

	        string eplMap = "select " +
                            "mapped(theString) as val0," +
                            "indexed(intPrimitive) as val1 " +
	                        "from MapEvent#lastevent, SupportBean unidirectional";
	        RunAssertionMap(eplMap);

	        string eplMapPrefixed = "select " +
                                    "sbcp.mapped(theString) as val0," +
                                    "sbcp.indexed(intPrimitive) as val1 " +
	                                "from MapEvent#lastevent sbcp, SupportBean unidirectional";
	        RunAssertionMap(eplMapPrefixed);

	        // test insert-int
	        IDictionary<string, object> defType = new Dictionary<string, object>();
	        defType.Put("name", typeof(string));
	        defType.Put("value", typeof(string));
	        defType.Put("properties", typeof(IDictionary<string, object>));
	        _epService.EPAdministrator.Configuration.AddEventType("InputEvent", defType);
	        _epService.EPAdministrator.CreateEPL("select name,value,properties(name) = value as ok from InputEvent").AddListener(_listener);

	        _listener.Reset();
	        _epService.EPRuntime.SendEvent(MakeMapEvent("name", "value1", Collections.SingletonDataMap("name", "xxxx")), "InputEvent");
	        Assert.IsFalse((Boolean) _listener.AssertOneGetNewAndReset().Get("ok"));

	        _epService.EPRuntime.SendEvent(MakeMapEvent("name", "value1", Collections.SingletonDataMap("name", "value1")), "InputEvent");
	        Assert.IsTrue((Boolean) _listener.AssertOneGetNewAndReset().Get("ok"));

	        // test Object-array-type
	        _epService.EPAdministrator.Configuration.AddEventType("ObjectArrayEvent", 
                new string[] { "mapped", "indexed" }, 
                new object[] {new Dictionary<string, object>(), typeof(int[])});
	        string eplObjectArray = "select " +
                                    "mapped(theString) as val0," +
                                    "indexed(intPrimitive) as val1 " +
	                                "from ObjectArrayEvent#lastevent, SupportBean unidirectional";
	        RunAssertionObjectArray(eplObjectArray);

	        string eplObjectArrayPrefixed = "select " +
                                            "sbcp.mapped(theString) as val0," +
                                            "sbcp.indexed(intPrimitive) as val1 " +
	                                        "from ObjectArrayEvent#lastevent sbcp, SupportBean unidirectional";
	        RunAssertionObjectArray(eplObjectArrayPrefixed);
	    }

	    private void RunAssertionMap(string epl) {
	        EPStatement stmtMap = _epService.EPAdministrator.CreateEPL(epl);
	        stmtMap.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(MakeMapEvent(), "MapEvent");
	        _epService.EPRuntime.SendEvent(new SupportBean("keyOne", 1));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "val0,val1".SplitCsv(), new object[] {"valueOne", 2});
	        stmtMap.Dispose();
	    }

	    private void RunAssertionObjectArray(string epl) {
	        EPStatement stmtObjectArray = _epService.EPAdministrator.CreateEPL(epl);
	        stmtObjectArray.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new object[] {Collections.SingletonMap("keyOne", "valueOne"), new int[] {1, 2}}, "ObjectArrayEvent");
	        _epService.EPRuntime.SendEvent(new SupportBean("keyOne", 1));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "val0,val1".SplitCsv(), new object[] {"valueOne", 2});
	        stmtObjectArray.Dispose();
	    }

	    private void RunAssertionBean(string epl) {
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(SupportBeanComplexProps.MakeDefaultBean());
	        _epService.EPRuntime.SendEvent(new SupportBean("keyOne", 1));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "val0,val1".SplitCsv(), new object[] {"valueOne", 2});
	        stmt.Dispose();
	    }

	    private IDictionary<string, object> MakeMapEvent() {
	        IDictionary<string, object> map = new Dictionary<string, object>();
	        map.Put("mapped", Collections.SingletonMap("keyOne", "valueOne"));
	        map.Put("indexed", new int[] {1, 2});
	        return map;
	    }

	    private IDictionary<string, object> MakeMapEvent(string name, string value, IDictionary<string, object> properties) {
	        IDictionary<string, object> map = new Dictionary<string, object>();
	        map.Put("name", name);
	        map.Put("value", value);
	        map.Put("properties", properties);
	        return map;
	    }
	}
} // end of namespace
