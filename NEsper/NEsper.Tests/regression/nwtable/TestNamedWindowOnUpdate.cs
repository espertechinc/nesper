///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestNamedWindowOnUpdate 
    {
        private EPServiceProvider epService;
        private SupportUpdateListener listenerWindow;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, this.GetType(), GetType().FullName);}
            listenerWindow = new SupportUpdateListener();
            epService.EPAdministrator.Configuration.AddEventType("SupportBean", typeof(SupportBean));
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_A", typeof(SupportBean_A));
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
            listenerWindow = null;
        }
    
        [Test]
        public void TestUpdateNonPropertySet() {
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("setBeanLongPrimitive999", this.GetType().FullName, "SetBeanLongPrimitive999");
            epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_S0));
            epService.EPAdministrator.CreateEPL("create window MyWindow.win:keepall() as SupportBean");
            epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");
            EPStatement stmt = epService.EPAdministrator.CreateEPL("on SupportBean_S0 as sb " +
                    "update MyWindow as mywin" +
                    " set mywin.set_IntPrimitive(10)," +
                    "     setBeanLongPrimitive999(mywin)");
            stmt.AddListener(listenerWindow);
    
            string[] fields = "IntPrimitive,LongPrimitive".Split(',');
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            EPAssertionUtil.AssertProps(listenerWindow.GetAndResetLastNewData()[0], fields, new object[]{10, 999L});
        }
    
        [Test]
        public void TestMultipleDataWindowIntersect() {
            string stmtTextCreate = "create window MyWindow.std:unique(TheString).win:length(2) as select * from SupportBean";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtCreate.AddListener(listenerWindow);
    
            string stmtTextInsertOne = "insert into MyWindow select * from SupportBean";
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            string stmtTextUpdate = "on SupportBean_A update MyWindow set IntPrimitive=IntPrimitive*100 where TheString=id";
            epService.EPAdministrator.CreateEPL(stmtTextUpdate);
            
            epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 3));
            epService.EPRuntime.SendEvent(new SupportBean_A("E2"));
            EventBean[] newevents = listenerWindow.LastNewData;
            EventBean[] oldevents = listenerWindow.LastOldData;
    
            Assert.AreEqual(1, newevents.Length);
            EPAssertionUtil.AssertProps(newevents[0], "IntPrimitive".Split(','), new object[]{300});
            Assert.AreEqual(1, oldevents.Length);
            oldevents = EPAssertionUtil.Sort(oldevents, "TheString");
            EPAssertionUtil.AssertPropsPerRow(oldevents, "TheString,IntPrimitive".Split(','), new object[][] { new object[] { "E2", 3 } });

            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), "TheString,IntPrimitive".Split(','), new object[][] { new object[] { "E1", 2 }, new object[] { "E2", 300 } });
        }
        
        [Test]
        public void TestMultipleDataWindowIntersectOnUpdate() {
            SupportUpdateListener listener = new SupportUpdateListener();
            string[] fields = "company,value,total".Split(',');
    
            // ESPER-568
            epService.EPAdministrator.CreateEPL("create schema S2 ( company string, value double, total double)");
    	    EPStatement stmtWin = epService.EPAdministrator.CreateEPL("create window S2Win.win:time(25 hour).std:firstunique(company) as S2");
            epService.EPAdministrator.CreateEPL("insert into S2Win select * from S2.std:firstunique(company)");
            epService.EPAdministrator.CreateEPL("on S2 as a update S2Win as b set total = b.value + a.value");
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select count(*) as cnt from S2Win");
            stmt.AddListener(listener);
    
            CreateSendEvent(epService, "S2", "AComp", 3.0, 0.0);
            Assert.AreEqual(1L, listener.AssertOneGetNewAndReset().Get("cnt"));
            EPAssertionUtil.AssertPropsPerRow(stmtWin.GetEnumerator(), fields, new object[][] { new object[] { "AComp", 3.0, 0.0 } });
    
            CreateSendEvent(epService, "S2", "AComp", 6.0, 0.0);
            Assert.AreEqual(1L, listener.AssertOneGetNewAndReset().Get("cnt"));
            EPAssertionUtil.AssertPropsPerRow(stmtWin.GetEnumerator(), fields, new object[][] { new object[] { "AComp", 3.0, 9.0 } });
    
            CreateSendEvent(epService, "S2", "AComp", 5.0, 0.0);
            Assert.AreEqual(1L, listener.AssertOneGetNewAndReset().Get("cnt"));
            EPAssertionUtil.AssertPropsPerRow(stmtWin.GetEnumerator(), fields, new object[][] { new object[] { "AComp", 3.0, 8.0 } });
    
            CreateSendEvent(epService, "S2", "BComp", 4.0, 0.0);
            Assert.AreEqual(2L, listener.AssertOneGetNewAndReset().Get("cnt"));
            EPAssertionUtil.AssertPropsPerRow(stmtWin.GetEnumerator(), fields, new object[][] { new object[] { "AComp", 3.0, 7.0 }, new object[] { "BComp", 4.0, 0.0 } });
        }
    
        private void CreateSendEvent(EPServiceProvider engine, string typeName, string company, double value, double total)
        {
            var map = new LinkedHashMap<string, object>();
            map.Put("company", company);
            map.Put("value", value);
            map.Put("total", total);
            if (EventRepresentationEnumExtensions.GetEngineDefault(epService).IsObjectArrayEvent()) {
                engine.EPRuntime.SendEvent(map.Values.ToArray(), typeName);
            }
            else {
                engine.EPRuntime.SendEvent(map, typeName);
            }
        }
    
        [Test]
        public void TestMultipleDataWindowUnion() {
            string stmtTextCreate = "create window MyWindow.std:unique(TheString).win:length(2) retain-union as select * from SupportBean";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtCreate.AddListener(listenerWindow);
    
            string stmtTextInsertOne = "insert into MyWindow select * from SupportBean";
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            string stmtTextUpdate = "on SupportBean_A update MyWindow mw set mw.IntPrimitive=IntPrimitive*100 where TheString=id";
            epService.EPAdministrator.CreateEPL(stmtTextUpdate);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 3));
            epService.EPRuntime.SendEvent(new SupportBean_A("E2"));
            EventBean[] newevents = listenerWindow.LastNewData;
            EventBean[] oldevents = listenerWindow.LastOldData;
    
            Assert.AreEqual(1, newevents.Length);
            EPAssertionUtil.AssertProps(newevents[0], "IntPrimitive".Split(','), new object[]{300});
            Assert.AreEqual(1, oldevents.Length);
            EPAssertionUtil.AssertPropsPerRow(oldevents, "TheString,IntPrimitive".Split(','), new object[][] { new object[] { "E2", 3 } });
    
            EventBean[] events = EPAssertionUtil.Sort(stmtCreate.GetEnumerator(), "TheString");
            EPAssertionUtil.AssertPropsPerRow(events, "TheString,IntPrimitive".Split(','), new object[][] { new object[] { "E1", 2 }, new object[] { "E2", 300 } });
        }
    
        [Test]
        public void TestSubclass()
        {
            // create window
            string stmtTextCreate = "create window MyWindow.win:keepall() as select * from " + typeof(SupportBeanAbstractSub).FullName;
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtCreate.AddListener(listenerWindow);
    
            // create insert into
            string stmtTextInsertOne = "insert into MyWindow select * from " + typeof(SupportBeanAbstractSub).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // create update
            string stmtTextUpdate = "on " + typeof(SupportBean).FullName + " update MyWindow set V1=TheString, V2=TheString";
            epService.EPAdministrator.CreateEPL(stmtTextUpdate);
            
            epService.EPRuntime.SendEvent(new SupportBeanAbstractSub("value2"));
            listenerWindow.Reset();
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(listenerWindow.LastNewData[0], new string[]{"V1", "V2"}, new object[]{"E1", "E1"});
        }
    
        public static void SetBeanLongPrimitive999(SupportBean @event) {
            @event.LongPrimitive = 999;
        }
    }
}
