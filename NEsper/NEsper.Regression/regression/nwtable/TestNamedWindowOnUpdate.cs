///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestNamedWindowOnUpdate 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listenerWindow;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
            _listenerWindow = new SupportUpdateListener();
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean_A>();
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
            _listenerWindow = null;
        }
    
        [Test]
        public void TestUpdateNonPropertySet() {
            _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("setBeanLongPrimitive999", this.GetType().FullName, "SetBeanLongPrimitive999");
            _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_S0));
            _epService.EPAdministrator.CreateEPL("create window MyWindow#keepall as SupportBean");
            _epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("on SupportBean_S0 as sb " +
                    "update MyWindow as mywin" +
                    " set mywin.set_IntPrimitive(10)," +
                    "     setBeanLongPrimitive999(mywin)");
            stmt.AddListener(_listenerWindow);
    
            string[] fields = "IntPrimitive,LongPrimitive".Split(',');
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            EPAssertionUtil.AssertProps(_listenerWindow.GetAndResetLastNewData()[0], fields, new object[]{10, 999L});
        }
    
        [Test]
        public void TestMultipleDataWindowIntersect() {
            string stmtTextCreate = "create window MyWindow#unique(TheString)#length(2) as select * from SupportBean";
            EPStatement stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtCreate.AddListener(_listenerWindow);
    
            string stmtTextInsertOne = "insert into MyWindow select * from SupportBean";
            _epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            string stmtTextUpdate = "on SupportBean_A update MyWindow set IntPrimitive=IntPrimitive*100 where TheString=id";
            _epService.EPAdministrator.CreateEPL(stmtTextUpdate);
            
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 3));
            _epService.EPRuntime.SendEvent(new SupportBean_A("E2"));
            EventBean[] newevents = _listenerWindow.LastNewData;
            EventBean[] oldevents = _listenerWindow.LastOldData;
    
            Assert.AreEqual(1, newevents.Length);
            EPAssertionUtil.AssertProps(newevents[0], "IntPrimitive".Split(','), new object[]{300});
            Assert.AreEqual(1, oldevents.Length);
            oldevents = EPAssertionUtil.Sort(oldevents, "TheString");
            EPAssertionUtil.AssertPropsPerRow(oldevents, "TheString,IntPrimitive".Split(','), new object[][] { new object[] { "E2", 3 } });

            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), "TheString,IntPrimitive".Split(','), new object[][] { new object[] { "E1", 2 }, new object[] { "E2", 300 } });
        }
        
        private void CreateSendEvent(EPServiceProvider engine, string typeName, string company, double value, double total)
        {
            var map = new LinkedHashMap<string, object>();
            map.Put("company", company);
            map.Put("value", value);
            map.Put("total", total);
            if (EventRepresentationChoiceExtensions.GetEngineDefault(_epService).IsObjectArrayEvent()) {
                engine.EPRuntime.SendEvent(map.Values.ToArray(), typeName);
            }
            else {
                engine.EPRuntime.SendEvent(map, typeName);
            }
        }
    
        [Test]
        public void TestMultipleDataWindowUnion() {
            string stmtTextCreate = "create window MyWindow#unique(TheString)#length(2) retain-union as select * from SupportBean";
            EPStatement stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtCreate.AddListener(_listenerWindow);
    
            string stmtTextInsertOne = "insert into MyWindow select * from SupportBean";
            _epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            string stmtTextUpdate = "on SupportBean_A update MyWindow mw set mw.IntPrimitive=IntPrimitive*100 where TheString=id";
            _epService.EPAdministrator.CreateEPL(stmtTextUpdate);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 3));
            _epService.EPRuntime.SendEvent(new SupportBean_A("E2"));
            EventBean[] newevents = _listenerWindow.LastNewData;
            EventBean[] oldevents = _listenerWindow.LastOldData;
    
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
            string stmtTextCreate = "create window MyWindow#keepall as select * from " + typeof(SupportBeanAbstractSub).FullName;
            EPStatement stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtCreate.AddListener(_listenerWindow);
    
            // create insert into
            string stmtTextInsertOne = "insert into MyWindow select * from " + typeof(SupportBeanAbstractSub).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // create update
            string stmtTextUpdate = "on " + typeof(SupportBean).FullName + " update MyWindow set V1=TheString, V2=TheString";
            _epService.EPAdministrator.CreateEPL(stmtTextUpdate);
            
            _epService.EPRuntime.SendEvent(new SupportBeanAbstractSub("value2"));
            _listenerWindow.Reset();
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(_listenerWindow.LastNewData[0], new string[]{"V1", "V2"}, new object[]{"E1", "E1"});
        }
    
        public static void SetBeanLongPrimitive999(SupportBean @event) {
            @event.LongPrimitive = 999;
        }
    }
}
