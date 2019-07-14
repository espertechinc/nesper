///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.epl.join
{
    public class EPLJoinStartStop
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EPLJoinStartStopSceneOne());
            execs.Add(new EPLJoinInvalidJoin());
            return execs;
        }

        private static void SendEvent(
            RegressionEnvironment env,
            object theEvent)
        {
            env.SendEventBean(theEvent);
        }

        internal class EPLJoinStartStopSceneOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var joinStatement = "@Name('s0') select * from " +
                                    "SupportMarketDataBean(symbol='IBM')#length(3) s0, " +
                                    "SupportMarketDataBean(symbol='CSCO')#length(3) s1" +
                                    " where s0.volume=s1.volume";
                env.CompileDeployAddListenerMileZero(joinStatement, "s0");

                var setOne = new object[5];
                var setTwo = new object[5];
                long[] volumesOne = {10, 20, 20, 40, 50};
                long[] volumesTwo = {10, 20, 30, 40, 50};
                for (var i = 0; i < setOne.Length; i++) {
                    setOne[i] = new SupportMarketDataBean("IBM", volumesOne[i], i, "");
                    setTwo[i] = new SupportMarketDataBean("CSCO", volumesTwo[i], i, "");
                }

                SendEvent(env, setOne[0]);
                SendEvent(env, setTwo[0]);
                Assert.IsNotNull(env.Listener("s0").LastNewData);
                env.Listener("s0").Reset();

                var listener = env.Listener("s0");
                env.UndeployAll();
                SendEvent(env, setOne[1]);
                SendEvent(env, setTwo[1]);
                Assert.IsFalse(listener.IsInvoked);

                env.CompileDeploy(joinStatement).AddListener("s0");
                SendEvent(env, setOne[2]);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
                SendEvent(env, setOne[3]);
                SendEvent(env, setOne[4]);
                SendEvent(env, setTwo[3]);

                env.CompileDeploy(joinStatement).AddListener("s0");
                SendEvent(env, setTwo[4]);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class EPLJoinInvalidJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var invalidJoin = "select * from SupportBean_A, SupportBean_B";
                TryInvalidCompile(
                    env,
                    invalidJoin,
                    "Joins require that at least one view is specified for each stream, no view was specified for SupportBean_A");

                invalidJoin = "select * from SupportBean_A#time(5 min), SupportBean_B";
                TryInvalidCompile(
                    env,
                    invalidJoin,
                    "Joins require that at least one view is specified for each stream, no view was specified for SupportBean_B");

                invalidJoin = "select * from SupportBean_A#time(5 min), pattern[SupportBean_A=>SupportBean_B]";
                TryInvalidCompile(
                    env,
                    invalidJoin,
                    "Joins require that at least one view is specified for each stream, no view was specified for pattern event stream");
            }
        }
    }
} // end of namespace