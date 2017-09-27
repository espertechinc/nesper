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

using Avro;
using Avro.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util;

using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestInsertIntoPopulateUndStreamSelect
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp() {
            var configuration = SupportConfigFactory.GetConfiguration();
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName);
            }
            _listener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.EndTest();
            }
            _listener = null;
        }
    
        [Test]
        public void TestNamedWindowInheritsMap() {
            var epl = "create objectarray schema Event();\n" +
                         "create objectarray schema ChildEvent(id string, action string) inherits Event;\n" +
                         "create objectarray schema Incident(name string, event Event);\n" +
                         "@Name('window') create window IncidentWindow#keepall as Incident;\n" +
                         "\n" +
                         "on ChildEvent e\n" +
                         "    merge IncidentWindow w\n" +
                         "    where e.id = cast(w.event.id? as string)\n" +
                         "    when not matched\n" +
                         "        then insert (name, event) select 'ChildIncident', e \n" +
                         "            where e.action = 'INSERT'\n" +
                         "    when matched\n" +
                         "        then update set w.event = e \n" +
                         "            where e.action = 'INSERT'\n" +
                         "        then delete\n" +
                         "            where e.action = 'CLEAR';";
            _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
    
            _epService.EPRuntime.SendEvent(new Object[] {"ID1", "INSERT"}, "ChildEvent");
            var @event = _epService.EPAdministrator.GetStatement("window").First();
            var underlying = @event.Underlying.UnwrapIntoArray<object>();
            Assert.AreEqual("ChildIncident", underlying[0]);
            var underlyingInner = ((EventBean) underlying[1]).Underlying.UnwrapIntoArray<object>();
            EPAssertionUtil.AssertEqualsExactOrder(new Object[] {"ID1", "INSERT"}, underlyingInner);
        }
    
        [Test]
        public void TestNamedWindowRep() {
            EnumHelper.ForEach<EventRepresentationChoice>(rep => RunAssertionNamedWindow(rep));
        }
    
        [Test]
        public void TestStreamInsertWWidenOA() {
            EnumHelper.ForEach<EventRepresentationChoice>(rep => RunAssertionStreamInsertWWidenMap(rep));
        }
    
        [Test]
        public void TestInvalid() {
            EnumHelper.ForEach<EventRepresentationChoice>(rep => RunAssertionInvalid(rep));
        }
    
        private void RunAssertionNamedWindow(EventRepresentationChoice rep) {
            if (rep.IsMapEvent()) {
                var typeinfo = new Dictionary<string, Object>();
                typeinfo.Put("myint", typeof(int));
                typeinfo.Put("mystr", typeof(string));
                _epService.EPAdministrator.Configuration.AddEventType("A", typeinfo);
                _epService.EPAdministrator.CreateEPL("create " + rep.GetOutputTypeCreateSchemaName() + " schema C as (addprop int) inherits A");
            } else if (rep.IsObjectArrayEvent()) {
                _epService.EPAdministrator.Configuration.AddEventType("A", new string[] {"myint", "mystr"}, new Object[] {typeof(int), typeof(string)});
                _epService.EPAdministrator.CreateEPL("create objectarray schema C as (addprop int) inherits A");
            } else if (rep.IsAvroEvent()) {
                Schema schemaA = SchemaBuilder.Record("A",
                    TypeBuilder.RequiredInt("myint"),
                    TypeBuilder.RequiredString("mystr"));
                    
                // Record("A").Fields().RequiredInt("myint").RequiredString("mystr").EndRecord();
                _epService.EPAdministrator.Configuration.AddEventTypeAvro("A", new ConfigurationEventTypeAvro().SetAvroSchema(schemaA));
                _epService.EPAdministrator.CreateEPL("create avro schema C as (addprop int) inherits A");
            } else {
                Assert.Fail();
            }
    
            _epService.EPAdministrator.CreateEPL("create window MyWindow#time(5 days) as C");
            var stmt = _epService.EPAdministrator.CreateEPL("select * from MyWindow");
            stmt.AddListener(_listener);
    
            // select underlying
            var stmtInsert = _epService.EPAdministrator.CreateEPL("insert into MyWindow select mya.* from A as mya");
            if (rep.IsMapEvent()) {
                _epService.EPRuntime.SendEvent(MakeMap(123, "abc"), "A");
            } else if (rep.IsObjectArrayEvent()) {
                _epService.EPRuntime.SendEvent(new Object[] {123, "abc"}, "A");
            } else if (rep.IsAvroEvent()) {
                _epService.EPRuntime.SendEventAvro(MakeAvro(123, "abc"), "A");
            } else {
                Assert.Fail();
            }
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "myint,mystr,addprop".SplitCsv(), new Object[] {123, "abc", null});
            stmtInsert.Dispose();
    
            // select underlying plus property
            _epService.EPAdministrator.CreateEPL("insert into MyWindow select mya.*, 1 as addprop from A as mya");
            if (rep.IsMapEvent()) {
                _epService.EPRuntime.SendEvent(MakeMap(456, "def"), "A");
            } else if (rep.IsObjectArrayEvent()) {
                _epService.EPRuntime.SendEvent(new Object[] {456, "def"}, "A");
            } else if (rep.IsAvroEvent()) {
                _epService.EPRuntime.SendEventAvro(MakeAvro(456, "def"), "A");
            }
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "myint,mystr,addprop".SplitCsv(), new Object[] {456, "def", 1});
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyWindow", false);
            _epService.EPAdministrator.Configuration.RemoveEventType("A", false);
            _epService.EPAdministrator.Configuration.RemoveEventType("C", false);
        }
    
        private void RunAssertionStreamInsertWWidenMap(EventRepresentationChoice rep) {
    
            EPServiceProvider epService = EPServiceProviderManager.GetDefaultProvider();
            epService.EPAdministrator.CreateEPL("create " + rep.GetOutputTypeCreateSchemaName() + " schema Src as (myint int, mystr string)");
    
            epService.EPAdministrator.CreateEPL("create " + rep.GetOutputTypeCreateSchemaName() + " schema D1 as (myint int, mystr string, addprop long)");
            var eplOne = "insert into D1 select 1 as addprop, mysrc.* from Src as mysrc";
            RunStreamInsertAssertion(rep, eplOne, "myint,mystr,addprop", new Object[] {123, "abc", 1L});
    
            epService.EPAdministrator.CreateEPL("create " + rep.GetOutputTypeCreateSchemaName() + " schema D2 as (mystr string, myint int, addprop double)");
            var eplTwo = "insert into D2 select 1 as addprop, mysrc.* from Src as mysrc";
            RunStreamInsertAssertion(rep, eplTwo, "myint,mystr,addprop", new Object[] {123, "abc", 1d});
    
            epService.EPAdministrator.CreateEPL("create " + rep.GetOutputTypeCreateSchemaName() + " schema D3 as (mystr string, addprop int)");
            var eplThree = "insert into D3 select 1 as addprop, mysrc.* from Src as mysrc";
            RunStreamInsertAssertion(rep, eplThree, "mystr,addprop", new Object[] {"abc", 1});
    
            epService.EPAdministrator.CreateEPL("create " + rep.GetOutputTypeCreateSchemaName() + " schema D4 as (myint int, mystr string)");
            var eplFour = "insert into D4 select mysrc.* from Src as mysrc";
            RunStreamInsertAssertion(rep, eplFour, "myint,mystr", new Object[] {123, "abc"});
    
            var eplFive = "insert into D4 select mysrc.*, 999 as myint, 'xxx' as mystr from Src as mysrc";
            RunStreamInsertAssertion(rep, eplFive, "myint,mystr", new Object[] {999, "xxx"});
            var eplSix = "insert into D4 select 999 as myint, 'xxx' as mystr, mysrc.* from Src as mysrc";
            RunStreamInsertAssertion(rep, eplSix, "myint,mystr", new Object[] {999, "xxx"});
    
            epService.EPAdministrator.DestroyAllStatements();
            foreach (string name in  new[]{ "Src", "D1", "D2", "D3", "D4" }) {
                epService.EPAdministrator.Configuration.RemoveEventType(name, false);
            }
        }
    
        private void RunAssertionInvalid(EventRepresentationChoice rep) {
            _epService.EPAdministrator.CreateEPL("create " + rep.GetOutputTypeCreateSchemaName() + " schema Src as (myint int, mystr string)");
    
            // mismatch in type
            _epService.EPAdministrator.CreateEPL("create " + rep.GetOutputTypeCreateSchemaName() + " schema E1 as (myint long)");
            var message = !rep.IsAvroEvent() ?
                             "Error starting statement: Type by name 'E1' in property 'myint' expected " + Name.Of<int>() + " but receives " + Name.Of<long>() :
                             "Error starting statement: Type by name 'E1' in property 'myint' expected schema '{\"type\":\"long\"}' but received schema '{\"type\":\"int\"}'";
            SupportMessageAssertUtil.TryInvalid(_epService, "insert into E1 select mysrc.* from Src as mysrc", message);
    
            // mismatch in column name
            _epService.EPAdministrator.CreateEPL("create " + rep.GetOutputTypeCreateSchemaName() + " schema E2 as (someprop long)");
            SupportMessageAssertUtil.TryInvalid(_epService, "insert into E2 select mysrc.*, 1 as otherprop from Src as mysrc",
                                                "Error starting statement: Failed to find column 'otherprop' in target type 'E2' [insert into E2 select mysrc.*, 1 as otherprop from Src as mysrc]");
    
            _epService.EPAdministrator.DestroyAllStatements();
            foreach (string name in new[] { "Src", "E1", "E2" }) {
                _epService.EPAdministrator.Configuration.RemoveEventType(name, false);
            }
        }
    
        private void RunStreamInsertAssertion(EventRepresentationChoice rep, string epl, string fields, Object[] expected) {
            var stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.AddListener(_listener);
            if (rep.IsMapEvent()) {
                _epService.EPRuntime.SendEvent(MakeMap(123, "abc"), "Src");
            } else if (rep.IsObjectArrayEvent()) {
                _epService.EPRuntime.SendEvent(new Object[] {123, "abc"}, "Src");
            } else if (rep.IsAvroEvent()) {
                GenericRecord @event = new GenericRecord(SupportAvroUtil.GetAvroSchema(_epService, "Src").AsRecordSchema());
                @event.Put("myint", 123);
                @event.Put("mystr", "abc");
                _epService.EPRuntime.SendEventAvro(@event, "Src");
            }
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields.SplitCsv(), expected);
            stmt.Dispose();
        }
    
        private IDictionary<string, Object> MakeMap(int myint, string mystr) {
            IDictionary<string, Object> @event = new Dictionary<string, Object>();
            @event.Put("myint", myint);
            @event.Put("mystr", mystr);
            return @event;
        }
    
        private GenericRecord MakeAvro(int myint, string mystr) {
            var record = new GenericRecord(SupportAvroUtil.GetAvroSchema(_epService, "A").AsRecordSchema());
            record.Put("myint", myint);
            record.Put("mystr", mystr);
            return record;
        }
    }
} // end of namespace
