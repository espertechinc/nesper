///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

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
    public class ExecEventBeanExplicitOnly : RegressionExecution {
        private readonly bool codegen;
    
        public ExecEventBeanExplicitOnly(bool codegen) {
            this.codegen = codegen;
        }
    
        public override void Configure(Configuration configuration) {
            var codeGeneration = codegen ? CodeGenerationEnum.ENABLED : CodeGenerationEnum.DISABLED;
            var legacyDef = new ConfigurationEventTypeLegacy();
            legacyDef.AccessorStyle = AccessorStyleEnum.EXPLICIT;
            legacyDef.CodeGeneration = codeGeneration;
            legacyDef.AddFieldProperty("explicitFNested", "fieldNested");
            legacyDef.AddMethodProperty("explicitMNested", "ReadLegacyNested");
            configuration.AddEventType("MyLegacyEvent", typeof(SupportLegacyBean).AssemblyQualifiedName, legacyDef);
    
            legacyDef = new ConfigurationEventTypeLegacy();
            legacyDef.AccessorStyle = AccessorStyleEnum.EXPLICIT;
            legacyDef.CodeGeneration = codeGeneration;
            legacyDef.AddFieldProperty("fieldNestedClassValue", "fieldNestedValue");
            legacyDef.AddMethodProperty("ReadNestedClassValue", "ReadNestedValue");
            configuration.AddEventType("MyLegacyNestedEvent", typeof(SupportLegacyBean.LegacyNested).AssemblyQualifiedName, legacyDef);
    
            legacyDef = new ConfigurationEventTypeLegacy();
            legacyDef.AccessorStyle = AccessorStyleEnum.EXPLICIT;
            legacyDef.CodeGeneration = codeGeneration;
            configuration.AddEventType("MySupportBean", typeof(SupportBean).AssemblyQualifiedName, legacyDef);
        }
    
        public override void Run(EPServiceProvider epService) {
            string statementText = "select " +
                    "explicitFNested.fieldNestedClassValue as fnested, " +
                    "explicitMNested.ReadNestedClassValue as mnested" +
                    " from MyLegacyEvent#length(5)";
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(statementText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            EventType eventType = statement.EventType;
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("fnested"));
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("mnested"));
    
            SupportLegacyBean legacyBean = ExecEventBeanPublicAccessors.MakeSampleEvent();
            epService.EPRuntime.SendEvent(legacyBean);
    
            Assert.AreEqual(legacyBean.fieldNested.ReadNestedValue(), listener.LastNewData[0].Get("fnested"));
            Assert.AreEqual(legacyBean.fieldNested.ReadNestedValue(), listener.LastNewData[0].Get("mnested"));
    
            try {
                // invalid statement, JavaBean-style getters not exposed
                statementText = "select IntPrimitive from MySupportBean#length(5)";
                epService.EPAdministrator.CreateEPL(statementText);
            } catch (EPStatementException) {
                // expected
            }
        }
    }
} // end of namespace
