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
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.bean.lambda;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;



namespace com.espertech.esper.regression.datetime
{
    [TestFixture]
    public class TestDTWithMin  {
    
        private EPServiceProvider epService;
        private SupportUpdateListener listener;
    
        [SetUp]
        public void SetUp() {
    
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("SupportDateTime", typeof(SupportDateTime));
            epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            listener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown() {
            listener = null;
        }
    
        [Test]
        public void TestInput() {
    
            String[] fields = "val0,val1".Split(',');
            String eplFragment = "select " +
                    "utildate.WithMin('month') as val0," +
                    "longdate.WithMin('month') as val1" +
                    " from SupportDateTime";
            EPStatement stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[]{typeof(DateTimeOffset?), typeof(long?)});
    
            String startTime = "2002-05-30 09:00:00.000";
            String expectedTime = "2002-01-30 09:00:00.000";
            epService.EPRuntime.SendEvent(SupportDateTime.Make(startTime));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, SupportDateTime.GetArrayCoerced(expectedTime, "util", "long"));
        }
    
        [Test]
        public void TestFields()
        {
            String[] fields = "val0,val1,val2,val3,val4,val5,val6,val7".Split(',');
            String eplFragment = "select " +
                    "utildate.WithMin('msec') as val0," +
                    "utildate.WithMin('sec') as val1," +
                    "utildate.WithMin('minutes') as val2," +
                    "utildate.WithMin('hour') as val3," +
                    "utildate.WithMin('day') as val4," +
                    "utildate.WithMin('month') as val5," +
                    "utildate.WithMin('year') as val6," +
                    "utildate.WithMin('week') as val7" +
                    " from SupportDateTime";
            EPStatement stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[]
            {
                typeof(DateTimeOffset?), typeof(DateTimeOffset?), 
                typeof(DateTimeOffset?), typeof(DateTimeOffset?),
                typeof(DateTimeOffset?), typeof(DateTimeOffset?),
                typeof(DateTimeOffset?), typeof(DateTimeOffset?)
            });
    
            String[] expected = {
                    "2002-05-30 09:01:02.000",
                    "2002-05-30 09:01:00.003",
                    "2002-05-30 09:00:02.003",
                    "2002-05-30 00:01:02.003",
                    "2002-05-01 09:01:02.003",
                    "2002-01-30 09:01:02.003",
                    "0001-05-30 09:01:02.003",
                    "2002-01-03 09:01:02.003",
            };
            String startTime = "2002-05-30 09:01:02.003";
            epService.EPRuntime.SendEvent(SupportDateTime.Make(startTime));
            //Console.Out.WriteLine("===> " + SupportDateTime.Print(listener.AssertOneGetNew().Get("val7")));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, SupportDateTime.GetArrayCoerced(expected, "util"));
        }
    }
}
