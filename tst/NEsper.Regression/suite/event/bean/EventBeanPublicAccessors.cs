///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.@event.bean
{
    public class EventBeanPublicAccessors : RegressionExecution
    {
        internal static SupportLegacyBean MakeSampleEvent()
        {
            IDictionary<string, string> mappedProperty = new Dictionary<string, string>();
            mappedProperty.Put("key1", "value1");
            mappedProperty.Put("key2", "value2");
            return new SupportLegacyBean("leg", new string[] { "a", "b" }, mappedProperty, "nest");
        }

        public void Run(RegressionEnvironment env)
        {
            // assert type metadata
            env.AssertThat(
                () => {
                    var type = env.Runtime.EventTypeService.GetEventTypePreconfigured("AnotherLegacyEvent");
                    ClassicAssert.AreEqual(EventTypeApplicationType.CLASS, type.Metadata.ApplicationType);
                    ClassicAssert.AreEqual("AnotherLegacyEvent", type.Metadata.Name);
                });

            var statementText = "@name('s0') select " +
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

            env.AssertStatement(
                "s0",
                statement => {
                    var eventType = statement.EventType;
                    ClassicAssert.AreEqual(typeof(string), eventType.GetPropertyType("fieldSimple"));
                    ClassicAssert.AreEqual(typeof(string[]), eventType.GetPropertyType("fieldArr"));
                    ClassicAssert.AreEqual(typeof(string), eventType.GetPropertyType("fieldArrIndexed"));
                    ClassicAssert.AreEqual(typeof(IDictionary<string, string>), eventType.GetPropertyType("fieldMap"));
                    ClassicAssert.AreEqual(typeof(SupportLegacyBean.LegacyNested), eventType.GetPropertyType("fieldNested"));
                    ClassicAssert.AreEqual(typeof(string), eventType.GetPropertyType("fieldNestedVal"));
                    ClassicAssert.AreEqual(typeof(string), eventType.GetPropertyType("simple"));
                    ClassicAssert.AreEqual(typeof(SupportLegacyBean.LegacyNested), eventType.GetPropertyType("nestedObject"));
                    ClassicAssert.AreEqual(typeof(string), eventType.GetPropertyType("nested"));
                    ClassicAssert.AreEqual(typeof(string), eventType.GetPropertyType("array"));
                    ClassicAssert.AreEqual(typeof(string), eventType.GetPropertyType("indexed"));
                    ClassicAssert.AreEqual(typeof(string), eventType.GetPropertyType("mapped"));
                    ClassicAssert.AreEqual(typeof(string), eventType.GetPropertyType("explicitFSimple"));
                    ClassicAssert.AreEqual(typeof(string), eventType.GetPropertyType("explicitFIndexed[0]"));
                    ClassicAssert.AreEqual(typeof(SupportLegacyBean.LegacyNested), eventType.GetPropertyType("explicitFNested"));
                    ClassicAssert.AreEqual(typeof(string), eventType.GetPropertyType("explicitMSimple"));
                    ClassicAssert.AreEqual(typeof(string), eventType.GetPropertyType("explicitMArray[0]"));
                    ClassicAssert.AreEqual(typeof(string), eventType.GetPropertyType("explicitMIndexed[1]"));
                    ClassicAssert.AreEqual(typeof(string), eventType.GetPropertyType("explicitMMapped('key2')"));
                });

            var legacyBean = MakeSampleEvent();
            env.SendEventBean(legacyBean, "AnotherLegacyEvent");

            env.AssertEventNew(
                "s0",
                eventBean => {
                    ClassicAssert.AreEqual(legacyBean.fieldLegacyVal, eventBean.Get("fieldSimple"));
                    ClassicAssert.AreEqual(legacyBean.fieldStringArray, eventBean.Get("fieldArr"));
                    ClassicAssert.AreEqual(legacyBean.fieldStringArray[1], eventBean.Get("fieldArrIndexed"));
                    ClassicAssert.AreEqual(legacyBean.fieldMapped, eventBean.Get("fieldMap"));
                    ClassicAssert.AreEqual(legacyBean.fieldNested, eventBean.Get("fieldNested"));
                    ClassicAssert.AreEqual(legacyBean.fieldNested.ReadNestedValue(), eventBean.Get("fieldNestedVal"));

                    ClassicAssert.AreEqual(legacyBean.ReadLegacyBeanVal(), eventBean.Get("simple"));
                    ClassicAssert.AreEqual(legacyBean.ReadLegacyNested(), eventBean.Get("nestedObject"));
                    ClassicAssert.AreEqual(legacyBean.ReadLegacyNested().ReadNestedValue(), eventBean.Get("nested"));
                    ClassicAssert.AreEqual(legacyBean.ReadStringIndexed(0), eventBean.Get("array"));
                    ClassicAssert.AreEqual(legacyBean.ReadStringIndexed(1), eventBean.Get("indexed"));
                    ClassicAssert.AreEqual(legacyBean.ReadMapByKey("key1"), eventBean.Get("mapped"));
                    ClassicAssert.AreEqual(legacyBean.ReadMap(), eventBean.Get("mapItself"));

                    ClassicAssert.AreEqual(legacyBean.ReadLegacyBeanVal(), eventBean.Get("explicitFSimple"));
                    ClassicAssert.AreEqual(legacyBean.ReadLegacyBeanVal(), eventBean.Get("explicitMSimple"));
                    ClassicAssert.AreEqual(legacyBean.ReadLegacyNested(), eventBean.Get("explicitFNested"));
                    ClassicAssert.AreEqual(legacyBean.ReadStringIndexed(0), eventBean.Get("explicitFIndexed[0]"));
                    ClassicAssert.AreEqual(legacyBean.ReadStringIndexed(0), eventBean.Get("explicitMArray[0]"));
                    ClassicAssert.AreEqual(legacyBean.ReadStringIndexed(1), eventBean.Get("explicitMIndexed[1]"));
                    ClassicAssert.AreEqual(legacyBean.ReadMapByKey("key2"), eventBean.Get("explicitMMapped('key2')"));
                });

            env.AssertStatement(
                "s0",
                statement => {
                    var stmtType = (EventTypeSPI)statement.EventType;
                    ClassicAssert.AreEqual(EventTypeBusModifier.NONBUS, stmtType.Metadata.BusModifier);
                    ClassicAssert.AreEqual(EventTypeApplicationType.MAP, stmtType.Metadata.ApplicationType);
                    ClassicAssert.AreEqual(EventTypeTypeClass.STATEMENTOUT, stmtType.Metadata.TypeClass);
                });

            env.UndeployAll();
        }
    }
} // end of namespace