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
    public class TestDTSet
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
            _listener = new SupportUpdateListener();
        }

        [TearDown]
        public void TearDown()
        {
            _listener = null;
        }

        [Test]
        public void TestInput()
        {

            String[] fields = "val0,val1".Split(',');
            const string eplFragment = "select " +
                                       "utildate.set('month', 1) as val0," +
                                       "msecdate.set('month', 1) as val1" +
                                       " from SupportDateTime";
            EPStatement stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[] { typeof(DateTime?), typeof(long?) });

            const string startTime = "2002-05-30 09:00:00.000";
            const string expectedTime = "2002-01-30 09:00:00.000";
            _epService.EPRuntime.SendEvent(SupportDateTime.Make(startTime));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, SupportDateTime.GetArrayCoerced(expectedTime, "util", "msec"));
        }

        [Test]
        public void TestFields()
        {
            String[] fields = "val0,val1,val2,val3,val4,val5,val6,val7".Split(',');
            String eplFragment = "select " +
                    "utildate.set('msec', 1) as val0," +
                    "utildate.set('sec', 2) as val1," +
                    "utildate.set('minutes', 3) as val2," +
                    "utildate.set('hour', 13) as val3," +
                    "utildate.set('day', 5) as val4," +
                    "utildate.set('month', 6) as val5," +
                    "utildate.set('year', 7) as val6," +
                    "utildate.set('week', 8) as val7" +
                    " from SupportDateTime";
            EPStatement stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[] { typeof(DateTime?), typeof(DateTime?), typeof(DateTime?), typeof(DateTime?), typeof(DateTime?), typeof(DateTime?), typeof(DateTime?), typeof(DateTime?) });

            String[] expected = {
                    "2002-05-30 09:00:00.001",
                    "2002-05-30 09:00:02.000",
                    "2002-05-30 09:03:00.000",
                    "2002-05-30 13:00:00.000",
                    "2002-05-05 09:00:00.000",
                    "2002-06-30 09:00:00.000",
                    "0007-05-30 09:00:00.000",
                    "2002-02-21 09:00:00.000",
            };
            String startTime = "2002-05-30 09:00:00.000";
            _epService.EPRuntime.SendEvent(SupportDateTime.Make(startTime));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, SupportDateTime.GetArrayCoerced(expected, "util"));
        }
    }
}
