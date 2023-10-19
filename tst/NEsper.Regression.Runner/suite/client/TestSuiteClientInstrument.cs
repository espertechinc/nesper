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
using com.espertech.esper.regressionrun.runner;
using com.espertech.esper.regressionrun.suite.core;

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.client
{
    [TestFixture]
    public class TestSuiteClientInstrument : AbstractTestContainer
    {
        [Test, RunInApplicationDomain]
        public void TestClientInstrumentInstrumentation()
        {
            using (var session = RegressionRunner.Session(Container)) {
                foreach (Type clazz in new Type[] { typeof(SupportBean) }) {
                    session.Configuration.Common.AddEventType(clazz);
                }

                RegressionRunner.Run(session, new ClientInstrumentInstrumentation());
            }
        }

        [Test, RunInApplicationDomain]
        public void TestClientInstrumentAudit()
        {
            using (var session = RegressionRunner.Session(Container)) {
                foreach (Type clazz in new Type[] { typeof(SupportBean), typeof(SupportBean_ST0), typeof(SupportBean_ST1) }) {
                    session.Configuration.Common.AddEventType(clazz);
                }

                session.Configuration.Runtime.Logging.AuditPattern = "[%u] [%d] [%s] [%i] [%c] %m";
                RegressionRunner.Run(session, ClientInstrumentAudit.Executions());
            }
        }

        [Test]
        public void TestClientInstrumentMetricsReportingStmtMetrics()
        {
            using (var session = RegressionRunner.Session(Container, true)) {
#if !NETCORE
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
#endif
            }
        }

        [Test, RunInApplicationDomain]
        public void TestClientInstrumentMetricsReportingStmtGroups()
        {
            using (var session = RegressionRunner.Session(Container)) {
                session.Configuration.Compiler.ByteCode.IsAllowSubscriber =true;

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
            }
        }

        [Test, RunInApplicationDomain]
        public void TestClientInstrumentMetricsReportingNW()
        {
            using (var session = RegressionRunner.Session(Container)) {
                ApplyMetricsConfig(session.Configuration, -1, 1000);
                RegressionRunner.Run(session, new ClientInstrumentMetricsReportingNW());
            }
        }

        [Test]
        public void TestClientInstrumentMetricsReportingRuntimeMetrics()
        {
            using (var session = RegressionRunner.Session(Container)) {
                ApplyMetricsConfig(session.Configuration, 10000, -1);
                RegressionRunner.Run(session, new ClientInstrumentMetricsReportingRuntimeMetrics());
            }
        }

        [Test, RunInApplicationDomain]
        public void TestClientInstrumentMetricsReportingDisableStatement()
        {
            using (var session = RegressionRunner.Session(Container)) {
                ApplyMetricsConfig(session.Configuration, -1, 10000);
                ConfigurationRuntimeMetricsReporting.StmtGroupMetrics configOne = new ConfigurationRuntimeMetricsReporting.StmtGroupMetrics();
                configOne.Interval = -1;
                configOne.AddIncludeLike("%@METRIC%");
                session.Configuration.Runtime.MetricsReporting.AddStmtGroup("metrics", configOne);
                RegressionRunner.Run(session, new ClientInstrumentMetricsReportingDisableStatement());
            }
        }

        [Test, RunInApplicationDomain]
        public void TestClientInstrumentMetricsReportingDisableRuntime()
        {
            using (var session = RegressionRunner.Session(Container)) {
                ApplyMetricsConfig(session.Configuration, 10000, 10000);
                RegressionRunner.Run(session, new ClientInstrumentMetricsReportingDisableRuntime());
            }
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