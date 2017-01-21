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
    public class TestDTGet
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
        public void TestInput() 
        {
            String[] fields = "val0,val1".Split(',');
            String epl = "select " +
                    "utildate.Get('month') as val0," +
                    "msecdate.Get('month') as val1" +
                    " from SupportDateTime";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmt.EventType, fields, new Type[]{typeof(int?), typeof(int?)});
    
            String startTime = "2002-05-30 09:00:00.000";
            _epService.EPRuntime.SendEvent(SupportDateTime.Make(startTime));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{5, 5});
    
            // try event as input
            ConfigurationEventTypeLegacy configBean = new ConfigurationEventTypeLegacy();
            configBean.StartTimestampPropertyName = "MsecdateStart";
            configBean.EndTimestampPropertyName = "MsecdateEnd";
            _epService.EPAdministrator.Configuration.AddEventType("SupportTimeStartEndA", typeof(SupportTimeStartEndA).FullName, configBean);
    
            stmt.Dispose();
            epl = "select abc.Get('month') as val0 from SupportTimeStartEndA as abc";
            stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;
            
            _epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("A0", startTime, 0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "val0".Split(','), new Object[]{5});
    
            // test "get" method on object is preferred
            _epService.EPAdministrator.Configuration.AddEventType(typeof(MyEvent));
            _epService.EPAdministrator.CreateEPL("select e.Get() as c0, e.Get('abc') as c1 from MyEvent as e").Events += _listener.Update;
            _epService.EPRuntime.SendEvent(new MyEvent());
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "c0,c1".Split(','), new Object[]{1, 2});
        }
    
        [Test]
        public void TestFields()
        {
            String[] fields = "val0,val1,val2,val3,val4,val5,val6,val7".Split(',');
            String eplFragment = "select " +
                    "utildate.Get('msec') as val0," +
                    "utildate.Get('sec') as val1," +
                    "utildate.Get('minutes') as val2," +
                    "utildate.Get('hour') as val3," +
                    "utildate.Get('day') as val4," +
                    "utildate.Get('month') as val5," +
                    "utildate.Get('year') as val6," +
                    "utildate.Get('week') as val7" +
                    " from SupportDateTime";
            EPStatement stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[]
            {
                typeof(int?), typeof(int?), typeof(int?), 
                typeof(int?), typeof(int?), typeof(int?), 
                typeof(int?), typeof(int?)
            });
    
            String startTime = "2002-05-30 09:01:02.003";
            _epService.EPRuntime.SendEvent(SupportDateTime.Make(startTime));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{3, 2, 1, 9, 30, 5, 2002, 22});
        }

        public class MyEvent
        {
            public int Get()
            {
                return 1;
            }

            public int Get(String abc)
            {
                return 2;
            }
        }
    }
}
