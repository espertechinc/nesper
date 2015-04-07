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
using com.espertech.esper.client.soda;
using com.espertech.esper.client.time;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.util;

using NUnit.Framework;

using SupportSubscriberMRD = com.espertech.esper.client.scopetest.SupportSubscriberMRD;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestForGroupDelivery
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;

        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("SupportBean", typeof(SupportBean));

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

        [Test]
        public void TestInvalid()
        {
            TryInvalid("select * from SupportBean for ",
                       "Incorrect syntax near end-of-input expecting an identifier but found end-of-input at line 1 column 30 [select * from SupportBean for ]");

            TryInvalid("select * from SupportBean for other_keyword",
                       "Error starting statement: Expected any of the [grouped_delivery, discrete_delivery] for-clause keywords after reserved keyword 'for' [select * from SupportBean for other_keyword]");

            TryInvalid("select * from SupportBean for grouped_delivery",
                       "Error starting statement: The for-clause with the grouped_delivery keyword requires one or more grouping expressions [select * from SupportBean for grouped_delivery]");

            TryInvalid("select * from SupportBean for grouped_delivery()",
                       "Error starting statement: The for-clause with the grouped_delivery keyword requires one or more grouping expressions [select * from SupportBean for grouped_delivery()]");

            TryInvalid("select * from SupportBean for grouped_delivery(dummy)",
                       "Error starting statement: Failed to validate for-clause expression 'dummy': Property named 'dummy' is not valid in any stream [select * from SupportBean for grouped_delivery(dummy)]");

            TryInvalid("select * from SupportBean for discrete_delivery(dummy)",
                       "Error starting statement: The for-clause with the discrete_delivery keyword does not allow grouping expressions [select * from SupportBean for discrete_delivery(dummy)]");

            TryInvalid("select * from SupportBean for discrete_delivery for grouped_delivery(IntPrimitive)",
                       "Incorrect syntax near 'for' (a reserved keyword) at line 1 column 48");
        }

        private void TryInvalid(String epl, String message)
        {
            try
            {
                _epService.EPAdministrator.CreateEPL(epl);
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                SupportMessageAssertUtil.AssertMessage(ex, message);
            }
        }

        [Test]
        public void TestSubscriberOnly()
        {
            var subscriber = new SupportSubscriberMRD();
            SendTimer(0);
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select irstream TheString,IntPrimitive from SupportBean.win:time_batch(1) for discrete_delivery");
            stmt.Subscriber = subscriber;

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 1));
            SendTimer(1000);
            Assert.AreEqual(3, subscriber.InsertStreamList.Count);
            EPAssertionUtil.AssertEqualsExactOrder(new Object[] { "E1", 1 }, subscriber.InsertStreamList[0][0]);
            EPAssertionUtil.AssertEqualsExactOrder(new Object[] { "E2", 2 }, subscriber.InsertStreamList[1][0]);
            EPAssertionUtil.AssertEqualsExactOrder(new Object[] { "E3", 1 }, subscriber.InsertStreamList[2][0]);

            stmt.Dispose();
            subscriber.Reset();
            stmt = _epService.EPAdministrator.CreateEPL("select irstream TheString,IntPrimitive from SupportBean.win:time_batch(1) for Grouped_delivery(IntPrimitive)");
            stmt.Subscriber = subscriber;

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 1));
            SendTimer(2000);
            Assert.AreEqual(2, subscriber.InsertStreamList.Count);
            Assert.AreEqual(2, subscriber.RemoveStreamList.Count);
            EPAssertionUtil.AssertEqualsExactOrder(new Object[] { "E1", 1 }, subscriber.InsertStreamList[0][0]);
            EPAssertionUtil.AssertEqualsExactOrder(new Object[] { "E3", 1 }, subscriber.InsertStreamList[0][1]);
            EPAssertionUtil.AssertEqualsExactOrder(new Object[] { "E2", 2 }, subscriber.InsertStreamList[1][0]);
            EPAssertionUtil.AssertEqualsExactOrder(new Object[] { "E1", 1 }, subscriber.RemoveStreamList[0][0]);
            EPAssertionUtil.AssertEqualsExactOrder(new Object[] { "E3", 1 }, subscriber.RemoveStreamList[0][1]);
            EPAssertionUtil.AssertEqualsExactOrder(new Object[] { "E2", 2 }, subscriber.RemoveStreamList[1][0]);
        }

        [Test]
        public void TestDiscreteDelivery()
        {
            SendTimer(0);
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select * from SupportBean.win:time_batch(1) for discrete_delivery");
            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 1));
            SendTimer(1000);
            Assert.AreEqual(3, _listener.NewDataList.Count);
            EPAssertionUtil.AssertPropsPerRow(_listener.NewDataList[0], "TheString,IntPrimitive".Split(','), new Object[][] { new Object[] { "E1", 1 } });
            EPAssertionUtil.AssertPropsPerRow(_listener.NewDataList[1], "TheString,IntPrimitive".Split(','), new Object[][] { new Object[] { "E2", 2 } });
            EPAssertionUtil.AssertPropsPerRow(_listener.NewDataList[2], "TheString,IntPrimitive".Split(','), new Object[][] { new Object[] { "E3", 1 } });

            _listener.Reset();

            // test no-event delivery
            _epService.EPAdministrator.Configuration.AddEventType<object>("ObjectEvent");
            String epl = "SELECT *  FROM ObjectEvent OUTPUT ALL EVERY 1 seconds for discrete_delivery";

            _epService.EPAdministrator.CreateEPL(epl).Events += _listener.Update;
            _epService.EPRuntime.SendEvent(new Object());
            SendTimer(2000);
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
            SendTimer(3000);
            Assert.IsFalse(_listener.GetAndClearIsInvoked());
        }

        [Test]
        public void TestGroupDelivery()
        {
            SendTimer(0);
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select * from SupportBean.win:time_batch(1) for grouped_delivery (IntPrimitive)");
            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 1));
            SendTimer(1000);
            Assert.AreEqual(2, _listener.NewDataList.Count);
            Assert.AreEqual(2, _listener.NewDataList[0].Length);
            EPAssertionUtil.AssertPropsPerRow(_listener.NewDataList[0], "TheString,IntPrimitive".Split(','), new Object[][] { new Object[] { "E1", 1 }, new Object[] { "E3", 1 } });
            Assert.AreEqual(1, _listener.NewDataList[1].Length);
            EPAssertionUtil.AssertPropsPerRow(_listener.NewDataList[1], "TheString,IntPrimitive".Split(','), new Object[][] { new Object[] { "E2", 2 } });

            // test sorted
            stmt.Dispose();
            stmt = _epService.EPAdministrator.CreateEPL("select * from SupportBean.win:time_batch(1) order by IntPrimitive desc for grouped_delivery (IntPrimitive)");
            stmt.Events += _listener.Update;
            _listener.Reset();

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 1));
            SendTimer(2000);
            Assert.AreEqual(2, _listener.NewDataList.Count);
            Assert.AreEqual(1, _listener.NewDataList[0].Length);
            EPAssertionUtil.AssertPropsPerRow(_listener.NewDataList[0], "TheString,IntPrimitive".Split(','), new Object[][] { new Object[] { "E2", 2 } });
            Assert.AreEqual(2, _listener.NewDataList[1].Length);
            EPAssertionUtil.AssertPropsPerRow(_listener.NewDataList[1], "TheString,IntPrimitive".Split(','), new Object[][] { new Object[] { "E1", 1 }, new Object[] { "E3", 1 } });

            // test multiple criteria
            stmt.Dispose();
            String stmtText = "select TheString, DoubleBoxed, EnumValue from SupportBean.win:time_batch(1) order by TheString, DoubleBoxed, EnumValue for grouped_delivery(DoubleBoxed, EnumValue)";
            stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
            _listener.Reset();

            SendEvent("E1", 10d, SupportEnum.ENUM_VALUE_2); // A (1)
            SendEvent("E2", 11d, SupportEnum.ENUM_VALUE_1); // B (2)
            SendEvent("E3", 9d, SupportEnum.ENUM_VALUE_2);  // C (3)
            SendEvent("E4", 10d, SupportEnum.ENUM_VALUE_2); // A
            SendEvent("E5", 10d, SupportEnum.ENUM_VALUE_1); // D (4)
            SendEvent("E6", 10d, SupportEnum.ENUM_VALUE_1); // D
            SendEvent("E7", 11d, SupportEnum.ENUM_VALUE_1); // B
            SendEvent("E8", 10d, SupportEnum.ENUM_VALUE_1); // D
            SendTimer(3000);
            Assert.AreEqual(4, _listener.NewDataList.Count);
            String[] fields = "TheString,DoubleBoxed,EnumValue".Split(',');
            EPAssertionUtil.AssertPropsPerRow(_listener.NewDataList[0], fields,
                    new Object[][] { new Object[] { "E1", 10d, SupportEnum.ENUM_VALUE_2 }, new Object[] { "E4", 10d, SupportEnum.ENUM_VALUE_2 } });
            EPAssertionUtil.AssertPropsPerRow(_listener.NewDataList[1], fields,
                    new Object[][] { new Object[] { "E2", 11d, SupportEnum.ENUM_VALUE_1 }, new Object[] { "E7", 11d, SupportEnum.ENUM_VALUE_1 } });
            EPAssertionUtil.AssertPropsPerRow(_listener.NewDataList[2], fields,
                    new Object[][] { new Object[] { "E3", 9d, SupportEnum.ENUM_VALUE_2 } });
            EPAssertionUtil.AssertPropsPerRow(_listener.NewDataList[3], fields,
                    new Object[][] { new Object[] { "E5", 10d, SupportEnum.ENUM_VALUE_1 }, new Object[] { "E6", 10d, SupportEnum.ENUM_VALUE_1 }, new Object[] { "E8", 10d, SupportEnum.ENUM_VALUE_1 } });

            // test SODA
            stmt.Dispose();
            _listener.Reset();
            EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(stmtText);
            Assert.AreEqual(stmtText, model.ToEPL());
            stmt = _epService.EPAdministrator.Create(model);
            stmt.Events += _listener.Update;

            SendEvent("E1", 10d, SupportEnum.ENUM_VALUE_2); // A (1)
            SendEvent("E2", 11d, SupportEnum.ENUM_VALUE_1); // B (2)
            SendEvent("E3", 11d, SupportEnum.ENUM_VALUE_1); // B (2)
            SendTimer(4000);
            Assert.AreEqual(2, _listener.NewDataList.Count);
            EPAssertionUtil.AssertPropsPerRow(_listener.NewDataList[0], fields,
                    new Object[][] { new Object[] { "E1", 10d, SupportEnum.ENUM_VALUE_2 } });
            EPAssertionUtil.AssertPropsPerRow(_listener.NewDataList[1], fields,
                    new Object[][] { new Object[] { "E2", 11d, SupportEnum.ENUM_VALUE_1 }, new Object[] { "E3", 11d, SupportEnum.ENUM_VALUE_1 } });
        }

        private void SendTimer(long timeInMSec)
        {
            CurrentTimeEvent theEvent = new CurrentTimeEvent(timeInMSec);
            EPRuntime runtime = _epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }

        private void SendEvent(String stringValue, Double doubleBoxed, SupportEnum enumVal)
        {
            SupportBean bean = new SupportBean();
            bean.TheString = stringValue;
            bean.DoubleBoxed = doubleBoxed;
            bean.EnumValue = enumVal;
            _epService.EPRuntime.SendEvent(bean);
        }
    }
}
