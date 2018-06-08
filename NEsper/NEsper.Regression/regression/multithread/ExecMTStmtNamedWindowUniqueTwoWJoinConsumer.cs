///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Threading;
using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using NUnit.Framework;

namespace com.espertech.esper.regression.multithread {
    public class ExecMTStmtNamedWindowUniqueTwoWJoinConsumer : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertion(1, true, null, null);
            RunAssertion(2, false, true, ConfigurationEngineDefaults.ThreadingConfig.Locking.SPIN);
            RunAssertion(3, false, true, ConfigurationEngineDefaults.ThreadingConfig.Locking.SUSPEND);
            RunAssertion(4, false, false, null);
        }

        private void RunAssertion(
            int engineNum, bool useDefault, bool? preserve,
            ConfigurationEngineDefaults.ThreadingConfig.Locking? locking) {
            var config = SupportConfigFactory.GetConfiguration();
            if (!useDefault) {
                config.EngineDefaults.Threading.IsNamedWindowConsumerDispatchPreserveOrder =
                    preserve.GetValueOrDefault();
                config.EngineDefaults.Threading.NamedWindowConsumerDispatchLocking = locking.GetValueOrDefault();
            }

            var epService = EPServiceProviderManager.GetProvider(
                SupportContainer.Instance, GetType().FullName + "_" + engineNum, config);
            epService.Initialize();
            epService.EPAdministrator.Configuration.AddEventType(typeof(EventOne));
            epService.EPAdministrator.Configuration.AddEventType(typeof(EventTwo));

            var epl =
                "create window EventOneWindow#unique(key) as EventOne;\n" +
                "insert into EventOneWindow select * from EventOne;\n" +
                "create window EventTwoWindow#unique(key) as EventTwo;\n" +
                "insert into EventTwoWindow select * from EventTwo;\n" +
                "@Name('out') select * from EventOneWindow as e1, EventTwoWindow as e2 where e1.key = e2.key";
            epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);

            var listener = new SupportMTUpdateListener();
            epService.EPAdministrator.GetStatement("out").Events += listener.Update;

            var runnableOne = new ThreadStart(
                () => {
                    for (var i = 0; i < 33; i++) {
                        var eventOne = new EventOne("TEST");
                        epService.EPRuntime.SendEvent(eventOne);
                        var eventTwo = new EventTwo("TEST");
                        epService.EPRuntime.SendEvent(eventTwo);
                    }
                });

            var runnableTwo = new ThreadStart(
                () => {
                    for (var i = 0; i < 33; i++) {
                        var eventTwo = new EventTwo("TEST");
                        epService.EPRuntime.SendEvent(eventTwo);
                        var eventOne = new EventOne("TEST");
                        epService.EPRuntime.SendEvent(eventOne);
                    }
                });
            var runnableThree = new ThreadStart(
                () => {
                    for (var i = 0; i < 34; i++) {
                        var eventTwo = new EventTwo("TEST");
                        epService.EPRuntime.SendEvent(eventTwo);
                        var eventOne = new EventOne("TEST");
                        epService.EPRuntime.SendEvent(eventOne);
                    }
                });

            var t1 = new Thread(runnableOne);
            var t2 = new Thread(runnableTwo);
            var t3 = new Thread(runnableThree);
            t1.Start();
            t2.Start();
            t3.Start();

            t1.Join();
            t2.Join();
            t3.Join();

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
                // comment-in when needed: Log.Info("Received " + idE1 + " " + idE2);

                if (previousIdE1 != null) {
                    var incorrect = idE1 != previousIdE1 && idE2 != previousIdE2;
                    if (!incorrect) {
                        incorrect = idE1 == previousIdE1 && idE2 != previousIdE2 + 1 ||
                                    idE2 == previousIdE2 && idE1 != previousIdE1 + 1;
                    }

                    if (incorrect) {
                        // comment-in when needed: Log.Info("Non-Monotone increase (this is still correct but noteworthy)");
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
                Assert.IsTrue(countNotMonotone < 50, "count not monotone: " + countNotMonotone);
                Assert.IsTrue(
                    delivered.Count >=
                    197); // its possible to not have 199 since there may not be events on one side of the join
            }
            else {
                Assert.IsTrue(countMultiDeliveries > 0, "multiple row deliveries: " + countMultiDeliveries);
                Assert.IsTrue(countNotMonotone > 5, "count not monotone: " + countNotMonotone);
            }

            epService.Dispose();
        }

        public class EventOne {
            private static readonly AtomicLong ATOMIC_LONG = new AtomicLong(1);

            public EventOne(string key) {
                Instance = ATOMIC_LONG.GetAndIncrement();
                Key = key;
            }

            public long Instance { get; }

            public string Key { get; }

            public override bool Equals(object o) {
                if (this == o) {
                    return true;
                }

                if (!(o is EventOne)) {
                    return false;
                }

                var eventOne = (EventOne) o;

                return Key.Equals(eventOne.Key);
            }

            public override int GetHashCode() {
                return Key.GetHashCode();
            }
        }

        public class EventTwo {
            private static readonly AtomicLong ATOMIC_LONG = new AtomicLong(1);

            public EventTwo(string key) {
                Instance = ATOMIC_LONG.GetAndIncrement();
                Key = key;
            }

            public long Instance { get; }

            public string Key { get; }

            public override bool Equals(object o) {
                if (this == o) {
                    return true;
                }

                if (!(o is EventTwo)) {
                    return false;
                }

                var eventTwo = (EventTwo) o;
                return Key.Equals(eventTwo.Key);
            }

            public override int GetHashCode() {
                return Key.GetHashCode();
            }
        }
    }
} // end of namespace