///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using Avro.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl.join
{
    public class ExecJoinEventRepresentation : RegressionExecution
    {
        public override void Configure(Configuration configuration)
        {
            var typeInfo = new Dictionary<string, object>();
            typeInfo.Put("id", typeof(string));
            typeInfo.Put("p00", typeof(int));
            configuration.AddEventType("MapS0", typeInfo);
            configuration.AddEventType("MapS1", typeInfo);
            configuration.EngineDefaults.Logging.IsEnableQueryPlan = true;
        }

        public override void Run(EPServiceProvider epService)
        {
            RunAssertionJoinEventRepresentations(epService);
            RunAssertionJoinMapEventNotUnique(epService);
            RunAssertionJoinWrapperEventNotUnique(epService);
        }

        private void RunAssertionJoinEventRepresentations(EPServiceProvider epService)
        {
            var eplOne =
                "select S0.id as S0_id, S1.id as S1_id, S0.p00 as S0_p00, S1.p00 as S1_p00 from S0#keepall as S0, S1#keepall as S1 where S0.id = S1.id";
            foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>())
            {
                TryJoinAssertion(epService, eplOne, rep, "S0_id,S1_id,S0_p00,S1_p00");
            }

            var eplTwo = "select * from S0#keepall as S0, S1#keepall as S1 where S0.id = S1.id";
            foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>())
            {
                TryJoinAssertion(epService, eplTwo, rep, "S0.id,S1.id,S0.p00,S1.p00");
            }
        }

        private void TryJoinAssertion(
            EPServiceProvider epService, string epl, EventRepresentationChoice rep, string columnNames)
        {
            if (rep.IsMapEvent())
            {
                var typeInfo = new Dictionary<string, object>();
                typeInfo.Put("id", typeof(string));
                typeInfo.Put("p00", typeof(int));
                epService.EPAdministrator.Configuration.AddEventType("S0", typeInfo);
                epService.EPAdministrator.Configuration.AddEventType("S1", typeInfo);
            }
            else if (rep.IsObjectArrayEvent())
            {
                var names = "id,p00".Split(',');
                var types = new object[] {typeof(string), typeof(int)};
                epService.EPAdministrator.Configuration.AddEventType("S0", names, types);
                epService.EPAdministrator.Configuration.AddEventType("S1", names, types);
            }
            else if (rep.IsAvroEvent())
            {
                var schema = SchemaBuilder.Record(
                    "name",
                    TypeBuilder.Field(
                        "id", TypeBuilder.StringType(
                            TypeBuilder.Property(AvroConstant.PROP_STRING_KEY, AvroConstant.PROP_STRING_VALUE))),
                    TypeBuilder.RequiredInt("p00"));
                epService.EPAdministrator.Configuration.AddEventTypeAvro(
                    "S0", new ConfigurationEventTypeAvro().SetAvroSchema(schema));
                epService.EPAdministrator.Configuration.AddEventTypeAvro(
                    "S1", new ConfigurationEventTypeAvro().SetAvroSchema(schema));
            }

            var stmt = epService.EPAdministrator.CreateEPL(rep.GetAnnotationText() + epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            SendRepEvent(epService, rep, "S0", "a", 1);
            Assert.IsFalse(listener.IsInvoked);

            SendRepEvent(epService, rep, "S1", "a", 2);
            var output = listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(output, columnNames.Split(','), new object[] {"a", "a", 1, 2});
            Assert.IsTrue(rep.MatchesClass(output.Underlying.GetType()));

            SendRepEvent(epService, rep, "S1", "b", 3);
            SendRepEvent(epService, rep, "S0", "c", 4);
            Assert.IsFalse(listener.IsInvoked);

            stmt.Dispose();
            epService.EPAdministrator.Configuration.RemoveEventType("S0", true);
            epService.EPAdministrator.Configuration.RemoveEventType("S1", true);
        }

        private void RunAssertionJoinMapEventNotUnique(EPServiceProvider epService)
        {
            // Test for Esper-122
            var joinStatement = "select S0.id, S1.id, S0.p00, S1.p00 from MapS0#keepall as S0, MapS1#keepall as S1" +
                                " where S0.id = S1.id";

            var stmt = epService.EPAdministrator.CreateEPL(joinStatement);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            for (var i = 0; i < 100; i++)
            {
                if (i % 2 == 1)
                {
                    SendMapEvent(epService, "MapS0", "a", 1);
                }
                else
                {
                    SendMapEvent(epService, "MapS1", "a", 1);
                }
            }

            stmt.Dispose();
        }

        private void RunAssertionJoinWrapperEventNotUnique(EPServiceProvider epService)
        {
            // Test for Esper-122
            epService.EPAdministrator.CreateEPL(
                "insert into S0 select 's0' as streamone, * from " + typeof(SupportBean).FullName);
            epService.EPAdministrator.CreateEPL(
                "insert into S1 select 's1' as streamtwo, * from " + typeof(SupportBean).FullName);
            var joinStatement = "select * from S0#keepall as a, S1#keepall as b where a.IntBoxed = b.IntBoxed";

            var stmt = epService.EPAdministrator.CreateEPL(joinStatement);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            for (var i = 0; i < 100; i++)
            {
                epService.EPRuntime.SendEvent(new SupportBean());
            }

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void SendMapEvent(EPServiceProvider epService, string name, string id, int p00)
        {
            var theEvent = new Dictionary<string, object>();
            theEvent.Put("id", id);
            theEvent.Put("p00", p00);
            epService.EPRuntime.SendEvent(theEvent, name);
        }

        private void SendRepEvent(
            EPServiceProvider epService, EventRepresentationChoice rep, string name, string id, int p00)
        {
            if (rep.IsMapEvent())
            {
                var theEvent = new Dictionary<string, object>();
                theEvent.Put("id", id);
                theEvent.Put("p00", p00);
                epService.EPRuntime.SendEvent(theEvent, name);
            }
            else if (rep.IsObjectArrayEvent())
            {
                epService.EPRuntime.SendEvent(new object[] {id, p00}, name);
            }
            else if (rep.IsAvroEvent())
            {
                var theEvent = new GenericRecord(SupportAvroUtil.GetAvroSchema(epService, name).AsRecordSchema());
                theEvent.Put("id", id);
                theEvent.Put("p00", p00);
                epService.EPRuntime.SendEventAvro(theEvent, name);
            }
            else
            {
                Assert.Fail();
            }
        }
    }
} // end of namespace