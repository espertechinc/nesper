///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.epl.insertinto
{
    public class EPLInsertIntoEventTypedColumnFromProp
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
#if REGRESSION_EXECUTIONS
            WithEventTypedColumnOnMerge(execs);
            With(PONOTypedColumnOnMerge)(execs);
#endif
            return execs;
        }

        public static IList<RegressionExecution> WithPONOTypedColumnOnMerge(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoPONOTypedColumnOnMerge());
            return execs;
        }

        public static IList<RegressionExecution> WithEventTypedColumnOnMerge(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoEventTypedColumnOnMerge());
            return execs;
        }

        private class EPLInsertIntoPONOTypedColumnOnMerge : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    $"create schema CarOutputStream(Status string, outputevent {typeof(SupportBean).MaskTypeName()});\n" +
                    $"create table StatusTable(TheString string primary key, lastevent {typeof(SupportBean).MaskTypeName()});\n" +
                    "on SupportBean as ce merge StatusTable as st where ce.TheString = st.TheString \n" +
                    "  when matched \n" +
                    "    then update set lastevent = ce \n" +
                    "  when not matched \n" +
                    "    then insert select ce.TheString as TheString, ce as lastevent\n" +
                    "    then insert into CarOutputStream select 'online' as Status, ce as outputevent;\n" +
                    "insert into CarTimeoutStream select e.* \n" +
                    "  from pattern[every e=SupportBean -> (timer:interval(1 minutes) and not SupportBean(TheString = e.TheString))];\n" +
                    "on CarTimeoutStream as cts merge StatusTable as st where cts.TheString = st.TheString \n" +
                    "  when matched \n" +
                    "    then delete \n" +
                    "    then insert into CarOutputStream select 'offline' as Status, lastevent as outputevent;\n" +
                    "@name('s0') select * from CarOutputStream";
                env.AdvanceTime(0);
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertEventNew("s0", @event => AssertReceivedPono(@event, "online", "E1"));

                env.Milestone(0);

                env.AdvanceTime(60000);
                env.AssertEventNew("s0", @event => AssertReceivedPono(@event, "offline", "E1"));

                env.UndeployAll();
            }
        }

        private class EPLInsertIntoEventTypedColumnOnMerge : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@public @buseventtype create schema CarEvent(CarId string, tracked boolean);\n" +
                    "create table StatusTable(CarId string primary key, lastevent CarEvent);\n" +
                    "on CarEvent(tracked=true) as ce merge StatusTable as st where ce.CarId = st.CarId \n" +
                    "  when matched \n" +
                    "    then update set lastevent = ce \n" +
                    "  when not matched \n" +
                    "    then insert(CarId, lastevent) select ce.CarId, ce \n" +
                    "    then insert into CarOutputStream select 'online' as Status, ce as outputevent;\n" +
                    "insert into CarTimeoutStream select e.* \n" +
                    "  from pattern[every e=CarEvent(tracked=true) -> (timer:interval(1 minutes) and not CarEvent(CarId = e.CarId, tracked=true))];\n" +
                    "on CarTimeoutStream as cts merge StatusTable as st where cts.CarId = st.CarId \n" +
                    "  when matched \n" +
                    "    then delete \n" +
                    "    then insert into CarOutputStream select 'offline' as Status, lastevent as outputevent;\n" +
                    "@name('s0') select * from CarOutputStream";
                env.AdvanceTime(0);
                env.CompileDeploy(epl).AddListener("s0");

                SendCarMap(env, "C1");
                env.AssertEventNew("s0", @event => AssertReceivedMap(@event, "online", "C1"));

                env.Milestone(0);

                env.AdvanceTime(60000);
                env.AssertEventNew("s0", @event => AssertReceivedMap(@event, "offline", "C1"));

                env.UndeployAll();
            }
        }

        private static void AssertReceivedMap(
            EventBean received,
            string status,
            string carId)
        {
            ClassicAssert.AreEqual(status, received.Get("Status"));
            var got = received.Get("outputevent");
            ClassicAssert.AreEqual(carId, received.Get("outputevent").AsStringDictionary().Get("CarId"));
        }

        private static void AssertReceivedPono(
            EventBean received,
            string status,
            string carId)
        {
            ClassicAssert.AreEqual(status, received.Get("Status"));
            ClassicAssert.AreEqual(carId, ((SupportBean)received.Get("outputevent")).TheString);
        }

        private static void SendCarMap(
            RegressionEnvironment env,
            string carId)
        {
            env.SendEventMap(CollectionUtil.BuildMap("CarId", carId, "tracked", true), "CarEvent");
        }
    }
} // end of namespace