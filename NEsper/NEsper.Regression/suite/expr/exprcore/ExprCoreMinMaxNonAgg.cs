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
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
    public class ExprCoreMinMaxNonAgg
    {
        private const string EPL = "select max(longBoxed,IntBoxed) as myMax, " +
                                   "max(longBoxed,IntBoxed,shortBoxed) as myMaxEx, " +
                                   "min(longBoxed,IntBoxed) as myMin, " +
                                   "min(longBoxed,IntBoxed,shortBoxed) as myMinEx" +
                                   " from SupportBean#length(3)";

        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ExecCoreMinMax());
            execs.Add(new ExecCoreMinMaxOM());
            execs.Add(new ExecCoreMinMaxCompile());
            return execs;
        }

        private static void TryMinMaxWindowStats(RegressionEnvironment env)
        {
            SendEvent(env, 10, 20, 4);
            var received = env.Listener("s0").GetAndResetLastNewData()[0];
            Assert.AreEqual(20L, received.Get("myMax"));
            Assert.AreEqual(10L, received.Get("myMin"));
            Assert.AreEqual(4L, received.Get("myMinEx"));
            Assert.AreEqual(20L, received.Get("myMaxEx"));

            SendEvent(env, -10, -20, -30);
            received = env.Listener("s0").GetAndResetLastNewData()[0];
            Assert.AreEqual(-10L, received.Get("myMax"));
            Assert.AreEqual(-20L, received.Get("myMin"));
            Assert.AreEqual(-30L, received.Get("myMinEx"));
            Assert.AreEqual(-10L, received.Get("myMaxEx"));
        }

        private static void SetUpMinMax(RegressionEnvironment env)
        {
            env.CompileDeploy("@Name('s0')  " + EPL).AddListener("s0");
        }

        private static void SendEvent(
            RegressionEnvironment env,
            long longBoxed,
            int intBoxed,
            short shortBoxed)
        {
            SendBoxedEvent(env, longBoxed, intBoxed, shortBoxed);
        }

        private static void SendBoxedEvent(
            RegressionEnvironment env,
            long? longBoxed,
            int? intBoxed,
            short? shortBoxed)
        {
            var bean = new SupportBean();
            bean.LongBoxed = longBoxed;
            bean.IntBoxed = intBoxed;
            bean.ShortBoxed = shortBoxed;
            env.SendEventBean(bean);
        }

        internal class ExecCoreMinMax : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SetUpMinMax(env);
                var type = env.Statement("s0").EventType;
                Assert.AreEqual(typeof(long?), type.GetPropertyType("myMax"));
                Assert.AreEqual(typeof(long?), type.GetPropertyType("myMin"));
                Assert.AreEqual(typeof(long?), type.GetPropertyType("myMinEx"));
                Assert.AreEqual(typeof(long?), type.GetPropertyType("myMaxEx"));

                TryMinMaxWindowStats(env);

                env.UndeployAll();
            }
        }

        internal class ExecCoreMinMaxOM : RegressionExecution
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

                model.FromClause = FromClause
                    .Create(
                        FilterStream.Create(typeof(SupportBean).FullName)
                            .AddView("length", Expressions.Constant(3)));
                model = env.CopyMayFail(model);
                Assert.AreEqual(EPL, model.ToEPL());

                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model).AddListener("s0");

                TryMinMaxWindowStats(env);

                env.UndeployAll();
            }
        }

        internal class ExecCoreMinMaxCompile : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.EplToModelCompileDeploy("@Name('s0') " + EPL).AddListener("s0");
                TryMinMaxWindowStats(env);
                env.UndeployAll();
            }
        }
    }
} // end of namespace