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
using com.espertech.esper.client.annotation;
using com.espertech.esper.client.hook;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.events;
using com.espertech.esper.events.bean;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util;

using NUnit.Framework;

using Avro;
using Avro.Generic;

using NEsper.Avro.Extensions;

using Newtonsoft.Json.Linq;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestNamedWindowViews
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listenerWindow;
        private SupportUpdateListener _listenerStmtOne;
        private SupportUpdateListener _listenerStmtTwo;
        private SupportUpdateListener _listenerStmtThree;
        private SupportUpdateListener _listenerStmtDelete;
    
        [SetUp]
        public void SetUp()
        {
            var types = new Dictionary<string, Object>();
            types.Put("key", typeof(string));
            types.Put("value", typeof(long));
    
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("MyMap", types);
            config.EngineDefaults.EventMeta.AvroSettings.ObjectValueTypeWidenerFactoryClass = typeof(MyAvroTypeWidenerFactory).FullName;
            config.EngineDefaults.EventMeta.AvroSettings.TypeRepresentationMapperClass = typeof(MyAvroTypeRepMapper).FullName;
    
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);
            }
            _listenerWindow = new SupportUpdateListener();
            _listenerStmtOne = new SupportUpdateListener();
            _listenerStmtTwo = new SupportUpdateListener();
            _listenerStmtThree = new SupportUpdateListener();
            _listenerStmtDelete = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.EndTest();
            }
            _listenerWindow = null;
            _listenerStmtOne = null;
            _listenerStmtTwo = null;
            _listenerStmtThree = null;
            _listenerStmtDelete = null;
        }
    
        public void TestBeanBacked() {
            RunAssertionBeanBacked(EventRepresentationChoice.ARRAY);
            RunAssertionBeanBacked(EventRepresentationChoice.MAP);
            RunAssertionBeanBacked(EventRepresentationChoice.DEFAULT);
    
            try {
                RunAssertionBeanBacked(EventRepresentationChoice.AVRO);
            } catch (EPStatementException ex) {
                SupportMessageAssertUtil.AssertMessage(ex, "Error starting statement: Avro event type does not allow contained beans");
            }
        }
    
        public void TestBeanContained() {
            foreach (EventRepresentationChoice rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                if (!rep.IsAvroEvent()) {
                    RunAssertionBeanContained(rep);
                }
            }
    
            try {
                RunAssertionBeanContained(EventRepresentationChoice.AVRO);
            } catch (EPStatementException ex) {
                SupportMessageAssertUtil.AssertMessage(ex, "Error starting statement: Avro event type does not allow contained beans");
            }
        }
    
        private void RunAssertionBeanContained(EventRepresentationChoice rep) {
            EPStatement stmtW = _epService.EPAdministrator.CreateEPL(rep.GetAnnotationText() + " create window MyWindow#keepall as (bean " + typeof(SupportBean_S0).FullName + ")");
            stmtW.AddListener(_listenerWindow);
            Assert.IsTrue(rep.MatchesClass(stmtW.EventType.UnderlyingType));
            _epService.EPAdministrator.CreateEPL("insert into MyWindow select bean.* as bean from " + typeof(SupportBean_S0).FullName + " as bean");
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), "bean.p00".SplitCsv(), new Object[] {"E1"});
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyWindow", true);
        }
    
        public void TestIntersection() {
            _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean));
            _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(
                "create window MyWindow#length(2)#unique(intPrimitive) as SupportBean;\n" +
                "insert into MyWindow select * from SupportBean;\n" +
                "@Name('out') select irstream * from MyWindow");
    
            string[] fields = "theString".SplitCsv();
            var listener = new SupportUpdateListener();
            _epService.EPAdministrator.GetStatement("out").AddListener(listener);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), fields, new Object[][] { new object[] {"E1"}}, null);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), fields, new Object[][] { new object[] {"E2"}}, null);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 2));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(listener.AssertInvokedAndReset(), fields, new Object[][] { new object[] {"E3"}}, new Object[][] { new object[] {"E1"}, new object[] {"E2"}});
        }
    
        private void RunAssertionBeanBacked(EventRepresentationChoice eventRepresentationEnum) {
            _epService.EPAdministrator.Configuration.AddEventType("SupportBean", typeof(SupportBean));
            _epService.EPAdministrator.Configuration.AddEventType("SupportBean_A", typeof(SupportBean_A));
    
            // Test create from class
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create window MyWindow#keepall as SupportBean");
            stmt.AddListener(_listenerWindow);
            _epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");
    
            EPStatementSPI stmtConsume = (EPStatementSPI) _epService.EPAdministrator.CreateEPL("select * from MyWindow");
            Assert.IsTrue(stmtConsume.StatementContext.IsStatelessSelect);
            stmtConsume.AddListener(_listenerStmtOne);
    
            _epService.EPRuntime.SendEvent(new SupportBean());
            AssertEvent(_listenerWindow.AssertOneGetNewAndReset());
            AssertEvent(_listenerStmtOne.AssertOneGetNewAndReset());
    
            EPStatement stmtUpdate = _epService.EPAdministrator.CreateEPL("on SupportBean_A update MyWindow set theString='s'");
            stmtUpdate.AddListener(_listenerStmtTwo);
            _epService.EPRuntime.SendEvent(new SupportBean_A("A1"));
            AssertEvent(_listenerStmtTwo.LastNewData[0]);
    
            // test bean-property
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyWindowOne", true);
            _listenerWindow.Reset();
            _listenerStmtOne.Reset();
        }
    
        public void TestBeanSchemaBacked() {
    
            // Test create from schema
            _epService.EPAdministrator.CreateEPL("create schema ABC as " + typeof(SupportBean).FullName);
            _epService.EPAdministrator.CreateEPL("create window MyWindow#keepall as ABC");
            _epService.EPAdministrator.CreateEPL("insert into MyWindow select * from " + typeof(SupportBean).FullName);
    
            _epService.EPRuntime.SendEvent(new SupportBean());
            AssertEvent(_epService.EPRuntime.ExecuteQuery("select * from MyWindow").Array[0]);
    
            EPStatement stmtABC = _epService.EPAdministrator.CreateEPL("select * from ABC");
            stmtABC.AddListener(_listenerStmtOne);
    
            _epService.EPRuntime.SendEvent(new SupportBean());
            Assert.IsTrue(_listenerStmtOne.IsInvoked);
        }
    
        private void AssertEvent(EventBean theEvent) {
            Assert.IsTrue(theEvent.EventType is BeanEventType);
            Assert.IsTrue(theEvent.Underlying is SupportBean);
            Assert.AreEqual(TypeClass.NAMED_WINDOW, ((EventTypeSPI) theEvent.EventType).Metadata.TypeClass);
            Assert.AreEqual("MyWindow", theEvent.EventType.Name);
        }
    
        public void TestDeepSupertypeInsert() {
            _epService.EPAdministrator.Configuration.AddEventType("SupportOverrideOneA", typeof(SupportOverrideOneA));
            _epService.EPAdministrator.Configuration.AddEventType("SupportOverrideOne", typeof(SupportOverrideOne));
            _epService.EPAdministrator.Configuration.AddEventType("SupportOverrideBase", typeof(SupportOverrideBase));
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("create window MyWindow#keepall as select * from SupportOverrideBase");
            _epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportOverrideOneA");
            _epService.EPRuntime.SendEvent(new SupportOverrideOneA("1a", "1", "base"));
            Assert.AreEqual("1a", stmt.First().Get("val"));
        }
    
        // Assert the named window is updated at the time that a subsequent event queries the named window
        public void TestOnInsertPremptiveTwoWindow() {
            _epService.EPAdministrator.CreateEPL("create schema TypeOne(col1 int)");
            _epService.EPAdministrator.CreateEPL("create schema TypeTwo(col2 int)");
            _epService.EPAdministrator.CreateEPL("create schema TypeTrigger(trigger int)");
            _epService.EPAdministrator.CreateEPL("create schema SupportBean as " + typeof(SupportBean).FullName);
    
            _epService.EPAdministrator.CreateEPL("create window WinOne#keepall as TypeOne");
            _epService.EPAdministrator.CreateEPL("create window WinTwo#keepall as TypeTwo");
    
            _epService.EPAdministrator.CreateEPL("insert into WinOne(col1) select intPrimitive from SupportBean");
    
            _epService.EPAdministrator.CreateEPL("on TypeTrigger insert into OtherStream select col1 from WinOne");
            _epService.EPAdministrator.CreateEPL("on TypeTrigger insert into WinTwo(col2) select col1 from WinOne");
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("on OtherStream select col2 from WinTwo");
            stmt.AddListener(_listenerStmtOne);
    
            // populate WinOne
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 9));
    
            // fire trigger
            if (EventRepresentationChoiceExtensions.GetEngineDefault(_epService).IsObjectArrayEvent()) {
                _epService.EPRuntime.GetEventSender("TypeTrigger").SendEvent(new Object[0]);
            } else {
                _epService.EPRuntime.GetEventSender("TypeTrigger").SendEvent(new Dictionary<string, object>());
            }
    
            Assert.AreEqual(9, _listenerStmtOne.AssertOneGetNewAndReset().Get("col2"));
        }
    
        public void TestWithDeleteUseAs() {
            TryCreateWindow("create window MyWindow#keepall as MyMap",
                            "on " + typeof(SupportMarketDataBean).FullName + " as s0 delete from MyWindow as s1 where s0.symbol = s1.key");
        }
    
        public void TestWithDeleteFirstAs() {
            TryCreateWindow("create window MyWindow#keepall as select key, value from MyMap",
                            "on " + typeof(SupportMarketDataBean).FullName + " delete from MyWindow as s1 where symbol = s1.key");
        }
    
        public void TestWithDeleteSecondAs() {
            TryCreateWindow("create window MyWindow#keepall as MyMap",
                            "on " + typeof(SupportMarketDataBean).FullName + " as s0 delete from MyWindow where s0.symbol = key");
        }
    
        public void TestWithDeleteNoAs() {
            TryCreateWindow("create window MyWindow#keepall as select key as key, value as value from MyMap",
                            "on " + typeof(SupportMarketDataBean).FullName + " delete from MyWindow where symbol = key");
        }
    
        private void TryCreateWindow(string createWindowStatement, string deleteStatement) {
            var fields = new string[] {"key", "value"};
            EPStatement stmtCreate = _epService.EPAdministrator.CreateEPL(createWindowStatement);
            stmtCreate.AddListener(_listenerWindow);
    
            Assert.AreEqual(typeof(string), stmtCreate.EventType.GetPropertyType("key"));
            Assert.AreEqual(typeof(long), stmtCreate.EventType.GetPropertyType("value"));
    
            string stmtTextInsert = "insert into MyWindow select theString as key, longBoxed as value from " + typeof(SupportBean).FullName;
            EPStatement stmtInsert = _epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            string stmtTextSelectOne = "select irstream key, value*2 as value from MyWindow";
            EPStatement stmtSelectOne = _epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            stmtSelectOne.AddListener(_listenerStmtOne);
    
            string stmtTextSelectTwo = "select irstream key, Sum(value) as value from MyWindow group by key";
            EPStatement stmtSelectTwo = _epService.EPAdministrator.CreateEPL(stmtTextSelectTwo);
            stmtSelectTwo.AddListener(_listenerStmtTwo);
    
            string stmtTextSelectThree = "select irstream key, value from MyWindow where value >= 10";
            EPStatement stmtSelectThree = _epService.EPAdministrator.CreateEPL(stmtTextSelectThree);
            stmtSelectThree.AddListener(_listenerStmtThree);
    
            // send events
            SendSupportBean("E1", 10L);
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new Object[] {"E1", 20L});
            EPAssertionUtil.AssertProps(_listenerStmtTwo.LastNewData[0], fields, new Object[] {"E1", 10L});
            EPAssertionUtil.AssertProps(_listenerStmtTwo.LastOldData[0], fields, new Object[] {"E1", null});
            _listenerStmtTwo.Reset();
            EPAssertionUtil.AssertProps(_listenerStmtThree.AssertOneGetNewAndReset(), fields, new Object[] {"E1", 10L});
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new Object[] {"E1", 10L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E1", 10L}});
            EPAssertionUtil.AssertPropsPerRow(stmtSelectOne.GetEnumerator(), fields, new Object[][] { new object[] {"E1", 20L}});
    
            SendSupportBean("E2", 20L);
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new Object[] {"E2", 40L});
            EPAssertionUtil.AssertProps(_listenerStmtTwo.LastNewData[0], fields, new Object[] {"E2", 20L});
            EPAssertionUtil.AssertProps(_listenerStmtTwo.LastOldData[0], fields, new Object[] {"E2", null});
            _listenerStmtTwo.Reset();
            EPAssertionUtil.AssertProps(_listenerStmtThree.AssertOneGetNewAndReset(), fields, new Object[] {"E2", 20L});
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new Object[] {"E2", 20L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E1", 10L}, new object[] {"E2", 20L}});
            EPAssertionUtil.AssertPropsPerRow(stmtSelectOne.GetEnumerator(), fields, new Object[][] { new object[] {"E1", 20L}, new object[] {"E2", 40L}});
    
            SendSupportBean("E3", 5L);
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new Object[] {"E3", 10L});
            EPAssertionUtil.AssertProps(_listenerStmtTwo.LastNewData[0], fields, new Object[] {"E3", 5L});
            EPAssertionUtil.AssertProps(_listenerStmtTwo.LastOldData[0], fields, new Object[] {"E3", null});
            _listenerStmtTwo.Reset();
            Assert.IsFalse(_listenerStmtThree.IsInvoked);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new Object[] {"E3", 5L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E1", 10L}, new object[] {"E2", 20L}, new object[] {"E3", 5L}});
    
            // create delete stmt
            EPStatement stmtDelete = _epService.EPAdministrator.CreateEPL(deleteStatement);
            stmtDelete.AddListener(_listenerStmtDelete);
    
            // send delete event
            SendMarketBean("E1");
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetOldAndReset(), fields, new Object[] {"E1", 20L});
            EPAssertionUtil.AssertProps(_listenerStmtTwo.LastNewData[0], fields, new Object[] {"E1", null});
            EPAssertionUtil.AssertProps(_listenerStmtTwo.LastOldData[0], fields, new Object[] {"E1", 10L});
            _listenerStmtTwo.Reset();
            EPAssertionUtil.AssertProps(_listenerStmtThree.AssertOneGetOldAndReset(), fields, new Object[] {"E1", 10L});
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetOldAndReset(), fields, new Object[] {"E1", 10L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E2", 20L}, new object[] {"E3", 5L}});
    
            // send delete event again, none deleted now
            SendMarketBean("E1");
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
            Assert.IsFalse(_listenerStmtTwo.IsInvoked);
            Assert.IsFalse(_listenerWindow.IsInvoked);
            Assert.IsTrue(_listenerStmtDelete.IsInvoked);
            _listenerStmtDelete.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E2", 20L}, new object[] {"E3", 5L}});
    
            // send delete event
            SendMarketBean("E2");
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetOldAndReset(), fields, new Object[] {"E2", 40L});
            EPAssertionUtil.AssertProps(_listenerStmtTwo.LastNewData[0], fields, new Object[] {"E2", null});
            EPAssertionUtil.AssertProps(_listenerStmtTwo.LastOldData[0], fields, new Object[] {"E2", 20L});
            _listenerStmtTwo.Reset();
            EPAssertionUtil.AssertProps(_listenerStmtThree.AssertOneGetOldAndReset(), fields, new Object[] {"E2", 20L});
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetOldAndReset(), fields, new Object[] {"E2", 20L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E3", 5L}});
    
            // send delete event
            SendMarketBean("E3");
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetOldAndReset(), fields, new Object[] {"E3", 10L});
            EPAssertionUtil.AssertProps(_listenerStmtTwo.LastNewData[0], fields, new Object[] {"E3", null});
            EPAssertionUtil.AssertProps(_listenerStmtTwo.LastOldData[0], fields, new Object[] {"E3", 5L});
            _listenerStmtTwo.Reset();
            Assert.IsFalse(_listenerStmtThree.IsInvoked);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetOldAndReset(), fields, new Object[] {"E3", 5L});
            Assert.IsTrue(_listenerStmtDelete.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, null);
    
            stmtSelectOne.Dispose();
            stmtSelectTwo.Dispose();
            stmtSelectThree.Dispose();
            stmtDelete.Dispose();
            stmtInsert.Dispose();
            stmtCreate.Dispose();
        }
    
        public void TestTimeWindow() {
            var fields = new string[] {"key", "value"};
    
            // create window
            string stmtTextCreate = "create window MyWindow#Time(10 sec) as MyMap";
            EPStatement stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtCreate.AddListener(_listenerWindow);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, null);
    
            // create insert into
            string stmtTextInsert = "insert into MyWindow select theString as key, longBoxed as value from " + typeof(SupportBean).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // create consumer
            string stmtTextSelectOne = "select irstream key, value as value from MyWindow";
            EPStatement stmtSelectOne = _epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            stmtSelectOne.AddListener(_listenerStmtOne);
    
            // create delete stmt
            string stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + " as s0 delete from MyWindow as s1 where s0.symbol = s1.key";
            EPStatement stmtDelete = _epService.EPAdministrator.CreateEPL(stmtTextDelete);
            stmtDelete.AddListener(_listenerStmtDelete);
    
            SendTimer(1000);
            SendSupportBean("E1", 1L);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new Object[] {"E1", 1L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new Object[] {"E1", 1L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E1", 1L}});
    
            SendTimer(5000);
            SendSupportBean("E2", 2L);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new Object[] {"E2", 2L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new Object[] {"E2", 2L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E1", 1L}, new object[] {"E2", 2L}});
    
            SendTimer(10000);
            SendSupportBean("E3", 3L);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new Object[] {"E3", 3L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new Object[] {"E3", 3L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E1", 1L}, new object[] {"E2", 2L}, new object[] {"E3", 3L}});
    
            // Should push out the window
            SendTimer(10999);
            Assert.IsFalse(_listenerWindow.IsInvoked);
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
            SendTimer(11000);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetOldAndReset(), fields, new Object[] {"E1", 1L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetOldAndReset(), fields, new Object[] {"E1", 1L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E2", 2L}, new object[] {"E3", 3L}});
    
            SendSupportBean("E4", 4L);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new Object[] {"E4", 4L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new Object[] {"E4", 4L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E2", 2L}, new object[] {"E3", 3L}, new object[] {"E4", 4L}});
    
            // delete E2
            SendMarketBean("E2");
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetOldAndReset(), fields, new Object[] {"E2", 2L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetOldAndReset(), fields, new Object[] {"E2", 2L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E3", 3L}, new object[] {"E4", 4L}});
    
            // nothing pushed
            SendTimer(15000);
            Assert.IsFalse(_listenerWindow.IsInvoked);
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
    
            // push last event
            SendTimer(19999);
            Assert.IsFalse(_listenerWindow.IsInvoked);
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
            SendTimer(20000);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetOldAndReset(), fields, new Object[] {"E3", 3L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetOldAndReset(), fields, new Object[] {"E3", 3L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E4", 4L}});
    
            // delete E4
            SendMarketBean("E4");
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetOldAndReset(), fields, new Object[] {"E4", 4L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetOldAndReset(), fields, new Object[] {"E4", 4L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, null);
    
            SendTimer(100000);
            Assert.IsFalse(_listenerWindow.IsInvoked);
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
        }
    
        public void TestTimeFirstWindow() {
            var fields = new string[] {"key", "value"};
    
            SendTimer(1000);
    
            // create window
            string stmtTextCreate = "create window MyWindow#Firsttime(10 sec) as MyMap";
            EPStatement stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtCreate.AddListener(_listenerWindow);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, null);
    
            // create insert into
            string stmtTextInsert = "insert into MyWindow select theString as key, longBoxed as value from " + typeof(SupportBean).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // create consumer
            string stmtTextSelectOne = "select irstream key, value as value from MyWindow";
            EPStatement stmtSelectOne = _epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            stmtSelectOne.AddListener(_listenerStmtOne);
    
            // create delete stmt
            string stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + " as s0 delete from MyWindow as s1 where s0.symbol = s1.key";
            EPStatement stmtDelete = _epService.EPAdministrator.CreateEPL(stmtTextDelete);
            stmtDelete.AddListener(_listenerStmtDelete);
    
            SendSupportBean("E1", 1L);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new Object[] {"E1", 1L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new Object[] {"E1", 1L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E1", 1L}});
    
            SendTimer(5000);
            SendSupportBean("E2", 2L);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new Object[] {"E2", 2L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new Object[] {"E2", 2L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E1", 1L}, new object[] {"E2", 2L}});
    
            SendTimer(10000);
            SendSupportBean("E3", 3L);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new Object[] {"E3", 3L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new Object[] {"E3", 3L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E1", 1L}, new object[] {"E2", 2L}, new object[] {"E3", 3L}});
    
            // Should not push out the window
            SendTimer(12000);
            Assert.IsFalse(_listenerWindow.IsInvoked);
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E1", 1L}, new object[] {"E2", 2L}, new object[] {"E3", 3L}});
    
            SendSupportBean("E4", 4L);
            Assert.IsFalse(_listenerWindow.IsInvoked);
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E1", 1L}, new object[] {"E2", 2L}, new object[] {"E3", 3L}});
    
            // delete E2
            SendMarketBean("E2");
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetOldAndReset(), fields, new Object[] {"E2", 2L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetOldAndReset(), fields, new Object[] {"E2", 2L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E1", 1L}, new object[] {"E3", 3L}});
    
            // nothing pushed
            SendTimer(100000);
            Assert.IsFalse(_listenerWindow.IsInvoked);
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
        }
    
        public void TestExtTimeWindow() {
            var fields = new string[] {"key", "value"};
    
            // create window
            string stmtTextCreate = "create window MyWindow#Ext_timed(value, 10 sec) as MyMap";
            EPStatement stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtCreate.AddListener(_listenerWindow);
    
            // create insert into
            string stmtTextInsert = "insert into MyWindow select theString as key, longBoxed as value from " + typeof(SupportBean).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // create consumer
            string stmtTextSelectOne = "select irstream key, value as value from MyWindow";
            EPStatement stmtSelectOne = _epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            stmtSelectOne.AddListener(_listenerStmtOne);
    
            // create delete stmt
            string stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + " delete from MyWindow where symbol = key";
            EPStatement stmtDelete = _epService.EPAdministrator.CreateEPL(stmtTextDelete);
            stmtDelete.AddListener(_listenerStmtDelete);
    
            SendSupportBean("E1", 1000L);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new Object[] {"E1", 1000L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new Object[] {"E1", 1000L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E1", 1000L}});
    
            SendSupportBean("E2", 5000L);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new Object[] {"E2", 5000L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new Object[] {"E2", 5000L});
    
            SendSupportBean("E3", 10000L);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new Object[] {"E3", 10000L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new Object[] {"E3", 10000L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E1", 1000L}, new object[] {"E2", 5000L}, new object[] {"E3", 10000L}});
    
            // Should push out the window
            SendSupportBean("E4", 11000L);
            EPAssertionUtil.AssertProps(_listenerWindow.LastNewData[0], fields, new Object[] {"E4", 11000L});
            EPAssertionUtil.AssertProps(_listenerWindow.LastOldData[0], fields, new Object[] {"E1", 1000L});
            _listenerWindow.Reset();
            EPAssertionUtil.AssertProps(_listenerStmtOne.LastNewData[0], fields, new Object[] {"E4", 11000L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.LastOldData[0], fields, new Object[] {"E1", 1000L});
            _listenerStmtOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E2", 5000L}, new object[] {"E3", 10000L}, new object[] {"E4", 11000L}});
    
            // delete E2
            SendMarketBean("E2");
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetOldAndReset(), fields, new Object[] {"E2", 5000L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetOldAndReset(), fields, new Object[] {"E2", 5000L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E3", 10000L}, new object[] {"E4", 11000L}});
    
            // nothing pushed other then E5 (E2 is deleted)
            SendSupportBean("E5", 15000L);
            EPAssertionUtil.AssertProps(_listenerWindow.LastNewData[0], fields, new Object[] {"E5", 15000L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.LastNewData[0], fields, new Object[] {"E5", 15000L});
            Assert.IsNull(_listenerWindow.LastOldData);
            Assert.IsNull(_listenerStmtOne.LastOldData);
        }
    
        public void TestTimeOrderWindow() {
            var fields = new string[] {"key", "value"};
    
            // create window
            string stmtTextCreate = "create window MyWindow#Time_order(value, 10 sec) as MyMap";
            EPStatement stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtCreate.AddListener(_listenerWindow);
    
            // create insert into
            string stmtTextInsert = "insert into MyWindow select theString as key, longBoxed as value from " + typeof(SupportBean).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // create consumer
            string stmtTextSelectOne = "select irstream key, value as value from MyWindow";
            EPStatement stmtSelectOne = _epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            stmtSelectOne.AddListener(_listenerStmtOne);
    
            // create delete stmt
            string stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + " delete from MyWindow where symbol = key";
            EPStatement stmtDelete = _epService.EPAdministrator.CreateEPL(stmtTextDelete);
            stmtDelete.AddListener(_listenerStmtDelete);
    
            SendTimer(5000);
            SendSupportBean("E1", 3000L);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new Object[] {"E1", 3000L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new Object[] {"E1", 3000L});
    
            SendTimer(6000);
            SendSupportBean("E2", 2000L);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new Object[] {"E2", 2000L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new Object[] {"E2", 2000L});
    
            SendTimer(10000);
            SendSupportBean("E3", 1000L);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new Object[] {"E3", 1000L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new Object[] {"E3", 1000L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E3", 1000L}, new object[] {"E2", 2000L}, new object[] {"E1", 3000L}});
    
            // Should push out the window
            SendTimer(11000);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetOldAndReset(), fields, new Object[] {"E3", 1000L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetOldAndReset(), fields, new Object[] {"E3", 1000L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E2", 2000L}, new object[] {"E1", 3000L}});
    
            // delete E2
            SendMarketBean("E2");
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetOldAndReset(), fields, new Object[] {"E2", 2000L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetOldAndReset(), fields, new Object[] {"E2", 2000L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E1", 3000L}});
    
            SendTimer(12999);
            Assert.IsFalse(_listenerWindow.IsInvoked);
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
    
            SendTimer(13000);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetOldAndReset(), fields, new Object[] {"E1", 3000L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetOldAndReset(), fields, new Object[] {"E1", 3000L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, null);
    
            SendTimer(100000);
            Assert.IsFalse(_listenerWindow.IsInvoked);
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
        }
    
        public void TestLengthWindow() {
            var fields = new string[] {"key", "value"};
    
            // create window
            string stmtTextCreate = "create window MyWindow#length(3) as MyMap";
            EPStatement stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtCreate.AddListener(_listenerWindow);
    
            // create insert into
            string stmtTextInsert = "insert into MyWindow select theString as key, longBoxed as value from " + typeof(SupportBean).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // create consumer
            string stmtTextSelectOne = "select irstream key, value as value from MyWindow";
            EPStatement stmtSelectOne = _epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            stmtSelectOne.AddListener(_listenerStmtOne);
    
            // create delete stmt
            string stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + " delete from MyWindow where symbol = key";
            EPStatement stmtDelete = _epService.EPAdministrator.CreateEPL(stmtTextDelete);
            stmtDelete.AddListener(_listenerStmtDelete);
    
            SendSupportBean("E1", 1L);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new Object[] {"E1", 1L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new Object[] {"E1", 1L});
    
            SendSupportBean("E2", 2L);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new Object[] {"E2", 2L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new Object[] {"E2", 2L});
    
            SendSupportBean("E3", 3L);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new Object[] {"E3", 3L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new Object[] {"E3", 3L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E1", 1L}, new object[] {"E2", 2L}, new object[] {"E3", 3L}});
    
            // delete E2
            SendMarketBean("E2");
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetOldAndReset(), fields, new Object[] {"E2", 2L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetOldAndReset(), fields, new Object[] {"E2", 2L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E1", 1L}, new object[] {"E3", 3L}});
    
            SendSupportBean("E4", 4L);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new Object[] {"E4", 4L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new Object[] {"E4", 4L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E1", 1L}, new object[] {"E3", 3L}, new object[] {"E4", 4L}});
    
            SendSupportBean("E5", 5L);
            EPAssertionUtil.AssertProps(_listenerWindow.LastNewData[0], fields, new Object[] {"E5", 5L});
            EPAssertionUtil.AssertProps(_listenerWindow.LastOldData[0], fields, new Object[] {"E1", 1L});
            _listenerWindow.Reset();
            EPAssertionUtil.AssertProps(_listenerStmtOne.LastNewData[0], fields, new Object[] {"E5", 5L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.LastOldData[0], fields, new Object[] {"E1", 1L});
            _listenerStmtOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E3", 3L}, new object[] {"E4", 4L}, new object[] {"E5", 5L}});
    
            SendSupportBean("E6", 6L);
            EPAssertionUtil.AssertProps(_listenerWindow.LastNewData[0], fields, new Object[] {"E6", 6L});
            EPAssertionUtil.AssertProps(_listenerWindow.LastOldData[0], fields, new Object[] {"E3", 3L});
            _listenerWindow.Reset();
            EPAssertionUtil.AssertProps(_listenerStmtOne.LastNewData[0], fields, new Object[] {"E6", 6L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.LastOldData[0], fields, new Object[] {"E3", 3L});
            _listenerStmtOne.Reset();
    
            Assert.IsFalse(_listenerWindow.IsInvoked);
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
        }
    
        public void TestLengthFirstWindow() {
            var fields = new string[] {"key", "value"};
    
            // create window
            string stmtTextCreate = "create window MyWindow#Firstlength(2) as MyMap";
            EPStatement stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtCreate.AddListener(_listenerWindow);
    
            // create insert into
            string stmtTextInsert = "insert into MyWindow select theString as key, longBoxed as value from " + typeof(SupportBean).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // create consumer
            string stmtTextSelectOne = "select irstream key, value as value from MyWindow";
            EPStatement stmtSelectOne = _epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            stmtSelectOne.AddListener(_listenerStmtOne);
    
            // create delete stmt
            string stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + " delete from MyWindow where symbol = key";
            EPStatement stmtDelete = _epService.EPAdministrator.CreateEPL(stmtTextDelete);
            stmtDelete.AddListener(_listenerStmtDelete);
    
            SendSupportBean("E1", 1L);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new Object[] {"E1", 1L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new Object[] {"E1", 1L});
    
            SendSupportBean("E2", 2L);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new Object[] {"E2", 2L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new Object[] {"E2", 2L});
    
            SendSupportBean("E3", 3L);
            Assert.IsFalse(_listenerWindow.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E1", 1L}, new object[] {"E2", 2L}});
    
            // delete E2
            SendMarketBean("E2");
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetOldAndReset(), fields, new Object[] {"E2", 2L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetOldAndReset(), fields, new Object[] {"E2", 2L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E1", 1L}});
    
            SendSupportBean("E4", 4L);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new Object[] {"E4", 4L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new Object[] {"E4", 4L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E1", 1L}, new object[] {"E4", 4L}});
    
            SendSupportBean("E5", 5L);
            Assert.IsFalse(_listenerWindow.IsInvoked);
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E1", 1L}, new object[] {"E4", 4L}});
        }
    
        public void TestTimeAccum() {
            var fields = new string[] {"key", "value"};
    
            // create window
            string stmtTextCreate = "create window MyWindow#Time_accum(10 sec) as MyMap";
            EPStatement stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtCreate.AddListener(_listenerWindow);
    
            // create insert into
            string stmtTextInsert = "insert into MyWindow select theString as key, longBoxed as value from " + typeof(SupportBean).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // create consumer
            string stmtTextSelectOne = "select irstream key, value as value from MyWindow";
            EPStatement stmtSelectOne = _epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            stmtSelectOne.AddListener(_listenerStmtOne);
    
            // create delete stmt
            string stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + " as s0 delete from MyWindow as s1 where s0.symbol = s1.key";
            EPStatement stmtDelete = _epService.EPAdministrator.CreateEPL(stmtTextDelete);
            stmtDelete.AddListener(_listenerStmtDelete);
    
            SendTimer(1000);
            SendSupportBean("E1", 1L);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new Object[] {"E1", 1L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new Object[] {"E1", 1L});
    
            SendTimer(5000);
            SendSupportBean("E2", 2L);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new Object[] {"E2", 2L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new Object[] {"E2", 2L});
    
            SendTimer(10000);
            SendSupportBean("E3", 3L);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new Object[] {"E3", 3L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new Object[] {"E3", 3L});
    
            SendTimer(15000);
            SendSupportBean("E4", 4L);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new Object[] {"E4", 4L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new Object[] {"E4", 4L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E1", 1L}, new object[] {"E2", 2L}, new object[] {"E3", 3L}, new object[] {"E4", 4L}});
    
            // delete E2
            SendMarketBean("E2");
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetOldAndReset(), fields, new Object[] {"E2", 2L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetOldAndReset(), fields, new Object[] {"E2", 2L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E1", 1L}, new object[] {"E3", 3L}, new object[] {"E4", 4L}});
    
            // nothing pushed
            SendTimer(24999);
            Assert.IsFalse(_listenerWindow.IsInvoked);
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
    
            SendTimer(25000);
            Assert.IsNull(_listenerWindow.LastNewData);
            EventBean[] oldData = _listenerWindow.LastOldData;
            Assert.AreEqual(3, oldData.Length);
            EPAssertionUtil.AssertProps(oldData[0], fields, new Object[] {"E1", 1L});
            EPAssertionUtil.AssertProps(oldData[1], fields, new Object[] {"E3", 3L});
            EPAssertionUtil.AssertProps(oldData[2], fields, new Object[] {"E4", 4L});
            _listenerWindow.Reset();
            _listenerStmtOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, null);
    
            // delete E4
            SendMarketBean("E4");
            Assert.IsFalse(_listenerWindow.IsInvoked);
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
    
            SendTimer(30000);
            SendSupportBean("E5", 5L);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new Object[] {"E5", 5L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new Object[] {"E5", 5L});
    
            SendTimer(31000);
            SendSupportBean("E6", 6L);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new Object[] {"E6", 6L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new Object[] {"E6", 6L});
    
            SendTimer(38000);
            SendSupportBean("E7", 7L);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new Object[] {"E7", 7L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new Object[] {"E7", 7L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E5", 5L}, new object[] {"E6", 6L}, new object[] {"E7", 7L}});
    
            // delete E7 - deleting the last should spit out the first 2 timely
            SendMarketBean("E7");
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetOldAndReset(), fields, new Object[] {"E7", 7L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetOldAndReset(), fields, new Object[] {"E7", 7L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E5", 5L}, new object[] {"E6", 6L}});
    
            SendTimer(40999);
            Assert.IsFalse(_listenerWindow.IsInvoked);
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
    
            SendTimer(41000);
            Assert.IsNull(_listenerStmtOne.LastNewData);
            oldData = _listenerStmtOne.LastOldData;
            Assert.AreEqual(2, oldData.Length);
            EPAssertionUtil.AssertProps(oldData[0], fields, new Object[] {"E5", 5L});
            EPAssertionUtil.AssertProps(oldData[1], fields, new Object[] {"E6", 6L});
            _listenerWindow.Reset();
            _listenerStmtOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, null);
    
            SendTimer(50000);
            SendSupportBean("E8", 8L);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new Object[] {"E8", 8L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new Object[] {"E8", 8L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E8", 8L}});
    
            SendTimer(55000);
            SendMarketBean("E8");
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetOldAndReset(), fields, new Object[] {"E8", 8L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetOldAndReset(), fields, new Object[] {"E8", 8L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, null);
    
            SendTimer(100000);
            Assert.IsFalse(_listenerWindow.IsInvoked);
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
        }
    
        public void TestTimeBatch() {
            var fields = new string[] {"key", "value"};
    
            // create window
            string stmtTextCreate = "create window MyWindow#Time_batch(10 sec) as MyMap";
            EPStatement stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtCreate.AddListener(_listenerWindow);
    
            // create insert into
            string stmtTextInsert = "insert into MyWindow select theString as key, longBoxed as value from " + typeof(SupportBean).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // create consumer
            string stmtTextSelectOne = "select key, value as value from MyWindow";
            EPStatement stmtSelectOne = _epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            stmtSelectOne.AddListener(_listenerStmtOne);
    
            // create delete stmt
            string stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + " as s0 delete from MyWindow as s1 where s0.symbol = s1.key";
            EPStatement stmtDelete = _epService.EPAdministrator.CreateEPL(stmtTextDelete);
            stmtDelete.AddListener(_listenerStmtDelete);
    
            SendTimer(1000);
            SendSupportBean("E1", 1L);
    
            SendTimer(5000);
            SendSupportBean("E2", 2L);
    
            SendTimer(10000);
            SendSupportBean("E3", 3L);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E1", 1L}, new object[] {"E2", 2L}, new object[] {"E3", 3L}});
    
            // delete E2
            SendMarketBean("E2");
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E1", 1L}, new object[] {"E3", 3L}});
    
            // nothing pushed
            SendTimer(10999);
            Assert.IsFalse(_listenerWindow.IsInvoked);
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
    
            SendTimer(11000);
            Assert.IsNull(_listenerWindow.LastOldData);
            EventBean[] newData = _listenerWindow.LastNewData;
            Assert.AreEqual(2, newData.Length);
            EPAssertionUtil.AssertProps(newData[0], fields, new Object[] {"E1", 1L});
            EPAssertionUtil.AssertProps(newData[1], fields, new Object[] {"E3", 3L});
            _listenerWindow.Reset();
            _listenerStmtOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, null);
    
            SendTimer(21000);
            Assert.IsNull(_listenerWindow.LastNewData);
            EventBean[] oldData = _listenerWindow.LastOldData;
            Assert.AreEqual(2, oldData.Length);
            EPAssertionUtil.AssertProps(oldData[0], fields, new Object[] {"E1", 1L});
            EPAssertionUtil.AssertProps(oldData[1], fields, new Object[] {"E3", 3L});
            _listenerWindow.Reset();
            _listenerStmtOne.Reset();
    
            // send and delete E4, leaving an empty batch
            SendSupportBean("E4", 4L);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E4", 4L}});
    
            SendMarketBean("E4");
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, null);
    
            SendTimer(31000);
            Assert.IsFalse(_listenerWindow.IsInvoked);
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
        }
    
        public void TestTimeBatchLateConsumer() {
            SendTimer(0);
    
            // create window
            string stmtTextCreate = "create window MyWindow#Time_batch(10 sec) as MyMap";
            EPStatement stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtCreate.AddListener(_listenerWindow);
    
            // create insert into
            string stmtTextInsert = "insert into MyWindow select theString as key, longBoxed as value from " + typeof(SupportBean).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            SendTimer(0);
            SendSupportBean("E1", 1L);
    
            SendTimer(5000);
            SendSupportBean("E2", 2L);
    
            // create consumer
            string stmtTextSelectOne = "select Sum(value) as value from MyWindow";
            EPStatement stmtSelectOne = _epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            stmtSelectOne.AddListener(_listenerStmtOne);
    
            SendTimer(8000);
            SendSupportBean("E3", 3L);
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
    
            SendTimer(10000);
            EventBean[] newData = _listenerStmtOne.LastNewData;
            Assert.AreEqual(1, newData.Length);
            EPAssertionUtil.AssertProps(newData[0], new string[] {"value"}, new Object[] {6L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), new string[] {"value"}, null);
        }
    
        public void TestLengthBatch() {
            var fields = new string[] {"key", "value"};
    
            // create window
            string stmtTextCreate = "create window MyWindow#Length_batch(3) as MyMap";
            EPStatement stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtCreate.AddListener(_listenerWindow);
    
            // create insert into
            string stmtTextInsert = "insert into MyWindow select theString as key, longBoxed as value from " + typeof(SupportBean).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // create consumer
            string stmtTextSelectOne = "select key, value as value from MyWindow";
            EPStatement stmtSelectOne = _epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            stmtSelectOne.AddListener(_listenerStmtOne);
    
            // create delete stmt
            string stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + " as s0 delete from MyWindow as s1 where s0.symbol = s1.key";
            EPStatement stmtDelete = _epService.EPAdministrator.CreateEPL(stmtTextDelete);
            stmtDelete.AddListener(_listenerStmtDelete);
    
            SendSupportBean("E1", 1L);
            SendSupportBean("E2", 2L);
            Assert.IsFalse(_listenerWindow.IsInvoked);
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E1", 1L}, new object[] {"E2", 2L}});
    
            // delete E2
            SendMarketBean("E2");
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E1", 1L}});
    
            SendSupportBean("E3", 3L);
            Assert.IsFalse(_listenerWindow.IsInvoked);
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E1", 1L}, new object[] {"E3", 3L}});
    
            SendSupportBean("E4", 4L);
            Assert.IsNull(_listenerWindow.LastOldData);
            EventBean[] newData = _listenerWindow.LastNewData;
            Assert.AreEqual(3, newData.Length);
            EPAssertionUtil.AssertProps(newData[0], fields, new Object[] {"E1", 1L});
            EPAssertionUtil.AssertProps(newData[1], fields, new Object[] {"E3", 3L});
            EPAssertionUtil.AssertProps(newData[2], fields, new Object[] {"E4", 4L});
            _listenerWindow.Reset();
            _listenerStmtOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, null);
    
            SendSupportBean("E5", 5L);
            SendSupportBean("E6", 6L);
            SendMarketBean("E5");
            SendMarketBean("E6");
            Assert.IsFalse(_listenerWindow.IsInvoked);
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, null);
    
            SendSupportBean("E7", 7L);
            SendSupportBean("E8", 8L);
            SendSupportBean("E9", 9L);
            EventBean[] oldData = _listenerWindow.LastOldData;
            newData = _listenerWindow.LastNewData;
            Assert.AreEqual(3, newData.Length);
            Assert.AreEqual(3, oldData.Length);
            EPAssertionUtil.AssertProps(newData[0], fields, new Object[] {"E7", 7L});
            EPAssertionUtil.AssertProps(newData[1], fields, new Object[] {"E8", 8L});
            EPAssertionUtil.AssertProps(newData[2], fields, new Object[] {"E9", 9L});
            EPAssertionUtil.AssertProps(oldData[0], fields, new Object[] {"E1", 1L});
            EPAssertionUtil.AssertProps(oldData[1], fields, new Object[] {"E3", 3L});
            EPAssertionUtil.AssertProps(oldData[2], fields, new Object[] {"E4", 4L});
            _listenerWindow.Reset();
            _listenerStmtOne.Reset();
    
            SendSupportBean("E10", 10L);
            SendSupportBean("E10", 11L);
            SendMarketBean("E10");
    
            SendSupportBean("E21", 21L);
            SendSupportBean("E22", 22L);
            Assert.IsFalse(_listenerWindow.IsInvoked);
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
            SendSupportBean("E23", 23L);
            oldData = _listenerWindow.LastOldData;
            newData = _listenerWindow.LastNewData;
            Assert.AreEqual(3, newData.Length);
            Assert.AreEqual(3, oldData.Length);
        }
    
        public void TestSortWindow() {
            var fields = new string[] {"key", "value"};
    
            // create window
            string stmtTextCreate = "create window MyWindow#Sort(3, value asc) as MyMap";
            EPStatement stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtCreate.AddListener(_listenerWindow);
    
            // create insert into
            string stmtTextInsert = "insert into MyWindow select theString as key, longBoxed as value from " + typeof(SupportBean).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // create consumer
            string stmtTextSelectOne = "select key, value as value from MyWindow";
            EPStatement stmtSelectOne = _epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            stmtSelectOne.AddListener(_listenerStmtOne);
    
            // create delete stmt
            string stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + " as s0 delete from MyWindow as s1 where s0.symbol = s1.key";
            EPStatement stmtDelete = _epService.EPAdministrator.CreateEPL(stmtTextDelete);
            stmtDelete.AddListener(_listenerStmtDelete);
    
            SendSupportBean("E1", 10L);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new Object[] {"E1", 10L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new Object[] {"E1", 10L});
    
            SendSupportBean("E2", 20L);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new Object[] {"E2", 20L});
    
            SendSupportBean("E3", 15L);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new Object[] {"E3", 15L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E1", 10L}, new object[] {"E3", 15L}, new object[] {"E2", 20L}});
    
            // delete E2
            SendMarketBean("E2");
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetOldAndReset(), fields, new Object[] {"E2", 20L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E1", 10L}, new object[] {"E3", 15L}});
    
            SendSupportBean("E4", 18L);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new Object[] {"E4", 18L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E1", 10L}, new object[] {"E3", 15L}, new object[] {"E4", 18L}});
    
            SendSupportBean("E5", 17L);
            EPAssertionUtil.AssertProps(_listenerWindow.LastNewData[0], fields, new Object[] {"E5", 17L});
            EPAssertionUtil.AssertProps(_listenerWindow.LastOldData[0], fields, new Object[] {"E4", 18L});
            _listenerWindow.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E1", 10L}, new object[] {"E3", 15L}, new object[] {"E5", 17L}});
    
            // delete E1
            SendMarketBean("E1");
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetOldAndReset(), fields, new Object[] {"E1", 10L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E3", 15L}, new object[] {"E5", 17L}});
    
            SendSupportBean("E6", 16L);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new Object[] {"E6", 16L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E3", 15L}, new object[] {"E6", 16L}, new object[] {"E5", 17L}});
    
            SendSupportBean("E7", 16L);
            EPAssertionUtil.AssertProps(_listenerWindow.LastNewData[0], fields, new Object[] {"E7", 16L});
            EPAssertionUtil.AssertProps(_listenerWindow.LastOldData[0], fields, new Object[] {"E5", 17L});
            _listenerWindow.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E3", 15L}, new object[] {"E7", 16L}, new object[] {"E6", 16L}});
    
            // delete E7 has no effect
            SendMarketBean("E7");
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetOldAndReset(), fields, new Object[] {"E7", 16L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E3", 15L}, new object[] {"E6", 16L}});
    
            SendSupportBean("E8", 1L);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new Object[] {"E8", 1L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E8", 1L}, new object[] {"E3", 15L}, new object[] {"E6", 16L}});
    
            SendSupportBean("E9", 1L);
            EPAssertionUtil.AssertProps(_listenerWindow.LastNewData[0], fields, new Object[] {"E9", 1L});
            EPAssertionUtil.AssertProps(_listenerWindow.LastOldData[0], fields, new Object[] {"E6", 16L});
            _listenerWindow.Reset();
        }
    
        public void TestTimeLengthBatch() {
            var fields = new string[] {"key", "value"};
    
            // create window
            string stmtTextCreate = "create window MyWindow#Time_length_batch(10 sec, 3) as MyMap";
            EPStatement stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtCreate.AddListener(_listenerWindow);
    
            // create insert into
            string stmtTextInsert = "insert into MyWindow select theString as key, longBoxed as value from " + typeof(SupportBean).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // create consumer
            string stmtTextSelectOne = "select key, value as value from MyWindow";
            EPStatement stmtSelectOne = _epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            stmtSelectOne.AddListener(_listenerStmtOne);
    
            // create delete stmt
            string stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + " as s0 delete from MyWindow as s1 where s0.symbol = s1.key";
            EPStatement stmtDelete = _epService.EPAdministrator.CreateEPL(stmtTextDelete);
            stmtDelete.AddListener(_listenerStmtDelete);
    
            SendTimer(1000);
            SendSupportBean("E1", 1L);
            SendSupportBean("E2", 2L);
            Assert.IsFalse(_listenerWindow.IsInvoked);
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E1", 1L}, new object[] {"E2", 2L}});
    
            // delete E2
            SendMarketBean("E2");
            Assert.IsFalse(_listenerWindow.IsInvoked);
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E1", 1L}});
    
            SendSupportBean("E3", 3L);
            Assert.IsFalse(_listenerWindow.IsInvoked);
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E1", 1L}, new object[] {"E3", 3L}});
    
            SendSupportBean("E4", 4L);
            Assert.IsNull(_listenerWindow.LastOldData);
            EventBean[] newData = _listenerWindow.LastNewData;
            Assert.AreEqual(3, newData.Length);
            EPAssertionUtil.AssertProps(newData[0], fields, new Object[] {"E1", 1L});
            EPAssertionUtil.AssertProps(newData[1], fields, new Object[] {"E3", 3L});
            EPAssertionUtil.AssertProps(newData[2], fields, new Object[] {"E4", 4L});
            _listenerWindow.Reset();
            _listenerStmtOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, null);
    
            SendTimer(5000);
            SendSupportBean("E5", 5L);
            SendSupportBean("E6", 6L);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E5", 5L}, new object[] {"E6", 6L}});
    
            SendMarketBean("E5");   // deleting E5
            Assert.IsFalse(_listenerWindow.IsInvoked);
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E6", 6L}});
    
            SendTimer(10999);
            Assert.IsFalse(_listenerWindow.IsInvoked);
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
    
            SendTimer(11000);
            newData = _listenerWindow.LastNewData;
            Assert.AreEqual(1, newData.Length);
            EPAssertionUtil.AssertProps(newData[0], fields, new Object[] {"E6", 6L});
            EventBean[] oldData = _listenerWindow.LastOldData;
            Assert.AreEqual(3, oldData.Length);
            EPAssertionUtil.AssertProps(oldData[0], fields, new Object[] {"E1", 1L});
            EPAssertionUtil.AssertProps(oldData[1], fields, new Object[] {"E3", 3L});
            EPAssertionUtil.AssertProps(oldData[2], fields, new Object[] {"E4", 4L});
            _listenerWindow.Reset();
            _listenerStmtOne.Reset();
        }
    
        public void TestLengthWindowPerGroup() {
            var fields = new string[] {"key", "value"};
    
            // create window
            string stmtTextCreate = "create window MyWindow#groupwin(value)#length(2) as MyMap";
            EPStatement stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtCreate.AddListener(_listenerWindow);
    
            // create insert into
            string stmtTextInsert = "insert into MyWindow select theString as key, longBoxed as value from " + typeof(SupportBean).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // create consumer
            string stmtTextSelectOne = "select irstream key, value as value from MyWindow";
            EPStatement stmtSelectOne = _epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            stmtSelectOne.AddListener(_listenerStmtOne);
    
            // create delete stmt
            string stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + " delete from MyWindow where symbol = key";
            EPStatement stmtDelete = _epService.EPAdministrator.CreateEPL(stmtTextDelete);
            stmtDelete.AddListener(_listenerStmtDelete);
    
            SendSupportBean("E1", 1L);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new Object[] {"E1", 1L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new Object[] {"E1", 1L});
    
            SendSupportBean("E2", 1L);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new Object[] {"E2", 1L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new Object[] {"E2", 1L});
    
            SendSupportBean("E3", 2L);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new Object[] {"E3", 2L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new Object[] {"E3", 2L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E1", 1L}, new object[] {"E2", 1L}, new object[] {"E3", 2L}});
    
            // delete E2
            SendMarketBean("E2");
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetOldAndReset(), fields, new Object[] {"E2", 1L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetOldAndReset(), fields, new Object[] {"E2", 1L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E1", 1L}, new object[] {"E3", 2L}});
    
            SendSupportBean("E4", 1L);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new Object[] {"E4", 1L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new Object[] {"E4", 1L});
    
            SendSupportBean("E5", 1L);
            EPAssertionUtil.AssertProps(_listenerWindow.LastNewData[0], fields, new Object[] {"E5", 1L});
            EPAssertionUtil.AssertProps(_listenerWindow.LastOldData[0], fields, new Object[] {"E1", 1L});
            _listenerWindow.Reset();
            EPAssertionUtil.AssertProps(_listenerStmtOne.LastNewData[0], fields, new Object[] {"E5", 1L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.LastOldData[0], fields, new Object[] {"E1", 1L});
            _listenerStmtOne.Reset();
    
            SendSupportBean("E6", 2L);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new Object[] {"E6", 2L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new Object[] {"E6", 2L});
    
            // delete E6
            SendMarketBean("E6");
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetOldAndReset(), fields, new Object[] {"E6", 2L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetOldAndReset(), fields, new Object[] {"E6", 2L});
    
            SendSupportBean("E7", 2L);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new Object[] {"E7", 2L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new Object[] {"E7", 2L});
    
            SendSupportBean("E8", 2L);
            EPAssertionUtil.AssertProps(_listenerWindow.LastNewData[0], fields, new Object[] {"E8", 2L});
            EPAssertionUtil.AssertProps(_listenerWindow.LastOldData[0], fields, new Object[] {"E3", 2L});
            _listenerWindow.Reset();
            EPAssertionUtil.AssertProps(_listenerStmtOne.LastNewData[0], fields, new Object[] {"E8", 2L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.LastOldData[0], fields, new Object[] {"E3", 2L});
            _listenerStmtOne.Reset();
        }
    
        public void TestTimeBatchPerGroup() {
            var fields = new string[] {"key", "value"};
    
            // create window
            SendTimer(0);
            string stmtTextCreate = "create window MyWindow#groupwin(value)#Time_batch(10 sec) as MyMap";
            EPStatement stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtCreate.AddListener(_listenerWindow);
    
            // create insert into
            string stmtTextInsert = "insert into MyWindow select theString as key, longBoxed as value from " + typeof(SupportBean).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // create consumer
            string stmtTextSelectOne = "select key, value as value from MyWindow";
            EPStatement stmtSelectOne = _epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            stmtSelectOne.AddListener(_listenerStmtOne);
    
            SendTimer(1000);
            SendSupportBean("E1", 10L);
            SendSupportBean("E2", 20L);
            SendSupportBean("E3", 20L);
            SendSupportBean("E4", 10L);
    
            SendTimer(11000);
            Assert.AreEqual(_listenerWindow.LastNewData.Length, 4);
            Assert.AreEqual(_listenerStmtOne.LastNewData.Length, 4);
            EPAssertionUtil.AssertProps(_listenerWindow.LastNewData[0], fields, new Object[] {"E1", 10L});
            EPAssertionUtil.AssertProps(_listenerWindow.LastNewData[1], fields, new Object[] {"E4", 10L});
            EPAssertionUtil.AssertProps(_listenerWindow.LastNewData[2], fields, new Object[] {"E2", 20L});
            EPAssertionUtil.AssertProps(_listenerWindow.LastNewData[3], fields, new Object[] {"E3", 20L});
            _listenerWindow.Reset();
            EPAssertionUtil.AssertProps(_listenerStmtOne.LastNewData[0], fields, new Object[] {"E1", 10L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.LastNewData[1], fields, new Object[] {"E4", 10L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.LastNewData[2], fields, new Object[] {"E2", 20L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.LastNewData[3], fields, new Object[] {"E3", 20L});
            _listenerStmtOne.Reset();
        }
    
        public void TestDoubleInsertSameWindow() {
            var fields = new string[] {"key", "value"};
    
            // create window
            string stmtTextCreate = "create window MyWindow#keepall as MyMap";
            EPStatement stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtCreate.AddListener(_listenerWindow);
    
            // create insert into
            string stmtTextInsert = "insert into MyWindow select theString as key, longBoxed+1 as value from " + typeof(SupportBean).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            stmtTextInsert = "insert into MyWindow select theString as key, longBoxed+2 as value from " + typeof(SupportBean).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // create consumer
            string stmtTextSelectOne = "select key, value as value from MyWindow";
            EPStatement stmtSelectOne = _epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            stmtSelectOne.AddListener(_listenerStmtOne);
    
            SendSupportBean("E1", 10L);
            Assert.AreEqual(2, _listenerWindow.NewDataList.Count);    // listener to window gets 2 individual events
            Assert.AreEqual(2, _listenerStmtOne.NewDataList.Count);   // listener to statement gets 1 individual event
            Assert.AreEqual(2, _listenerWindow.GetNewDataListFlattened().Length);
            Assert.AreEqual(2, _listenerStmtOne.GetNewDataListFlattened().Length);
            EPAssertionUtil.AssertPropsPerRow(_listenerStmtOne.GetNewDataListFlattened(), fields, new Object[][] { new object[] {"E1", 11L}, new object[] {"E1", 12L}});
            _listenerStmtOne.Reset();
        }
    
        public void TestLastEvent() {
            var fields = new string[] {"key", "value"};
    
            // create window
            string stmtTextCreate = "create window MyWindow#lastevent as MyMap";
            EPStatement stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtCreate.AddListener(_listenerWindow);
    
            // create insert into
            string stmtTextInsert = "insert into MyWindow select theString as key, longBoxed as value from " + typeof(SupportBean).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // create consumer
            string stmtTextSelectOne = "select irstream key, value as value from MyWindow";
            EPStatement stmtSelectOne = _epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            stmtSelectOne.AddListener(_listenerStmtOne);
    
            // create delete stmt
            string stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + " as s0 delete from MyWindow as s1 where s0.symbol = s1.key";
            EPStatement stmtDelete = _epService.EPAdministrator.CreateEPL(stmtTextDelete);
            stmtDelete.AddListener(_listenerStmtDelete);
    
            SendSupportBean("E1", 1L);
            EPAssertionUtil.AssertProps(_listenerStmtOne.LastNewData[0], fields, new Object[] {"E1", 1L});
            Assert.IsNull(_listenerStmtOne.LastOldData);
            _listenerStmtOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E1", 1L}});
    
            SendSupportBean("E2", 2L);
            EPAssertionUtil.AssertProps(_listenerStmtOne.LastNewData[0], fields, new Object[] {"E2", 2L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.LastOldData[0], fields, new Object[] {"E1", 1L});
            _listenerStmtOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E2", 2L}});
    
            // delete E2
            SendMarketBean("E2");
            EPAssertionUtil.AssertProps(_listenerStmtOne.LastOldData[0], fields, new Object[] {"E2", 2L});
            Assert.IsNull(_listenerStmtOne.LastNewData);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, null);
    
            SendSupportBean("E3", 3L);
            EPAssertionUtil.AssertProps(_listenerStmtOne.LastNewData[0], fields, new Object[] {"E3", 3L});
            Assert.IsNull(_listenerStmtOne.LastOldData);
            _listenerStmtOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E3", 3L}});
    
            // delete E3
            SendMarketBean("E3");
            EPAssertionUtil.AssertProps(_listenerStmtOne.LastOldData[0], fields, new Object[] {"E3", 3L});
            Assert.IsNull(_listenerStmtOne.LastNewData);
            _listenerStmtOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, null);
    
            SendSupportBean("E4", 4L);
            EPAssertionUtil.AssertProps(_listenerStmtOne.LastNewData[0], fields, new Object[] {"E4", 4L});
            Assert.IsNull(_listenerStmtOne.LastOldData);
            _listenerStmtOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E4", 4L}});
    
            // delete other event
            SendMarketBean("E1");
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
        }
    
        public void TestFirstEvent() {
            var fields = new string[] {"key", "value"};
    
            // create window
            string stmtTextCreate = "create window MyWindow#firstevent as MyMap";
            EPStatement stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtCreate.AddListener(_listenerWindow);
    
            // create insert into
            string stmtTextInsert = "insert into MyWindow select theString as key, longBoxed as value from " + typeof(SupportBean).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // create consumer
            string stmtTextSelectOne = "select irstream key, value as value from MyWindow";
            EPStatement stmtSelectOne = _epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            stmtSelectOne.AddListener(_listenerStmtOne);
    
            // create delete stmt
            string stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + " as s0 delete from MyWindow as s1 where s0.symbol = s1.key";
            EPStatement stmtDelete = _epService.EPAdministrator.CreateEPL(stmtTextDelete);
            stmtDelete.AddListener(_listenerStmtDelete);
    
            SendSupportBean("E1", 1L);
            EPAssertionUtil.AssertProps(_listenerStmtOne.LastNewData[0], fields, new Object[] {"E1", 1L});
            Assert.IsNull(_listenerStmtOne.LastOldData);
            _listenerStmtOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E1", 1L}});
    
            SendSupportBean("E2", 2L);
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E1", 1L}});
    
            // delete E2
            SendMarketBean("E1");
            EPAssertionUtil.AssertProps(_listenerStmtOne.LastOldData[0], fields, new Object[] {"E1", 1L});
            Assert.IsNull(_listenerStmtOne.LastNewData);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, null);
    
            SendSupportBean("E3", 3L);
            EPAssertionUtil.AssertProps(_listenerStmtOne.LastNewData[0], fields, new Object[] {"E3", 3L});
            Assert.IsNull(_listenerStmtOne.LastOldData);
            _listenerStmtOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E3", 3L}});
    
            // delete E3
            SendMarketBean("E2");   // no effect
            SendMarketBean("E3");
            EPAssertionUtil.AssertProps(_listenerStmtOne.LastOldData[0], fields, new Object[] {"E3", 3L});
            Assert.IsNull(_listenerStmtOne.LastNewData);
            _listenerStmtOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, null);
    
            SendSupportBean("E4", 4L);
            EPAssertionUtil.AssertProps(_listenerStmtOne.LastNewData[0], fields, new Object[] {"E4", 4L});
            Assert.IsNull(_listenerStmtOne.LastOldData);
            _listenerStmtOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E4", 4L}});
    
            // delete other event
            SendMarketBean("E1");
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
        }
    
        public void TestUnique() {
            var fields = new string[] {"key", "value"};
    
            // create window
            string stmtTextCreate = "create window MyWindow#unique(key) as MyMap";
            EPStatement stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtCreate.AddListener(_listenerWindow);
    
            // create insert into
            string stmtTextInsert = "insert into MyWindow select theString as key, longBoxed as value from " + typeof(SupportBean).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // create consumer
            string stmtTextSelectOne = "select irstream key, value as value from MyWindow";
            EPStatement stmtSelectOne = _epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            stmtSelectOne.AddListener(_listenerStmtOne);
    
            // create delete stmt
            string stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + " as s0 delete from MyWindow as s1 where s0.symbol = s1.key";
            EPStatement stmtDelete = _epService.EPAdministrator.CreateEPL(stmtTextDelete);
            stmtDelete.AddListener(_listenerStmtDelete);
    
            SendSupportBean("G1", 1L);
            EPAssertionUtil.AssertProps(_listenerStmtOne.LastNewData[0], fields, new Object[] {"G1", 1L});
            Assert.IsNull(_listenerStmtOne.LastOldData);
            _listenerStmtOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"G1", 1L}});
    
            SendSupportBean("G2", 20L);
            EPAssertionUtil.AssertProps(_listenerStmtOne.LastNewData[0], fields, new Object[] {"G2", 20L});
            Assert.IsNull(_listenerStmtOne.LastOldData);
            _listenerStmtOne.Reset();
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"G1", 1L}, new object[] {"G2", 20L}});
    
            // delete G2
            SendMarketBean("G2");
            EPAssertionUtil.AssertProps(_listenerStmtOne.LastOldData[0], fields, new Object[] {"G2", 20L});
            Assert.IsNull(_listenerStmtOne.LastNewData);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"G1", 1L}});
    
            SendSupportBean("G1", 2L);
            EPAssertionUtil.AssertProps(_listenerStmtOne.LastNewData[0], fields, new Object[] {"G1", 2L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.LastOldData[0], fields, new Object[] {"G1", 1L});
            _listenerStmtOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"G1", 2L}});
    
            SendSupportBean("G2", 21L);
            EPAssertionUtil.AssertProps(_listenerStmtOne.LastNewData[0], fields, new Object[] {"G2", 21L});
            Assert.IsNull(_listenerStmtOne.LastOldData);
            _listenerStmtOne.Reset();
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"G1", 2L}, new object[] {"G2", 21L}});
    
            SendSupportBean("G2", 22L);
            EPAssertionUtil.AssertProps(_listenerStmtOne.LastNewData[0], fields, new Object[] {"G2", 22L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.LastOldData[0], fields, new Object[] {"G2", 21L});
            _listenerStmtOne.Reset();
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"G1", 2L}, new object[] {"G2", 22L}});
    
            SendMarketBean("G1");
            EPAssertionUtil.AssertProps(_listenerStmtOne.LastOldData[0], fields, new Object[] {"G1", 2L});
            Assert.IsNull(_listenerStmtOne.LastNewData);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"G2", 22L}});
        }
    
        public void TestFirstUnique() {
            var fields = new string[] {"key", "value"};
    
            // create window
            string stmtTextCreate = "create window MyWindow#Firstunique(key) as MyMap";
            EPStatement stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtCreate.AddListener(_listenerWindow);
    
            // create insert into
            string stmtTextInsert = "insert into MyWindow select theString as key, longBoxed as value from " + typeof(SupportBean).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // create consumer
            string stmtTextSelectOne = "select irstream key, value as value from MyWindow";
            EPStatement stmtSelectOne = _epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            stmtSelectOne.AddListener(_listenerStmtOne);
    
            // create delete stmt
            string stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + " as s0 delete from MyWindow as s1 where s0.symbol = s1.key";
            EPStatement stmtDelete = _epService.EPAdministrator.CreateEPL(stmtTextDelete);
            stmtDelete.AddListener(_listenerStmtDelete);
    
            SendSupportBean("G1", 1L);
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new Object[] {"G1", 1L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"G1", 1L}});
    
            SendSupportBean("G2", 20L);
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new Object[] {"G2", 20L});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"G1", 1L}, new object[] {"G2", 20L}});
    
            // delete G2
            SendMarketBean("G2");
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetOldAndReset(), fields, new Object[] {"G2", 20L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"G1", 1L}});
    
            SendSupportBean("G1", 2L);  // ignored
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"G1", 1L}});
    
            SendSupportBean("G2", 21L);
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new Object[] {"G2", 21L});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"G1", 1L}, new object[] {"G2", 21L}});
    
            SendSupportBean("G2", 22L); // ignored
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"G1", 1L}, new object[] {"G2", 21L}});
    
            SendMarketBean("G1");
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetOldAndReset(), fields, new Object[] {"G1", 1L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"G2", 21L}});
        }
    
        public void TestFilteringConsumer() {
            var fields = new string[] {"key", "value"};
    
            // create window
            string stmtTextCreate = "create window MyWindow#unique(key) as select theString as key, intPrimitive as value from " + typeof(SupportBean).FullName;
            EPStatement stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtCreate.AddListener(_listenerWindow);
    
            // create insert into
            string stmtTextInsert = "insert into MyWindow select theString as key, intPrimitive as value from " + typeof(SupportBean).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // create consumer
            string stmtTextSelectOne = "select irstream key, value as value from MyWindow(value > 0, value < 10)";
            EPStatement stmtSelectOne = _epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            stmtSelectOne.AddListener(_listenerStmtOne);
    
            // create delete stmt
            string stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + " as s0 delete from MyWindow as s1 where s0.symbol = s1.key";
            EPStatement stmtDelete = _epService.EPAdministrator.CreateEPL(stmtTextDelete);
            stmtDelete.AddListener(_listenerStmtDelete);
    
            SendSupportBeanInt("G1", 5);
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new Object[] {"G1", 5});
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new Object[] {"G1", 5});
    
            SendSupportBeanInt("G1", 15);
            EPAssertionUtil.AssertProps(_listenerStmtOne.LastOldData[0], fields, new Object[] {"G1", 5});
            Assert.IsNull(_listenerStmtOne.LastNewData);
            _listenerStmtOne.Reset();
            EPAssertionUtil.AssertProps(_listenerWindow.LastOldData[0], fields, new Object[] {"G1", 5});
            EPAssertionUtil.AssertProps(_listenerWindow.LastNewData[0], fields, new Object[] {"G1", 15});
            _listenerWindow.Reset();
    
            // send G2
            SendSupportBeanInt("G2", 8);
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new Object[] {"G2", 8});
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new Object[] {"G2", 8});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"G1", 15}, new object[] {"G2", 8}});
            EPAssertionUtil.AssertPropsPerRow(stmtSelectOne.GetEnumerator(), fields, new Object[][] { new object[] {"G2", 8}});
    
            // delete G2
            SendMarketBean("G2");
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetOldAndReset(), fields, new Object[] {"G2", 8});
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetOldAndReset(), fields, new Object[] {"G2", 8});
    
            // send G3
            SendSupportBeanInt("G3", -1);
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new Object[] {"G3", -1});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"G1", 15}, new object[] {"G3", -1}});
            EPAssertionUtil.AssertPropsPerRow(stmtSelectOne.GetEnumerator(), fields, null);
    
            // delete G2
            SendMarketBean("G3");
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetOldAndReset(), fields, new Object[] {"G3", -1});
    
            SendSupportBeanInt("G1", 6);
            SendSupportBeanInt("G2", 7);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtSelectOne.GetEnumerator(), fields, new Object[][] { new object[] {"G1", 6}, new object[] {"G2", 7}});
    
            stmtSelectOne.Dispose();
            stmtDelete.Dispose();
        }
    
        public void TestSelectGroupedViewLateStart() {
            // create window
            string stmtTextCreate = "create window MyWindow#groupwin(theString, intPrimitive)#length(9) as select theString, intPrimitive from " + typeof(SupportBean).FullName;
            EPStatement stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
    
            // create insert into
            string stmtTextInsert = "insert into MyWindow select theString, intPrimitive from " + typeof(SupportBean).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // fill window
            var stringValues = new string[] {"c0", "c1", "c2"};
            for (int i = 0; i < stringValues.Length; i++) {
                for (int j = 0; j < 3; j++) {
                    _epService.EPRuntime.SendEvent(new SupportBean(stringValues[i], j));
                }
            }
            _epService.EPRuntime.SendEvent(new SupportBean("c0", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("c1", 2));
            _epService.EPRuntime.SendEvent(new SupportBean("c3", 3));
            EventBean[] received = EPAssertionUtil.EnumeratorToArray(stmtCreate.GetEnumerator());
            Assert.AreEqual(12, received.Length);
    
            // create select stmt
            string stmtTextSelect = "select theString, intPrimitive, Count(*) from MyWindow group by theString, intPrimitive order by theString, intPrimitive";
            EPStatement stmtSelect = _epService.EPAdministrator.CreateEPL(stmtTextSelect);
            received = EPAssertionUtil.EnumeratorToArray(stmtSelect.GetEnumerator());
            Assert.AreEqual(10, received.Length);
    
            EPAssertionUtil.AssertPropsPerRow(received, "theString,intPrimitive,Count(*)".SplitCsv(),
            new Object[][] {
                new object[] {"c0", 0, 1L},
                new object[] {"c0", 1, 2L},
                new object[] {"c0", 2, 1L},
                new object[] {"c1", 0, 1L},
                new object[] {"c1", 1, 1L},
                new object[] {"c1", 2, 2L},
                new object[] {"c2", 0, 1L},
                new object[] {"c2", 1, 1L},
                new object[] {"c2", 2, 1L},
                new object[] {"c3", 3, 1L},
            });
    
            stmtSelect.Dispose();
            stmtCreate.Dispose();
        }
    
        public void TestSelectGroupedViewLateStartVariableIterate() {
            // create window
            string stmtTextCreate = "create window MyWindow#groupwin(theString, intPrimitive)lLength(9) as select theString, intPrimitive, longPrimitive, boolPrimitive from " + typeof(SupportBean).FullName;
            EPStatement stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
    
            // create insert into
            string stmtTextInsert = "insert into MyWindow select theString, intPrimitive, longPrimitive, boolPrimitive from " + typeof(SupportBean).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // create variable
            _epService.EPAdministrator.CreateEPL("create variable string var_1_1_1");
            _epService.EPAdministrator.CreateEPL("on " + typeof(SupportVariableSetEvent).FullName + "(variableName='var_1_1_1') set var_1_1_1 = value");
    
            // fill window
            var stringValues = new string[] {"c0", "c1", "c2"};
            for (int i = 0; i < stringValues.Length; i++) {
                for (int j = 0; j < 3; j++) {
                    var beanX = new SupportBean(stringValues[i], j);
                    beanX.LongPrimitive = j;
                    beanX.BoolPrimitive = true;
                    _epService.EPRuntime.SendEvent(beanX);
                }
            }
            // extra record to create non-uniform data
            var bean = new SupportBean("c1", 1);
            bean.LongPrimitive = 10;
            bean.BoolPrimitive = true;
            _epService.EPRuntime.SendEvent(bean);
            EventBean[] received = EPAssertionUtil.EnumeratorToArray(stmtCreate.GetEnumerator());
            Assert.AreEqual(10, received.Length);
    
            // create select stmt
            string stmtTextSelect = "select theString, intPrimitive, Avg(longPrimitive) as avgLong, Count(boolPrimitive) as cntBool" +
                                    " from MyWindow group by theString, intPrimitive having theString = var_1_1_1 order by theString, intPrimitive";
            EPStatement stmtSelect = _epService.EPAdministrator.CreateEPL(stmtTextSelect);
    
            // set variable to C0
            _epService.EPRuntime.SendEvent(new SupportVariableSetEvent("var_1_1_1", "c0"));
    
            // get iterator results
            received = EPAssertionUtil.EnumeratorToArray(stmtSelect.GetEnumerator());
            Assert.AreEqual(3, received.Length);
            EPAssertionUtil.AssertPropsPerRow(received, "theString,intPrimitive,avgLong,cntBool".SplitCsv(),
            new Object[][] {
                new object[] {"c0", 0, 0.0, 1L},
                new object[] {"c0", 1, 1.0, 1L},
                new object[] {"c0", 2, 2.0, 1L},
            });
    
            // set variable to C1
            _epService.EPRuntime.SendEvent(new SupportVariableSetEvent("var_1_1_1", "c1"));
    
            received = EPAssertionUtil.EnumeratorToArray(stmtSelect.GetEnumerator());
            Assert.AreEqual(3, received.Length);
            EPAssertionUtil.AssertPropsPerRow(received, "theString,intPrimitive,avgLong,cntBool".SplitCsv(),
            new Object[][] {
                new object[] {"c1", 0, 0.0, 1L},
                new object[] {"c1", 1, 5.5, 2L},
                new object[] {"c1", 2, 2.0, 1L},
            });
    
            stmtSelect.Dispose();
            stmtCreate.Dispose();
        }
    
        public void TestFilteringConsumerLateStart() {
            var fields = new string[] {"sumvalue"};
    
            // create window
            string stmtTextCreate = "create window MyWindow#keepall as select theString as key, intPrimitive as value from " + typeof(SupportBean).FullName;
            EPStatement stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtCreate.AddListener(_listenerWindow);
    
            // create insert into
            string stmtTextInsert = "insert into MyWindow select theString as key, intPrimitive as value from " + typeof(SupportBean).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            SendSupportBeanInt("G1", 5);
            SendSupportBeanInt("G2", 15);
            SendSupportBeanInt("G3", 2);
    
            // create consumer
            string stmtTextSelectOne = "select irstream Sum(value) as sumvalue from MyWindow(value > 0, value < 10)";
            EPStatement stmtSelectOne = _epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            stmtSelectOne.AddListener(_listenerStmtOne);
            EPAssertionUtil.AssertPropsPerRow(stmtSelectOne.GetEnumerator(), fields, new Object[][] { new object[] { 7 }});
    
            SendSupportBeanInt("G4", 1);
            EPAssertionUtil.AssertProps(_listenerStmtOne.LastNewData[0], fields, new Object[] {8});
            EPAssertionUtil.AssertProps(_listenerStmtOne.LastOldData[0], fields, new Object[] {7});
            _listenerStmtOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtSelectOne.GetEnumerator(), fields, new Object[][] { new object[] { 8 }});
    
            SendSupportBeanInt("G5", 20);
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmtSelectOne.GetEnumerator(), fields, new Object[][] { new object[] { 8 }});
    
            SendSupportBeanInt("G6", 9);
            EPAssertionUtil.AssertProps(_listenerStmtOne.LastNewData[0], fields, new Object[] {17});
            EPAssertionUtil.AssertProps(_listenerStmtOne.LastOldData[0], fields, new Object[] {8});
            _listenerStmtOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtSelectOne.GetEnumerator(), fields, new Object[][] { new object[] { 17 }});
    
            // create delete stmt
            string stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + " as s0 delete from MyWindow as s1 where s0.symbol = s1.key";
            EPStatement stmtDelete = _epService.EPAdministrator.CreateEPL(stmtTextDelete);
            stmtDelete.AddListener(_listenerStmtDelete);
    
            SendMarketBean("G4");
            EPAssertionUtil.AssertProps(_listenerStmtOne.LastNewData[0], fields, new Object[] {16});
            EPAssertionUtil.AssertProps(_listenerStmtOne.LastOldData[0], fields, new Object[] {17});
            _listenerStmtOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtSelectOne.GetEnumerator(), fields, new Object[][] { new object[] { 16 }});
    
            SendMarketBean("G5");
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmtSelectOne.GetEnumerator(), fields, new Object[][] { new object[] { 16 }});
    
            stmtSelectOne.Dispose();
            stmtDelete.Dispose();
        }
    
        public void TestInvalid() {
            Assert.AreEqual("Error starting statement: Named windows require one or more child views that are data window views [create window MyWindow#groupwin(value)#uni(value) as MyMap]",
                         TryInvalid("create window MyWindow#groupwin(value)#uni(value) as MyMap"));
    
            Assert.AreEqual("Named windows require one or more child views that are data window views [create window MyWindow as MyMap]",
                         TryInvalid("create window MyWindow as MyMap"));
    
            Assert.AreEqual("Named window or table 'dummy' has not been declared [on MyMap delete from dummy]",
                         TryInvalid("on MyMap delete from dummy"));
    
            _epService.EPAdministrator.CreateEPL("create window SomeWindow#keepall as (a int)");
            SupportMessageAssertUtil.TryInvalid(_epService, "update SomeWindow set a = 'a' where a = 'b'",
                                                "Provided EPL expression is an on-demand query expression (not a continuous query), please use the runtime executeQuery API instead");
            SupportMessageAssertUtil.TryInvalidExecuteQuery(_epService, "update istream SomeWindow set a = 'a' where a = 'b'",
                    "Provided EPL expression is a continuous query expression (not an on-demand query), please use the administrator createEPL API instead");
    
            // test model-after with no field
            var innerType = new Dictionary<string, Object>();
            innerType.Put("key", typeof(string));
            _epService.EPAdministrator.Configuration.AddEventType("InnerMap", innerType);
            var outerType = new Dictionary<string, Object>();
            outerType.Put("innermap", "InnerMap");
            _epService.EPAdministrator.Configuration.AddEventType("OuterMap", outerType);
            try {
                _epService.EPAdministrator.CreateEPL("create window MyWindow#keepall as select innermap.abc from OuterMap");
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual("Failed to validate select-clause expression 'innermap.abc': Failed to resolve property 'innermap.abc' to a stream or nested property in a stream [create window MyWindow#keepall as select innermap.abc from OuterMap]", ex.Message);
            }
        }
    
        public void TestAlreadyExists() {
            _epService.EPAdministrator.CreateEPL("create window MyWindow#keepall as MyMap");
            try {
                _epService.EPAdministrator.CreateEPL("create window MyWindow#keepall as MyMap");
                Assert.Fail();
            } catch (EPException ex) {
                Assert.AreEqual("Error starting statement: A named window by name 'MyWindow' has already been created [create window MyWindow#keepall as MyMap]", ex.Message);
            }
        }
    
        public void TestConsumerDataWindow() {
            _epService.EPAdministrator.CreateEPL("create window MyWindow#keepall as MyMap");
            try {
                _epService.EPAdministrator.CreateEPL("select key, value as value from MyWindow#Time(10 sec)");
                Assert.Fail();
            } catch (EPException ex) {
                Assert.AreEqual("Error starting statement: Consuming statements to a named window cannot declare a data window view onto the named window [select key, value as value from MyWindow#Time(10 sec)]", ex.Message);
            }
        }
    
        private string TryInvalid(string expression) {
            try {
                _epService.EPAdministrator.CreateEPL(expression);
                Assert.Fail();
            } catch (EPException ex) {
                return ex.Message;
            }
            return null;
        }
    
        public void TestPriorStats() {
            var fieldsPrior = new string[] {"priorKeyOne", "priorKeyTwo"};
            var fieldsStat = new string[] {"average"};
    
            string stmtTextCreate = "create window MyWindow#keepall as MyMap";
            EPStatement stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtCreate.AddListener(_listenerWindow);
    
            Assert.AreEqual(typeof(string), stmtCreate.EventType.GetPropertyType("key"));
            Assert.AreEqual(typeof(long), stmtCreate.EventType.GetPropertyType("value"));
    
            string stmtTextInsert = "insert into MyWindow select theString as key, longBoxed as value from " + typeof(SupportBean).FullName;
            EPStatement stmtInsert = _epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            string stmtTextSelectOne = "select Prior(1, key) as priorKeyOne, Prior(2, key) as priorKeyTwo from MyWindow";
            EPStatement stmtSelectOne = _epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            stmtSelectOne.AddListener(_listenerStmtOne);
    
            string stmtTextSelectThree = "select average from MyWindow#uni(value)";
            EPStatement stmtSelectThree = _epService.EPAdministrator.CreateEPL(stmtTextSelectThree);
            stmtSelectThree.AddListener(_listenerStmtThree);
    
            // send events
            SendSupportBean("E1", 1L);
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fieldsPrior, new Object[] {null, null});
            EPAssertionUtil.AssertProps(_listenerStmtThree.LastNewData[0], fieldsStat, new Object[] {1d});
            EPAssertionUtil.AssertPropsPerRow(stmtSelectThree.GetEnumerator(), fieldsStat, new Object[][] { new object[] { 1d}});
    
            SendSupportBean("E2", 2L);
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fieldsPrior, new Object[] {"E1", null});
            EPAssertionUtil.AssertProps(_listenerStmtThree.LastNewData[0], fieldsStat, new Object[] {1.5d});
            EPAssertionUtil.AssertPropsPerRow(stmtSelectThree.GetEnumerator(), fieldsStat, new Object[][] { new object[] { 1.5d}});
    
            SendSupportBean("E3", 2L);
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fieldsPrior, new Object[] {"E2", "E1"});
            EPAssertionUtil.AssertProps(_listenerStmtThree.LastNewData[0], fieldsStat, new Object[] {5 / 3d});
            EPAssertionUtil.AssertPropsPerRow(stmtSelectThree.GetEnumerator(), fieldsStat, new Object[][] { new object[] { 5 / 3d}});
    
            SendSupportBean("E4", 2L);
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fieldsPrior, new Object[] {"E3", "E2"});
            EPAssertionUtil.AssertProps(_listenerStmtThree.LastNewData[0], fieldsStat, new Object[] {1.75});
            EPAssertionUtil.AssertPropsPerRow(stmtSelectThree.GetEnumerator(), fieldsStat, new Object[][] { new object[] { 1.75d}});
        }
    
        public void TestLateConsumer() {
            var fieldsWin = new string[] {"key", "value"};
            var fieldsStat = new string[] {"average"};
            var fieldsCnt = new string[] {"cnt"};
    
            string stmtTextCreate = "create window MyWindow#keepall as MyMap";
            EPStatement stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtCreate.AddListener(_listenerWindow);
    
            Assert.AreEqual(typeof(string), stmtCreate.EventType.GetPropertyType("key"));
            Assert.AreEqual(typeof(long), stmtCreate.EventType.GetPropertyType("value"));
    
            string stmtTextInsert = "insert into MyWindow select theString as key, longBoxed as value from " + typeof(SupportBean).FullName;
            EPStatement stmtInsert = _epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // send events
            SendSupportBean("E1", 1L);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fieldsWin, new Object[] {"E1", 1L});
    
            SendSupportBean("E2", 2L);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fieldsWin, new Object[] {"E2", 2L});
    
            string stmtTextSelectOne = "select irstream average from MyWindow#uni(value)";
            EPStatement stmtSelectOne = _epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            stmtSelectOne.AddListener(_listenerStmtOne);
            EPAssertionUtil.AssertPropsPerRow(stmtSelectOne.GetEnumerator(), fieldsStat, new Object[][] { new object[] { 1.5d}});
    
            SendSupportBean("E3", 2L);
            EPAssertionUtil.AssertProps(_listenerStmtOne.LastNewData[0], fieldsStat, new Object[] {5 / 3d});
            EPAssertionUtil.AssertProps(_listenerStmtOne.LastOldData[0], fieldsStat, new Object[] {3 / 2d});
            _listenerStmtOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtSelectOne.GetEnumerator(), fieldsStat, new Object[][] { new object[] { 5 / 3d}});
    
            SendSupportBean("E4", 2L);
            EPAssertionUtil.AssertProps(_listenerStmtOne.LastNewData[0], fieldsStat, new Object[] {7 / 4d});
            EPAssertionUtil.AssertPropsPerRow(stmtSelectOne.GetEnumerator(), fieldsStat, new Object[][] { new object[] { 7 / 4d}});
    
            string stmtTextSelectTwo = "select Count(*) as cnt from MyWindow";
            EPStatement stmtSelectTwo = _epService.EPAdministrator.CreateEPL(stmtTextSelectTwo);
            stmtSelectTwo.AddListener(_listenerStmtTwo);
            EPAssertionUtil.AssertPropsPerRow(stmtSelectTwo.GetEnumerator(), fieldsCnt, new Object[][] { new object[] { 4L}});
            EPAssertionUtil.AssertPropsPerRow(stmtSelectOne.GetEnumerator(), fieldsStat, new Object[][] { new object[] { 7 / 4d}});
    
            SendSupportBean("E5", 3L);
            EPAssertionUtil.AssertProps(_listenerStmtOne.LastNewData[0], fieldsStat, new Object[] {10 / 5d});
            EPAssertionUtil.AssertProps(_listenerStmtOne.LastOldData[0], fieldsStat, new Object[] {7 / 4d});
            _listenerStmtOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtSelectOne.GetEnumerator(), fieldsStat, new Object[][] { new object[] { 10 / 5d}});
            EPAssertionUtil.AssertPropsPerRow(stmtSelectTwo.GetEnumerator(), fieldsCnt, new Object[][] { new object[] { 5L}});
        }
    
        public void TestLateConsumerJoin() {
            var fieldsWin = new string[] {"key", "value"};
            var fieldsJoin = new string[] {"key", "value", "symbol"};
    
            string stmtTextCreate = "create window MyWindow#keepall as MyMap";
            EPStatement stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtCreate.AddListener(_listenerWindow);
    
            Assert.AreEqual(typeof(string), stmtCreate.EventType.GetPropertyType("key"));
            Assert.AreEqual(typeof(long), stmtCreate.EventType.GetPropertyType("value"));
    
            string stmtTextInsert = "insert into MyWindow select theString as key, longBoxed as value from " + typeof(SupportBean).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // send events
            SendSupportBean("E1", 1L);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fieldsWin, new Object[] {"E1", 1L});
    
            SendSupportBean("E2", 1L);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fieldsWin, new Object[] {"E2", 1L});
    
            // This replays into MyWindow
            string stmtTextSelectTwo = "select key, value, symbol from MyWindow as s0" +
                                       " left outer join " + typeof(SupportMarketDataBean).FullName + "#keepall as s1" +
                                       " on s0.value = s1.volume";
            EPStatement stmtSelectTwo = _epService.EPAdministrator.CreateEPL(stmtTextSelectTwo);
            stmtSelectTwo.AddListener(_listenerStmtTwo);
            Assert.IsFalse(_listenerStmtTwo.IsInvoked);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtSelectTwo.GetEnumerator(), fieldsJoin, new Object[][] { new object[] {"E1", 1L, null}, new object[] {"E2", 1L, null}});
    
            SendMarketBean("S1", 1);    // join on long
            Assert.AreEqual(2, _listenerStmtTwo.LastNewData.Length);
            if (_listenerStmtTwo.LastNewData[0].Get("key").Equals("E1")) {
                EPAssertionUtil.AssertProps(_listenerStmtTwo.LastNewData[0], fieldsJoin, new Object[] {"E1", 1L, "S1"});
                EPAssertionUtil.AssertProps(_listenerStmtTwo.LastNewData[1], fieldsJoin, new Object[] {"E2", 1L, "S1"});
            } else {
                EPAssertionUtil.AssertProps(_listenerStmtTwo.LastNewData[0], fieldsJoin, new Object[] {"E2", 1L, "S1"});
                EPAssertionUtil.AssertProps(_listenerStmtTwo.LastNewData[1], fieldsJoin, new Object[] {"E1", 1L, "S1"});
            }
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtSelectTwo.GetEnumerator(), fieldsJoin, new Object[][] { new object[] {"E1", 1L, "S1"}, new object[] {"E2", 1L, "S1"}});
            _listenerStmtTwo.Reset();
    
            SendMarketBean("S2", 2);    // join on long
            Assert.IsFalse(_listenerStmtTwo.IsInvoked);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtSelectTwo.GetEnumerator(), fieldsJoin, new Object[][] { new object[] {"E1", 1L, "S1"}, new object[] {"E2", 1L, "S1"}});
    
            SendSupportBean("E3", 2L);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fieldsWin, new Object[] {"E3", 2L});
            EPAssertionUtil.AssertProps(_listenerStmtTwo.LastNewData[0], fieldsJoin, new Object[] {"E3", 2L, "S2"});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtSelectTwo.GetEnumerator(), fieldsJoin, new Object[][] { new object[] {"E1", 1L, "S1"}, new object[] {"E2", 1L, "S1"}, new object[] {"E3", 2L, "S2"}});
        }
    
        public void TestPattern() {
            var fields = new string[] {"key", "value"};
            string stmtTextCreate = "create window MyWindow#keepall as MyMap";
            EPStatement stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtCreate.AddListener(_listenerWindow);
    
            string stmtTextPattern = "select a.key as key, a.value as value from pattern [every a=MyWindow(key='S1') or a=MyWindow(key='S2')]";
            EPStatement stmtPattern = _epService.EPAdministrator.CreateEPL(stmtTextPattern);
            stmtPattern.AddListener(_listenerStmtOne);
    
            string stmtTextInsert = "insert into MyWindow select theString as key, longBoxed as value from " + typeof(SupportBean).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            SendSupportBean("E1", 1L);
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
    
            SendSupportBean("S1", 2L);
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new Object[] {"S1", 2L});
    
            SendSupportBean("S1", 3L);
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new Object[] {"S1", 3L});
    
            SendSupportBean("S2", 4L);
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new Object[] {"S2", 4L});
    
            SendSupportBean("S1", 1L);
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
        }
    
        public void TestExternallyTimedBatch() {
            var fields = new string[] {"key", "value"};
    
            // create window
            string stmtTextCreate = "create window MyWindow#Ext_timed_batch(value, 10 sec, 0L) as MyMap";
            EPStatement stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtCreate.AddListener(_listenerWindow);
    
            // create insert into
            string stmtTextInsert = "insert into MyWindow select theString as key, longBoxed as value from " + typeof(SupportBean).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // create consumer
            string stmtTextSelectOne = "select irstream key, value as value from MyWindow";
            EPStatement stmtSelectOne = _epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            stmtSelectOne.AddListener(_listenerStmtOne);
    
            // create delete stmt
            string stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + " as s0 delete from MyWindow as s1 where s0.symbol = s1.key";
            EPStatement stmtDelete = _epService.EPAdministrator.CreateEPL(stmtTextDelete);
            stmtDelete.AddListener(_listenerStmtDelete);
    
            SendSupportBean("E1", 1000L);
            SendSupportBean("E2", 8000L);
            SendSupportBean("E3", 9999L);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E1", 1000L}, new object[] {"E2", 8000L}, new object[] {"E3", 9999L}});
    
            // delete E2
            SendMarketBean("E2");
            EPAssertionUtil.AssertPropsPerRow(_listenerWindow.AssertInvokedAndReset(), fields, null, new Object[][] { new object[] {"E2", 8000L}});
            EPAssertionUtil.AssertPropsPerRow(_listenerStmtOne.AssertInvokedAndReset(), fields, null, new Object[][] { new object[] {"E2", 8000L}});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E1", 1000L}, new object[] {"E3", 9999L}});
    
            SendSupportBean("E4", 10000L);
            EPAssertionUtil.AssertPropsPerRow(_listenerWindow.AssertInvokedAndReset(), fields,
            new Object[][] { new object[] {"E1", 1000L}, new object[] {"E3", 9999L}}, null);
            EPAssertionUtil.AssertPropsPerRow(_listenerStmtOne.AssertInvokedAndReset(), fields,
            new Object[][] { new object[] {"E1", 1000L}, new object[] {"E3", 9999L}}, null);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E4", 10000L}});
    
            // delete E4
            SendMarketBean("E4");
            EPAssertionUtil.AssertPropsPerRow(_listenerWindow.AssertInvokedAndReset(), fields, null, new Object[][] { new object[] {"E4", 10000L}});
            EPAssertionUtil.AssertPropsPerRow(_listenerStmtOne.AssertInvokedAndReset(), fields, null, new Object[][] { new object[] {"E4", 10000L}});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, null);
    
            SendSupportBean("E5", 14000L);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E5", 14000L}});
    
            SendSupportBean("E6", 21000L);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][] { new object[] {"E6", 21000L}});
            EPAssertionUtil.AssertPropsPerRow(_listenerWindow.AssertInvokedAndReset(), fields,
            new Object[][] { new object[] {"E5", 14000L}}, new Object[][] { new object[] {"E1", 1000L}, new object[] {"E3", 9999L}});
        }
    
        private SupportBean SendSupportBean(string theString, long longBoxed) {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.LongBoxed = longBoxed;
            _epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    
        private SupportBean SendSupportBeanInt(string theString, int intPrimitive) {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            _epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    
        private void SendMarketBean(string symbol) {
            var bean = new SupportMarketDataBean(symbol, 0, 0L, "");
            _epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendMarketBean(string symbol, long volume) {
            var bean = new SupportMarketDataBean(symbol, 0, volume, "");
            _epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendTimer(long timeInMSec) {
            var theEvent = new CurrentTimeEvent(timeInMSec);
            EPRuntime runtime = _epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }

        public class MyAvroTypeWidenerFactory : ObjectValueTypeWidenerFactory
        {
            public TypeWidener Make(ObjectValueTypeWidenerFactoryContext context) {
                if (context.GetClazz() == typeof(SupportBean_S0)) {
                    return val => {
                        var row = new GenericRecord(GetSupportBeanS0Schema());
                        row.Put("p00", ((SupportBean_S0)val).P00);
                        return row;
                    };
                }
                return null;
            }
        }
    
        public class MyAvroTypeRepMapper : TypeRepresentationMapper {
            public Object Map(TypeRepresentationMapperContext context) {
                if (context.GetClazz() == typeof(SupportBean_S0)) {
                    return GetSupportBeanS0Schema();
                }
                return null;
            }
        }
    
        public static RecordSchema GetSupportBeanS0Schema() {
            return SchemaBuilder.Record("SupportBean_S0", TypeBuilder.RequiredString("p00"));
        }
    }
} // end of namespace
