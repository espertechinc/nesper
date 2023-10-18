///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Globalization;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.resultset.outputlimit
{
	public class ResultSetOutputLimitCrontabWhen
	{

		public static ICollection<RegressionExecution> Executions()
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

		private class ResultSetOutputCrontabAtVariable : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{

				// every 15 minutes 8am to 5pm
				SendTimeEvent(env, 1, 17, 10, 0, 0);
				var epl = "create variable int VFREQ = 15;\n" +
				          "create variable int VMIN = 8;\n" +
				          "create variable int VMAX = 17;\n" +
				          "@name('s0') select * from SupportMarketDataBean#lastevent output at (*/VFREQ, VMIN:VMAX, *, *, *);\n";
				env.CompileDeploy(epl).AddListener("s0");

				TryAssertionCrontab(env, 1);
			}
		}

		private class ResultSetOutputCrontabAt : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{

				// every 15 minutes 8am to 5pm
				SendTimeEvent(env, 1, 17, 10, 0, 0);
				var expression =
					"@name('s0') select * from SupportMarketDataBean#lastevent output at (*/15, 8:17, *, *, *)";
				env.CompileDeploy(expression).AddListener("s0");

				TryAssertionCrontab(env, 1);
			}
		}

		private class ResultSetOutputCrontabAtOMCreate : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{

				// every 15 minutes 8am to 5pm
				SendTimeEvent(env, 1, 17, 10, 0, 0);
				var expression = "select * from SupportMarketDataBean#lastevent output at (*/15, 8:17, *, *, *)";

				var model = new EPStatementObjectModel();
				model.SelectClause = SelectClause.CreateWildcard();
				model.FromClause = FromClause.Create(FilterStream.Create("SupportMarketDataBean").AddView("lastevent"));
				var crontabParams = new Expression[] {
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

		private class ResultSetOutputCrontabAtOMCompile : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				// every 15 minutes 8am to 5pm
				SendTimeEvent(env, 1, 17, 10, 0, 0);
				var expression =
					"@name('s0') select * from SupportMarketDataBean#lastevent output at (*/15, 8:17, *, *, *)";

				env.EplToModelCompileDeploy(expression).AddListener("s0");

				TryAssertionCrontab(env, 1);
			}
		}

		private class ResultSetOutputWhenThenExpression : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				env.RuntimeSetVariable(null, "myvar", 0);
				SendTimeEvent(env, 1, 8, 0, 0, 0);
				env.CompileDeploy("on SupportBean set myvar = intPrimitive");

				var expression =
					"@name('s0') select symbol from SupportMarketDataBean#length(2) output when myvar=1 then set myvar=0, count_insert_var=count_insert";
				env.CompileDeploy(expression).AddListener("s0").SetSubscriber("s0");
				TryAssertion(env, 1);
				env.UndeployAll();
			}
		}

		private class ResultSetOutputWhenThenExpressionSODA : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				env.RuntimeSetVariable(null, "myvar", 0);
				SendTimeEvent(env, 1, 8, 0, 0, 0);
				env.CompileDeploy("on SupportBean set myvar = intPrimitive");

				var expression =
					"@name('s0') select symbol from SupportMarketDataBean#length(2) output when myvar=1 then set myvar=0, count_insert_var=count_insert";
				var model = new EPStatementObjectModel();
				model.SelectClause = SelectClause.Create("symbol");
				model.FromClause = FromClause.Create(
					FilterStream.Create("SupportMarketDataBean").AddView("length", Expressions.Constant(2)));
				model.OutputLimitClause = OutputLimitClause.Create(Expressions.Eq("myvar", 1))
					.WithAddThenAssignment(Expressions.Eq(Expressions.Property("myvar"), Expressions.Constant(0)))
					.WithAddThenAssignment(
						Expressions.Eq(Expressions.Property("count_insert_var"), Expressions.Property("count_insert")));
				model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
				var epl = model.ToEPL();
				Assert.AreEqual(expression, epl);
				env.RuntimeSetVariable(null, "myvar", 0);
				env.CompileDeploy(model).AddListener("s0");

				env.UndeployAll();
			}
		}

		private class ResultSetOutputWhenThenSameVarTwice : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				// test same variable referenced multiple times JIRA-386
				SendTimer(env, 0);
				env.CompileDeploy("@name('s1') select * from SupportMarketDataBean output last when myvar=100")
					.AddListener("s1");
				env.CompileDeploy("@name('s2') select * from SupportMarketDataBean output last when myvar=100")
					.AddListener("s2");

				env.SendEventBean(new SupportMarketDataBean("ABC", "E1", 100));
				env.SendEventBean(new SupportMarketDataBean("ABC", "E2", 100));

				SendTimer(env, 1000);
				env.AssertListenerNotInvoked("s1");
				env.AssertListenerNotInvoked("s2");

				env.RuntimeSetVariable(null, "myvar", 100);
				SendTimer(env, 2000);
				env.AssertListenerInvoked("s1");
				env.AssertListenerInvoked("s2");

				env.UndeployModuleContaining("s1");
				env.UndeployModuleContaining("s2");
			}
		}

		private class ResultSetOutputWhenThenWVariable : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{

				// test when-then with condition triggered by output events
				SendTimeEvent(env, 2, 8, 0, 0, 0);
				var eplToDeploy = "create variable boolean varOutputTriggered = false\n;" +
				                  "@Audit @Name('s0') select * from SupportBean#lastevent output snapshot when (count_insert > 1 and varOutputTriggered = false) then set varOutputTriggered = true;";
				env.CompileDeploy(eplToDeploy).AddListener("s0");

				env.SendEventBean(new SupportBean("E1", 1));
				env.AssertListenerNotInvoked("s0");

				env.SendEventBean(new SupportBean("E2", 2));
				env.AssertEqualsNew("s0", "theString", "E2");

				env.SendEventBean(new SupportBean("E3", 3));
				env.SendEventBean(new SupportBean("E4", 4));
				env.AssertListenerNotInvoked("s0");

				env.RuntimeSetVariable("s0", "varOutputTriggered", false); // turns true right away as triggering output

				env.SendEventBean(new SupportBean("E5", 5));
				SendTimeEvent(env, 2, 8, 0, 1, 0);
				env.AssertEqualsNew("s0", "theString", "E5");

				env.SendEventBean(new SupportBean("E6", 6));
				env.AssertListenerNotInvoked("s0");

				env.UndeployAll();
			}
		}

		private class ResultSetOutputWhenThenWCount : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				// test count_total for insert and remove
				var path = new RegressionPath();
				env.CompileDeploy("@name('var') @public create variable int var_cnt_total = 3", path);
				var expressionTotal =
					"@name('s0') select theString from SupportBean#length(2) output when count_insert_total = var_cnt_total or count_remove_total > 2";
				env.CompileDeploy(expressionTotal, path).AddListener("s0");

				env.SendEventBean(new SupportBean("E1", 1));
				env.SendEventBean(new SupportBean("E2", 1));
				env.AssertListenerNotInvoked("s0");

				env.SendEventBean(new SupportBean("E3", 1));
				env.AssertPropsPerRowLastNew(
					"s0",
					"theString".SplitCsv(),
					new object[][] { new object[] { "E1" }, new object[] { "E2" }, new object[] { "E3" } });

				env.RuntimeSetVariable("var", "var_cnt_total", -1);

				env.SendEventBean(new SupportBean("E4", 1));
				env.AssertListenerNotInvoked("s0");

				env.SendEventBean(new SupportBean("E5", 1));
				env.AssertPropsPerRowLastNew(
					"s0",
					"theString".SplitCsv(),
					new object[][] { new object[] { "E4" }, new object[] { "E5" } });

				env.UndeployAll();
			}
		}

		private class ResultSetOutputWhenExpression : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				SendTimeEvent(env, 1, 8, 0, 0, 0);
				env.CompileDeploy("on SupportBean set myint = intPrimitive, mystring = theString");

				var expression =
					"@name('s0') select symbol from SupportMarketDataBean#length(2) output when myint = 1 and mystring like 'F%'";
				env.CompileDeploy(expression).SetSubscriber("s0");
				SendEvent(env, "S1", 0);

				env.SendEventBean(new SupportBean("E1", 1));
				env.AssertRuntime(
					runtime => {
						Assert.AreEqual(1, runtime.VariableService.GetVariableValue(null, "myint"));
						Assert.AreEqual("E1", runtime.VariableService.GetVariableValue(null, "mystring"));
					});

				SendEvent(env, "S2", 0);
				SendTimeEvent(env, 1, 8, 0, 1, 0);
				env.AssertSubscriber("s0", subscriber => Assert.IsFalse(subscriber.IsInvoked));

				env.SendEventBean(new SupportBean("F1", 0));
				env.AssertRuntime(
					runtime => {
						Assert.AreEqual(0, runtime.VariableService.GetVariableValue(null, "myint"));
						Assert.AreEqual("F1", runtime.VariableService.GetVariableValue(null, "mystring"));
					});

				SendTimeEvent(env, 1, 8, 0, 2, 0);
				SendEvent(env, "S3", 0);
				env.AssertSubscriber("s0", subscriber => Assert.IsFalse(subscriber.IsInvoked));

				env.SendEventBean(new SupportBean("F2", 1));
				env.AssertRuntime(
					runtime => {
						Assert.AreEqual(1, runtime.VariableService.GetVariableValue(null, "myint"));
						Assert.AreEqual("F2", runtime.VariableService.GetVariableValue(null, "mystring"));
					});

				SendEvent(env, "S4", 0);
				env.AssertSubscriber(
					"s0",
					subscriber => EPAssertionUtil.AssertEqualsExactOrder(
						new object[] { "S1", "S2", "S3", "S4" },
						subscriber.GetAndResetLastNewData()));

				env.UndeployAll();
			}
		}

		private class ResultSetOutputWhenBuiltInCountInsert : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var expression =
					"@name('s0') select symbol from SupportMarketDataBean#length(2) output when count_insert >= 3";
				env.CompileDeploy(expression).SetSubscriber("s0");

				SendEvent(env, "S1", 0);
				SendEvent(env, "S2", 0);
				env.AssertSubscriber("s0", subscriber => Assert.IsFalse(subscriber.IsInvoked));

				SendEvent(env, "S3", 0);
				env.AssertSubscriber(
					"s0",
					subscriber => EPAssertionUtil.AssertEqualsExactOrder(
						new object[] { "S1", "S2", "S3" },
						subscriber.GetAndResetLastNewData()));

				SendEvent(env, "S4", 0);
				SendEvent(env, "S5", 0);
				env.AssertSubscriber("s0", subscriber => Assert.IsFalse(subscriber.IsInvoked));

				SendEvent(env, "S6", 0);
				env.AssertSubscriber(
					"s0",
					subscriber => EPAssertionUtil.AssertEqualsExactOrder(
						new object[] { "S4", "S5", "S6" },
						subscriber.GetAndResetLastNewData()));

				SendEvent(env, "S7", 0);
				env.AssertSubscriber("s0", subscriber => Assert.IsFalse(subscriber.IsInvoked));

				env.UndeployAll();
			}
		}

		private class ResultSetOutputWhenBuiltInCountRemove : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var expression =
					"@name('s0') select symbol from SupportMarketDataBean#length(2) output when count_remove >= 2";
				env.CompileDeploy(expression).SetSubscriber("s0");

				SendEvent(env, "S1", 0);
				SendEvent(env, "S2", 0);
				SendEvent(env, "S3", 0);
				env.AssertSubscriber("s0", subscriber => Assert.IsFalse(subscriber.IsInvoked));

				SendEvent(env, "S4", 0);
				env.AssertSubscriber(
					"s0",
					subscriber => EPAssertionUtil.AssertEqualsExactOrder(
						new object[] { "S1", "S2", "S3", "S4" },
						subscriber.GetAndResetLastNewData()));

				SendEvent(env, "S5", 0);
				env.AssertSubscriber("s0", subscriber => Assert.IsFalse(subscriber.IsInvoked));

				SendEvent(env, "S6", 0);
				env.AssertSubscriber(
					"s0",
					subscriber => EPAssertionUtil.AssertEqualsExactOrder(
						new object[] { "S5", "S6" },
						subscriber.GetAndResetLastNewData()));

				SendEvent(env, "S7", 0);
				env.AssertSubscriber("s0", subscriber => Assert.IsFalse(subscriber.IsInvoked));

				env.UndeployAll();
			}
		}

		private class ResultSetOutputWhenBuiltInLastTimestamp : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				SendTimeEvent(env, 1, 8, 0, 0, 0);
				var expression =
					"@name('s0') select symbol from SupportMarketDataBean#length(2) output when current_timestamp - last_output_timestamp >= 2000";
				env.CompileDeploy(expression).SetSubscriber("s0");

				SendEvent(env, "S1", 0);

				SendTimeEvent(env, 1, 8, 0, 1, 900);
				SendEvent(env, "S2", 0);

				SendTimeEvent(env, 1, 8, 0, 2, 0);
				env.AssertSubscriber("s0", subscriber => Assert.IsFalse(subscriber.IsInvoked));

				SendEvent(env, "S3", 0);
				env.AssertSubscriber(
					"s0",
					subscriber => EPAssertionUtil.AssertEqualsExactOrder(
						new object[] { "S1", "S2", "S3" },
						subscriber.GetAndResetLastNewData()));

				SendTimeEvent(env, 1, 8, 0, 3, 0);
				SendEvent(env, "S4", 0);

				SendTimeEvent(env, 1, 8, 0, 3, 500);
				SendEvent(env, "S5", 0);
				env.AssertSubscriber("s0", subscriber => Assert.IsFalse(subscriber.IsInvoked));

				SendTimeEvent(env, 1, 8, 0, 4, 0);
				SendEvent(env, "S6", 0);
				env.AssertSubscriber(
					"s0",
					subscriber => EPAssertionUtil.AssertEqualsExactOrder(
						new object[] { "S4", "S5", "S6" },
						subscriber.GetAndResetLastNewData()));

				env.UndeployAll();
			}
		}

		private class ResultSetInvalid : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				env.TryInvalidCompile(
					"select * from SupportMarketDataBean output when myvardummy",
					"The when-trigger expression in the OUTPUT WHEN clause must return a boolean-type value [select * from SupportMarketDataBean output when myvardummy]");

				env.TryInvalidCompile(
					"select * from SupportMarketDataBean output when true then set myvardummy = 'b'",
					"Failed to validate the output rate limiting clause: Failed to validate assignment expression 'myvardummy=\"b\"': Variable 'myvardummy' of declared type Integer cannot be assigned a value of type String [select * from SupportMarketDataBean output when true then set myvardummy = 'b']");

				env.TryInvalidCompile(
					"select * from SupportMarketDataBean output when true then set myvardummy = sum(myvardummy)",
					"Aggregation functions may not be used within update-set [select * from SupportMarketDataBean output when true then set myvardummy = sum(myvardummy)]");

				env.TryInvalidCompile(
					"select * from SupportMarketDataBean output when true then set 1",
					"Failed to validate the output rate limiting clause: Failed to validate assignment expression '1': Assignment expression must receive a single variable value");

				env.TryInvalidCompile(
					"select * from SupportMarketDataBean output when sum(price) > 0",
					"Failed to validate output limit expression '(sum(price))>0': Property named 'price' is not valid in any stream [select * from SupportMarketDataBean output when sum(price) > 0]");

				env.TryInvalidCompile(
					"select * from SupportMarketDataBean output when sum(count_insert) > 0",
					"An aggregate function may not appear in a OUTPUT LIMIT clause [select * from SupportMarketDataBean output when sum(count_insert) > 0]");

				env.TryInvalidCompile(
					"select * from SupportMarketDataBean output when prev(1, count_insert) = 0",
					"Failed to validate output limit expression 'prev(1,count_insert)=0': Previous function cannot be used in this context [select * from SupportMarketDataBean output when prev(1, count_insert) = 0]");

				env.TryInvalidCompile(
					"select theString, count(*) from SupportBean#length(2) group by theString output all every 0 seconds",
					"Invalid time period expression returns a zero or negative time interval [select theString, count(*) from SupportBean#length(2) group by theString output all every 0 seconds]");
			}
		}

		private static void TryAssertionCrontab(
			RegressionEnvironment env,
			int days)
		{
			var fields = "symbol".SplitCsv();
			SendEvent(env, "S1", 0);
			env.AssertListenerNotInvoked("s0");

			SendTimeEvent(env, days, 17, 14, 59, 0);
			SendEvent(env, "S2", 0);
			env.AssertListenerNotInvoked("s0");

			SendTimeEvent(env, days, 17, 15, 0, 0);
			env.AssertPropsPerRowLastNew("s0", fields, new object[][] { new object[] { "S1" }, new object[] { "S2" } });

			SendTimeEvent(env, days, 17, 18, 0, 0);
			SendEvent(env, "S3", 0);
			env.AssertListenerNotInvoked("s0");

			SendTimeEvent(env, days, 17, 30, 0, 0);
			env.AssertPropsPerRowLastNew("s0", fields, new object[][] { new object[] { "S3" } });

			SendTimeEvent(env, days, 17, 35, 0, 0);
			SendTimeEvent(env, days, 17, 45, 0, 0);
			env.AssertPropsPerRowLastNew("s0", fields, null);

			SendEvent(env, "S4", 0);
			SendEvent(env, "S5", 0);
			SendTimeEvent(env, days, 18, 0, 0, 0);
			env.AssertListenerNotInvoked("s0");

			SendTimeEvent(env, days, 18, 1, 0, 0);
			SendEvent(env, "S6", 0);

			SendTimeEvent(env, days, 18, 15, 0, 0);
			env.AssertListenerNotInvoked("s0");

			SendTimeEvent(env, days + 1, 7, 59, 59, 0);
			env.AssertListenerNotInvoked("s0");

			SendTimeEvent(env, days + 1, 8, 0, 0, 0);
			env.AssertPropsPerRowLastNew(
				"s0",
				fields,
				new object[][] { new object[] { "S4" }, new object[] { "S5" }, new object[] { "S6" } });

			env.UndeployAll();
		}

		private static void TryAssertion(
			RegressionEnvironment env,
			int days)
		{

			SendEvent(env, "S1", 0);

			// now scheduled for output
			env.SendEventBean(new SupportBean("E1", 1));
			env.AssertRuntime(
				runtime => Assert.AreEqual(0, env.Runtime.VariableService.GetVariableValue(null, "myvar")));
			env.AssertSubscriber("s0", subscriber => Assert.IsFalse(subscriber.IsInvoked));

			SendTimeEvent(env, days, 8, 0, 1, 0);
			env.AssertSubscriber(
				"s0",
				subscriber => EPAssertionUtil.AssertEqualsExactOrder(
					new object[] { "S1" },
					subscriber.GetAndResetLastNewData()));
			env.AssertRuntime(
				runtime => {
					Assert.AreEqual(0, runtime.VariableService.GetVariableValue(null, "myvar"));
					Assert.AreEqual(1, runtime.VariableService.GetVariableValue(null, "count_insert_var"));
				});

			SendEvent(env, "S2", 0);
			SendEvent(env, "S3", 0);
			SendTimeEvent(env, days, 8, 0, 2, 0);
			SendTimeEvent(env, days, 8, 0, 3, 0);
			env.SendEventBean(new SupportBean("E2", 1));
			env.AssertRuntime(
				runtime => {
					Assert.AreEqual(0, runtime.VariableService.GetVariableValue(null, "myvar"));
					Assert.AreEqual(2, runtime.VariableService.GetVariableValue(null, "count_insert_var"));
				});

			env.AssertSubscriber("s0", subscriber => Assert.IsFalse(subscriber.IsInvoked));
			SendTimeEvent(env, days, 8, 0, 4, 0);
			env.AssertSubscriber(
				"s0",
				subscriber => EPAssertionUtil.AssertEqualsExactOrder(
					new object[] { "S2", "S3" },
					subscriber.GetAndResetLastNewData()));
			env.AssertRuntime(runtime => Assert.AreEqual(0, runtime.VariableService.GetVariableValue(null, "myvar")));

			SendTimeEvent(env, days, 8, 0, 5, 0);
			env.AssertSubscriber("s0", subscriber => Assert.IsFalse(subscriber.IsInvoked));
			env.SendEventBean(new SupportBean("E1", 1));
			env.AssertRuntime(runtime => Assert.AreEqual(0, runtime.VariableService.GetVariableValue(null, "myvar")));
			env.AssertSubscriber("s0", subscriber => Assert.IsFalse(subscriber.IsInvoked));

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
			var dateTimeEx = DateTimeEx.GetInstance(TimeZoneInfo.Utc)
				.Set(2008, 1, day, hour, minute, second)
				.SetMillis(millis);
			env.AdvanceTime(dateTimeEx.UtcMillis);
		}
	}
} // end of namespace
