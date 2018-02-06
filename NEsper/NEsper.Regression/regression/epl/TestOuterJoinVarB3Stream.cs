///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
	public class TestOuterJoinVarB3Stream
    {
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _updateListener;

        private static readonly string EVENT_S0 = typeof(SupportBean_S0).FullName;
        private static readonly string EVENT_S1 = typeof(SupportBean_S1).FullName;
        private static readonly string EVENT_S2 = typeof(SupportBean_S2).FullName;

        [SetUp]
	    public void SetUp() {
	        _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);
	        }
	        _updateListener = new SupportUpdateListener();
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.EndTest();
	        }
	        _updateListener = null;
	    }

        [Test]
	    public void TestOuterInnerJoin_root_s0() {
	        /// <summary>
	        /// Query:
	        /// s0
	        /// </summary>
	        string joinStatement = "select * from " +
	                               EVENT_S0 + "#length(1000) as s0 " +
	                               " left outer join " + EVENT_S1 + "#length(1000) as s1 on s0.p00 = s1.p10 " +
	                               " right outer join " + EVENT_S2 + "#length(1000) as s2 on s0.p00 = s2.p20 ";

	        EPStatement joinView = _epService.EPAdministrator.CreateEPL(joinStatement);
	        joinView.AddListener(_updateListener);

	        RunAsserts();
	    }

        [Test]
	    public void TestOuterInnerJoin_root_s1() {
	        /// <summary>
	        /// Query:
	        /// s0
	        /// </summary>
	        string joinStatement = "select * from " +
	                               EVENT_S1 + "#length(1000) as s1 " +
	                               " right outer join " + EVENT_S0 + "#length(1000) as s0 on s0.p00 = s1.p10 " +
	                               " right outer join " + EVENT_S2 + "#length(1000) as s2 on s0.p00 = s2.p20 ";

	        EPStatement joinView = _epService.EPAdministrator.CreateEPL(joinStatement);
	        joinView.AddListener(_updateListener);

	        RunAsserts();
	    }

        [Test]
	    public void TestOuterInnerJoin_root_s2() {
	        /// <summary>
	        /// Query:
	        /// s0
	        /// </summary>
	        string joinStatement = "select * from " +
	                               EVENT_S2 + "#length(1000) as s2 " +
	                               " left outer join " + EVENT_S0 + "#length(1000) as s0 on s0.p00 = s2.p20 " +
	                               " left outer join " + EVENT_S1 + "#length(1000) as s1 on s0.p00 = s1.p10 ";

	        EPStatement joinView = _epService.EPAdministrator.CreateEPL(joinStatement);
	        joinView.AddListener(_updateListener);

	        RunAsserts();
	    }

	    private void RunAsserts() {
	        object[] s0Events = null;
	        object[] s1Events = null;
	        object[] s2Events = null;

	        // Test s0 ... s1 with 1 rows, s2 with 0 rows
	        //
	        s1Events = SupportBean_S1.MakeS1("A", new string[] {"A-s1-1"});
	        SendEvent(s1Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s0Events = SupportBean_S0.MakeS0("A", new string[] {"A-s0-1"});
	        SendEvent(s0Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        // Test s0 ... s1 with 0 rows, s2 with 1 rows
	        //
	        s2Events = SupportBean_S2.MakeS2("B", new string[] {"B-s2-1"});
	        SendEventsAndReset(s2Events);

	        s0Events = SupportBean_S0.MakeS0("B", new string[] {"B-s0-1"});
	        SendEvent(s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new object[][] { new object[] { s0Events[0], null, s2Events[0] } }, GetAndResetNewEvents());

	        // Test s0 ... s1 with 1 rows, s2 with 1 rows
	        //
	        s1Events = SupportBean_S1.MakeS1("C", new string[] {"C-s1-1"});
	        SendEvent(s1Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s2Events = SupportBean_S2.MakeS2("C", new string[] {"C-s2-1"});
	        SendEventsAndReset(s2Events);

	        s0Events = SupportBean_S0.MakeS0("C", new string[] {"C-s0-1"});
	        SendEvent(s0Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] { new object[] {s0Events[0], s1Events[0], s2Events[0]}}, GetAndResetNewEvents());

	        // Test s0 ... s1 with 2 rows, s2 with 1 rows
	        //
	        s1Events = SupportBean_S1.MakeS1("D", new string[] {"D-s1-1", "D-s1-2"});
	        SendEvent(s1Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s2Events = SupportBean_S2.MakeS2("D", new string[] {"D-s2-1"});
	        SendEventsAndReset(s2Events);

	        s0Events = SupportBean_S0.MakeS0("D", new string[] {"D-s0-1"});
	        SendEvent(s0Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] { s0Events[0], s1Events[0], s2Events[0]},
	            new object[] { s0Events[0], s1Events[1], s2Events[0]}
	        }, GetAndResetNewEvents());

	        // Test s0 ... s1 with 2 rows, s2 with 2 rows
	        //
	        s1Events = SupportBean_S1.MakeS1("E", new string[] {"E-s1-1", "E-s1-2"});
	        SendEvent(s1Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s2Events = SupportBean_S2.MakeS2("E", new string[] {"E-s2-1", "E-s2-2"});
	        SendEventsAndReset(s2Events);

	        s0Events = SupportBean_S0.MakeS0("E", new string[] {"E-s0-1"});
	        SendEvent(s0Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] { s0Events[0], s1Events[0], s2Events[0]},
	            new object[] { s0Events[0], s1Events[1], s2Events[0]},
	            new object[] { s0Events[0], s1Events[0], s2Events[1]},
	            new object[] { s0Events[0], s1Events[1], s2Events[1]}
	        }, GetAndResetNewEvents());

	        // Test s0 ... s1 with 0 rows, s2 with 2 rows
	        //
	        s2Events = SupportBean_S2.MakeS2("F", new string[] {"F-s2-1", "F-s2-2"});
	        SendEventsAndReset(s2Events);

	        s0Events = SupportBean_S0.MakeS0("F", new string[] {"F-s0-1"});
	        SendEvent(s0Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] { s0Events[0], null, s2Events[0]},
	            new object[] { s0Events[0], null, s2Events[1]}
	        }, GetAndResetNewEvents());

	        // Test s1 ... s0 with 0 rows, s2 with 1 rows
	        //
	        s2Events = SupportBean_S2.MakeS2("H", new string[] {"H-s2-1"});
	        SendEventsAndReset(s2Events);

	        s1Events = SupportBean_S1.MakeS1("H", new string[] {"H-s1-1"});
	        SendEvent(s1Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        // Test s1 ... s0 with 1 rows, s2 with 0 rows
	        //
	        s0Events = SupportBean_S0.MakeS0("I", new string[] {"I-s0-1"});
	        SendEventsAndReset(s0Events);

	        s1Events = SupportBean_S1.MakeS1("I", new string[] {"I-s1-1"});
	        SendEvent(s1Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        // Test s1 ... s0 with 1 rows, s2 with 1 rows
	        //
	        s0Events = SupportBean_S0.MakeS0("J", new string[] {"J-s0-1"});
	        SendEventsAndReset(s0Events);

	        s2Events = SupportBean_S2.MakeS2("J", new string[] {"J-s2-1"});
	        SendEventsAndReset(s2Events);

	        s1Events = SupportBean_S1.MakeS1("J", new string[] {"J-s1-1"});
	        SendEvent(s1Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] { s0Events[0], s1Events[0], s2Events[0]}
	        }, GetAndResetNewEvents());

	        // Test s1 ... s0 with 1 rows, s2 with 2 rows
	        //
	        s0Events = SupportBean_S0.MakeS0("K", new string[] {"K-s0-1"});
	        SendEventsAndReset(s0Events);

	        s2Events = SupportBean_S2.MakeS2("K", new string[] {"K-s2-1","K-s2-2"});
	        SendEventsAndReset(s2Events);

	        s1Events = SupportBean_S1.MakeS1("K", new string[] {"K-s1-1"});
	        SendEvent(s1Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] { s0Events[0], s1Events[0], s2Events[0]},
	            new object[] { s0Events[0], s1Events[0], s2Events[1]}
	        }, GetAndResetNewEvents());

	        // Test s1 ... s0 with 2 rows, s2 with 2 rows
	        //
	        s0Events = SupportBean_S0.MakeS0("L", new string[] {"L-s0-1", "L-s0-2"});
	        SendEventsAndReset(s0Events);

	        s2Events = SupportBean_S2.MakeS2("L", new string[] {"L-s2-1","L-s2-2"});
	        SendEventsAndReset(s2Events);

	        s1Events = SupportBean_S1.MakeS1("L", new string[] {"L-s1-1"});
	        SendEvent(s1Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] { s0Events[0], s1Events[0], s2Events[0]},
	            new object[] { s0Events[0], s1Events[0], s2Events[1]},
	            new object[] { s0Events[1], s1Events[0], s2Events[0]},
	            new object[] { s0Events[1], s1Events[0], s2Events[1]}
	        }, GetAndResetNewEvents());

	        // Test s2 ... s0 with 0 rows, s1 with 1 rows
	        //
	        s1Events = SupportBean_S1.MakeS1("P", new string[] {"P-s1-1"});
	        SendEventsAndReset(s1Events);

	        s2Events = SupportBean_S2.MakeS2("P", new string[] {"P-s2-1"});
	        SendEvent(s2Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] { null, null, s2Events[0]}
	        }, GetAndResetNewEvents());

	        // Test s2 ... s1 with 0 rows, s0 with 1 rows
	        //
	        s0Events = SupportBean_S0.MakeS0("Q", new string[] {"Q-s0-1"});
	        SendEventsAndReset(s0Events);

	        s2Events = SupportBean_S2.MakeS2("Q", new string[] {"Q-s2-1"});
	        SendEvent(s2Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] { s0Events[0], null, s2Events[0]}
	        }, GetAndResetNewEvents());

	        // Test s2 ... s1 with 1 rows, s0 with 1 rows
	        //
	        s0Events = SupportBean_S0.MakeS0("R", new string[] {"R-s0-1"});
	        SendEventsAndReset(s0Events);

	        s1Events = SupportBean_S1.MakeS1("R", new string[] {"R-s1-1"});
	        SendEventsAndReset(s1Events);

	        s2Events = SupportBean_S2.MakeS2("R", new string[] {"R-s2-1"});
	        SendEvent(s2Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] { s0Events[0], s1Events[0], s2Events[0]}
	        }, GetAndResetNewEvents());

	        // Test s2 ... s1 with 2 rows, s0 with 1 rows
	        //
	        s0Events = SupportBean_S0.MakeS0("S", new string[] {"S-s0-1"});
	        SendEventsAndReset(s0Events);

	        s1Events = SupportBean_S1.MakeS1("S", new string[] {"S-s1-1", "S-s1-2"});
	        SendEventsAndReset(s1Events);

	        s2Events = SupportBean_S2.MakeS2("S", new string[] {"S-s2-1"});
	        SendEvent(s2Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] { s0Events[0], s1Events[0], s2Events[0]},
	            new object[] { s0Events[0], s1Events[1], s2Events[0]}
	        }, GetAndResetNewEvents());

	        // Test s2 ... s1 with 0 rows, s0 with 2 rows
	        //
	        s0Events = SupportBean_S0.MakeS0("T", new string[] {"T-s0-1", "T-s0-1"});
	        SendEventsAndReset(s0Events);

	        s2Events = SupportBean_S2.MakeS2("T", new string[] {"T-s2-1"});
	        SendEvent(s2Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] { s0Events[0], null, s2Events[0]},
	            new object[] { s0Events[1], null, s2Events[0]}
	        }, GetAndResetNewEvents());

	        // Test s2 ... s1 with 1 rows, s0 with 2 rows
	        //
	        s0Events = SupportBean_S0.MakeS0("U", new string[] {"U-s0-1", "U-s0-1"});
	        SendEventsAndReset(s0Events);

	        s1Events = SupportBean_S1.MakeS1("U", new string[] {"U-s1-1"});
	        SendEventsAndReset(s1Events);

	        s2Events = SupportBean_S2.MakeS2("U", new string[] {"U-s2-1"});
	        SendEvent(s2Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] { s0Events[0], s1Events[0], s2Events[0]},
	            new object[] { s0Events[1], s1Events[0], s2Events[0]}
	        }, GetAndResetNewEvents());

	        // Test s2 ... s1 with 2 rows, s0 with 2 rows
	        //
	        s0Events = SupportBean_S0.MakeS0("V", new string[] {"V-s0-1", "V-s0-1"});
	        SendEventsAndReset(s0Events);

	        s1Events = SupportBean_S1.MakeS1("V", new string[] {"V-s1-1", "V-s1-1"});
	        SendEventsAndReset(s1Events);

	        s2Events = SupportBean_S2.MakeS2("V", new string[] {"V-s2-1"});
	        SendEvent(s2Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] { s0Events[0], s1Events[0], s2Events[0]},
	            new object[] { s0Events[0], s1Events[1], s2Events[0]},
	            new object[] { s0Events[1], s1Events[0], s2Events[0]},
	            new object[] { s0Events[1], s1Events[1], s2Events[0]}
	        }, GetAndResetNewEvents());
	    }

	    private void SendEvent(object theEvent) {
	        _epService.EPRuntime.SendEvent(theEvent);
	    }

	    private void SendEventsAndReset(object[] events) {
	        SendEvent(events);
	        _updateListener.Reset();
	    }

	    private void SendEvent(object[] events) {
	        for (int i = 0; i < events.Length; i++) {
	            _epService.EPRuntime.SendEvent(events[i]);
	        }
	    }

        private object[][] GetAndResetNewEvents()
        {
            EventBean[] newEvents = _updateListener.LastNewData;
            Assert.IsNotNull(newEvents, "no events received");
            _updateListener.Reset();
            return ArrayHandlingUtil.GetUnderlyingEvents(
                newEvents, new string[]
                {
                    "s0",
                    "s1",
                    "s2"
                });
        }
    }
} // end of namespace
