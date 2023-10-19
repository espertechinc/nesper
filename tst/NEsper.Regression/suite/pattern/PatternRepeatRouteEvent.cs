///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.pattern
{
    public class PatternRepeatRouteEvent
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
#if REGRESSION_EXECUTIONS
            WithSingle(execs);
            WithCascade(execs);
            With(Timer)(execs);
#endif
            return execs;
        }

        public static IList<RegressionExecution> WithTimer(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternRouteTimer());
            return execs;
        }

        public static IList<RegressionExecution> WithCascade(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternRouteCascade());
            return execs;
        }

        public static IList<RegressionExecution> WithSingle(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternRouteSingle());
            return execs;
        }

        private static void SendEvent(
            EPRuntime epService,
            int intValue)
        {
            var theEvent = new SupportBean();
            theEvent.IntPrimitive = intValue;
            epService.EventService.SendEventBean(theEvent, theEvent.GetType().Name);
        }

        private static void RouteEvent(
            EPRuntime epService,
            int intValue)
        {
            var theEvent = new SupportBean();
            theEvent.IntPrimitive = intValue;
            epService.EventService.RouteEventBean(theEvent, theEvent.GetType().Name);
        }

        /// <summary>
        ///     Test route of an event within a env.Listener("s0").
        ///     The Listener when it receives an event will generate a single new event
        ///     that it routes back into the eventService, up to X number of times.
        /// </summary>
        internal class PatternRouteSingle : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.OBSERVEROPS);
            }

            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select * from pattern[every tag=SupportBean]";
                env.CompileDeploy(epl);

                var listener = new SingleRouteUpdateListener(env.Runtime);
                env.Statement("s0").AddListener(listener);

                // Send first event that triggers the loop
                SendEvent(env.Runtime, 0);

                // Should have fired X times
                Assert.AreEqual(1000, listener.Count);

                env.UndeployAll();
            }
        }

        /// <summary>
        ///     Test route of multiple events within a env.Listener("s0").
        ///     The Listener when it receives an event will generate multiple new events
        ///     that it routes back into the eventService, up to X number of times.
        /// </summary>
        internal class PatternRouteCascade : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.OBSERVEROPS);
            }

            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select * from pattern[every tag=SupportBean]";
                env.CompileDeploy(epl);

                var listener = new CascadeRouteUpdateListener(env.Runtime);
                env.Statement("s0").AddListener(listener);

                // Send first event that triggers the loop
                SendEvent(env.Runtime, 2); // the 2 translates to number of new events routed

                // Should have fired X times
                Assert.AreEqual(9, listener.CountReceived);
                Assert.AreEqual(8, listener.CountRouted);

                //  Num    Received         Routes      Num
                //  2             1           2         3
                //  3             2           6         4
                //  4             6             -

                env.UndeployAll();
            }
        }

        internal class PatternRouteTimer : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.OBSERVEROPS);
            }

            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);

                var epl = "@name('s0') select * from pattern[every tag=SupportBean]";
                var eventListener = new SingleRouteUpdateListener(env.Runtime);
                env.CompileDeploy(epl).Statement("s0").AddListener(eventListener);

                epl = "@name('s1') select * from pattern[every timer:at(*,*,*,*,*,*)]";
                var timeListener = new SingleRouteUpdateListener(env.Runtime);
                env.CompileDeploy(epl).Statement("s1").AddListener(timeListener);

                Assert.AreEqual(0, timeListener.Count);
                Assert.AreEqual(0, eventListener.Count);

                env.AdvanceTime(10000);

                Assert.AreEqual(1, timeListener.Count);
                Assert.AreEqual(1000, eventListener.Count);

                env.UndeployAll();
            }
        }

        internal class SingleRouteUpdateListener : UpdateListener
        {
            private readonly EPRuntime runtime;

            public SingleRouteUpdateListener(EPRuntime runtime)
            {
                this.runtime = runtime;
            }

            public int Count { get; private set; }

            public void Update(
                object sender,
                UpdateEventArgs eventArgs)
            {
                Count++;
                if (Count < 1000) {
                    RouteEvent(runtime, 0);
                }
            }
        }

        internal class CascadeRouteUpdateListener : UpdateListener
        {
            private readonly EPRuntime runtime;

            public CascadeRouteUpdateListener(EPRuntime runtime)
            {
                this.runtime = runtime;
            }

            internal int CountReceived { get; private set; }

            internal int CountRouted { get; private set; }

            public void Update(
                object sender,
                UpdateEventArgs eventArgs)
            {
                CountReceived++;
                var newEvents = eventArgs.NewEvents;
                var theEvent = (SupportBean)newEvents[0].Get("tag");
                var numNewEvents = theEvent.IntPrimitive;

                for (var i = 0; i < numNewEvents; i++) {
                    if (numNewEvents < 4) {
                        RouteEvent(runtime, numNewEvents + 1);
                        CountRouted++;
                    }
                }
            }
        }
    }
} // end of namespace