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
    public class TestDTPlusMinus
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp() {
    
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("SupportDateTime", typeof(SupportDateTime));
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _listener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }
    
        [Test]
        public void TestPlusMinus() {
    
            _epService.EPAdministrator.CreateEPL("create variable long varmsec");
            String startTime = "2002-05-30 9:00:00.000";
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(startTime)));
    
            String[] fields = "val0,val1,val2,val4,val5,val6".Split(',');
            String eplFragment = "select " +
                    "current_timestamp.plus(varmsec) as val0," +
                    "utildate.plus(varmsec) as val1," +
                    "longdate.plus(varmsec) as val2," +
                    "current_timestamp.minus(varmsec) as val4," +
                    "utildate.minus(varmsec) as val5," +
                    "longdate.minus(varmsec) as val6" +
                    " from SupportDateTime";
            EPStatement stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[] { typeof(long?), typeof(DateTimeOffset?), typeof(long?), typeof(long?), typeof(DateTimeOffset?), typeof(long?) });
    
            _epService.EPRuntime.SendEvent(SupportDateTime.Make(null));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{
                SupportDateTime.GetValueCoerced(startTime, "long"), null, null,
                SupportDateTime.GetValueCoerced(startTime, "long"), null, null,
            });
    
            Object[] expectedPlus = SupportDateTime.GetArrayCoerced(startTime, "long", "util", "long");
            Object[] expectedMinus = SupportDateTime.GetArrayCoerced(startTime, "long", "util", "long");
            _epService.EPRuntime.SendEvent(SupportDateTime.Make(startTime));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, EPAssertionUtil.ConcatenateArray(expectedPlus, expectedMinus));
    
            _epService.EPRuntime.SetVariableValue("varmsec", 1000);
            _epService.EPRuntime.SendEvent(SupportDateTime.Make(startTime));
            //Console.WriteLine("===> " + SupportDateTime.Print(listener.AssertOneGetNew().Get("val4")));
            expectedPlus = SupportDateTime.GetArrayCoerced("2002-05-30 09:00:01.000", "long", "util", "long");
            expectedMinus = SupportDateTime.GetArrayCoerced("2002-05-30 08:59:59.000", "long", "util", "long");
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, EPAssertionUtil.ConcatenateArray(expectedPlus, expectedMinus));
    
            _epService.EPRuntime.SetVariableValue("varmsec", 2 * 24 * 60 * 60 * 1000);
            _epService.EPRuntime.SendEvent(SupportDateTime.Make(startTime));
            expectedMinus = SupportDateTime.GetArrayCoerced("2002-05-28 09:00:00.000", "long", "util", "long");
            expectedPlus = SupportDateTime.GetArrayCoerced("2002-06-01 09:00:00.000", "long", "util", "long");
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, EPAssertionUtil.ConcatenateArray(expectedPlus, expectedMinus));
        }
    
        [Test]
        public void TestPlusMinusTimePeriod() {
    
            String startTime = "2002-05-30 9:00:00.000";
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(startTime)));
    
            String[] fields = "val0,val1,val2,val4,val5,val6".Split(',');
            String eplFragment = "select " +
                    "current_timestamp.plus(1 hour 10 sec 20 msec) as val0," +
                    "utildate.plus(1 hour 10 sec 20 msec) as val1," +
                    "longdate.plus(1 hour 10 sec 20 msec) as val2," +
                    "current_timestamp.minus(1 hour 10 sec 20 msec) as val4," +
                    "utildate.minus(1 hour 10 sec 20 msec) as val5," +
                    "longdate.minus(1 hour 10 sec 20 msec) as val6" +
                    " from SupportDateTime";
            EPStatement stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[] { typeof(long?), typeof(DateTimeOffset?), typeof(long?), typeof(long?), typeof(DateTimeOffset?), typeof(long?) });
    
            _epService.EPRuntime.SendEvent(SupportDateTime.Make(startTime));
            Object[] expectedPlus = SupportDateTime.GetArrayCoerced("2002-05-30 10:00:10.020", "long", "util", "long");
            Object[] expectedMinus = SupportDateTime.GetArrayCoerced("2002-05-30 07:59:49.980", "long", "util", "long");
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, EPAssertionUtil.ConcatenateArray(expectedPlus, expectedMinus));
    
            _epService.EPRuntime.SendEvent(SupportDateTime.Make(null));
            expectedPlus = SupportDateTime.GetArrayCoerced("2002-05-30 10:00:10.020", "long", "null", "null");
            expectedMinus = SupportDateTime.GetArrayCoerced("2002-05-30 07:59:49.980", "long", "null", "null");
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, EPAssertionUtil.ConcatenateArray(expectedPlus, expectedMinus));
        }
    }
}
