///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.suite.resultset.outputlimit;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.extend.aggfunc;
using com.espertech.esper.regressionrun.Runner;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionrun.suite.resultset
{
    [TestFixture]
    public class TestSuiteResultSetOutputLimit
    {
        private RegressionSession session;

        [SetUp]
        public void SetUp()
        {
            session = RegressionRunner.Session();
            Configure(session.Configuration);
        }

        [TearDown]
        public void TearDown()
        {
            session.Destroy();
            session = null;
        }

        [Test]
        public void TestResultSetOutputLimitSimple()
        {
            RegressionRunner.Run(session, ResultSetOutputLimitSimple.Executions());
        }

        [Test]
        public void TestResultSetOutputLimitRowForAll()
        {
            RegressionRunner.Run(session, ResultSetOutputLimitRowForAll.Executions());
        }

        [Test]
        public void TestResultSetOutputLimitRowPerEvent()
        {
            RegressionRunner.Run(session, ResultSetOutputLimitRowPerEvent.Executions());
        }

        [Test]
        public void TestResultSetOutputLimitRowPerGroup()
        {
            RegressionRunner.Run(session, ResultSetOutputLimitRowPerGroup.Executions());
        }

        [Test]
        public void TestResultSetOutputLimitAggregateGrouped()
        {
            RegressionRunner.Run(session, ResultSetOutputLimitAggregateGrouped.Executions());
        }

        [Test]
        public void TestResultSetOutputLimitRowPerGroupRollup()
        {
            RegressionRunner.Run(session, ResultSetOutputLimitRowPerGroupRollup.Executions());
        }

        [Test]
        public void TestResultSetOutputLimitRowLimit()
        {
            RegressionRunner.Run(session, ResultSetOutputLimitRowLimit.Executions());
        }

        [Test]
        public void TestResultSetOutputLimitChangeSetOpt()
        {
            RegressionRunner.Run(session, new ResultSetOutputLimitChangeSetOpt(true));
        }

        [Test]
        public void TestResultSetOutputLimitFirstHaving()
        {
            RegressionRunner.Run(session, ResultSetOutputLimitFirstHaving.Executions());
        }

        [Test]
        public void TestResultSetOutputLimitCrontabWhen()
        {
            RegressionRunner.Run(session, ResultSetOutputLimitCrontabWhen.Executions());
        }

        [Test]
        public void TestResultSetOutputLimitMicrosecondResolution()
        {
            RegressionRunner.Run(session, new ResultSetOutputLimitMicrosecondResolution(0, "1", 1000, 1000));
            RegressionRunner.Run(session, new ResultSetOutputLimitMicrosecondResolution(789123456789L, "0.1", 789123456789L + 100, 100));
        }

        [Test]
        public void TestResultSetOutputLimitParameterizedByContext()
        {
            RegressionRunner.Run(session, new ResultSetOutputLimitParameterizedByContext());
        }

        [Test]
        public void TestResultSetOutputLimitAfter()
        {
            RegressionRunner.Run(session, ResultSetOutputLimitAfter.Executions());
        }

        [Test]
        public void TestResultSetOutputLimitInsertInto()
        {
            RegressionRunner.Run(session, ResultSetOutputLimitInsertInto.Executions());
        }

        private static void Configure(Configuration configuration)
        {
            foreach (Type clazz in new Type[]{
                typeof(SupportBean),
                typeof(SupportBean_S0),
                typeof(SupportBean_S1),
                typeof(SupportMarketDataBean),
                typeof(SupportBeanNumeric),
                typeof(SupportBean_ST0),
                typeof(SupportBean_A),
                typeof(SupportScheduleSimpleEvent),
                typeof(SupportBeanString)})
            {
                configuration.Common.AddEventType(clazz);
            }

            ConfigurationCommon common = configuration.Common;
            common.AddVariable("D", typeof(int), 1);
            common.AddVariable("H", typeof(int), 2);
            common.AddVariable("M", typeof(int), 3);
            common.AddVariable("S", typeof(int), 4);
            common.AddVariable("MS", typeof(int), 5);

            common.AddVariable("varoutone", typeof(bool), false);
            common.AddVariable("myint", typeof(int), 0);
            common.AddVariable("mystring", typeof(string), "");
            common.AddVariable("myvar", typeof(int), 0);
            common.AddVariable("count_insert_var", typeof(int), 0);
            common.AddVariable("myvardummy", typeof(int), 0);
            common.AddVariable("myvarlong", typeof(long), 0);

            configuration.Compiler.ByteCode.AllowSubscriber = true;
            configuration.Compiler.AddPlugInAggregationFunctionForge("customagg", typeof(SupportInvocationCountForge));
        }
    }
} // end of namespace