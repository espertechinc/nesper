///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.datetime;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;

using static com.espertech.esper.common.client.scopetest.EPAssertionUtil;
using static com.espertech.esper.regressionlib.support.stage.SupportStageUtil;

namespace com.espertech.esper.regressionlib.suite.client.stage
{
	public class ClientStageAdvanceTime
	{
		public static IList<RegressionExecution> Executions()
		{
			IList<RegressionExecution> execs = new List<RegressionExecution>();
			execs.Add(new ClientStageAdvanceWindowTime());
			execs.Add(new ClientStageAdvanceWindowTimeBatch());
			execs.Add(new ClientStageAdvanceRelativeTime());
			execs.Add(new ClientStageCurrentTime());
			return execs;
		}

		private class ClientStageCurrentTime : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				AdvanceTime(env, null, "2002-05-30T09:00:00.000");
				env.StageService.GetStage("ST");
				Assert.AreEqual(
					DateTimeParsingFunctions.ParseDefaultMSec("2002-05-30T09:00:00.000"),
					env.StageService.GetStage("ST").EventService.CurrentTime);

				AdvanceTime(env, "ST", "2002-05-30T09:00:05.000");
				Assert.AreEqual(
					DateTimeParsingFunctions.ParseDefaultMSec("2002-05-30T09:00:05.000"),
					env.StageService.GetStage("ST").EventService.CurrentTime);

				env.Milestone(0);

				Assert.AreEqual(
					DateTimeParsingFunctions.ParseDefaultMSec("2002-05-30T09:00:05.000"),
					env.StageService.GetStage("ST").EventService.CurrentTime);

				env.UndeployAll();
			}
		}

		private class ClientStageAdvanceRelativeTime : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				AdvanceTime(env, null, "2002-05-30T09:00:00.000");

				string epl = "@Name('s0') select * from pattern[timer:interval(10 seconds)];\n";
				env.CompileDeploy(epl).AddListener("s0");
				string deploymentId = env.DeploymentId("s0");

				env.StageService.GetStage("ST");
				AdvanceTime(env, "ST", "2002-05-30T09:00:05.000");

				StageIt(env, "ST", deploymentId);

				AdvanceTime(env, "ST", "2002-05-30T09:00:10.000");
				env.ListenerStage("ST", "s0").AssertOneGetNewAndReset();

				UnstageIt(env, "ST", deploymentId);

				env.UndeployAll();
			}
		}

		private class ClientStageAdvanceWindowTimeBatch : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				AdvanceTime(env, null, "2002-05-30T09:00:00.000");

				string epl = "@Name('s0') select * from SupportBean#time_batch(10)";
				env.CompileDeploy(epl).AddListener("s0");
				string deploymentId = env.DeploymentId("s0");
				string[] fields = new string[] {"TheString"};

				env.SendEventBean(new SupportBean("E1", 1));

				env.Milestone(0);

				env.StageService.GetStage("ST");
				StageIt(env, "ST", deploymentId);

				env.Milestone(1);

				AdvanceTime(env, "ST", "2002-05-30T09:00:09.999");
				Assert.IsFalse(env.ListenerStage("ST", "s0").GetAndClearIsInvoked());
				AdvanceTime(env, "ST", "2002-05-30T09:00:10.000");
				AssertPropsPerRow(env.ListenerStage("ST", "s0").GetAndResetLastNewData(), fields, new object[][] {
					new object[] {"E1"}
				});
				env.SendEventBeanStage("ST", new SupportBean("E2", 1));

				env.Milestone(2);

				AdvanceTime(env, "2002-05-30T09:00:19.999");
				AdvanceTime(env, "ST", "2002-05-30T09:00:19.999");
				Assert.IsFalse(env.ListenerStage("ST", "s0").GetAndClearIsInvoked());

				UnstageIt(env, "ST", deploymentId);

				env.Milestone(3);

				AdvanceTime(env, "ST", "2002-05-30T09:00:20.000");
				Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());
				AdvanceTime(env, "2002-05-30T09:00:20.000");
				AssertPropsPerRow(env.Listener("s0").GetAndResetLastNewData(), fields, new object[][] {
					new object[] {"E2"}
				});

				env.UndeployAll();
			}
		}

		private class ClientStageAdvanceWindowTime : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				env.AdvanceTime(0);

				string epl = "@Name('s0') select irstream * from SupportBean#time(10)";
				env.CompileDeploy(epl).AddListener("s0");
				string deploymentId = env.DeploymentId("s0");

				env.SendEventBean(new SupportBean("E1", 1));

				env.AdvanceTime(2000);
				env.SendEventBean(new SupportBean("E2", 2));

				env.AdvanceTime(4000);
				env.SendEventBean(new SupportBean("E3", 3));
				env.Listener("s0").Reset();

				env.Milestone(0);

				env.StageService.GetStage("P1");
				StageIt(env, "P1", deploymentId);

				env.Milestone(1);

				env.AdvanceTimeStage("P1", 9999);
				Assert.IsFalse(env.ListenerStage("P1", "s0").GetAndClearIsInvoked());
				env.AdvanceTimeStage("P1", 10000);
				Assert.AreEqual("E1", env.ListenerStage("P1", "s0").AssertOneGetOldAndReset().Get("TheString"));

				env.Milestone(2);

				env.AdvanceTimeStage("P1", 11999);
				Assert.IsFalse(env.ListenerStage("P1", "s0").GetAndClearIsInvoked());
				env.AdvanceTimeStage("P1", 12000);
				Assert.AreEqual("E2", env.ListenerStage("P1", "s0").AssertOneGetOldAndReset().Get("TheString"));

				env.AdvanceTime(12000);
				Assert.IsFalse(env.ListenerStage("P1", "s0").GetAndClearIsInvoked());

				UnstageIt(env, "P1", deploymentId);

				env.Milestone(3);

				env.AdvanceTime(13999);
				env.AdvanceTimeStage("P1", 14000);
				SupportListener listener = env.Listener("s0");
				Assert.IsFalse(listener.GetAndClearIsInvoked());
				env.AdvanceTime(14000);
				Assert.AreEqual("E3", env.Listener("s0").AssertOneGetOldAndReset().Get("TheString"));

				env.UndeployAll();
			}
		}

		private static void AdvanceTime(
			RegressionEnvironment env,
			string stageUri,
			string time)
		{
			env.AdvanceTimeStage(stageUri, DateTimeParsingFunctions.ParseDefaultMSec(time));
		}

		private static void AdvanceTime(
			RegressionEnvironment env,
			string time)
		{
			AdvanceTime(env, null, time);
		}
	}
} // end of namespace
