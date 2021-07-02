///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.join
{
    public class EPLJoin3StreamAndPropertyPerformance
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithAllProps(execs);
            WithPartialProps(execs);
            WithPartialStreams(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithPartialStreams(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLJoinPerfPartialStreams());
            return execs;
        }

        public static IList<RegressionExecution> WithPartialProps(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLJoinPerfPartialProps());
            return execs;
        }

        public static IList<RegressionExecution> WithAllProps(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLJoinPerfAllProps());
            return execs;
        }

        private static void TryJoinPerf3Streams(
            RegressionEnvironment env,
            string epl)
        {
            var methodName = ".tryJoinPerf3Streams";

            env.CompileDeployAddListenerMileZero(epl, "s0");

            // Send events for each stream
            log.Info(methodName + " Preloading events");
            var startTime = PerformanceObserver.MilliTime;
            for (var i = 0; i < 100; i++) {
                SendEvent(env, new SupportBean_A("CSCO_" + i));
                SendEvent(env, new SupportBean_B("IBM_" + i));
                SendEvent(env, new SupportBean_C("GE_" + i));
            }

            log.Info(methodName + " Done preloading");

            var endTime = PerformanceObserver.MilliTime;
            log.Info(methodName + " delta=" + (endTime - startTime));

            // Stay below 500, no index would be 4 sec plus
            Assert.IsTrue(endTime - startTime < 500);

            env.UndeployAll();
        }

        private static void SendEvent(
            RegressionEnvironment env,
            object theEvent)
        {
            env.SendEventBean(theEvent);
        }

        internal class EPLJoinPerfAllProps : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Statement where all streams are reachable from each other via properties
                var stmt = "@Name('s0') select * from " +
                           "SupportBean_A()#length(1000000) S1," +
                           "SupportBean_B()#length(1000000) S2," +
                           "SupportBean_C()#length(1000000) S3" +
                           " where S1.Id=S2.Id and S2.Id=S3.Id and S1.Id=S3.Id";
                TryJoinPerf3Streams(env, stmt);
            }
        }

        internal class EPLJoinPerfPartialProps : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Statement where the s1 stream is not reachable by joining s2 to s3 and s3 to s1
                var stmt = "@Name('s0') select * from " +
                           "SupportBean_A#length(1000000) S1," +
                           "SupportBean_B#length(1000000) S2," +
                           "SupportBean_C#length(1000000) S3" +
                           " where S1.Id=S2.Id and S2.Id=S3.Id"; // ==> therefore S1.Id = S3.Id
                TryJoinPerf3Streams(env, stmt);
            }
        }

        internal class EPLJoinPerfPartialStreams : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var methodName = ".testPerfPartialStreams";

                // Statement where the s1 stream is not reachable by joining s2 to s3 and s3 to s1
                var epl = "@Name('s0') select * from " +
                          "SupportBean_A#length(1000000) S1," +
                          "SupportBean_B#length(1000000) S2," +
                          "SupportBean_C#length(1000000) S3" +
                          " where S1.Id=S2.Id"; // ==> stream s3 no properties supplied, full s3 scan
                env.CompileDeployAddListenerMileZero(epl, "s0");

                // preload s3 with just 1 event
                SendEvent(env, new SupportBean_C("GE_0"));

                // Send events for each stream
                log.Info(methodName + " Preloading events");
                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 1000; i++) {
                    SendEvent(env, new SupportBean_A("CSCO_" + i));
                    SendEvent(env, new SupportBean_B("IBM_" + i));
                }

                log.Info(methodName + " Done preloading");

                var endTime = PerformanceObserver.MilliTime;
                log.Info(methodName + " delta=" + (endTime - startTime));

                // Stay below 500, no index would be 4 sec plus
                Assert.IsTrue(endTime - startTime < 500);
                env.UndeployAll();
            }
        }
    }
} // end of namespace