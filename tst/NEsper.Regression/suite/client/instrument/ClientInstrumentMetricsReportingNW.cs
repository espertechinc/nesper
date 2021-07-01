///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;

using com.espertech.esper.common.client.metric;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

namespace com.espertech.esper.regressionlib.suite.client.instrument
{
    public class ClientInstrumentMetricsReportingNW : RegressionExecution
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void Run(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            env.AdvanceTime(0);
            env.CompileDeploy("@Name('0') create schema StatementMetric as " + typeof(StatementMetric).FullName);
            env.CompileDeploy("@Name('A') create window MyWindow#lastevent as select * from SupportBean", path);
            env.CompileDeploy("@Name('B1') insert into MyWindow select * from SupportBean", path);
            env.CompileDeploy("@Name('B2') insert into MyWindow select * from SupportBean", path);
            env.CompileDeploy("@Name('C') select sum(IntPrimitive) from MyWindow", path);
            env.CompileDeploy("@Name('D') select sum(w1.IntPrimitive) from MyWindow w1, MyWindow w2", path);

            var appModuleTwo = "@Name('W') create window SupportBeanWindow#keepall as SupportBean;" +
                               "" +
                               "@Name('M') on SupportBean oe\n" +
                               "  merge SupportBeanWindow Pw\n" +
                               "  where Pw.TheString = oe.TheString\n" +
                               "  when not matched \n" +
                               "    then insert select *\n" +
                               "  when matched and oe.IntPrimitive=1\n" +
                               "    then delete\n" +
                               "  when matched\n" +
                               "    then update set Pw.IntPrimitive = oe.IntPrimitive";
            env.CompileDeploy(appModuleTwo, path);

            env.CompileDeploy("@Name('X') select * from " + typeof(StatementMetric).FullName).AddListener("X");
            var fields = new [] { "StatementName","NumInput" };
            
            env.SendEventBean(new SupportBean("E1", 1));
            env.AdvanceTime(1000);
            var received = ArrayHandlingUtil.Reorder("StatementName", env.Listener("X").NewDataListFlattened);
            foreach (var theEvent in received) {
                Log.Info(theEvent.Get("StatementName") + " = " + theEvent.Get("NumInput"));
            }

            EPAssertionUtil.AssertPropsPerRow(
                received,
                fields,
                new[] {
                    new object[] {"A", 2L},
                    new object[] {"B1", 1L},
                    new object[] {"B2", 1L},
                    new object[] {"C", 2L},
                    new object[] {"D", 2L},
                    new object[] {"M", 1L},
                    new object[] {"W", 1L}
                });

            env.UndeployAll();
        }
    }
} // end of namespace