///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestNamedWindowProcessingOrder 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("Event", typeof(SupportBean));
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
            _listener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
            _listener = null;
        }
    
        [Test]
        public void TestDispatchBackQueue() {
            RunAssertionDispatchBackQueue(EventRepresentationEnum.OBJECTARRAY);
            RunAssertionDispatchBackQueue(EventRepresentationEnum.DEFAULT);
            RunAssertionDispatchBackQueue(EventRepresentationEnum.MAP);
        }
    
        public void RunAssertionDispatchBackQueue(EventRepresentationEnum eventRepresentationEnum) {
            _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema StartValueEvent as (dummy string)");
            _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema TestForwardEvent as (prop1 string)");
            _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema TestInputEvent as (dummy string)");
            _epService.EPAdministrator.CreateEPL("insert into TestForwardEvent select'V1' as prop1 from TestInputEvent");
    
            _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create window NamedWin.std:unique(prop1) (prop1 string, prop2 string)");
    
            _epService.EPAdministrator.CreateEPL("insert into NamedWin select 'V1' as prop1, 'O1' as prop2 from StartValueEvent");
    
            _epService.EPAdministrator.CreateEPL("on TestForwardEvent update NamedWin as work set prop2 = 'U1' where work.prop1 = 'V1'");
    
            string[] fields = "prop1,prop2".Split(',');
            string eplSelect = "select irstream prop1, prop2 from NamedWin";
            _epService.EPAdministrator.CreateEPL(eplSelect).AddListener(_listener);
    
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                _epService.EPRuntime.SendEvent(new object[] {"dummyValue"}, "StartValueEvent");
            }
            else {
                _epService.EPRuntime.SendEvent(new Dictionary<string, object>(), "StartValueEvent");
            }
    
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"V1", "O1"});
    
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                _epService.EPRuntime.SendEvent(new object[] {"dummyValue"}, "TestInputEvent");
            }
            else {
                _epService.EPRuntime.SendEvent(new Dictionary<string, object>(), "TestInputEvent");
            }
            EPAssertionUtil.AssertProps(_listener.LastOldData[0], fields, new object[]{"V1", "O1"});
            EPAssertionUtil.AssertProps(_listener.GetAndResetLastNewData()[0], fields, new object[]{"V1", "U1"});
    
            _epService.Initialize();
        }
    
        [Test]
        public void TestOrderedDeleteAndSelect()
        {
            string stmtText;
            stmtText = "create window MyWindow.std:lastevent() as select * from Event";
            _epService.EPAdministrator.CreateEPL(stmtText);
    
            stmtText = "insert into MyWindow select * from Event";
            _epService.EPAdministrator.CreateEPL(stmtText);
    
            stmtText = "on MyWindow e delete from MyWindow win where win.TheString=e.TheString and e.IntPrimitive = 7";
            _epService.EPAdministrator.CreateEPL(stmtText);
    
            stmtText = "on MyWindow e delete from MyWindow win where win.TheString=e.TheString and e.IntPrimitive = 5";
            _epService.EPAdministrator.CreateEPL(stmtText);
    
            stmtText = "on MyWindow e insert into ResultStream select e.* from MyWindow";
            _epService.EPAdministrator.CreateEPL(stmtText);
    
            stmtText = "select * from ResultStream";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.AddListener(_listener);
            
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 7));
            Assert.IsFalse(_listener.IsInvoked, "E1");
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 8));
            Assert.AreEqual("E2", _listener.AssertOneGetNewAndReset().Get("TheString"));
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 5));
            Assert.IsFalse(_listener.IsInvoked, "E3");
    
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 6));
            Assert.AreEqual("E4", _listener.AssertOneGetNewAndReset().Get("TheString"));
        }
    }
}
