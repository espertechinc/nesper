///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.events;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestTableOnUpdate
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            foreach (Type clazz in new Type[] {typeof(SupportBean), typeof(SupportBean_S0), typeof(MyUpdateEvent)}) {
                _epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
            _listener = new SupportUpdateListener();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
        }
    
        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
            _listener = null;
        }
    
        [Test]
        public void TestOnUpdateWTypeWiden()
        {
            SupportUpdateListener listenerUpdate = new SupportUpdateListener();
    
            string[] fields = "keyOne,keyTwo,p0".Split(',');
            _epService.EPAdministrator.CreateEPL("create table varagg as (" +
                    "keyOne string primary key, keyTwo int primary key, p0 long)");
            _epService.EPAdministrator.CreateEPL("on SupportBean merge varagg where TheString = keyOne and " +
                    "IntPrimitive = keyTwo when not matched then insert select TheString as keyOne, IntPrimitive as keyTwo, 1 as p0");
            _epService.EPAdministrator.CreateEPL("select varagg[p00, id].p0 as value from SupportBean_S0").AddListener(_listener);
            EPStatement stmtUpdate = _epService.EPAdministrator.CreateEPL("on MyUpdateEvent update varagg set p0 = newValue " +
                    "where k1 = keyOne and k2 = keyTwo");
            stmtUpdate.AddListener(listenerUpdate);

            object[][] expectedType = new object[][] { new object[] { "keyOne", typeof(string) }, new object[] { "keyTwo", typeof(int) }, new object[] { "p0", typeof(long) } };
            EventTypeAssertionUtil.AssertEventTypeProperties(expectedType, stmtUpdate.EventType, EventTypeAssertionEnum.NAME, EventTypeAssertionEnum.TYPE);
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            AssertValues(new object[][] { new object[] { "G1", 10 } }, new long?[] { 1L });
    
            _epService.EPRuntime.SendEvent(new MyUpdateEvent("G1", 10, 2));
            AssertValues(new object[][] { new object[] { "G1", 10 } }, new long?[] { 2L });
            EPAssertionUtil.AssertProps(listenerUpdate.LastNewData[0], fields, new object[]{"G1", 10, 2L});
            EPAssertionUtil.AssertProps(listenerUpdate.GetAndResetLastOldData()[0], fields, new object[] {"G1", 10, 1L});
    
            // try property method invocation
            _epService.EPAdministrator.CreateEPL("create table MyTableSuppBean as (sb SupportBean)");
            _epService.EPAdministrator.CreateEPL("on SupportBean_S0 update MyTableSuppBean sb set sb.set_LongPrimitive(10)");
        }
    
        private void AssertValues(object[][] keys, long?[] values)
        {
            Assert.AreEqual(keys.Length, values.Length);
            for (int i = 0; i < keys.Length; i++) {
                _epService.EPRuntime.SendEvent(new SupportBean_S0( (int) keys[i][1], (string) keys[i][0]));
                EventBean @event = _listener.AssertOneGetNewAndReset();
                Assert.AreEqual(values[i], @event.Get("value"), "Failed for key '" + keys[i].Render() + "'");
            }
        }
    
        public class MyUpdateEvent
        {
            public MyUpdateEvent(string k1, int k2, int newValue)
            {
                this.K1 = k1;
                this.K2 = k2;
                this.NewValue = newValue;
            }

            public string K1 { get; private set; }

            public int K2 { get; private set; }

            public int NewValue { get; private set; }
        }
    }
}
