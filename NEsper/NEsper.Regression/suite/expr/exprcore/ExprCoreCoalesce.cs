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

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
    public class ExprCoreCoalesce
    {
        public static IList<RegressionExecution> Executions()
        {
            var executions = new List<RegressionExecution>();
            executions.Add(new ExprCoreCoalesceBeans());
            executions.Add(new ExprCoreCoalesceLong());
            executions.Add(new ExprCoreCoalesceLongOM());
            executions.Add(new ExprCoreCoalesceLongCompile());
            executions.Add(new ExprCoreCoalesceDouble());
            executions.Add(new ExprCoreCoalesceNull());
            executions.Add(new ExprCoreCoalesceInvalid());
            return executions;
        }

        private static void TryCoalesceInvalid(
            RegressionEnvironment env,
            string coalesceExpr)
        {
            var epl = "select " + coalesceExpr + " as result from SupportBean";
            TryInvalidCompile(env, epl, "skip");
        }

        private static void TryCoalesceLong(RegressionEnvironment env)
        {
            SendEvent(env, 1L, 2, 3);
            Assert.AreEqual(1L, env.Listener("s0").AssertOneGetNewAndReset().Get("result"));

            SendBoxedEvent(env, null, 2, null);
            Assert.AreEqual(2L, env.Listener("s0").AssertOneGetNewAndReset().Get("result"));

            SendBoxedEvent(env, null, null, short.Parse("3"));
            Assert.AreEqual(3L, env.Listener("s0").AssertOneGetNewAndReset().Get("result"));

            SendBoxedEvent(env, null, null, null);
            Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("result"));
        }

        private static SupportBean SendEvent(
            RegressionEnvironment env,
            string theString)
        {
            var bean = new SupportBean();
            bean.TheString = theString;
            env.SendEventBean(bean);
            return bean;
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

        private static void SendEventWithDouble(
            RegressionEnvironment env,
            byte? byteBoxed,
            short? shortBoxed,
            int? intBoxed,
            long? longBoxed,
            float? floatBoxed,
            double? doubleBoxed)
        {
            var bean = new SupportBean();
            bean.ByteBoxed = byteBoxed;
            bean.ShortBoxed = shortBoxed;
            bean.IntBoxed = intBoxed;
            bean.LongBoxed = longBoxed;
            bean.FloatBoxed = floatBoxed;
            bean.DoubleBoxed = doubleBoxed;
            env.SendEventBean(bean);
        }

        internal class ExprCoreCoalesceBeans : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select coalesce(a.TheString, b.TheString) as myString, coalesce(a, b) as myBean" +
                    " from pattern [every (a=SupportBean(theString='s0') or b=SupportBean(theString='s1'))]";
                env.CompileDeploy(epl).AddListener("s0");

                var theEvent = SendEvent(env, "s0");
                var eventReceived = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual("s0", eventReceived.Get("myString"));
                Assert.AreSame(theEvent, eventReceived.Get("myBean"));

                theEvent = SendEvent(env, "s1");
                eventReceived = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual("s1", eventReceived.Get("myString"));
                Assert.AreSame(theEvent, eventReceived.Get("myBean"));

                env.UndeployAll();
            }
        }

        internal class ExprCoreCoalesceLong : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                        "@Name('s0')  select coalesce(longBoxed, intBoxed, shortBoxed) as result from SupportBean")
                    .AddListener("s0");

                Assert.AreEqual(typeof(long?), env.Statement("s0").EventType.GetPropertyType("result"));

                TryCoalesceLong(env);

                env.UndeployAll();
            }
        }

        internal class ExprCoreCoalesceLongOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "select coalesce(longBoxed,intBoxed,shortBoxed) as result" +
                          " from SupportBean#length(1000)";

                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.Create()
                    .Add(
                        Expressions.Coalesce(
                            "LongBoxed",
                            "IntBoxed",
                            "ShortBoxed"),
                        "result");
                model.FromClause = FromClause.Create(
                    FilterStream.Create(typeof(SupportBean).Name).AddView("length", Expressions.Constant(1000)));
                model = env.CopyMayFail(model);
                Assert.AreEqual(epl, model.ToEPL());

                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model).AddListener("s0");
                Assert.AreEqual(typeof(long?), env.Statement("s0").EventType.GetPropertyType("result"));

                TryCoalesceLong(env);

                env.UndeployAll();
            }
        }

        internal class ExprCoreCoalesceLongCompile : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select coalesce(longBoxed,intBoxed,shortBoxed) as result" +
                          " from SupportBean#length(1000)";

                env.EplToModelCompileDeploy(epl).AddListener("s0");
                Assert.AreEqual(typeof(long?), env.Statement("s0").EventType.GetPropertyType("result"));

                TryCoalesceLong(env);

                env.UndeployAll();
            }
        }

        internal class ExprCoreCoalesceDouble : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select coalesce(null, byteBoxed, shortBoxed, intBoxed, longBoxed, floatBoxed, doubleBoxed) as result from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");
                Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("result"));

                SendEventWithDouble(env, null, null, null, null, null, null);
                Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("result"));

                SendEventWithDouble(env, null, short.Parse("2"), null, null, null, 1d);
                Assert.AreEqual(2d, env.Listener("s0").AssertOneGetNewAndReset().Get("result"));

                SendEventWithDouble(env, null, null, null, null, null, 100d);
                Assert.AreEqual(100d, env.Listener("s0").AssertOneGetNewAndReset().Get("result"));

                SendEventWithDouble(env, null, null, null, null, 10f, 100d);
                Assert.AreEqual(10d, env.Listener("s0").AssertOneGetNewAndReset().Get("result"));

                SendEventWithDouble(env, null, null, 1, 5L, 10f, 100d);
                Assert.AreEqual(1d, env.Listener("s0").AssertOneGetNewAndReset().Get("result"));

                SendEventWithDouble(env, byte.Parse("3"), null, null, null, null, null);
                Assert.AreEqual(3d, env.Listener("s0").AssertOneGetNewAndReset().Get("result"));

                SendEventWithDouble(env, null, null, null, 5L, 10f, 100d);
                Assert.AreEqual(5d, env.Listener("s0").AssertOneGetNewAndReset().Get("result"));

                env.UndeployAll();
            }
        }

        internal class ExprCoreCoalesceInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryCoalesceInvalid(env, "coalesce(IntPrimitive)");
                TryCoalesceInvalid(env, "coalesce(intPrimitive, string)");
                TryCoalesceInvalid(env, "coalesce(intPrimitive, xxx)");
                TryCoalesceInvalid(env, "coalesce(intPrimitive, booleanBoxed)");
                TryCoalesceInvalid(env, "coalesce(charPrimitive, longBoxed)");
                TryCoalesceInvalid(env, "coalesce(charPrimitive, string, string)");
                TryCoalesceInvalid(env, "coalesce(string, longBoxed)");
                TryCoalesceInvalid(env, "coalesce(null, longBoxed, string)");
                TryCoalesceInvalid(env, "coalesce(null, null, boolBoxed, 1l)");
            }
        }

        internal class ExprCoreCoalesceNull : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select coalesce(null, null) as result from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");
                Assert.AreEqual(null, env.Statement("s0").EventType.GetPropertyType("result"));

                env.SendEventBean(new SupportBean());
                Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("result"));

                env.UndeployAll();
            }
        }
    }
} // end of namespace