///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.join
{
	public class EPLJoinMultiKeyAndRange
	{
		public static IList<RegressionExecution> Executions()
		{
			IList<RegressionExecution> execs = new List<RegressionExecution>();
			execs.Add(new EPLJoinRangeNullAndDupAndInvalid());
			execs.Add(new EPLJoinMultikeyWArrayHashJoinArray());
			execs.Add(new EPLJoinMultikeyWArrayHashJoin2Prop());
			execs.Add(new EPLJoinMultikeyWArrayCompositeArray());
			execs.Add(new EPLJoinMultikeyWArrayComposite2Prop());
			return execs;
		}

		private class EPLJoinMultikeyWArrayComposite2Prop : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string eplOne = "@name('s0') select * " +
				                "from SupportBean_S0#keepall as s0, SupportBean_S1#keepall as s1 " +
				                "where s0.p00 = s1.p10 and s0.p01 = s1.p11 and s0.p02 > s1.p12";
				env.CompileDeploy(eplOne).AddListener("s0");

				SendS0(env, 10, "a0", "b0", "X");
				SendS1(env, 20, "a0", "b0", "F");
				AssertReceived(
					env,
					new[] {
						new object[] {10, 20}
					});

				env.Milestone(0);

				SendS0(env, 11, "a1", "b0", "X");
				SendS1(env, 22, "a0", "b1", "F");
				SendS0(env, 12, "a0", "b1", "A");
				Assert.IsFalse(env.Listener("s0").IsInvoked);

				SendS0(env, 13, "a0", "b1", "Z");
				AssertReceived(
					env,
					new[] {
						new object[] {13, 22}
					});

				SendS1(env, 23, "a1", "b0", "A");
				AssertReceived(
					env,
					new[] {
						new object[] {11, 23}
					});

				env.UndeployAll();
			}

			private void AssertReceived(
				RegressionEnvironment env,
				object[][] expected)
			{
				string[] fields = "s0.id,s1.id".SplitCsv();
				EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").GetAndResetLastNewData(), fields, expected);
			}

			private void SendS0(
				RegressionEnvironment env,
				int id,
				string p00,
				string p01,
				string p02)
			{
				env.SendEventBean(new SupportBean_S0(id, p00, p01, p02));
			}

			private void SendS1(
				RegressionEnvironment env,
				int id,
				string p10,
				string p11,
				string p12)
			{
				env.SendEventBean(new SupportBean_S1(id, p10, p11, p12));
			}
		}

		private class EPLJoinMultikeyWArrayCompositeArray : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string eplOne = "@name('s0') select * " +
				                "from SupportEventWithIntArray#keepall as si, SupportEventWithManyArray#keepall as sm " +
				                "where si.array = sm.intOne and si.value > sm.value";
				env.CompileDeploy(eplOne).AddListener("s0");

				SendIntArray(env, "I1", new[] {1, 2}, 10);
				SendManyArray(env, "M1", new[] {1, 2}, 5);
				AssertReceived(
					env,
					new[] {
						new object[] {"I1", "M1"}
					});

				env.Milestone(0);

				SendIntArray(env, "I2", new[] {1, 2}, 20);
				AssertReceived(
					env,
					new[] {
						new object[] {"I2", "M1"}
					});

				SendManyArray(env, "M2", new[] {1, 2}, 1);
				AssertReceived(
					env,
					new[] {
						new object[] {"I1", "M2"},
						new object[] {"I2", "M2"}
					});

				SendManyArray(env, "M3", new[] {1}, 1);
				Assert.IsFalse(env.Listener("s0").IsInvoked);

				SendIntArray(env, "I3", new[] {2}, 30);
				Assert.IsFalse(env.Listener("s0").IsInvoked);

				SendIntArray(env, "I4", new[] {1}, 40);
				AssertReceived(
					env,
					new[] {
						new object[] {"I4", "M3"}
					});

				SendManyArray(env, "M4", new[] {2}, 2);
				AssertReceived(
					env,
					new[] {
						new object[] {"I3", "M4"}
					});

				env.UndeployAll();
			}

			private void AssertReceived(
				RegressionEnvironment env,
				object[][] expected)
			{
				string[] fields = "si.id,sm.id".SplitCsv();
				EPAssertionUtil.AssertPropsPerRowAnyOrder(env.Listener("s0").GetAndResetLastNewData(), fields, expected);
			}

			private void SendManyArray(
				RegressionEnvironment env,
				string id,
				int[] ints,
				int value)
			{
				env.SendEventBean(new SupportEventWithManyArray(id).WithIntOne(ints).WithValue(value));
			}

			private void SendIntArray(
				RegressionEnvironment env,
				string id,
				int[] array,
				int value)
			{
				env.SendEventBean(new SupportEventWithIntArray(id, array, value));
			}
		}

		private class EPLJoinMultikeyWArrayHashJoinArray : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string eplOne = "@name('s0') select * " +
				                "from SupportEventWithIntArray#keepall as si, SupportEventWithManyArray#keepall as sm " +
				                "where si.array = sm.intOne";
				env.CompileDeploy(eplOne).AddListener("s0");

				SendIntArray(env, "I1", new[] {1, 2});
				SendManyArray(env, "M1", new[] {1, 2});
				AssertReceived(
					env,
					new[] {
						new object[] {"I1", "M1"}
					});

				env.Milestone(0);

				SendIntArray(env, "I2", new[] {1, 2});
				AssertReceived(
					env,
					new[] {
						new object[] {"I2", "M1"}
					});

				SendManyArray(env, "M2", new[] {1, 2});
				AssertReceived(
					env,
					new[] {
						new object[] {"I1", "M2"},
						new object[] {"I2", "M2"}
					});

				SendManyArray(env, "M3", new[] {1});
				Assert.IsFalse(env.Listener("s0").IsInvoked);

				SendIntArray(env, "I3", new[] {2});
				Assert.IsFalse(env.Listener("s0").IsInvoked);

				SendIntArray(env, "I4", new[] {1});
				AssertReceived(
					env,
					new[] {
						new object[] {"I4", "M3"}
					});

				SendManyArray(env, "M4", new[] {2});
				AssertReceived(
					env,
					new[] {
						new object[] {"I3", "M4"}
					});

				env.UndeployAll();
			}

			private void AssertReceived(
				RegressionEnvironment env,
				object[][] expected)
			{
				string[] fields = "si.id,sm.id".SplitCsv();
				EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").GetAndResetLastNewData(), fields, expected);
			}

			private void SendManyArray(
				RegressionEnvironment env,
				string id,
				int[] ints)
			{
				env.SendEventBean(new SupportEventWithManyArray(id).WithIntOne(ints));
			}

			private void SendIntArray(
				RegressionEnvironment env,
				string id,
				int[] array)
			{
				env.SendEventBean(new SupportEventWithIntArray(id, array));
			}
		}

		private class EPLJoinRangeNullAndDupAndInvalid : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string eplOne =
					"@name('s0') select sb.* from SupportBean#keepall sb, SupportBeanRange#lastevent where intBoxed between rangeStart and rangeEnd";
				env.CompileDeploy(eplOne).AddListener("s0");

				string eplTwo =
					"@name('s1') select sb.* from SupportBean#keepall sb, SupportBeanRange#lastevent where theString = key and intBoxed in [rangeStart: rangeEnd]";
				env.CompileDeploy(eplTwo).AddListener("s1");

				// null join lookups
				SendEvent(env, new SupportBeanRange("R1", "G", (int?) null, null));
				SendEvent(env, new SupportBeanRange("R2", "G", null, 10));
				SendEvent(env, new SupportBeanRange("R3", "G", 10, null));
				SendSupportBean(env, "G", -1, null);

				// range invalid
				SendEvent(env, new SupportBeanRange("R4", "G", 10, 0));
				Assert.IsFalse(env.Listener("s0").IsInvoked);
				Assert.IsFalse(env.Listener("s1").IsInvoked);

				// duplicates
				object eventOne = SendSupportBean(env, "G", 100, 5);
				object eventTwo = SendSupportBean(env, "G", 101, 5);
				SendEvent(env, new SupportBeanRange("R4", "G", 0, 10));
				EventBean[] events = env.Listener("s0").GetAndResetLastNewData();
				EPAssertionUtil.AssertEqualsAnyOrder(new[] {eventOne, eventTwo}, EPAssertionUtil.GetUnderlying(events));
				events = env.Listener("s1").GetAndResetLastNewData();
				EPAssertionUtil.AssertEqualsAnyOrder(new[] {eventOne, eventTwo}, EPAssertionUtil.GetUnderlying(events));

				// test string compare
				string eplThree =
					"@name('s2') select sb.* from SupportBeanRange#keepall sb, SupportBean#lastevent where theString in [rangeStartStr:rangeEndStr]";
				env.CompileDeploy(eplThree).AddListener("s2");

				SendSupportBean(env, "P", 1, 1);
				SendEvent(env, new SupportBeanRange("R5", "R5", "O", "Q"));
				Assert.IsTrue(env.Listener("s0").IsInvoked);

				env.UndeployAll();
			}
		}

		private class EPLJoinMultikeyWArrayHashJoin2Prop : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{

				string joinStatement = "@name('s0') select * from " +
				                       "SupportBean(theString like 'A%')#length(3) as streamA," +
				                       "SupportBean(theString like 'B%')#length(3) as streamB" +
				                       " where streamA.intPrimitive = streamB.intPrimitive " +
				                       "and streamA.intBoxed = streamB.intBoxed";
				env.CompileDeploy(joinStatement).AddListener("s0");
				string[] fields = "streamA.theString,streamB.theString".SplitCsv();

				Assert.AreEqual(typeof(SupportBean), env.Statement("s0").EventType.GetPropertyType("streamA"));
				Assert.AreEqual(typeof(SupportBean), env.Statement("s0").EventType.GetPropertyType("streamB"));
				Assert.AreEqual(2, env.Statement("s0").EventType.PropertyNames.Length);

				int[][] eventData = {
					new[] {1, 100},
					new[] {2, 100},
					new[] {1, 200},
					new[] {2, 200}
				};
				SupportBean[] eventsA = new SupportBean[eventData.Length];
				SupportBean[] eventsB = new SupportBean[eventData.Length];

				for (int i = 0; i < eventData.Length; i++) {
					eventsA[i] = new SupportBean();
					eventsA[i].TheString = "A" + i;
					eventsA[i].IntPrimitive = eventData[i][0];
					eventsA[i].IntBoxed = eventData[i][1];

					eventsB[i] = new SupportBean();
					eventsB[i].TheString = "B" + i;
					eventsB[i].IntPrimitive = eventData[i][0];
					eventsB[i].IntBoxed = eventData[i][1];
				}

				SendEvent(env, eventsA[0]);
				SendEvent(env, eventsB[1]);
				SendEvent(env, eventsB[2]);
				SendEvent(env, eventsB[3]);
				Assert.IsNull(env.Listener("s0").LastNewData); // No events expected

				env.Milestone(0);

				SendSupportBean(env, "AX", 2, 100);
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"AX", "B1"});

				SendSupportBean(env, "BX", 1, 100);
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"A0", "BX"});

				env.UndeployAll();
			}
		}

		private static void SendEvent(
			RegressionEnvironment env,
			object theEvent)
		{
			env.SendEventBean(theEvent);
		}

		private static SupportBean SendSupportBean(
			RegressionEnvironment env,
			string theString,
			int intPrimitive,
			int? intBoxed)
		{
			SupportBean bean = new SupportBean(theString, intPrimitive);
			bean.IntBoxed = intBoxed;
			env.SendEventBean(bean);
			return bean;
		}
	}
} // end of namespace
