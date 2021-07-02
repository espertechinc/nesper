///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Numerics;

using com.espertech.esper.common.client.scopetest;
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
			execs ??= new List<RegressionExecution>();
			execs.Add(new ExprCoreBigNumberCastAndUDF());
			return execs;
		}

		public static IList<RegressionExecution> WithJoin(IList<RegressionExecution> execs = null)
		{
			execs ??= new List<RegressionExecution>();
			execs.Add(new ExprCoreBigNumberJoin());
			return execs;
		}

		public static IList<RegressionExecution> WithFilterEquals(IList<RegressionExecution> execs = null)
		{
			execs ??= new List<RegressionExecution>();
			execs.Add(new ExprCoreBigNumberFilterEquals());
			return execs;
		}

		public static IList<RegressionExecution> WithMinMax(IList<RegressionExecution> execs = null)
		{
			execs ??= new List<RegressionExecution>();
			execs.Add(new ExprCoreBigNumberMinMax());
			return execs;
		}

		public static IList<RegressionExecution> WithAggregation(IList<RegressionExecution> execs = null)
		{
			execs ??= new List<RegressionExecution>();
			execs.Add(new ExprCoreBigNumberAggregation());
			return execs;
		}

		public static IList<RegressionExecution> WithMath(IList<RegressionExecution> execs = null)
		{
			execs ??= new List<RegressionExecution>();
			execs.Add(new ExprCoreBigNumberMath());
			return execs;
		}

		public static IList<RegressionExecution> WithIn(IList<RegressionExecution> execs = null)
		{
			execs ??= new List<RegressionExecution>();
			execs.Add(new ExprCoreBigNumberIn());
			return execs;
		}

		public static IList<RegressionExecution> WithBetween(IList<RegressionExecution> execs = null)
		{
			execs ??= new List<RegressionExecution>();
			execs.Add(new ExprCoreBigNumberBetween());
			return execs;
		}

		public static IList<RegressionExecution> WithRelOp(IList<RegressionExecution> execs = null)
		{
			execs ??= new List<RegressionExecution>();
			execs.Add(new ExprCoreBigNumberRelOp());
			return execs;
		}

		public static IList<RegressionExecution> WithEquals(IList<RegressionExecution> execs = null)
		{
			execs ??= new List<RegressionExecution>();
			execs.Add(new ExprCoreBigNumberEquals());
			return execs;
		}

		private class ExprCoreBigNumberEquals : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				AtomicLong milestone = new AtomicLong();

				// test equals BigDecimal
				var epl = "@Name('s0') select * from SupportBeanNumeric where DecimalOne = 1 or DecimalOne = IntOne or DecimalOne = DoubleOne";
				env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());

				SendBigNumEvent(env, -1, 1m);
				Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());
				SendBigNumEvent(env, -1, 2m);
				Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

				env.SendEventBean(new SupportBeanNumeric(2, 0, null, 2m, 0, 0));
				Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());
				env.SendEventBean(new SupportBeanNumeric(3, 0, null, 2m, 0, 0));
				Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

				env.SendEventBean(new SupportBeanNumeric(0, 0, null, 3m, 3d, 0));
				Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());
				env.SendEventBean(new SupportBeanNumeric(0, 0, null, 3.9999m, 4d, 0));
				Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

				// test equals BigInteger
				env.UndeployAll();
				epl = "@Name('s0') select * from SupportBeanNumeric where DecimalOne = Bigint or Bigint = IntOne or Bigint = 1";
				env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());

				env.SendEventBean(new SupportBeanNumeric(0, 0, new BigInteger(2), 2m, 0, 0));
				Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());
				env.SendEventBean(new SupportBeanNumeric(0, 0, new BigInteger(3), 2m, 0, 0));
				Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

				env.SendEventBean(new SupportBeanNumeric(2, 0, new BigInteger(2), null, 0, 0));
				Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());
				env.SendEventBean(new SupportBeanNumeric(3, 0, new BigInteger(2), null, 0, 0));
				Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

				env.SendEventBean(new SupportBeanNumeric(0, 0, BigInteger.One, null, 0, 0));
				Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());
				env.SendEventBean(new SupportBeanNumeric(0, 0, new BigInteger(4), null, 0, 0));
				Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

				env.UndeployAll();
			}
		}

		private class ExprCoreBigNumberRelOp : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				// relational op tests handled by relational op unit test
				var epl = "@Name('s0') select * from SupportBeanNumeric where DecimalOne < 10 and Bigint > 10";
				env.CompileDeploy(epl).AddListener("s0");

				SendBigNumEvent(env, 10, 10m);
				Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

				SendBigNumEvent(env, 11, 9m);
				Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());
				env.UndeployAll();

				epl = "@Name('s0') select * from SupportBeanNumeric where DecimalOne < 10.0";
				env.CompileDeployAddListenerMile(epl, "s0", 1);

				SendBigNumEvent(env, 0, 11m);
				Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

				env.SendEventBean(new SupportBeanNumeric(null, 9.999m));
				Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());
				env.UndeployAll();

				// test float
				env.CompileDeployAddListenerMile("@Name('s0') select * from SupportBeanNumeric where FloatOne < 10f and FloatTwo > 10f", "s0", 1);

				env.SendEventBean(new SupportBeanNumeric(true, 1f, 20f));
				Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());
				env.SendEventBean(new SupportBeanNumeric(true, 20f, 1f));
				Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

				env.UndeployAll();
			}
		}

		private class ExprCoreBigNumberBetween : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "@Name('s0') select * from SupportBeanNumeric where DecimalOne between 10 and 20 or Bigint between 100 and 200";
				env.CompileDeploy(epl).AddListener("s0");

				SendBigNumEvent(env, 0, 9m);
				Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

				SendBigNumEvent(env, 0, 10m);
				Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());

				SendBigNumEvent(env, 99, 0m);
				Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

				SendBigNumEvent(env, 100, 0m);
				Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());

				env.UndeployAll();
			}
		}

		private class ExprCoreBigNumberIn : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "@Name('s0') select * from SupportBeanNumeric where DecimalOne in (10, 20d) or Bigint in (0x02, 3)";
				env.CompileDeploy(epl).AddListener("s0");

				SendBigNumEvent(env, 0, 9m);
				Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

				SendBigNumEvent(env, 0, 10m);
				Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());

				env.SendEventBean(new SupportBeanNumeric(null, 20m));
				Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());

				SendBigNumEvent(env, 99, 0m);
				Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

				SendBigNumEvent(env, 2, 0m);
				Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());

				SendBigNumEvent(env, 3, 0m);
				Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());

				env.UndeployAll();
			}
		}

		private class ExprCoreBigNumberMath : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "@Name('s0') select * from SupportBeanNumeric " +
				          "where DecimalOne+Bigint=100 or DecimalOne+1=2 or DecimalOne+2d=5.0 or Bigint+5L=8 or Bigint+5d=9.0";
				env.CompileDeploy(epl).AddListener("s0");

				SendBigNumEvent(env, 50, 49m);
				Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

				SendBigNumEvent(env, 50, 50m);
				Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());

				SendBigNumEvent(env, 0, 1m);
				Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());

				SendBigNumEvent(env, 0, 2m);
				Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

				SendBigNumEvent(env, 0, 3m);
				Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());

				SendBigNumEvent(env, 0, 0m);
				Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

				SendBigNumEvent(env, 3, 0m);
				Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());

				SendBigNumEvent(env, 4, 0m);
				Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());
				env.UndeployAll();

				env.CompileDeployAddListenerMile(
					"@Name('s0') select DecimalOne+Bigint as v1, DecimalOne+2 as v2, DecimalOne+3d as v3, Bigint+5L as v4, Bigint+5d as v5 " +
					" from SupportBeanNumeric",
					"s0",
					1);
				env.Listener("s0").Reset();

				Assert.AreEqual(typeof(decimal?), env.Statement("s0").EventType.GetPropertyType("v1"));
				Assert.AreEqual(typeof(decimal?), env.Statement("s0").EventType.GetPropertyType("v2"));
				Assert.AreEqual(typeof(decimal?), env.Statement("s0").EventType.GetPropertyType("v3"));
				Assert.AreEqual(typeof(BigInteger?), env.Statement("s0").EventType.GetPropertyType("v4"));
				Assert.AreEqual(typeof(decimal?), env.Statement("s0").EventType.GetPropertyType("v5"));

				SendBigNumEvent(env, 1, 2m);
				var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
				EPAssertionUtil.AssertProps(
					theEvent,
					"v1,v2,v3,v4,v5".SplitCsv(),
					3m,
					4m,
					5m,
					new BigInteger(6),
					6m);

				// test aggregation-sum, multiplication and division all together; test for ESPER-340
				env.UndeployAll();

				env.CompileDeployAddListenerMile(
					"@Name('s0') select (sum(DecimalTwo * DecimalOne)/sum(DecimalOne)) as avgRate from SupportBeanNumeric",
					"s0",
					2);
				Assert.AreEqual(typeof(decimal?), env.Statement("s0").EventType.GetPropertyType("avgRate"));
				SendBigNumEvent(env, 0, 5m);
				var avgRate = env.Listener("s0").AssertOneGetNewAndReset().Get("avgRate");
				Assert.That(avgRate, Is.InstanceOf<decimal>());
				Assert.That(avgRate, Is.EqualTo(5.0m));

				env.UndeployAll();
			}
		}

		private class ExprCoreBigNumberAggregation : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var fields = new[] {
					"sum(Bigint)",
					"sum(DecimalOne)",
					"avg(Bigint)",
					"avg(DecimalOne)",
					"median(Bigint)",
					"median(DecimalOne)",
					"stddev(Bigint)",
					"stddev(DecimalOne)",
					"avedev(Bigint)",
					"avedev(DecimalOne)",
					"min(Bigint)",
					"min(DecimalOne)"
				};
				var epl = "@Name('s0') select " + string.Join(",", fields) + " from SupportBeanNumeric";
				env.CompileDeploy(epl).AddListener("s0");

				SendBigNumEvent(env, 1, 2m);
				var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
				EPAssertionUtil.AssertProps(
					theEvent,
					fields,
					BigInteger.One, // sum(Bigint)
					2m, // sum(DecimalOne)
					BigInteger.One, // avg(Bigint)
					2m, // avg(DecimalOne)
					// TBD - do better
					1.0d, // median(Bigint)
					2.0d, // median(DecimalOne)
					null, // stddev(Bigint)
					null, // stddev(DecimalOne)
					0.0d, // avedev(Bigint)
					0.0d, // avedev(DecimalOne)
					BigInteger.One, // min(Bigint)
					2m // min(DecimalOne)
				);

				env.Milestone(1);

				SendBigNumEvent(env, 4, 5m);
				theEvent = env.Listener("s0").AssertOneGetNewAndReset();
				EPAssertionUtil.AssertProps(
					theEvent,
					fields,
					new BigInteger(5), // sum(Bigint)
					7m, // sum(DecimalOne)
					new BigInteger(2), // avg(Bigint)
					3.5m, // avg(DecimalOne)
					2.5d, // median(Bigint)
					3.5d, // median(DecimalOne)
					2.1213203435596424, // stddev(Bigint)
					2.1213203435596424, // stddev(DecimalOne)
					1.5, // avedev(Bigint)
					1.5, // avedev(DecimalOne)
					BigInteger.One, // min(Bigint)
					2m // min(DecimalOne)
					); 

				env.UndeployAll();
			}
		}

		private class ExprCoreBigNumberMinMax : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "@Name('s0') select " +
				          "min(Bigint, 10) as v1, " +
				          "min(10, Bigint) as v2, " +
				          "max(DecimalOne, 10) as v3, " +
				          "max(10, 100d, Bigint, DecimalOne) as v4 " +
				          "from SupportBeanNumeric";
				env.CompileDeploy(epl).AddListener("s0");

				var fieldList = "v1,v2,v3,v4".SplitCsv();

				SendBigNumEvent(env, 1, 2m);
				EPAssertionUtil.AssertProps(
					env.Listener("s0").AssertOneGetNewAndReset(),
					fieldList,
					BigInteger.One,
					BigInteger.One,
					10m,
					100m);

				SendBigNumEvent(env, 40, 300m);
				EPAssertionUtil.AssertProps(
					env.Listener("s0").AssertOneGetNewAndReset(),
					fieldList,
					new BigInteger(10),
					new BigInteger(10),
					300m,
					300m);

				SendBigNumEvent(env, 250, 200m);
				EPAssertionUtil.AssertProps(
					env.Listener("s0").AssertOneGetNewAndReset(),
					fieldList,
					new BigInteger(10),
					new BigInteger(10),
					200m,
					250m);

				env.UndeployAll();
			}
		}

		private class ExprCoreBigNumberFilterEquals : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var fieldList = "DecimalOne".SplitCsv();
				var epl = "@Name('s0') select DecimalOne from SupportBeanNumeric(DecimalOne = 4)";
				env.CompileDeploy(epl).AddListener("s0");

				SendBigNumEvent(env, 0, 2);
				Assert.IsFalse(env.Listener("s0").IsInvoked);

				SendBigNumEvent(env, 0, 4);
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fieldList, 4m);

				env.UndeployAll();
				env.CompileDeployAddListenerMile("@Name('s0') select DecimalOne from SupportBeanNumeric(DecimalOne = 4d)", "s0", 1);

				SendBigNumEvent(env, 0, 4m);
				Assert.IsTrue(env.Listener("s0").IsInvoked);
				env.Listener("s0").Reset();

				env.SendEventBean(new SupportBeanNumeric(new BigInteger(0), 4m));
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fieldList, 4m);

				env.UndeployAll();
				env.CompileDeployAddListenerMile("@Name('s0') select DecimalOne from SupportBeanNumeric(Bigint = 4)", "s0", 2);

				SendBigNumEvent(env, 3, 4m);
				Assert.IsFalse(env.Listener("s0").IsInvoked);

				SendBigNumEvent(env, 4, 3m);
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fieldList, 3m);

				env.UndeployAll();
			}
		}

		private class ExprCoreBigNumberJoin : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var fieldList = "Bigint,DecimalOne".SplitCsv();
				var epl = "@Name('s0') select Bigint,DecimalOne from SupportBeanNumeric#keepall(), SupportBean#keepall " +
				          "where IntPrimitive = Bigint and DoublePrimitive = DecimalOne";
				env.CompileDeploy(epl).AddListener("s0");

				SendSupportBean(env, 2, 3);
				SendBigNumEvent(env, 0, 2m);
				SendBigNumEvent(env, 2, 0m);
				Assert.IsFalse(env.Listener("s0").IsInvoked);

				env.SendEventBean(new SupportBeanNumeric(new BigInteger(2), 3m));
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fieldList, new BigInteger(2), 3m);

				env.UndeployAll();
			}
		}

		private class ExprCoreBigNumberCastAndUDF : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl =
					"@Name('s0') select " +
					"SupportStaticMethodLib.MyBigIntFunc(cast(2, BigInteger)) as v1, " +
					"SupportStaticMethodLib.MyDecimalFunc(cast(3d, decimal)) as v2 from SupportBeanNumeric";
				env.CompileDeploy(epl).AddListener("s0");

				var fieldList = "v1,v2".SplitCsv();
				SendBigNumEvent(env, 0, 2m);
				EPAssertionUtil.AssertProps(
					env.Listener("s0").AssertOneGetNewAndReset(),
					fieldList,
					new object[] {new BigInteger(2), 3m});

				env.UndeployAll();
			}
		}

		private static void SendBigNumEvent(
			RegressionEnvironment env,
			int bigInt,
			decimal decimalValue)
		{
			var bean = new SupportBeanNumeric(new BigInteger(bigInt), decimalValue);
			bean.DecimalTwo = decimalValue;
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
