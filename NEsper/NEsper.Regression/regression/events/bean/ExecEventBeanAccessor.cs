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
    public class ExecEventBeanAccessor : RegressionExecution {
        private readonly bool _codegen;
    
        public ExecEventBeanAccessor(bool codegen) {
            this._codegen = codegen;
        }
    
        public override void Configure(Configuration configuration) {
            var legacyDef = new ConfigurationEventTypeLegacy();
            legacyDef.AccessorStyle = AccessorStyleEnum.NATIVE;
            legacyDef.CodeGeneration = _codegen ? CodeGenerationEnum.ENABLED : CodeGenerationEnum.DISABLED;
            legacyDef.AddFieldProperty("explicitFInt", "fieldIntPrimitive");
            legacyDef.AddMethodProperty("explicitMGetInt", "get_IntPrimitive");
            legacyDef.AddMethodProperty("explicitMReadInt", "ReadIntPrimitive");
            configuration.AddEventType("MyLegacyEvent", typeof(SupportLegacyBeanInt).AssemblyQualifiedName, legacyDef);
        }
    
        public override void Run(EPServiceProvider epService) {
            string statementText = "select IntPrimitive, explicitFInt, explicitMGetInt, explicitMReadInt " +
                    " from MyLegacyEvent#length(5)";
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(statementText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
            EventType eventType = statement.EventType;
    
            var theEvent = new SupportLegacyBeanInt(10);
            epService.EPRuntime.SendEvent(theEvent);
    
            foreach (string name in new string[]{"IntPrimitive", "explicitFInt", "explicitMGetInt", "explicitMReadInt"}) {
                Assert.AreEqual(typeof(int), eventType.GetPropertyType(name));
                Assert.AreEqual(10, listener.LastNewData[0].Get(name));
            }
        }
    }
} // end of namespace
