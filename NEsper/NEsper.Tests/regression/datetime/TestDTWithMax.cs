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
using com.espertech.esper.support.bean;
using com.espertech.esper.support.bean.lambda;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.datetime
{
    [TestFixture]
    public class TestDTWithMax
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp() {
    
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("SupportDateTime", typeof(SupportDateTime));
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            _listener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown() {
            _listener = null;
        }
    
        [Test]
        public void TestInput() {
    
            String[] fields = "val0,val1".Split(',');
            String eplFragment = "select " +
                    "utildate.withMax('month') as val0," +
                    "msecdate.withMax('month') as val1" +
                    " from SupportDateTime";
            EPStatement stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[]{typeof(DateTimeOffset?), typeof(long?)});
    
            String startTime = "2002-05-30 09:00:00.000";
            String expectedTime = "2002-12-30 09:00:00.000";
            _epService.EPRuntime.SendEvent(SupportDateTime.Make(startTime));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, SupportDateTime.GetArrayCoerced(expectedTime, "util", "msec"));
        }
    
        [Test]
        public void TestFields() {
    
            String[] fields = "val0,val1,val2,val3,val4,val5,val6,val7".Split(',');
            String eplFragment = "select " +
                    "utildate.withMax('msec') as val0," +
                    "utildate.withMax('sec') as val1," +
                    "utildate.withMax('minutes') as val2," +
                    "utildate.withMax('hour') as val3," +
                    "utildate.withMax('day') as val4," +
                    "utildate.withMax('month') as val5," +
                    "utildate.withMax('year') as val6," +
                    "utildate.withMax('week') as val7" +
                    " from SupportDateTime";
            EPStatement stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[]{typeof(DateTimeOffset?), typeof(DateTimeOffset?), typeof(DateTimeOffset?), typeof(DateTimeOffset?), typeof(DateTimeOffset?), typeof(DateTimeOffset?), typeof(DateTimeOffset?), typeof(DateTimeOffset?)});
    
            String[] expected = {
                    "2002-5-30 09:00:00.999",
                    "2002-5-30 09:00:59.000",
                    "2002-5-30 09:59:00.000",
                    "2002-5-30 23:00:00.000",
                    "2002-5-31 09:00:00.000",
                    "2002-12-30 09:00:00.000",
                    "9999-05-30 09:00:00.000",
                    "2002-12-26 09:00:00.000"
            };
            String startTime = "2002-05-30 09:00:00.000";
            _epService.EPRuntime.SendEvent(SupportDateTime.Make(startTime));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, SupportDateTime.GetArrayCoerced(expected, "util"));
        }
    }
}
