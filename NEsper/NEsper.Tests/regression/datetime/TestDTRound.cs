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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.bean.lambda;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.datetime
{
    [TestFixture]
    public class TestDTRound
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;

        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("SupportDateTime", typeof(SupportDateTime));
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _listener = new SupportUpdateListener();
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        [Test]
        public void TestInput()
        {

            String[] fields = "val0,val1".Split(',');
            String eplFragment = "select " +
                    "utildate.roundCeiling('hour') as val0," +
                    "msecdate.roundCeiling('hour') as val1" +
                    " from SupportDateTime";
            EPStatement stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[] { typeof(DateTimeOffset?), typeof(long?) });

            String startTime = "2002-05-30 09:01:02.003";
            String expectedTime = "2002-5-30 10:00:00.000";
            _epService.EPRuntime.SendEvent(SupportDateTime.Make(startTime));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, SupportDateTime.GetArrayCoerced(expectedTime, "util", "msec"));
        }

        [Test]
        public void TestRoundCeil()
        {
            String[] fields = "val0,val1,val2,val3,val4,val5,val6".Split(',');
            String eplFragment = "select " +
                    "utildate.roundCeiling('msec') as val0," +
                    "utildate.roundCeiling('sec') as val1," +
                    "utildate.roundCeiling('minutes') as val2," +
                    "utildate.roundCeiling('hour') as val3," +
                    "utildate.roundCeiling('day') as val4," +
                    "utildate.roundCeiling('month') as val5," +
                    "utildate.roundCeiling('year') as val6" +
                    " from SupportDateTime";
            EPStatement stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[] { typeof(DateTimeOffset?), typeof(DateTimeOffset?), typeof(DateTimeOffset?), typeof(DateTimeOffset?), typeof(DateTimeOffset?), typeof(DateTimeOffset?), typeof(DateTimeOffset?) });

            String[] expected = {
                    "2002-05-30 09:01:02.003",
                    "2002-05-30 09:01:03.000",
                    "2002-05-30 09:02:00.000",
                    "2002-05-30 10:00:00.000",
                    "2002-05-31 00:00:00.000",
                    "2002-06-1 00:00:00.000",
                    "2003-01-1 00:00:00.000",
            };
            String startTime = "2002-05-30 09:01:02.003";
            _epService.EPRuntime.SendEvent(SupportDateTime.Make(startTime));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, SupportDateTime.GetArrayCoerced(expected, "util"));
        }

        [Test]
        public void TestroundFloor()
        {

            String[] fields = "val0,val1,val2,val3,val4,val5,val6".Split(',');
            String eplFragment = "select " +
                    "utildate.roundFloor('msec') as val0," +
                    "utildate.roundFloor('sec') as val1," +
                    "utildate.roundFloor('minutes') as val2," +
                    "utildate.roundFloor('hour') as val3," +
                    "utildate.roundFloor('day') as val4," +
                    "utildate.roundFloor('month') as val5," +
                    "utildate.roundFloor('year') as val6" +
                    " from SupportDateTime";
            EPStatement stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[] { typeof(DateTimeOffset?), typeof(DateTimeOffset?), typeof(DateTimeOffset?), typeof(DateTimeOffset?), typeof(DateTimeOffset?), typeof(DateTimeOffset?), typeof(DateTimeOffset?) });

            String[] expected = {
                    "2002-05-30 09:01:02.003",
                    "2002-05-30 09:01:02.000",
                    "2002-05-30 09:01:00.000",
                    "2002-05-30 9:00:00.000",
                    "2002-05-30 00:00:00.000",
                    "2002-05-1 00:00:00.000",
                    "2002-01-1 00:00:00.000",
            };
            String startTime = "2002-05-30 09:01:02.003";
            _epService.EPRuntime.SendEvent(SupportDateTime.Make(startTime));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, SupportDateTime.GetArrayCoerced(expected, "util"));
        }

        [Test]
        public void TestroundHalf()
        {

            String[] fields = "val0,val1,val2,val3,val4,val5,val6".Split(',');
            String eplFragment = "select " +
                    "utildate.roundHalf('msec') as val0," +
                    "utildate.roundHalf('sec') as val1," +
                    "utildate.roundHalf('minutes') as val2," +
                    "utildate.roundHalf('hour') as val3," +
                    "utildate.roundHalf('day') as val4," +
                    "utildate.roundHalf('month') as val5," +
                    "utildate.roundHalf('year') as val6" +
                    " from SupportDateTime";
            EPStatement stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[] { typeof(DateTimeOffset?), typeof(DateTimeOffset?), typeof(DateTimeOffset?), typeof(DateTimeOffset?), typeof(DateTimeOffset?), typeof(DateTimeOffset?), typeof(DateTimeOffset?) });

            String[] expected = {
                    "2002-05-30 15:30:02.550",
                    "2002-05-30 15:30:03.000",
                    "2002-05-30 15:30:00.000",
                    "2002-05-30 16:00:00.00",
                    "2002-05-31 00:00:00.000",
                    "2002-06-01 00:00:00.000",
                    "2002-01-01 00:00:00.000",
            };
            String startTime = "2002-05-30 15:30:02.550";
            _epService.EPRuntime.SendEvent(SupportDateTime.Make(startTime));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, SupportDateTime.GetArrayCoerced(expected, "util"));

            // test rounding up/down
            stmtFragment.Dispose();
            fields = "val0".Split(',');
            eplFragment = "select utildate.roundHalf('min') as val0 from SupportDateTime";
            stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(SupportDateTime.Make("2002-05-30 15:30:29.999"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { SupportDateTime.GetValueCoerced("2002-05-30 15:30:00.000", "util") });

            _epService.EPRuntime.SendEvent(SupportDateTime.Make("2002-05-30 15:30:30.000"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { SupportDateTime.GetValueCoerced("2002-05-30 15:31:00.000", "util") });

            _epService.EPRuntime.SendEvent(SupportDateTime.Make("2002-05-30 15:30:30.001"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { SupportDateTime.GetValueCoerced("2002-05-30 15:31:00.000", "util") });
        }
    }
}
