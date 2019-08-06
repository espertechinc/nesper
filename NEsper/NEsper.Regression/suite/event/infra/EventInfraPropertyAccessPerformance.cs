///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;

using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
    public class EventInfraPropertyAccessPerformance : RegressionExecution
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void Run(RegressionEnvironment env)
        {
            var methodName = ".testPerfPropertyAccess";

            var joinStatement = "@Name('s0') select * from " +
                                "SupportBeanCombinedProps#length(1)" +
                                " where indexed[0].mapped('a').Value = 'dummy'";
            env.CompileDeploy(joinStatement).AddListener("s0");

            // Send events for each stream
            var theEvent = SupportBeanCombinedProps.MakeDefaultBean();
            log.Info(methodName + " Sending events");

            var startTime = PerformanceObserver.MilliTime;
            for (var i = 0; i < 10000; i++) {
                SendEvent(env, theEvent);
            }

            log.Info(methodName + " Done sending events");

            var endTime = PerformanceObserver.MilliTime;
            log.Info(methodName + " delta=" + (endTime - startTime));

            // Stays at 250, below 500ms
            Assert.IsTrue(endTime - startTime < 1000);

            env.UndeployAll();
        }

        private void SendEvent(
            RegressionEnvironment env,
            object theEvent)
        {
            env.SendEventBean(theEvent);
        }
    }
} // end of namespace