///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Linq;

using Avro;
using Avro.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.client.hook;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util;

using NUnit.Framework;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;

using Newtonsoft.Json.Linq;
using System;

namespace com.espertech.esper.regression.events
{
    [TestFixture]
	public class TestAvroEventHook
    {
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp()
        {
	        var configuration = SupportConfigFactory.GetConfiguration();
	        configuration.EngineDefaults.EventMeta.AvroSettings.TypeRepresentationMapperClass = typeof(MyTypeRepresentationMapper).FullName;
	        configuration.EngineDefaults.EventMeta.AvroSettings.ObjectValueTypeWidenerFactoryClass = typeof(MyObjectValueTypeWidenerFactory).FullName;
	        _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
	        _epService.Initialize();
	        _listener = new SupportUpdateListener();
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);
	        }
	        foreach (var clazz in Collections.List(typeof(SupportBean), typeof(SupportBean_S0))) {
	            _epService.EPAdministrator.Configuration.AddEventType(clazz);
	        }
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.EndTest();
	        }
	        _listener = null;
	    }

	    /// <summary>
	    /// Writeable-property tests: when a simple writable property needs to be converted
	    /// </summary>
        [Test]
	    public void TestSimpleWriteablePropertyCoerce()
        {
            Schema schema = SchemaBuilder.Record("MyEventSchema", TypeBuilder.Field("isodate", TypeBuilder.String(
                TypeBuilder.Property(AvroConstant.PROP_STRING_KEY, AvroConstant.PROP_ARRAY_VALUE))));
	        _epService.EPAdministrator.Configuration.AddEventTypeAvro("MyEvent", new ConfigurationEventTypeAvro(schema));
            _epService.EPAdministrator.Configuration.AddEventType<MyEventWithDateTimeOffset>();
            _epService.EPAdministrator.Configuration.AddEventType<MyEventWithDateTimeEx>();

            // invalid without explicit conversion
            SupportMessageAssertUtil.TryInvalid(
                _epService, "insert into MyEvent(isodate) select dto from MyEventWithDateTimeOffset",
                "Error starting statement: Invalid assignment of column 'isodate' of type '" + Name.Of<DateTimeOffset>(false) + "' to event property 'isodate' typed as '" + Name.Of<char[]>(false) + "', column and parameter types mismatch");

	        // with hook
	        var stmt = _epService.EPAdministrator.CreateEPL("insert into MyEvent(isodate) select dtx from MyEventWithDateTimeEx");
	        stmt.AddListener(_listener);

            DateTimeEx now = DateTimeEx.NowLocal();
	        _epService.EPRuntime.SendEvent(new MyEventWithDateTimeEx(now));
	        Assert.AreEqual(DateTimeFormatter.ISO_DATE_TIME.Invoke(now), _listener.AssertOneGetNewAndReset().Get("isodate"));
	    }

	    /// <summary>
	    /// Schema-from-Class
	    /// </summary>
        [Test]
	    public void TestSchemaFromClass()
        {
            var epl = EventRepresentationChoice.AVRO.GetAnnotationText() + "insert into MyEvent select " + this.GetType().FullName + ".MakeDateTime() as isodate from SupportBean as e1";
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        var listener = new SupportUpdateListener();
	        stmt.AddListener(listener);

	        Schema schema = SupportAvroUtil.GetAvroSchema(stmt.EventType);
	        Assert.AreEqual("{\"type\":\"record\",\"name\":\"MyEvent\",\"fields\":[{\"name\":\"isodate\",\"type\":\"string\"}]}", schema.ToString());

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
	        var @event = listener.AssertOneGetNewAndReset();
	        SupportAvroUtil.AvroToJson(@event);
	        Assert.IsTrue(@event.Get("isodate").ToString().Length > 10);
	    }

	    /// <summary>
	    /// Mapping of Class to GenericRecord
	    /// </summary>
        [Test]
	    public void TestPopulate()
        {
            MySupportBeanWidener.supportBeanSchema = SchemaBuilder.Record("SupportBeanSchema",
                TypeBuilder.Field("theString", "string"),
                TypeBuilder.Field("intPrimitive", "int"));

            //MySupportBeanWidener.supportBeanSchema = SchemaExtensions.Record("SupportBeanSchema")
            //    .Fields()
            //    .RequiredString("theString")
            //    .RequiredInt("intPrimitive").EndRecord();

            var schema = SchemaBuilder.Record("MyEventSchema",
                TypeBuilder.Field("sb", MySupportBeanWidener.supportBeanSchema));

            //Schema schema = SchemaExtensions.Record("MyEventSchema")
            //    .Fields().Name("sb")
            //    .Type(MySupportBeanWidener.supportBeanSchema)
            //    .NoDefault()
            //    .EndRecord();

            _epService.EPAdministrator.Configuration.AddEventTypeAvro("MyEvent", new ConfigurationEventTypeAvro(schema));

	        var epl = "insert into MyEvent(sb) select " + this.GetType().FullName + ".MakeSupportBean() from SupportBean_S0 as e1";
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        var listener = new SupportUpdateListener();
	        stmt.AddListener(listener);

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(10));
	        var @event = listener.AssertOneGetNewAndReset();
	        Assert.AreEqual("{\"sb\":{\"theString\":\"E1\",\"intPrimitive\":10}}", SupportAvroUtil.AvroToJson(@event));
	    }

        [Test]
	    public void TestNamedWindowPropertyAssignment()
        {
            MySupportBeanWidener.supportBeanSchema = SchemaBuilder.Record("SupportBeanSchema",
                TypeBuilder.Field("theString", "string"),
                TypeBuilder.Field("intPrimitive", "int"));

            //MySupportBeanWidener.supportBeanSchema = SchemaExtensions.Record("SupportBeanSchema")
            //    .Fields()
            //    .RequiredString("theString")
            //    .RequiredInt("intPrimitive").EndRecord();

            var schema = SchemaBuilder.Record("MyEventSchema",
                TypeBuilder.Field((string) "sb", TypeBuilder.Union(
                    TypeBuilder.Null(), MySupportBeanWidener.supportBeanSchema)));

            //Schema schema = SchemaExtensions.Record("MyEventSchema")
            //    .Fields().Name("sb")
            //    .Type(Union().NullType().And().Type(MySupportBeanWidener.supportBeanSchema).EndUnion())
            //    .NoDefault()
            //    .EndRecord();

	        _epService.EPAdministrator.Configuration.AddEventTypeAvro("MyEvent", new ConfigurationEventTypeAvro(schema));

	        _epService.EPAdministrator.CreateEPL("@Name('NamedWindow') create window MyWindow#keepall as MyEvent");
	        _epService.EPAdministrator.CreateEPL("insert into MyWindow select * from MyEvent");
	        _epService.EPAdministrator.CreateEPL("on SupportBean thebean update MyWindow set sb = thebean");

	        var @event = new GenericRecord(schema);
	        _epService.EPRuntime.SendEventAvro(@event, "MyEvent");
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));

	        EventBean eventBean = _epService.EPAdministrator.GetStatement("NamedWindow").First();
	        Assert.AreEqual("{\"sb\":{\"SupportBeanSchema\":{\"theString\":\"E1\",\"intPrimitive\":10}}}", SupportAvroUtil.AvroToJson(eventBean));
	    }

	    public static DateTimeEx MakeDateTime()
	    {
	        return DateTimeEx.NowLocal();
	    }

	    public static SupportBean MakeSupportBean()
        {
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
	        private ObjectValueTypeWidenerFactoryContext _context;

	        public TypeWidener Make(ObjectValueTypeWidenerFactoryContext context) {
	            this._context = context;
	            if (context.GetClazz() == typeof(DateTimeEx)) {
                    return MyDTXTypeWidener.INSTANCE.Widen;
	            }
	            if (context.GetClazz() == typeof(SupportBean)) {
                    return (new MySupportBeanWidener()).Widen;
	            }
	            return null;
	        }

	        public ObjectValueTypeWidenerFactoryContext GetContext() {
	            return _context;
	        }
	    }

	    public class MyDTXTypeWidener 
        {
	        public readonly static MyDTXTypeWidener INSTANCE = new MyDTXTypeWidener();

	        private MyDTXTypeWidener() {
	        }

	        public object Widen(object input) {
	            var dtx = (DateTimeEx) input;
	            return DateTimeFormatter.ISO_DATE_TIME.Invoke(dtx);
	        }
	    }

	    public class MySupportBeanWidener
        {
	        public static RecordSchema supportBeanSchema;

	        public object Widen(object input) {
	            var sb = (SupportBean) input;
	            var record = new GenericRecord(supportBeanSchema);
	            record.Put("theString", sb.TheString);
	            record.Put("intPrimitive", sb.IntPrimitive);
	            return record;
	        }
	    }

	    public class MyTypeRepresentationMapper : TypeRepresentationMapper
        {
	        public object Map(TypeRepresentationMapperContext context) {
	            if (context.GetClazz() == typeof(DateTimeEx)) {
                    return SchemaExtensions.ToAvro(new JValue("string"));
	            }
	            return null;
	        }
	    }
	}
} // end of namespace
