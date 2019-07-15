///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Threading;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.support.client.SupportCompileDeployUtil;

namespace com.espertech.esper.regressionlib.suite.multithread
{
    public class MultithreadStmtNamedWindowUniqueTwoWJoinConsumer
    {
        private int count;

        public void Run(Configuration configuration)
        {
            configuration.Common.AddEventType(typeof(EventOne));
            configuration.Common.AddEventType(typeof(EventTwo));

            RunAssertion(1, true, null, null, configuration);
            RunAssertion(2, false, true, Locking.SPIN, configuration);
            RunAssertion(3, false, true, Locking.SUSPEND, configuration);
            RunAssertion(4, false, false, null, configuration);
        }

        private void RunAssertion(
            int runtimeNum,
            bool useDefault,
            bool? preserve,
            Locking? locking,
            Configuration config)
        {
            if (!useDefault) {
                config.Runtime.Threading.IsNamedWindowConsumerDispatchPreserveOrder = preserve.GetValueOrDefault();
                config.Runtime.Threading.NamedWindowConsumerDispatchLocking = locking.GetValueOrDefault();
                config.Runtime.Threading.NamedWindowConsumerDispatchTimeout = 100000;

                if (!preserve.GetValueOrDefault(false)) {
                    // In this setting there is no guarantee:
                    // (1) A thread T1 may process the first event to create a pair {E1, null}
                    // (2) A thread T2 may process the second event to create a pair {E2, E1}
                    // (3) Thread T2 pair may process first against consumer index
                    // (4) Thread T1 pair processes against consumer index and since its a unique index it fails
                    config.Runtime.ExceptionHandling.HandlerFactories.Clear();
                }
            }

            var runtime = EPRuntimeProvider.GetRuntime(
                typeof(MultithreadStmtNamedWindowUniqueTwoWJoinConsumer).Name + "_" + runtimeNum + "_" + count++,
                config);
            runtime.Initialize();

            var epl = "create window EventOneWindow#unique(key) as EventOne;\n" +
                      "insert into EventOneWindow select * from EventOne;\n" +
                      "create window EventTwoWindow#unique(key) as EventTwo;\n" +
                      "insert into EventTwoWindow select * from EventTwo;\n" +
                      "@Name('out') select * from EventOneWindow as e1, EventTwoWindow as e2 where e1.key = e2.key";
            var deployed = CompileDeploy(epl, runtime, config);

            var listener = new SupportMTUpdateListener();
            runtime.DeploymentService.GetStatement(deployed.DeploymentId, "out").AddListener(listener);

            ThreadStart runnableOne = () => {
                for (var i = 0; i < 33; i++) {
                    var eventOne = new EventOne("TEST");
                    runtime.EventService.SendEventBean(eventOne, eventOne.GetType().Name);
                    var eventTwo = new EventTwo("TEST");
                    runtime.EventService.SendEventBean(eventTwo, eventTwo.GetType().Name);
                }
            };

            ThreadStart runnableTwo = () => {
                for (var i = 0; i < 33; i++) {
                    var eventTwo = new EventTwo("TEST");
                    runtime.EventService.SendEventBean(eventTwo, eventTwo.GetType().Name);
                    var eventOne = new EventOne("TEST");
                    runtime.EventService.SendEventBean(eventOne, eventOne.GetType().Name);
                }
            };

            ThreadStart runnableThree = () => {
                for (var i = 0; i < 34; i++) {
                    var eventTwo = new EventTwo("TEST");
                    runtime.EventService.SendEventBean(eventTwo, eventTwo.GetType().Name);
                    var eventOne = new EventOne("TEST");
                    runtime.EventService.SendEventBean(eventOne, eventOne.GetType().Name);
                }
            };

            var t1 = new Thread(runnableOne) {
                Name = typeof(MultithreadStmtNamedWindowUniqueTwoWJoinConsumer).Name + "-one"
            };
            var t2 = new Thread(runnableTwo) {
                Name = typeof(MultithreadStmtNamedWindowUniqueTwoWJoinConsumer).Name + "-two"
            };
            var t3 = new Thread(runnableThree) {
                Name = typeof(MultithreadStmtNamedWindowUniqueTwoWJoinConsumer).Name + "-three"
            };
            t1.Start();
            t2.Start();
            t3.Start();
            ThreadSleep(1000);

            ThreadJoin(t1);
            ThreadJoin(t2);
            ThreadJoin(t3);
            ThreadSleep(200);

            var delivered = listener.NewDataList;

            // count deliveries of multiple rows
            var countMultiDeliveries = 0;
            foreach (var events in delivered) {
                countMultiDeliveries += events.Length > 1 ? 1 : 0;
            }

            // count deliveries where instance doesn't monotonically increase from previous row for one column
            var countNotMonotone = 0;
            long? previousIdE1 = null;
            long? previousIdE2 = null;
            foreach (var events in delivered) {
                var idE1 = events[0].Get("e1.instance").AsLong();
                var idE2 = events[0].Get("e2.instance").AsLong();
                // comment-in when needed: System.out.println("Received " + IdE1 + " " + IdE2);

                if (previousIdE1 != null) {
                    var incorrect = idE1 != previousIdE1 && idE2 != previousIdE2;
                    if (!incorrect) {
                        incorrect = idE1 == previousIdE1 && idE2 != previousIdE2 + 1 ||
                                    idE2 == previousIdE2 && idE1 != previousIdE1 + 1;
                    }

                    if (incorrect) {
                        // comment-in when needed: System.out.println("Non-Monotone increase (this is still correct but noteworthy)");
                        countNotMonotone++;
                    }
                }

                previousIdE1 = idE1;
                previousIdE2 = idE2;
            }

            if (useDefault || preserve.GetValueOrDefault()) {
                Assert.AreEqual(0, countMultiDeliveries, "multiple row deliveries: " + countMultiDeliveries);
                // the number of non-monotone delivers should be small but not zero
                // this is because when the event get generated and when the event actually gets processed may not be in the same order
                Assert.IsTrue(countNotMonotone < 100, "count not monotone: " + countNotMonotone);
                Assert.IsTrue(
                    delivered.Count >=
                    197); // its possible to not have 199 since there may not be events on one side of the join
            }
            else {
                Assert.IsTrue(countNotMonotone > 5, "count not monotone: " + countNotMonotone);
            }

            runtime.Destroy();
        }

        public class EventOne
        {
            private static readonly AtomicLong ATOMIC_LONG = new AtomicLong(1);

            internal EventOne(string key)
            {
                Instance = ATOMIC_LONG.GetAndIncrement();
                Key = key;
            }

            public string Key { get; }

            public long Instance { get; }

            public override bool Equals(object o)
            {
                if (this == o) {
                    return true;
                }

                if (!(o is EventOne)) {
                    return false;
                }

                var eventOne = (EventOne) o;

                return Key.Equals(eventOne.Key);
            }

            public override int GetHashCode()
            {
                return Key.GetHashCode();
            }
        }

        public class EventTwo
        {
            private static readonly AtomicLong ATOMIC_LONG = new AtomicLong(1);

            public EventTwo(string key)
            {
                Instance = ATOMIC_LONG.GetAndIncrement();
                Key = key;
            }

            public long Instance { get; }

            public string Key { get; }

            public override bool Equals(object o)
            {
                if (this == o) {
                    return true;
                }

                if (!(o is EventTwo)) {
                    return false;
                }

                var eventTwo = (EventTwo) o;

                return Key.Equals(eventTwo.Key);
            }

            public override int GetHashCode()
            {
                return Key.GetHashCode();
            }
        }
    }
} // end of namespace