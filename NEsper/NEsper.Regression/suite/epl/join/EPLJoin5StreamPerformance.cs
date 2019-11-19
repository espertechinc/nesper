///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.join
{
    public class EPLJoin5StreamPerformance : RegressionExecution
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void Run(RegressionEnvironment env)
        {
            var statement = "@Name('s0') select * from " +
                            "SupportBean_S0#length(100000) as S0," +
                            "SupportBean_S1#length(100000) as S1," +
                            "SupportBean_S2#length(100000) as S2," +
                            "SupportBean_S3#length(100000) as S3," +
                            "SupportBean_S4#length(100000) as S4" +
                            " where S0.P00 = S1.P10 " +
                            "and S1.P10 = S2.P20 " +
                            "and S2.P20 = S3.P30 " +
                            "and S3.P30 = S4.P40 ";
            env.CompileDeployAddListenerMileZero(statement, "s0");

            log.Info(".testPerfAllProps Preloading events");
            var startTime = PerformanceObserver.MilliTime;
            for (var i = 0; i < 1000; i++) {
                SendEvents(env, new[] {0, 0, 0, 0, 0}, new[] {"s0" + i, "s1" + i, "s2" + i, "s3" + i, "s4" + i});
            }

            var endTime = PerformanceObserver.MilliTime;
            log.Info(".testPerfAllProps delta=" + (endTime - startTime));
            Assert.IsTrue(endTime - startTime < 1500);

            // test if join returns data
            Assert.IsNull(env.Listener("s0").LastNewData);
            string[] propertyValues = {"x", "x", "x", "x", "x"};
            int[] ids = {1, 2, 3, 4, 5};
            SendEvents(env, ids, propertyValues);
            AssertEventsReceived(env.Listener("s0"), ids);

            env.UndeployAll();
        }

        private static void AssertEventsReceived(
            SupportListener updateListener,
            int[] expectedIds)
        {
            Assert.AreEqual(1, updateListener.LastNewData.Length);
            Assert.IsNull(updateListener.LastOldData);
            var theEvent = updateListener.LastNewData[0];
            Assert.AreEqual(expectedIds[0], ((SupportBean_S0) theEvent.Get("S0")).Id);
            Assert.AreEqual(expectedIds[1], ((SupportBean_S1) theEvent.Get("S1")).Id);
            Assert.AreEqual(expectedIds[2], ((SupportBean_S2) theEvent.Get("S2")).Id);
            Assert.AreEqual(expectedIds[3], ((SupportBean_S3) theEvent.Get("S3")).Id);
            Assert.AreEqual(expectedIds[4], ((SupportBean_S4) theEvent.Get("S4")).Id);
        }

        private static void SendEvent(
            RegressionEnvironment env,
            object theEvent)
        {
            env.SendEventBean(theEvent);
        }

        private static void SendEvents(
            RegressionEnvironment env,
            int[] ids,
            string[] propertyValues)
        {
            SendEvent(env, new SupportBean_S0(ids[0], propertyValues[0]));
            SendEvent(env, new SupportBean_S1(ids[1], propertyValues[1]));
            SendEvent(env, new SupportBean_S2(ids[2], propertyValues[2]));
            SendEvent(env, new SupportBean_S3(ids[3], propertyValues[3]));
            SendEvent(env, new SupportBean_S4(ids[4], propertyValues[4]));
        }
    }
} // end of namespace