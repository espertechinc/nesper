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
    public class TestDTWithTime
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
        public void TestWithTime()
        {
            _epService.EPAdministrator.CreateEPL("create variable int varhour");
            _epService.EPAdministrator.CreateEPL("create variable int varmin");
            _epService.EPAdministrator.CreateEPL("create variable int varsec");
            _epService.EPAdministrator.CreateEPL("create variable int varmsec");
            String startTime = "2002-05-30 9:00:00.000";
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(startTime)));
    
            String[] fields = "val0,val1,val2".Split(',');
            String eplFragment = "select " +
                    "current_timestamp.WithTime(varhour, varmin, varsec, varmsec) as val0," +
                    "utildate.WithTime(varhour, varmin, varsec, varmsec) as val1," +
                    "msecdate.WithTime(varhour, varmin, varsec, varmsec) as val2" +
                    " from SupportDateTime";
            EPStatement stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[] { typeof(long?), typeof(DateTimeOffset?), typeof(long?) });
    
            _epService.EPRuntime.SendEvent(SupportDateTime.Make(null));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, 
                new Object[]{SupportDateTime.GetValueCoerced(startTime, "msec"), null, null});
    
            String expectedTime = "2002-05-30 09:00:00.000";
            _epService.EPRuntime.SetVariableValue("varhour", null); // variable is null
            _epService.EPRuntime.SendEvent(SupportDateTime.Make(startTime));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, 
                SupportDateTime.GetArrayCoerced(expectedTime, "msec", "util", "msec"));
    
            expectedTime = "2002-05-30 1:02:03.004";
            _epService.EPRuntime.SetVariableValue("varhour", 1);
            _epService.EPRuntime.SetVariableValue("varmin", 2);
            _epService.EPRuntime.SetVariableValue("varsec", 3);
            _epService.EPRuntime.SetVariableValue("varmsec", 4);
            _epService.EPRuntime.SendEvent(SupportDateTime.Make(startTime));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, 
                SupportDateTime.GetArrayCoerced(expectedTime, "msec", "util", "msec"));
    
            expectedTime = "2002-05-30 0:00:00.006";
            _epService.EPRuntime.SetVariableValue("varhour", 0);
            _epService.EPRuntime.SetVariableValue("varmin", null);
            _epService.EPRuntime.SetVariableValue("varsec", null);
            _epService.EPRuntime.SetVariableValue("varmsec", 6);
            _epService.EPRuntime.SendEvent(SupportDateTime.Make(startTime));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, 
                SupportDateTime.GetArrayCoerced(expectedTime, "msec", "util", "msec"));
        }
    }
}
