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
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl.other
{
    public class ExecEPLUpdate : RegressionExecution
    {
        public override void Configure(Configuration configuration)
        {
            configuration.AddEventType(typeof(SupportBeanReadOnly));
            configuration.AddEventType(typeof(SupportBeanErrorTestingOne));
            configuration.AddEventType<SupportBean>();
            configuration.EngineDefaults.Execution.IsPrioritized = true;

            var legacy = new ConfigurationEventTypeLegacy();
            legacy.CopyMethod = "MyCopyMethod";
            configuration.AddEventType("SupportBeanCopyMethod", typeof(SupportBeanCopyMethod).FullName, legacy);
        }

        public override void Run(EPServiceProvider epService)
        {
            RunAssertionFieldUpdateOrder(epService);
            RunAssertionInvalid(epService);
            RunAssertionInsertIntoWBeanWhere(epService);
            RunAssertionInsertIntoWMapNoWhere(epService);
            RunAssertionFieldsWithPriority(epService);
            RunAssertionInsertDirectBeanTypeInheritance(epService);
            RunAssertionNamedWindow(epService);
            RunAssertionTypeWidener(epService);
            RunAssertionSendRouteSenderPreprocess(epService);
            RunAssertionSODA(epService);
            RunAssertionXMLEvent(epService);
            RunAssertionWrappedObject(epService);
            RunAssertionCopyMethod(epService);
            RunAssertionSubquery(epService);
            RunAssertionUnprioritizedOrder(epService);
            RunAssertionListenerDeliveryMultiupdate(epService);
            RunAssertionListenerDeliveryMultiupdateMixed(epService);
        }

        private void RunAssertionFieldUpdateOrder(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddVariable("myvar", typeof(int?), 10);

            epService.EPAdministrator.CreateEPL(
                "update istream SupportBean " +
                "set intPrimitive=myvar, intBoxed=intPrimitive");
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from SupportBean");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            string[] fields = "intPrimitive,intBoxed".Split(',');

            epService.EPRuntime.SendEvent(MakeSupportBean("E1", 1, 2));
            EPAssertionUtil.AssertProps(listener.GetAndResetLastNewData()[0], fields, new object[] {10, 1});

            stmt.Dispose();
        }

        private void RunAssertionInvalid(EPServiceProvider epService)
        {
            var type = new Dictionary<string, object>();
            type.Put("p0", typeof(long));
            type.Put("p1", typeof(long));
            type.Put("p2", typeof(long));
            type.Put("p3", typeof(string));
            epService.EPAdministrator.Configuration.AddEventType("MyMapTypeInv", type);

            var configXML = new ConfigurationEventTypeXMLDOM();
            configXML.RootElementName = "MyXMLEvent";
            epService.EPAdministrator.Configuration.AddEventType("MyXmlEvent", configXML);

            epService.EPAdministrator.CreateEPL("insert into SupportBeanStream select * from SupportBean");
            epService.EPAdministrator.CreateEPL(
                "insert into SupportBeanStreamTwo select * from pattern[a=SupportBean -> b=SupportBean]");
            epService.EPAdministrator.CreateEPL("insert into SupportBeanStreamRO select * from SupportBeanReadOnly");

            TryInvalid(
                epService, "update istream SupportBeanStream set intPrimitive=longPrimitive",
                "Error starting statement: Invalid assignment of column 'longPrimitive' of type 'long' to event property 'intPrimitive' typed as 'int', column and parameter types mismatch [update istream SupportBeanStream set intPrimitive=longPrimitive]");
            TryInvalid(
                epService, "update istream SupportBeanStream set xxx='abc'",
                "Error starting statement: Property 'xxx' is not available for write access [update istream SupportBeanStream set xxx='abc']");
            TryInvalid(
                epService, "update istream SupportBeanStream set intPrimitive=null",
                "Error starting statement: Invalid assignment of column 'null' of null type to event property 'intPrimitive' typed as 'int', nullable type mismatch [update istream SupportBeanStream set intPrimitive=null]");
            TryInvalid(
                epService, "update istream SupportBeanStreamTwo set a.intPrimitive=10",
                "Error starting statement: Property 'a.intPrimitive' is not available for write access [update istream SupportBeanStreamTwo set a.intPrimitive=10]");
            TryInvalid(
                epService, "update istream SupportBeanStreamRO set side='a'",
                "Error starting statement: Property 'side' is not available for write access [update istream SupportBeanStreamRO set side='a']");
            TryInvalid(
                epService, "update istream SupportBean set longPrimitive=sum(intPrimitive)",
                "Error starting statement: Aggregation functions may not be used within an update-clause [update istream SupportBean set longPrimitive=sum(intPrimitive)]");
            TryInvalid(
                epService, "update istream SupportBean set longPrimitive=longPrimitive where sum(intPrimitive) = 1",
                "Error starting statement: Aggregation functions may not be used within an update-clause [update istream SupportBean set longPrimitive=longPrimitive where sum(intPrimitive) = 1]");
            TryInvalid(
                epService, "update istream SupportBean set longPrimitive=Prev(1, longPrimitive)",
                "Error starting statement: Previous function cannot be used in this context [update istream SupportBean set longPrimitive=Prev(1, longPrimitive)]");
            TryInvalid(
                epService, "update istream MyXmlEvent set abc=1",
                "Error starting statement: Property 'abc' is not available for write access [update istream MyXmlEvent set abc=1]");
            TryInvalid(
                epService, "update istream SupportBeanErrorTestingOne set value='1'",
                "Error starting statement: The update-clause requires the underlying event representation to support copy (via Serializable by default) [update istream SupportBeanErrorTestingOne set value='1']");
            TryInvalid(
                epService,
                "update istream SupportBean set longPrimitive=(select p0 from MyMapTypeInv#lastevent where theString=p3)",
                "Error starting statement: Failed to plan subquery number 1 querying MyMapTypeInv: Failed to validate filter expression 'theString=p3': Property named 'theString' must be prefixed by a stream name, use the stream name itself or use the as-clause to name the stream with the property in the format \"stream.property\" [update istream SupportBean set longPrimitive=(select p0 from MyMapTypeInv#lastevent where theString=p3)]");
            TryInvalid(
                epService, "update istream XYZ.GYH set a=1",
                "Failed to resolve event type: Event type or class named 'XYZ.GYH' was not found [update istream XYZ.GYH set a=1]");
            TryInvalid(
                epService, "update istream SupportBean set 1",
                "Error starting statement: Missing property assignment expression in assignment number 0 [update istream SupportBean set 1]");

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionInsertIntoWBeanWhere(EPServiceProvider epService)
        {
            var container = epService.Container;

            var listenerInsert = new SupportUpdateListener();
            EPStatement stmtInsert =
                epService.EPAdministrator.CreateEPL("insert into MyStreamBW select * from SupportBean");
            stmtInsert.Events += listenerInsert.Update;

            var listenerUpdate = new SupportUpdateListener();
            EPStatement stmtUpdOne = epService.EPAdministrator.CreateEPL(
                "update istream MyStreamBW set intPrimitive=10, theString='O_' || theString where intPrimitive=1");
            stmtUpdOne.Events += listenerUpdate.Update;

            EPStatement stmtSelect = epService.EPAdministrator.CreateEPL("select * from MyStreamBW");
            var listener = new SupportUpdateListener();
            stmtSelect.Events += listener.Update;

            string[] fields = "theString,intPrimitive".Split(',');
            epService.EPRuntime.SendEvent(new SupportBean("E1", 9));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E1", 9});
            EPAssertionUtil.AssertProps(listenerInsert.AssertOneGetNewAndReset(), fields, new object[] {"E1", 9});
            Assert.IsFalse(listenerUpdate.IsInvoked);

            epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"O_E2", 10});
            EPAssertionUtil.AssertProps(listenerInsert.AssertOneGetNewAndReset(), fields, new object[] {"E2", 1});
            EPAssertionUtil.AssertProps(listenerUpdate.AssertOneGetOld(), fields, new object[] {"E2", 1});
            EPAssertionUtil.AssertProps(listenerUpdate.AssertOneGetNew(), fields, new object[] {"O_E2", 10});
            listenerUpdate.Reset();

            epService.EPRuntime.SendEvent(new SupportBean("E3", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E3", 2});
            EPAssertionUtil.AssertProps(listenerInsert.AssertOneGetNewAndReset(), fields, new object[] {"E3", 2});
            Assert.IsFalse(listenerUpdate.IsInvoked);

            epService.EPRuntime.SendEvent(new SupportBean("E4", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"O_E4", 10});
            EPAssertionUtil.AssertProps(listenerInsert.AssertOneGetNewAndReset(), fields, new object[] {"E4", 1});
            EPAssertionUtil.AssertProps(listenerUpdate.AssertOneGetOld(), fields, new object[] {"E4", 1});
            EPAssertionUtil.AssertProps(listenerUpdate.AssertOneGetNew(), fields, new object[] {"O_E4", 10});
            listenerUpdate.Reset();

            EPStatement stmtUpdTwo = epService.EPAdministrator.CreateEPL(
                "update istream MyStreamBW as xyz set intPrimitive=xyz.intPrimitive + 1000 where intPrimitive=2");
            stmtUpdTwo.Events += listenerUpdate.Update;

            epService.EPRuntime.SendEvent(new SupportBean("E5", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E5", 1002});
            EPAssertionUtil.AssertProps(listenerInsert.AssertOneGetNewAndReset(), fields, new object[] {"E5", 2});
            EPAssertionUtil.AssertProps(listenerUpdate.AssertOneGetOld(), fields, new object[] {"E5", 2});
            EPAssertionUtil.AssertProps(listenerUpdate.AssertOneGetNew(), fields, new object[] {"E5", 1002});
            listenerUpdate.Reset();

            stmtUpdOne.Dispose();

            epService.EPRuntime.SendEvent(new SupportBean("E6", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E6", 1});
            EPAssertionUtil.AssertProps(listenerInsert.AssertOneGetNewAndReset(), fields, new object[] {"E6", 1});
            Assert.IsFalse(listenerUpdate.IsInvoked);

            epService.EPRuntime.SendEvent(new SupportBean("E7", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E7", 1002});
            EPAssertionUtil.AssertProps(listenerInsert.AssertOneGetNewAndReset(), fields, new object[] {"E7", 2});
            EPAssertionUtil.AssertProps(listenerUpdate.AssertOneGetOld(), fields, new object[] {"E7", 2});
            EPAssertionUtil.AssertProps(listenerUpdate.AssertOneGetNew(), fields, new object[] {"E7", 1002});
            listenerUpdate.Reset();
            Assert.IsFalse(stmtUpdTwo.HasFirst());

            stmtUpdTwo.RemoveAllEventHandlers();

            epService.EPRuntime.SendEvent(new SupportBean("E8", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E8", 1002});
            EPAssertionUtil.AssertProps(listenerInsert.AssertOneGetNewAndReset(), fields, new object[] {"E8", 2});
            Assert.IsFalse(listenerUpdate.IsInvoked);

            var subscriber = new SupportSubscriber();
            stmtUpdTwo.Subscriber = subscriber;

            epService.EPRuntime.SendEvent(new SupportBean("E9", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E9", 1002});
            EPAssertionUtil.AssertProps(listenerInsert.AssertOneGetNewAndReset(), fields, new object[] {"E9", 2});
            EPAssertionUtil.AssertPropsPono(container, subscriber.GetOldDataListFlattened()[0], fields, new object[] {"E9", 2});
            EPAssertionUtil.AssertPropsPono(container, subscriber.GetNewDataListFlattened()[0], fields, new object[] {"E9", 1002});
            subscriber.Reset();

            stmtUpdTwo.Dispose();

            epService.EPRuntime.SendEvent(new SupportBean("E10", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E10", 2});
            EPAssertionUtil.AssertProps(listenerInsert.AssertOneGetNewAndReset(), fields, new object[] {"E10", 2});
            Assert.IsFalse(listenerUpdate.IsInvoked);

            EPStatement stmtUpdThree =
                epService.EPAdministrator.CreateEPL("update istream MyStreamBW set intPrimitive=intBoxed");
            stmtUpdThree.Events += listenerUpdate.Update;

            epService.EPRuntime.SendEvent(new SupportBean("E11", 2));
            EPAssertionUtil.AssertProps(listenerUpdate.AssertOneGetNew(), fields, new object[] {"E11", 2});
            listenerUpdate.Reset();

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionInsertIntoWMapNoWhere(EPServiceProvider epService)
        {
            var type = new Dictionary<string, object>();
            type.Put("p0", typeof(long));
            type.Put("p1", typeof(long));
            type.Put("p2", typeof(long));
            epService.EPAdministrator.Configuration.AddEventType("MyMapTypeII", type);

            var listenerInsert = new SupportUpdateListener();
            EPStatement stmtInsert =
                epService.EPAdministrator.CreateEPL("insert into MyStreamII select * from MyMapTypeII");
            stmtInsert.Events += listenerInsert.Update;

            EPStatement stmtUpd = epService.EPAdministrator.CreateEPL("update istream MyStreamII set p0=p1, p1=p0");

            EPStatement stmtSelect = epService.EPAdministrator.CreateEPL("select * from MyStreamII");
            var listener = new SupportUpdateListener();
            stmtSelect.Events += listener.Update;

            string[] fields = "p0,p1,p2".Split(',');
            epService.EPRuntime.SendEvent(MakeMap("p0", 10, "p1", 1, "p2", 100), "MyMapTypeII");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {1, 10, 100});
            EPAssertionUtil.AssertProps(listenerInsert.AssertOneGetNewAndReset(), fields, new object[] {10, 1, 100});

            stmtUpd.Stop();
            stmtUpd.Start();

            epService.EPRuntime.SendEvent(MakeMap("p0", 5, "p1", 4, "p2", 101), "MyMapTypeII");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {4, 5, 101});
            EPAssertionUtil.AssertProps(listenerInsert.AssertOneGetNewAndReset(), fields, new object[] {5, 4, 101});

            stmtUpd.Dispose();

            epService.EPRuntime.SendEvent(MakeMap("p0", 20, "p1", 0, "p2", 102), "MyMapTypeII");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {20, 0, 102});
            EPAssertionUtil.AssertProps(listenerInsert.AssertOneGetNewAndReset(), fields, new object[] {20, 0, 102});

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionFieldsWithPriority(EPServiceProvider epService)
        {
            foreach (EventRepresentationChoice rep in EnumHelper.GetValues<EventRepresentationChoice>())
            {
                TryAssertionFieldsWithPriority(epService, rep);
            }
        }

        private void TryAssertionFieldsWithPriority(
            EPServiceProvider epService, EventRepresentationChoice eventRepresentationEnum)
        {
            epService.EPAdministrator.CreateEPL(
                eventRepresentationEnum.GetAnnotationText() +
                " insert into MyStream select theString, intPrimitive from SupportBean(theString not like 'Z%')");
            epService.EPAdministrator.CreateEPL(
                eventRepresentationEnum.GetAnnotationText() +
                " insert into MyStream select 'AX'||theString as theString, intPrimitive from SupportBean(theString like 'Z%')");
            epService.EPAdministrator.CreateEPL(
                eventRepresentationEnum.GetAnnotationText() +
                " @Name('a') @Priority(12) update istream MyStream set intPrimitive=-2 where intPrimitive=-1");
            epService.EPAdministrator.CreateEPL(
                eventRepresentationEnum.GetAnnotationText() +
                " @Name('b') @Priority(11) update istream MyStream set intPrimitive=-1 where theString like 'D%'");
            epService.EPAdministrator.CreateEPL(
                eventRepresentationEnum.GetAnnotationText() +
                " @Name('c') @Priority(9) update istream MyStream set intPrimitive=9 where theString like 'A%'");
            epService.EPAdministrator.CreateEPL(
                eventRepresentationEnum.GetAnnotationText() +
                " @Name('d') @Priority(8) update istream MyStream set intPrimitive=8 where theString like 'A%' or theString like 'C%'");
            epService.EPAdministrator.CreateEPL(
                " @Name('e') @Priority(10) update istream MyStream set intPrimitive=10 where theString like 'A%'");
            epService.EPAdministrator.CreateEPL(
                " @Name('f') @Priority(7) update istream MyStream set intPrimitive=7 where theString like 'A%' or theString like 'C%'");
            epService.EPAdministrator.CreateEPL(
                " @Name('g') @Priority(6) update istream MyStream set intPrimitive=6 where theString like 'A%'");
            epService.EPAdministrator.CreateEPL(
                " @Name('h') @Drop update istream MyStream set intPrimitive=6 where theString like 'B%'");

            EPStatement stmtSelect =
                epService.EPAdministrator.CreateEPL("select * from MyStream where intPrimitive > 0");
            var listener = new SupportUpdateListener();
            stmtSelect.Events += listener.Update;

            string[] fields = "theString,intPrimitive".Split(',');
            epService.EPRuntime.SendEvent(new SupportBean("A1", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"A1", 10});

            epService.EPRuntime.SendEvent(new SupportBean("B1", 0));
            Assert.IsFalse(listener.IsInvoked);

            epService.EPRuntime.SendEvent(new SupportBean("C1", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"C1", 8});

            epService.EPRuntime.SendEvent(new SupportBean("D1", 100));
            Assert.IsFalse(listener.IsInvoked);

            stmtSelect.Stop();
            stmtSelect = epService.EPAdministrator.CreateEPL("select * from MyStream");
            stmtSelect.Events += listener.Update;
            Assert.IsTrue(eventRepresentationEnum.MatchesClass(stmtSelect.EventType.UnderlyingType));

            epService.EPRuntime.SendEvent(new SupportBean("D1", -2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"D1", -2});

            epService.EPRuntime.SendEvent(new SupportBean("Z1", -3));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"AXZ1", 10});

            epService.EPAdministrator.GetStatement("e").Stop();
            epService.EPRuntime.SendEvent(new SupportBean("Z2", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"AXZ2", 9});

            epService.EPAdministrator.GetStatement("c").Stop();
            epService.EPAdministrator.GetStatement("d").Stop();
            epService.EPAdministrator.GetStatement("f").Stop();
            epService.EPAdministrator.GetStatement("g").Stop();
            epService.EPRuntime.SendEvent(new SupportBean("Z3", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"AXZ3", 0});

            epService.EPAdministrator.DestroyAllStatements();
            foreach (string name in "MyStream".Split(','))
            {
                epService.EPAdministrator.Configuration.RemoveEventType(name, true);
            }
        }

        private void RunAssertionInsertDirectBeanTypeInheritance(EPServiceProvider epService)
        {
            var type = new Dictionary<string, object>();
            type.Put("p0", typeof(string));
            type.Put("p1", typeof(string));
            epService.EPAdministrator.Configuration.AddEventType("MyMapTypeIDB", type);
            epService.EPAdministrator.Configuration.AddEventType("BaseInterface", typeof(BaseInterface));
            epService.EPAdministrator.Configuration.AddEventType("BaseOne", typeof(BaseOne));
            epService.EPAdministrator.Configuration.AddEventType("BaseOneA", typeof(BaseOneA));
            epService.EPAdministrator.Configuration.AddEventType("BaseOneB", typeof(BaseOneB));
            epService.EPAdministrator.Configuration.AddEventType("BaseTwo", typeof(BaseTwo));

            // test update applies to child types via interface
            EPStatement stmtInsert =
                epService.EPAdministrator.CreateEPL("insert into BaseOne select p0 as i, p1 as p from MyMapTypeIDB");
            epService.EPAdministrator.CreateEPL(
                "@Name('a') update istream BaseInterface set i='XYZ' where i like 'E%'");
            EPStatement stmtSelect = epService.EPAdministrator.CreateEPL("select * from BaseOne");
            var listener = new SupportUpdateListener();
            stmtSelect.Events += listener.Update;

            string[] fields = "i,p".Split(',');
            epService.EPRuntime.SendEvent(MakeMap("p0", "E1", "p1", "E1"), "MyMapTypeIDB");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"XYZ", "E1"});

            epService.EPRuntime.SendEvent(MakeMap("p0", "F1", "p1", "E2"), "MyMapTypeIDB");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"F1", "E2"});

            epService.EPAdministrator.CreateEPL("@Priority(2) @Name('b') update istream BaseOne set i='BLANK'");

            epService.EPRuntime.SendEvent(MakeMap("p0", "somevalue", "p1", "E3"), "MyMapTypeIDB");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"BLANK", "E3"});

            epService.EPAdministrator.CreateEPL("@Priority(3) @Name('c') update istream BaseOneA set i='FINAL'");

            epService.EPRuntime.SendEvent(MakeMap("p0", "somevalue", "p1", "E4"), "MyMapTypeIDB");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"BLANK", "E4"});

            stmtInsert.Stop();
            stmtInsert = epService.EPAdministrator.CreateEPL(
                "insert into BaseOneA select p0 as i, p1 as p, 'a' as pa from MyMapTypeIDB");

            epService.EPRuntime.SendEvent(MakeMap("p0", "somevalue", "p1", "E5"), "MyMapTypeIDB");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"FINAL", "E5"});

            stmtInsert.Stop();
            stmtInsert = epService.EPAdministrator.CreateEPL(
                "insert into BaseOneB select p0 as i, p1 as p, 'b' as pb from MyMapTypeIDB");

            epService.EPRuntime.SendEvent(MakeMap("p0", "somevalue", "p1", "E6"), "MyMapTypeIDB");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"BLANK", "E6"});

            stmtInsert.Stop();
            stmtInsert = epService.EPAdministrator.CreateEPL("insert into BaseTwo select p0 as i, p1 as p from MyMapTypeIDB");

            stmtSelect.Stop();
            stmtSelect = epService.EPAdministrator.CreateEPL("select * from BaseInterface");
            stmtSelect.Events += listener.Update;

            epService.EPRuntime.SendEvent(MakeMap("p0", "E2", "p1", "E7"), "MyMapTypeIDB");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), new[] {"i"}, new object[] {"XYZ"});

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionNamedWindow(EPServiceProvider epService)
        {
            var type = new Dictionary<string, object>();
            type.Put("p0", typeof(string));
            type.Put("p1", typeof(string));
            epService.EPAdministrator.Configuration.AddEventType("MyMapTypeNW", type);

            string[] fields = "p0,p1".Split(',');
            var listenerWindow = new SupportUpdateListener();
            var listenerInsert = new SupportUpdateListener();
            var listenerOnSelect = new SupportUpdateListener();
            var listenerInsertOnSelect = new SupportUpdateListener();
            var listenerWindowSelect = new SupportUpdateListener();

            epService.EPAdministrator.CreateEPL("create window AWindow#keepall select * from MyMapTypeNW")
                .Events += listenerWindow.Update;
            epService.EPAdministrator.CreateEPL("insert into AWindow select * from MyMapTypeNW")
                .Events += listenerInsert.Update;
            epService.EPAdministrator.CreateEPL("select * from AWindow").Events += listenerWindowSelect.Update;
            epService.EPAdministrator.CreateEPL("update istream AWindow set p1='newvalue'");

            epService.EPRuntime.SendEvent(MakeMap("p0", "E1", "p1", "oldvalue"), "MyMapTypeNW");
            EPAssertionUtil.AssertProps(
                listenerWindow.AssertOneGetNewAndReset(), fields, new object[] {"E1", "newvalue"});
            EPAssertionUtil.AssertProps(
                listenerInsert.AssertOneGetNewAndReset(), fields, new object[] {"E1", "oldvalue"});
            EPAssertionUtil.AssertProps(
                listenerWindowSelect.AssertOneGetNewAndReset(), fields, new object[] {"E1", "newvalue"});

            epService.EPAdministrator.CreateEPL("on SupportBean(theString='A') select win.* from AWindow as win")
                .Events += listenerOnSelect.Update;
            epService.EPRuntime.SendEvent(new SupportBean("A", 0));
            EPAssertionUtil.AssertProps(
                listenerOnSelect.AssertOneGetNewAndReset(), fields, new object[] {"E1", "newvalue"});

            epService.EPAdministrator
                .CreateEPL("on SupportBean(theString='B') insert into MyOtherStream select win.* from AWindow as win")
                .Events += listenerOnSelect.Update;
            epService.EPRuntime.SendEvent(new SupportBean("B", 1));
            EPAssertionUtil.AssertProps(
                listenerOnSelect.AssertOneGetNewAndReset(), fields, new object[] {"E1", "newvalue"});

            epService.EPAdministrator.CreateEPL("update istream MyOtherStream set p0='a', p1='b'");
            epService.EPAdministrator.CreateEPL("select * from MyOtherStream").Events += listenerInsertOnSelect.Update;
            epService.EPRuntime.SendEvent(new SupportBean("B", 1));
            EPAssertionUtil.AssertProps(
                listenerOnSelect.AssertOneGetNewAndReset(), fields, new object[] {"E1", "newvalue"});
            EPAssertionUtil.AssertProps(
                listenerInsertOnSelect.AssertOneGetNewAndReset(), fields, new object[] {"a", "b"});

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionTypeWidener(EPServiceProvider epService)
        {
            string[] fields = "theString,longBoxed,intBoxed".Split(',');
            epService.EPAdministrator.CreateEPL("insert into AStream select * from SupportBean");
            epService.EPAdministrator.CreateEPL("update istream AStream set longBoxed=intBoxed, intBoxed=null");
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("select * from AStream").Events += listener.Update;

            var bean = new SupportBean("E1", 0);
            bean.LongBoxed = 888L;
            bean.IntBoxed = 999;
            epService.EPRuntime.SendEvent(bean);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E1", 999L, null});

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionSendRouteSenderPreprocess(EPServiceProvider epService)
        {
            var type = new Dictionary<string, object>();
            type.Put("p0", typeof(string));
            type.Put("p1", typeof(string));
            epService.EPAdministrator.Configuration.AddEventType("MyMapTypeSR", type);

            // test map
            EPStatement stmtSelect = epService.EPAdministrator.CreateEPL("select * from MyMapTypeSR");
            var listener = new SupportUpdateListener();
            stmtSelect.Events += listener.Update;
            epService.EPAdministrator.CreateEPL("update istream MyMapTypeSR set p0='a'");

            string[] fields = "p0,p1".Split(',');
            epService.EPRuntime.SendEvent(MakeMap("p0", "E1", "p1", "E1"), "MyMapTypeSR");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"a", "E1"});

            EventSender sender = epService.EPRuntime.GetEventSender("MyMapTypeSR");
            sender.SendEvent(MakeMap("p0", "E2", "p1", "E2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"a", "E2"});

            EPStatement stmtTrigger = epService.EPAdministrator.CreateEPL("select * from SupportBean");
            stmtTrigger.Events += (sender1, args) =>
            {
                epService.EPRuntime.Route(MakeMap("p0", "E3", "p1", "E3"), "MyMapTypeSR");
            };
            epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"a", "E3"});

            EPStatement stmtDrop = epService.EPAdministrator.CreateEPL("@Drop update istream MyMapTypeSR set p0='a'");
            sender.SendEvent(MakeMap("p0", "E4", "p1", "E4"));
            epService.EPRuntime.SendEvent(MakeMap("p0", "E5", "p1", "E5"), "MyMapTypeSR");
            epService.EPRuntime.SendEvent(new SupportBean());
            Assert.IsFalse(listener.IsInvoked);

            stmtDrop.Dispose();
            stmtSelect.Dispose();
            stmtTrigger.Dispose();

            // test bean
            stmtSelect = epService.EPAdministrator.CreateEPL("select * from SupportBean");
            stmtSelect.Events += listener.Update;
            epService.EPAdministrator.CreateEPL("update istream SupportBean set intPrimitive=999");

            fields = "theString,intPrimitive".Split(',');
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E1", 999});

            sender = epService.EPRuntime.GetEventSender("SupportBean");
            epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E2", 999});

            stmtTrigger = epService.EPAdministrator.CreateEPL("select * from MyMapTypeSR");
            stmtTrigger.Events += (sender2, args) => { epService.EPRuntime.Route(new SupportBean("E3", 0)); };
            epService.EPRuntime.SendEvent(MakeMap("p0", "", "p1", ""), "MyMapTypeSR");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E3", 999});

            epService.EPAdministrator.CreateEPL("@Drop update istream SupportBean set intPrimitive=1");
            sender.SendEvent(new SupportBean("E4", 0));
            epService.EPRuntime.SendEvent(new SupportBean("E4", 0));
            epService.EPRuntime.SendEvent(MakeMap("p0", "", "p1", ""), "MyMapTypeSR");
            Assert.IsFalse(listener.IsInvoked);

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionSODA(EPServiceProvider epService)
        {
            var type = new Dictionary<string, object>();
            type.Put("p0", typeof(string));
            type.Put("p1", typeof(string));
            epService.EPAdministrator.Configuration.AddEventType("MyMapTypeSODA", type);

            var model = new EPStatementObjectModel();
            model.UpdateClause = UpdateClause.Create(
                "MyMapTypeSODA", Expressions.Eq(Expressions.Property("p1"), Expressions.Constant("newvalue")));
            model.UpdateClause.OptionalAsClauseStreamName = "mytype";
            model.UpdateClause.OptionalWhereClause = Expressions.Eq("p0", "E1");
            Assert.AreEqual(
                "update istream MyMapTypeSODA as mytype set p1=\"newvalue\" where p0=\"E1\"", model.ToEPL());

            // test map
            EPStatement stmtSelect = epService.EPAdministrator.CreateEPL("select * from MyMapTypeSODA");
            var listener = new SupportUpdateListener();
            stmtSelect.Events += listener.Update;
            epService.EPAdministrator.Create(model);

            string[] fields = "p0,p1".Split(',');
            epService.EPRuntime.SendEvent(MakeMap("p0", "E1", "p1", "E1"), "MyMapTypeSODA");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E1", "newvalue"});

            // test unmap
            string text = "update istream MyMapTypeSODA as mytype set p1=\"newvalue\" where p0=\"E1\"";
            model = epService.EPAdministrator.CompileEPL(text);
            Assert.AreEqual(text, model.ToEPL());

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionXMLEvent(EPServiceProvider epService)
        {
            string xml = "<simpleEvent><prop1>SAMPLE_V1</prop1></simpleEvent>";

            var simpleDoc = new XmlDocument();
            simpleDoc.LoadXml(xml);

            var config = new ConfigurationEventTypeXMLDOM();
            config.RootElementName = "simpleEvent";
            epService.EPAdministrator.Configuration.AddEventType("MyXMLEvent", config);

            epService.EPAdministrator.CreateEPL(
                "insert into ABCStreamXML select 1 as valOne, 2 as valTwo, * from MyXMLEvent");
            epService.EPAdministrator.CreateEPL(
                "update istream ABCStreamXML set valOne = 987, valTwo=123 where prop1='SAMPLE_V1'");
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("select * from ABCStreamXML").Events += listener.Update;

            epService.EPRuntime.SendEvent(simpleDoc);
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), "valOne,valTwo,prop1".Split(','),
                new object[] {987, 123, "SAMPLE_V1"});

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionWrappedObject(EPServiceProvider epService)
        {
            epService.EPAdministrator.CreateEPL(
                "insert into ABCStreamWO select 1 as valOne, 2 as valTwo, * from SupportBean");
            EPStatement stmtUpd =
                epService.EPAdministrator.CreateEPL("update istream ABCStreamWO set valOne = 987, valTwo=123");
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("select * from ABCStreamWO").Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), "valOne,valTwo,theString".Split(','),
                new object[] {987, 123, "E1"});

            stmtUpd.Dispose();
            stmtUpd = epService.EPAdministrator.CreateEPL("update istream ABCStreamWO set theString = 'A'");

            epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), "valOne,valTwo,theString".Split(','), new object[] {1, 2, "A"});

            stmtUpd.Dispose();
            epService.EPAdministrator.CreateEPL("update istream ABCStreamWO set theString = 'B', valOne = 555");

            epService.EPRuntime.SendEvent(new SupportBean("E3", 0));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), "valOne,valTwo,theString".Split(','), new object[] {555, 2, "B"});

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionCopyMethod(EPServiceProvider epService)
        {
            epService.EPAdministrator.CreateEPL("insert into ABCStreamCM select * from SupportBeanCopyMethod");
            epService.EPAdministrator.CreateEPL("update istream ABCStreamCM set valOne = 'x', valTwo='y'");
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("select * from ABCStreamCM").Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBeanCopyMethod("1", "2"));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), "valOne,valTwo".Split(','), new object[] {"x", "y"});

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionSubquery(EPServiceProvider epService)
        {
            var type = new Dictionary<string, object>();
            type.Put("s0", typeof(string));
            type.Put("s1", typeof(int));
            epService.EPAdministrator.Configuration.AddEventType("MyMapTypeSelect", type);

            type = new Dictionary<string, object>();
            type.Put("w0", typeof(int));
            epService.EPAdministrator.Configuration.AddEventType("MyMapTypeWhere", type);

            string[] fields = "theString,intPrimitive".Split(',');
            epService.EPAdministrator.CreateEPL("insert into ABCStreamSQ select * from SupportBean");
            EPStatement stmtUpd = epService.EPAdministrator.CreateEPL(
                "update istream ABCStreamSQ set theString = (select s0 from MyMapTypeSelect#lastevent) where intPrimitive in (select w0 from MyMapTypeWhere#keepall)");
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("select * from ABCStreamSQ").Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E1", 0});

            epService.EPRuntime.SendEvent(MakeMap("w0", 1), "MyMapTypeWhere");
            epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {null, 1});

            epService.EPRuntime.SendEvent(new SupportBean("E3", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E3", 2});

            epService.EPRuntime.SendEvent(MakeMap("s0", "newvalue"), "MyMapTypeSelect");
            epService.EPRuntime.SendEvent(new SupportBean("E4", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"newvalue", 1});

            epService.EPRuntime.SendEvent(MakeMap("s0", "othervalue"), "MyMapTypeSelect");
            epService.EPRuntime.SendEvent(new SupportBean("E5", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"othervalue", 1});

            // test correlated subquery
            stmtUpd.Dispose();
            stmtUpd = epService.EPAdministrator.CreateEPL(
                "update istream ABCStreamSQ set intPrimitive = (select s1 from MyMapTypeSelect#keepall where s0 = ABCStreamSQ.theString)");

            // note that this will log an error (int primitive set to null), which is good, and leave the value unchanged
            epService.EPRuntime.SendEvent(new SupportBean("E6", 8));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E6", 8});

            epService.EPRuntime.SendEvent(MakeMap("s0", "E7", "s1", 91), "MyMapTypeSelect");
            epService.EPRuntime.SendEvent(new SupportBean("E7", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E7", 91});

            // test correlated with as-clause
            stmtUpd.Dispose();
            epService.EPAdministrator.CreateEPL(
                "update istream ABCStreamSQ as mystream set intPrimitive = (select s1 from MyMapTypeSelect#keepall where s0 = mystream.theString)");

            // note that this will log an error (int primitive set to null), which is good, and leave the value unchanged
            epService.EPRuntime.SendEvent(new SupportBean("E8", 111));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E8", 111});

            epService.EPRuntime.SendEvent(MakeMap("s0", "E9", "s1", -1), "MyMapTypeSelect");
            epService.EPRuntime.SendEvent(new SupportBean("E9", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E9", -1});

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionUnprioritizedOrder(EPServiceProvider epService)
        {
            var type = new Dictionary<string, object>();
            type.Put("s0", typeof(string));
            type.Put("s1", typeof(int));
            epService.EPAdministrator.Configuration.AddEventType("MyMapTypeUO", type);

            string[] fields = "s0,s1".Split(',');
            epService.EPAdministrator.CreateEPL("insert into ABCStreamUO select * from MyMapTypeUO");
            epService.EPAdministrator.CreateEPL("@Name('A') update istream ABCStreamUO set s0='A'");
            epService.EPAdministrator.CreateEPL("@Name('B') update istream ABCStreamUO set s0='B'");
            epService.EPAdministrator.CreateEPL("@Name('C') update istream ABCStreamUO set s0='C'");
            epService.EPAdministrator.CreateEPL("@Name('D') update istream ABCStreamUO set s0='D'");
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("select * from ABCStreamUO").Events += listener.Update;

            epService.EPRuntime.SendEvent(MakeMap("s0", "", "s1", 1), "MyMapTypeUO");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"D", 1});

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionListenerDeliveryMultiupdate(EPServiceProvider epService)
        {
            var listenerInsert = new SupportUpdateListener();
            var listeners = new SupportUpdateListener[5];
            for (int i = 0; i < listeners.Length; i++)
            {
                listeners[i] = new SupportUpdateListener();
            }

            string[] fields = "theString,intPrimitive,value1".Split(',');
            epService.EPAdministrator.CreateEPL("insert into ABCStreamLD select *, 'orig' as value1 from SupportBean")
                .Events += listenerInsert.Update;
            epService.EPAdministrator
                .CreateEPL(
                    "@Name('A') update istream ABCStreamLD set theString='A', value1='a' where intPrimitive in (1,2)")
                .Events += listeners[0].Update;
            epService.EPAdministrator
                .CreateEPL(
                    "@Name('B') update istream ABCStreamLD set theString='B', value1='b' where intPrimitive in (1,3)")
                .Events += listeners[1].Update;
            epService.EPAdministrator
                .CreateEPL(
                    "@Name('C') update istream ABCStreamLD set theString='C', value1='c' where intPrimitive in (2,3)")
                .Events += listeners[2].Update;
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("select * from ABCStreamLD").Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(
                listenerInsert.AssertOneGetNewAndReset(), fields, new object[] {"E1", 1, "orig"});
            EPAssertionUtil.AssertProps(listeners[0].AssertOneGetOld(), fields, new object[] {"E1", 1, "orig"});
            EPAssertionUtil.AssertProps(listeners[0].AssertOneGetNew(), fields, new object[] {"A", 1, "a"});
            EPAssertionUtil.AssertProps(listeners[1].AssertOneGetOld(), fields, new object[] {"A", 1, "a"});
            EPAssertionUtil.AssertProps(listeners[1].AssertOneGetNew(), fields, new object[] {"B", 1, "b"});
            Assert.IsFalse(listeners[2].IsInvoked);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"B", 1, "b"});
            Reset(listeners);

            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertProps(
                listenerInsert.AssertOneGetNewAndReset(), fields, new object[] {"E2", 2, "orig"});
            EPAssertionUtil.AssertProps(listeners[0].AssertOneGetOld(), fields, new object[] {"E2", 2, "orig"});
            EPAssertionUtil.AssertProps(listeners[0].AssertOneGetNew(), fields, new object[] {"A", 2, "a"});
            Assert.IsFalse(listeners[1].IsInvoked);
            EPAssertionUtil.AssertProps(listeners[2].AssertOneGetOld(), fields, new object[] {"A", 2, "a"});
            EPAssertionUtil.AssertProps(listeners[2].AssertOneGetNew(), fields, new object[] {"C", 2, "c"});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"C", 2, "c"});
            Reset(listeners);

            epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            EPAssertionUtil.AssertProps(
                listenerInsert.AssertOneGetNewAndReset(), fields, new object[] {"E3", 3, "orig"});
            Assert.IsFalse(listeners[0].IsInvoked);
            EPAssertionUtil.AssertProps(listeners[1].AssertOneGetOld(), fields, new object[] {"E3", 3, "orig"});
            EPAssertionUtil.AssertProps(listeners[1].AssertOneGetNew(), fields, new object[] {"B", 3, "b"});
            EPAssertionUtil.AssertProps(listeners[2].AssertOneGetOld(), fields, new object[] {"B", 3, "b"});
            EPAssertionUtil.AssertProps(listeners[2].AssertOneGetNew(), fields, new object[] {"C", 3, "c"});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"C", 3, "c"});
            Reset(listeners);

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionListenerDeliveryMultiupdateMixed(EPServiceProvider epService)
        {
            var listenerInsert = new SupportUpdateListener();
            var listeners = new SupportUpdateListener[5];
            for (int i = 0; i < listeners.Length; i++)
            {
                listeners[i] = new SupportUpdateListener();
            }

            string[] fields = "theString,intPrimitive,value1".Split(',');
            epService.EPAdministrator.CreateEPL("insert into ABCStreamLDM select *, 'orig' as value1 from SupportBean")
                .Events += listenerInsert.Update;
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("select * from ABCStreamLDM").Events += listener.Update;

            epService.EPAdministrator.CreateEPL("@Name('A') update istream ABCStreamLDM set theString='A', value1='a'");
            epService.EPAdministrator.CreateEPL("@Name('B') update istream ABCStreamLDM set theString='B', value1='b'")
                .Events += listeners[1].Update;
            epService.EPAdministrator.CreateEPL("@Name('C') update istream ABCStreamLDM set theString='C', value1='c'");
            epService.EPAdministrator.CreateEPL("@Name('D') update istream ABCStreamLDM set theString='D', value1='d'")
                .Events += listeners[3].Update;
            epService.EPAdministrator.CreateEPL("@Name('E') update istream ABCStreamLDM set theString='E', value1='e'");

            epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            EPAssertionUtil.AssertProps(
                listenerInsert.AssertOneGetNewAndReset(), fields, new object[] {"E4", 4, "orig"});
            Assert.IsFalse(listeners[0].IsInvoked);
            EPAssertionUtil.AssertProps(listeners[1].AssertOneGetOld(), fields, new object[] {"A", 4, "a"});
            EPAssertionUtil.AssertProps(listeners[1].AssertOneGetNew(), fields, new object[] {"B", 4, "b"});
            Assert.IsFalse(listeners[2].IsInvoked);
            EPAssertionUtil.AssertProps(listeners[3].AssertOneGetOld(), fields, new object[] {"C", 4, "c"});
            EPAssertionUtil.AssertProps(listeners[3].AssertOneGetNew(), fields, new object[] {"D", 4, "d"});
            Assert.IsFalse(listeners[4].IsInvoked);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E", 4, "e"});
            Reset(listeners);

            epService.EPAdministrator.GetStatement("B").RemoveAllEventHandlers();
            epService.EPAdministrator.GetStatement("D").RemoveAllEventHandlers();
            epService.EPAdministrator.GetStatement("A").Events += listeners[0].Update;
            epService.EPAdministrator.GetStatement("E").Events += listeners[4].Update;

            epService.EPRuntime.SendEvent(new SupportBean("E5", 5));
            EPAssertionUtil.AssertProps(
                listenerInsert.AssertOneGetNewAndReset(), fields, new object[] {"E5", 5, "orig"});
            EPAssertionUtil.AssertProps(listeners[0].AssertOneGetOld(), fields, new object[] {"E5", 5, "orig"});
            EPAssertionUtil.AssertProps(listeners[0].AssertOneGetNew(), fields, new object[] {"A", 5, "a"});
            Assert.IsFalse(listeners[1].IsInvoked);
            Assert.IsFalse(listeners[2].IsInvoked);
            Assert.IsFalse(listeners[3].IsInvoked);
            EPAssertionUtil.AssertProps(listeners[4].AssertOneGetOld(), fields, new object[] {"D", 5, "d"});
            EPAssertionUtil.AssertProps(listeners[4].AssertOneGetNew(), fields, new object[] {"E", 5, "e"});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E", 5, "e"});
            Reset(listeners);

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void Reset(SupportUpdateListener[] listeners)
        {
            foreach (SupportUpdateListener listener in listeners)
            {
                listener.Reset();
            }
        }

        private IDictionary<string, object> MakeMap(
            string prop1, object val1, string prop2, object val2, string prop3, object val3)
        {
            var map = new Dictionary<string, object>();
            map.Put(prop1, val1);
            map.Put(prop2, val2);
            map.Put(prop3, val3);
            return map;
        }

        private IDictionary<string, object> MakeMap(string prop1, object val1, string prop2, object val2)
        {
            var map = new Dictionary<string, object>();
            map.Put(prop1, val1);
            map.Put(prop2, val2);
            return map;
        }

        private IDictionary<string, object> MakeMap(string prop1, object val1)
        {
            var map = new Dictionary<string, object>();
            map.Put(prop1, val1);
            return map;
        }

        private SupportBean MakeSupportBean(string theString, int intPrimitive, double doublePrimitive)
        {
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

            public BaseOne(string i, string p)
            {
                this.I = i;
                this.P = p;
            }

            public string I { get; set; }

            public string P { get; set; }
        }

        [Serializable]
        public class BaseTwo : BaseInterface
        {

            public BaseTwo()
            {
            }

            public BaseTwo(string p)
            {
                P = p;
            }

            public string P { get; set; }

            public string I { get; set; }
        }

        public class BaseOneA : BaseOne
        {
            public BaseOneA()
            {
            }

            public BaseOneA(string i, string p, string pa) : base(i, p)
            {
                Pa = pa;
            }

            public string Pa { get; set; }
        }

        public class BaseOneB : BaseOne
        {
            public BaseOneB()
            {
            }

            public BaseOneB(string i, string p, string pb) : base(i, p)
            {
                Pb = pb;
            }

            public string Pb { get; set; }
        }

        public static void SetIntBoxedValue(SupportBean sb, int value)
        {
            sb.IntBoxed = value;
        }
    }
} // end of namespace
