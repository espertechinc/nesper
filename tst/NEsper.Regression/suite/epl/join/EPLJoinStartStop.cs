///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework; // assertFalse

// assertNotNull

namespace com.espertech.esper.regressionlib.suite.epl.join
{
    public class EPLJoinStartStop
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithStartStopSceneOne(execs);
            WithInvalidJoin(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInvalidJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinInvalidJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithStartStopSceneOne(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinStartStopSceneOne());
            return execs;
        }

        private class EPLJoinStartStopSceneOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var joinStatement = "@name('s0') select * from " +
                                    "SupportMarketDataBean(Symbol='IBM')#length(3) s0, " +
                                    "SupportMarketDataBean(Symbol='CSCO')#length(3) s1" +
                                    " where s0.Volume=s1.Volume";
                env.CompileDeployAddListenerMileZero(joinStatement, "s0");

                var setOne = new object[5];
                var setTwo = new object[5];
                var volumesOne = new long[] { 10, 20, 20, 40, 50 };
                var volumesTwo = new long[] { 10, 20, 30, 40, 50 };
                for (var i = 0; i < setOne.Length; i++) {
                    setOne[i] = new SupportMarketDataBean("IBM", volumesOne[i], i, "");
                    setTwo[i] = new SupportMarketDataBean("CSCO", volumesTwo[i], i, "");
                }

                SendEvent(env, setOne[0]);
                SendEvent(env, setTwo[0]);
                env.AssertListener("s0", listener => Assert.IsNotNull(listener.GetAndResetLastNewData()));

                var listener = env.Listener("s0");
                env.UndeployAll();
                SendEvent(env, setOne[1]);
                SendEvent(env, setTwo[1]);
                Assert.IsFalse(listener.IsInvoked);

                env.CompileDeploy(joinStatement).AddListener("s0");
                SendEvent(env, setOne[2]);
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
                SendEvent(env, setOne[3]);
                SendEvent(env, setOne[4]);
                SendEvent(env, setTwo[3]);

                env.CompileDeploy(joinStatement).AddListener("s0");
                SendEvent(env, setTwo[4]);
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.OBSERVEROPS);
            }
        }

        private class EPLJoinInvalidJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var invalidJoin = "select * from SupportBean_A, SupportBean_B";
                env.TryInvalidCompile(
                    invalidJoin,
                    "Joins require that at least one view is specified for each stream, no view was specified for SupportBean_A");

                invalidJoin = "select * from SupportBean_A#time(5 min), SupportBean_B";
                env.TryInvalidCompile(
                    invalidJoin,
                    "Joins require that at least one view is specified for each stream, no view was specified for SupportBean_B");

                invalidJoin = "select * from SupportBean_A#time(5 min), pattern[SupportBean_A->SupportBean_B]";
                env.TryInvalidCompile(
                    invalidJoin,
                    "Joins require that at least one view is specified for each stream, no view was specified for pattern event stream");
            }
        }

        private static void SendEvent(
            RegressionEnvironment env,
            object theEvent)
        {
            env.SendEventBean(theEvent);
        }
    }
} // end of namespace