///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.support.client.SupportCompileDeployUtil;

namespace com.espertech.esper.regressionlib.suite.multithread
{
    /// <summary>
    ///     Test for multithread-safety (or lack thereof) for iterators: iterators fail with concurrent mods as expected
    ///     behavior
    /// </summary>
    public class MultithreadContextTemporalStartStop : RegressionExecution
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.MULTITHREADED);
        }

        public void Run(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            env.CompileDeploy(
                "@public create context EverySecond as start (*, *, *, *, *, *) end (*, *, *, *, *, *)",
                path);
            env.CompileDeploy("context EverySecond select * from SupportBean", path);

            var timerRunnable = new TimerRunnable(env, 0, 24 * 60 * 60 * 1000, 1000);
            var timerThread = new Thread(timerRunnable.Run);
            timerThread.Name = GetType().Name + "-timer";

            var eventRunnable = new EventRunnable(env, 1000000);
            var eventThread = new Thread(eventRunnable.Run);
            eventThread.Name = GetType().Name + "event";

            timerThread.Start();
            eventThread.Start();

            ThreadJoin(timerThread);
            ThreadJoin(eventThread);
            Assert.IsNull(eventRunnable.Exception);
            Assert.IsNull(timerRunnable.Exception);

            env.UndeployAll();
        }

        public class TimerRunnable : IRunnable
        {
            private readonly long end;

            private readonly RegressionEnvironment env;
            private readonly long increment;
            private readonly long start;

            public TimerRunnable(
                RegressionEnvironment env,
                long start,
                long end,
                long increment)
            {
                this.env = env;
                this.start = start;
                this.end = end;
                this.increment = increment;
            }

            public Exception Exception { get; private set; }

            public void Run()
            {
                Log.Info("Started time drive");
                try {
                    var current = start;
                    long stepCount = 0;
                    var expectedSteps = (end - start) / increment;
                    while (current < end) {
                        env.AdvanceTime(current);
                        current += increment;
                        stepCount++;

                        if (stepCount % 10000 == 0) {
                            Log.Info("Sending step #" + stepCount + " of " + expectedSteps);
                        }
                    }
                }
                catch (Exception ex) {
                    Log.Error("Exception encountered: " + ex.Message, ex);
                    Exception = ex;
                }

                Log.Info("Completed time drive");
            }
        }

        public class EventRunnable : IRunnable
        {
            private readonly RegressionEnvironment env;
            private readonly long numEvents;

            public EventRunnable(
                RegressionEnvironment env,
                long numEvents)
            {
                this.env = env;
                this.numEvents = numEvents;
            }

            public Exception Exception { get; private set; }

            public void Run()
            {
                Log.Info("Started event send");
                try {
                    long count = 0;
                    while (count < numEvents) {
                        env.SendEventBean(new SupportBean());
                        count++;

                        if (count % 10000 == 0) {
                            Log.Info("Sending event #" + count);
                        }
                    }
                }
                catch (Exception ex) {
                    Log.Error("Exception encountered: " + ex.Message, ex);
                    Exception = ex;
                }

                Log.Info("Completed event send");
            }
        }
    }
} // end of namespace