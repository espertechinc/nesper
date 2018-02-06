///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.datetime
{
    [TestFixture]
    public class TestDTProperty
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("SupportDateTime", typeof (SupportDateTime));
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

        #endregion

        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;

        public interface MyInterface
        {
            // ReSharper disable InconsistentNaming
            long StartTS { get; }
            long EndTS { get; }
            // ReSharper restore InconsistentNaming
        }

        public class MyImplOne : MyInterface
        {
            public MyImplOne(String datestr, long duration)
            {
                StartTS = DateTimeParser.ParseDefaultMSec(datestr);
                EndTS = StartTS + duration;
            }

            #region MyInterface Members

            public long StartTS { get; private set; }
            public long EndTS { get; private set; }

            #endregion
        }

        [Test]
        public void TestProp()
        {
            String startTime = "2002-05-30 9:01:02.003";
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(startTime)));

            String[] fields =
                "valmoh,valmoy,valdom,valdow,valdoy,valhod,valmos,valsom,valwye,valyea,val1,val2".Split(',');
            String eplFragment = "select " +
                                 "current_timestamp.GetMinuteOfHour() as valmoh," +
                                 "current_timestamp.GetMonthOfYear() as valmoy," +
                                 "current_timestamp.GetDayOfMonth() as valdom," +
                                 "current_timestamp.GetDayOfWeek() as valdow," +
                                 "current_timestamp.GetDayOfYear() as valdoy," +
                                 "current_timestamp.GetHourOfDay() as valhod," +
                                 "current_timestamp.GetMillisOfSecond() as valmos," +
                                 "current_timestamp.GetSecondOfMinute() as valsom," +
                                 "current_timestamp.GetWeekYear() as valwye," +
                                 "current_timestamp.GetYear() as valyea," +
                                 "utildate.GetHourOfDay() as val1," +
                                 "longdate.GetHourOfDay() as val2" +
                                 " from SupportDateTime";
            EPStatement stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;
            Assert.That(stmtFragment.EventType.GetPropertyType("valmoh"), Is.EqualTo(typeof(int)));
            Assert.That(stmtFragment.EventType.GetPropertyType("valmoy"), Is.EqualTo(typeof(int)));
            Assert.That(stmtFragment.EventType.GetPropertyType("valdom"), Is.EqualTo(typeof(int)));
            Assert.That(stmtFragment.EventType.GetPropertyType("valdow"), Is.EqualTo(typeof(DayOfWeek)));
            Assert.That(stmtFragment.EventType.GetPropertyType("valdoy"), Is.EqualTo(typeof(int)));
            Assert.That(stmtFragment.EventType.GetPropertyType("valhod"), Is.EqualTo(typeof(int)));
            Assert.That(stmtFragment.EventType.GetPropertyType("valmos"), Is.EqualTo(typeof(int)));
            Assert.That(stmtFragment.EventType.GetPropertyType("valsom"), Is.EqualTo(typeof(int)));
            Assert.That(stmtFragment.EventType.GetPropertyType("valwye"), Is.EqualTo(typeof(int)));
            Assert.That(stmtFragment.EventType.GetPropertyType("valyea"), Is.EqualTo(typeof(int)));
            Assert.That(stmtFragment.EventType.GetPropertyType("val1"), Is.EqualTo(typeof(int)));
            Assert.That(stmtFragment.EventType.GetPropertyType("val2"), Is.EqualTo(typeof(int)));

            _epService.EPRuntime.SendEvent(SupportDateTime.Make(startTime));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]
            {
                1, 5, 30, DayOfWeek.Thursday, 150, 9, 3, 2, 22, 2002, 9, 9
            });

            // test Map inheritance via create-schema
            _epService.EPAdministrator.CreateEPL(
                "create schema ParentType as (StartTS long, EndTS long) starttimestamp StartTS endtimestamp EndTS");
            _epService.EPAdministrator.CreateEPL("create schema ChildType as (foo string) inherits ParentType");
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select * from ChildType dt where dt.before(current_timestamp())");
            Assert.AreEqual("StartTS", stmt.EventType.StartTimestampPropertyName);
            Assert.AreEqual("EndTS", stmt.EventType.EndTimestampPropertyName);

            // test PONO inheritance via create-schema
            _epService.EPAdministrator.CreateEPL(string.Format("create schema InterfaceType as {0} starttimestamp StartTS endtimestamp EndTS", typeof (MyInterface).MaskTypeName()));
            _epService.EPAdministrator.CreateEPL(string.Format("create schema DerivedType as {0}", typeof (MyImplOne).MaskTypeName()));
            EPStatement stmtTwo = _epService.EPAdministrator.CreateEPL("select * from DerivedType dt where dt.before(current_timestamp())");
            Assert.AreEqual("StartTS", stmtTwo.EventType.StartTimestampPropertyName);
            Assert.AreEqual("EndTS", stmtTwo.EventType.EndTimestampPropertyName);

            // test incompatible
            _epService.EPAdministrator.CreateEPL(
                "create schema T1 as (StartTS long, EndTS long) starttimestamp StartTS endtimestamp EndTS");
            _epService.EPAdministrator.CreateEPL(
                "create schema T2 as (StartTSOne long, EndTSOne long) starttimestamp StartTSOne endtimestamp EndTSOne");
            try
            {
                _epService.EPAdministrator.CreateEPL("create schema T12 as () inherits T1,T2");
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual(
                    "Error starting statement: Event type declares start timestamp as property 'StartTS' however inherited event type 'T2' declares start timestamp as property 'StartTSOne' [create schema T12 as () inherits T1,T2]",
                    ex.Message);
            }
            try
            {
                _epService.EPAdministrator.CreateEPL(
                    "create schema T12 as (StartTSOne long, EndTSXXX long) inherits T2 starttimestamp StartTSOne endtimestamp EndTSXXX");
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual(
                    "Error starting statement: Event type declares end timestamp as property 'EndTSXXX' however inherited event type 'T2' declares end timestamp as property 'EndTSOne' [create schema T12 as (StartTSOne long, EndTSXXX long) inherits T2 starttimestamp StartTSOne endtimestamp EndTSXXX]",
                    ex.Message);
            }
        }
    }
}
