///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.support.client;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.multithread
{
    [TestFixture]
	public class TestMTStmtNamedWindowUniqueTwoWJoinConsumer
    {
        [Test]
	    public void TestUniqueNamedWindowDispatch()
        {
	        RunAssertion(true, null, null);
	        RunAssertion(false, true, ConfigurationEngineDefaults.Threading.Locking.SPIN);
	        RunAssertion(false, true, ConfigurationEngineDefaults.Threading.Locking.SUSPEND);
	        RunAssertion(false, false, null);
	    }

	    private void RunAssertion(bool useDefault, bool? preserve, ConfigurationEngineDefaults.Threading.Locking? locking)
        {
	        var config = SupportConfigFactory.GetConfiguration();
	        if (!useDefault) {
	            config.EngineDefaults.ThreadingConfig.IsNamedWindowConsumerDispatchPreserveOrder = preserve.Value;
	            config.EngineDefaults.ThreadingConfig.NamedWindowConsumerDispatchLocking = locking.Value;
	        }

	        var epService = EPServiceProviderManager.GetDefaultProvider(config);
	        epService.Initialize();
	        epService.EPAdministrator.Configuration.AddEventType(typeof(EventOne));
	        epService.EPAdministrator.Configuration.AddEventType(typeof(EventTwo));

	        var epl =
	                "create window EventOneWindow.std:unique(key) as EventOne;\n" +
	                "insert into EventOneWindow select * from EventOne;\n" +
	                "create window EventTwoWindow.std:unique(key) as EventTwo;\n" +
	                "insert into EventTwoWindow select * from EventTwo;\n" +
	                "@name('out') select * from EventOneWindow as e1, EventTwoWindow as e2 where e1.key = e2.key";
	        epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);

	        var listener = new SupportMTUpdateListener();
	        epService.EPAdministrator.GetStatement("out").Events += listener.Update;

            ThreadStart runnableOne = () =>
            {
	            for (var i = 0; i < 33; i++) {
	                var eventOne = new EventOne("TEST");
	                epService.EPRuntime.SendEvent(eventOne);
	                var eventTwo = new EventTwo("TEST");
	                epService.EPRuntime.SendEvent(eventTwo);
	            }
	        };
            ThreadStart runnableTwo = () =>
            {
	            for (var i = 0; i < 33; i++) {
	                var eventTwo = new EventTwo("TEST");
	                epService.EPRuntime.SendEvent(eventTwo);
	                var eventOne = new EventOne("TEST");
	                epService.EPRuntime.SendEvent(eventOne);
	            }
	        };
	        ThreadStart runnableThree = () =>
            {
	            for (var i = 0; i < 34; i++) {
	                var eventTwo = new EventTwo("TEST");
	                epService.EPRuntime.SendEvent(eventTwo);
	                var eventOne = new EventOne("TEST");
	                epService.EPRuntime.SendEvent(eventOne);
	            }
	        };

	        var t1 = new Thread(runnableOne);
	        var t2 = new Thread(runnableTwo);
	        var t3 = new Thread(runnableThree);
	        t1.Start();
	        t2.Start();
	        t3.Start();

	        t1.Join();
	        t2.Join();
	        t3.Join();

	        var delivered = listener.GetNewDataList();

	        // count deliveries of multiple rows
	        var countMultiDeliveries = 0;
	        foreach (var events in delivered) {
	            countMultiDeliveries += (events.Length > 1 ? 1 : 0);
	        }

	        // count deliveries where instance doesn't monotonically increase from previous row for one column
	        var countNotMonotone = 0;
	        long? previousIdE1 = null;
	        long? previousIdE2 = null;
	        foreach (var events in delivered)
	        {
	            var idE1 = events[0].Get("e1.instance").AsLong();
	            var idE2 = events[0].Get("e2.instance").AsLong();
	            // comment-in when needed: System.out.println("Received " + idE1 + " " + idE2);

	            if (previousIdE1 != null) {
	                var incorrect = idE1 != previousIdE1 && idE2 != previousIdE2;
	                if (!incorrect) {
	                    incorrect = (idE1 == previousIdE1 && idE2 != (previousIdE2 + 1) ||
	                            (idE2 == previousIdE2 && idE1 != (previousIdE1 + 1)));
	                }
	                if (incorrect) {
	                    // comment-in when needed: System.out.println("Non-Monotone increase (this is still correct but noteworthy)");
	                    countNotMonotone++;
	                }
	            }

	            previousIdE1 = idE1;
	            previousIdE2 = idE2;
	        }

	        if (useDefault || preserve.Value) {
	            Assert.AreEqual(0, countMultiDeliveries, "multiple row deliveries: " + countMultiDeliveries);
	            // the number of non-monotone delivers should be small but not zero
	            // this is because when the event get generated and when the event actually gets processed may not be in the same order
	            Assert.That(countNotMonotone, Is.LessThan(50), "count not monotone: " + countNotMonotone);
	            Assert.That(delivered.Count, Is.GreaterThanOrEqualTo(197)); // its possible to not have 199 since there may not be events on one side of the join
	        }
	        else {
	            Assert.That(countMultiDeliveries, Is.GreaterThan(0), "multiple row deliveries: " + countMultiDeliveries);
	            Assert.That(countNotMonotone, Is.GreaterThan(5), "count not monotone: " + countNotMonotone);
	        }
	    }

	    public class EventOne
        {
	        private static readonly AtomicLong ATOMIC_LONG = new AtomicLong(1);

	        public EventOne(string key)
	        {
	            Instance = ATOMIC_LONG.IncrementAndGet();
	            Key = key;
	        }

	        public string Key { get; private set; }

	        public long Instance { get; private set; }

	        protected bool Equals(EventOne other)
	        {
	            return string.Equals(Key, other.Key);
	        }

	        public override bool Equals(object obj)
	        {
	            if (ReferenceEquals(null, obj))
	                return false;
	            if (ReferenceEquals(this, obj))
	                return true;
	            if (obj.GetType() != this.GetType())
	                return false;
	            return Equals((EventOne) obj);
	        }

	        public override int GetHashCode()
	        {
	            return (Key != null ? Key.GetHashCode() : 0);
	        }
        }

	    public class EventTwo
        {
	        private static readonly AtomicLong ATOMIC_LONG = new AtomicLong(1);

	        public EventTwo(string key) {
	            Instance = ATOMIC_LONG.IncrementAndGet();
	            Key = key;
	        }

	        public long Instance { get; private set; }

	        public string Key { get; private set; }

	        protected bool Equals(EventTwo other)
	        {
	            return string.Equals(Key, other.Key);
	        }

	        public override bool Equals(object obj)
	        {
	            if (ReferenceEquals(null, obj))
	                return false;
	            if (ReferenceEquals(this, obj))
	                return true;
	            if (obj.GetType() != this.GetType())
	                return false;
	            return Equals((EventTwo) obj);
	        }

	        public override int GetHashCode()
	        {
	            return (Key != null ? Key.GetHashCode() : 0);
	        }
        }
	}
} // end of namespace
