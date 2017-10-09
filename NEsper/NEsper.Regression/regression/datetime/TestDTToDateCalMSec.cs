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
using com.espertech.esper.supportregression.bean.lambda;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.datetime
{
    [TestFixture]
    public class TestDTToDateCalMSec
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
            _listener = null;
        }

        [Test]
        public void TestToDateCalMilli()
        {
            String startTime = "2002-05-30 09:00:00.000";
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(startTime)));
    
            String[] fields = "val0,val1,val2,val4,val5,val6,val8,val9,val10".Split(',');
            String eplFragment = "select " +
                    "current_timestamp.toDate() as val0," +
                    "utildate.toDate() as val1," +
                    "longdate.toDate() as val2," +
                    "current_timestamp.toCalendar() as val4," +
                    "utildate.toCalendar() as val5," +
                    "longdate.toCalendar() as val6," +
                    "current_timestamp.toMillisec() as val8," +
                    "utildate.toMillisec() as val9," +
                    "longdate.toMillisec() as val10" +
                    " from SupportDateTime";
            EPStatement stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[]{
                typeof(DateTimeOffset), typeof(DateTimeOffset), typeof(DateTimeOffset), 
                typeof(DateTimeEx), typeof(DateTimeEx), typeof(DateTimeEx),
                typeof(long), typeof(long), typeof(long)
            });
    
            _epService.EPRuntime.SendEvent(SupportDateTime.Make(startTime));
            Object[] expectedUtil = SupportDateTime.GetArrayCoerced(startTime, "util", "util", "util");
            Object[] expectedCal = SupportDateTime.GetArrayCoerced(startTime, "cal", "cal", "cal");
            Object[] expectedMsec = SupportDateTime.GetArrayCoerced(startTime, "long", "long", "long");
            Object[] expected = EPAssertionUtil.ConcatenateArray(expectedUtil, expectedCal, expectedMsec);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, expected);
    
            _epService.EPRuntime.SendEvent(SupportDateTime.Make(null));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{
                    SupportDateTime.GetValueCoerced(startTime, "util"), null, null,
                    SupportDateTime.GetValueCoerced(startTime, "cal"), null, null,
                    SupportDateTime.GetValueCoerced(startTime, "long"), null, null,
            });
        }
    }
}
