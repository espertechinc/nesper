///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using Avro;
using Avro.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util;

using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;

using NUnit.Framework;

using static NEsper.Avro.Extensions.TypeBuilder;
// using static org.apache.avro.SchemaBuilder.record;

namespace com.espertech.esper.regression.epl.insertinto
{
    public class ExecInsertIntoPopulateUndStreamSelect : RegressionExecution
    {
        public override void Run(EPServiceProvider epService)
        {
            RunAssertionNamedWindowInheritsMap(epService);
            RunAssertionNamedWindowRep(epService);
            RunAssertionStreamInsertWWidenOA(epService);
            RunAssertionInvalid(epService);
        }

        private void RunAssertionNamedWindowInheritsMap(EPServiceProvider epService)
        {
            var epl = "create objectarray schema Event();\n" +
                      "create objectarray schema ChildEvent(id string, action string) inherits Event;\n" +
                      "create objectarray schema Incident(name string, event Event);\n" +
                      "@Name('window') create window IncidentWindow#keepall as Incident;\n" +
                      "\n" +
                      "on ChildEvent e\n" +
                      "    merge IncidentWindow w\n" +
                      "    where e.id = Cast(w.event.id? as string)\n" +
                      "    when not matched\n" +
                      "        then insert (name, event) select 'ChildIncident', e \n" +
                      "            where e.action = 'INSERT'\n" +
                      "    when matched\n" +
                      "        then update set w.event = e \n" +
                      "            where e.action = 'INSERT'\n" +
                      "        then delete\n" +
                      "            where e.action = 'CLEAR';";
            var deployed = epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);

            epService.EPRuntime.SendEvent(new object[] {"ID1", "INSERT"}, "ChildEvent");
            var @event = epService.EPAdministrator.GetStatement("window").First();
            var underlying = (object[]) @event.Underlying;
            Assert.AreEqual("ChildIncident", underlying[0]);
            var underlyingInner = (object[]) ((EventBean) underlying[1]).Underlying;
            EPAssertionUtil.AssertEqualsExactOrder(new object[] {"ID1", "INSERT"}, underlyingInner);

            epService.EPAdministrator.DeploymentAdmin.UndeployRemove(deployed.DeploymentId);
        }

        private void RunAssertionNamedWindowRep(EPServiceProvider epService)
        {
            foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>())
            {
                TryAssertionNamedWindow(epService, rep);
            }
        }

        private void RunAssertionStreamInsertWWidenOA(EPServiceProvider epService)
        {
            foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>())
            {
                TryAssertionStreamInsertWWidenMap(epService, rep);
            }
        }

        private void RunAssertionInvalid(EPServiceProvider epService)
        {
            foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>())
            {
                TryAssertionInvalid(epService, rep);
            }
        }

        private void TryAssertionNamedWindow(EPServiceProvider epService, EventRepresentationChoice rep)
        {
            if (rep.IsMapEvent())
            {
                var typeinfo = new Dictionary<string, object>();
                typeinfo.Put("myint", typeof(int));
                typeinfo.Put("mystr", typeof(string));
                epService.EPAdministrator.Configuration.AddEventType("A", typeinfo);
                epService.EPAdministrator.CreateEPL(
                    "create " + rep.GetOutputTypeCreateSchemaName() + " schema C as (addprop int) inherits A");
            }
            else if (rep.IsObjectArrayEvent())
            {
                epService.EPAdministrator.Configuration.AddEventType(
                    "A", new[] {"myint", "mystr"}, new object[] {typeof(int), typeof(string)});
                epService.EPAdministrator.CreateEPL("create objectarray schema C as (addprop int) inherits A");
            }
            else if (rep.IsAvroEvent())
            {
                var schemaA = SchemaBuilder.Record("A",
                    RequiredInt("myint"),
                    RequiredString("mystr"));
                epService.EPAdministrator.Configuration.AddEventTypeAvro(
                    "A", new ConfigurationEventTypeAvro().SetAvroSchema(schemaA));
                epService.EPAdministrator.CreateEPL("create avro schema C as (addprop int) inherits A");
            }
            else
            {
                Assert.Fail();
            }

            epService.EPAdministrator.CreateEPL("create window MyWindow#time(5 days) as C");
            var stmt = epService.EPAdministrator.CreateEPL("select * from MyWindow");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            // select underlying
            var stmtInsert = epService.EPAdministrator.CreateEPL("insert into MyWindow select mya.* from A as mya");
            if (rep.IsMapEvent())
            {
                epService.EPRuntime.SendEvent(MakeMap(123, "abc"), "A");
            }
            else if (rep.IsObjectArrayEvent())
            {
                epService.EPRuntime.SendEvent(new object[] {123, "abc"}, "A");
            }
            else if (rep.IsAvroEvent())
            {
                epService.EPRuntime.SendEventAvro(MakeAvro(epService, 123, "abc"), "A");
            }
            else
            {
                Assert.Fail();
            }

            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), "myint,mystr,addprop".Split(','), new object[] {123, "abc", null});
            stmtInsert.Dispose();

            // select underlying plus property
            epService.EPAdministrator.CreateEPL("insert into MyWindow select mya.*, 1 as addprop from A as mya");
            if (rep.IsMapEvent())
            {
                epService.EPRuntime.SendEvent(MakeMap(456, "def"), "A");
            }
            else if (rep.IsObjectArrayEvent())
            {
                epService.EPRuntime.SendEvent(new object[] {456, "def"}, "A");
            }
            else if (rep.IsAvroEvent())
            {
                epService.EPRuntime.SendEventAvro(MakeAvro(epService, 456, "def"), "A");
            }

            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), "myint,mystr,addprop".Split(','), new object[] {456, "def", 1});

            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyWindow", false);
            epService.EPAdministrator.Configuration.RemoveEventType("A", false);
            epService.EPAdministrator.Configuration.RemoveEventType("C", false);
        }

        private void TryAssertionStreamInsertWWidenMap(EPServiceProvider epService, EventRepresentationChoice rep)
        {
            epService.EPAdministrator.CreateEPL(
                "create " + rep.GetOutputTypeCreateSchemaName() + " schema Src as (myint int, mystr string)");

            epService.EPAdministrator.CreateEPL(
                "create " + rep.GetOutputTypeCreateSchemaName() +
                " schema D1 as (myint int, mystr string, addprop long)");
            var eplOne = "insert into D1 select 1 as addprop, mysrc.* from Src as mysrc";
            RunStreamInsertAssertion(epService, rep, eplOne, "myint,mystr,addprop", new object[] {123, "abc", 1L});

            epService.EPAdministrator.CreateEPL(
                "create " + rep.GetOutputTypeCreateSchemaName() +
                " schema D2 as (mystr string, myint int, addprop double)");
            var eplTwo = "insert into D2 select 1 as addprop, mysrc.* from Src as mysrc";
            RunStreamInsertAssertion(epService, rep, eplTwo, "myint,mystr,addprop", new object[] {123, "abc", 1d});

            epService.EPAdministrator.CreateEPL(
                "create " + rep.GetOutputTypeCreateSchemaName() + " schema D3 as (mystr string, addprop int)");
            var eplThree = "insert into D3 select 1 as addprop, mysrc.* from Src as mysrc";
            RunStreamInsertAssertion(epService, rep, eplThree, "mystr,addprop", new object[] {"abc", 1});

            epService.EPAdministrator.CreateEPL(
                "create " + rep.GetOutputTypeCreateSchemaName() + " schema D4 as (myint int, mystr string)");
            var eplFour = "insert into D4 select mysrc.* from Src as mysrc";
            RunStreamInsertAssertion(epService, rep, eplFour, "myint,mystr", new object[] {123, "abc"});

            var eplFive = "insert into D4 select mysrc.*, 999 as myint, 'xxx' as mystr from Src as mysrc";
            RunStreamInsertAssertion(epService, rep, eplFive, "myint,mystr", new object[] {999, "xxx"});
            var eplSix = "insert into D4 select 999 as myint, 'xxx' as mystr, mysrc.* from Src as mysrc";
            RunStreamInsertAssertion(epService, rep, eplSix, "myint,mystr", new object[] {999, "xxx"});

            epService.EPAdministrator.DestroyAllStatements();
            foreach (var name in Collections.List("Src", "D1", "D2", "D3", "D4"))
            {
                epService.EPAdministrator.Configuration.RemoveEventType(name, false);
            }
        }

        private void TryAssertionInvalid(EPServiceProvider epService, EventRepresentationChoice rep)
        {
            epService.EPAdministrator.CreateEPL(
                "create " + rep.GetOutputTypeCreateSchemaName() + " schema Src as (myint int, mystr string)");

            // mismatch in type
            epService.EPAdministrator.CreateEPL(
                "create " + rep.GetOutputTypeCreateSchemaName() + " schema E1 as (myint long)");
            var message = !rep.IsAvroEvent()
                ? "Error starting statement: Type by name 'E1' in property 'myint' expected " + Name.Clean<int>() + " but receives " + Name.Clean<long>()
                : "Error starting statement: Type by name 'E1' in property 'myint' expected schema '{\"type\":\"long\"}' but received schema '{\"type\":\"int\"}'";
            SupportMessageAssertUtil.TryInvalid(epService, "insert into E1 select mysrc.* from Src as mysrc", message);

            // mismatch in column name
            epService.EPAdministrator.CreateEPL(
                "create " + rep.GetOutputTypeCreateSchemaName() + " schema E2 as (someprop long)");
            SupportMessageAssertUtil.TryInvalid(
                epService, "insert into E2 select mysrc.*, 1 as otherprop from Src as mysrc",
                "Error starting statement: Failed to find column 'otherprop' in target type 'E2' [insert into E2 select mysrc.*, 1 as otherprop from Src as mysrc]");

            epService.EPAdministrator.DestroyAllStatements();
            foreach (var name in Collections.List("Src", "E1", "E2"))
            {
                epService.EPAdministrator.Configuration.RemoveEventType(name, false);
            }
        }

        private void RunStreamInsertAssertion(
            EPServiceProvider epService, EventRepresentationChoice rep, string epl, string fields, object[] expected)
        {
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            if (rep.IsMapEvent())
            {
                epService.EPRuntime.SendEvent(MakeMap(123, "abc"), "Src");
            }
            else if (rep.IsObjectArrayEvent())
            {
                epService.EPRuntime.SendEvent(new object[] {123, "abc"}, "Src");
            }
            else if (rep.IsAvroEvent())
            {
                var @event = new GenericRecord(SupportAvroUtil.GetAvroSchema(epService, "Src").AsRecordSchema());
                @event.Put("myint", 123);
                @event.Put("mystr", "abc");
                epService.EPRuntime.SendEventAvro(@event, "Src");
            }

            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields.Split(','), expected);
            stmt.Dispose();
        }

        private IDictionary<string, object> MakeMap(int myint, string mystr)
        {
            IDictionary<string, object> @event = new Dictionary<string, object>();
            @event.Put("myint", myint);
            @event.Put("mystr", mystr);
            return @event;
        }

        private GenericRecord MakeAvro(EPServiceProvider epService, int myint, string mystr)
        {
            var record = new GenericRecord(SupportAvroUtil.GetAvroSchema(epService, "A").AsRecordSchema());
            record.Put("myint", myint);
            record.Put("mystr", mystr);
            return record;
        }
    }
} // end of namespace