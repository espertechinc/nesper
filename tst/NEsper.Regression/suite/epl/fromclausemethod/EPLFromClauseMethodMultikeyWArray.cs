///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.fromclausemethod
{
	public class EPLFromClauseMethodMultikeyWArray
	{
		public static IList<RegressionExecution> Executions()
		{
			IList<RegressionExecution> execs = new List<RegressionExecution>();
			execs.Add(new EPLFromClauseMultikeyWArrayJoinArray());
			execs.Add(new EPLFromClauseMultikeyWArrayJoinTwoField());
			execs.Add(new EPLFromClauseMultikeyWArrayJoinComposite());
			execs.Add(new EPLFromClauseMultikeyWArrayParameterizedByArray());
			execs.Add(new EPLFromClauseMultikeyWArrayParameterizedByTwoField());
			return execs;
		}

		private class EPLFromClauseMultikeyWArrayParameterizedByTwoField : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl = "@Name('s0') select * from SupportEventWithManyArray as e,\n" +
				             "method:" +
				             typeof(SupportJoinResultIsArray).MaskTypeName() +
				             ".GetResultTwoField(e.Id, e.IntOne) as s";
				env.CompileDeploy(epl).AddListener("s0");

				SupportBean sb1 = SendManyArrayGetSB(env, "MA1", new int[] {1, 2});
				SupportBean sb2 = SendManyArrayGetSB(env, "MA2", new int[] {1});
				SupportBean sb3 = SendManyArrayGetSB(env, "MA3", new int[] { });
				SupportBean sb4 = SendManyArrayGetSB(env, "MA4", null);

				SendManyArray(env, "MA3", new int[] { });
				AssertReceivedUUID(env, sb3.TheString);

				SendManyArray(env, "MA1", new int[] {1, 2});
				AssertReceivedUUID(env, sb1.TheString);

				SendManyArray(env, "MA4", null);
				AssertReceivedUUID(env, sb4.TheString);

				SendManyArray(env, "MA2", new int[] {1});
				AssertReceivedUUID(env, sb2.TheString);

				SupportBean sb5 = SendManyArrayGetSB(env, "MA1", new int[] {1, 3});
				Assert.AreNotEqual(sb5.TheString, sb1.TheString);

				env.UndeployAll();
			}
		}

		private class EPLFromClauseMultikeyWArrayParameterizedByArray : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl = "@Name('s0') select * from SupportEventWithManyArray as e,\n" +
				             "method:" +
				             typeof(SupportJoinResultIsArray).MaskTypeName() +
				             ".GetResultIntArray(e.IntOne) as s";
				env.CompileDeploy(epl).AddListener("s0");

				SendManyArray(env, "E1", new int[] {1, 2});
				SupportBean sb12 = (SupportBean) env.Listener("s0").AssertOneGetNewAndReset().Get("s");

				SendManyArray(env, "E2", new int[] {1, 2});
				AssertReceivedUUID(env, sb12.TheString);

				SendManyArray(env, "E3", new int[] {3});
				SupportBean sb3 = (SupportBean) env.Listener("s0").AssertOneGetNewAndReset().Get("s");

				SendManyArray(env, "E4", new int[] {3});
				AssertReceivedUUID(env, sb3.TheString);

				SendManyArray(env, "E5", new int[] {1, 2});
				AssertReceivedUUID(env, sb12.TheString);

				env.UndeployAll();
			}
		}

		private class EPLFromClauseMultikeyWArrayJoinComposite : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl = "@Name('s0') select * from SupportEventWithManyArray as e,\n" +
				             "method:" +
				             typeof(SupportJoinResultIsArray).MaskTypeName() +
				             ".GetArray() as s " +
				             "where s.DoubleArray = e.DoubleOne and s.IntArray = e.IntOne and s.Value > e.Value";

				RunAssertion(env, epl);

				SendManyArray(env, "E3", new double[] {3, 4}, new int[] {30, 40}, 1000);
				Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

				env.UndeployAll();
			}
		}

		private class EPLFromClauseMultikeyWArrayJoinTwoField : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl = "@Name('s0') select * from SupportEventWithManyArray as e,\n" +
				             "method:" +
				             typeof(SupportJoinResultIsArray).MaskTypeName() +
				             ".GetArray() as s " +
				             "where s.DoubleArray = e.DoubleOne and s.IntArray = e.IntOne";

				RunAssertion(env, epl);

				SendManyArray(env, "E3", new double[] {3, 4}, new int[] {30, 41}, 0);
				Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

				env.UndeployAll();
			}
		}

		private class EPLFromClauseMultikeyWArrayJoinArray : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl = "@Name('s0') select * from SupportEventWithManyArray as e,\n" +
				             "method:" + typeof(SupportJoinResultIsArray).MaskTypeName() + ".GetArray() as s " +
				             "where s.DoubleArray = e.DoubleOne";

				RunAssertion(env, epl);

				env.UndeployAll();
			}
		}

		private static void RunAssertion(
			RegressionEnvironment env,
			string epl)
		{

			env.CompileDeploy(epl).AddListener("s0");

			SendManyArray(env, "E1", new double[] {3, 4}, new int[] {30, 40}, 50);
			AssertReceived(env, "E1", "DA2");

			env.Milestone(0);

			SendManyArray(env, "E2", new double[] {1, 2}, new int[] {10, 20}, 60);
			AssertReceived(env, "E2", "DA1");

			SendManyArray(env, "E3", new double[] {3, 4}, new int[] {30, 40}, 70);
			AssertReceived(env, "E3", "DA2");

			SendManyArray(env, "E4", new double[] {1}, new int[] {30, 40}, 80);
			Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());
		}

		private static void SendManyArray(
			RegressionEnvironment env,
			string id,
			double[] doubles,
			int[] ints,
			int value)
		{
			env.SendEventBean(new SupportEventWithManyArray(id).WithDoubleOne(doubles).WithIntOne(ints).WithValue(value));
		}

		private static void SendManyArray(
			RegressionEnvironment env,
			string id,
			int[] ints)
		{
			env.SendEventBean(new SupportEventWithManyArray(id).WithIntOne(ints));
		}

		private static SupportBean SendManyArrayGetSB(
			RegressionEnvironment env,
			string id,
			int[] ints)
		{
			env.SendEventBean(new SupportEventWithManyArray(id).WithIntOne(ints));
			return (SupportBean) env.Listener("s0").AssertOneGetNewAndReset().Get("s");
		}

		private static void AssertReceived(
			RegressionEnvironment env,
			string idOne,
			string idTwo)
		{
			EPAssertionUtil.AssertProps(
				env.Listener("s0").AssertOneGetNewAndReset(),
				"e.Id,s.Id".SplitCsv(),
				new object[] {idOne, idTwo});
		}

		public class SupportJoinResultIsArray
		{
			public static SupportDoubleAndIntArray[] GetArray()
			{
				return new SupportDoubleAndIntArray[] {
					new SupportDoubleAndIntArray("DA1", new double[] {1, 2}, new int[] {10, 20}, 100),
					new SupportDoubleAndIntArray("DA2", new double[] {3, 4}, new int[] {30, 40}, 300),
				};
			}

			public static SupportBean GetResultIntArray(int[] array)
			{
				return new SupportBean(UuidGenerator.Generate(), 0);
			}

			public static SupportBean GetResultTwoField(
				string id,
				int[] array)
			{
				return new SupportBean(UuidGenerator.Generate(), 0);
			}
		}

		private static void AssertReceivedUUID(
			RegressionEnvironment env,
			string uuidExpected)
		{
			SupportBean sb = (SupportBean) env.Listener("s0").AssertOneGetNewAndReset().Get("s");
			Assert.AreEqual(uuidExpected, sb.TheString);
		}

		public class SupportDoubleAndIntArray
		{
			private readonly string id;
			private readonly double[] doubleArray;
			private readonly int[] intArray;
			private readonly int value;

			public SupportDoubleAndIntArray(
				string id,
				double[] doubleArray,
				int[] intArray,
				int value)
			{
				this.id = id;
				this.doubleArray = doubleArray;
				this.intArray = intArray;
				this.value = value;
			}

			public string Id => id;

			public double[] DoubleArray => doubleArray;

			public int[] IntArray => intArray;

			public int Value => value;
		}
	}
} // end of namespace
