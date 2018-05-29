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
using Avro.Generic;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.events;
using com.espertech.esper.events.map;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;
using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;
using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.namedwindow
{
    public class ExecNamedWindowTypes : RegressionExecution {
        public override void Configure(Configuration configuration) {
            var types = new Dictionary<string, Object>();
            types.Put("key", typeof(string));
            types.Put("primitive", typeof(long));
            types.Put("boxed", typeof(long));
            configuration.AddEventType("MyMap", types);
        }
    
        public override void Run(EPServiceProvider epService) {
    
            foreach (EventRepresentationChoice rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                RunAssertionEventTypeColumnDef(epService, rep);
            }
    
            RunAssertionMapTranspose(epService);
            RunAssertionNoWildcardWithAs(epService);
            RunAssertionNoWildcardNoAs(epService);
            RunAssertionConstantsAs(epService);
            RunAssertionCreateSchemaModelAfter(epService);
            RunAssertionCreateTableArray(epService);
            RunAssertionCreateTableSyntax(epService);
            RunAssertionWildcardNoFieldsNoAs(epService);
            RunAssertionModelAfterMap(epService);
            RunAssertionWildcardInheritance(epService);
            RunAssertionNoSpecificationBean(epService);
            RunAssertionWildcardWithFields(epService);
        }
    
        private void RunAssertionEventTypeColumnDef(EPServiceProvider epService, EventRepresentationChoice eventRepresentationEnum) {
            EPStatement stmtSchema = epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema SchemaOne(col1 int, col2 int)");
            Assert.IsTrue(eventRepresentationEnum.MatchesClass(stmtSchema.EventType.UnderlyingType));
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create window SchemaWindow#lastevent as (s1 SchemaOne)");
            Assert.IsTrue(eventRepresentationEnum.MatchesClass(stmt.EventType.UnderlyingType));
            var listenerWindow = new SupportUpdateListener();
            stmt.Events += listenerWindow.Update;
            epService.EPAdministrator.CreateEPL("insert into SchemaWindow (s1) select sone from SchemaOne as sone");
    
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                epService.EPRuntime.SendEvent(new object[]{10, 11}, "SchemaOne");
            } else if (eventRepresentationEnum.IsMapEvent()) {
                var theEvent = new LinkedHashMap<string, object>();
                theEvent.Put("col1", 10);
                theEvent.Put("col2", 11);
                epService.EPRuntime.SendEvent(theEvent, "SchemaOne");
            } else if (eventRepresentationEnum.IsAvroEvent()) {
                var theEvent = new GenericRecord(SupportAvroUtil.GetAvroSchema(epService, "SchemaOne").AsRecordSchema());
                theEvent.Put("col1", 10);
                theEvent.Put("col2", 11);
                epService.EPRuntime.SendEventAvro(theEvent, "SchemaOne");
            } else {
                Assert.Fail();
            }
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), "s1.col1,s1.col2".Split(','), new object[]{10, 11});
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("SchemaOne", true);
            epService.EPAdministrator.Configuration.RemoveEventType("SchemaWindow", true);
        }
    
        private void RunAssertionMapTranspose(EPServiceProvider epService) {
            TryAssertionMapTranspose(epService, EventRepresentationChoice.ARRAY);
            TryAssertionMapTranspose(epService, EventRepresentationChoice.MAP);
            TryAssertionMapTranspose(epService, EventRepresentationChoice.DEFAULT);
        }
    
        private void TryAssertionMapTranspose(EPServiceProvider epService, EventRepresentationChoice eventRepresentationEnum) {
    
            var innerTypeOne = new Dictionary<string, object>();
            innerTypeOne.Put("i1", typeof(int));
            var innerTypeTwo = new Dictionary<string, object>();
            innerTypeTwo.Put("i2", typeof(int));
            var outerType = new Dictionary<string, object>();
            outerType.Put("one", "T1");
            outerType.Put("two", "T2");
            epService.EPAdministrator.Configuration.AddEventType("T1", innerTypeOne);
            epService.EPAdministrator.Configuration.AddEventType("T2", innerTypeTwo);
            epService.EPAdministrator.Configuration.AddEventType("OuterType", outerType);
    
            // create window
            string stmtTextCreate = eventRepresentationEnum.GetAnnotationText() + " create window MyWindowMT#keepall as select one, two from OuterType";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            Assert.IsTrue(eventRepresentationEnum.MatchesClass(stmtCreate.EventType.UnderlyingType));
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
            EPAssertionUtil.AssertEqualsAnyOrder(stmtCreate.EventType.PropertyNames, new string[]{"one", "two"});
            EventType eventType = stmtCreate.EventType;
            Assert.AreEqual("T1", eventType.GetFragmentType("one").FragmentType.Name);
            Assert.AreEqual("T2", eventType.GetFragmentType("two").FragmentType.Name);
    
            // create insert into
            string stmtTextInsertOne = "insert into MyWindowMT select one, two from OuterType";
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            var innerDataOne = new Dictionary<string, object>();
            innerDataOne.Put("i1", 1);
            var innerDataTwo = new Dictionary<string, object>();
            innerDataTwo.Put("i2", 2);
            var outerData = new Dictionary<string, object>();
            outerData.Put("one", innerDataOne);
            outerData.Put("two", innerDataTwo);
    
            epService.EPRuntime.SendEvent(outerData, "OuterType");
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), "one.i1,two.i2".Split(','), new object[]{1, 2});
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyWindowMT", true);
        }
    
        private void RunAssertionNoWildcardWithAs(EPServiceProvider epService) {
            // create window
            string stmtTextCreate = "create window MyWindowNW#keepall as select TheString as a, LongPrimitive as b, LongBoxed as c from " + typeof(SupportBean).FullName;
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
            EPAssertionUtil.AssertEqualsAnyOrder(stmtCreate.EventType.PropertyNames, new string[]{"a", "b", "c"});
            Assert.AreEqual(typeof(string), stmtCreate.EventType.GetPropertyType("a"));
            Assert.AreEqual(typeof(long), stmtCreate.EventType.GetPropertyType("b"));
            Assert.AreEqual(typeof(long?), stmtCreate.EventType.GetPropertyType("c"));
    
            // assert type metadata
            EventTypeSPI type = (EventTypeSPI) ((EPServiceProviderSPI) epService).EventAdapterService.GetEventTypeByName("MyWindowNW");
            Assert.AreEqual(null, type.Metadata.OptionalApplicationType);
            Assert.AreEqual(null, type.Metadata.OptionalSecondaryNames);
            Assert.AreEqual("MyWindowNW", type.Metadata.PrimaryName);
            Assert.AreEqual("MyWindowNW", type.Metadata.PublicName);
            Assert.AreEqual("MyWindowNW", type.Name);
            Assert.AreEqual(TypeClass.NAMED_WINDOW, type.Metadata.TypeClass);
            Assert.AreEqual(false, type.Metadata.IsApplicationConfigured);
            Assert.AreEqual(false, type.Metadata.IsApplicationPreConfigured);
            Assert.AreEqual(false, type.Metadata.IsApplicationPreConfiguredStatic);
    
            // create insert into
            string stmtTextInsertOne = "insert into MyWindowNW select TheString as a, LongPrimitive as b, LongBoxed as c from " + typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            string stmtTextInsertTwo = "insert into MyWindowNW select symbol as a, volume as b, volume as c from " + typeof(SupportMarketDataBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsertTwo);
    
            string stmtTextInsertThree = "insert into MyWindowNW select key as a, boxed as b, primitive as c from MyMap";
            epService.EPAdministrator.CreateEPL(stmtTextInsertThree);
    
            // create consumer
            string stmtTextSelectOne = "select a, b, c from MyWindowNW";
            EPStatement stmtSelectOne = epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            var listenerStmtOne = new SupportUpdateListener();
            stmtSelectOne.Events += listenerStmtOne.Update;
            EPAssertionUtil.AssertEqualsAnyOrder(stmtSelectOne.EventType.PropertyNames, new string[]{"a", "b", "c"});
            Assert.AreEqual(typeof(string), stmtCreate.EventType.GetPropertyType("a"));
            Assert.AreEqual(typeof(long), stmtCreate.EventType.GetPropertyType("b"));
            Assert.AreEqual(typeof(long?), stmtCreate.EventType.GetPropertyType("c"));
    
            // create delete stmt
            string stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + " as s0 delete from MyWindowNW as s1 where s0.symbol = s1.a";
            EPStatement stmtDelete = epService.EPAdministrator.CreateEPL(stmtTextDelete);
            var listenerStmtDelete = new SupportUpdateListener();
            stmtDelete.Events += listenerStmtDelete.Update;
    
            SendSupportBean(epService, "E1", 1L, 10L);
            var fields = new string[]{"a", "b", "c"};
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1L, 10L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1L, 10L});
    
            SendMarketBean(epService, "S1", 99L);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"S1", 99L, 99L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"S1", 99L, 99L});
    
            SendMap(epService, "M1", 100L, 101L);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"M1", 101L, 100L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"M1", 101L, 100L});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionNoWildcardNoAs(EPServiceProvider epService) {
            // create window
            string stmtTextCreate = "create window MyWindowNWNA#keepall as select TheString, LongPrimitive, LongBoxed from " + typeof(SupportBean).FullName;
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
    
            // create insert into
            string stmtTextInsertOne = "insert into MyWindowNWNA select TheString, LongPrimitive, LongBoxed from " + typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            string stmtTextInsertTwo = "insert into MyWindowNWNA select symbol as TheString, volume as LongPrimitive, volume as LongBoxed from " + typeof(SupportMarketDataBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsertTwo);
    
            string stmtTextInsertThree = "insert into MyWindowNWNA select key as TheString, boxed as LongPrimitive, primitive as LongBoxed from MyMap";
            epService.EPAdministrator.CreateEPL(stmtTextInsertThree);
    
            // create consumer
            string stmtTextSelectOne = "select TheString, LongPrimitive, LongBoxed from MyWindowNWNA";
            EPStatement stmtSelectOne = epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            var listenerStmtOne = new SupportUpdateListener();
            stmtSelectOne.Events += listenerStmtOne.Update;
    
            SendSupportBean(epService, "E1", 1L, 10L);
            var fields = new string[]{"TheString", "LongPrimitive", "LongBoxed"};
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1L, 10L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1L, 10L});
    
            SendMarketBean(epService, "S1", 99L);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"S1", 99L, 99L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"S1", 99L, 99L});
    
            SendMap(epService, "M1", 100L, 101L);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"M1", 101L, 100L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"M1", 101L, 100L});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionConstantsAs(EPServiceProvider epService) {
            // create window
            string stmtTextCreate = "create window MyWindowCA#keepall as select '' as TheString, 0L as LongPrimitive, 0L as LongBoxed from MyMap";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
    
            // create insert into
            string stmtTextInsertOne = "insert into MyWindowCA select TheString, LongPrimitive, LongBoxed from " + typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            string stmtTextInsertTwo = "insert into MyWindowCA select symbol as TheString, volume as LongPrimitive, volume as LongBoxed from " + typeof(SupportMarketDataBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsertTwo);
    
            // create consumer
            string stmtTextSelectOne = "select TheString, LongPrimitive, LongBoxed from MyWindowCA";
            EPStatement stmtSelectOne = epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            var listenerStmtOne = new SupportUpdateListener();
            stmtSelectOne.Events += listenerStmtOne.Update;
    
            SendSupportBean(epService, "E1", 1L, 10L);
            var fields = new string[]{"TheString", "LongPrimitive", "LongBoxed"};
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1L, 10L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1L, 10L});
    
            SendMarketBean(epService, "S1", 99L);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"S1", 99L, 99L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"S1", 99L, 99L});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionCreateSchemaModelAfter(EPServiceProvider epService) {
            foreach (EventRepresentationChoice rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                TryAssertionCreateSchemaModelAfter(epService, rep);
            }
    
            // test model-after for PONO with inheritance
            epService.EPAdministrator.CreateEPL("create window ParentWindow#keepall as select * from " + typeof(NWTypesParentClass).MaskTypeName());
            epService.EPAdministrator.CreateEPL("insert into ParentWindow select * from " + typeof(NWTypesParentClass).MaskTypeName());
            epService.EPAdministrator.CreateEPL("create window ChildWindow#keepall as select * from " + typeof(NWTypesChildClass).MaskTypeName());
            epService.EPAdministrator.CreateEPL("insert into ChildWindow select * from " + typeof(NWTypesChildClass).MaskTypeName());
    
            var listener = new SupportUpdateListener();
            string parentQuery = "@Name('Parent') select parent from ParentWindow as parent";
            epService.EPAdministrator.CreateEPL(parentQuery).Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new NWTypesChildClass());
            Assert.AreEqual(1, listener.GetNewDataListFlattened().Length);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryAssertionCreateSchemaModelAfter(EPServiceProvider epService, EventRepresentationChoice eventRepresentationEnum) {
            epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema EventTypeOne (hsi int)");
            epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema EventTypeTwo (event EventTypeOne)");
            EPStatement stmt = epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create window NamedWindow#unique(event.hsi) as EventTypeTwo");
            epService.EPAdministrator.CreateEPL("on EventTypeOne as ev insert into NamedWindow select ev as event");
    
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                epService.EPRuntime.SendEvent(new object[]{10}, "EventTypeOne");
            } else if (eventRepresentationEnum.IsMapEvent()) {
                epService.EPRuntime.SendEvent(Collections.SingletonDataMap("hsi", 10), "EventTypeOne");
            } else if (eventRepresentationEnum.IsAvroEvent()) {
                var theEvent = new GenericRecord(SupportAvroUtil.GetAvroSchema(epService, "EventTypeOne").AsRecordSchema());
                theEvent.Put("hsi", 10);
                epService.EPRuntime.SendEventAvro(theEvent, "EventTypeOne");
            } else {
                Assert.Fail();
            }
            EventBean result = stmt.First();
            EventPropertyGetter getter = result.EventType.GetGetter("event.hsi");
            Assert.AreEqual(10, getter.Get(result));
    
            epService.EPAdministrator.DestroyAllStatements();
            foreach (string name in "EventTypeOne,EventTypeTwo,NamedWindow".Split(',')) {
                epService.EPAdministrator.Configuration.RemoveEventType(name, true);
            }
        }
    
        private void RunAssertionCreateTableArray(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create schema SecurityData (name string, roles string[])");
            epService.EPAdministrator.CreateEPL("create window SecurityEvent#time(30 sec) (ipAddress string, userId string, secData SecurityData, historySecData SecurityData[])");
    
            // create window
            string stmtTextCreate = "create window MyWindowCTA#keepall (myvalue string[])";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
    
            // create insert into
            string stmtTextInsertOne = "insert into MyWindowCTA select {'a','b'} as myvalue from " + typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            SendSupportBean(epService, "E1", 1L, 10L);
            string[] values = (string[]) listenerWindow.AssertOneGetNewAndReset().Get("myvalue");
            EPAssertionUtil.AssertEqualsExactOrder(values, new string[]{"a", "b"});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionCreateTableSyntax(EPServiceProvider epService) {
            // create window
            string stmtTextCreate = "create window MyWindowCTS#keepall (stringValOne varchar, stringValTwo string, intVal int, longVal long)";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
    
            // assert type metadata
            EventTypeSPI type = (EventTypeSPI) ((EPServiceProviderSPI) epService).EventAdapterService.GetEventTypeByName("MyWindowCTS");
            Assert.AreEqual(null, type.Metadata.OptionalApplicationType);
            Assert.AreEqual(null, type.Metadata.OptionalSecondaryNames);
            Assert.AreEqual("MyWindowCTS", type.Metadata.PrimaryName);
            Assert.AreEqual("MyWindowCTS", type.Metadata.PublicName);
            Assert.AreEqual("MyWindowCTS", type.Name);
            Assert.AreEqual(TypeClass.NAMED_WINDOW, type.Metadata.TypeClass);
            Assert.AreEqual(false, type.Metadata.IsApplicationConfigured);
            Assert.AreEqual(false, type.Metadata.IsApplicationPreConfigured);
            Assert.AreEqual(false, type.Metadata.IsApplicationPreConfiguredStatic);
    
            // create insert into
            string stmtTextInsertOne = "insert into MyWindowCTS select TheString as stringValOne, TheString as stringValTwo, Cast(LongPrimitive, int) as intVal, LongBoxed as longVal from " + typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // create consumer
            string stmtTextSelectOne = "select stringValOne, stringValTwo, intVal, longVal from MyWindowCTS";
            EPStatement stmtSelectOne = epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            var listenerStmtOne = new SupportUpdateListener();
            stmtSelectOne.Events += listenerStmtOne.Update;
    
            SendSupportBean(epService, "E1", 1L, 10L);
            string[] fields = "stringValOne,stringValTwo,intVal,longVal".Split(',');
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E1", "E1", 1, 10L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E1", "E1", 1, 10L});
    
            // create window with two views
            stmtTextCreate = "create window MyWindowCTSTwo#unique(stringValOne)#keepall (stringValOne varchar, stringValTwo string, intVal int, longVal long)";
            epService.EPAdministrator.CreateEPL(stmtTextCreate);
    
            //create window with statement object model
            string text = "create window MyWindowCTSThree#keepall as (a string, b integer, c integer)";
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(text);
            Assert.AreEqual(text, model.ToEPL());
            stmtCreate = epService.EPAdministrator.Create(model);
            Assert.AreEqual(typeof(string), stmtCreate.EventType.GetPropertyType("a"));
            Assert.AreEqual(typeof(int), stmtCreate.EventType.GetPropertyType("b"));
            Assert.AreEqual(typeof(int), stmtCreate.EventType.GetPropertyType("c"));
            Assert.AreEqual(text, model.ToEPL());
    
            text = "create window MyWindowCTSFour#unique(a)#unique(b) retain-union as (a string, b integer, c integer)";
            model = epService.EPAdministrator.CompileEPL(text);
            epService.EPAdministrator.Create(model);
            Assert.AreEqual(text, model.ToEPL());
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionWildcardNoFieldsNoAs(EPServiceProvider epService) {
            // create window
            string stmtTextCreate = "create window MyWindowWNF#keepall select * from " + typeof(SupportBean_A).FullName;
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
    
            // create insert into
            string stmtTextInsertOne = "insert into MyWindowWNF select * from " + typeof(SupportBean_A).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // create consumer
            string stmtTextSelectOne = "select id from default.MyWindowWNF";
            EPStatement stmtSelectOne = epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            var listenerStmtOne = new SupportUpdateListener();
            stmtSelectOne.Events += listenerStmtOne.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_A("E1"));
            var fields = new string[]{"id"};
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E1"});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E1"});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionModelAfterMap(EPServiceProvider epService) {
            // create window
            string stmtTextCreate = "create window MyWindowMAM#keepall select * from MyMap";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            Assert.IsTrue(stmtCreate.EventType is MapEventType);
    
            // create insert into
            string stmtTextInsertOne = "insert into MyWindowMAM select * from MyMap";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
            var listenerWindow = new SupportUpdateListener();
            stmt.Events += listenerWindow.Update;
    
            SendMap(epService, "k1", 100L, 200L);
            EventBean theEvent = listenerWindow.AssertOneGetNewAndReset();
            Assert.IsTrue(theEvent is MappedEventBean);
            EPAssertionUtil.AssertProps(theEvent, "key,primitive".Split(','), new object[]{"k1", 100L});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionWildcardInheritance(EPServiceProvider epService) {
            // create window
            string stmtTextCreate = "create window MyWindowWI#keepall as select * from " + typeof(SupportBeanBase).MaskTypeName();
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
    
            // create insert into
            string stmtTextInsertOne = "insert into MyWindowWI select * from " + typeof(SupportBean_A).MaskTypeName();
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // create insert into
            string stmtTextInsertTwo = "insert into MyWindowWI select * from " + typeof(SupportBean_B).MaskTypeName();
            epService.EPAdministrator.CreateEPL(stmtTextInsertTwo);
    
            // create consumer
            string stmtTextSelectOne = "select id from MyWindowWI";
            EPStatement stmtSelectOne = epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            var listenerStmtOne = new SupportUpdateListener();
            stmtSelectOne.Events += listenerStmtOne.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_A("E1"));
            var fields = new string[]{"id"};
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E1"});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E1"});
    
            epService.EPRuntime.SendEvent(new SupportBean_B("E2"));
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E2"});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E2"});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionNoSpecificationBean(EPServiceProvider epService) {
            // create window
            string stmtTextCreate = "create window MyWindowNSB#keepall as " + typeof(SupportBean_A).FullName;
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
    
            // create insert into
            string stmtTextInsertOne = "insert into MyWindowNSB select * from " + typeof(SupportBean_A).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // create consumer
            string stmtTextSelectOne = "select id from MyWindowNSB";
            EPStatement stmtSelectOne = epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            var listenerStmtOne = new SupportUpdateListener();
            stmtSelectOne.Events += listenerStmtOne.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_A("E1"));
            var fields = new string[]{"id"};
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E1"});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E1"});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionWildcardWithFields(EPServiceProvider epService) {
            // create window
            string stmtTextCreate = "create window MyWindowWWF#keepall as select *, id as myid from " + typeof(SupportBean_A).FullName;
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
    
            // create insert into
            string stmtTextInsertOne = "insert into MyWindowWWF select *, id || 'A' as myid from " + typeof(SupportBean_A).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // create consumer
            string stmtTextSelectOne = "select id, myid from MyWindowWWF";
            EPStatement stmtSelectOne = epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            var listenerStmtOne = new SupportUpdateListener();
            stmtSelectOne.Events += listenerStmtOne.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_A("E1"));
            var fields = new string[]{"id", "myid"};
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E1", "E1A"});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E1", "E1A"});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void SendSupportBean(EPServiceProvider epService, string theString, long longPrimitive, long longBoxed) {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.LongPrimitive = longPrimitive;
            bean.LongBoxed = longBoxed;
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendMarketBean(EPServiceProvider epService, string symbol, long volume) {
            var bean = new SupportMarketDataBean(symbol, 0, volume, "");
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendMap(EPServiceProvider epService, string key, long primitive, long boxed) {
            var map = new Dictionary<string, Object>();
            map.Put("key", key);
            map.Put("primitive", primitive);
            map.Put("boxed", boxed);
            epService.EPRuntime.SendEvent(map, "MyMap");
        }
    
        public class NWTypesParentClass {
        }
    
        public class NWTypesChildClass : NWTypesParentClass{
        }
    }
} // end of namespace
