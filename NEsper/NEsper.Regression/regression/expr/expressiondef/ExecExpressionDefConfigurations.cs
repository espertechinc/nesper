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
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.expr.expressiondef
{
    public class ExecExpressionDefConfigurations : RegressionExecution {
    
        private readonly int? configuredCacheSize;
        private readonly int expectedInvocationCount;
    
        public ExecExpressionDefConfigurations(int? configuredCacheSize, int expectedInvocationCount) {
            this.configuredCacheSize = configuredCacheSize;
            this.expectedInvocationCount = expectedInvocationCount;
        }
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("SupportBean_ST0", typeof(SupportBean_ST0));
            configuration.AddEventType("SupportBean_ST1", typeof(SupportBean_ST1));
    
            // set cache size
            if (configuredCacheSize != null) {
                configuration.EngineDefaults.Execution.DeclaredExprValueCacheSize = configuredCacheSize.Value;
            }
        }
    
        public override void Run(EPServiceProvider epService) {
    
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("alwaysTrue", typeof(SupportStaticMethodLib), "AlwaysTrue");
    
            // set up
            EPStatement stmt = epService.EPAdministrator.CreateEPL(
                    "expression myExpr {v => alwaysTrue(null) } select myExpr(st0) as c0, myExpr(st1) as c1, myExpr(st0) as c2, myExpr(st1) as c3 from SupportBean_ST0#lastevent as st0, SupportBean_ST1#lastevent as st1");
            stmt.Events += new SupportUpdateListener().Update;
    
            // send event and assert
            SupportStaticMethodLib.Invocations.Clear();
            epService.EPRuntime.SendEvent(new SupportBean_ST0("a", 0));
            epService.EPRuntime.SendEvent(new SupportBean_ST1("a", 0));
            Assert.AreEqual(expectedInvocationCount, SupportStaticMethodLib.Invocations.Count);
        }
    }
} // end of namespace
