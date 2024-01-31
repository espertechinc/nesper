///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.regressionrun.runner;
using com.espertech.esper.regressionrun.suite.core;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionrun.suite.resultset
{
    [TestFixture]
    public class TestSuiteResultSetOutputLimit : AbstractTestBase
    {
        public TestSuiteResultSetOutputLimit() : base(Configure)
        {
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetOutputLimitChangeSetOpt()
        {
            RegressionRunner.Run(_session, new ResultSetOutputLimitChangeSetOpt(true));
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetOutputLimitMicrosecondResolution()
        {
            RegressionRunner.Run(_session, new ResultSetOutputLimitMicrosecondResolution(0, "1", 1000, 1000));
            RegressionRunner.Run(_session, new ResultSetOutputLimitMicrosecondResolution(789123456789L, "0.1", 789123456789L + 100, 100));
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetOutputLimitParameterizedByContext()
        {
            RegressionRunner.Run(_session, new ResultSetOutputLimitParameterizedByContext());
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetOutputLimitAfter()
        {
            RegressionRunner.Run(_session, ResultSetOutputLimitAfter.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetOutputLimitInsertInto()
        {
            RegressionRunner.Run(_session, ResultSetOutputLimitInsertInto.Executions());
        }

        public static void Configure(Configuration configuration)
        {
            foreach (Type clazz in new Type[] {
                         typeof(SupportBean),
                         typeof(SupportBean_S0),
                         typeof(SupportBean_S1),
                         typeof(SupportMarketDataBean),
                         typeof(SupportBeanNumeric),
                         typeof(SupportBean_ST0),
                         typeof(SupportBean_A),
                         typeof(SupportScheduleSimpleEvent),
                         typeof(SupportBeanString),
                         typeof(SupportEventWithIntArray)
                     }) {
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

            configuration.Compiler.ByteCode.IsAllowSubscriber = true;
            configuration.Compiler.AddPlugInAggregationFunctionForge("customagg", typeof(SupportInvocationCountForge));
        }

        /// <summary>
        /// Auto-test(s): ResultSetOutputLimitRowForAll
        /// <code>
        /// RegressionRunner.Run(_session, ResultSetOutputLimitRowForAll.Executions());
        /// </code>
        /// </summary>

        public class TestResultSetOutputLimitRowForAll : AbstractTestBase
        {
            public TestResultSetOutputLimitRowForAll() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithOutputSnapshotGetValue() => RegressionRunner.Run(_session, ResultSetOutputLimitRowForAll.WithOutputSnapshotGetValue());

            [Test, RunInApplicationDomain]
            public void WithLimitSnapshotJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowForAll.WithLimitSnapshotJoin());

            [Test, RunInApplicationDomain]
            public void WithLimitSnapshot() => RegressionRunner.Run(_session, ResultSetOutputLimitRowForAll.WithLimitSnapshot());

            [Test, RunInApplicationDomain]
            public void WithTimeBatchOutputCount() => RegressionRunner.Run(_session, ResultSetOutputLimitRowForAll.WithTimeBatchOutputCount());

            [Test, RunInApplicationDomain]
            public void WithTimeWindowOutputCountLast() => RegressionRunner.Run(_session, ResultSetOutputLimitRowForAll.WithTimeWindowOutputCountLast());

            [Test, RunInApplicationDomain]
            public void WithMaxTimeWindow() => RegressionRunner.Run(_session, ResultSetOutputLimitRowForAll.WithMaxTimeWindow());

            [Test, RunInApplicationDomain]
            public void WithJoinSortWindow() => RegressionRunner.Run(_session, ResultSetOutputLimitRowForAll.WithJoinSortWindow());

            [Test, RunInApplicationDomain]
            public void WithAggAllHavingJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowForAll.WithAggAllHavingJoin());

            [Test, RunInApplicationDomain]
            public void WithAggAllHaving() => RegressionRunner.Run(_session, ResultSetOutputLimitRowForAll.WithAggAllHaving());

            [Test, RunInApplicationDomain]
            public void WithOutputLastWithInsertInto() => RegressionRunner.Run(_session, ResultSetOutputLimitRowForAll.WithOutputLastWithInsertInto());

            [Test, RunInApplicationDomain]
            public void With18SnapshotNoHavingNoJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowForAll.With18SnapshotNoHavingNoJoin());

            [Test, RunInApplicationDomain]
            public void With17FirstNoHavingNoJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowForAll.With17FirstNoHavingNoJoin());

            [Test, RunInApplicationDomain]
            public void With16LastHavingJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowForAll.With16LastHavingJoin());

            [Test, RunInApplicationDomain]
            public void With15LastHavingNoJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowForAll.With15LastHavingNoJoin());

            [Test, RunInApplicationDomain]
            public void With14LastNoHavingJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowForAll.With14LastNoHavingJoin());

            [Test, RunInApplicationDomain]
            public void With13LastNoHavingNoJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowForAll.With13LastNoHavingNoJoin());

            [Test, RunInApplicationDomain]
            public void With12AllHavingJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowForAll.With12AllHavingJoin());

            [Test, RunInApplicationDomain]
            public void With11AllHavingNoJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowForAll.With11AllHavingNoJoin());

            [Test, RunInApplicationDomain]
            public void With10AllNoHavingJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowForAll.With10AllNoHavingJoin());

            [Test, RunInApplicationDomain]
            public void With9AllNoHavingNoJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowForAll.With9AllNoHavingNoJoin());

            [Test, RunInApplicationDomain]
            public void With8DefaultHavingJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowForAll.With8DefaultHavingJoin());

            [Test, RunInApplicationDomain]
            public void With7DefaultHavingNoJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowForAll.With7DefaultHavingNoJoin());

            [Test, RunInApplicationDomain]
            public void With6DefaultNoHavingJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowForAll.With6DefaultNoHavingJoin());

            [Test, RunInApplicationDomain]
            public void With5DefaultNoHavingNoJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowForAll.With5DefaultNoHavingNoJoin());

            [Test, RunInApplicationDomain]
            public void With4NoneHavingJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowForAll.With4NoneHavingJoin());

            [Test, RunInApplicationDomain]
            public void With3NoneHavingNoJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowForAll.With3NoneHavingNoJoin());

            [Test, RunInApplicationDomain]
            public void With2NoneNoHavingJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowForAll.With2NoneNoHavingJoin());

            [Test, RunInApplicationDomain]
            public void With1NoneNoHavingNoJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowForAll.With1NoneNoHavingNoJoin());
        }

        /// <summary>
        /// Auto-test(s): ResultSetOutputLimitRowPerEvent
        /// <code>
        /// RegressionRunner.Run(_session, ResultSetOutputLimitRowPerEvent.Executions());
        /// </code>
        /// </summary>

        public class TestResultSetOutputLimitRowPerEvent : AbstractTestBase
        {
            public TestResultSetOutputLimitRowPerEvent() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithCount() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerEvent.WithCount());

            [Test, RunInApplicationDomain]
            public void WithTime() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerEvent.WithTime());

            [Test, RunInApplicationDomain]
            public void WithRowPerEventJoinLast() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerEvent.WithRowPerEventJoinLast());

            [Test, RunInApplicationDomain]
            public void WithRowPerEventJoinAll() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerEvent.WithRowPerEventJoinAll());

            [Test, RunInApplicationDomain]
            public void WithRowPerEventNoJoinLast() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerEvent.WithRowPerEventNoJoinLast());

            [Test, RunInApplicationDomain]
            public void WithJoinSortWindow() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerEvent.WithJoinSortWindow());

            [Test, RunInApplicationDomain]
            public void WithLimitSnapshotJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerEvent.WithLimitSnapshotJoin());

            [Test, RunInApplicationDomain]
            public void WithLimitSnapshot() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerEvent.WithLimitSnapshot());

            [Test, RunInApplicationDomain]
            public void WithMaxTimeWindow() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerEvent.WithMaxTimeWindow());

            [Test, RunInApplicationDomain]
            public void WithHavingJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerEvent.WithHavingJoin());

            [Test, RunInApplicationDomain]
            public void WithHaving() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerEvent.WithHaving());

            [Test, RunInApplicationDomain]
            public void With18SnapshotNoHavingNoJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerEvent.With18SnapshotNoHavingNoJoin());

            [Test, RunInApplicationDomain]
            public void With17FirstNoHavingNoJoinIRStream() => RegressionRunner.Run(
                _session,
                ResultSetOutputLimitRowPerEvent.With17FirstNoHavingNoJoinIRStream());

            [Test, RunInApplicationDomain]
            public void With17FirstNoHavingNoJoinIStreamOnly() => RegressionRunner.Run(
                _session,
                ResultSetOutputLimitRowPerEvent.With17FirstNoHavingNoJoinIStreamOnly());

            [Test, RunInApplicationDomain]
            public void With16LastHavingJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerEvent.With16LastHavingJoin());

            [Test, RunInApplicationDomain]
            public void With15LastHavingNoJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerEvent.With15LastHavingNoJoin());

            [Test, RunInApplicationDomain]
            public void With14LastNoHavingJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerEvent.With14LastNoHavingJoin());

            [Test, RunInApplicationDomain]
            public void With13LastNoHavingNoJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerEvent.With13LastNoHavingNoJoin());

            [Test, RunInApplicationDomain]
            public void With12AllHavingJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerEvent.With12AllHavingJoin());

            [Test, RunInApplicationDomain]
            public void With11AllHavingNoJoinHinted() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerEvent.With11AllHavingNoJoinHinted());

            [Test, RunInApplicationDomain]
            public void With11AllHavingNoJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerEvent.With11AllHavingNoJoin());

            [Test, RunInApplicationDomain]
            public void With10AllNoHavingJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerEvent.With10AllNoHavingJoin());

            [Test, RunInApplicationDomain]
            public void With9AllNoHavingNoJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerEvent.With9AllNoHavingNoJoin());

            [Test, RunInApplicationDomain]
            public void With8DefaultHavingJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerEvent.With8DefaultHavingJoin());

            [Test, RunInApplicationDomain]
            public void With7DefaultHavingNoJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerEvent.With7DefaultHavingNoJoin());

            [Test, RunInApplicationDomain]
            public void With6DefaultNoHavingJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerEvent.With6DefaultNoHavingJoin());

            [Test, RunInApplicationDomain]
            public void With5DefaultNoHavingNoJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerEvent.With5DefaultNoHavingNoJoin());

            [Test, RunInApplicationDomain]
            public void With4NoneHavingJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerEvent.With4NoneHavingJoin());

            [Test, RunInApplicationDomain]
            public void With3NoneHavingNoJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerEvent.With3NoneHavingNoJoin());

            [Test, RunInApplicationDomain]
            public void With2NoneNoHavingJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerEvent.With2NoneNoHavingJoin());

            [Test, RunInApplicationDomain]
            public void With1NoneNoHavingNoJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerEvent.With1NoneNoHavingNoJoin());
        }

        /// <summary>
        /// Auto-test(s): ResultSetOutputLimitRowPerGroup
        /// <code>
        /// RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroup.Executions());
        /// </code>
        /// </summary>

        public class TestResultSetOutputLimitRowPerGroup : AbstractTestBase
        {
            public TestResultSetOutputLimitRowPerGroup() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithOutputSnapshotMultikeyWArray() =>
                RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroup.WithOutputSnapshotMultikeyWArray());

            [Test, RunInApplicationDomain]
            public void WithOutputLastMultikeyWArray() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroup.WithOutputLastMultikeyWArray());

            [Test, RunInApplicationDomain]
            public void WithOutputAllMultikeyWArray() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroup.WithOutputAllMultikeyWArray());

            [Test, RunInApplicationDomain]
            public void WithOutputFirstMultikeyWArray() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroup.WithOutputFirstMultikeyWArray());

            [Test, RunInApplicationDomain]
            public void WithOutputFirstEveryNEvents() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroup.WithOutputFirstEveryNEvents());

            [Test, RunInApplicationDomain]
            public void WithOutputFirstCrontab() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroup.WithOutputFirstCrontab());

            [Test, RunInApplicationDomain]
            public void WithOutputFirstHavingJoinNoJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroup.WithOutputFirstHavingJoinNoJoin());

            [Test, RunInApplicationDomain]
            public void WithCrontabNumberSetVariations() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroup.WithCrontabNumberSetVariations());

            [Test, RunInApplicationDomain]
            public void WithJoinAll() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroup.WithJoinAll());

            [Test, RunInApplicationDomain]
            public void WithJoinLast() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroup.WithJoinLast());

            [Test, RunInApplicationDomain]
            public void WithNoJoinAll() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroup.WithNoJoinAll());

            [Test, RunInApplicationDomain]
            public void WithNoOutputClauseJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroup.WithNoOutputClauseJoin());

            [Test, RunInApplicationDomain]
            public void WithNoOutputClauseView() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroup.WithNoOutputClauseView());

            [Test, RunInApplicationDomain]
            public void WithNoJoinLast() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroup.WithNoJoinLast());

            [Test, RunInApplicationDomain]
            public void WithMaxTimeWindow() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroup.WithMaxTimeWindow());

            [Test, RunInApplicationDomain]
            public void WithGroupByDefault() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroup.WithGroupByDefault());

            [Test, RunInApplicationDomain]
            public void WithGroupByAll() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroup.WithGroupByAll());

            [Test, RunInApplicationDomain]
            public void WithLimitSnapshotLimit() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroup.WithLimitSnapshotLimit());

            [Test, RunInApplicationDomain]
            public void WithLimitSnapshot() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroup.WithLimitSnapshot());

            [Test, RunInApplicationDomain]
            public void WithJoinSortWindow() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroup.WithJoinSortWindow());

            [Test, RunInApplicationDomain]
            public void With18SnapshotNoHavingJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroup.With18SnapshotNoHavingJoin());

            [Test, RunInApplicationDomain]
            public void With18SnapshotNoHavingNoJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroup.With18SnapshotNoHavingNoJoin());

            [Test, RunInApplicationDomain]
            public void With17FirstNoHavingJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroup.With17FirstNoHavingJoin());

            [Test, RunInApplicationDomain]
            public void With17FirstNoHavingNoJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroup.With17FirstNoHavingNoJoin());

            [Test, RunInApplicationDomain]
            public void With16LastHavingJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroup.With16LastHavingJoin());

            [Test, RunInApplicationDomain]
            public void With15LastHavingNoJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroup.With15LastHavingNoJoin());

            [Test, RunInApplicationDomain]
            public void With14LastNoHavingJoinWOrderBy() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroup.With14LastNoHavingJoinWOrderBy());

            [Test, RunInApplicationDomain]
            public void With13LastNoHavingNoJoinWOrderBy() =>
                RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroup.With13LastNoHavingNoJoinWOrderBy());

            [Test, RunInApplicationDomain]
            public void With14LastNoHavingJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroup.With14LastNoHavingJoin());

            [Test, RunInApplicationDomain]
            public void With13LastNoHavingNoJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroup.With13LastNoHavingNoJoin());

            [Test, RunInApplicationDomain]
            public void With12AllHavingJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroup.With12AllHavingJoin());

            [Test, RunInApplicationDomain]
            public void With11AllHavingNoJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroup.With11AllHavingNoJoin());

            [Test, RunInApplicationDomain]
            public void With10AllNoHavingJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroup.With10AllNoHavingJoin());

            [Test, RunInApplicationDomain]
            public void With9AllNoHavingNoJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroup.With9AllNoHavingNoJoin());

            [Test, RunInApplicationDomain]
            public void With8DefaultHavingJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroup.With8DefaultHavingJoin());

            [Test, RunInApplicationDomain]
            public void With7DefaultHavingNoJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroup.With7DefaultHavingNoJoin());

            [Test, RunInApplicationDomain]
            public void With6DefaultNoHavingJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroup.With6DefaultNoHavingJoin());

            [Test, RunInApplicationDomain]
            public void With5DefaultNoHavingNoJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroup.With5DefaultNoHavingNoJoin());

            [Test, RunInApplicationDomain]
            public void With4NoneHavingJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroup.With4NoneHavingJoin());

            [Test, RunInApplicationDomain]
            public void With3NoneHavingNoJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroup.With3NoneHavingNoJoin());

            [Test, RunInApplicationDomain]
            public void With2NoneNoHavingJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroup.With2NoneNoHavingJoin());

            [Test, RunInApplicationDomain]
            public void With1NoneNoHavingNoJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroup.With1NoneNoHavingNoJoin());

            [Test, RunInApplicationDomain]
            public void WithOutputFirstWhenThen() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroup.WithOutputFirstWhenThen());
        }

        /// <summary>
        /// Auto-test(s): ResultSetOutputLimitAggregateGrouped
        /// <code>
        /// RegressionRunner.Run(_session, ResultSetOutputLimitAggregateGrouped.Executions());
        /// </code>
        /// </summary>

        public class TestResultSetOutputLimitAggregateGrouped : AbstractTestBase
        {
            public TestResultSetOutputLimitAggregateGrouped() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithOutputFirstHavingJoinNoJoin() => RegressionRunner.Run(
                _session,
                ResultSetOutputLimitAggregateGrouped.WithOutputFirstHavingJoinNoJoin());

            [Test, RunInApplicationDomain]
            public void WithJoinLast() => RegressionRunner.Run(_session, ResultSetOutputLimitAggregateGrouped.WithJoinLast());

            [Test, RunInApplicationDomain]
            public void WithJoinAll() => RegressionRunner.Run(_session, ResultSetOutputLimitAggregateGrouped.WithJoinAll());

            [Test, RunInApplicationDomain]
            public void WithNoJoinAll() => RegressionRunner.Run(_session, ResultSetOutputLimitAggregateGrouped.WithNoJoinAll());

            [Test, RunInApplicationDomain]
            public void WithJoinDefault() => RegressionRunner.Run(_session, ResultSetOutputLimitAggregateGrouped.WithJoinDefault());

            [Test, RunInApplicationDomain]
            public void WithNoJoinDefault() => RegressionRunner.Run(_session, ResultSetOutputLimitAggregateGrouped.WithNoJoinDefault());

            [Test, RunInApplicationDomain]
            public void WithNoOutputClauseView() => RegressionRunner.Run(_session, ResultSetOutputLimitAggregateGrouped.WithNoOutputClauseView());

            [Test, RunInApplicationDomain]
            public void WithNoJoinLast() => RegressionRunner.Run(_session, ResultSetOutputLimitAggregateGrouped.WithNoJoinLast());

            [Test, RunInApplicationDomain]
            public void WithMaxTimeWindow() => RegressionRunner.Run(_session, ResultSetOutputLimitAggregateGrouped.WithMaxTimeWindow());

            [Test, RunInApplicationDomain]
            public void WithLimitSnapshotJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitAggregateGrouped.WithLimitSnapshotJoin());

            [Test, RunInApplicationDomain]
            public void WithLimitSnapshot() => RegressionRunner.Run(_session, ResultSetOutputLimitAggregateGrouped.WithLimitSnapshot());

            [Test, RunInApplicationDomain]
            public void WithJoinSortWindow() => RegressionRunner.Run(_session, ResultSetOutputLimitAggregateGrouped.WithJoinSortWindow());

            [Test, RunInApplicationDomain]
            public void WithHavingJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitAggregateGrouped.WithHavingJoin());

            [Test, RunInApplicationDomain]
            public void WithHaving() => RegressionRunner.Run(_session, ResultSetOutputLimitAggregateGrouped.WithHaving());

            [Test, RunInApplicationDomain]
            public void With18SnapshotNoHavingNoJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitAggregateGrouped.With18SnapshotNoHavingNoJoin());

            [Test, RunInApplicationDomain]
            public void With17FirstNoHavingJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitAggregateGrouped.With17FirstNoHavingJoin());

            [Test, RunInApplicationDomain]
            public void With17FirstNoHavingNoJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitAggregateGrouped.With17FirstNoHavingNoJoin());

            [Test, RunInApplicationDomain]
            public void With16LastHavingJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitAggregateGrouped.With16LastHavingJoin());

            [Test, RunInApplicationDomain]
            public void With15LastHavingNoJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitAggregateGrouped.With15LastHavingNoJoin());

            [Test, RunInApplicationDomain]
            public void With14LastNoHavingJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitAggregateGrouped.With14LastNoHavingJoin());

            [Test, RunInApplicationDomain]
            public void With13LastNoHavingNoJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitAggregateGrouped.With13LastNoHavingNoJoin());

            [Test, RunInApplicationDomain]
            public void With12AllHavingJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitAggregateGrouped.With12AllHavingJoin());

            [Test, RunInApplicationDomain]
            public void With11AllHavingNoJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitAggregateGrouped.With11AllHavingNoJoin());

            [Test, RunInApplicationDomain]
            public void With10AllNoHavingJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitAggregateGrouped.With10AllNoHavingJoin());

            [Test, RunInApplicationDomain]
            public void With9AllNoHavingNoJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitAggregateGrouped.With9AllNoHavingNoJoin());

            [Test, RunInApplicationDomain]
            public void With8DefaultHavingJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitAggregateGrouped.With8DefaultHavingJoin());

            [Test, RunInApplicationDomain]
            public void With7DefaultHavingNoJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitAggregateGrouped.With7DefaultHavingNoJoin());

            [Test, RunInApplicationDomain]
            public void With6DefaultNoHavingJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitAggregateGrouped.With6DefaultNoHavingJoin());

            [Test, RunInApplicationDomain]
            public void With5DefaultNoHavingNoJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitAggregateGrouped.With5DefaultNoHavingNoJoin());

            [Test, RunInApplicationDomain]
            public void With4NoneHavingJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitAggregateGrouped.With4NoneHavingJoin());

            [Test, RunInApplicationDomain]
            public void With3NoneHavingNoJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitAggregateGrouped.With3NoneHavingNoJoin());

            [Test, RunInApplicationDomain]
            public void With2NoneNoHavingJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitAggregateGrouped.With2NoneNoHavingJoin());

            [Test, RunInApplicationDomain]
            public void With1NoneNoHavingNoJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitAggregateGrouped.With1NoneNoHavingNoJoin());

            [Test, RunInApplicationDomain]
            public void WithUnaggregatedOutputFirst() => RegressionRunner.Run(_session, ResultSetOutputLimitAggregateGrouped.WithUnaggregatedOutputFirst());
        }

        /// <summary>
        /// Auto-test(s): ResultSetOutputLimitRowPerGroupRollup
        /// <code>
        /// RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroupRollup.Executions());
        /// </code>
        /// </summary>

        public class TestResultSetOutputLimitRowPerGroupRollup : AbstractTestBase
        {
            public TestResultSetOutputLimitRowPerGroupRollup() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithOutputSnapshotOrderWLimit() =>
                RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroupRollup.WithOutputSnapshotOrderWLimit());

            [Test, RunInApplicationDomain]
            public void With6OutputLimitSnapshot() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroupRollup.With6OutputLimitSnapshot());

            [Test, RunInApplicationDomain]
            public void With5OutputLimitFirst() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroupRollup.With5OutputLimitFirst());

            [Test, RunInApplicationDomain]
            public void With2OutputLimitDefault() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroupRollup.With2OutputLimitDefault());

            [Test, RunInApplicationDomain]
            public void With1NoOutputLimit() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroupRollup.With1NoOutputLimit());

            [Test, RunInApplicationDomain]
            public void With4OutputLimitLast() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroupRollup.With4OutputLimitLast());

            [Test, RunInApplicationDomain]
            public void With3OutputLimitAll() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroupRollup.With3OutputLimitAll());

            [Test, RunInApplicationDomain]
            public void WithOutputFirst() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroupRollup.WithOutputFirst());

            [Test, RunInApplicationDomain]
            public void WithOutputFirstSorted() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroupRollup.WithOutputFirstSorted());

            [Test, RunInApplicationDomain]
            public void WithOutputFirstHaving() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroupRollup.WithOutputFirstHaving());

            [Test, RunInApplicationDomain]
            public void WithOutputDefaultSorted() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroupRollup.WithOutputDefaultSorted());

            [Test, RunInApplicationDomain]
            public void WithOutputDefault() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroupRollup.WithOutputDefault());

            [Test, RunInApplicationDomain]
            public void WithOutputAllSorted() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroupRollup.WithOutputAllSorted());

            [Test, RunInApplicationDomain]
            public void WithOutputAll() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroupRollup.WithOutputAll());

            [Test, RunInApplicationDomain]
            public void WithOutputLastSorted() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroupRollup.WithOutputLastSorted());

            [Test, RunInApplicationDomain]
            public void WithOutputLast() => RegressionRunner.Run(_session, ResultSetOutputLimitRowPerGroupRollup.WithOutputLast());
        }

        /// <summary>
        /// Auto-test(s): ResultSetOutputLimitRowLimit
        /// <code>
        /// RegressionRunner.Run(_session, ResultSetOutputLimitRowLimit.Executions());
        /// </code>
        /// </summary>

        public class TestResultSetOutputLimitRowLimit : AbstractTestBase
        {
            public TestResultSetOutputLimitRowLimit() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithLengthOffsetVariable() => RegressionRunner.Run(_session, ResultSetOutputLimitRowLimit.WithLengthOffsetVariable());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, ResultSetOutputLimitRowLimit.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithGroupedSnapshotNegativeRowcount() => RegressionRunner.Run(
                _session,
                ResultSetOutputLimitRowLimit.WithGroupedSnapshotNegativeRowcount());

            [Test, RunInApplicationDomain]
            public void WithGroupedSnapshot() => RegressionRunner.Run(_session, ResultSetOutputLimitRowLimit.WithGroupedSnapshot());

            [Test, RunInApplicationDomain]
            public void WithEventPerRowUnGrouped() => RegressionRunner.Run(_session, ResultSetOutputLimitRowLimit.WithEventPerRowUnGrouped());

            [Test, RunInApplicationDomain]
            public void WithFullyGroupedOrdered() => RegressionRunner.Run(_session, ResultSetOutputLimitRowLimit.WithFullyGroupedOrdered());

            [Test, RunInApplicationDomain]
            public void WithBatchOffsetNoOrderOM() => RegressionRunner.Run(_session, ResultSetOutputLimitRowLimit.WithBatchOffsetNoOrderOM());

            [Test, RunInApplicationDomain]
            public void WithOrderBy() => RegressionRunner.Run(_session, ResultSetOutputLimitRowLimit.WithOrderBy());

            [Test, RunInApplicationDomain]
            public void WithBatchNoOffsetNoOrder() => RegressionRunner.Run(_session, ResultSetOutputLimitRowLimit.WithBatchNoOffsetNoOrder());

            [Test, RunInApplicationDomain]
            public void WithLimitOneWithOrderOptimization() => RegressionRunner.Run(_session, ResultSetOutputLimitRowLimit.WithLimitOneWithOrderOptimization());
        }

        /// <summary>
        /// Auto-test(s): ResultSetOutputLimitSimple
        /// <code>
        /// RegressionRunner.Run(_session, ResultSetOutputLimitSimple.Executions());
        /// </code>
        /// </summary>

        public class TestResultSetOutputLimitSimple : AbstractTestBase
        {
            public TestResultSetOutputLimitSimple() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithOutputFirstUnidirectionalJoinNamedWindow() => RegressionRunner.Run(
                _session,
                ResultSetOutputLimitSimple.WithOutputFirstUnidirectionalJoinNamedWindow());

            [Test, RunInApplicationDomain]
            public void WithFirstMonthScoped() => RegressionRunner.Run(_session, ResultSetOutputLimitSimple.WithFirstMonthScoped());

            [Test, RunInApplicationDomain]
            public void WithSnapshotMonthScoped() => RegressionRunner.Run(_session, ResultSetOutputLimitSimple.WithSnapshotMonthScoped());

            [Test, RunInApplicationDomain]
            public void WithLimitSnapshotJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitSimple.WithLimitSnapshotJoin());

            [Test, RunInApplicationDomain]
            public void WithFirstSimpleHavingAndNoHaving() => RegressionRunner.Run(_session, ResultSetOutputLimitSimple.WithFirstSimpleHavingAndNoHaving());

            [Test, RunInApplicationDomain]
            public void WithLimitSnapshot() => RegressionRunner.Run(_session, ResultSetOutputLimitSimple.WithLimitSnapshot());

            [Test, RunInApplicationDomain]
            public void WithLimitEventSimple() => RegressionRunner.Run(_session, ResultSetOutputLimitSimple.WithLimitEventSimple());

            [Test, RunInApplicationDomain]
            public void WithSimpleJoinLast() => RegressionRunner.Run(_session, ResultSetOutputLimitSimple.WithSimpleJoinLast());

            [Test, RunInApplicationDomain]
            public void WithSimpleJoinAll() => RegressionRunner.Run(_session, ResultSetOutputLimitSimple.WithSimpleJoinAll());

            [Test, RunInApplicationDomain]
            public void WithSimpleNoJoinLast() => RegressionRunner.Run(_session, ResultSetOutputLimitSimple.WithSimpleNoJoinLast());

            [Test, RunInApplicationDomain]
            public void WithSimpleNoJoinAll() => RegressionRunner.Run(_session, ResultSetOutputLimitSimple.WithSimpleNoJoinAll());

            [Test, RunInApplicationDomain]
            public void WithTimeBatchOutputEvents() => RegressionRunner.Run(_session, ResultSetOutputLimitSimple.WithTimeBatchOutputEvents());

            [Test, RunInApplicationDomain]
            public void WithLimitTime() => RegressionRunner.Run(_session, ResultSetOutputLimitSimple.WithLimitTime());

            [Test, RunInApplicationDomain]
            public void WithLimitEventJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitSimple.WithLimitEventJoin());

            [Test, RunInApplicationDomain]
            public void WithIterator() => RegressionRunner.Run(_session, ResultSetOutputLimitSimple.WithIterator());

            [Test, RunInApplicationDomain]
            public void WithAggAllHavingJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitSimple.WithAggAllHavingJoin());

            [Test, RunInApplicationDomain]
            public void WithAggAllHaving() => RegressionRunner.Run(_session, ResultSetOutputLimitSimple.WithAggAllHaving());

            [Test, RunInApplicationDomain]
            public void WithOutputEveryTimePeriodVariable() => RegressionRunner.Run(_session, ResultSetOutputLimitSimple.WithOutputEveryTimePeriodVariable());

            [Test, RunInApplicationDomain]
            public void WithOutputEveryTimePeriod() => RegressionRunner.Run(_session, ResultSetOutputLimitSimple.WithOutputEveryTimePeriod());

            [Test, RunInApplicationDomain]
            public void With18SnapshotNoHavingNoJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitSimple.With18SnapshotNoHavingNoJoin());

            [Test, RunInApplicationDomain]
            public void With17FirstNoHavingJoinIRStream() => RegressionRunner.Run(_session, ResultSetOutputLimitSimple.With17FirstNoHavingJoinIRStream());

            [Test, RunInApplicationDomain]
            public void With17FirstNoHavingNoJoinIRStream() => RegressionRunner.Run(_session, ResultSetOutputLimitSimple.With17FirstNoHavingNoJoinIRStream());

            [Test, RunInApplicationDomain]
            public void With17FirstNoHavingJoinIStream() => RegressionRunner.Run(_session, ResultSetOutputLimitSimple.With17FirstNoHavingJoinIStream());

            [Test, RunInApplicationDomain]
            public void With17FirstNoHavingNoJoinIStream() => RegressionRunner.Run(_session, ResultSetOutputLimitSimple.With17FirstNoHavingNoJoinIStream());

            [Test, RunInApplicationDomain]
            public void With16LastHavingJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitSimple.With16LastHavingJoin());

            [Test, RunInApplicationDomain]
            public void With15LastHavingNoJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitSimple.With15LastHavingNoJoin());

            [Test, RunInApplicationDomain]
            public void With14LastNoHavingJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitSimple.With14LastNoHavingJoin());

            [Test, RunInApplicationDomain]
            public void With13LastNoHavingNoJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitSimple.With13LastNoHavingNoJoin());

            [Test, RunInApplicationDomain]
            public void With12AllHavingJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitSimple.With12AllHavingJoin());

            [Test, RunInApplicationDomain]
            public void With11AllHavingNoJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitSimple.With11AllHavingNoJoin());

            [Test, RunInApplicationDomain]
            public void With10AllNoHavingJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitSimple.With10AllNoHavingJoin());

            [Test, RunInApplicationDomain]
            public void With9AllNoHavingNoJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitSimple.With9AllNoHavingNoJoin());

            [Test, RunInApplicationDomain]
            public void With8DefaultHavingJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitSimple.With8DefaultHavingJoin());

            [Test, RunInApplicationDomain]
            public void With7DefaultHavingNoJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitSimple.With7DefaultHavingNoJoin());

            [Test, RunInApplicationDomain]
            public void With6DefaultNoHavingJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitSimple.With6DefaultNoHavingJoin());

            [Test, RunInApplicationDomain]
            public void With5DefaultNoHavingNoJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitSimple.With5DefaultNoHavingNoJoin());

            [Test, RunInApplicationDomain]
            public void With4NoneHavingJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitSimple.With4NoneHavingJoin());

            [Test, RunInApplicationDomain]
            public void With3NoneHavingNoJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitSimple.With3NoneHavingNoJoin());

            [Test, RunInApplicationDomain]
            public void With2NoneNoHavingJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitSimple.With2NoneNoHavingJoin());

            [Test, RunInApplicationDomain]
            public void With1NoneNoHavingNoJoin() => RegressionRunner.Run(_session, ResultSetOutputLimitSimple.With1NoneNoHavingNoJoin());
        }

        /// <summary>
        /// Auto-test(s): ResultSetOutputLimitFirstHaving
        /// <code>
        /// RegressionRunner.Run(_session, ResultSetOutputLimitFirstHaving.Executions());
        /// </code>
        /// </summary>

        public class TestResultSetOutputLimitFirstHaving : AbstractTestBase
        {
            public TestResultSetOutputLimitFirstHaving() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithAvgOutputFirstEveryTwoMinutes() => RegressionRunner.Run(
                _session,
                ResultSetOutputLimitFirstHaving.WithAvgOutputFirstEveryTwoMinutes());

            [Test, RunInApplicationDomain]
            public void WithNoAvgOutputFirstMinutes() => RegressionRunner.Run(_session, ResultSetOutputLimitFirstHaving.WithNoAvgOutputFirstMinutes());

            [Test, RunInApplicationDomain]
            public void WithNoAvgOutputFirstEvents() => RegressionRunner.Run(_session, ResultSetOutputLimitFirstHaving.WithNoAvgOutputFirstEvents());
        }

        /// <summary>
        /// Auto-test(s): ResultSetOutputLimitCrontabWhen
        /// <code>
        /// RegressionRunner.Run(_session, ResultSetOutputLimitCrontabWhen.Executions());
        /// </code>
        /// </summary>

        public class TestResultSetOutputLimitCrontabWhen : AbstractTestBase
        {
            public TestResultSetOutputLimitCrontabWhen() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, ResultSetOutputLimitCrontabWhen.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithOutputWhenThenWCount() => RegressionRunner.Run(_session, ResultSetOutputLimitCrontabWhen.WithOutputWhenThenWCount());

            [Test, RunInApplicationDomain]
            public void WithOutputWhenThenWVariable() => RegressionRunner.Run(_session, ResultSetOutputLimitCrontabWhen.WithOutputWhenThenWVariable());

            [Test, RunInApplicationDomain]
            public void WithOutputWhenThenSameVarTwice() => RegressionRunner.Run(_session, ResultSetOutputLimitCrontabWhen.WithOutputWhenThenSameVarTwice());

            [Test, RunInApplicationDomain]
            public void WithOutputWhenThenExpressionSODA() =>
                RegressionRunner.Run(_session, ResultSetOutputLimitCrontabWhen.WithOutputWhenThenExpressionSODA());

            [Test, RunInApplicationDomain]
            public void WithOutputWhenThenExpression() => RegressionRunner.Run(_session, ResultSetOutputLimitCrontabWhen.WithOutputWhenThenExpression());

            [Test, RunInApplicationDomain]
            public void WithOutputWhenExpression() => RegressionRunner.Run(_session, ResultSetOutputLimitCrontabWhen.WithOutputWhenExpression());

            [Test, RunInApplicationDomain]
            public void WithOutputCrontabAtVariable() => RegressionRunner.Run(_session, ResultSetOutputLimitCrontabWhen.WithOutputCrontabAtVariable());

            [Test, RunInApplicationDomain]
            public void WithOutputWhenBuiltInLastTimestamp() => RegressionRunner.Run(
                _session,
                ResultSetOutputLimitCrontabWhen.WithOutputWhenBuiltInLastTimestamp());

            [Test, RunInApplicationDomain]
            public void WithOutputWhenBuiltInCountRemove() =>
                RegressionRunner.Run(_session, ResultSetOutputLimitCrontabWhen.WithOutputWhenBuiltInCountRemove());

            [Test, RunInApplicationDomain]
            public void WithOutputWhenBuiltInCountInsert() =>
                RegressionRunner.Run(_session, ResultSetOutputLimitCrontabWhen.WithOutputWhenBuiltInCountInsert());

            [Test, RunInApplicationDomain]
            public void WithOutputCrontabAtOMCompile() => RegressionRunner.Run(_session, ResultSetOutputLimitCrontabWhen.WithOutputCrontabAtOMCompile());

            [Test, RunInApplicationDomain]
            public void WithOutputCrontabAtOMCreate() => RegressionRunner.Run(_session, ResultSetOutputLimitCrontabWhen.WithOutputCrontabAtOMCreate());

            [Test, RunInApplicationDomain]
            public void WithOutputCrontabAt() => RegressionRunner.Run(_session, ResultSetOutputLimitCrontabWhen.WithOutputCrontabAt());
        }
    }
} // end of namespace