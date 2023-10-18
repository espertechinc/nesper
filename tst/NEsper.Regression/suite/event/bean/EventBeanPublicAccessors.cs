///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework; // assertEquals

namespace com.espertech.esper.regressionlib.suite.@event.bean
{
	public class EventBeanPublicAccessors : RegressionExecution {
	    internal static SupportLegacyBean MakeSampleEvent() {
	        IDictionary<string, string> mappedProperty = new Dictionary<string, string>();
	        mappedProperty.Put("key1", "value1");
	        mappedProperty.Put("key2", "value2");
	        return new SupportLegacyBean("leg", new string[]{"a", "b"}, mappedProperty, "nest");
	    }

	    public void Run(RegressionEnvironment env) {

	        // assert type metadata
	        env.AssertThat(() => {
	            var type = env.Runtime.EventTypeService.GetEventTypePreconfigured("AnotherLegacyEvent");
	            Assert.AreEqual(EventTypeApplicationType.CLASS, type.Metadata.ApplicationType);
	            Assert.AreEqual("AnotherLegacyEvent", type.Metadata.Name);
	        });

	        var statementText = "@name('s0') select " +
	                            "fieldLegacyVal as fieldSimple," +
	                            "fieldStringArray as fieldArr," +
	                            "fieldStringArray[1] as fieldArrIndexed," +
	                            "fieldMapped as fieldMap," +
	                            "fieldNested as fieldNested," +
	                            "fieldNested.readNestedValue as fieldNestedVal," +
	                            "readLegacyBeanVal as simple," +
	                            "readLegacyNested as nestedObject," +
	                            "readLegacyNested.readNestedValue as nested," +
	                            "readStringArray[0] as array," +
	                            "readStringIndexed[1] as indexed," +
	                            "readMapByKey('key1') as mapped," +
	                            "readMap as mapItself," +
	                            "explicitFSimple, " +
	                            "explicitFIndexed[0], " +
	                            "explicitFNested, " +
	                            "explicitMSimple, " +
	                            "explicitMArray[0], " +
	                            "explicitMIndexed[1], " +
	                            "explicitMMapped('key2')" +
	                            " from AnotherLegacyEvent#length(5)";
	        env.CompileDeploy(statementText).AddListener("s0");

	        env.AssertStatement("s0", statement => {
	            var eventType = statement.EventType;
	            Assert.AreEqual(typeof(string), eventType.GetPropertyType("fieldSimple"));
	            Assert.AreEqual(typeof(string[]), eventType.GetPropertyType("fieldArr"));
	            Assert.AreEqual(typeof(string), eventType.GetPropertyType("fieldArrIndexed"));
	            Assert.AreEqual(typeof(IDictionary<string, object>), eventType.GetPropertyType("fieldMap"));
	            Assert.AreEqual(typeof(SupportLegacyBean.LegacyNested), eventType.GetPropertyType("fieldNested"));
	            Assert.AreEqual(typeof(string), eventType.GetPropertyType("fieldNestedVal"));
	            Assert.AreEqual(typeof(string), eventType.GetPropertyType("simple"));
	            Assert.AreEqual(typeof(SupportLegacyBean.LegacyNested), eventType.GetPropertyType("nestedObject"));
	            Assert.AreEqual(typeof(string), eventType.GetPropertyType("nested"));
	            Assert.AreEqual(typeof(string), eventType.GetPropertyType("array"));
	            Assert.AreEqual(typeof(string), eventType.GetPropertyType("indexed"));
	            Assert.AreEqual(typeof(string), eventType.GetPropertyType("mapped"));
	            Assert.AreEqual(typeof(string), eventType.GetPropertyType("explicitFSimple"));
	            Assert.AreEqual(typeof(string), eventType.GetPropertyType("explicitFIndexed[0]"));
	            Assert.AreEqual(typeof(SupportLegacyBean.LegacyNested), eventType.GetPropertyType("explicitFNested"));
	            Assert.AreEqual(typeof(string), eventType.GetPropertyType("explicitMSimple"));
	            Assert.AreEqual(typeof(string), eventType.GetPropertyType("explicitMArray[0]"));
	            Assert.AreEqual(typeof(string), eventType.GetPropertyType("explicitMIndexed[1]"));
	            Assert.AreEqual(typeof(string), eventType.GetPropertyType("explicitMMapped('key2')"));
	        });

	        var legacyBean = MakeSampleEvent();
	        env.SendEventBean(legacyBean, "AnotherLegacyEvent");

	        env.AssertEventNew("s0", eventBean => {
	            Assert.AreEqual(legacyBean.fieldLegacyVal, eventBean.Get("fieldSimple"));
	            Assert.AreEqual(legacyBean.fieldStringArray, eventBean.Get("fieldArr"));
	            Assert.AreEqual(legacyBean.fieldStringArray[1], eventBean.Get("fieldArrIndexed"));
	            Assert.AreEqual(legacyBean.fieldMapped, eventBean.Get("fieldMap"));
	            Assert.AreEqual(legacyBean.fieldNested, eventBean.Get("fieldNested"));
	            Assert.AreEqual(legacyBean.fieldNested.ReadNestedValue(), eventBean.Get("fieldNestedVal"));

	            Assert.AreEqual(legacyBean.ReadLegacyBeanVal(), eventBean.Get("simple"));
	            Assert.AreEqual(legacyBean.ReadLegacyNested(), eventBean.Get("nestedObject"));
	            Assert.AreEqual(legacyBean.ReadLegacyNested().ReadNestedValue(), eventBean.Get("nested"));
	            Assert.AreEqual(legacyBean.ReadStringIndexed(0), eventBean.Get("array"));
	            Assert.AreEqual(legacyBean.ReadStringIndexed(1), eventBean.Get("indexed"));
	            Assert.AreEqual(legacyBean.ReadMapByKey("key1"), eventBean.Get("mapped"));
	            Assert.AreEqual(legacyBean.ReadMap(), eventBean.Get("mapItself"));

	            Assert.AreEqual(legacyBean.ReadLegacyBeanVal(), eventBean.Get("explicitFSimple"));
	            Assert.AreEqual(legacyBean.ReadLegacyBeanVal(), eventBean.Get("explicitMSimple"));
	            Assert.AreEqual(legacyBean.ReadLegacyNested(), eventBean.Get("explicitFNested"));
	            Assert.AreEqual(legacyBean.ReadStringIndexed(0), eventBean.Get("explicitFIndexed[0]"));
	            Assert.AreEqual(legacyBean.ReadStringIndexed(0), eventBean.Get("explicitMArray[0]"));
	            Assert.AreEqual(legacyBean.ReadStringIndexed(1), eventBean.Get("explicitMIndexed[1]"));
	            Assert.AreEqual(legacyBean.ReadMapByKey("key2"), eventBean.Get("explicitMMapped('key2')"));
	        });

	        env.AssertStatement("s0", statement => {
	            var stmtType = (EventTypeSPI) statement.EventType;
	            Assert.AreEqual(EventTypeBusModifier.NONBUS, stmtType.Metadata.BusModifier);
	            Assert.AreEqual(EventTypeApplicationType.MAP, stmtType.Metadata.ApplicationType);
	            Assert.AreEqual(EventTypeTypeClass.STATEMENTOUT, stmtType.Metadata.TypeClass);
	        });

	        env.UndeployAll();
	    }
	}
} // end of namespace
