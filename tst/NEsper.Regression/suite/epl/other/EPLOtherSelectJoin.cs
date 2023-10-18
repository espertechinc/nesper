///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.other
{
	public class EPLOtherSelectJoin
	{
		public static IList<RegressionExecution> Executions()
		{
			IList<RegressionExecution> execs = new List<RegressionExecution>();
			execs.Add(new EPLOtherJoinUniquePerId());
			execs.Add(new EPLOtherJoinNonUniquePerId());
			return execs;
		}

		private class EPLOtherJoinUniquePerId : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var holder = SetupStmt(env);

				SendEvent(env, holder.eventsA[0]);
				SendEvent(env, holder.eventsB[1]);
				env.AssertListenerNotInvoked("s0");

				// Test join new B with id 0
				SendEvent(env, holder.eventsB[0]);
				env.AssertListener(
					"s0",
					listener => {
						Assert.AreSame(holder.eventsA[0], listener.LastNewData[0].Get("streamA"));
						Assert.AreSame(holder.eventsB[0], listener.LastNewData[0].Get("streamB"));
						Assert.IsNull(listener.LastOldData);
						listener.Reset();
					});

				// Test join new A with id 1
				SendEvent(env, holder.eventsA[1]);
				env.AssertListener(
					"s0",
					listener => {
						Assert.AreSame(holder.eventsA[1], listener.LastNewData[0].Get("streamA"));
						Assert.AreSame(holder.eventsB[1], listener.LastNewData[0].Get("streamB"));
						Assert.IsNull(listener.LastOldData);
						listener.Reset();
					});

				SendEvent(env, holder.eventsA[2]);
				env.AssertListener("s0", listener => Assert.IsNull(listener.LastOldData));

				// Test join old A id 0 leaves length window of 3 events
				SendEvent(env, holder.eventsA[3]);
				env.AssertListener(
					"s0",
					listener => {
						Assert.AreSame(holder.eventsA[0], listener.LastOldData[0].Get("streamA"));
						Assert.AreSame(holder.eventsB[0], listener.LastOldData[0].Get("streamB"));
						Assert.IsNull(listener.LastNewData);
						listener.Reset();
					});

				// Test join old B id 1 leaves window
				SendEvent(env, holder.eventsB[4]);
				env.AssertListener("s0", listener => Assert.IsNull(listener.LastOldData));

				SendEvent(env, holder.eventsB[5]);
				env.AssertListener(
					"s0",
					listener => {
						Assert.AreSame(holder.eventsA[1], listener.LastOldData[0].Get("streamA"));
						Assert.AreSame(holder.eventsB[1], listener.LastOldData[0].Get("streamB"));
						Assert.IsNull(listener.LastNewData);
					});

				env.UndeployAll();
			}
		}

		private class EPLOtherJoinNonUniquePerId : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var holder = SetupStmt(env);

				SendEvent(env, holder.eventsA[0]);
				SendEvent(env, holder.eventsA[1]);
				SendEvent(env, holder.eventsASetTwo[0]);
				env.AssertListener(
					"s0",
					listener => Assert.IsTrue(listener.LastOldData == null && listener.LastNewData == null));

				SendEvent(env, holder.eventsB[0]); // Event B id 0 joins to A id 0 twice
				env.AssertListener(
					"s0",
					listener => {
						var data = listener.LastNewData;

						Assert.That(
							holder.eventsASetTwo[0],
							Is
								.EqualTo(data[0].Get("streamA"))
								.Or
								.EqualTo(data[1].Get("streamA"))); // Order arbitrary
						
						Assert.AreSame(holder.eventsB[0], data[0].Get("streamB"));
						
						Assert.That(
							holder.eventsA[0],
							Is
								.EqualTo(data[0].Get("streamA"))
								.Or
								.EqualTo(data[1].Get("streamA")));
						
						Assert.AreSame(holder.eventsB[0], data[1].Get("streamB"));
						Assert.IsNull(listener.LastOldData);
						listener.Reset();
					});

				SendEvent(env, holder.eventsB[2]);
				SendEvent(env, holder.eventsBSetTwo[0]); // Ignore events generated
				env.ListenerReset("s0");

				SendEvent(env, holder.eventsA[3]); // Pushes A id 0 out of window, which joins to B id 0 twice
				env.AssertListener(
					"s0",
					listener => {
						var data = listener.LastOldData;
						Assert.AreSame(holder.eventsA[0], listener.LastOldData[0].Get("streamA"));
						Assert.That(
							holder.eventsASetTwo[0],
							Is
								.EqualTo(data[0].Get("streamB"))
								.Or
								.EqualTo(data[1].Get("streamB"))); // B order arbitrary
						
						Assert.AreSame(holder.eventsA[0], listener.LastOldData[1].Get("streamA"));

						Assert.That(
							holder.eventsBSetTwo[0],
							Is
								.EqualTo(data[0].Get("streamB"))
								.Or
								.EqualTo(data[1].Get("streamB")));
						
						Assert.IsNull(listener.LastNewData);
						listener.Reset();
					});

				SendEvent(env, holder.eventsBSetTwo[2]); // Pushes B id 0 out of window, which joins to A set two id 0
				env.AssertListener(
					"s0",
					listener => {
						Assert.AreSame(holder.eventsASetTwo[0], listener.LastOldData[0].Get("streamA"));
						Assert.AreSame(holder.eventsB[0], listener.LastOldData[0].Get("streamB"));
						Assert.AreEqual(1, listener.LastOldData.Length);
					});

				env.UndeployAll();
			}
		}

		private static SelectJoinHolder SetupStmt(RegressionEnvironment env)
		{
			var holder = new SelectJoinHolder();

			var epl =
				"@name('s0') select irstream * from SupportBean_A#length(3) as streamA, SupportBean_B#length(3) as streamB where streamA.id = streamB.id";
			env.CompileDeploy(epl).AddListener("s0");

			env.AssertStatement(
				"s0",
				statement => {
					Assert.AreEqual(typeof(SupportBean_A), statement.EventType.GetPropertyType("streamA"));
					Assert.AreEqual(typeof(SupportBean_B), statement.EventType.GetPropertyType("streamB"));
					Assert.AreEqual(2, statement.EventType.PropertyNames.Length);
				});

			holder.eventsA = new SupportBean_A[10];
			holder.eventsASetTwo = new SupportBean_A[10];
			holder.eventsB = new SupportBean_B[10];
			holder.eventsBSetTwo = new SupportBean_B[10];
			for (var i = 0; i < holder.eventsA.Length; i++) {
				holder.eventsA[i] = new SupportBean_A(Convert.ToString(i));
				holder.eventsASetTwo[i] = new SupportBean_A(Convert.ToString(i));
				holder.eventsB[i] = new SupportBean_B(Convert.ToString(i));
				holder.eventsBSetTwo[i] = new SupportBean_B(Convert.ToString(i));
			}

			return holder;
		}

		private static void SendEvent(
			RegressionEnvironment env,
			object theEvent)
		{
			env.SendEventBean(theEvent);
		}

		private class SelectJoinHolder
		{
			internal SupportBean_A[] eventsA;
			internal SupportBean_A[] eventsASetTwo;
			internal SupportBean_B[] eventsB;
			internal SupportBean_B[] eventsBSetTwo;
		}
	}
} // end of namespace
