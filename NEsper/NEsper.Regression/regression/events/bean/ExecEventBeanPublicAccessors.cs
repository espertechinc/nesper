///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.events;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.events.bean
{
    public class ExecEventBeanPublicAccessors : RegressionExecution {
        private readonly bool _codegen;
    
        public ExecEventBeanPublicAccessors(bool codegen) {
            _codegen = codegen;
        }
    
        internal static SupportLegacyBean MakeSampleEvent() {
            var mappedProperty = new Dictionary<string, string>();
            mappedProperty.Put("key1", "value1");
            mappedProperty.Put("key2", "value2");
            return new SupportLegacyBean("leg", new string[]{"a", "b"}, mappedProperty, "nest");
        }
    
        public override void Run(EPServiceProvider epService) {
            var legacyDef = new ConfigurationEventTypeLegacy();
            legacyDef.AccessorStyle = AccessorStyleEnum.PUBLIC;
            legacyDef.CodeGeneration = _codegen ? CodeGenerationEnum.ENABLED : CodeGenerationEnum.DISABLED;
            legacyDef.AddFieldProperty("explicitFSimple", "fieldLegacyVal");
            legacyDef.AddFieldProperty("explicitFIndexed", "fieldStringArray");
            legacyDef.AddFieldProperty("explicitFNested", "fieldNested");
            legacyDef.AddMethodProperty("explicitMSimple", "ReadLegacyBeanVal");
            legacyDef.AddMethodProperty("explicitMArray", "ReadStringArray");
            legacyDef.AddMethodProperty("explicitMIndexed", "ReadStringIndexed");
            legacyDef.AddMethodProperty("explicitMMapped", "ReadMapByKey");
            epService.EPAdministrator.Configuration.AddEventType<SupportLegacyBean>("MyLegacyEvent", legacyDef);
    
            legacyDef = new ConfigurationEventTypeLegacy();
            legacyDef.AccessorStyle = AccessorStyleEnum.PUBLIC;
            legacyDef.CodeGeneration = CodeGenerationEnum.DISABLED;
            epService.EPAdministrator.Configuration.AddEventType<SupportLegacyBean.LegacyNested>("MyLegacyNestedEvent", legacyDef);
    
            // assert type metadata
            EventTypeSPI type = (EventTypeSPI) ((EPServiceProviderSPI) epService).EventAdapterService.GetEventTypeByName("MyLegacyEvent");
            Assert.AreEqual(ApplicationType.CLASS, type.Metadata.OptionalApplicationType);
            Assert.AreEqual(1, type.Metadata.OptionalSecondaryNames.Count);
            Assert.AreEqual(typeof(SupportLegacyBean).Name, type.Metadata.OptionalSecondaryNames.First());
            Assert.AreEqual("MyLegacyEvent", type.Metadata.PrimaryName);
            Assert.AreEqual("MyLegacyEvent", type.Metadata.PublicName);
            Assert.AreEqual(TypeClass.APPLICATION, type.Metadata.TypeClass);
            Assert.AreEqual(true, type.Metadata.IsApplicationConfigured);
            Assert.AreEqual(false, type.Metadata.IsApplicationPreConfigured);
            Assert.AreEqual(false, type.Metadata.IsApplicationPreConfiguredStatic);
    
            string statementText = "select " +
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
                    " from MyLegacyEvent#length(5)";
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(statementText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            EventType eventType = statement.EventType;
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
    
            SupportLegacyBean legacyBean = MakeSampleEvent();
            epService.EPRuntime.SendEvent(legacyBean);
    
            Assert.AreEqual(legacyBean.fieldLegacyVal, listener.LastNewData[0].Get("fieldSimple"));
            Assert.AreEqual(legacyBean.fieldStringArray, listener.LastNewData[0].Get("fieldArr"));
            Assert.AreEqual(legacyBean.fieldStringArray[1], listener.LastNewData[0].Get("fieldArrIndexed"));
            Assert.AreEqual(legacyBean.fieldMapped, listener.LastNewData[0].Get("fieldMap"));
            Assert.AreEqual(legacyBean.fieldNested, listener.LastNewData[0].Get("fieldNested"));
            Assert.AreEqual(legacyBean.fieldNested.ReadNestedValue(), listener.LastNewData[0].Get("fieldNestedVal"));
    
            Assert.AreEqual(legacyBean.ReadLegacyBeanVal(), listener.LastNewData[0].Get("simple"));
            Assert.AreEqual(legacyBean.ReadLegacyNested(), listener.LastNewData[0].Get("nestedObject"));
            Assert.AreEqual(legacyBean.ReadLegacyNested().ReadNestedValue(), listener.LastNewData[0].Get("nested"));
            Assert.AreEqual(legacyBean.ReadStringIndexed(0), listener.LastNewData[0].Get("array"));
            Assert.AreEqual(legacyBean.ReadStringIndexed(1), listener.LastNewData[0].Get("indexed"));
            Assert.AreEqual(legacyBean.ReadMapByKey("key1"), listener.LastNewData[0].Get("mapped"));
            Assert.AreEqual(legacyBean.ReadMap(), listener.LastNewData[0].Get("mapItself"));
    
            Assert.AreEqual(legacyBean.ReadLegacyBeanVal(), listener.LastNewData[0].Get("explicitFSimple"));
            Assert.AreEqual(legacyBean.ReadLegacyBeanVal(), listener.LastNewData[0].Get("explicitMSimple"));
            Assert.AreEqual(legacyBean.ReadLegacyNested(), listener.LastNewData[0].Get("explicitFNested"));
            Assert.AreEqual(legacyBean.ReadStringIndexed(0), listener.LastNewData[0].Get("explicitFIndexed[0]"));
            Assert.AreEqual(legacyBean.ReadStringIndexed(0), listener.LastNewData[0].Get("explicitMArray[0]"));
            Assert.AreEqual(legacyBean.ReadStringIndexed(1), listener.LastNewData[0].Get("explicitMIndexed[1]"));
            Assert.AreEqual(legacyBean.ReadMapByKey("key2"), listener.LastNewData[0].Get("explicitMMapped('key2')"));
    
            EventTypeSPI stmtType = (EventTypeSPI) statement.EventType;
            Assert.AreEqual(ApplicationType.MAP, stmtType.Metadata.OptionalApplicationType);
            Assert.AreEqual(null, stmtType.Metadata.OptionalSecondaryNames);
            Assert.IsNotNull(stmtType.Metadata.PrimaryName);
            Assert.IsNotNull(stmtType.Metadata.PublicName);
            Assert.IsNotNull(stmtType.Name);
            Assert.AreEqual(TypeClass.ANONYMOUS, stmtType.Metadata.TypeClass);
            Assert.AreEqual(false, stmtType.Metadata.IsApplicationConfigured);
            Assert.AreEqual(false, stmtType.Metadata.IsApplicationPreConfigured);
            Assert.AreEqual(false, stmtType.Metadata.IsApplicationPreConfiguredStatic);
        }
    }
} // end of namespace
