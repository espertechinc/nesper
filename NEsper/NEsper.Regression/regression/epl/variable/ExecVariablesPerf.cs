///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl.variable
{
    public class ExecVariablesPerf : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType<SupportBean_S0>();
            epService.EPAdministrator.CreateEPL("create window MyWindow#keepall as SupportBean");
            epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");
            epService.EPAdministrator.CreateEPL("create const variable string MYCONST = 'E331'");
    
            for (int i = 0; i < 10000; i++) {
                epService.EPRuntime.SendEvent(new SupportBean("E" + i, i * -1));
            }
    
            // test join
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from SupportBean_S0 s0 unidirectional, MyWindow sb where TheString = MYCONST");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            long delta = PerformanceObserver.TimeMillis(
                () =>
                {
                    ;
                    for (int i = 0; i < 10000; i++) {
                        epService.EPRuntime.SendEvent(new SupportBean_S0(i, "E" + i));
                        EPAssertionUtil.AssertProps(
                            listener.AssertOneGetNewAndReset(), "sb.TheString,sb.IntPrimitive".Split(','),
                            new object[] {"E331", -331});
                    }
                });

            Assert.IsTrue(delta < 500, "delta=" + delta);
            stmt.Dispose();
    
            // test subquery
            EPStatement stmtSubquery = epService.EPAdministrator.CreateEPL("select * from SupportBean_S0 where exists (select * from MyWindow where TheString = MYCONST)");
            stmtSubquery.Events += listener.Update;

            delta = PerformanceObserver.TimeMillis(
                () =>
                {
                    for (int i = 0; i < 10000; i++) {
                        epService.EPRuntime.SendEvent(new SupportBean_S0(i, "E" + i));
                        Assert.IsTrue(listener.GetAndClearIsInvoked());
                    }
                });
            Assert.IsTrue(delta < 500, "delta=" + delta);
        }
    }
} // end of namespace
