///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.client.metric;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    [TestFixture]
    public class TestMetricsReporting
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
        private SupportUpdateListener _listenerStmtMetric;
        private SupportUpdateListener _listenerEngineMetric;
        private SupportUpdateListener _listenerTwo;

        private const long CpuGoalOneNano = 80 * 1000 * 1000;
        private const long CpuGoalTwoNano = 50 * 1000 * 1000;
        private const long WallGoalOneMsec = 200;
        private const long WallGoalTwoMsec = 400;

        [SetUp]
        public void SetUp()
        {
            EPServiceProviderManager.PurgeAllProviders();

            _listener = new SupportUpdateListener();
            _listenerTwo = new SupportUpdateListener();

            _listenerStmtMetric = new SupportUpdateListener();
            _listenerEngineMetric = new SupportUpdateListener();
        }

        [TearDown]
        public void TearDown()
        {
            _listener = null;
            _listenerTwo = null;
            _listenerEngineMetric = null;
            _listenerStmtMetric = null;

            try
            {
                if (_epService != null)
                {
                    _epService.Dispose();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        [Test]
        public void TestNamedWindowAndViewShare()
        {
            _epService = EPServiceProviderManager.GetDefaultProvider(GetConfig(-1, 1000, false));
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            _epService.EPAdministrator.CreateEPL("@Name('0') create schema StatementMetric as " + typeof(StatementMetric).FullName);
            _epService.EPAdministrator.CreateEPL("@Name('A') create window MyWindow.std:lastevent() as select * from SupportBean");
            _epService.EPAdministrator.CreateEPL("@Name('B1') insert into MyWindow select * from SupportBean");
            _epService.EPAdministrator.CreateEPL("@Name('B2') insert into MyWindow select * from SupportBean");
            _epService.EPAdministrator.CreateEPL("@Name('C') select sum(IntPrimitive) from MyWindow");
            _epService.EPAdministrator.CreateEPL("@Name('D') select sum(w1.IntPrimitive) from MyWindow w1, MyWindow w2");

            String appModuleTwo =
                "@Name('W') create window SupportBeanWindow.win:keepall() as SupportBean;" +
                "" +
                "@Name('M') on SupportBean oe\n" +
                "  merge SupportBeanWindow pw\n" +
                "  where pw.TheString = oe.TheString\n" +
                "  when not matched \n" +
                "    then insert select *\n" +
                "  when matched and oe.IntPrimitive=1\n" +
                "    then delete\n" +
                "  when matched\n" +
                "    then Update set pw.IntPrimitive = oe.IntPrimitive";
            _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(appModuleTwo, null, null, null);

            EPStatement stmt = _epService.EPAdministrator.CreateEPL("@Name('X') select * from StatementMetric");
            stmt.Events += _listener.Update;
            String[] fields = "StatementName,numInput".Split(',');

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
            EventBean[] received = ArrayHandlingUtil.Reorder("StatementName", _listener.GetNewDataListFlattened());

            received.ToList().ForEach(
                theEvent => Debug.WriteLine(theEvent.Get("StatementName") + " = " + theEvent.Get("numInput")));

            EPAssertionUtil.AssertPropsPerRow(
                received, fields, 
                new Object[][]
                {
                    new Object[] { "A", 2L }, 
                    new Object[] { "B1", 1L }, 
                    new Object[] { "B2", 1L }, 
                    new Object[] { "C", 2L }, 
                    new Object[] { "D", 2L }, 
                    new Object[] { "M", 1L }, 
                    new Object[] { "W", 1L }
                });

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        [Test]
        public void TestEngineMetrics()
        {
            _epService = EPServiceProviderManager.GetProvider("MyURI", GetConfig(10000, -1, true));
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            String[] engineFields = "engineURI,timestamp,inputCount,inputCountDelta,scheduleDepth".Split(',');
            SendTimer(1000);

            String text = "select * from " + typeof(EngineMetric).FullName;
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(text);
            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean());

            SendTimer(10999);
            Assert.IsFalse(_listener.IsInvoked);

            _epService.EPAdministrator.CreateEPL("select * from pattern[timer:interval(5 sec)]");

            SendTimer(11000);
            EventBean theEvent = _listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(theEvent, engineFields, new Object[] { "MyURI", 11000L, 1L, 1L, 1L });

            _epService.EPRuntime.SendEvent(new SupportBean());
            _epService.EPRuntime.SendEvent(new SupportBean());

            SendTimer(20000);
            SendTimer(21000);
            theEvent = _listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(theEvent, engineFields, new Object[] { "MyURI", 21000L, 4L, 3L, 0L });

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        [Test]
        public void TestStatementGroups()
        {
            Configuration config = GetConfig(-1, 7000, true);

            ConfigurationMetricsReporting.StmtGroupMetrics groupOne = new ConfigurationMetricsReporting.StmtGroupMetrics();
            groupOne.Interval = 8000;
            groupOne.AddIncludeLike("%GroupOne%");
            groupOne.IsReportInactive = true;
            config.EngineDefaults.MetricsReportingConfig.AddStmtGroup("GroupOneStatements", groupOne);

            ConfigurationMetricsReporting.StmtGroupMetrics groupTwo = new ConfigurationMetricsReporting.StmtGroupMetrics();
            groupTwo.Interval = 6000;
            groupTwo.IsDefaultInclude = true;
            groupTwo.AddExcludeLike("%Default%");
            groupTwo.AddExcludeLike("%Metrics%");
            config.EngineDefaults.MetricsReportingConfig.AddStmtGroup("GroupTwoNonDefaultStatements", groupTwo);

            ConfigurationMetricsReporting.StmtGroupMetrics groupThree = new ConfigurationMetricsReporting.StmtGroupMetrics();
            groupThree.Interval = -1;
            groupThree.AddIncludeLike("%Metrics%");
            config.EngineDefaults.MetricsReportingConfig.AddStmtGroup("MetricsStatements", groupThree);

            _epService = EPServiceProviderManager.GetProvider("MyURI", config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            SendTimer(0);

            _epService.EPAdministrator.CreateEPL("select * from SupportBean(IntPrimitive = 1).win:keepall()", "GroupOne");
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select * from SupportBean(IntPrimitive = 2).win:keepall()", "GroupTwo");
            stmt.Subscriber = new SupportSubscriber();
            _epService.EPAdministrator.CreateEPL("select * from SupportBean(IntPrimitive = 3).win:keepall()", "Default");   // no listener

            stmt = _epService.EPAdministrator.CreateEPL("select * from " + typeof(StatementMetric).FullName, "StmtMetrics");
            stmt.Events += _listener.Update;

            SendTimer(6000);
            SendTimer(7000);
            Assert.IsFalse(_listener.IsInvoked);

            SendTimer(8000);
            String[] fields = "StatementName,OutputIStreamCount,NumInput".Split(',');
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { "GroupOne", 0L, 0L });

            SendTimer(12000);
            SendTimer(14000);
            SendTimer(15999);
            Assert.IsFalse(_listener.IsInvoked);

            SendTimer(16000);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { "GroupOne", 0L, 0L });

            // should report as groupTwo
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            SendTimer(17999);
            Assert.IsFalse(_listener.IsInvoked);

            SendTimer(18000);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { "GroupTwo", 1L, 1L });

            // should report as groupTwo
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 3));
            SendTimer(20999);
            Assert.IsFalse(_listener.IsInvoked);

            SendTimer(21000);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { "Default", 0L, 1L });

            // turn off group 1
            _epService.EPAdministrator.Configuration.SetMetricsReportingInterval("GroupOneStatements", -1);
            SendTimer(24000);
            Assert.IsFalse(_listener.IsInvoked);

            // turn on group 1
            _epService.EPAdministrator.Configuration.SetMetricsReportingInterval("GroupOneStatements", 1000);
            SendTimer(25000);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { "GroupOne", 0L, 0L });

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        [Test]
        public void TestStatementMetrics()
        {
            Configuration config = GetConfig(-1, -1, true);

            // report on all statements every 10 seconds
            ConfigurationMetricsReporting.StmtGroupMetrics configOne = new ConfigurationMetricsReporting.StmtGroupMetrics();
            configOne.Interval = 10000;
            configOne.AddIncludeLike("%cpuStmt%");
            configOne.AddIncludeLike("%wallStmt%");
            config.EngineDefaults.MetricsReportingConfig.AddStmtGroup("nonmetrics", configOne);

            // exclude metrics themselves from reporting
            ConfigurationMetricsReporting.StmtGroupMetrics configTwo = new ConfigurationMetricsReporting.StmtGroupMetrics();
            configTwo.Interval = -1;
            configOne.AddExcludeLike("%metrics%");
            config.EngineDefaults.MetricsReportingConfig.AddStmtGroup("metrics", configTwo);

            _epService = EPServiceProviderManager.GetProvider("MyURI", config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            SendTimer(1000);

            EPStatement[] statements = new EPStatement[5];
            statements[0] = _epService.EPAdministrator.CreateEPL("select * from " + typeof(StatementMetric).FullName, "stmt_metrics");
            statements[0].Events += _listener.Update;

            statements[1] = _epService.EPAdministrator.CreateEPL("select * from SupportBean(IntPrimitive=1).win:keepall() where MyMetricFunctions.TakeCpuTime(LongPrimitive)", "cpuStmtOne");
            statements[1].Events += _listenerTwo.Update;
            statements[2] = _epService.EPAdministrator.CreateEPL("select * from SupportBean(IntPrimitive=2).win:keepall() where MyMetricFunctions.TakeCpuTime(LongPrimitive)", "cpuStmtTwo");
            statements[2].Events += _listenerTwo.Update;
            statements[3] = _epService.EPAdministrator.CreateEPL("select * from SupportBean(IntPrimitive=3).win:keepall() where MyMetricFunctions.TakeWallTime(LongPrimitive)", "wallStmtThree");
            statements[3].Events += _listenerTwo.Update;
            statements[4] = _epService.EPAdministrator.CreateEPL("select * from SupportBean(IntPrimitive=4).win:keepall() where MyMetricFunctions.TakeWallTime(LongPrimitive)", "wallStmtFour");
            statements[4].Events += _listenerTwo.Update;

            SendEvent("E1", 1, CpuGoalOneNano);
            SendEvent("E2", 2, CpuGoalTwoNano);
            SendEvent("E3", 3, WallGoalOneMsec);
            SendEvent("E4", 4, WallGoalTwoMsec);

            SendTimer(10999);
            Assert.IsFalse(_listener.IsInvoked);

            SendTimer(11000);
            RunAssertion(11000);

            SendEvent("E1", 1, CpuGoalOneNano);
            SendEvent("E2", 2, CpuGoalTwoNano);
            SendEvent("E3", 3, WallGoalOneMsec);
            SendEvent("E4", 4, WallGoalTwoMsec);

            SendTimer(21000);
            RunAssertion(21000);

            // destroy all application stmts
            for (int i = 1; i < 5; i++)
            {
                statements[i].Dispose();
            }
            SendTimer(31000);
            Assert.IsFalse(_listener.IsInvoked);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        [Test]
        public void TestEnabledDisableRuntime()
        {
            EPStatement[] statements = new EPStatement[5];
            Configuration config = GetConfig(10000, 10000, true);
            _epService = EPServiceProviderManager.GetProvider("MyURI", config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            SendTimer(1000);

            statements[0] = _epService.EPAdministrator.CreateEPL("select * from " + typeof(StatementMetric).FullName, "stmtmetric");
            statements[0].Events += _listenerStmtMetric.Update;

            statements[1] = _epService.EPAdministrator.CreateEPL("select * from " + typeof(EngineMetric).FullName, "enginemetric");
            statements[1].Events += _listenerEngineMetric.Update;

            statements[2] = _epService.EPAdministrator.CreateEPL("select * from SupportBean(IntPrimitive=1).win:keepall() where MyMetricFunctions.TakeCpuTime(LongPrimitive)");
            SendEvent("E1", 1, CpuGoalOneNano);

            SendTimer(11000);
            Assert.IsTrue(_listenerStmtMetric.GetAndClearIsInvoked());
            Assert.IsTrue(_listenerEngineMetric.GetAndClearIsInvoked());

            _epService.EPAdministrator.Configuration.SetMetricsReportingDisabled();
            SendEvent("E2", 2, CpuGoalOneNano);
            SendTimer(21000);
            Assert.IsFalse(_listenerStmtMetric.GetAndClearIsInvoked());
            Assert.IsFalse(_listenerEngineMetric.GetAndClearIsInvoked());

            SendTimer(31000);
            SendEvent("E3", 3, CpuGoalOneNano);
            Assert.IsFalse(_listenerStmtMetric.GetAndClearIsInvoked());
            Assert.IsFalse(_listenerEngineMetric.GetAndClearIsInvoked());

            _epService.EPAdministrator.Configuration.SetMetricsReportingEnabled();
            SendEvent("E4", 4, CpuGoalOneNano);
            SendTimer(41000);
            Assert.IsTrue(_listenerStmtMetric.GetAndClearIsInvoked());
            Assert.IsTrue(_listenerEngineMetric.GetAndClearIsInvoked());

            statements[2].Dispose();
            SendTimer(51000);
            Assert.IsTrue(_listenerStmtMetric.IsInvoked); // metrics statements reported themselves
            Assert.IsTrue(_listenerEngineMetric.IsInvoked);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        [Test]
        public void TestEnabledDisableStatement()
        {
            var fields = new String[] { "StatementName" };
            var statements = new EPStatement[5];
            var config = GetConfig(-1, 10000, true);

            var configOne = new ConfigurationMetricsReporting.StmtGroupMetrics();
            configOne.Interval = -1;
            configOne.AddIncludeLike("%@METRIC%");
            config.EngineDefaults.MetricsReportingConfig.AddStmtGroup("metrics", configOne);

            _epService = EPServiceProviderManager.GetProvider("MyURI", config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            SendTimer(1000);

            statements[0] = _epService.EPAdministrator.CreateEPL("select * from " + typeof(StatementMetric).FullName, "MyStatement@METRIC");
            statements[0].Events += _listenerStmtMetric.Update;

            statements[1] = _epService.EPAdministrator.CreateEPL("select * from SupportBean(IntPrimitive=1).win:keepall() where 2=2", "stmtone");
            SendEvent("E1", 1, CpuGoalOneNano);
            statements[2] = _epService.EPAdministrator.CreateEPL("select * from SupportBean(IntPrimitive>0).std:lastevent() where 1=1", "stmttwo");
            SendEvent("E2", 1, CpuGoalOneNano);

            SendTimer(11000);
            EPAssertionUtil.AssertPropsPerRow(_listenerStmtMetric.GetNewDataListFlattened(), fields, new Object[][] { new Object[] { "stmtone" }, new Object[] { "stmttwo" } });
            _listenerStmtMetric.Reset();

            SendEvent("E1", 1, CpuGoalOneNano);
            SendTimer(21000);
            EPAssertionUtil.AssertPropsPerRow(_listenerStmtMetric.GetNewDataListFlattened(), fields, new Object[][] { new Object[] { "stmtone" }, new Object[] { "stmttwo" } });
            _listenerStmtMetric.Reset();

            _epService.EPAdministrator.Configuration.SetMetricsReportingStmtDisabled("stmtone");

            SendEvent("E1", 1, CpuGoalOneNano);
            SendTimer(31000);
            EPAssertionUtil.AssertPropsPerRow(_listenerStmtMetric.GetNewDataListFlattened(), fields, new Object[][] { new Object[] { "stmttwo" } });
            _listenerStmtMetric.Reset();

            _epService.EPAdministrator.Configuration.SetMetricsReportingStmtEnabled("stmtone");
            _epService.EPAdministrator.Configuration.SetMetricsReportingStmtDisabled("stmttwo");

            SendEvent("E1", 1, CpuGoalOneNano);
            SendTimer(41000);
            EPAssertionUtil.AssertPropsPerRow(_listenerStmtMetric.GetNewDataListFlattened(), fields, new Object[][] { new Object[] { "stmtone" } });
            _listenerStmtMetric.Reset();

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        private void RunAssertion(long timestamp)
        {
            String[] fields = "engineURI,StatementName".Split(',');

            Assert.AreEqual(4, _listener.NewDataList.Count);
            EventBean[] received = _listener.GetNewDataListFlattened();

            EPAssertionUtil.AssertProps(received[0], fields, new Object[] { "MyURI", "cpuStmtOne" });
            EPAssertionUtil.AssertProps(received[1], fields, new Object[] { "MyURI", "cpuStmtTwo" });
            EPAssertionUtil.AssertProps(received[2], fields, new Object[] { "MyURI", "wallStmtThree" });
            EPAssertionUtil.AssertProps(received[3], fields, new Object[] { "MyURI", "wallStmtFour" });

            long cpuOne = received[0].Get("CpuTime").AsLong();
            long cpuTwo = received[1].Get("CpuTime").AsLong();
            long wallOne = received[2].Get("WallTime").AsLong();
            long wallTwo = received[3].Get("WallTime").AsLong();

            // Windows has been consistently giving us less measured time than we actually measure
            // on the wall (userTime + kernelTime < wallTime).  We're not sure why this occurs, but
            // there are articles about inaccuracies in measurements that occur when the timer model
            // uses a tight sleep or spin lock.  I'll be reviewing them, but until that is complete
            // we will be using a 7% loss coefficient.
            const double lossCoefficient = 0.92;

            Assert.IsTrue(cpuOne > CpuGoalOneNano * lossCoefficient, "cpuOne=" + cpuOne);
            Assert.IsTrue(cpuTwo > CpuGoalTwoNano * lossCoefficient, "cpuTwo=" + cpuTwo);
            Assert.IsTrue((wallOne + 50) > WallGoalOneMsec, "wallOne=" + wallOne);
            Assert.IsTrue((wallTwo + 50) > WallGoalTwoMsec, "wallTwo=" + wallTwo);

            for (int i = 0; i < 4; i++)
            {
                Assert.AreEqual(1L, received[i].Get("OutputIStreamCount"));
                Assert.AreEqual(0L, received[i].Get("OutputRStreamCount"));
                Assert.AreEqual(timestamp, received[i].Get("timestamp"));
            }

            _listener.Reset();
        }

#if false
        [Test]
        public void TestTakeCPUTime() {
            ThreadMXBean mbean = ManagementFactory.ThreadMXBean;
            if (!mbean.IsThreadCpuTimeEnabled) {
                Fail("ThreadMXBean CPU time reporting is not enabled");
            }
    
            long msecMultiplier = 1000 * 1000;
            long msecGoal = 10;
            long cpuGoal = msecGoal * msecMultiplier;
    
            long beforeCPU = mbean.CurrentThreadCpuTime;
            MyMetricFunctions.TakeCPUTime(cpuGoal);
            long afterCPU = mbean.CurrentThreadCpuTime;
            Assert.IsTrue((afterCPU - beforeCPU) > cpuGoal);
        }
#endif

        /// <summary>
        /// Comment-in this test for manual/threading tests.
        /// </summary>
        [Test]
        public void TestManual()
        {
            Configuration config = GetConfig(1000, 1000, true);
            config.EngineDefaults.MetricsReportingConfig.IsThreading = true;

#if false
            epService = EPServiceProviderManager.GetProvider("MyURI", config);
            epService.Initialize();
    
            EPStatement[] statements = new EPStatement[5];
    
            statements[0] = epService.EPAdministrator.CreateEPL("select * from " + typeof(StatementMetric).FullName, "stmt_metrics");
            statements[0].AddListener(new PrintUpdateListener());
    
            statements[1] = epService.EPAdministrator.CreateEPL("select * from " + typeof(EngineMetric).FullName, "engine_metrics");
            statements[1].AddListener(new PrintUpdateListener());
    
            statements[2] = epService.EPAdministrator.CreateEPL("select * from SupportBean(IntPrimitive=1).win:keepall() where MyMetricFunctions.TakeCPUTime(LongPrimitive)", "cpuStmtOne");
    
            Sleep(20000);
#endif
        }

        private void SendTimer(long currentTime)
        {
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(currentTime));
        }

        private Configuration GetConfig(long engineMetricInterval, long stmtMetricInterval, bool shareViews)
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            configuration.EngineDefaults.ViewResourcesConfig.IsShareViews = shareViews;
            configuration.EngineDefaults.MetricsReportingConfig.IsEnableMetricsReporting = true;
            configuration.EngineDefaults.MetricsReportingConfig.IsThreading = false;
            configuration.EngineDefaults.MetricsReportingConfig.EngineInterval = engineMetricInterval;
            configuration.EngineDefaults.MetricsReportingConfig.StatementInterval = stmtMetricInterval;

            configuration.AddImport<MyMetricFunctions>();
            configuration.AddEventType<SupportBean>();

            return configuration;
        }

        private void SendEvent(String id, int intPrimitive, long longPrimitive)
        {
            SupportBean bean = new SupportBean(id, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            _epService.EPRuntime.SendEvent(bean);
        }

        private void Sleep(long msec)
        {
            Thread.Sleep((int)msec);
        }
    }
}
