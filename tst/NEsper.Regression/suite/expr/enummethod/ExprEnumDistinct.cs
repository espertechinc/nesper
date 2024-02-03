///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.expreval;
using com.espertech.esper.regressionlib.support.util;

using static com.espertech.esper.regressionlib.support.util.LambdaAssertionUtil;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.expr.enummethod
{
    public class ExprEnumDistinct
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithEvents(execs);
            WithScalar(execs);
            WithEventsMultikeyWArray(execs);
            WithScalarMultikeyWArray(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithScalarMultikeyWArray(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumDistinctScalarMultikeyWArray());
            return execs;
        }

        public static IList<RegressionExecution> WithEventsMultikeyWArray(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumDistinctEventsMultikeyWArray());
            return execs;
        }

        public static IList<RegressionExecution> WithScalar(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumDistinctScalar());
            return execs;
        }

        public static IList<RegressionExecution> WithEvents(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumDistinctEvents());
            return execs;
        }

        private class ExprEnumDistinctScalarMultikeyWArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select IntArrayCollection.distinctOf() as c0, IntArrayCollection.distinctOf(v => v) as c1 from SupportEventWithManyArray";
                env.CompileDeploy(epl).AddListener("s0");

                ICollection<int[]> coll = new List<int[]>();
                coll.Add(new int[] { 1, 2 });
                coll.Add(new int[] { 2 });
                coll.Add(new int[] { 1, 2 });
                coll.Add(new int[] { 2 });
                var @event = new SupportEventWithManyArray().WithIntArrayCollection(coll);
                env.SendEventBean(@event);
                env.AssertEventNew(
                    "s0",
                    received => {
                        AssertField(received, "c0");
                        AssertField(received, "c1");
                    });

                env.UndeployAll();
            }

            private void AssertField(
                EventBean received,
                string field)
            {
                EPAssertionUtil.AssertEqualsExactOrder(
                    new object[] {
                        new int[] { 1, 2 },
                        new int[] { 2 }
                    },
                    received.Get(field).Unwrap<object>());
            }
        }

        private class ExprEnumDistinctEventsMultikeyWArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplFragment =
                    "@name('s0') select (select * from SupportEventWithManyArray#keepall).distinctOf(r => r.IntOne) as c0 " +
                    " from SupportBean";
                env.CompileDeploy(eplFragment).AddListener("s0");

                SendManyArray(env, "E1", new int[] { 1, 2 });
                SendManyArray(env, "E2", new int[] { 2 });
                SendManyArray(env, "E3", new int[] { 1, 2 });
                SendManyArray(env, "E4", new int[] { 2 });

                env.SendEventBean(new SupportBean("SB1", 0));
                env.AssertEventNew(
                    "s0",
                    @event => {
                        var collection = (ICollection<SupportEventWithManyArray>)@event.Get("c0");
                        ClassicAssert.AreEqual(2, collection.Count);
                    });

                env.UndeployAll();
            }

            private void SendManyArray(
                RegressionEnvironment env,
                string id,
                int[] ints)
            {
                env.SendEventBean(new SupportEventWithManyArray(id).WithIntOne(ints));
            }
        }

        private class ExprEnumDistinctEvents : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2,c3".SplitCsv();
                var builder = new SupportEvalBuilder("SupportBean_ST0_Container");
                builder.WithExpression(fields[0], "Contained.distinctOf(x => P00)");
                builder.WithExpression(
                    fields[1],
                    "Contained.distinctOf( (x, i) => case when i<2 then P00 else -1*P00 end)");
                builder.WithExpression(
                    fields[2],
                    "Contained.distinctOf( (x, i, s) => case when s<=2 then P00 else 0 end)");
                builder.WithExpression(fields[3], "Contained.distinctOf(x => null)");

                builder.WithStatementConsumer(
                    stmt =>
                        SupportEventPropUtil.AssertTypesAllSame(
                            stmt.EventType,
                            fields,
                            typeof(ICollection<SupportBean_ST0>)));

                builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,1", "E2,2", "E3,1"))
                    .Verify("c0", val => AssertST0Id(val, "E1,E2"))
                    .Verify("c1", val => AssertST0Id(val, "E1,E2,E3"))
                    .Verify("c2", val => AssertST0Id(val, "E1"))
                    .Verify("c3", val => AssertST0Id(val, "E1"));

                builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E3,1", "E2,2", "E4,1", "E1,2"))
                    .Verify("c0", val => AssertST0Id(val, "E3,E2"))
                    .Verify("c1", val => AssertST0Id(val, "E3,E2,E4,E1"))
                    .Verify("c2", val => AssertST0Id(val, "E3"))
                    .Verify("c3", val => AssertST0Id(val, "E3"));

                builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E3,1", "E2,2"))
                    .Verify("c0", val => AssertST0Id(val, "E3,E2"))
                    .Verify("c1", val => AssertST0Id(val, "E3,E2"))
                    .Verify("c2", val => AssertST0Id(val, "E3,E2"))
                    .Verify("c3", val => AssertST0Id(val, "E3"));

                builder.WithAssertion(SupportBean_ST0_Container.Make2ValueNull())
                    .Verify("c0", val => AssertST0Id(val, null))
                    .Verify("c1", val => AssertST0Id(val, null))
                    .Verify("c2", val => AssertST0Id(val, null))
                    .Verify("c3", val => AssertST0Id(val, null));

                builder.WithAssertion(SupportBean_ST0_Container.Make2Value())
                    .Verify("c0", val => AssertST0Id(val, ""))
                    .Verify("c1", val => AssertST0Id(val, ""))
                    .Verify("c2", val => AssertST0Id(val, ""))
                    .Verify("c3", val => AssertST0Id(val, ""));

                builder.Run(env);
            }
        }

        private class ExprEnumDistinctScalar : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2,c3,c4".SplitCsv();
                var builder = new SupportEvalBuilder("SupportCollection");
                builder.WithExpression(fields[0], "Strvals.distinctOf()");
                builder.WithExpression(fields[1], "Strvals.distinctOf(v => extractNum(v))");
                builder.WithExpression(
                    fields[2],
                    "Strvals.distinctOf((v, i) => case when i<2 then extractNum(v) else 0 end)");
                builder.WithExpression(
                    fields[3],
                    "Strvals.distinctOf((v, i, s) => case when s<=2 then extractNum(v) else 0 end)");
                builder.WithExpression(fields[4], "Strvals.distinctOf(v => null)");

                builder.WithStatementConsumer(
                    stmt => SupportEventPropUtil.AssertTypesAllSame(
                        stmt.EventType,
                        fields,
                        typeof(ICollection<string>)));

                builder.WithAssertion(SupportCollection.MakeString("E2,E1,E2,E2"))
                    .Verify("c0", val => AssertValuesArrayScalar(val, "E2", "E1"))
                    .Verify("c1", val => AssertValuesArrayScalar(val, "E2", "E1"))
                    .Verify("c2", val => AssertValuesArrayScalar(val, "E2", "E1", "E2"))
                    .Verify("c3", val => AssertValuesArrayScalar(val, "E2"))
                    .Verify("c4", val => AssertValuesArrayScalar(val, "E2"));

                LambdaAssertionUtil.AssertSingleAndEmptySupportColl(builder, fields);

                builder.Run(env);
            }
        }
    }
} // end of namespace