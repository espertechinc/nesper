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
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.events;
using com.espertech.esper.events.map;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestNamedWindowTypes 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listenerWindow;
        private SupportUpdateListener _listenerStmtOne;
        private SupportUpdateListener _listenerStmtDelete;
    
        [SetUp]
        public void SetUp()
        {
            IDictionary<String, object> types = new Dictionary<string, object>();
            types.Put("key", typeof(string));
            types.Put("primitive", typeof(long));
            types.Put("boxed", typeof(long?));
    
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("MyMap", types);
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
    
            _listenerWindow = new SupportUpdateListener();
            _listenerStmtOne = new SupportUpdateListener();
            _listenerStmtDelete = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
            _listenerWindow = null;
            _listenerStmtOne = null;
            _listenerStmtDelete = null;
        }
    
        [Test]
        public void TestEventTypeColumnDef()
        {
            RunAssertionEventTypeColumnDef(EventRepresentationEnum.OBJECTARRAY);
            RunAssertionEventTypeColumnDef(EventRepresentationEnum.MAP);
            RunAssertionEventTypeColumnDef(EventRepresentationEnum.DEFAULT);
        }
    
        public void RunAssertionEventTypeColumnDef(EventRepresentationEnum eventRepresentationEnum)
        {
            var stmtSchema = _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema SchemaOne(col1 int, col2 int)");
            Assert.AreEqual(eventRepresentationEnum.GetOutputClass(), stmtSchema.EventType.UnderlyingType);
    
            var stmt = _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create window SchemaWindow.std:lastevent() as (s1 SchemaOne)");
            Assert.AreEqual(eventRepresentationEnum.GetOutputClass(), stmt.EventType.UnderlyingType);
    
            stmt.AddListener(_listenerWindow);
            _epService.EPAdministrator.CreateEPL("insert into SchemaWindow (s1) select sone from SchemaOne as sone");
            
            IDictionary<String, object> theEvent = new LinkedHashMap<string, object>();
            theEvent.Put("col1", 10);
            theEvent.Put("col2", 11);
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                _epService.EPRuntime.SendEvent(theEvent.Values.ToArray(), "SchemaOne");
            }
            else {
                _epService.EPRuntime.SendEvent(theEvent, "SchemaOne");
            }
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), "s1.col1,s1.col2".Split(','), new object[]{10, 11});
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("SchemaOne", true);
            _epService.EPAdministrator.Configuration.RemoveEventType("SchemaWindow", true);
        }
    
        [Test]
        public void TestMapTranspose()
        {
            RunAssertionMapTranspose(EventRepresentationEnum.OBJECTARRAY);
            RunAssertionMapTranspose(EventRepresentationEnum.MAP);
            RunAssertionMapTranspose(EventRepresentationEnum.DEFAULT);
        }
    
        private void RunAssertionMapTranspose(EventRepresentationEnum eventRepresentationEnum)
        {
            IDictionary<String, object> innerTypeOne = new Dictionary<string, object>();
            innerTypeOne.Put("i1", typeof(int));
            IDictionary<String, object> innerTypeTwo = new Dictionary<string, object>();
            innerTypeTwo.Put("i2", typeof(int));
            IDictionary<String, object> outerType = new Dictionary<string, object>();
            outerType.Put("one", "T1");
            outerType.Put("two", "T2");
            _epService.EPAdministrator.Configuration.AddEventType("T1", innerTypeOne);
            _epService.EPAdministrator.Configuration.AddEventType("T2", innerTypeTwo);
            _epService.EPAdministrator.Configuration.AddEventType("OuterType", outerType);
    
            // create window
            var stmtTextCreate = eventRepresentationEnum.GetAnnotationText() + " create window MyWindow.win:keepall() as select one, two from OuterType";
            var stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
            Assert.AreEqual(eventRepresentationEnum.GetOutputClass(), stmtCreate.EventType.UnderlyingType);
            stmtCreate.AddListener(_listenerWindow);
            EPAssertionUtil.AssertEqualsAnyOrder(stmtCreate.EventType.PropertyNames, new string[]{"one", "two"});
            var eventType = stmtCreate.EventType;
            Assert.AreEqual("T1", eventType.GetFragmentType("one").FragmentType.Name);
            Assert.AreEqual("T2", eventType.GetFragmentType("two").FragmentType.Name);
    
            // create insert into
            var stmtTextInsertOne = "insert into MyWindow select one, two from OuterType";
            _epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            IDictionary<String, object> innerDataOne = new Dictionary<string, object>();
            innerDataOne.Put("i1", 1);
            IDictionary<String, object> innerDataTwo = new Dictionary<string, object>();
            innerDataTwo.Put("i2", 2);
            IDictionary<String, object> outerData = new Dictionary<string, object>();
            outerData.Put("one", innerDataOne);
            outerData.Put("two", innerDataTwo);
    
            _epService.EPRuntime.SendEvent(outerData, "OuterType");
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), "one.i1,two.i2".Split(','), new object[]{1, 2});
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyWindow", true);
        }
    
        [Test]
        public void TestNoWildcardWithAs()
        {
            // create window
            var stmtTextCreate = "create window MyWindow.win:keepall() as select TheString as a, LongPrimitive as b, LongBoxed as c from " + typeof(SupportBean).FullName;
            var stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtCreate.AddListener(_listenerWindow);
            EPAssertionUtil.AssertEqualsAnyOrder(stmtCreate.EventType.PropertyNames, new string[]{"a", "b", "c"});
            Assert.AreEqual(typeof(string), stmtCreate.EventType.GetPropertyType("a"));
            Assert.AreEqual(typeof(long?), stmtCreate.EventType.GetPropertyType("b"));
            Assert.AreEqual(typeof(long?), stmtCreate.EventType.GetPropertyType("c"));
    
            // assert type metadata
            var type = (EventTypeSPI) ((EPServiceProviderSPI)_epService).EventAdapterService.GetEventTypeByName("MyWindow");
            Assert.AreEqual(null, type.Metadata.OptionalApplicationType);
            Assert.AreEqual(null, type.Metadata.OptionalSecondaryNames);
            Assert.AreEqual("MyWindow", type.Metadata.PrimaryName);
            Assert.AreEqual("MyWindow", type.Metadata.PublicName);
            Assert.AreEqual("MyWindow", type.Name);
            Assert.AreEqual(TypeClass.NAMED_WINDOW, type.Metadata.TypeClass);
            Assert.AreEqual(false, type.Metadata.IsApplicationConfigured);
            Assert.AreEqual(false, type.Metadata.IsApplicationPreConfigured);
            Assert.AreEqual(false, type.Metadata.IsApplicationPreConfiguredStatic);
    
            // create insert into
            var stmtTextInsertOne = "insert into MyWindow select TheString as a, LongPrimitive as b, LongBoxed as c from " + typeof(SupportBean).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            var stmtTextInsertTwo = "insert into MyWindow select symbol as a, volume as b, volume as c from " + typeof(SupportMarketDataBean).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsertTwo);
    
            var stmtTextInsertThree = "insert into MyWindow select key as a, boxed as b, primitive as c from MyMap";
            _epService.EPAdministrator.CreateEPL(stmtTextInsertThree);
    
            // create consumer
            var stmtTextSelectOne = "select a, b, c from MyWindow";
            var stmtSelectOne = _epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            stmtSelectOne.AddListener(_listenerStmtOne);
            EPAssertionUtil.AssertEqualsAnyOrder(stmtSelectOne.EventType.PropertyNames, new string[]{"a", "b", "c"});
            Assert.AreEqual(typeof(string), stmtCreate.EventType.GetPropertyType("a"));
            Assert.AreEqual(typeof(long?), stmtCreate.EventType.GetPropertyType("b"));
            Assert.AreEqual(typeof(long?), stmtCreate.EventType.GetPropertyType("c"));
    
            // create delete stmt
            var stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + " as s0 delete from MyWindow as s1 where s0.symbol = s1.a";
            var stmtDelete = _epService.EPAdministrator.CreateEPL(stmtTextDelete);
            stmtDelete.AddListener(_listenerStmtDelete);
    
            SendSupportBean("E1", 1L, 10L);
            var fields = new string[] {"a", "b", "c"};
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1L, 10L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1L, 10L});
    
            SendMarketBean("S1", 99L);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"S1", 99L, 99L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"S1", 99L, 99L});
    
            SendMap("M1", 100L, 101L);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"M1", 101L, 100L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"M1", 101L, 100L});
        }
    
        [Test]
        public void TestNoWildcardNoAs()
        {
            // create window
            var stmtTextCreate = "create window MyWindow.win:keepall() as select TheString, LongPrimitive, LongBoxed from " + typeof(SupportBean).FullName;
            var stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtCreate.AddListener(_listenerWindow);
    
            // create insert into
            var stmtTextInsertOne = "insert into MyWindow select TheString, LongPrimitive, LongBoxed from " + typeof(SupportBean).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            var stmtTextInsertTwo = "insert into MyWindow select symbol as TheString, volume as LongPrimitive, volume as LongBoxed from " + typeof(SupportMarketDataBean).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsertTwo);
    
            var stmtTextInsertThree = "insert into MyWindow select key as TheString, boxed as LongPrimitive, primitive as LongBoxed from MyMap";
            _epService.EPAdministrator.CreateEPL(stmtTextInsertThree);
    
            // create consumer
            var stmtTextSelectOne = "select TheString, LongPrimitive, LongBoxed from MyWindow";
            var stmtSelectOne = _epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            stmtSelectOne.AddListener(_listenerStmtOne);
    
            SendSupportBean("E1", 1L, 10L);
            var fields = new string[] {"TheString", "LongPrimitive", "LongBoxed"};
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1L, 10L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1L, 10L});
    
            SendMarketBean("S1", 99L);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"S1", 99L, 99L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"S1", 99L, 99L});
    
            SendMap("M1", 100L, 101L);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"M1", 101L, 100L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"M1", 101L, 100L});
        }
    
        [Test]
        public void TestConstantsAs()
        {
            // create window
            var stmtTextCreate = "create window MyWindow.win:keepall() as select '' as TheString, 0L as LongPrimitive, 0L as LongBoxed from MyMap";
            var stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtCreate.AddListener(_listenerWindow);
    
            // create insert into
            var stmtTextInsertOne = "insert into MyWindow select TheString, LongPrimitive, LongBoxed from " + typeof(SupportBean).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            var stmtTextInsertTwo = "insert into MyWindow select symbol as TheString, volume as LongPrimitive, volume as LongBoxed from " + typeof(SupportMarketDataBean).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsertTwo);
    
            // create consumer
            var stmtTextSelectOne = "select TheString, LongPrimitive, LongBoxed from MyWindow";
            var stmtSelectOne = _epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            stmtSelectOne.AddListener(_listenerStmtOne);
    
            SendSupportBean("E1", 1L, 10L);
            var fields = new string[] {"TheString", "LongPrimitive", "LongBoxed"};
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1L, 10L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1L, 10L});
    
            SendMarketBean("S1", 99L);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"S1", 99L, 99L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"S1", 99L, 99L});
        }
    
        [Test]
        public void TestCreateSchemaModelAfter() {
            RunAssertionCreateSchemaModelAfter(EventRepresentationEnum.OBJECTARRAY);
            RunAssertionCreateSchemaModelAfter(EventRepresentationEnum.MAP);
            RunAssertionCreateSchemaModelAfter(EventRepresentationEnum.DEFAULT);
    
            // test model-after for PONO with inheritance
            _epService.EPAdministrator.CreateEPL("create window ParentWindow.win:keepall() as select * from " + typeof(NWTypesParentClass).MaskTypeName());
            _epService.EPAdministrator.CreateEPL("insert into ParentWindow select * from " + typeof(NWTypesParentClass).MaskTypeName());
            _epService.EPAdministrator.CreateEPL("create window ChildWindow.win:keepall() as select * from " + typeof(NWTypesChildClass).MaskTypeName());
            _epService.EPAdministrator.CreateEPL("insert into ChildWindow select * from " + typeof(NWTypesChildClass).MaskTypeName());
    
            var listener = new SupportUpdateListener();
            var parentQuery = "@Name('Parent') select parent from ParentWindow as parent";
            _epService.EPAdministrator.CreateEPL(parentQuery).AddListener(listener);
    
            _epService.EPRuntime.SendEvent(new NWTypesChildClass());
            Assert.AreEqual(1, listener.GetNewDataListFlattened().Length);
        }
    
        public void RunAssertionCreateSchemaModelAfter(EventRepresentationEnum eventRepresentationEnum)
        {
            _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema EventTypeOne (hsi int)");
            _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema EventTypeTwo (event EventTypeOne)");
            var stmt = _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create window NamedWindow.std:unique(event.hsi) as EventTypeTwo");
            _epService.EPAdministrator.CreateEPL("on EventTypeOne as ev insert into NamedWindow select ev as event");
    
            IDictionary<String, object> theEvent = new LinkedHashMap<string, object>();
            theEvent.Put("hsi", 10);
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                _epService.EPRuntime.SendEvent(theEvent.Values.ToArray(), "EventTypeOne");
            }
            else {
                _epService.EPRuntime.SendEvent(theEvent, "EventTypeOne");
            }
            var result = stmt.First();
            var getter = result.EventType.GetGetter("event.hsi");
            Assert.AreEqual(10, getter.Get(result));
    
            _epService.Initialize();
        }
    
        [Test]
        public void TestCreateTableArray()
        {
            _epService.EPAdministrator.CreateEPL("create schema SecurityData (name String, roles String[])");
            _epService.EPAdministrator.CreateEPL("create window SecurityEvent.win:time(30 sec) (ipAddress string, userId String, secData SecurityData, historySecData SecurityData[])");
    
            // create window
            var stmtTextCreate = "create window MyWindow.win:keepall() (myvalue string[])";
            var stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtCreate.AddListener(_listenerWindow);
    
            // create insert into
            var stmtTextInsertOne = "insert into MyWindow select {'a','b'} as myvalue from " + typeof(SupportBean).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
            
            SendSupportBean("E1", 1L, 10L);
            var values = (string[]) _listenerWindow.AssertOneGetNewAndReset().Get("myvalue");
            EPAssertionUtil.AssertEqualsExactOrder(values, new string[]{"a", "b"});
        }
    
        [Test]
        public void TestCreateTableSyntax()
        {
            // create window
            var stmtTextCreate = "create window MyWindow.win:keepall() (stringValOne varchar, stringValTwo string, intVal int, longVal long)";
            var stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtCreate.AddListener(_listenerWindow);
    
            // assert type metadata
            var type = (EventTypeSPI) ((EPServiceProviderSPI)_epService).EventAdapterService.GetEventTypeByName("MyWindow");
            Assert.AreEqual(null, type.Metadata.OptionalApplicationType);
            Assert.AreEqual(null, type.Metadata.OptionalSecondaryNames);
            Assert.AreEqual("MyWindow", type.Metadata.PrimaryName);
            Assert.AreEqual("MyWindow", type.Metadata.PublicName);
            Assert.AreEqual("MyWindow", type.Name);
            Assert.AreEqual(TypeClass.NAMED_WINDOW, type.Metadata.TypeClass);
            Assert.AreEqual(false, type.Metadata.IsApplicationConfigured);
            Assert.AreEqual(false, type.Metadata.IsApplicationPreConfigured);
            Assert.AreEqual(false, type.Metadata.IsApplicationPreConfiguredStatic);
    
            // create insert into
            var stmtTextInsertOne = "insert into MyWindow select TheString as stringValOne, TheString as stringValTwo, cast(LongPrimitive, int) as intVal, LongBoxed as longVal from " + typeof(SupportBean).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // create consumer
            var stmtTextSelectOne = "select stringValOne, stringValTwo, intVal, longVal from MyWindow";
            var stmtSelectOne = _epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            stmtSelectOne.AddListener(_listenerStmtOne);
    
            SendSupportBean("E1", 1L, 10L);
            var fields = "stringValOne,stringValTwo,intVal,longVal".Split(',');
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E1", "E1", 1, 10L});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E1", "E1", 1, 10L});
    
            // create window with two views
            stmtTextCreate = "create window MyWindowTwo.std:unique(stringValOne).win:keepall() (stringValOne varchar, stringValTwo string, intVal int, longVal long)";
            stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
    
            //create window with statement object model
            var text = "create window MyWindowThree.win:keepall() as (a string, b integer, c integer)";
            var model = _epService.EPAdministrator.CompileEPL(text);
            Assert.AreEqual(text, model.ToEPL());
            stmtCreate = _epService.EPAdministrator.Create(model);
            Assert.AreEqual(typeof(string), stmtCreate.EventType.GetPropertyType("a"));
            Assert.AreEqual(typeof(int), stmtCreate.EventType.GetPropertyType("b"));
            Assert.AreEqual(typeof(int), stmtCreate.EventType.GetPropertyType("c"));
            Assert.AreEqual(text, model.ToEPL());
    
            text = "create window MyWindowFour.std:unique(a).std:unique(b) retain-union as (a string, b integer, c integer)";
            model = _epService.EPAdministrator.CompileEPL(text);
            stmtCreate = _epService.EPAdministrator.Create(model);
            Assert.AreEqual(text, model.ToEPL());
        }
    
        [Test]
        public void TestWildcardNoFieldsNoAs()
        {
            // create window
            var stmtTextCreate = "create window MyWindow.win:keepall() select * from " + typeof(SupportBean_A).FullName;
            var stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtCreate.AddListener(_listenerWindow);
    
            // create insert into
            var stmtTextInsertOne = "insert into MyWindow select * from " + typeof(SupportBean_A).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // create consumer
            var stmtTextSelectOne = "select id from default.MyWindow";
            var stmtSelectOne = _epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            stmtSelectOne.AddListener(_listenerStmtOne);
    
            _epService.EPRuntime.SendEvent(new SupportBean_A("E1"));
            var fields = new string[] {"id"};
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E1"});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E1"});
        }
    
        [Test]
        public void TestModelAfterMap()
        {
            // create window
            var stmtTextCreate = "create window MyWindow.win:keepall() select * from MyMap";
            var stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
            Assert.IsTrue(stmtCreate.EventType is MapEventType);
    
            // create insert into
            var stmtTextInsertOne = "insert into MyWindow select * from MyMap";
            var stmt = _epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
            stmt.AddListener(_listenerWindow);
    
            SendMap("k1", 100L, 200L);
            var theEvent = _listenerWindow.AssertOneGetNewAndReset();
            Assert.IsTrue(theEvent is MappedEventBean);
            EPAssertionUtil.AssertProps(theEvent, "key,primitive".Split(','), new object[]{"k1", 100L});
        }
    
        [Test]
        public void TestWildcardInheritance()
        {
            // create window
            var stmtTextCreate = "create window MyWindow.win:keepall() as select * from " + typeof(SupportBeanBase).FullName;
            var stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtCreate.AddListener(_listenerWindow);
    
            // create insert into
            var stmtTextInsertOne = "insert into MyWindow select * from " + typeof(SupportBean_A).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // create insert into
            var stmtTextInsertTwo = "insert into MyWindow select * from " + typeof(SupportBean_B).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsertTwo);
    
            // create consumer
            var stmtTextSelectOne = "select id from MyWindow";
            var stmtSelectOne = _epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            stmtSelectOne.AddListener(_listenerStmtOne);
    
            _epService.EPRuntime.SendEvent(new SupportBean_A("E1"));
            var fields = new string[] {"id"};
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E1"});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E1"});
    
            _epService.EPRuntime.SendEvent(new SupportBean_B("E2"));
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E2"});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E2"});
        }
    
        [Test]
        public void TestNoSpecificationBean()
        {
            // create window
            var stmtTextCreate = "create window MyWindow.win:keepall() as " + typeof(SupportBean_A).FullName;
            var stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtCreate.AddListener(_listenerWindow);
    
            // create insert into
            var stmtTextInsertOne = "insert into MyWindow select * from " + typeof(SupportBean_A).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // create consumer
            var stmtTextSelectOne = "select id from MyWindow";
            var stmtSelectOne = _epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            stmtSelectOne.AddListener(_listenerStmtOne);
    
            _epService.EPRuntime.SendEvent(new SupportBean_A("E1"));
            var fields = new string[] {"id"};
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E1"});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E1"});
        }
    
        [Test]
        public void TestWildcardWithFields()
        {
            // create window
            var stmtTextCreate = "create window MyWindow.win:keepall() as select *, id as myid from " + typeof(SupportBean_A).FullName;
            var stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtCreate.AddListener(_listenerWindow);
    
            // create insert into
            var stmtTextInsertOne = "insert into MyWindow select *, id || 'A' as myid from " + typeof(SupportBean_A).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // create consumer
            var stmtTextSelectOne = "select id, myid from MyWindow";
            var stmtSelectOne = _epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            stmtSelectOne.AddListener(_listenerStmtOne);
    
            _epService.EPRuntime.SendEvent(new SupportBean_A("E1"));
            var fields = new string[] {"id", "myid"};
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E1", "E1A"});
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E1", "E1A"});
        }
    
        private SupportBean SendSupportBean(string theString, long longPrimitive, long? longBoxed)
        {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.LongPrimitive = longPrimitive;
            bean.LongBoxed = longBoxed;
            _epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    
        private void SendMarketBean(string symbol, long volume)
        {
            var bean = new SupportMarketDataBean(symbol, 0, volume, "");
            _epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendMap(string key, long primitive, long? boxed)
        {
            IDictionary<String, object> map = new Dictionary<string, object>();
            map.Put("key", key);
            map.Put("primitive", primitive);
            map.Put("boxed", boxed);
            _epService.EPRuntime.SendEvent(map, "MyMap");
        }
    
        public class NWTypesParentClass 
        {
        }
    
        public class NWTypesChildClass : NWTypesParentClass 
        {
        }
    }
}
