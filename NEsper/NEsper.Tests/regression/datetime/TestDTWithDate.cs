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
using com.espertech.esper.support.bean;
using com.espertech.esper.support.bean.lambda;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.datetime
{
    [TestFixture]
    public class TestDTWithDate
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
        public void TestWithDate()
        {
            _epService.EPAdministrator.CreateEPL("create variable int varyear");
            _epService.EPAdministrator.CreateEPL("create variable int varmonth");
            _epService.EPAdministrator.CreateEPL("create variable int varday");

            String startTime = "2002-05-30 09:00:00.000";
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(startTime)));

            String[] fields = "val0,val1,val2".Split(',');
            String eplFragment = "select " +
                    "current_timestamp.withDate(varyear, varmonth, varday) as val0," +
                    "utildate.withDate(varyear, varmonth, varday) as val1," +
                    "msecdate.withDate(varyear, varmonth, varday) as val2" +
                    " from SupportDateTime";
            EPStatement stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[] { typeof(long?), typeof(DateTimeOffset?), typeof(long?) });

            _epService.EPRuntime.SendEvent(SupportDateTime.Make(null));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
                new Object[] { SupportDateTime.GetValueCoerced(startTime, "msec"), null, null });

            String expectedTime = "2004-09-03 09:00:00.000";
            _epService.EPRuntime.SetVariableValue("varyear", 2004);
            _epService.EPRuntime.SetVariableValue("varmonth", 9);
            _epService.EPRuntime.SetVariableValue("varday", 3);
            _epService.EPRuntime.SendEvent(SupportDateTime.Make(startTime));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
                SupportDateTime.GetArrayCoerced(expectedTime, "msec", "util", "msec"));

            expectedTime = "2002-09-30 09:00:00.000";
            _epService.EPRuntime.SetVariableValue("varyear", null);
            _epService.EPRuntime.SetVariableValue("varmonth", 9);
            _epService.EPRuntime.SetVariableValue("varday", null);
            _epService.EPRuntime.SendEvent(SupportDateTime.Make(startTime));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
                SupportDateTime.GetArrayCoerced(expectedTime, "msec", "util", "msec"));
        }
    }
}
