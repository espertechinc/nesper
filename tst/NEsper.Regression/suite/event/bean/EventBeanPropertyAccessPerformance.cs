///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.bean
{
    public class EventBeanPropertyAccessPerformance : RegressionExecution
    {
        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
        }

        public void Run(RegressionEnvironment env)
        {
            var methodName = ".testPerfPropertyAccess";

            var joinStatement = "@name('s0') select * from " +
                                "SupportBeanCombinedProps#length(1)" +
                                " where Indexed[0].Mapped('a').Value = 'dummy'";
            env.CompileDeploy(joinStatement).AddListener("s0");

            // Send events for each stream
            var theEvent = SupportBeanCombinedProps.MakeDefaultBean();
            log.Info(methodName + " Sending events");

            var delta = PerformanceObserver.TimeMillis(
                () => {
                    for (var i = 0; i < 10000; i++) {
                        SendEvent(env, theEvent);
                    }

                    log.Info(methodName + " Done sending events");
                });

            log.Info(methodName + " delta=" + delta);

            // Stays at 250, below 500ms
            Assert.That(delta, Is.LessThan(1000));

            env.UndeployAll();
        }

        private void SendEvent(
            RegressionEnvironment env,
            object theEvent)
        {
            env.SendEventBean(theEvent);
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(EventBeanPropertyAccessPerformance));
    }
} // end of namespace