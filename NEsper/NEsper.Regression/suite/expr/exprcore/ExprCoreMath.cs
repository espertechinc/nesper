///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Numerics;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
    public class ExprCoreMath
    {
        public static IList<RegressionExecution> Executions()
        {
            var executions = new List<RegressionExecution>();
            executions.Add(new ExprCoreMathDouble());
            executions.Add(new ExprCoreMathLong());
            executions.Add(new ExprCoreMathFloat());
            executions.Add(new ExprCoreMathIntWNull());
            executions.Add(new ExprCoreMathBigDec());
            executions.Add(new ExprCoreMathBigDecConv());
            executions.Add(new ExprCoreMathBigInt());
            executions.Add(new ExprCoreMathBigIntConv());
            executions.Add(new ExprCoreMathShortAndByteArithmetic());
            executions.Add(new ExprCoreMathModulo());
            return executions;
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

        private static void SendEvent(
            RegressionEnvironment env,
            int intPrimitive,
            int? intBoxed)
        {
            var bean = new SupportBean();
            bean.IntBoxed = intBoxed;
            bean.IntPrimitive = intPrimitive;
            env.SendEventBean(bean);
        }

        private static void AssertTypes(
            EPStatement stmt,
            string[] fields,
            params Type[] types)
        {
            for (var i = 0; i < fields.Length; i++) {
                Assert.AreEqual(types[i], stmt.EventType.GetPropertyType(fields[i]), "failed for " + i);
            }
        }

        internal class ExprCoreMathDouble : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select " +
                          "10d+5d as c0," +
                          "10d-5d as c1," +
                          "10d*5d as c2," +
                          "10d/5d as c3," +
                          "10d%4d as c4" +
                          " from SupportBean";

                env.CompileDeploy(epl).AddListener("s0");

                var fields = new [] { "c0", "c1", "c2", "c3", "c4" };
                AssertTypes(
                    env.Statement("s0"),
                    fields,
                    typeof(double?),
                    typeof(double?),
                    typeof(double?),
                    typeof(double?),
                    typeof(double?));

                env.SendEventBean(new SupportBean());
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {15d, 5d, 50d, 2d, 2d});

                env.UndeployAll();
            }
        }

        internal class ExprCoreMathLong : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select " +
                          "10L+5L as c0," +
                          "10L-5L as c1," +
                          "10L*5L as c2," +
                          "10L/5L as c3" +
                          " from SupportBean";

                env.CompileDeploy(epl).AddListener("s0");

                var fields = new [] { "c0", "c1", "c2", "c3" };
                AssertTypes(env.Statement("s0"), fields, typeof(long?), typeof(long?), typeof(long?), typeof(double?));

                env.SendEventBean(new SupportBean());
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {15L, 5L, 50L, 2d});

                env.UndeployAll();
            }
        }

        internal class ExprCoreMathFloat : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select " +
                          "10f+5f as c0," +
                          "10f-5f as c1," +
                          "10f*5f as c2," +
                          "10f/5f as c3," +
                          "10f%4f as c4" +
                          " from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");

                var fields = new [] { "c0", "c1", "c2", "c3", "c4" };
                AssertTypes(
                    env.Statement("s0"),
                    fields,
                    typeof(float?),
                    typeof(float?),
                    typeof(float?),
                    typeof(double?),
                    typeof(float?));

                env.SendEventBean(new SupportBean());
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {15f, 5f, 50f, 2d, 2f});

                env.UndeployAll();
            }
        }

        internal class ExprCoreMathIntWNull : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select IntPrimitive/IntBoxed as result from SupportBean";

                env.CompileDeploy(epl).AddListener("s0");
                Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("result"));

                SendEvent(env, 100, 3);
                Assert.AreEqual(100 / 3d, env.Listener("s0").AssertOneGetNewAndReset().Get("result"));

                SendEvent(env, 100, null);
                Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("result"));

                SendEvent(env, 100, 0);
                Assert.AreEqual(double.PositiveInfinity, env.Listener("s0").AssertOneGetNewAndReset().Get("result"));

                SendEvent(env, -5, 0);
                Assert.AreEqual(double.NegativeInfinity, env.Listener("s0").AssertOneGetNewAndReset().Get("result"));

                env.UndeployAll();
            }
        }

        internal class ExprCoreMathBigDecConv : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select " +
                          "10+5.0m as c0," +
                          "10-5.0m as c1," +
                          "10*5.0m as c2," +
                          "10/5.0m as c3" +
                          " from SupportBean";

                env.CompileDeploy(epl).AddListener("s0");

                var fields = new [] { "c0", "c1", "c2", "c3" };
                AssertTypes(
                    env.Statement("s0"),
                    fields,
                    typeof(decimal),
                    typeof(decimal),
                    typeof(decimal),
                    typeof(decimal));

                env.SendEventBean(new SupportBean());
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {
                        15m, 5m, 50m,
                        2m
                    });

                env.UndeployAll();
            }
        }

        internal class ExprCoreMathBigIntConv : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select " +
                          "10+BigInteger.ValueOf(5) as c0," +
                          "10-BigInteger.ValueOf(5) as c1," +
                          "10*BigInteger.ValueOf(5) as c2," +
                          "10/BigInteger.ValueOf(5) as c3" +
                          " from SupportBean";

                env.CompileDeploy(epl).AddListener("s0");

                var fields = new [] { "c0", "c1", "c2", "c3" };
                AssertTypes(
                    env.Statement("s0"),
                    fields,
                    typeof(BigInteger),
                    typeof(BigInteger),
                    typeof(BigInteger),
                    typeof(double?));

                env.SendEventBean(new SupportBean());
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {new BigInteger(15), new BigInteger(5), new BigInteger(50), 2d});

                env.UndeployAll();
            }
        }

        internal class ExprCoreMathBigInt : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select " +
                          "BigInteger.ValueOf(10)+BigInteger.ValueOf(5) as c0," +
                          "BigInteger.ValueOf(10)-BigInteger.ValueOf(5) as c1," +
                          "BigInteger.ValueOf(10)*BigInteger.ValueOf(5) as c2," +
                          "BigInteger.ValueOf(10)/BigInteger.ValueOf(5) as c3" +
                          " from SupportBean";

                env.CompileDeploy(epl).AddListener("s0");

                var fields = new [] { "c0", "c1", "c2", "c3" };
                AssertTypes(
                    env.Statement("s0"),
                    fields,
                    typeof(BigInteger),
                    typeof(BigInteger),
                    typeof(BigInteger),
                    typeof(double?));

                env.SendEventBean(new SupportBean());
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {new BigInteger(15), new BigInteger(5), new BigInteger(50), 2d});

                env.UndeployAll();
            }
        }

        internal class ExprCoreMathBigDec : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select " +
                          "10.0m+5.0m as c0," +
                          "10.0m-5.0m as c1," +
                          "10.0m*5.0m as c2," +
                          "10.0m/5.0m as c3" +
                          " from SupportBean";

                env.CompileDeploy(epl).AddListener("s0");

                var fields = new [] { "c0", "c1", "c2", "c3" };
                AssertTypes(
                    env.Statement("s0"),
                    fields,
                    typeof(decimal),
                    typeof(decimal),
                    typeof(decimal),
                    typeof(decimal));

                env.SendEventBean(new SupportBean());
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {
                        15m, 5m, 50m,
                        2m
                    });

                env.UndeployAll();
            }
        }

        internal class ExprCoreMathShortAndByteArithmetic : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select " +
                          "ShortPrimitive + ShortBoxed as c0," +
                          "BytePrimitive + ByteBoxed as c1, " +
                          "ShortPrimitive - ShortBoxed as c2," +
                          "BytePrimitive - ByteBoxed as c3, " +
                          "ShortPrimitive * ShortBoxed as c4," +
                          "BytePrimitive * ByteBoxed as c5, " +
                          "ShortPrimitive / ShortBoxed as c6," +
                          "BytePrimitive / ByteBoxed as c7," +
                          "ShortPrimitive + LongPrimitive as c8," +
                          "BytePrimitive + LongPrimitive as c9 " +
                          "from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");
                var fields = new [] { "c0", "c1", "c2", "c3", "c4", "c5", "c6", "c7", "c8", "c9" };

                foreach (var field in fields) {
                    var expected = typeof(int?);
                    if (field.Equals("c6") || field.Equals("c7")) {
                        expected = typeof(double?);
                    }

                    if (field.Equals("c8") || field.Equals("c9")) {
                        expected = typeof(long?);
                    }

                    Assert.AreEqual(
                        expected,
                        env.Statement("s0").EventType.GetPropertyType(field),
                        "for field " + field);
                }

                var bean = new SupportBean();
                bean.ShortPrimitive = 5;
                bean.ShortBoxed = 6;
                bean.BytePrimitive = 4;
                bean.ByteBoxed = 2;
                bean.LongPrimitive = 10;
                env.SendEventBean(bean);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {11, 6, -1, 2, 30, 8, 5d / 6d, 2d, 15L, 14L});

                env.UndeployAll();
            }
        }

        internal class ExprCoreMathModulo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select LongBoxed % IntBoxed as myMod from SupportBean#length(3) where not(LongBoxed > IntBoxed)";
                env.CompileDeploy(epl).AddListener("s0");

                SendEvent(env, 1, 1, 0);
                Assert.AreEqual(0L, env.Listener("s0").LastNewData[0].Get("myMod"));
                env.Listener("s0").Reset();

                SendEvent(env, 2, 1, 0);
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                SendEvent(env, 2, 3, 0);
                Assert.AreEqual(2L, env.Listener("s0").LastNewData[0].Get("myMod"));
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }
    }
} // end of namespace