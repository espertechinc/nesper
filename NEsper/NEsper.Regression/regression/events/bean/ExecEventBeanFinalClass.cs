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
    public class ExecEventBeanFinalClass : RegressionExecution {
        private readonly bool codegen;
    
        public ExecEventBeanFinalClass(bool codegen) {
            this.codegen = codegen;
        }
    
        public override void Configure(Configuration configuration) {
            var legacyDef = new ConfigurationEventTypeLegacy();
            legacyDef.AccessorStyle = AccessorStyleEnum.NATIVE;
            legacyDef.CodeGeneration = codegen ? CodeGenerationEnum.ENABLED : CodeGenerationEnum.DISABLED;
            configuration.AddEventType("MyFinalEvent", typeof(SupportBeanFinal).AssemblyQualifiedName, legacyDef);
        }
    
        public override void Run(EPServiceProvider epService) {
            string statementText = "select IntPrimitive " +
                    "from " + typeof(SupportBeanFinal).FullName + "#length(5)";
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(statementText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            var theEvent = new SupportBeanFinal(10);
            epService.EPRuntime.SendEvent(theEvent);
            Assert.AreEqual(10, listener.LastNewData[0].Get("IntPrimitive"));
        }
    }
} // end of namespace
