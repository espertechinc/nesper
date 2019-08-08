///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.runtime;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.suite.client.instrument;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.regressionrun.Runner;

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.client
{
    [TestFixture]
    public class TestSuiteClientInstrument
    {
        [Test]
        public void TestClientInstrumentInstrumentation()
        {
            RegressionSession session = RegressionRunner.Session();
            foreach (Type clazz in new Type[] { typeof(SupportBean) })
            {
                session.Configuration.Common.AddEventType(clazz);
            }
            RegressionRunner.Run(session, new ClientInstrumentInstrumentation());
            session.Destroy();
        }

        [Test]
        public void TestClientInstrumentAudit()
        {
            RegressionSession session = RegressionRunner.Session();
            foreach (Type clazz in new Type[] { typeof(SupportBean), typeof(SupportBean_ST0), typeof(SupportBean_ST1) })
            {
                session.Configuration.Common.AddEventType(clazz);
            }
            session.Configuration.Runtime.Logging.AuditPattern = "[%u] [%d] [%s] [%i] [%c] %m";
            RegressionRunner.Run(session, ClientInstrumentAudit.Executions());
            session.Destroy();
        }

        [Test]
        public void TestClientInstrumentMetricsReportingStmtMetrics()
        {
            RegressionSession session = RegressionRunner.Session();

            ApplyMetricsConfig(session.Configuration, -1, -1);

            ConfigurationRuntimeMetricsReporting.StmtGroupMetrics configOne = new ConfigurationRuntimeMetricsReporting.StmtGroupMetrics();
            configOne.Interval = 10000;
            configOne.AddIncludeLike("%cpuStmt%");
            configOne.AddIncludeLike("%wallStmt%");
            session.Configuration.Runtime.MetricsReporting.AddStmtGroup("nonmetrics", configOne);

            // exclude metrics themselves from reporting
            ConfigurationRuntimeMetricsReporting.StmtGroupMetrics configTwo = new ConfigurationRuntimeMetricsReporting.StmtGroupMetrics();
            configTwo.Interval = -1;
            configOne.AddExcludeLike("%metrics%");
            session.Configuration.Runtime.MetricsReporting.AddStmtGroup("metrics", configTwo);

            RegressionRunner.Run(session, new ClientInstrumentMetricsReportingStmtMetrics());

            session.Destroy();
        }

        [Test]
        public void TestClientInstrumentMetricsReportingStmtGroups()
        {
            RegressionSession session = RegressionRunner.Session();
            session.Configuration.Compiler.ByteCode.AllowSubscriber = true;

            ApplyMetricsConfig(session.Configuration, -1, 7000);

            ConfigurationRuntimeMetricsReporting.StmtGroupMetrics groupOne = new ConfigurationRuntimeMetricsReporting.StmtGroupMetrics();
            groupOne.Interval = 8000;
            groupOne.AddIncludeLike("%GroupOne%");
            groupOne.IsReportInactive = true;
            session.Configuration.Runtime.MetricsReporting.AddStmtGroup("GroupOneStatements", groupOne);

            ConfigurationRuntimeMetricsReporting.StmtGroupMetrics groupTwo = new ConfigurationRuntimeMetricsReporting.StmtGroupMetrics();
            groupTwo.Interval = 6000;
            groupTwo.IsDefaultInclude = true;
            groupTwo.AddExcludeLike("%Default%");
            groupTwo.AddExcludeLike("%Metrics%");
            session.Configuration.Runtime.MetricsReporting.AddStmtGroup("GroupTwoNonDefaultStatements", groupTwo);

            ConfigurationRuntimeMetricsReporting.StmtGroupMetrics groupThree = new ConfigurationRuntimeMetricsReporting.StmtGroupMetrics();
            groupThree.Interval = -1;
            groupThree.AddIncludeLike("%Metrics%");
            session.Configuration.Runtime.MetricsReporting.AddStmtGroup("MetricsStatements", groupThree);

            RegressionRunner.Run(session, new ClientInstrumentMetricsReportingStmtGroups());

            session.Destroy();
        }

        [Test]
        public void TestClientInstrumentMetricsReportingNW()
        {
            RegressionSession session = RegressionRunner.Session();
            ApplyMetricsConfig(session.Configuration, -1, 1000);
            RegressionRunner.Run(session, new ClientInstrumentMetricsReportingNW());
            session.Destroy();
        }

        [Test]
        public void TestClientInstrumentMetricsReportingRuntimeMetrics()
        {
            RegressionSession session = RegressionRunner.Session();
            ApplyMetricsConfig(session.Configuration, 10000, -1);
            RegressionRunner.Run(session, new ClientInstrumentMetricsReportingRuntimeMetrics());
            session.Destroy();
        }

        [Test]
        public void TestClientInstrumentMetricsReportingDisableStatement()
        {
            RegressionSession session = RegressionRunner.Session();
            ApplyMetricsConfig(session.Configuration, -1, 10000);
            ConfigurationRuntimeMetricsReporting.StmtGroupMetrics configOne = new ConfigurationRuntimeMetricsReporting.StmtGroupMetrics();
            configOne.Interval = -1;
            configOne.AddIncludeLike("%@METRIC%");
            session.Configuration.Runtime.MetricsReporting.AddStmtGroup("metrics", configOne);
            RegressionRunner.Run(session, new ClientInstrumentMetricsReportingDisableStatement());
            session.Destroy();
        }

        [Test]
        public void TestClientInstrumentMetricsReportingDisableRuntime()
        {
            RegressionSession session = RegressionRunner.Session();
            ApplyMetricsConfig(session.Configuration, 10000, 10000);
            RegressionRunner.Run(session, new ClientInstrumentMetricsReportingDisableRuntime());
            session.Destroy();
        }

        private static void ApplyMetricsConfig(Configuration configuration, long runtimeMetricInterval, long stmtMetricInterval)
        {
            configuration.Runtime.MetricsReporting.IsEnableMetricsReporting = true;
            configuration.Runtime.MetricsReporting.IsThreading = false;
            configuration.Runtime.MetricsReporting.RuntimeInterval = runtimeMetricInterval;
            configuration.Runtime.MetricsReporting.StatementInterval = stmtMetricInterval;
            configuration.Common.AddImportType(typeof(MyMetricFunctions));
            configuration.Common.AddEventType(typeof(SupportBean));
        }
    }
} // end of namespace