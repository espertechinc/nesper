///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Xml;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestUpdate 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportBean>();
            config.EngineDefaults.ExecutionConfig.IsPrioritized = true;
    
            var legacy = new ConfigurationEventTypeLegacy();
            legacy.CopyMethod = "myCopyMethod";
            config.AddEventType("SupportBeanCopyMethod", typeof(SupportBeanCopyMethod).FullName, legacy);
    
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _listener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }
    
        [Test]
        public void TestFieldUpdateOrder() {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddVariable("myvar", typeof(int), 10);
    
            _epService.EPAdministrator.CreateEPL("update istream SupportBean " +
                    "set IntPrimitive=myvar, IntBoxed=IntPrimitive");
            var stmt = _epService.EPAdministrator.CreateEPL("select * from SupportBean");
            stmt.Events += _listener.Update;
            var fields = "IntPrimitive,IntBoxed".Split(',');
    
            _epService.EPRuntime.SendEvent(MakeSupportBean("E1", 1, 2));
            EPAssertionUtil.AssertProps(_listener.GetAndResetLastNewData()[0], fields, new Object[]{10, 1});
        }
    
        [Test]
        public void TestInvalid()
        {
            IDictionary<String, Object> type = new Dictionary<String, Object>();
            type.Put("p0", typeof(long));
            type.Put("p1", typeof(long));
            type.Put("p2", typeof(long));
            type.Put("p3", typeof(String));
            _epService.EPAdministrator.Configuration.AddEventType("MyMapType", type);
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddEventType("SupportBeanReadOnly", typeof(SupportBeanReadOnly));
            _epService.EPAdministrator.Configuration.AddEventType("SupportBeanErrorTestingOne", typeof(SupportBeanErrorTestingOne));
    
            var configXML = new ConfigurationEventTypeXMLDOM();
            configXML.RootElementName = "MyXMLEvent";
            _epService.EPAdministrator.Configuration.AddEventType("MyXmlEvent", configXML);
    
            _epService.EPAdministrator.CreateEPL("insert into SupportBeanStream select * from SupportBean");
            _epService.EPAdministrator.CreateEPL("insert into SupportBeanStreamTwo select * from pattern[a=SupportBean -> b=SupportBean]");
            _epService.EPAdministrator.CreateEPL("insert into SupportBeanStreamRO select * from SupportBeanReadOnly");
    
            TryInvalid("update istream SupportBeanStream set IntPrimitive=LongPrimitive",
                       "Error starting statement: Invalid assignment of column 'LongPrimitive' of type '" + Name.Of<long>() + "' to event property 'IntPrimitive' typed as '" + Name.Of<int>(false) + "', column and parameter types mismatch [update istream SupportBeanStream set IntPrimitive=LongPrimitive]");
            TryInvalid("update istream SupportBeanStream set xxx='abc'",
                       "Error starting statement: Property 'xxx' is not available for write access [update istream SupportBeanStream set xxx='abc']");
            TryInvalid("update istream SupportBeanStream set IntPrimitive=null",
                       "Error starting statement: Invalid assignment of column 'null' of null type to event property 'IntPrimitive' typed as '" + Name.Of<int>(false) + "', nullable type mismatch [update istream SupportBeanStream set IntPrimitive=null]");
            TryInvalid("update istream SupportBeanStreamTwo set a.IntPrimitive=10",
                       "Error starting statement: Property 'a.IntPrimitive' is not available for write access [update istream SupportBeanStreamTwo set a.IntPrimitive=10]");
            TryInvalid("update istream SupportBeanStreamRO set side='a'",
                       "Error starting statement: Property 'side' is not available for write access [update istream SupportBeanStreamRO set side='a']");
            TryInvalid("update istream SupportBean set LongPrimitive=sum(IntPrimitive)",
                       "Error starting statement: Aggregation functions may not be used within an update-clause [update istream SupportBean set LongPrimitive=sum(IntPrimitive)]");
            TryInvalid("update istream SupportBean set LongPrimitive=LongPrimitive where Sum(IntPrimitive) = 1",
                       "Error starting statement: Aggregation functions may not be used within an update-clause [update istream SupportBean set LongPrimitive=LongPrimitive where Sum(IntPrimitive) = 1]");
            TryInvalid("update istream SupportBean set LongPrimitive=prev(1, LongPrimitive)",
                       "Error starting statement: Previous function cannot be used in this context [update istream SupportBean set LongPrimitive=prev(1, LongPrimitive)]");
            TryInvalid("update istream MyXmlEvent set abc=1",
                       "Error starting statement: Property 'abc' is not available for write access [update istream MyXmlEvent set abc=1]");
            TryInvalid("update istream SupportBeanErrorTestingOne set Value='1'",
                       "Error starting statement: The update-clause requires the underlying event representation to support copy (via Serializable by default) [update istream SupportBeanErrorTestingOne set Value='1']");
            TryInvalid("update istream SupportBean set LongPrimitive=(select p0 from MyMapType.std:lastevent() where TheString=p3)",
                       "Error starting statement: Failed to plan subquery number 1 querying MyMapType: Failed to validate filter expression 'TheString=p3': Property named 'TheString' must be prefixed by a stream name, use the stream name itself or use the as-clause to name the stream with the property in the format \"stream.property\" [update istream SupportBean set LongPrimitive=(select p0 from MyMapType.std:lastevent() where TheString=p3)]");
            TryInvalid("update istream XYZ.GYH set a=1",
                       "Failed to resolve event type: Event type or class named 'XYZ.GYH' was not found [update istream XYZ.GYH set a=1]");
            TryInvalid("update istream SupportBean set 1",
                        "Error starting statement: Missing property assignment expression in assignment number 0 [update istream SupportBean set 1]");
        }
    
        [Test]
        public void TestInsertIntoWBeanWhere()
        {
            var listenerInsert = new SupportUpdateListener() { Tag = "insert" };
            var stmtInsert = _epService.EPAdministrator.CreateEPL("insert into MyStream select * from SupportBean");
            stmtInsert.Events += listenerInsert.Update;

            var listenerUpdate = new SupportUpdateListener() { Tag = "update" };
            var stmtUpdOne = _epService.EPAdministrator.CreateEPL("update istream MyStream set IntPrimitive=10, TheString='O_' || TheString where IntPrimitive=1");
            stmtUpdOne.Events += listenerUpdate.Update;
    
            var stmtSelect = _epService.EPAdministrator.CreateEPL("select * from MyStream");
            stmtSelect.Events += _listener.Update;
    
            var fields = "TheString,IntPrimitive".Split(',');
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 9));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"E1", 9});
            EPAssertionUtil.AssertProps(listenerInsert.AssertOneGetNewAndReset(), fields, new Object[]{"E1", 9});
            Assert.IsFalse(listenerUpdate.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"O_E2", 10});
            EPAssertionUtil.AssertProps(listenerInsert.AssertOneGetNewAndReset(), fields, new Object[]{"E2", 1});
            EPAssertionUtil.AssertProps(listenerUpdate.AssertOneGetOld(), fields, new Object[]{"E2", 1});
            EPAssertionUtil.AssertProps(listenerUpdate.AssertOneGetNew(), fields, new Object[]{"O_E2", 10});
            listenerUpdate.Reset();
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 2));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"E3", 2});
            EPAssertionUtil.AssertProps(listenerInsert.AssertOneGetNewAndReset(), fields, new Object[]{"E3", 2});
            Assert.IsFalse(listenerUpdate.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"O_E4", 10});
            EPAssertionUtil.AssertProps(listenerInsert.AssertOneGetNewAndReset(), fields, new Object[]{"E4", 1});
            EPAssertionUtil.AssertProps(listenerUpdate.AssertOneGetOld(), fields, new Object[]{"E4", 1});
            EPAssertionUtil.AssertProps(listenerUpdate.AssertOneGetNew(), fields, new Object[]{"O_E4", 10});
            listenerUpdate.Reset();
    
            var stmtUpdTwo = _epService.EPAdministrator.CreateEPL("update istream MyStream as xyz set IntPrimitive=xyz.IntPrimitive + 1000 where IntPrimitive=2");
            stmtUpdTwo.Events += listenerUpdate.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E5", 2));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"E5", 1002});
            EPAssertionUtil.AssertProps(listenerInsert.AssertOneGetNewAndReset(), fields, new Object[]{"E5", 2});
            EPAssertionUtil.AssertProps(listenerUpdate.AssertOneGetOld(), fields, new Object[]{"E5", 2});
            EPAssertionUtil.AssertProps(listenerUpdate.AssertOneGetNew(), fields, new Object[]{"E5", 1002});
            listenerUpdate.Reset();
    
            stmtUpdOne.Dispose();
    
            _epService.EPRuntime.SendEvent(new SupportBean("E6", 1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"E6", 1});
            EPAssertionUtil.AssertProps(listenerInsert.AssertOneGetNewAndReset(), fields, new Object[]{"E6", 1});
            Assert.IsFalse(listenerUpdate.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E7", 2));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"E7", 1002});
            EPAssertionUtil.AssertProps(listenerInsert.AssertOneGetNewAndReset(), fields, new Object[]{"E7", 2});
            EPAssertionUtil.AssertProps(listenerUpdate.AssertOneGetOld(), fields, new Object[]{"E7", 2});
            EPAssertionUtil.AssertProps(listenerUpdate.AssertOneGetNew(), fields, new Object[]{"E7", 1002});
            listenerUpdate.Reset();
            Assert.IsFalse(stmtUpdTwo.GetEnumerator().MoveNext());
    
            stmtUpdTwo.RemoveAllEventHandlers();
    
            _epService.EPRuntime.SendEvent(new SupportBean("E8", 2));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"E8", 1002});
            EPAssertionUtil.AssertProps(listenerInsert.AssertOneGetNewAndReset(), fields, new Object[]{"E8", 2});
            Assert.IsFalse(listenerUpdate.IsInvoked);
    
            var subscriber = new SupportSubscriber();
            stmtUpdTwo.Subscriber = subscriber;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E9", 2));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"E9", 1002});
            EPAssertionUtil.AssertProps(listenerInsert.AssertOneGetNewAndReset(), fields, new Object[]{"E9", 2});
            EPAssertionUtil.AssertPropsPONO(subscriber.GetOldDataListFlattened()[0], fields, new Object[]{"E9", 2});
            EPAssertionUtil.AssertPropsPONO(subscriber.GetNewDataListFlattened()[0], fields, new Object[]{"E9", 1002});
            subscriber.Reset();
    
            stmtUpdTwo.Dispose();
    
            _epService.EPRuntime.SendEvent(new SupportBean("E10", 2));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"E10", 2});
            EPAssertionUtil.AssertProps(listenerInsert.AssertOneGetNewAndReset(), fields, new Object[]{"E10", 2});
            Assert.IsFalse(listenerUpdate.IsInvoked);
    
            var stmtUpdThree = _epService.EPAdministrator.CreateEPL("update istream MyStream set IntPrimitive=IntBoxed");
            stmtUpdThree.Events += listenerUpdate.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E11", 2));
            EPAssertionUtil.AssertProps(listenerUpdate.AssertOneGetNew(), fields, new Object[]{"E11", 2});
            listenerUpdate.Reset();
        }
    
        [Test]
        public void TestInsertIntoWMapNoWhere()
        {
            IDictionary<String, Object> type = new Dictionary<String, Object>();
            type.Put("p0", typeof(long));
            type.Put("p1", typeof(long));
            type.Put("p2", typeof(long));
            _epService.EPAdministrator.Configuration.AddEventType("MyMapType", type);
    
            var listenerInsert = new SupportUpdateListener();
            var stmtInsert = _epService.EPAdministrator.CreateEPL("insert into MyStream select * from MyMapType");
            stmtInsert.Events += listenerInsert.Update;
    
            var stmtUpd = _epService.EPAdministrator.CreateEPL("update istream MyStream set p0=p1, p1=p0");
    
            var stmtSelect = _epService.EPAdministrator.CreateEPL("select * from MyStream");
            stmtSelect.Events += _listener.Update;
    
            var fields = "p0,p1,p2".Split(',');
            _epService.EPRuntime.SendEvent(MakeMap("p0", 10, "p1", 1, "p2", 100), "MyMapType");
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{1, 10, 100});
            EPAssertionUtil.AssertProps(listenerInsert.AssertOneGetNewAndReset(), fields, new Object[]{10, 1, 100});
    
            stmtUpd.Stop();
            stmtUpd.Start();
            
            _epService.EPRuntime.SendEvent(MakeMap("p0", 5, "p1", 4, "p2", 101), "MyMapType");
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{4, 5, 101});
            EPAssertionUtil.AssertProps(listenerInsert.AssertOneGetNewAndReset(), fields, new Object[]{5, 4, 101});
    
            stmtUpd.Dispose();
    
            _epService.EPRuntime.SendEvent(MakeMap("p0", 20, "p1", 0, "p2", 102), "MyMapType");
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{20, 0, 102});
            EPAssertionUtil.AssertProps(listenerInsert.AssertOneGetNewAndReset(), fields, new Object[]{20, 0, 102});
        }
    
        [Test]
        public void TestFieldsWithPriority() {
            RunAssertionFieldsWithPriority(EventRepresentationEnum.OBJECTARRAY);
            RunAssertionFieldsWithPriority(EventRepresentationEnum.MAP);
            RunAssertionFieldsWithPriority(EventRepresentationEnum.DEFAULT);
        }
    
        private void RunAssertionFieldsWithPriority(EventRepresentationEnum eventRepresentationEnum)
        {
            _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " insert into MyStream select TheString, IntPrimitive from SupportBean(TheString not like 'Z%')");
            _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " insert into MyStream select 'AX'||TheString as TheString, IntPrimitive from SupportBean(TheString like 'Z%')");
            _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " @Name('a') @Priority(12) update istream MyStream set IntPrimitive=-2 where IntPrimitive=-1");
            _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " @Name('b') @Priority(11) update istream MyStream set IntPrimitive=-1 where TheString like 'D%'");
            _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " @Name('c') @Priority(9) update istream MyStream set IntPrimitive=9 where TheString like 'A%'");
            _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " @Name('d') @Priority(8) update istream MyStream set IntPrimitive=8 where TheString like 'A%' or TheString like 'C%'");
            _epService.EPAdministrator.CreateEPL(" @Name('e') @Priority(10) update istream MyStream set IntPrimitive=10 where TheString like 'A%'");
            _epService.EPAdministrator.CreateEPL(" @Name('f') @Priority(7) update istream MyStream set IntPrimitive=7 where TheString like 'A%' or TheString like 'C%'");
            _epService.EPAdministrator.CreateEPL(" @Name('g') @Priority(6) update istream MyStream set IntPrimitive=6 where TheString like 'A%'");
            _epService.EPAdministrator.CreateEPL(" @Name('h') @Drop update istream MyStream set IntPrimitive=6 where TheString like 'B%'");
    
            var stmtSelect = _epService.EPAdministrator.CreateEPL("select * from MyStream where IntPrimitive > 0");
            stmtSelect.Events += _listener.Update;
    
            var fields = "TheString,IntPrimitive".Split(',');
            _epService.EPRuntime.SendEvent(new SupportBean("A1", 0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"A1", 10});
    
            _epService.EPRuntime.SendEvent(new SupportBean("B1", 0));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("C1", 0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"C1", 8});
    
            _epService.EPRuntime.SendEvent(new SupportBean("D1", 100));
            Assert.IsFalse(_listener.IsInvoked);
    
            stmtSelect.Stop();
            stmtSelect = _epService.EPAdministrator.CreateEPL("select * from MyStream");
            stmtSelect.Events += _listener.Update;
            Assert.AreEqual(eventRepresentationEnum.GetOutputClass(), stmtSelect.EventType.UnderlyingType);
    
            _epService.EPRuntime.SendEvent(new SupportBean("D1", -2));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"D1", -2});
    
            _epService.EPRuntime.SendEvent(new SupportBean("Z1", -3));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"AXZ1", 10});
    
            _epService.EPAdministrator.GetStatement("e").Stop();
            _epService.EPRuntime.SendEvent(new SupportBean("Z2", 0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"AXZ2", 9});
    
            _epService.EPAdministrator.GetStatement("c").Stop();
            _epService.EPAdministrator.GetStatement("d").Stop();
            _epService.EPAdministrator.GetStatement("f").Stop();
            _epService.EPAdministrator.GetStatement("g").Stop();
            _epService.EPRuntime.SendEvent(new SupportBean("Z3", 0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"AXZ3", 0});
    
            _epService.Initialize();
        }
    
        [Test]
        public void TestInsertDirectBeanTypeInheritance()
        {
            IDictionary<String, Object> type = new Dictionary<String, Object>();
            type.Put("p0", typeof(String));
            type.Put("p1", typeof(String));
            _epService.EPAdministrator.Configuration.AddEventType("MyMapType", type);
            _epService.EPAdministrator.Configuration.AddEventType("BaseInterface", typeof(BaseInterface));
            _epService.EPAdministrator.Configuration.AddEventType("BaseOne", typeof(BaseOne));
            _epService.EPAdministrator.Configuration.AddEventType("BaseOneA", typeof(BaseOneA));
            _epService.EPAdministrator.Configuration.AddEventType("BaseOneB", typeof(BaseOneB));
            _epService.EPAdministrator.Configuration.AddEventType("BaseTwo", typeof(BaseTwo));
    
            // test update applies to child types via interface
            var stmtInsert = _epService.EPAdministrator.CreateEPL("insert into BaseOne select p0 as I, p1 as P from MyMapType");
            _epService.EPAdministrator.CreateEPL("@Name('a') update istream BaseInterface set I='XYZ' where I like 'E%'");
            var stmtSelect = _epService.EPAdministrator.CreateEPL("select * from BaseOne");
            stmtSelect.Events += _listener.Update;
    
            var fields = "i,p".Split(',');
            _epService.EPRuntime.SendEvent(MakeMap("p0", "E1", "p1", "E1"), "MyMapType");
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"XYZ", "E1"});
    
            _epService.EPRuntime.SendEvent(MakeMap("p0", "F1", "p1", "E2"), "MyMapType");
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"F1", "E2"});
    
            _epService.EPAdministrator.CreateEPL("@Priority(2) @Name('b') update istream BaseOne set I='BLANK'");
    
            _epService.EPRuntime.SendEvent(MakeMap("p0", "somevalue", "p1", "E3"), "MyMapType");
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"BLANK", "E3"});

            _epService.EPAdministrator.CreateEPL("@Priority(3) @Name('c') update istream BaseOneA set I='FINAL'");
    
            _epService.EPRuntime.SendEvent(MakeMap("p0", "somevalue", "p1", "E4"), "MyMapType");
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"BLANK", "E4"});
    
            stmtInsert.Stop();
            stmtInsert = _epService.EPAdministrator.CreateEPL("insert into BaseOneA select p0 as I, p1 as P, 'a' as Pa from MyMapType");
    
            _epService.EPRuntime.SendEvent(MakeMap("p0", "somevalue", "p1", "E5"), "MyMapType");
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"FINAL", "E5"});
    
            stmtInsert.Stop();
            stmtInsert = _epService.EPAdministrator.CreateEPL("insert into BaseOneB select p0 as I, p1 as P, 'b' as Pb from MyMapType");
    
            _epService.EPRuntime.SendEvent(MakeMap("p0", "somevalue", "p1", "E6"), "MyMapType");
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"BLANK", "E6"});
    
            stmtInsert.Stop();
            stmtInsert = _epService.EPAdministrator.CreateEPL("insert into BaseTwo select p0 as I, p1 as P from MyMapType");
    
            stmtSelect.Stop();
            stmtSelect = _epService.EPAdministrator.CreateEPL("select * from BaseInterface");
            stmtSelect.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(MakeMap("p0", "E2", "p1", "E7"), "MyMapType");
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), new String[]{"i"}, new Object[]{"XYZ"});
        }
    
        [Test]
        public void TestNamedWindow()
        {
            IDictionary<String, Object> type = new Dictionary<string, object>();
            type.Put("p0", typeof(String));
            type.Put("p1", typeof(String));
            _epService.EPAdministrator.Configuration.AddEventType("MyMapType", type);
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            var fields = "p0,p1".Split(',');
            var listenerWindow = new SupportUpdateListener();
            var listenerInsert = new SupportUpdateListener();
            var listenerOnSelect = new SupportUpdateListener();
            var listenerInsertOnSelect = new SupportUpdateListener();
            var listenerWindowSelect = new SupportUpdateListener();
    
            _epService.EPAdministrator.CreateEPL("create window AWindow.win:keepall() select * from MyMapType").Events += listenerWindow.Update;
            _epService.EPAdministrator.CreateEPL("insert into AWindow select * from MyMapType").Events += listenerInsert.Update;
            _epService.EPAdministrator.CreateEPL("select * from AWindow").Events += listenerWindowSelect.Update;
            _epService.EPAdministrator.CreateEPL("update istream AWindow set p1='newvalue'");
    
            _epService.EPRuntime.SendEvent(MakeMap("p0", "E1", "p1", "oldvalue"), "MyMapType");
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new Object[]{"E1", "newvalue"});
            EPAssertionUtil.AssertProps(listenerInsert.AssertOneGetNewAndReset(), fields, new Object[]{"E1", "oldvalue"});
            EPAssertionUtil.AssertProps(listenerWindowSelect.AssertOneGetNewAndReset(), fields, new Object[]{"E1", "newvalue"});
    
            _epService.EPAdministrator.CreateEPL("on SupportBean(TheString='A') select win.* from AWindow as win").Events += listenerOnSelect.Update;
            _epService.EPRuntime.SendEvent(new SupportBean("A", 0));
            EPAssertionUtil.AssertProps(listenerOnSelect.AssertOneGetNewAndReset(), fields, new Object[]{"E1", "newvalue"});
    
            _epService.EPAdministrator.CreateEPL("on SupportBean(TheString='B') insert into MyOtherStream select win.* from AWindow as win").Events += listenerOnSelect.Update;
            _epService.EPRuntime.SendEvent(new SupportBean("B", 1));
            EPAssertionUtil.AssertProps(listenerOnSelect.AssertOneGetNewAndReset(), fields, new Object[]{"E1", "newvalue"});
    
            _epService.EPAdministrator.CreateEPL("update istream MyOtherStream set p0='a', p1='b'");
            _epService.EPAdministrator.CreateEPL("select * from MyOtherStream").Events += listenerInsertOnSelect.Update;
            _epService.EPRuntime.SendEvent(new SupportBean("B", 1));
            EPAssertionUtil.AssertProps(listenerOnSelect.AssertOneGetNewAndReset(), fields, new Object[]{"E1", "newvalue"});
            EPAssertionUtil.AssertProps(listenerInsertOnSelect.AssertOneGetNewAndReset(), fields, new Object[]{"a", "b"});
        }
    
        [Test]
        public void TestTypeWidener()
        {
            var fields = "TheString,LongBoxed,IntBoxed".Split(',');
            _epService.EPAdministrator.CreateEPL("insert into AStream select * from SupportBean");
            _epService.EPAdministrator.CreateEPL("update istream AStream set LongBoxed=IntBoxed, IntBoxed=null");
            _epService.EPAdministrator.CreateEPL("select * from AStream").Events += _listener.Update;
    
            var bean = new SupportBean("E1", 0);
            bean.LongBoxed = 888L;
            bean.IntBoxed = 999;
            _epService.EPRuntime.SendEvent(bean);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"E1", 999L, null});
        }
    
        [Test]
        public void TestSendRouteSenderPreprocess()
        {
            IDictionary<String, Object> type = new Dictionary<string, object>();
            type.Put("p0", typeof(String));
            type.Put("p1", typeof(String));
            _epService.EPAdministrator.Configuration.AddEventType("MyMapType", type);
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            // test map
            var stmtSelect = _epService.EPAdministrator.CreateEPL("select * from MyMapType");
            stmtSelect.Events += _listener.Update;
            _epService.EPAdministrator.CreateEPL("update istream MyMapType set p0='a'");
    
            var fields = "p0,p1".Split(',');
            _epService.EPRuntime.SendEvent(MakeMap("p0", "E1", "p1", "E1"), "MyMapType");
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"a", "E1"});
    
            var sender = _epService.EPRuntime.GetEventSender("MyMapType");
            sender.SendEvent(MakeMap("p0", "E2", "p1", "E2"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"a", "E2"});
    
            var stmtTrigger = _epService.EPAdministrator.CreateEPL("select * from SupportBean");
            stmtTrigger.Events += (s, args) => _epService.EPRuntime.Route(MakeMap("p0", "E3", "p1", "E3"), "MyMapType");
            _epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"a", "E3"});
            
            var stmtDrop = _epService.EPAdministrator.CreateEPL("@Drop update istream MyMapType set p0='a'");
            sender.SendEvent(MakeMap("p0", "E4", "p1", "E4"));
            _epService.EPRuntime.SendEvent(MakeMap("p0", "E5", "p1", "E5"), "MyMapType");
            _epService.EPRuntime.SendEvent(new SupportBean());
            Assert.IsFalse(_listener.IsInvoked);
    
            stmtDrop.Dispose();
            stmtSelect.Dispose();
            stmtTrigger.Dispose();
    
            // test bean
            stmtSelect = _epService.EPAdministrator.CreateEPL("select * from SupportBean");
            stmtSelect.Events += _listener.Update;
            _epService.EPAdministrator.CreateEPL("update istream SupportBean set IntPrimitive=999");
    
            fields = "TheString,IntPrimitive".Split(',');
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"E1", 999});
    
            sender = _epService.EPRuntime.GetEventSender("SupportBean");
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"E2", 999});
    
            stmtTrigger = _epService.EPAdministrator.CreateEPL("select * from MyMapType");
            stmtTrigger.Events += (s, args) => _epService.EPRuntime.Route(new SupportBean("E3", 0));
            _epService.EPRuntime.SendEvent(MakeMap("p0", "", "p1", ""), "MyMapType");
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"E3", 999});
    
            stmtDrop = _epService.EPAdministrator.CreateEPL("@Drop update istream SupportBean set IntPrimitive=1");
            sender.SendEvent(new SupportBean("E4", 0));
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 0));
            _epService.EPRuntime.SendEvent(MakeMap("p0", "", "p1", ""), "MyMapType");
            Assert.IsFalse(_listener.IsInvoked);
        }
    
        [Test]
        public void TestSODA()
        {
            IDictionary<String, Object> type = new Dictionary<string, object>();
            type.Put("p0", typeof(String));
            type.Put("p1", typeof(String));
            _epService.EPAdministrator.Configuration.AddEventType("MyMapType", type);
    
            var model = new EPStatementObjectModel();
            model.UpdateClause = UpdateClause.Create("MyMapType", Expressions.Eq(Expressions.Property("p1"), Expressions.Constant("newvalue")));
            model.UpdateClause.OptionalAsClauseStreamName = "mytype";
            model.UpdateClause.OptionalWhereClause = Expressions.Eq("p0", "E1");
            Assert.AreEqual("update istream MyMapType as mytype set p1=\"newvalue\" where p0=\"E1\"", model.ToEPL());
            
            // test map
            var stmtSelect = _epService.EPAdministrator.CreateEPL("select * from MyMapType");
            stmtSelect.Events += _listener.Update;
            _epService.EPAdministrator.Create(model);
    
            var fields = "p0,p1".Split(',');
            _epService.EPRuntime.SendEvent(MakeMap("p0", "E1", "p1", "E1"), "MyMapType");
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"E1", "newvalue"});
    
            // test unmap
            var text = "update istream MyMapType as mytype set p1=\"newvalue\" where p0=\"E1\"";
            model = _epService.EPAdministrator.CompileEPL(text);
            Assert.AreEqual(text, model.ToEPL());
        }
    
        [Test]
        public void TestXMLEvent()
        {
            var xml = "<simpleEvent><prop1>SAMPLE_V1</prop1></simpleEvent>";

            var simpleDoc = new XmlDocument();
            simpleDoc.LoadXml(xml);
    
            var config = new ConfigurationEventTypeXMLDOM();
            config.RootElementName = "simpleEvent";
            _epService.EPAdministrator.Configuration.AddEventType("MyXMLEvent", config);
    
            _epService.EPAdministrator.CreateEPL("insert into ABCStream select 1 as valOne, 2 as valTwo, * from MyXMLEvent");
            _epService.EPAdministrator.CreateEPL("update istream ABCStream set valOne = 987, valTwo=123 where prop1='SAMPLE_V1'");
            _epService.EPAdministrator.CreateEPL("select * from ABCStream").Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(simpleDoc);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "valOne,valTwo,prop1".Split(','), new Object[]{987, 123, "SAMPLE_V1"});
        }
    
        [Test]
        public void TestWrappedObject()
        {
            _epService.EPAdministrator.CreateEPL("insert into ABCStream select 1 as valOne, 2 as valTwo, * from SupportBean");
            var stmtUpd = _epService.EPAdministrator.CreateEPL("update istream ABCStream set valOne = 987, valTwo=123");
            _epService.EPAdministrator.CreateEPL("select * from ABCStream").Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "valOne,valTwo,TheString".Split(','), new Object[]{987, 123, "E1"});
    
            stmtUpd.Dispose();
            stmtUpd = _epService.EPAdministrator.CreateEPL("update istream ABCStream set TheString = 'A'");
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "valOne,valTwo,TheString".Split(','), new Object[]{1, 2, "A"});
    
            stmtUpd.Dispose();
            stmtUpd = _epService.EPAdministrator.CreateEPL("update istream ABCStream set TheString = 'B', valOne = 555");
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "valOne,valTwo,TheString".Split(','), new Object[]{555, 2, "B"});
        }
    
        [Test]
        public void TestCopyMethod()
        {
            _epService.EPAdministrator.CreateEPL("insert into ABCStream select * from SupportBeanCopyMethod");
            _epService.EPAdministrator.CreateEPL("update istream ABCStream set ValOne = 'x', ValTwo='y'");
            _epService.EPAdministrator.CreateEPL("select * from ABCStream").Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBeanCopyMethod("1", "2"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "ValOne,ValTwo".Split(','), new Object[]{"x", "y"});
        }
    
        [Test]
        public void TestSubquery()
        {
            IDictionary<String, Object> type = new Dictionary<string, object>();
            type.Put("s0", typeof(String));
            type.Put("s1", typeof(int));
            _epService.EPAdministrator.Configuration.AddEventType("MyMapTypeSelect", type);
    
            type = new Dictionary<String, Object>();
            type.Put("w0", typeof(int));
            _epService.EPAdministrator.Configuration.AddEventType("MyMapTypeWhere", type);
    
            var fields = "TheString,IntPrimitive".Split(',');
            _epService.EPAdministrator.CreateEPL("insert into ABCStream select * from SupportBean");
            var stmtUpd = _epService.EPAdministrator.CreateEPL("update istream ABCStream set TheString = (select s0 from MyMapTypeSelect.std:lastevent()) where IntPrimitive in (select w0 from MyMapTypeWhere.win:keepall())");
            _epService.EPAdministrator.CreateEPL("select * from ABCStream").Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"E1", 0});
    
            _epService.EPRuntime.SendEvent(MakeMap("w0", 1), "MyMapTypeWhere");
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{null, 1});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 2));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"E3", 2});
    
            _epService.EPRuntime.SendEvent(MakeMap("s0", "newvalue"), "MyMapTypeSelect");
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"newvalue", 1});
    
            _epService.EPRuntime.SendEvent(MakeMap("s0", "othervalue"), "MyMapTypeSelect");
            _epService.EPRuntime.SendEvent(new SupportBean("E5", 1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"othervalue", 1});
    
            // test correlated subquery
            stmtUpd.Dispose();
            stmtUpd = _epService.EPAdministrator.CreateEPL("update istream ABCStream set IntPrimitive = (select s1 from MyMapTypeSelect.win:keepall() where s0 = ABCStream.TheString)");
    
            // note that this will log an error (int primitive set to null), which is good, and leave the value unchanged
            _epService.EPRuntime.SendEvent(new SupportBean("E6", 8));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"E6", 8});
    
            _epService.EPRuntime.SendEvent(MakeMap("s0", "E7", "s1", 91), "MyMapTypeSelect");
            _epService.EPRuntime.SendEvent(new SupportBean("E7", 0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"E7", 91});
    
            // test correlated with as-clause
            stmtUpd.Dispose();
            _epService.EPAdministrator.CreateEPL("update istream ABCStream as mystream set IntPrimitive = (select s1 from MyMapTypeSelect.win:keepall() where s0 = mystream.TheString)");
    
            // note that this will log an error (int primitive set to null), which is good, and leave the value unchanged
            _epService.EPRuntime.SendEvent(new SupportBean("E8", 111));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"E8", 111});
    
            _epService.EPRuntime.SendEvent(MakeMap("s0", "E9", "s1", -1), "MyMapTypeSelect");
            _epService.EPRuntime.SendEvent(new SupportBean("E9", 0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"E9", -1});
        }
    
        [Test]
        public void TestUnprioritizedOrder()
        {
            IDictionary<String, Object> type = new Dictionary<string, object>();
            type.Put("s0", typeof(String));
            type.Put("s1", typeof(int));
            _epService.EPAdministrator.Configuration.AddEventType("MyMapType", type);
    
            var fields = "s0,s1".Split(',');
            _epService.EPAdministrator.CreateEPL("insert into ABCStream select * from MyMapType");
            _epService.EPAdministrator.CreateEPL("@Name('A') update istream ABCStream set s0='A'");
            _epService.EPAdministrator.CreateEPL("@Name('B') update istream ABCStream set s0='B'");
            _epService.EPAdministrator.CreateEPL("@Name('C') update istream ABCStream set s0='C'");
            _epService.EPAdministrator.CreateEPL("@Name('D') update istream ABCStream set s0='D'");
            _epService.EPAdministrator.CreateEPL("select * from ABCStream").Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(MakeMap("s0", "", "s1", 1), "MyMapType");
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"D", 1});
        }
    
        [Test]
        public void TestListenerDeliveryMultiupdate()
        {
            var listenerInsert = new SupportUpdateListener();
            var listeners = new SupportUpdateListener[5];
            for (var i = 0; i < listeners.Length; i++)
            {
                listeners[i] = new SupportUpdateListener();
            }
            
            var fields = "TheString,IntPrimitive,value1".Split(',');
            _epService.EPAdministrator.CreateEPL("insert into ABCStream select *, 'orig' as value1 from SupportBean").Events += listenerInsert.Update;
            _epService.EPAdministrator.CreateEPL("@Name('A') update istream ABCStream set TheString='A', value1='a' where IntPrimitive in (1,2)").Events += listeners[0].Update;
            _epService.EPAdministrator.CreateEPL("@Name('B') update istream ABCStream set TheString='B', value1='b' where IntPrimitive in (1,3)").Events += listeners[1].Update;
            _epService.EPAdministrator.CreateEPL("@Name('C') update istream ABCStream set TheString='C', value1='c' where IntPrimitive in (2,3)").Events += listeners[2].Update;
            _epService.EPAdministrator.CreateEPL("select * from ABCStream").Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(listenerInsert.AssertOneGetNewAndReset(), fields, new Object[]{"E1", 1, "orig"});
            EPAssertionUtil.AssertProps(listeners[0].AssertOneGetOld(), fields, new Object[]{"E1", 1, "orig"});
            EPAssertionUtil.AssertProps(listeners[0].AssertOneGetNew(), fields, new Object[]{"A", 1, "a"});
            EPAssertionUtil.AssertProps(listeners[1].AssertOneGetOld(), fields, new Object[]{"A", 1, "a"});
            EPAssertionUtil.AssertProps(listeners[1].AssertOneGetNew(), fields, new Object[]{"B", 1, "b"});
            Assert.IsFalse(listeners[2].IsInvoked);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"B", 1, "b"});
            Reset(listeners);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertProps(listenerInsert.AssertOneGetNewAndReset(), fields, new Object[]{"E2", 2, "orig"});
            EPAssertionUtil.AssertProps(listeners[0].AssertOneGetOld(), fields, new Object[]{"E2", 2, "orig"});
            EPAssertionUtil.AssertProps(listeners[0].AssertOneGetNew(), fields, new Object[]{"A", 2, "a"});
            Assert.IsFalse(listeners[1].IsInvoked);
            EPAssertionUtil.AssertProps(listeners[2].AssertOneGetOld(), fields, new Object[]{"A", 2, "a"});
            EPAssertionUtil.AssertProps(listeners[2].AssertOneGetNew(), fields, new Object[]{"C", 2, "c"});
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"C", 2, "c"});
            Reset(listeners);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            EPAssertionUtil.AssertProps(listenerInsert.AssertOneGetNewAndReset(), fields, new Object[]{"E3", 3, "orig"});
            Assert.IsFalse(listeners[0].IsInvoked);
            EPAssertionUtil.AssertProps(listeners[1].AssertOneGetOld(), fields, new Object[]{"E3", 3, "orig"});
            EPAssertionUtil.AssertProps(listeners[1].AssertOneGetNew(), fields, new Object[]{"B", 3, "b"});
            EPAssertionUtil.AssertProps(listeners[2].AssertOneGetOld(), fields, new Object[]{"B", 3, "b"});
            EPAssertionUtil.AssertProps(listeners[2].AssertOneGetNew(), fields, new Object[]{"C", 3, "c"});
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"C", 3, "c"});
            Reset(listeners);
        }
    
        [Test]
        public void TestListenerDeliveryMultiupdateMixed()
        {
            var listenerInsert = new SupportUpdateListener();
            var listeners = new SupportUpdateListener[5];
            for (var i = 0; i < listeners.Length; i++)
            {
                listeners[i] = new SupportUpdateListener();
            }
    
            var fields = "TheString,IntPrimitive,value1".Split(',');
            _epService.EPAdministrator.CreateEPL("insert into ABCStream select *, 'orig' as value1 from SupportBean").Events += listenerInsert.Update;
            _epService.EPAdministrator.CreateEPL("select * from ABCStream").Events += _listener.Update;
    
            _epService.EPAdministrator.CreateEPL("@Name('A') update istream ABCStream set TheString='A', value1='a'");
            _epService.EPAdministrator.CreateEPL("@Name('B') update istream ABCStream set TheString='B', value1='b'").Events += listeners[1].Update;
            _epService.EPAdministrator.CreateEPL("@Name('C') update istream ABCStream set TheString='C', value1='c'");
            _epService.EPAdministrator.CreateEPL("@Name('D') update istream ABCStream set TheString='D', value1='d'").Events += listeners[3].Update;
            _epService.EPAdministrator.CreateEPL("@Name('E') update istream ABCStream set TheString='E', value1='e'");
    
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            EPAssertionUtil.AssertProps(listenerInsert.AssertOneGetNewAndReset(), fields, new Object[]{"E4", 4, "orig"});
            Assert.IsFalse(listeners[0].IsInvoked);
            EPAssertionUtil.AssertProps(listeners[1].AssertOneGetOld(), fields, new Object[]{"A", 4, "a"});
            EPAssertionUtil.AssertProps(listeners[1].AssertOneGetNew(), fields, new Object[]{"B", 4, "b"});
            Assert.IsFalse(listeners[2].IsInvoked);
            EPAssertionUtil.AssertProps(listeners[3].AssertOneGetOld(), fields, new Object[]{"C", 4, "c"});
            EPAssertionUtil.AssertProps(listeners[3].AssertOneGetNew(), fields, new Object[]{"D", 4, "d"});
            Assert.IsFalse(listeners[4].IsInvoked);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"E", 4, "e"});
            Reset(listeners);
    
            _epService.EPAdministrator.GetStatement("B").RemoveAllEventHandlers();
            _epService.EPAdministrator.GetStatement("D").RemoveAllEventHandlers();
            _epService.EPAdministrator.GetStatement("A").Events += listeners[0].Update;
            _epService.EPAdministrator.GetStatement("E").Events += listeners[4].Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E5", 5));
            EPAssertionUtil.AssertProps(listenerInsert.AssertOneGetNewAndReset(), fields, new Object[]{"E5", 5, "orig"});
            EPAssertionUtil.AssertProps(listeners[0].AssertOneGetOld(), fields, new Object[]{"E5", 5, "orig"});
            EPAssertionUtil.AssertProps(listeners[0].AssertOneGetNew(), fields, new Object[]{"A", 5, "a"});
            Assert.IsFalse(listeners[1].IsInvoked);
            Assert.IsFalse(listeners[2].IsInvoked);
            Assert.IsFalse(listeners[3].IsInvoked);
            EPAssertionUtil.AssertProps(listeners[4].AssertOneGetOld(), fields, new Object[]{"D", 5, "d"});
            EPAssertionUtil.AssertProps(listeners[4].AssertOneGetNew(), fields, new Object[]{"E", 5, "e"});
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"E", 5, "e"});
            Reset(listeners);
        }
    
        private void Reset(SupportUpdateListener[] listeners)
        {
            foreach (var listener in listeners)
            {
                listener.Reset();
            }
        }
    
        private IDictionary<String, Object> MakeMap(String prop1, Object val1, String prop2, Object val2, String prop3, Object val3)
        {
            IDictionary<String, Object> map = new Dictionary<string, object>();
            map.Put(prop1, val1);
            map.Put(prop2, val2);
            map.Put(prop3, val3);
            return map;
        }
    
        private IDictionary<String, Object> MakeMap(String prop1, Object val1, String prop2, Object val2)
        {
            IDictionary<String, Object> map = new Dictionary<string, object>();
            map.Put(prop1, val1);
            map.Put(prop2, val2);
            return map;
        }
    
        private IDictionary<String, Object> MakeMap(String prop1, Object val1)
        {
            IDictionary<String, Object> map = new Dictionary<string, object>();
            map.Put(prop1, val1);
            return map;
        }
    
        private void TryInvalid(String expression, String message)
        {
            try
            {
                _epService.EPAdministrator.CreateEPL(expression);
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual(message, ex.Message);
            }
        }
    
        private SupportBean MakeSupportBean(String theString, int intPrimitive, double doublePrimitive) {
            var sb = new SupportBean(theString, intPrimitive);
            sb.DoublePrimitive = doublePrimitive;
            return sb;
        }
    
        public interface BaseInterface
        {
            string I { get; set; }
        }
    
        [Serializable]
        public class BaseOne : BaseInterface
        {
            public BaseOne()
            {
            }
    
            public BaseOne(String i, String p)
            {
                I = i;
                P = p;
            }

            public string P { get; set; }

            public string I { get; set; }
        }

        [Serializable]
        public class BaseTwo : BaseInterface
        {
            public BaseTwo()
            {
            }
    
            public BaseTwo(String p)
            {
                P = p;
            }

            public string P { get; set; }

            public string I { get; set; }
        }

        [Serializable]
        public class BaseOneA : BaseOne
        {
            public BaseOneA()
            {
            }
    
            public BaseOneA(String i, String p, String pa) : base(i, p)
            {
                Pa = pa;
            }

            public string Pa { get; set; }
        }

        [Serializable]
        public class BaseOneB : BaseOne
        {
            public BaseOneB()
            {
            }
    
            public BaseOneB(String i, String p, String pb) : base(i, p)
            {
                Pb = pb;
            }

            public string Pb { get; set; }
        }
    
        public static void SetIntBoxedValue(SupportBean sb, int value) {
            sb.IntBoxed = value;
        }
    }
}
