///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Numerics;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
    public class ExprCoreBigNumberSupport
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithEquals(execs);
            WithRelOp(execs);
            WithBetween(execs);
            WithIn(execs);
            WithMath(execs);
            WithAggregation(execs);
            WithMinMax(execs);
            WithFilterEquals(execs);
            WithJoin(execs);
            WithCastAndUDF(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithCastAndUDF(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreBigNumberCastAndUDF());
            return execs;
        }

        public static IList<RegressionExecution> WithJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreBigNumberJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithFilterEquals(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreBigNumberFilterEquals());
            return execs;
        }

        public static IList<RegressionExecution> WithMinMax(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreBigNumberMinMax());
            return execs;
        }

        public static IList<RegressionExecution> WithAggregation(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreBigNumberAggregation());
            return execs;
        }

        public static IList<RegressionExecution> WithMath(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreBigNumberMath());
            return execs;
        }

        public static IList<RegressionExecution> WithIn(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreBigNumberIn());
            return execs;
        }

        public static IList<RegressionExecution> WithBetween(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreBigNumberBetween());
            return execs;
        }

        public static IList<RegressionExecution> WithRelOp(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreBigNumberRelOp());
            return execs;
        }

        public static IList<RegressionExecution> WithEquals(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreBigNumberEquals());
            return execs;
        }

        private class ExprCoreBigNumberEquals : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                // test equals decimal
                var epl =
                    "@name('s0') select * from SupportBeanNumeric where bigdec = 1 or bigdec = intOne or bigdec = doubleOne";
                env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());

                SendBigNumEvent(env, -1, 1);
                env.AssertListenerInvoked("s0");
                SendBigNumEvent(env, -1, 2);
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBeanNumeric(2, 0, null, 2m, 0, 0));
                env.AssertListenerInvoked("s0");
                env.SendEventBean(new SupportBeanNumeric(3, 0, null, 2m, 0, 0));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBeanNumeric(0, 0, null, 3m, 3d, 0));
                env.AssertListenerInvoked("s0");
                env.SendEventBean(new SupportBeanNumeric(0, 0, null, 3.9999m, 4d, 0));
                env.AssertListenerNotInvoked("s0");

                // test equals BigInteger
                env.UndeployAll();
                epl =
                    "@name('s0') select * from SupportBeanNumeric where bigdec = bigint or bigint = intOne or bigint = 1";
                env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());

                env.SendEventBean(new SupportBeanNumeric(0, 0, new BigInteger(2), 2m, 0, 0));
                env.AssertListenerInvoked("s0");
                env.SendEventBean(new SupportBeanNumeric(0, 0, new BigInteger(3), 2m, 0, 0));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBeanNumeric(2, 0, new BigInteger(2), null, 0, 0));
                env.AssertListenerInvoked("s0");
                env.SendEventBean(new SupportBeanNumeric(3, 0, new BigInteger(2), null, 0, 0));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBeanNumeric(0, 0, new BigInteger(1), null, 0, 0));
                env.AssertListenerInvoked("s0");
                env.SendEventBean(new SupportBeanNumeric(0, 0, new BigInteger(4), null, 0, 0));
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        private class ExprCoreBigNumberRelOp : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // relational op tests handled by relational op unit test
                var epl = "@name('s0') select * from SupportBeanNumeric where bigdec < 10 and bigint > 10";
                env.CompileDeploy(epl).AddListener("s0");

                SendBigNumEvent(env, 10, 10);
                env.AssertListenerNotInvoked("s0");

                SendBigNumEvent(env, 11, 9);
                env.AssertListenerInvoked("s0");
                env.UndeployAll();

                epl = "@name('s0') select * from SupportBeanNumeric where bigdec < 10.0";
                env.CompileDeployAddListenerMile(epl, "s0", 1);

                SendBigNumEvent(env, 0, 11);
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBeanNumeric(null, 9.999m));
                env.AssertListenerInvoked("s0");
                env.UndeployAll();

                // test float
                env.CompileDeployAddListenerMile(
                    "@name('s0') select * from SupportBeanNumeric where floatOne < 10f and floatTwo > 10f",
                    "s0",
                    2);

                env.SendEventBean(new SupportBeanNumeric(true, 1f, 20f));
                env.AssertListenerInvoked("s0");
                env.SendEventBean(new SupportBeanNumeric(true, 20f, 1f));
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        private class ExprCoreBigNumberBetween : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select * from SupportBeanNumeric where bigdec between 10 and 20 or bigint between 100 and 200";
                env.CompileDeploy(epl).AddListener("s0");

                SendBigNumEvent(env, 0, 9);
                env.AssertListenerNotInvoked("s0");

                SendBigNumEvent(env, 0, 10);
                env.AssertListenerInvoked("s0");

                SendBigNumEvent(env, 99, 0);
                env.AssertListenerNotInvoked("s0");

                SendBigNumEvent(env, 100, 0);
                env.AssertListenerInvoked("s0");

                env.UndeployAll();
            }
        }

        private class ExprCoreBigNumberIn : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select * from SupportBeanNumeric where bigdec in (10, 20d) or bigint in (0x02, 3)";
                env.CompileDeploy(epl).AddListener("s0");

                SendBigNumEvent(env, 0, 9);
                env.AssertListenerNotInvoked("s0");

                SendBigNumEvent(env, 0, 10);
                env.AssertListenerInvoked("s0");

                env.SendEventBean(new SupportBeanNumeric(null, 20m));
                env.AssertListenerInvoked("s0");

                SendBigNumEvent(env, 99, 0);
                env.AssertListenerNotInvoked("s0");

                SendBigNumEvent(env, 2, 0);
                env.AssertListenerInvoked("s0");

                SendBigNumEvent(env, 3, 0);
                env.AssertListenerInvoked("s0");

                env.UndeployAll();
            }
        }

        private class ExprCoreBigNumberMath : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select * from SupportBeanNumeric " +
                          "where bigdec+bigint=100 or bigdec+1=2 or bigdec+2d=5.0 or bigint+5L=8 or bigint+5d=9.0";
                env.CompileDeploy(epl).AddListener("s0");

                SendBigNumEvent(env, 50, 49);
                env.AssertListenerNotInvoked("s0");

                SendBigNumEvent(env, 50, 50);
                env.AssertListenerInvoked("s0");

                SendBigNumEvent(env, 0, 1);
                env.AssertListenerInvoked("s0");

                SendBigNumEvent(env, 0, 2);
                env.AssertListenerNotInvoked("s0");

                SendBigNumEvent(env, 0, 3);
                env.AssertListenerInvoked("s0");

                SendBigNumEvent(env, 0, 0);
                env.AssertListenerNotInvoked("s0");

                SendBigNumEvent(env, 3, 0);
                env.AssertListenerInvoked("s0");

                SendBigNumEvent(env, 4, 0);
                env.AssertListenerInvoked("s0");
                env.UndeployAll();

                env.CompileDeployAddListenerMile(
                    "@name('s0') select bigdec+bigint as v1, bigdec+2 as v2, bigdec+3d as v3, bigint+5L as v4, bigint+5d as v5 " +
                    " from SupportBeanNumeric",
                    "s0",
                    1);
                env.ListenerReset("s0");

                env.AssertStatement(
                    "s0",
                    statement => {
                        Assert.AreEqual(typeof(decimal), statement.EventType.GetPropertyType("v1"));
                        Assert.AreEqual(typeof(decimal), statement.EventType.GetPropertyType("v2"));
                        Assert.AreEqual(typeof(decimal), statement.EventType.GetPropertyType("v3"));
                        Assert.AreEqual(typeof(BigInteger), statement.EventType.GetPropertyType("v4"));
                        Assert.AreEqual(typeof(decimal), statement.EventType.GetPropertyType("v5"));
                    });

                SendBigNumEvent(env, 1, 2);
                env.AssertPropsNew(
                    "s0",
                    "v1,v2,v3,v4,v5".SplitCsv(),
                    new object[] { 3m, 4m, 5m, new BigInteger(6), 6m });

                // test aggregation-sum, multiplication and division all together; test for ESPER-340
                env.UndeployAll();

                env.CompileDeployAddListenerMile(
                    "@name('s0') select (sum(bigdecTwo * bigdec)/sum(bigdec)) as avgRate from SupportBeanNumeric",
                    "s0",
                    2);
                env.AssertStatement(
                    "s0",
                    statement => Assert.AreEqual(typeof(decimal), statement.EventType.GetPropertyType("avgRate")));
                SendBigNumEvent(env, 0, 5);
                env.AssertEventNew(
                    "s0",
                    @event => {
                        var avgRate = @event.Get("avgRate");
                        Assert.IsTrue(avgRate is decimal);
                        Assert.AreEqual(5m, avgRate);
                    });

                env.UndeployAll();
            }
        }

        private class ExprCoreBigNumberAggregation : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "sum(bigint),sum(bigdec)," +
                             "avg(bigint),avg(bigdec)," +
                             "median(bigint),median(bigdec)," +
                             "stddev(bigint),stddev(bigdec)," +
                             "avedev(bigint),avedev(bigdec)," +
                             "min(bigint),min(bigdec)";
                var epl = "@name('s0') select " + fields + " from SupportBeanNumeric";
                env.CompileDeploy(epl).AddListener("s0");

                var fieldList = fields.SplitCsv();
                SendBigNumEvent(env, 1, 2);
                env.AssertPropsNew(
                    "s0",
                    fieldList,
                    new object[] {
                        new BigInteger(1), 2m, // sum
                        1m, 2m, // avg
                        1d, 2d, // median
                        null, null,
                        0.0, 0.0,
                        new BigInteger(1), 2m,
                    });

                env.Milestone(1);

                SendBigNumEvent(env, 4, 5);
                env.AssertPropsNew(
                    "s0",
                    fieldList,
                    new object[] {
                        new BigInteger(5),
                        7m, // sum
                        2.5m,
                        3.5m, // avg
                        2.5d, 3.5d, // median
                        2.1213203435596424, 2.1213203435596424,
                        1.5, 1.5,
                        new BigInteger(1),
                        2m,
                    });

                env.UndeployAll();
            }
        }

        private class ExprCoreBigNumberMinMax : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select min(bigint, 10) as v1, min(10, bigint) as v2, " +
                          "max(bigdec, 10) as v3, max(10, 100d, bigint, bigdec) as v4 from SupportBeanNumeric";
                env.CompileDeploy(epl).AddListener("s0");

                var fieldList = "v1,v2,v3,v4".SplitCsv();

                SendBigNumEvent(env, 1, 2);
                env.AssertPropsNew(
                    "s0",
                    fieldList,
                    new object[] { new BigInteger(1), new BigInteger(1), 10m, 100m });

                SendBigNumEvent(env, 40, 300);
                env.AssertPropsNew(
                    "s0",
                    fieldList,
                    new object[] { new BigInteger(10), new BigInteger(10), 300m, 300m });

                SendBigNumEvent(env, 250, 200);
                env.AssertPropsNew(
                    "s0",
                    fieldList,
                    new object[] { new BigInteger(10), new BigInteger(10), 200m, 250m });

                env.UndeployAll();
            }
        }

        private class ExprCoreBigNumberFilterEquals : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fieldList = "bigdec".SplitCsv();
                var epl = "@name('s0') select bigdec from SupportBeanNumeric(bigdec = 4)";
                env.CompileDeploy(epl).AddListener("s0");

                SendBigNumEvent(env, 0, 2);
                env.AssertListenerNotInvoked("s0");

                SendBigNumEvent(env, 0, 4);
                env.AssertPropsNew("s0", fieldList, new object[] { 4m });

                env.UndeployAll();
                env.CompileDeployAddListenerMile(
                    "@name('s0') select bigdec from SupportBeanNumeric(bigdec = 4d)",
                    "s0",
                    1);

                SendBigNumEvent(env, 0, 4);
                env.AssertListenerInvoked("s0");

                env.SendEventBean(new SupportBeanNumeric(new BigInteger(0), 4m));
                env.AssertPropsNew("s0", fieldList, new object[] { 4m });

                env.UndeployAll();
                env.CompileDeployAddListenerMile(
                    "@name('s0') select bigdec from SupportBeanNumeric(bigint = 4)",
                    "s0",
                    2);

                SendBigNumEvent(env, 3, 4);
                env.AssertListenerNotInvoked("s0");

                SendBigNumEvent(env, 4, 3);
                env.AssertPropsNew("s0", fieldList, new object[] { 3m });

                env.UndeployAll();
            }
        }

        private class ExprCoreBigNumberJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fieldList = "bigint,bigdec".SplitCsv();
                var epl = "@name('s0') select bigint,bigdec from SupportBeanNumeric#keepall(), SupportBean#keepall " +
                          "where intPrimitive = bigint and doublePrimitive = bigdec";
                env.CompileDeploy(epl).AddListener("s0");

                SendSupportBean(env, 2, 3);
                SendBigNumEvent(env, 0, 2);
                SendBigNumEvent(env, 2, 0);
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBeanNumeric(new BigInteger(2), 3m));
                env.AssertPropsNew("s0", fieldList, new object[] { new BigInteger(2), 3m });

                env.UndeployAll();
            }
        }

        private class ExprCoreBigNumberCastAndUDF : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select SupportStaticMethodLib.myBigIntFunc(cast(2, BigInteger)) as v1, SupportStaticMethodLib.myBigDecFunc(cast(3d, decimal)) as v2 from SupportBeanNumeric";
                env.CompileDeploy(epl).AddListener("s0");

                var fieldList = "v1,v2".SplitCsv();
                SendBigNumEvent(env, 0, 2);
                env.AssertPropsNew("s0", fieldList, new object[] { new BigInteger(2), 3.0m });

                env.UndeployAll();
            }
        }

        private static void SendBigNumEvent(
            RegressionEnvironment env,
            int bigInt,
            decimal decimalPrimitive)
        {
            var bean = new SupportBeanNumeric(new BigInteger(bigInt), decimalPrimitive);
            bean.DecimalTwo = decimalPrimitive;
            env.SendEventBean(bean);
        }

        private static void SendSupportBean(
            RegressionEnvironment env,
            int intPrimitive,
            double doublePrimitive)
        {
            var bean = new SupportBean();
            bean.IntPrimitive = intPrimitive;
            bean.DoublePrimitive = doublePrimitive;
            env.SendEventBean(bean);
        }
    }
} // end of namespace