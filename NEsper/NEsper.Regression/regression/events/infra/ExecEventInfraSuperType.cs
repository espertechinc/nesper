///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using Avro;
using Avro.Generic;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.events;
using com.espertech.esper.supportregression.execution;
using NEsper.Avro.Extensions;
using static com.espertech.esper.client.scopetest.SupportUpdateListener;
using static com.espertech.esper.supportregression.events.SupportEventInfra;

using NUnit.Framework;

namespace com.espertech.esper.regression.events.infra
{
    public class ExecEventInfraSuperType : RegressionExecution
    {

        public override void Run(EPServiceProvider epService)
        {
            AddMapEventTypes(epService);
            AddOAEventTypes(epService);
            AddAvroEventTypes(epService);
            AddBeanTypes(epService);

            // Bean
            RunAssertion(
                epService, "Bean", FBEANWTYPE, new Bean_Type_Root(), new Bean_Type_1(), new Bean_Type_2(),
                new Bean_Type_2_1());

            // Map
            RunAssertion(
                epService, "Map", FMAPWTYPE, new Dictionary<string, object>(), new Dictionary<string, object>(),
                new Dictionary<string, object>(), new Dictionary<string, object>());

            // OA
            RunAssertion(epService, "OA", FOAWTYPE, new Object[0], new Object[0], new Object[0], new Object[0]);

            // Avro
            var fake = SchemaBuilder.Record("fake");
            RunAssertion(
                epService, "Avro", FAVROWTYPE, new GenericRecord(fake), new GenericRecord(fake),
                new GenericRecord(fake), new GenericRecord(fake));
        }

        private void RunAssertion(
            EPServiceProvider epService,
            string typePrefix,
            FunctionSendEventWType sender,
            Object root, Object type_1, Object type_2, Object type_2_1)
        {

            string[] typeNames = "Type_Root,Type_1,Type_2,Type_2_1".Split(',');
            var statements = new EPStatement[4];
            var listeners = new SupportUpdateListener[4];
            for (int i = 0; i < typeNames.Length; i++)
            {
                statements[i] = epService.EPAdministrator.CreateEPL("select * from " + typePrefix + "_" + typeNames[i]);
                listeners[i] = new SupportUpdateListener();
                statements[i].Events += listeners[i].Update;
            }

            sender.Invoke(epService, root, typePrefix + "_" + typeNames[0]);
            EPAssertionUtil.AssertEqualsExactOrder(
                new bool[] {true, false, false, false}, GetInvokedFlagsAndReset(listeners));

            sender.Invoke(epService, type_1, typePrefix + "_" + typeNames[1]);
            EPAssertionUtil.AssertEqualsExactOrder(
                new bool[] {true, true, false, false}, GetInvokedFlagsAndReset(listeners));

            sender.Invoke(epService, type_2, typePrefix + "_" + typeNames[2]);
            EPAssertionUtil.AssertEqualsExactOrder(
                new bool[] {true, false, true, false}, GetInvokedFlagsAndReset(listeners));

            sender.Invoke(epService, type_2_1, typePrefix + "_" + typeNames[3]);
            EPAssertionUtil.AssertEqualsExactOrder(
                new bool[] {true, false, true, true}, GetInvokedFlagsAndReset(listeners));

            for (int i = 0; i < statements.Length; i++)
            {
                statements[i].Dispose();
            }
        }

        private void AddMapEventTypes(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddEventType("Map_Type_Root", Collections.EmptyDataMap);
            epService.EPAdministrator.Configuration.AddEventType(
                "Map_Type_1", Collections.EmptyDataMap, new string[] {"Map_Type_Root"});
            epService.EPAdministrator.Configuration.AddEventType(
                "Map_Type_2", Collections.EmptyDataMap, new string[] {"Map_Type_Root"});
            epService.EPAdministrator.Configuration.AddEventType(
                "Map_Type_2_1", Collections.EmptyDataMap, new string[] {"Map_Type_2"});
        }

        private void AddOAEventTypes(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddEventType("OA_Type_Root", new string[0], new Object[0]);

            var array_1 = new ConfigurationEventTypeObjectArray();
            array_1.SuperTypes = Collections.SingletonSet("OA_Type_Root");
            epService.EPAdministrator.Configuration.AddEventType("OA_Type_1", new string[0], new Object[0], array_1);

            var array_2 = new ConfigurationEventTypeObjectArray();
            array_2.SuperTypes = Collections.SingletonSet("OA_Type_Root");
            epService.EPAdministrator.Configuration.AddEventType("OA_Type_2", new string[0], new Object[0], array_2);

            var array_2_1 = new ConfigurationEventTypeObjectArray();
            array_2_1.SuperTypes = Collections.SingletonSet("OA_Type_2");
            epService.EPAdministrator.Configuration.AddEventType(
                "OA_Type_2_1", new string[0], new Object[0], array_2_1);
        }

        private void AddAvroEventTypes(EPServiceProvider epService)
        {
            var fake = SchemaBuilder.Record("fake");
            var avro_root = new ConfigurationEventTypeAvro();
            avro_root.AvroSchema = fake;
            epService.EPAdministrator.Configuration.AddEventTypeAvro("Avro_Type_Root", avro_root);
            var avro_1 = new ConfigurationEventTypeAvro();
            avro_1.SuperTypes = Collections.SingletonSet("Avro_Type_Root");
            avro_1.AvroSchema = fake;
            epService.EPAdministrator.Configuration.AddEventTypeAvro("Avro_Type_1", avro_1);
            var avro_2 = new ConfigurationEventTypeAvro();
            avro_2.SuperTypes = Collections.SingletonSet("Avro_Type_Root");
            avro_2.AvroSchema = fake;
            epService.EPAdministrator.Configuration.AddEventTypeAvro("Avro_Type_2", avro_2);
            var avro_2_1 = new ConfigurationEventTypeAvro();
            avro_2_1.SuperTypes = Collections.SingletonSet("Avro_Type_2");
            avro_2_1.AvroSchema = fake;
            epService.EPAdministrator.Configuration.AddEventTypeAvro("Avro_Type_2_1", avro_2_1);
        }

        private void AddBeanTypes(EPServiceProvider epService)
        {
            foreach (var clazz in Collections.List(
                typeof(Bean_Type_Root), typeof(Bean_Type_1), typeof(Bean_Type_2), typeof(Bean_Type_2_1)))
            {
                epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
        }

        public class Bean_Type_Root
        {
        }

        public class Bean_Type_1 : Bean_Type_Root
        {
        }

        public class Bean_Type_2 : Bean_Type_Root
        {
        }

        public class Bean_Type_2_1 : Bean_Type_2
        {
        }
    }
} // end of namespace
