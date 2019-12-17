///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.bean
{
    public class EventBeanPublicAccessors : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            // assert type metadata
            var type = env.Runtime.EventTypeService.GetEventTypePreconfigured("AnotherLegacyEvent");
            Assert.AreEqual(EventTypeApplicationType.CLASS, type.Metadata.ApplicationType);
            Assert.AreEqual("AnotherLegacyEvent", type.Metadata.Name);

            var statementText = "@Name('s0') select " +
                                "fieldLegacyVal as fieldSimple," +
                                "fieldStringArray as fieldArr," +
                                "fieldStringArray[1] as fieldArrIndexed," +
                                "fieldMapped as fieldMap," +
                                "fieldNested as fieldNested," +
                                "fieldNested.ReadNestedValue as fieldNestedVal," +
                                "ReadLegacyBeanVal as simple," +
                                "ReadLegacyNested as nestedObject," +
                                "ReadLegacyNested.ReadNestedValue as nested," +
                                "ReadStringArray[0] as array," +
                                "ReadStringIndexed[1] as indexed," +
                                "ReadMapByKey('key1') as mapped," +
                                "ReadMap as mapItself," +
                                "explicitFSimple, " +
                                "explicitFIndexed[0], " +
                                "explicitFNested, " +
                                "explicitMSimple, " +
                                "explicitMArray[0], " +
                                "explicitMIndexed[1], " +
                                "explicitMMapped('key2')" +
                                " from AnotherLegacyEvent#length(5)";
            env.CompileDeploy(statementText).AddListener("s0");

            var eventType = env.Statement("s0").EventType;
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("fieldSimple"));
            Assert.AreEqual(typeof(string[]), eventType.GetPropertyType("fieldArr"));
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("fieldArrIndexed"));
            Assert.AreEqual(typeof(IDictionary<string, string>), eventType.GetPropertyType("fieldMap"));
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

            var legacyBean = MakeSampleEvent();
            env.SendEventBean(legacyBean, "AnotherLegacyEvent");

            Assert.AreEqual(legacyBean.fieldLegacyVal, env.Listener("s0").LastNewData[0].Get("fieldSimple"));
            Assert.AreEqual(legacyBean.fieldStringArray, env.Listener("s0").LastNewData[0].Get("fieldArr"));
            Assert.AreEqual(legacyBean.fieldStringArray[1], env.Listener("s0").LastNewData[0].Get("fieldArrIndexed"));
            Assert.AreEqual(legacyBean.fieldMapped, env.Listener("s0").LastNewData[0].Get("fieldMap"));
            Assert.AreEqual(legacyBean.fieldNested, env.Listener("s0").LastNewData[0].Get("fieldNested"));
            Assert.AreEqual(
                legacyBean.fieldNested.ReadNestedValue(),
                env.Listener("s0").LastNewData[0].Get("fieldNestedVal"));

            Assert.AreEqual(legacyBean.ReadLegacyBeanVal(), env.Listener("s0").LastNewData[0].Get("simple"));
            Assert.AreEqual(legacyBean.ReadLegacyNested(), env.Listener("s0").LastNewData[0].Get("nestedObject"));
            Assert.AreEqual(
                legacyBean.ReadLegacyNested().ReadNestedValue(),
                env.Listener("s0").LastNewData[0].Get("nested"));
            Assert.AreEqual(legacyBean.ReadStringIndexed(0), env.Listener("s0").LastNewData[0].Get("array"));
            Assert.AreEqual(legacyBean.ReadStringIndexed(1), env.Listener("s0").LastNewData[0].Get("indexed"));
            Assert.AreEqual(legacyBean.ReadMapByKey("key1"), env.Listener("s0").LastNewData[0].Get("mapped"));
            Assert.AreEqual(legacyBean.ReadMap(), env.Listener("s0").LastNewData[0].Get("mapItself"));

            Assert.AreEqual(legacyBean.ReadLegacyBeanVal(), env.Listener("s0").LastNewData[0].Get("explicitFSimple"));
            Assert.AreEqual(legacyBean.ReadLegacyBeanVal(), env.Listener("s0").LastNewData[0].Get("explicitMSimple"));
            Assert.AreEqual(legacyBean.ReadLegacyNested(), env.Listener("s0").LastNewData[0].Get("explicitFNested"));
            Assert.AreEqual(
                legacyBean.ReadStringIndexed(0),
                env.Listener("s0").LastNewData[0].Get("explicitFIndexed[0]"));
            Assert.AreEqual(
                legacyBean.ReadStringIndexed(0),
                env.Listener("s0").LastNewData[0].Get("explicitMArray[0]"));
            Assert.AreEqual(
                legacyBean.ReadStringIndexed(1),
                env.Listener("s0").LastNewData[0].Get("explicitMIndexed[1]"));
            Assert.AreEqual(
                legacyBean.ReadMapByKey("key2"),
                env.Listener("s0").LastNewData[0].Get("explicitMMapped('key2')"));

            var stmtType = (EventTypeSPI) env.Statement("s0").EventType;
            Assert.AreEqual(EventTypeBusModifier.NONBUS, stmtType.Metadata.BusModifier);
            Assert.AreEqual(EventTypeApplicationType.MAP, stmtType.Metadata.ApplicationType);
            Assert.AreEqual(EventTypeTypeClass.STATEMENTOUT, stmtType.Metadata.TypeClass);

            env.UndeployAll();
        }

        internal static SupportLegacyBean MakeSampleEvent()
        {
            IDictionary<string, string> mappedProperty = new Dictionary<string, string>();
            mappedProperty.Put("key1", "value1");
            mappedProperty.Put("key2", "value2");
            return new SupportLegacyBean("leg", new[] {"a", "b"}, mappedProperty, "nest");
        }
    }
} // end of namespace