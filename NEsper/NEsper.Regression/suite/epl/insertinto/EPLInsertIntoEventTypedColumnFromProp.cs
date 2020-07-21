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
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.insertinto
{
	public class EPLInsertIntoEventTypedColumnFromProp
	{
		public static IList<RegressionExecution> Executions()
		{
			IList<RegressionExecution> execs = new List<RegressionExecution>();
			execs.Add(new EPLInsertIntoEventTypedColumnOnMerge());
			execs.Add(new EPLInsertIntoPONOTypedColumnOnMerge());
			return execs;
		}

		private class EPLInsertIntoPONOTypedColumnOnMerge : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl =
					$"create schema CarOutputStream(status string, outputevent {typeof(SupportBean).MaskTypeName()});\n" +
					$"create table StatusTable(theString string primary key, lastevent {typeof(SupportBean).MaskTypeName()});\n" +
					"on SupportBean as ce merge StatusTable as st where ce.theString = st.theString \n" +
					"  when matched \n" +
					"    then update set lastevent = ce \n" +
					"  when not matched \n" +
					"    then insert select ce.theString as theString, ce as lastevent\n" +
					"    then insert into CarOutputStream select 'online' as status, ce as outputevent;\n" +
					"insert into CarTimeoutStream select e.* \n" +
					"  from pattern[every e=SupportBean -> (timer:interval(1 minutes) and not SupportBean(theString = e.theString))];\n" +
					"on CarTimeoutStream as cts merge StatusTable as st where cts.theString = st.theString \n" +
					"  when matched \n" +
					"    then delete \n" +
					"    then insert into CarOutputStream select 'offline' as status, lastevent as outputevent;\n" +
					"@name('s0') select * from CarOutputStream";
				env.AdvanceTime(0);
				env.CompileDeploy(epl).AddListener("s0");

				env.SendEventBean(new SupportBean("E1", 1));
				AssertReceivedPojo(env.Listener("s0").AssertOneGetNewAndReset(), "online", "E1");

				env.Milestone(0);

				env.AdvanceTime(60000);
				AssertReceivedPojo(env.Listener("s0").AssertOneGetNewAndReset(), "offline", "E1");

				env.UndeployAll();
			}
		}

		private class EPLInsertIntoEventTypedColumnOnMerge : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "@public @buseventtype create schema CarEvent(carId string, tracked boolean);\n" +
				          "create table StatusTable(carId string primary key, lastevent CarEvent);\n" +
				          "on CarEvent(tracked=true) as ce merge StatusTable as st where ce.carId = st.carId \n" +
				          "  when matched \n" +
				          "    then update set lastevent = ce \n" +
				          "  when not matched \n" +
				          "    then insert(carId, lastevent) select ce.carId, ce \n" +
				          "    then insert into CarOutputStream select 'online' as status, ce as outputevent;\n" +
				          "insert into CarTimeoutStream select e.* \n" +
				          "  from pattern[every e=CarEvent(tracked=true) -> (timer:interval(1 minutes) and not CarEvent(carId = e.carId, tracked=true))];\n" +
				          "on CarTimeoutStream as cts merge StatusTable as st where cts.carId = st.carId \n" +
				          "  when matched \n" +
				          "    then delete \n" +
				          "    then insert into CarOutputStream select 'offline' as status, lastevent as outputevent;\n" +
				          "@name('s0') select * from CarOutputStream";
				env.AdvanceTime(0);
				env.CompileDeploy(epl).AddListener("s0");

				SendCarMap(env, "C1");
				AssertReceivedMap(env.Listener("s0").AssertOneGetNewAndReset(), "online", "C1");

				env.Milestone(0);

				env.AdvanceTime(60000);
				AssertReceivedMap(env.Listener("s0").AssertOneGetNewAndReset(), "offline", "C1");

				env.UndeployAll();
			}
		}

		private static void AssertReceivedMap(
			EventBean received,
			string status,
			string carId)
		{
			Assert.AreEqual(status, received.Get("status"));
			var got = received.Get("outputevent");
			Assert.AreEqual(carId, received.Get("outputevent").AsStringDictionary().Get("carId"));
		}

		private static void AssertReceivedPojo(
			EventBean received,
			string status,
			string carId)
		{
			Assert.AreEqual(status, received.Get("status"));
			Assert.AreEqual(carId, ((SupportBean) received.Get("outputevent")).TheString);
		}

		private static void SendCarMap(
			RegressionEnvironment env,
			string carId)
		{
			env.SendEventMap(CollectionUtil.BuildMap("carId", carId, "tracked", true), "CarEvent");
		}
	}
} // end of namespace
