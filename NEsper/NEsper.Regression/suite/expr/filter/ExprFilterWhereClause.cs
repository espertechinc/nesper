///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.filter
{
    public class ExprFilterWhereClause
    {
        public static IList<RegressionExecution> Executions()
        {
            var executions = new List<RegressionExecution>();
            executions.Add(new ExprFilterWhereClauseSimple());
            executions.Add(new ExprFilterWhereClauseNumericType());
            return executions;
        }

        private static void SendMarketDataEvent(
            RegressionEnvironment env,
            string symbol)
        {
            var theEvent = new SupportMarketDataBean(symbol, 0, 0L, "");
            env.SendEventBean(theEvent);
        }

        private static void SendSupportBeanEvent(
            RegressionEnvironment env,
            int intPrimitive,
            long longPrimitive,
            float floatPrimitive,
            double doublePrimitive)
        {
            var theEvent = new SupportBean();
            theEvent.IntPrimitive = intPrimitive;
            theEvent.LongPrimitive = longPrimitive;
            theEvent.FloatPrimitive = floatPrimitive;
            theEvent.DoublePrimitive = doublePrimitive;
            env.SendEventBean(theEvent);
        }

        internal class ExprFilterWhereClauseSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select * from SupportMarketDataBean#length(3) where Symbol='CSCO'";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendMarketDataEvent(env, "IBM");
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                SendMarketDataEvent(env, "CSCO");
                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());

                SendMarketDataEvent(env, "IBM");
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                SendMarketDataEvent(env, "CSCO");
                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());

                // invalid return type for filter during compilation time
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "Select TheString From SupportBean#time(30 seconds) where IntPrimitive group by TheString",
                    "Error validating expression: The where-clause filter expression must return a boolean value");

                // invalid return type for filter at eventService
                epl = "select * From MapEventWithCriteriaBool#time(30 seconds) where criteria";
                env.CompileDeploy(epl);

                try {
                    env.SendEventMap(Collections.SingletonDataMap("criteria", 15), "MapEventWithCriteriaBool");
                    Assert.Fail(); // ensure exception handler rethrows
                }
                catch (EPException) {
                    // fine
                }

                env.UndeployAll();
            }
        }

        internal class ExprFilterWhereClauseNumericType : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select " +
                          " IntPrimitive + LongPrimitive as p1," +
                          " IntPrimitive * DoublePrimitive as p2," +
                          " FloatPrimitive / DoublePrimitive as p3" +
                          " from SupportBean#length(3) where " +
                          "IntPrimitive=LongPrimitive and IntPrimitive=DoublePrimitive and FloatPrimitive=DoublePrimitive";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendSupportBeanEvent(env, 1, 2, 3, 4);
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                SendSupportBeanEvent(env, 2, 2, 2, 2);
                var theEvent = env.Listener("s0").GetAndResetLastNewData()[0];
                Assert.AreEqual(typeof(long?), theEvent.EventType.GetPropertyType("p1"));
                Assert.AreEqual(4L, theEvent.Get("p1"));
                Assert.AreEqual(typeof(double?), theEvent.EventType.GetPropertyType("p2"));
                Assert.AreEqual(4d, theEvent.Get("p2"));
                Assert.AreEqual(typeof(double?), theEvent.EventType.GetPropertyType("p3"));
                Assert.AreEqual(1d, theEvent.Get("p3"));

                env.UndeployAll();
            }
        }
    }
} // end of namespace