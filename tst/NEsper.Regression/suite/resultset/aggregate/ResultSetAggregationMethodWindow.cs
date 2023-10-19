///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

using static com.espertech.esper.regressionlib.suite.resultset.aggregate.ResultSetAggregationMethodSorted; // assertType
using NUnit.Framework; // assertEquals

namespace com.espertech.esper.regressionlib.suite.resultset.aggregate
{
    public class ResultSetAggregationMethodWindow
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithNonTable(execs);
            WithTableAccess(execs);
            WithTableIdentWCount(execs);
            WithListReference(execs);
            WithInvalid(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateWindowInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithListReference(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateWindowListReference());
            return execs;
        }

        public static IList<RegressionExecution> WithTableIdentWCount(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateWindowTableIdentWCount());
            return execs;
        }

        public static IList<RegressionExecution> WithTableAccess(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateWindowTableAccess());
            return execs;
        }

        public static IList<RegressionExecution> WithNonTable(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateWindowNonTable());
            return execs;
        }

        private class ResultSetAggregateWindowListReference : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create table MyTable(windowcol window(*) @type('SupportBean'));\n" +
                          "into table MyTable select window(*) as windowcol from SupportBean;\n" +
                          "@name('s0') select MyTable.windowcol.listReference() as collref from SupportBean_S0";
                env.CompileDeploy(epl).AddListener("s0");

                AssertType(env, typeof(IList<EventBean>), "collref");

                var sb1 = MakeSendBean(env, "E1", 10);
                var sb2 = MakeSendBean(env, "E1", 10);
                env.SendEventBean(new SupportBean_S0(-1));
                env.AssertEventNew(
                    "s0",
                    @event => {
                        var events = (IList<EventBean>)@event.Get("collref");
                        Assert.AreEqual(2, events.Count);
                        EPAssertionUtil.AssertEqualsExactOrder(
                            new object[] {
                                events[0].Underlying,
                                events[1].Underlying
                            },
                            new object[] { sb1, sb2 });
                    });

                env.Milestone(0);

                env.UndeployAll();
            }
        }

        private class ResultSetAggregateWindowTableIdentWCount : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create table MyTable(windowcol window(*) @type('SupportBean'));\n" +
                          "into table MyTable select window(*) as windowcol from SupportBean;\n" +
                          "@name('s0') select windowcol.first(intPrimitive) as c0, windowcol.last(intPrimitive) as c1, windowcol.countEvents() as c2 from SupportBean_S0, MyTable";
                env.CompileDeploy(epl).AddListener("s0");

                AssertType(env, typeof(int?), "c0,c1,c2");

                MakeSendBean(env, "E1", 10);
                MakeSendBean(env, "E2", 20);
                MakeSendBean(env, "E3", 30);

                env.SendEventBean(new SupportBean_S0(-1));
                env.AssertPropsNew("s0", "c0,c1,c2".SplitCsv(), new object[] { 10, 30, 3 });

                env.Milestone(0);

                env.UndeployAll();
            }
        }

        private class ResultSetAggregateWindowTableAccess : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create table MyTable(windowcol window(*) @type('SupportBean'));\n" +
                          "into table MyTable select window(*) as windowcol from SupportBean#length(2);\n" +
                          "@name('s0') select MyTable.windowcol.first() as c0, MyTable.windowcol.last() as c1 from SupportBean_S0";
                env.CompileDeploy(epl).AddListener("s0");

                AssertType(env, typeof(SupportBean), "c0,c1");

                SendAssert(env, null, null);

                var sb1 = MakeSendBean(env, "E1", 10);
                SendAssert(env, sb1, sb1);

                var sb2 = MakeSendBean(env, "E2", 20);
                SendAssert(env, sb1, sb2);

                var sb3 = MakeSendBean(env, "E3", 0);
                SendAssert(env, sb2, sb3);

                env.Milestone(0);

                env.UndeployAll();
            }

            private void SendAssert(
                RegressionEnvironment env,
                SupportBean first,
                SupportBean last)
            {
                var fields = "c0,c1".SplitCsv();
                env.SendEventBean(new SupportBean_S0(-1));
                env.AssertPropsNew("s0", fields, new object[] { first, last });
            }
        }

        private class ResultSetAggregateWindowNonTable : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "theString,c0,c1".SplitCsv();
                var epl =
                    "@name('s0') select theString, window(*).first() as c0, window(*).last() as c1 from SupportBean#length(3) as sb group by theString";
                env.CompileDeploy(epl).AddListener("s0");

                AssertType(env, typeof(SupportBean), "c0,c1");

                var sb1 = MakeSendBean(env, "A", 1);
                env.AssertPropsNew("s0", fields, new object[] { "A", sb1, sb1 });

                var sb2 = MakeSendBean(env, "A", 2);
                env.AssertPropsNew("s0", fields, new object[] { "A", sb1, sb2 });

                var sb3 = MakeSendBean(env, "A", 3);
                env.AssertPropsNew("s0", fields, new object[] { "A", sb1, sb3 });

                var sb4 = MakeSendBean(env, "A", 4);
                env.AssertPropsNew("s0", fields, new object[] { "A", sb2, sb4 });

                var sb5 = MakeSendBean(env, "B", 5);
                env.AssertPropsPerRowNewOnly(
                    "s0",
                    fields,
                    new object[][] { new object[] { "B", sb5, sb5 }, new object[] { "A", sb3, sb4 } });

                var sb6 = MakeSendBean(env, "A", 6);
                env.AssertPropsNew("s0", fields, new object[] { "A", sb4, sb6 });

                env.Milestone(0);

                env.UndeployAll();
            }
        }

        private class ResultSetAggregateWindowInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public create table MyTable(windowcol window(*) @type('SupportBean'));\n", path);

                env.TryInvalidCompile(
                    path,
                    "select MyTable.windowcol.first(id) from SupportBean_S0",
                    "Failed to validate select-clause expression 'MyTable.windowcol.first(id)': Failed to validate aggregation function parameter expression 'id': Property named 'id' is not valid in any stream");

                env.TryInvalidCompile(
                    path,
                    "select MyTable.windowcol.listReference(intPrimitive) from SupportBean_S0",
                    "Failed to validate select-clause expression 'MyTable.windowcol.listReference(int...(45 chars)': Invalid number of parameters");

                env.UndeployAll();
            }
        }

        private static SupportBean MakeSendBean(
            RegressionEnvironment env,
            string theString,
            int intPrimitive)
        {
            var sb = new SupportBean(theString, intPrimitive);
            env.SendEventBean(sb);
            return sb;
        }
    }
} // end of namespace