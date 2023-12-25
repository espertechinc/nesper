///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.expreval;

using NUnit.Framework;
namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
    public class ExprCoreMinMaxNonAgg
    {
        private const string EPL = "select max(LongBoxed,IntBoxed) as myMax, " +
                                   "max(LongBoxed,IntBoxed,ShortBoxed) as myMaxEx, " +
                                   "min(LongBoxed,IntBoxed) as myMin, " +
                                   "min(LongBoxed,IntBoxed,ShortBoxed) as myMinEx" +
                                   " from SupportBean#length(3)";

        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithMinMax(execs);
            WithMinMaxOM(execs);
            WithMinMaxCompile(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithMinMaxCompile(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExecCoreMinMaxCompile());
            return execs;
        }

        public static IList<RegressionExecution> WithMinMaxOM(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExecCoreMinMaxOM());
            return execs;
        }

        public static IList<RegressionExecution> WithMinMax(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExecCoreMinMax());
            return execs;
        }

        private class ExecCoreMinMax : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "myMax,myMaxEx,myMin,myMinEx".SplitCsv();
                var builder = new SupportEvalBuilder("SupportBean")
                    .WithExpression(fields[0], "max(LongBoxed,IntBoxed)")
                    .WithExpression(fields[1], "max(LongBoxed,IntBoxed,ShortBoxed)")
                    .WithExpression(fields[2], "min(LongBoxed,IntBoxed)")
                    .WithExpression(fields[3], "min(LongBoxed,IntBoxed,ShortBoxed)");

                builder.WithStatementConsumer(
                    stmt => {
                        var type = stmt.EventType;
                        Assert.AreEqual(typeof(long?), type.GetPropertyType("myMax"));
                        Assert.AreEqual(typeof(long?), type.GetPropertyType("myMin"));
                        Assert.AreEqual(typeof(long?), type.GetPropertyType("myMinEx"));
                        Assert.AreEqual(typeof(long?), type.GetPropertyType("myMaxEx"));
                    });

                builder.WithAssertion(MakeBoxedEvent(10L, 20, 4)).Expect(fields, 20L, 20L, 10L, 4L);
                builder.WithAssertion(MakeBoxedEvent(-10L, -20, -30)).Expect(fields, -10L, -10L, -20L, -30L);
                builder.WithAssertion(MakeBoxedEvent(null, null, null)).Expect(fields, null, null, null, null);
                builder.WithAssertion(MakeBoxedEvent(1L, null, 1)).Expect(fields, null, null, null, null);

                builder.Run(env);
                env.UndeployAll();
            }
        }

        private class ExecCoreMinMaxOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.Create()
                    .Add(Expressions.Max("LongBoxed", "IntBoxed"), "myMax")
                    .Add(
                        Expressions.Max(
                            Expressions.Property("LongBoxed"),
                            Expressions.Property("IntBoxed"),
                            Expressions.Property("ShortBoxed")),
                        "myMaxEx")
                    .Add(Expressions.Min("LongBoxed", "IntBoxed"), "myMin")
                    .Add(
                        Expressions.Min(
                            Expressions.Property("LongBoxed"),
                            Expressions.Property("IntBoxed"),
                            Expressions.Property("ShortBoxed")),
                        "myMinEx");

                model.FromClause = FromClause.Create(
                    FilterStream.Create(nameof(SupportBean)).AddView("length", Expressions.Constant(3)));
                model = env.CopyMayFail(model);
                Assert.AreEqual(EPL, model.ToEPL());

                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model).AddListener("s0");

                TryMinMaxWindowStats(env);

                env.UndeployAll();
            }
        }

        private class ExecCoreMinMaxCompile : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.EplToModelCompileDeploy("@name('s0') " + EPL).AddListener("s0");
                TryMinMaxWindowStats(env);
                env.UndeployAll();
            }
        }

        private static void TryMinMaxWindowStats(RegressionEnvironment env)
        {
            SendEvent(env, 10, 20, 4);
            env.AssertListener(
                "s0",
                listener => {
                    var received = listener.GetAndResetLastNewData()[0];
                    Assert.AreEqual(20L, received.Get("myMax"));
                    Assert.AreEqual(10L, received.Get("myMin"));
                    Assert.AreEqual(4L, received.Get("myMinEx"));
                    Assert.AreEqual(20L, received.Get("myMaxEx"));
                });

            SendEvent(env, -10, -20, -30);
            env.AssertListener(
                "s0",
                listener => {
                    var received = listener.GetAndResetLastNewData()[0];
                    Assert.AreEqual(-10L, received.Get("myMax"));
                    Assert.AreEqual(-20L, received.Get("myMin"));
                    Assert.AreEqual(-30L, received.Get("myMinEx"));
                    Assert.AreEqual(-10L, received.Get("myMaxEx"));
                });
        }

        private static void SendEvent(
            RegressionEnvironment env,
            long longBoxed,
            int intBoxed,
            short shortBoxed)
        {
            env.SendEventBean(MakeBoxedEvent(longBoxed, intBoxed, shortBoxed));
        }

        private static SupportBean MakeBoxedEvent(
            long? longBoxed,
            int? intBoxed,
            short? shortBoxed)
        {
            var bean = new SupportBean();
            bean.LongBoxed = longBoxed;
            bean.IntBoxed = intBoxed;
            bean.ShortBoxed = shortBoxed;
            return bean;
        }
    }
} // end of namespace