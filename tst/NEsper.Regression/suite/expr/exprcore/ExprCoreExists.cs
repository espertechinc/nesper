///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.expreval;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;
using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;
using SupportMarkerInterface = com.espertech.esper.regressionlib.support.bean.SupportMarkerInterface; // assertEquals

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
    public class ExprCoreExists
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithExistsSimple(execs);
            WithExistsInner(execs);
            WithCastDoubleAndNullOM(execs);
            WithCastStringAndNullCompile(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithCastStringAndNullCompile(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreCastStringAndNullCompile());
            return execs;
        }

        public static IList<RegressionExecution> WithCastDoubleAndNullOM(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreCastDoubleAndNullOM());
            return execs;
        }

        public static IList<RegressionExecution> WithExistsInner(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreExistsInner());
            return execs;
        }

        public static IList<RegressionExecution> WithExistsSimple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreExistsSimple());
            return execs;
        }

        private class ExprCoreExistsSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2,c3,c4".SplitCsv();
                var builder = new SupportEvalBuilder("SupportBean")
                    .WithExpressions(
                        fields,
                        "exists(TheString)",
                        "exists(IntBoxed?)",
                        "exists(dummy?)",
                        "exists(IntPrimitive?)",
                        "exists(IntPrimitive)");
                builder.WithStatementConsumer(
                    stmt => {
                        for (var i = 0; i < 5; i++) {
                            Assert.AreEqual(typeof(bool?), stmt.EventType.GetPropertyType("c" + i));
                        }
                    });

                var bean = new SupportBean("abc", 100);
                bean.FloatBoxed = 9.5f;
                bean.IntBoxed = 3;
                builder.WithAssertion(bean).Expect(fields, true, true, false, true, true);

                builder.Run(env);
                env.UndeployAll();
            }
        }

        private class ExprCoreExistsInner : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select exists(Item?.Id) as t0, " +
                          " exists(Item?.Id?) as t1, " +
                          " exists(Item?.Item.IntBoxed) as t2, " +
                          " exists(Item?.indexed[0]?) as t3, " +
                          " exists(Item?.mapped('keyOne')?) as t4, " +
                          " exists(Item?.Nested?) as t5, " +
                          " exists(Item?.Nested.NestedValue?) as t6, " +
                          " exists(Item?.Nested.NestedNested?) as t7, " +
                          " exists(Item?.Nested.NestedNested.NestedNestedValue?) as t8, " +
                          " exists(Item?.Nested.NestedNested.NestedNestedValue.dummy?) as t9, " +
                          " exists(Item?.Nested.NestedNested.dummy?) as t10 " +
                          " from SupportMarkerInterface";
                env.CompileDeploy(epl).AddListener("s0");

                env.AssertStatement(
                    "s0",
                    statement => {
                        for (var i = 0; i < 11; i++) {
                            Assert.AreEqual(typeof(bool?), statement.EventType.GetPropertyType("t" + i));
                        }
                    });

                // cannot exists if the inner is null
                env.SendEventBean(new SupportBeanDynRoot(null));
                env.AssertEventNew(
                    "s0",
                    @event => AssertResults(
                        @event,
                        new bool[] { false, false, false, false, false, false, false, false, false, false, false }));

                // try nested, indexed and mapped
                env.SendEventBean(new SupportBeanDynRoot(SupportBeanComplexProps.MakeDefaultBean()));
                env.AssertEventNew(
                    "s0",
                    @event => AssertResults(
                        @event,
                        new bool[] { false, false, false, true, true, true, true, true, true, false, false }));

                // try nested, indexed and mapped
                env.SendEventBean(new SupportBeanDynRoot(SupportBeanComplexProps.MakeDefaultBean()));
                env.AssertEventNew(
                    "s0",
                    @event => AssertResults(
                        @event,
                        new bool[] { false, false, false, true, true, true, true, true, true, false, false }));

                // try a boxed that returns null but does exists
                env.SendEventBean(new SupportBeanDynRoot(new SupportBeanDynRoot(new SupportBean())));
                env.AssertEventNew(
                    "s0",
                    @event => AssertResults(
                        @event,
                        new bool[] { false, false, true, false, false, false, false, false, false, false, false }));

                env.SendEventBean(new SupportBeanDynRoot(new SupportBean_A("10")));
                env.AssertEventNew(
                    "s0",
                    @event => AssertResults(
                        @event,
                        new bool[] { true, true, false, false, false, false, false, false, false, false, false }));

                env.UndeployAll();
            }
        }

        private class ExprCoreCastDoubleAndNullOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "select exists(Item?.IntBoxed) as t0 from SupportMarkerInterface";

                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.Create().Add(Expressions.ExistsProperty("Item?.IntBoxed"), "t0");
                model.FromClause = FromClause.Create(FilterStream.Create(nameof(SupportMarkerInterface)));
                model = env.CopyMayFail(model);
                Assert.AreEqual(stmtText, model.ToEPL());
                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));

                env.CompileDeploy(model).AddListener("s0");

                AssertStringAndNull(env);

                env.UndeployAll();
            }
        }

        private class ExprCoreCastStringAndNullCompile : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select exists(Item?.IntBoxed) as t0 from SupportMarkerInterface";
                env.EplToModelCompileDeploy(epl).AddListener("s0").Milestone(0);

                AssertStringAndNull(env);

                env.UndeployAll();
            }
        }

        private static void AssertStringAndNull(RegressionEnvironment env)
        {
            env.AssertStatement(
                "s0",
                statement => Assert.AreEqual(typeof(bool?), statement.EventType.GetPropertyType("t0")));

            env.SendEventBean(new SupportBeanDynRoot(new SupportBean()));
            env.AssertEqualsNew("s0", "t0", true);

            env.SendEventBean(new SupportBeanDynRoot(null));
            env.AssertEqualsNew("s0", "t0", false);

            env.SendEventBean(new SupportBeanDynRoot("abc"));
            env.AssertEqualsNew("s0", "t0", false);
        }

        private static void AssertResults(
            EventBean theEvent,
            bool[] result)
        {
            for (var i = 0; i < result.Length; i++) {
                Assert.AreEqual(result[i], theEvent.Get("t" + i), "failed for index " + i);
            }
        }
    }
} // end of namespace