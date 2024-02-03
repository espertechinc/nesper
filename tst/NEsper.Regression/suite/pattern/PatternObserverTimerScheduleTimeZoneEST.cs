///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.datetime;
using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.pattern
{
    public class PatternObserverTimerScheduleTimeZoneEST : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            SendCurrentTime(env, "2012-10-01T08:59:00.000GMT-04:00");

            var epl = "@name('s0') select * from pattern[timer:schedule(date: current_timestamp.withTime(9, 0, 0, 0))]";
            env.CompileDeploy(epl).AddListener("s0");

            SendCurrentTime(env, "2012-10-01T08:59:59.999GMT-4:00");
            env.AssertListenerNotInvoked("s0");

            SendCurrentTime(env, "2012-10-01T09:00:00.000GMT-4:00");
            env.AssertListenerInvoked("s0");

            SendCurrentTime(env, "2012-10-03T09:00:00.000GMT-4:00");
            env.AssertListenerNotInvoked("s0");

            env.UndeployAll();
        }

        private static void SendCurrentTime(
            RegressionEnvironment env,
            string time)
        {
            env.AdvanceTime(DateTimeParsingFunctions.ParseDefaultMSecWZone(time));
        }
    }
} // end of namespace