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

namespace com.espertech.esper.regression.epl.other
{
    public class ExecEPLStaticFunctionsNoUDFCache : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.AddImport(typeof(SupportStaticMethodLib).FullName);
            configuration.AddPlugInSingleRowFunction("sleepme", typeof(SupportStaticMethodLib), "Sleep", ValueCacheEnum.ENABLED);
            configuration.EngineDefaults.Expression.IsUdfCache = false;
            configuration.AddEventType("Temperature", typeof(SupportTemperatureBean));
        }
    
        public override void Run(EPServiceProvider epService) {
            string text = "select SupportStaticMethodLib.Sleep(100) as val from Temperature as temp";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            long startTime = DateTimeHelper.CurrentTimeMillis;
            epService.EPRuntime.SendEvent(new SupportTemperatureBean("a"));
            epService.EPRuntime.SendEvent(new SupportTemperatureBean("a"));
            epService.EPRuntime.SendEvent(new SupportTemperatureBean("a"));
            long endTime = DateTimeHelper.CurrentTimeMillis;
            long delta = endTime - startTime;
    
            Assert.IsTrue(delta > 120, "Failed perf test, delta=" + delta);
            stmt.Dispose();
    
            // test plug-in single-row function
            string textSingleRow = "select " +
                    "Sleepme(100) as val" +
                    " from Temperature as temp";
            EPStatement stmtSingleRow = epService.EPAdministrator.CreateEPL(textSingleRow);
            var listenerSingleRow = new SupportUpdateListener();
            stmtSingleRow.Events += listenerSingleRow.Update;
    
            startTime = DateTimeHelper.CurrentTimeMillis;
            for (int i = 0; i < 1000; i++) {
                epService.EPRuntime.SendEvent(new SupportTemperatureBean("a"));
            }
            delta = DateTimeHelper.CurrentTimeMillis - startTime;
    
            Assert.IsTrue(delta < 1000, "Failed perf test, delta=" + delta);
            stmtSingleRow.Dispose();
        }
    }
} // end of namespace
