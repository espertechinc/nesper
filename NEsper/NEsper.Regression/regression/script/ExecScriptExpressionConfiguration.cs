///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.script
{
    public class ExecScriptExpressionConfiguration : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Scripts.DefaultDialect = "dummy";
            configuration.AddEventType<SupportBean>();
        }
    
        public override void Run(EPServiceProvider epService) {
            try {
                epService.EPAdministrator.CreateEPL("expression abc [10] select * from SupportBean");
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual("Failed to obtain script engine for dialect 'dummy' for script 'abc' [expression abc [10] select * from SupportBean]", ex.Message);
            }
        }
    }
} // end of namespace
