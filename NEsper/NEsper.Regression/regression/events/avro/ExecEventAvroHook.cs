///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;
using Avro;
using Avro.Generic;
using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.client.hook;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util;
using Newtonsoft.Json.Linq;
using NEsper.Avro.Core;
using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;

// using static org.apache.avro.SchemaBuilder.*;

using NUnit.Framework;

namespace com.espertech.esper.regression.events.avro
{
    public class ExecEventAvroHook : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.EventMeta.AvroSettings.TypeRepresentationMapperClass = typeof(MyTypeRepresentationMapper).FullName;
            configuration.EngineDefaults.EventMeta.AvroSettings.ObjectValueTypeWidenerFactoryClass = typeof(MyObjectValueTypeWidenerFactory).FullName;
        }
    
        public override void Run(EPServiceProvider epService)
        {
            var clazzes = Collections.List(
                typeof(SupportBean),
                typeof(SupportBean_S0));
            foreach (var clazz in clazzes) {
                epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
    
            RunAssertionSimpleWriteablePropertyCoerce(epService);
            RunAssertionSchemaFromClass(epService);
            RunAssertionPopulate(epService);
            RunAssertionNamedWindowPropertyAssignment(epService);
        }
    
        /// <summary>
        /// Writeable-property tests: when a simple writable property needs to be converted
        /// </summary>
        private void RunAssertionSimpleWriteablePropertyCoerce(EPServiceProvider epService) {
            Schema schema = SchemaBuilder.Record("MyEventSchema", TypeBuilder.Field("isodate", TypeBuilder.StringType(
                TypeBuilder.Property(AvroConstant.PROP_STRING_KEY, AvroConstant.PROP_ARRAY_VALUE))));
            epService.EPAdministrator.Configuration.AddEventTypeAvro("MyEvent", new ConfigurationEventTypeAvro(schema));
            epService.EPAdministrator.Configuration.AddEventType<MyEventWithDateTimeOffset>();
            epService.EPAdministrator.Configuration.AddEventType<MyEventWithDateTimeEx>();

            // invalid without explicit conversion
            SupportMessageAssertUtil.TryInvalid(
                epService,
                "insert into MyEvent(isodate) select dto from MyEventWithDateTimeOffset",
                "Error starting statement: Invalid assignment of column 'isodate' of type '" + Name.Of<DateTimeOffset>(false) + "' to event property 'isodate' typed as '" + Name.Of<char[]>(false) + "', column and parameter types mismatch"
                );

            // with hook
            var stmt = epService.EPAdministrator.CreateEPL("insert into MyEvent(isodate) select dtx from MyEventWithDateTimeEx");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            DateTimeEx now = DateTimeEx.NowLocal();
            epService.EPRuntime.SendEvent(new MyEventWithDateTimeEx(now));
            Assert.AreEqual(DateTimeFormatter.ISO_DATE_TIME.Invoke(now), listener.AssertOneGetNewAndReset().Get("isodate"));
    
            stmt.Dispose();
        }
    
        /// <summary>Schema-from-Type</summary>
        private void RunAssertionSchemaFromClass(EPServiceProvider epService)
        {

            string epl = EventRepresentationChoice.AVRO.GetAnnotationText() +
                         "insert into MyEventOut select " + GetType().FullName + 
                         ".MakeDateTime() as isodate from SupportBean as e1";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            Schema schema = SupportAvroUtil.GetAvroSchema(stmt.EventType);
            Assert.AreEqual("{\"type\":\"record\",\"name\":\"MyEventOut\",\"fields\":[{\"name\":\"isodate\",\"type\":\"string\"}]}", schema.ToString());
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EventBean @event = listener.AssertOneGetNewAndReset();
            SupportAvroUtil.AvroToJson(@event);
            Assert.IsTrue(@event.Get("isodate").ToString().Length > 10);
    
            stmt.Dispose();
        }
    
        /// <summary>Mapping of Type to GenericRecord</summary>
        private void RunAssertionPopulate(EPServiceProvider epService) {
            MySupportBeanWidener.supportBeanSchema = SchemaBuilder.Record("SupportBeanSchema",
                TypeBuilder.Field("TheString", "string"),
                TypeBuilder.Field("IntPrimitive", "int"));
            var schema = SchemaBuilder.Record("MyEventSchema",
                TypeBuilder.Field("sb", MySupportBeanWidener.supportBeanSchema));
            epService.EPAdministrator.Configuration.AddEventTypeAvro("MyEventPopulate", new ConfigurationEventTypeAvro(schema));
    
            string epl = "insert into MyEventPopulate(sb) select " + GetType().FullName + ".MakeSupportBean() from SupportBean_S0 as e1";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(10));
            EventBean @event = listener.AssertOneGetNewAndReset();
            Assert.AreEqual("{\"sb\":{\"TheString\":\"E1\",\"IntPrimitive\":10}}", SupportAvroUtil.AvroToJson(@event));
        }
    
        private void RunAssertionNamedWindowPropertyAssignment(EPServiceProvider epService) {
            MySupportBeanWidener.supportBeanSchema = SchemaBuilder.Record("SupportBeanSchema",
                TypeBuilder.Field("TheString", "string"),
                TypeBuilder.Field("IntPrimitive", "int"));
            var schema = SchemaBuilder.Record("MyEventSchema",
                TypeBuilder.Field((string)"sb", TypeBuilder.Union(
                    TypeBuilder.NullType(), MySupportBeanWidener.supportBeanSchema)));
            epService.EPAdministrator.Configuration.AddEventTypeAvro("MyEventWSchema", new ConfigurationEventTypeAvro(schema));
    
            epService.EPAdministrator.CreateEPL("@Name('NamedWindow') create window MyWindow#keepall as MyEventWSchema");
            epService.EPAdministrator.CreateEPL("insert into MyWindow select * from MyEventWSchema");
            epService.EPAdministrator.CreateEPL("on SupportBean thebean update MyWindow set sb = thebean");
    
            GenericRecord @event = new GenericRecord(schema);
            epService.EPRuntime.SendEventAvro(@event, "MyEventWSchema");
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
    
            EventBean eventBean = epService.EPAdministrator.GetStatement("NamedWindow").First();
            Assert.AreEqual("{\"sb\":{\"SupportBeanSchema\":{\"TheString\":\"E1\",\"IntPrimitive\":10}}}", SupportAvroUtil.AvroToJson(eventBean));
        }

        public static DateTimeEx MakeDateTime()
        {
            return DateTimeEx.NowLocal();
        }

        public static SupportBean MakeSupportBean() {
            return new SupportBean("E1", 10);
        }

        public class MyEventWithDateTimeOffset
        {
            public MyEventWithDateTimeOffset(DateTimeOffset dto)
            {
                this.DateTime = dto;
            }

            [PropertyName("dto")]
            public DateTimeOffset DateTime { get; private set; }
        }

        public class MyEventWithDateTimeEx
        {
            public MyEventWithDateTimeEx(DateTimeEx dtx)
            {
                this.DateTime = dtx;
            }

            [PropertyName("dtx")]
            public DateTimeEx DateTime { get; private set; }
        }

        public class MyObjectValueTypeWidenerFactory : ObjectValueTypeWidenerFactory
        {
            private static ObjectValueTypeWidenerFactoryContext _context;

            public TypeWidener Make(ObjectValueTypeWidenerFactoryContext context)
            {
                _context = context;
                if (context.GetClazz() == typeof(DateTimeEx))
                {
                    return MyDTXTypeWidener.INSTANCE.Widen;
                }
                if (context.GetClazz() == typeof(SupportBean))
                {
                    return (new MySupportBeanWidener()).Widen;
                }
                return null;
            }

            public static ObjectValueTypeWidenerFactoryContext GetContext() {
                return _context;
            }
        }

        public class MyDTXTypeWidener
        {
            public readonly static MyDTXTypeWidener INSTANCE = new MyDTXTypeWidener();

            private MyDTXTypeWidener()
            {
            }

            public object Widen(object input)
            {
                var dtx = (DateTimeEx)input;
                return DateTimeFormatter.ISO_DATE_TIME.Invoke(dtx);
            }
        }

        public class MySupportBeanWidener
        {
            public static RecordSchema supportBeanSchema;

            public object Widen(object input)
            {
                var sb = (SupportBean)input;
                var record = new GenericRecord(supportBeanSchema);
                record.Put("TheString", sb.TheString);
                record.Put("IntPrimitive", sb.IntPrimitive);
                return record;
            }
        }

        public class MyTypeRepresentationMapper : TypeRepresentationMapper
        {
            public object Map(TypeRepresentationMapperContext context)
            {
                if (context.GetClazz() == typeof(DateTimeEx))
                {
                    return SchemaExtensions.ToAvro(new JValue("string"));
                }
                return null;
            }
        }
    }
} // end of namespace
