///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestViewExternallyBatched
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;

        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType(typeof (MyEvent));
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            _listener = new SupportUpdateListener();
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }

        private void RunAssertionWithRefTime(String epl)
        {
            String[] fields = "id".Split(',');
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(MyEvent.MakeTime("E1", "08:00:00.000"));
            _epService.EPRuntime.SendEvent(MyEvent.MakeTime("E2", "08:00:04.999"));
            Assert.IsFalse(_listener.IsInvoked);

            _epService.EPRuntime.SendEvent(MyEvent.MakeTime("E3", "08:00:05.000"));
            EPAssertionUtil.AssertPropsPerRow(
                _listener.AssertInvokedAndReset(), fields,
                new Object[][]
                {
                    new Object[]
                    {
                        "E1"
                    }, new Object[]
                    {
                        "E2"
                    }
                }, null);

            _epService.EPRuntime.SendEvent(MyEvent.MakeTime("E4", "08:00:04.000"));
            _epService.EPRuntime.SendEvent(MyEvent.MakeTime("E5", "07:00:00.000"));
            _epService.EPRuntime.SendEvent(MyEvent.MakeTime("E6", "08:01:04.999"));
            Assert.IsFalse(_listener.IsInvoked);

            _epService.EPRuntime.SendEvent(MyEvent.MakeTime("E7", "08:01:05.000"));
            EPAssertionUtil.AssertPropsPerRow(
                _listener.AssertInvokedAndReset(), fields,
                new Object[][]
                {
                    new Object[] { "E3" },
                    new Object[] { "E4" },
                    new Object[] { "E5" }, 
                    new Object[] { "E6" }
                }, 
                new Object[][]
                {
                    new Object[] { "E1" }, 
                    new Object[] { "E2" }
                });

            _epService.EPRuntime.SendEvent(MyEvent.MakeTime("E8", "08:03:55.000"));
            EPAssertionUtil.AssertPropsPerRow(
                _listener.AssertInvokedAndReset(), fields,
                new Object[][]
                {
                    new Object[] { "E7" }
                }, 
                new Object[][]
                {
                    new Object[] { "E3" }, 
                    new Object[] { "E4" },
                    new Object[] { "E5" },
                    new Object[] { "E6" }
                });

            _epService.EPRuntime.SendEvent(MyEvent.MakeTime("E9", "00:00:00.000"));
            _epService.EPRuntime.SendEvent(MyEvent.MakeTime("E10", "08:04:04.999"));
            _epService.EPRuntime.SendEvent(MyEvent.MakeTime("E11", "08:04:05.000"));
            EPAssertionUtil.AssertPropsPerRow(
                _listener.AssertInvokedAndReset(), fields,
                new Object[][]
                {
                    new Object[] { "E8" },
                    new Object[] { "E9" },
                    new Object[] { "E10" }
                }, 
                new Object[][]
                {
                    new Object[] { "E7" }
                });

            stmt.Dispose();
        }

        [Test]
        public void TestExtBatchedNoReference()
        {
            String[] fields = "id".Split(',');
            EPStatement stmt =
                _epService.EPAdministrator.CreateEPL(
                    "select irstream * from MyEvent.win:ext_timed_batch(mytimestamp, 1 minute)");
            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(MyEvent.MakeTime("E1", "08:00:00.000"));
            _epService.EPRuntime.SendEvent(MyEvent.MakeTime("E2", "08:00:30.000"));
            _epService.EPRuntime.SendEvent(MyEvent.MakeTime("E3", "08:00:59.999"));
            Assert.IsFalse(_listener.IsInvoked);

            _epService.EPRuntime.SendEvent(MyEvent.MakeTime("E4", "08:01:00.000"));
            EPAssertionUtil.AssertPropsPerRow(
                _listener.AssertInvokedAndReset(), fields,
                new Object[][]
                {
                    new Object[] { "E1" }, 
                    new Object[] { "E2" }, 
                    new Object[] { "E3" }
                }, null);

            _epService.EPRuntime.SendEvent(MyEvent.MakeTime("E5", "08:01:02.000"));
            _epService.EPRuntime.SendEvent(MyEvent.MakeTime("E6", "08:01:05.000"));
            _epService.EPRuntime.SendEvent(MyEvent.MakeTime("E7", "08:02:00.000"));
            EPAssertionUtil.AssertPropsPerRow(
                _listener.AssertInvokedAndReset(), fields,
                new Object[][]
                {
                    new Object[] { "E4" }, 
                    new Object[] { "E5" }, 
                    new Object[] { "E6" }
                },
                new Object[][]
                {
                    new Object[] { "E1" }, 
                    new Object[] { "E2" }, 
                    new Object[] { "E3" }
                });

            _epService.EPRuntime.SendEvent(MyEvent.MakeTime("E8", "08:03:59.000"));
            EPAssertionUtil.AssertPropsPerRow(
                _listener.AssertInvokedAndReset(), fields,
                new Object[][]
                {
                    new Object[] { "E7" }
                },
                new Object[][]
                {
                    new Object[] { "E4" }, 
                    new Object[] { "E5" },
                    new Object[] { "E6" }
                });

            _epService.EPRuntime.SendEvent(MyEvent.MakeTime("E9", "08:03:59.000"));
            Assert.IsFalse(_listener.IsInvoked);

            _epService.EPRuntime.SendEvent(MyEvent.MakeTime("E10", "08:04:00.000"));
            EPAssertionUtil.AssertPropsPerRow(
                _listener.AssertInvokedAndReset(), fields,
                new Object[][]
                {
                    new Object[] { "E8" },
                    new Object[] { "E9" }
                },
                new Object[][]
                {
                    new Object[] { "E7" }
                });

            _epService.EPRuntime.SendEvent(MyEvent.MakeTime("E11", "08:06:30.000"));
            EPAssertionUtil.AssertPropsPerRow(
                _listener.AssertInvokedAndReset(), fields,
                new Object[][]
                {
                    new Object[] { "E10" }
                },
                new Object[][]
                {
                    new Object[] { "E8" },
                    new Object[] { "E9" }
                });

            _epService.EPRuntime.SendEvent(MyEvent.MakeTime("E12", "08:06:59.999"));
            Assert.IsFalse(_listener.IsInvoked);

            _epService.EPRuntime.SendEvent(MyEvent.MakeTime("E13", "08:07:00.001"));
            EPAssertionUtil.AssertPropsPerRow(
                _listener.AssertInvokedAndReset(), fields,
                new Object[][]
                {
                    new Object[] { "E11" },
                    new Object[] { "E12" }
                },
                new Object[][]
                {
                    new Object[] { "E10" }
                });
        }

        [Test]
        public void TestExtBatchedWithRefTime()
        {
            String epl = "select irstream * from MyEvent.win:ext_timed_batch(mytimestamp, 1 minute, 5000)";
            RunAssertionWithRefTime(epl);

            epl = "select irstream * from MyEvent.win:ext_timed_batch(mytimestamp, 1 minute, 65000)";
            RunAssertionWithRefTime(epl);
        }

        [Serializable]
        public class MyEvent
        {
            public MyEvent(String id, long mytimestamp)
            {
                Id = id;
                Mytimestamp = mytimestamp;
            }

            public string Id { get; private set; }

            public long Mytimestamp { get; private set; }

            public static MyEvent MakeTime(String id, String mytime)
            {
                long msec = DateTimeParser.ParseDefaultMSec("2002-05-1T" + mytime);
                return new MyEvent(id, msec);
            }
        }

    }
}
