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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestTableSubquery  {
        private EPServiceProvider epService;
        private SupportUpdateListener listener;
    
        [SetUp]
        public void SetUp() {
            epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            epService.Initialize();
            foreach (Type clazz in new Type[] {typeof(SupportBean), typeof(SupportBean_S0), typeof(SupportBean_S1)}) {
                epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
            listener = new SupportUpdateListener();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, this.GetType(), GetType().FullName);}
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
            listener = null;
        }
    
        [Test]
        public void TestSubquery() {
            // subquery against keyed
            epService.EPAdministrator.CreateEPL("create table varagg as (" +
                    "key string primary key, total sum(int))");
            epService.EPAdministrator.CreateEPL("into table varagg " +
                    "select sum(IntPrimitive) as total from SupportBean group by TheString");
            epService.EPAdministrator.CreateEPL("select (select total from varagg where key = s0.p00) as value " +
                    "from SupportBean_S0 as s0").AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean("G2", 200));
            AssertValues(epService, listener, "G1,G2", new int?[] {null, 200});
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 100));
            AssertValues(epService, listener, "G1,G2", new int?[] {100, 200});
            epService.EPAdministrator.DestroyAllStatements();
    
            // subquery against unkeyed
            epService.EPAdministrator.CreateEPL("create table InfraOne (string string, IntPrimitive int)");
            epService.EPAdministrator.CreateEPL("select (select IntPrimitive from InfraOne where string = s0.p00) as c0 from SupportBean_S0 as s0").AddListener(listener);
            epService.EPAdministrator.CreateEPL("insert into InfraOne select TheString as string, IntPrimitive from SupportBean");
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "E1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "c0".Split(','), new object[]{10});
        }
    
        private static void AssertValues(EPServiceProvider engine, SupportUpdateListener listener, string keys, int?[] values) {
            string[] keyarr = keys.Split(',');
            for (int i = 0; i < keyarr.Length; i++) {
                engine.EPRuntime.SendEvent(new SupportBean_S0(0, keyarr[i]));
                EventBean @event = listener.AssertOneGetNewAndReset();
                Assert.AreEqual(values[i], @event.Get("value"), "Failed for key '" + keyarr[i] + "'");
            }
        }
    }
}
