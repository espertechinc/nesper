///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.regressionlib.framework;

using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;

using Newtonsoft.Json.Linq;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{


	public class EventInfraGetterDynamicSimplePropertyPredefined : RegressionExecution {
	    public void Run(RegressionEnvironment env) {
	        // Bean
	        BiConsumer<EventType, string> bean = (type, property) => {
	            env.SendEventBean(new LocalEvent(property));
	        };
	        var beanepl = $"@public @buseventtype create schema LocalEvent as {typeof(LocalEvent).MaskTypeName()}";
	        RunAssertion(env, beanepl, bean);

	        // Map
	        BiConsumer<EventType, string> map = (type, property) => {
	            env.SendEventMap(Collections.SingletonDataMap("Property", property), "LocalEvent");
	        };
	        RunAssertion(env, GetEpl("map"), map);

	        // Object-array
	        BiConsumer<EventType, string> oa = (type, property) => {
	            env.SendEventObjectArray(new object[]{property}, "LocalEvent");
	        };
	        RunAssertion(env, GetEpl("objectarray"), oa);

	        // Json
	        BiConsumer<EventType, string> json = (type, property) => {
	            if (property == null) {
	                env.SendEventJson(new JObject(new JProperty("Property", null)).ToString(), "LocalEvent");
	            } else {
	                env.SendEventJson(new JObject(new JProperty("Property", property)).ToString(), "LocalEvent");
	            }
	        };
	        RunAssertion(env, GetEpl("json"), json);

	        // Json-Class-Predefined
	        var eplJsonPredefined = "@JsonSchema(ClassName='" + typeof(MyLocalJsonProvided).MaskTypeName() + "') @buseventtype @public " +
	                                "create json schema LocalEvent();\n";
	        RunAssertion(env, eplJsonPredefined, json);

	        // Avro
	        BiConsumer<EventType, string> avro = (type, property) => {
	            var @event = new GenericRecord(SupportAvroUtil.GetAvroSchema(type).AsRecordSchema());
	            @event.Put("Property", property);
	            env.SendEventAvro(@event, "LocalEvent");
	        };
	        RunAssertion(env, GetEpl("avro"), avro);
	    }

	    private string GetEpl(string underlying) {
	        return "@buseventtype @public create " + underlying + " schema LocalEvent(Property string);\n";
	    }

	    public void RunAssertion(RegressionEnvironment env,
	                             string createSchemaEPL,
	                             BiConsumer<EventType, string> sender) {

	        var path = new RegressionPath();
	        env.CompileDeploy(createSchemaEPL, path);

	        env.CompileDeploy("@Name('s0') select * from LocalEvent", path).AddListener("s0");
	        var eventType = env.Statement("s0").EventType;
	        var g0 = eventType.GetGetter("Property?");

	        if (sender == null) {
	            Assert.IsNull(g0);
	            env.UndeployAll();
	            return;
	        } else {
	            var propepl = "@Name('s1') select Property? as c0, exists(Property?) as c1, typeof(Property?) as c2 from LocalEvent;\n";
	            env.CompileDeploy(propepl, path).AddListener("s1");
	        }

	        sender.Invoke(eventType, "a");
	        var @event = env.Listener("s0").AssertOneGetNewAndReset();
	        AssertGetter(@event, g0, true, "a");
	        AssertProps(env, true, "a");

	        sender.Invoke(eventType, null);
	        @event = env.Listener("s0").AssertOneGetNewAndReset();
	        AssertGetter(@event, g0, true, null);
	        AssertProps(env, true, null);

	        env.UndeployAll();
	    }

	    private void AssertGetter(EventBean @event, EventPropertyGetter getter, bool exists, string value) {
	        Assert.AreEqual(exists, getter.IsExistsProperty(@event));
	        Assert.AreEqual(value, getter.Get(@event));
	        Assert.IsNull(getter.GetFragment(@event));
	    }

	    private void AssertProps(RegressionEnvironment env, bool exists, string value) {
	        var @event = env.Listener("s1").AssertOneGetNewAndReset();
	        Assert.AreEqual(value, @event.Get("c0"));
	        Assert.AreEqual(exists, @event.Get("c1"));
	        Assert.AreEqual(value != null ? "String" : null, @event.Get("c2"));
	    }

	    public class LocalEvent
	    {
		    public LocalEvent(string property)
		    {
			    this.Property = property;
		    }

		    public string Property { get; }
	    }

	    [Serializable]
	    public class MyLocalJsonProvided
	    {
		    public string Property;
	    }
	}
} // end of namespace
