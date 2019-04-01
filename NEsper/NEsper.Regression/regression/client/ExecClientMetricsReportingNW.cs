///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.client.metric;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;

namespace com.espertech.esper.regression.client
{
    public class ExecClientMetricsReportingNW : RegressionExecution
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override void Configure(Configuration configuration) {
            ApplyMetricsConfig(configuration, -1, 1000, false);
        }
    
        public override void Run(EPServiceProvider epService) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            epService.EPAdministrator.CreateEPL("@Name('0') create schema StatementMetric as " + typeof(StatementMetric).FullName);
            epService.EPAdministrator.CreateEPL("@Name('A') create window MyWindow#lastevent as select * from SupportBean");
            epService.EPAdministrator.CreateEPL("@Name('B1') insert into MyWindow select * from SupportBean");
            epService.EPAdministrator.CreateEPL("@Name('B2') insert into MyWindow select * from SupportBean");
            epService.EPAdministrator.CreateEPL("@Name('C') select sum(IntPrimitive) from MyWindow");
            epService.EPAdministrator.CreateEPL("@Name('D') select sum(w1.IntPrimitive) from MyWindow w1, MyWindow w2");
    
            string appModuleTwo = "@Name('W') create window SupportBeanWindow#keepall as SupportBean;" +
                    "" +
                    "@Name('M') on SupportBean oe\n" +
                    "  merge SupportBeanWindow pw\n" +
                    "  where pw.TheString = oe.TheString\n" +
                    "  when not matched \n" +
                    "    then insert select *\n" +
                    "  when matched and oe.IntPrimitive=1\n" +
                    "    then delete\n" +
                    "  when matched\n" +
                    "    then update set pw.IntPrimitive = oe.IntPrimitive";
            epService.EPAdministrator.DeploymentAdmin.ParseDeploy(appModuleTwo, null, null, null);
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("@Name('X') select * from StatementMetric");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            string[] fields = "statementName,numInput".Split(',');
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
            EventBean[] received = ArrayHandlingUtil.Reorder("statementName", listener.GetNewDataListFlattened());
            foreach (EventBean theEvent in received) {
                Log.Info(theEvent.Get("statementName") + " = " + theEvent.Get("numInput"));
            }
            EPAssertionUtil.AssertPropsPerRow(received, fields, new object[][]{new object[] {"A", 2L}, new object[] {"B1", 1L}, new object[] {"B2", 1L}, new object[] {"C", 2L}, new object[] {"D", 2L}, new object[] {"M", 1L}, new object[] {"W", 1L}});
    
            /* Comment-in for printout.
            for (int i = 0; i < received.Length; i++) {
                EventBean @event = received[i];
                Log.Info(@event.Get("statementName") + " " + @event.Get("wallTime") + " " + @event.Get("numInput"));
            }
            */
        }
    
        internal static void ApplyMetricsConfig(Configuration configuration, long engineMetricInterval, long stmtMetricInterval, bool shareViews) {
            configuration.EngineDefaults.ViewResources.IsShareViews = shareViews;
            configuration.EngineDefaults.MetricsReporting.IsEnableMetricsReporting = true;
            configuration.EngineDefaults.MetricsReporting.IsThreading = false;
            configuration.EngineDefaults.MetricsReporting.EngineInterval = engineMetricInterval;
            configuration.EngineDefaults.MetricsReporting.StatementInterval = stmtMetricInterval;
            configuration.AddImport(typeof(MyMetricFunctions).FullName);
            configuration.AddEventType<SupportBean>();
        }
    }
} // end of namespace
