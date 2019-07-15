///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.resultset.outputlimit
{
    public class ResultSetOutputLimitCrontabWhen
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ResultSetOutputCrontabAt());
            execs.Add(new ResultSetOutputCrontabAtOMCreate());
            execs.Add(new ResultSetOutputCrontabAtOMCompile());
            execs.Add(new ResultSetOutputWhenBuiltInCountInsert());
            execs.Add(new ResultSetOutputWhenBuiltInCountRemove());
            execs.Add(new ResultSetOutputWhenBuiltInLastTimestamp());
            execs.Add(new ResultSetOutputCrontabAtVariable());
            execs.Add(new ResultSetOutputWhenExpression());
            execs.Add(new ResultSetOutputWhenThenExpression());
            execs.Add(new ResultSetOutputWhenThenExpressionSODA());
            execs.Add(new ResultSetOutputWhenThenSameVarTwice());
            execs.Add(new ResultSetOutputWhenThenWVariable());
            execs.Add(new ResultSetOutputWhenThenWCount());
            execs.Add(new ResultSetInvalid());
            return execs;
        }

        private static void TryAssertionCrontab(
            RegressionEnvironment env,
            int days)
        {
            var fields = "Symbol".SplitCsv();
            SendEvent(env, "S1", 0);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            SendTimeEvent(env, days, 17, 14, 59, 0);
            SendEvent(env, "S2", 0);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            SendTimeEvent(env, days, 17, 15, 0, 0);
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").GetAndResetLastNewData(),
                fields,
                new[] {new object[] {"S1"}, new object[] {"S2"}});

            SendTimeEvent(env, days, 17, 18, 0, 0);
            SendEvent(env, "S3", 0);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            SendTimeEvent(env, days, 17, 30, 0, 0);
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").GetAndResetLastNewData(),
                fields,
                new[] {new object[] {"S3"}});

            SendTimeEvent(env, days, 17, 35, 0, 0);
            SendTimeEvent(env, days, 17, 45, 0, 0);
            EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").GetAndResetLastNewData(), fields, null);

            SendEvent(env, "S4", 0);
            SendEvent(env, "S5", 0);
            SendTimeEvent(env, days, 18, 0, 0, 0);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            SendTimeEvent(env, days, 18, 1, 0, 0);
            SendEvent(env, "S6", 0);

            SendTimeEvent(env, days, 18, 15, 0, 0);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            SendTimeEvent(env, days + 1, 7, 59, 59, 0);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            SendTimeEvent(env, days + 1, 8, 0, 0, 0);
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").GetAndResetLastNewData(),
                fields,
                new[] {new object[] {"S4"}, new object[] {"S5"}, new object[] {"S6"}});

            env.UndeployAll();
        }

        private static void TryAssertion(
            RegressionEnvironment env,
            int days)
        {
            var subscriber = new SupportSubscriber();
            env.Statement("s0").Subscriber = subscriber;

            SendEvent(env, "S1", 0);

            // now scheduled for output
            env.SendEventBean(new SupportBean("E1", 1));
            Assert.AreEqual(0, env.Runtime.VariableService.GetVariableValue(null, "myvar"));
            Assert.IsFalse(subscriber.IsInvoked);

            SendTimeEvent(env, days, 8, 0, 1, 0);
            EPAssertionUtil.AssertEqualsExactOrder(new object[] {"S1"}, subscriber.GetAndResetLastNewData());
            Assert.AreEqual(0, env.Runtime.VariableService.GetVariableValue(null, "myvar"));
            Assert.AreEqual(1, env.Runtime.VariableService.GetVariableValue(null, "count_insert_var"));

            SendEvent(env, "S2", 0);
            SendEvent(env, "S3", 0);
            SendTimeEvent(env, days, 8, 0, 2, 0);
            SendTimeEvent(env, days, 8, 0, 3, 0);
            env.SendEventBean(new SupportBean("E2", 1));
            Assert.AreEqual(0, env.Runtime.VariableService.GetVariableValue(null, "myvar"));
            Assert.AreEqual(2, env.Runtime.VariableService.GetVariableValue(null, "count_insert_var"));

            Assert.IsFalse(subscriber.IsInvoked);
            SendTimeEvent(env, days, 8, 0, 4, 0);
            EPAssertionUtil.AssertEqualsExactOrder(new object[] {"S2", "S3"}, subscriber.GetAndResetLastNewData());
            Assert.AreEqual(0, env.Runtime.VariableService.GetVariableValue(null, "myvar"));

            SendTimeEvent(env, days, 8, 0, 5, 0);
            Assert.IsFalse(subscriber.IsInvoked);
            env.SendEventBean(new SupportBean("E1", 1));
            Assert.AreEqual(0, env.Runtime.VariableService.GetVariableValue(null, "myvar"));
            Assert.IsFalse(subscriber.IsInvoked);

            env.UndeployAll();
        }

        private static void SendTimer(
            RegressionEnvironment env,
            long timeInMSec)
        {
            env.AdvanceTime(timeInMSec);
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string symbol,
            double price)
        {
            var bean = new SupportMarketDataBean(symbol, price, 0L, null);
            env.SendEventBean(bean);
        }

        private static void SendTimeEvent(
            RegressionEnvironment env,
            int day,
            int hour,
            int minute,
            int second,
            int millis)
        {
            var dateTimeEx = DateTimeEx.GetInstance(TimeZoneInfo.Local)
                .Set(2008, 1, day, hour, minute, second)
                .SetMillis(millis);
            env.AdvanceTime(dateTimeEx.TimeInMillis);
        }

        internal class ResultSetOutputCrontabAtVariable : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // every 15 minutes 8am to 5pm
                SendTimeEvent(env, 1, 17, 10, 0, 0);
                var epl = "create variable int VFREQ = 15;\n" +
                          "create variable int VMIN = 8;\n" +
                          "create variable int VMAX = 17;\n" +
                          "@Name('s0') select * from SupportMarketDataBean#lastevent output at (*/VFREQ, VMIN:VMAX, *, *, *);\n";
                env.CompileDeploy(epl).AddListener("s0");

                TryAssertionCrontab(env, 1);
            }
        }

        internal class ResultSetOutputCrontabAt : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // every 15 minutes 8am to 5pm
                SendTimeEvent(env, 1, 17, 10, 0, 0);
                var expression =
                    "@Name('s0') select * from SupportMarketDataBean#lastevent output at (*/15, 8:17, *, *, *)";
                env.CompileDeploy(expression).AddListener("s0");

                TryAssertionCrontab(env, 1);
            }
        }

        internal class ResultSetOutputCrontabAtOMCreate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // every 15 minutes 8am to 5pm
                SendTimeEvent(env, 1, 17, 10, 0, 0);
                var expression = "select * from SupportMarketDataBean#lastevent output at (*/15, 8:17, *, *, *)";

                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.CreateWildcard();
                model.FromClause = FromClause.Create(FilterStream.Create("SupportMarketDataBean").AddView("lastevent"));
                Expression[] crontabParams = {
                    Expressions.CrontabScheduleFrequency(15),
                    Expressions.CrontabScheduleRange(8, 17),
                    Expressions.CrontabScheduleWildcard(),
                    Expressions.CrontabScheduleWildcard(),
                    Expressions.CrontabScheduleWildcard()
                };
                model.OutputLimitClause = OutputLimitClause.CreateSchedule(crontabParams);

                var epl = model.ToEPL();
                Assert.AreEqual(expression, epl);

                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model).AddListener("s0");

                TryAssertionCrontab(env, 1);
            }
        }

        internal class ResultSetOutputCrontabAtOMCompile : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // every 15 minutes 8am to 5pm
                SendTimeEvent(env, 1, 17, 10, 0, 0);
                var expression =
                    "@Name('s0') select * from SupportMarketDataBean#lastevent output at (*/15, 8:17, *, *, *)";

                env.EplToModelCompileDeploy(expression).AddListener("s0");

                TryAssertionCrontab(env, 1);
            }
        }

        internal class ResultSetOutputWhenThenExpression : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.Runtime.VariableService.SetVariableValue(null, "myvar", 0);
                SendTimeEvent(env, 1, 8, 0, 0, 0);
                env.CompileDeploy("on SupportBean set myvar = IntPrimitive");

                var expression =
                    "@Name('s0') select Symbol from SupportMarketDataBean#length(2) output when myvar=1 then set myvar=0, count_insert_var=count_insert";
                env.CompileDeploy(expression).AddListener("s0");
                TryAssertion(env, 1);
                env.UndeployAll();
            }
        }

        internal class ResultSetOutputWhenThenExpressionSODA : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.Runtime.VariableService.SetVariableValue(null, "myvar", 0);
                SendTimeEvent(env, 1, 8, 0, 0, 0);
                env.CompileDeploy("on SupportBean set myvar = IntPrimitive");

                var expression =
                    "@Name('s0') select Symbol from SupportMarketDataBean#length(2) output when myvar=1 then set myvar=0, count_insert_var=count_insert";
                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.Create("Symbol");
                model.FromClause = FromClause.Create(
                    FilterStream.Create("SupportMarketDataBean").AddView("length", Expressions.Constant(2)));
                model.OutputLimitClause = OutputLimitClause.Create(Expressions.Eq("myvar", 1))
                    .WithAddThenAssignment(Expressions.Eq(Expressions.Property("myvar"), Expressions.Constant(0)))
                    .WithAddThenAssignment(
                        Expressions.Eq(Expressions.Property("count_insert_var"), Expressions.Property("count_insert")));
                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                var epl = model.ToEPL();
                Assert.AreEqual(expression, epl);
                env.Runtime.VariableService.SetVariableValue(null, "myvar", 0);
                env.CompileDeploy(model).AddListener("s0");

                env.UndeployAll();
            }
        }

        internal class ResultSetOutputWhenThenSameVarTwice : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // test same variable referenced multiple times JIRA-386
                SendTimer(env, 0);
                env.CompileDeploy("@Name('s1') select * from SupportMarketDataBean output last when myvar=100")
                    .AddListener("s1");
                env.CompileDeploy("@Name('s2') select * from SupportMarketDataBean output last when myvar=100")
                    .AddListener("s2");

                env.SendEventBean(new SupportMarketDataBean("ABC", "E1", 100));
                env.SendEventBean(new SupportMarketDataBean("ABC", "E2", 100));

                SendTimer(env, 1000);
                Assert.IsFalse(env.Listener("s1").IsInvoked);
                Assert.IsFalse(env.Listener("s2").IsInvoked);

                env.Runtime.VariableService.SetVariableValue(null, "myvar", 100);
                SendTimer(env, 2000);
                Assert.IsTrue(env.Listener("s2").IsInvoked);
                Assert.IsTrue(env.Listener("s1").IsInvoked);

                env.UndeployModuleContaining("s1");
                env.UndeployModuleContaining("s2");
            }
        }

        internal class ResultSetOutputWhenThenWVariable : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // test when-then with condition triggered by output events
                SendTimeEvent(env, 2, 8, 0, 0, 0);
                var eplToDeploy = "create variable boolean varOutputTriggered = false\n;" +
                                  "@Audit @Name('s0') select * from SupportBean#lastevent output snapshot when (count_insert > 1 and varOutputTriggered = false) then set varOutputTriggered = true;";
                env.CompileDeploy(eplToDeploy).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("E2", 2));
                Assert.AreEqual("E2", env.Listener("s0").AssertOneGetNewAndReset().Get("TheString"));

                env.SendEventBean(new SupportBean("E3", 3));
                env.SendEventBean(new SupportBean("E4", 4));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Runtime.VariableService.SetVariableValue(
                    env.DeploymentId("s0"),
                    "varOutputTriggered",
                    false); // turns true right away as triggering output

                env.SendEventBean(new SupportBean("E5", 5));
                SendTimeEvent(env, 2, 8, 0, 1, 0);
                Assert.AreEqual("E5", env.Listener("s0").AssertOneGetNewAndReset().Get("TheString"));

                env.SendEventBean(new SupportBean("E6", 6));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class ResultSetOutputWhenThenWCount : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // test count_total for insert and remove
                var path = new RegressionPath();
                env.CompileDeploy("@Name('var') create variable int var_cnt_total = 3", path);
                var expressionTotal =
                    "@Name('s0') select TheString from SupportBean#length(2) output when count_insert_total = var_cnt_total or count_remove_total > 2";
                env.CompileDeploy(expressionTotal, path).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 1));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("E3", 1));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    "TheString".SplitCsv(),
                    new[] {new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}});

                env.Runtime.VariableService.SetVariableValue(env.DeploymentId("var"), "var_cnt_total", -1);

                env.SendEventBean(new SupportBean("E4", 1));
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                env.SendEventBean(new SupportBean("E5", 1));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    "TheString".SplitCsv(),
                    new[] {new object[] {"E4"}, new object[] {"E5"}});

                env.UndeployAll();
            }
        }

        internal class ResultSetOutputWhenExpression : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimeEvent(env, 1, 8, 0, 0, 0);
                env.CompileDeploy("on SupportBean set myint = IntPrimitive, mystring = TheString");

                var expression =
                    "@Name('s0') select Symbol from SupportMarketDataBean#length(2) output when myint = 1 and mystring like 'F%'";
                env.CompileDeploy(expression);
                var stmt = env.Statement("s0");
                var subscriber = new SupportSubscriber();
                stmt.Subscriber = subscriber;

                SendEvent(env, "S1", 0);

                env.SendEventBean(new SupportBean("E1", 1));
                Assert.AreEqual(1, env.Runtime.VariableService.GetVariableValue(null, "myint"));
                Assert.AreEqual("E1", env.Runtime.VariableService.GetVariableValue(null, "mystring"));

                SendEvent(env, "S2", 0);
                SendTimeEvent(env, 1, 8, 0, 1, 0);
                Assert.IsFalse(subscriber.IsInvoked);

                env.SendEventBean(new SupportBean("F1", 0));
                Assert.AreEqual(0, env.Runtime.VariableService.GetVariableValue(null, "myint"));
                Assert.AreEqual("F1", env.Runtime.VariableService.GetVariableValue(null, "mystring"));

                SendTimeEvent(env, 1, 8, 0, 2, 0);
                SendEvent(env, "S3", 0);
                Assert.IsFalse(subscriber.IsInvoked);

                env.SendEventBean(new SupportBean("F2", 1));
                Assert.AreEqual(1, env.Runtime.VariableService.GetVariableValue(null, "myint"));
                Assert.AreEqual("F2", env.Runtime.VariableService.GetVariableValue(null, "mystring"));

                SendEvent(env, "S4", 0);
                EPAssertionUtil.AssertEqualsExactOrder(
                    new object[] {"S1", "S2", "S3", "S4"},
                    subscriber.GetAndResetLastNewData());

                env.UndeployAll();
            }
        }

        internal class ResultSetOutputWhenBuiltInCountInsert : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var expression =
                    "@Name('s0') select Symbol from SupportMarketDataBean#length(2) output when count_insert >= 3";
                var stmt = env.CompileDeploy(expression).Statement("s0");
                var subscriber = new SupportSubscriber();
                stmt.Subscriber = subscriber;

                SendEvent(env, "S1", 0);
                SendEvent(env, "S2", 0);
                Assert.IsFalse(subscriber.IsInvoked);

                SendEvent(env, "S3", 0);
                EPAssertionUtil.AssertEqualsExactOrder(
                    new object[] {"S1", "S2", "S3"},
                    subscriber.GetAndResetLastNewData());

                SendEvent(env, "S4", 0);
                SendEvent(env, "S5", 0);
                Assert.IsFalse(subscriber.IsInvoked);

                SendEvent(env, "S6", 0);
                EPAssertionUtil.AssertEqualsExactOrder(
                    new object[] {"S4", "S5", "S6"},
                    subscriber.GetAndResetLastNewData());

                SendEvent(env, "S7", 0);
                Assert.IsFalse(subscriber.IsInvoked);

                env.UndeployAll();
            }
        }

        internal class ResultSetOutputWhenBuiltInCountRemove : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var expression =
                    "@Name('s0') select Symbol from SupportMarketDataBean#length(2) output when count_remove >= 2";
                var stmt = env.CompileDeploy(expression).Statement("s0");
                var subscriber = new SupportSubscriber();
                stmt.Subscriber = subscriber;

                SendEvent(env, "S1", 0);
                SendEvent(env, "S2", 0);
                SendEvent(env, "S3", 0);
                Assert.IsFalse(subscriber.IsInvoked);

                SendEvent(env, "S4", 0);
                EPAssertionUtil.AssertEqualsExactOrder(
                    new object[] {"S1", "S2", "S3", "S4"},
                    subscriber.GetAndResetLastNewData());

                SendEvent(env, "S5", 0);
                Assert.IsFalse(subscriber.IsInvoked);

                SendEvent(env, "S6", 0);
                EPAssertionUtil.AssertEqualsExactOrder(new object[] {"S5", "S6"}, subscriber.GetAndResetLastNewData());

                SendEvent(env, "S7", 0);
                Assert.IsFalse(subscriber.IsInvoked);

                env.UndeployAll();
            }
        }

        internal class ResultSetOutputWhenBuiltInLastTimestamp : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimeEvent(env, 1, 8, 0, 0, 0);
                var expression =
                    "@Name('s0') select Symbol from SupportMarketDataBean#length(2) output when current_timestamp - last_output_timestamp >= 2000";
                var stmt = env.CompileDeploy(expression).Statement("s0");
                var subscriber = new SupportSubscriber();
                stmt.Subscriber = subscriber;

                SendEvent(env, "S1", 0);

                SendTimeEvent(env, 1, 8, 0, 1, 900);
                SendEvent(env, "S2", 0);

                SendTimeEvent(env, 1, 8, 0, 2, 0);
                Assert.IsFalse(subscriber.IsInvoked);

                SendEvent(env, "S3", 0);
                EPAssertionUtil.AssertEqualsExactOrder(
                    new object[] {"S1", "S2", "S3"},
                    subscriber.GetAndResetLastNewData());

                SendTimeEvent(env, 1, 8, 0, 3, 0);
                SendEvent(env, "S4", 0);

                SendTimeEvent(env, 1, 8, 0, 3, 500);
                SendEvent(env, "S5", 0);
                Assert.IsFalse(subscriber.IsInvoked);

                SendTimeEvent(env, 1, 8, 0, 4, 0);
                SendEvent(env, "S6", 0);
                EPAssertionUtil.AssertEqualsExactOrder(
                    new object[] {"S4", "S5", "S6"},
                    subscriber.GetAndResetLastNewData());

                env.UndeployAll();
            }
        }

        internal class ResultSetInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select * from SupportMarketDataBean output when myvardummy",
                    "The when-trigger expression in the OUTPUT WHEN clause must return a boolean-type value [select * from SupportMarketDataBean output when myvardummy]");

                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select * from SupportMarketDataBean output when true then set myvardummy = 'b'",
                    "Error in the output rate limiting clause: Variable 'myvardummy' of declared type System.Integer cannot be assigned a value of type System.String [select * from SupportMarketDataBean output when true then set myvardummy = 'b']");

                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select * from SupportMarketDataBean output when true then set myvardummy = sum(myvardummy)",
                    "An aggregate function may not appear in a OUTPUT LIMIT clause [select * from SupportMarketDataBean output when true then set myvardummy = sum(myvardummy)]");

                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select * from SupportMarketDataBean output when true then set 1",
                    "Error in the output rate limiting clause: Missing variable assignment expression in assignment number 0 [select * from SupportMarketDataBean output when true then set 1]");

                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select * from SupportMarketDataBean output when sum(price) > 0",
                    "Failed to valIdate output limit expression '(sum(price))>0': Property named 'price' is not valId in any stream [select * from SupportMarketDataBean output when sum(price) > 0]");

                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select * from SupportMarketDataBean output when sum(count_insert) > 0",
                    "An aggregate function may not appear in a OUTPUT LIMIT clause [select * from SupportMarketDataBean output when sum(count_insert) > 0]");

                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select * from SupportMarketDataBean output when prev(1, count_insert) = 0",
                    "Failed to valIdate output limit expression 'prev(1,count_insert)=0': Previous function cannot be used in this context [select * from SupportMarketDataBean output when prev(1, count_insert) = 0]");

                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select TheString, count(*) from SupportBean#length(2) group by TheString output all every 0 seconds",
                    "InvalId time period expression returns a zero or negative time interval [select TheString, count(*) from SupportBean#length(2) group by TheString output all every 0 seconds]");
            }
        }
    }
} // end of namespace