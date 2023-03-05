///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.suite.pattern;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.regressionrun.runner;
using com.espertech.esper.regressionrun.suite.core;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionrun.suite.pattern
{
    [TestFixture]
    public class TestSuitePatternWConfig : AbstractTestContainer
    {
        [Test, RunInApplicationDomain]
        public void TestMax2Noprevent()
        {
            using (var session = RegressionRunner.Session(Container)) {
                Configure(2, false, session.Configuration);
                RegressionRunner.Run(session, new PatternOperatorFollowedByMax2Noprevent());
            }
        }

        [Test, RunInApplicationDomain]
        public void TestMax2Prevent()
        {
            using (var session = RegressionRunner.Session(Container)) {
                Configure(2, true, session.Configuration);
                RegressionRunner.Run(session, new PatternOperatorFollowedByMax2Prevent());
            }
        }

        [Test, RunInApplicationDomain]
        public void TestMax4Prevent()
        {
            using (var session = RegressionRunner.Session(Container, true)) {
                Configure(4, true, session.Configuration);
                RegressionRunner.Run(session, new PatternOperatorFollowedByMax4Prevent());
            }
        }

        [Test, RunInApplicationDomain]
        public void TestPatternMicrosecondResolution()
        {
            using (var session = RegressionRunner.Session(Container)) {
                session.Configuration.Common.TimeSource.TimeUnit = TimeUnit.MICROSECONDS;
                RegressionRunner.Run(session, new PatternMicrosecondResolution(true));
            }
        }

        [Test]
        public void TestPatternMicrosecondResolutionCrontab()
        {
            using (var session = RegressionRunner.Session(Container)) {
                session.Configuration.Common.TimeSource.TimeUnit = TimeUnit.MICROSECONDS;
                RegressionRunner.Run(session, new PatternMicrosecondResolutionCrontab());
            }
        }

        [Test, RunInApplicationDomain]
        public void TestPatternObserverTimerScheduleTimeZoneEST()
        {
            using (var session = RegressionRunner.Session(Container)) {
                session.Configuration.Runtime.Expression.TimeZone = TimeZoneHelper.GetTimeZoneInfo("GMT-4:00");
                RegressionRunner.Run(session, new PatternObserverTimerScheduleTimeZoneEST());
            }
        }

        private void Configure(
            long max,
            bool preventStart,
            Configuration configuration)
        {
            configuration.Runtime.ConditionHandling.AddClass(typeof(SupportConditionHandlerFactory));
            configuration.Runtime.Patterns.MaxSubexpressions = max;
            configuration.Runtime.Patterns.IsMaxSubexpressionPreventStart = preventStart;

            foreach (Type clazz in new Type[] { typeof(SupportBean_A), typeof(SupportBean_B), typeof(SupportBean) }) {
                configuration.Common.AddEventType(clazz.Name, clazz);
            }
        }
    }
} // end of namespace