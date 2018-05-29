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
using com.espertech.esper.client.annotation;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.events;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util.support;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;
// using static com.espertech.esper.supportregression.@event.SupportEventInfra.*;
// using static org.apache.avro.SchemaBuilder.*;

using NUnit.Framework;

using static com.espertech.esper.supportregression.events.SupportEventInfra;

namespace com.espertech.esper.regression.events.infra
{
    using StringMap = IDictionary<string, string>;

    public class ExecEventInfraPropertyMappedIndexed : RegressionExecution
    {
        private static readonly Type BEAN_TYPE = typeof(MyIMEvent);
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType(BEAN_TYPE);
            AddMapEventType(epService);
            AddOAEventType(epService);
            AddAvroEventType(epService);

            RunAssertion(
                epService,
                BEAN_TYPE.Name,
                FBEAN,
                new MyIMEvent(new[] { "v1", "v2" }, Collections.SingletonMap("k1", "v1")));

            RunAssertion(
                epService,
                SupportEventInfra.MAP_TYPENAME,
                SupportEventInfra.FMAP,
                SupportEventInfra.TwoEntryMap("indexed", new[] { "v1", "v2" }, "mapped", Collections.SingletonMap("k1", "v1")));

            RunAssertion(
                epService,
                SupportEventInfra.OA_TYPENAME,
                SupportEventInfra.FOA,
                new object[] { new[] { "v1", "v2" }, Collections.SingletonMap("k1", "v1") });

            // Avro
            var datum = new GenericRecord(GetAvroSchema());
            datum.Put("indexed", Collections.List("v1", "v2"));
            datum.Put("mapped", Collections.SingletonMap("k1", "v1"));
            RunAssertion(epService, AVRO_TYPENAME, FAVRO, datum);
        }
    
        private void RunAssertion(EPServiceProvider epService,
                                  string typename,
                                  FunctionSendEvent send,
                                  object underlying)
        {
            RunAssertionTypeValidProp(epService, typename, underlying);
            RunAssertionTypeInvalidProp(epService, typename);
    
            string stmtText = "select * from " + typename;
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            send.Invoke(epService, underlying);
            EventBean @event = listener.AssertOneGetNewAndReset();
    
            EventPropertyGetterMapped mappedGetter = @event.EventType.GetGetterMapped("mapped");
            Assert.AreEqual("v1", mappedGetter.Get(@event, "k1"));
    
            EventPropertyGetterIndexed indexedGetter = @event.EventType.GetGetterIndexed("indexed");
            Assert.AreEqual("v2", indexedGetter.Get(@event, 1));
    
            RunAssertionEventInvalidProp(@event);
            SupportEventTypeAssertionUtil.AssertConsistency(@event);
    
            stmt.Dispose();
        }
    
        private void AddMapEventType(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType(MAP_TYPENAME,
                TwoEntryMap("indexed", typeof(string[]), "mapped", typeof(IDictionary<string,string>)));
        }
    
        private void AddOAEventType(EPServiceProvider epService) {
            string[] names = { "indexed", "mapped"};
            object[] types = {typeof(string[]), typeof(StringMap)};
            epService.EPAdministrator.Configuration.AddEventType(OA_TYPENAME, names, types);
        }
    
        private void AddAvroEventType(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventTypeAvro(AVRO_TYPENAME, 
                new ConfigurationEventTypeAvro(GetAvroSchema()));
        }
    
        private static RecordSchema GetAvroSchema()
        {
            return SchemaBuilder.Record(
                "AvroSchema",
                TypeBuilder.Field(
                    "indexed", TypeBuilder.Array(
                        TypeBuilder.StringType(
                            TypeBuilder.Property(AvroConstant.PROP_STRING_KEY, AvroConstant.PROP_STRING_VALUE)))),
                TypeBuilder.Field(
                    "mapped", TypeBuilder.Map(
                        TypeBuilder.StringType(
                            TypeBuilder.Property(AvroConstant.PROP_STRING_KEY, AvroConstant.PROP_STRING_VALUE))))
            );
        }

        private void RunAssertionEventInvalidProp(EventBean @event) {
            foreach (string prop in Collections.List("xxxx", "mapped[1]", "indexed('a')", "mapped.x", "indexed.x")) {
                SupportMessageAssertUtil.TryInvalidProperty(@event, prop);
                SupportMessageAssertUtil.TryInvalidGetFragment(@event, prop);
            }
        }
    
        private void RunAssertionTypeValidProp(EPServiceProvider epService, string typeName, object underlying) {
            EventType eventType = epService.EPAdministrator.Configuration.GetEventType(typeName);
    
            var expectedType = new[]
            {
                new object[]{ "indexed", typeof(string[]), null, null },
                new object[]{ "mapped", typeof(StringMap), null, null }
            };
            SupportEventTypeAssertionUtil.AssertEventTypeProperties(expectedType, eventType, SupportEventTypeAssertionEnumExtensions.GetSetWithFragment());
    
            EPAssertionUtil.AssertEqualsAnyOrder(new[]{"indexed", "mapped" }, eventType.PropertyNames);
    
            Assert.IsNotNull(eventType.GetGetter("mapped"));
            Assert.IsNotNull(eventType.GetGetter("mapped('a')"));
            Assert.IsNotNull(eventType.GetGetter("indexed"));
            Assert.IsNotNull(eventType.GetGetter("indexed[0]"));
            Assert.IsTrue(eventType.IsProperty("mapped"));
            Assert.IsTrue(eventType.IsProperty("mapped('a')"));
            Assert.IsTrue(eventType.IsProperty("indexed"));
            Assert.IsTrue(eventType.IsProperty("indexed[0]"));
            Assert.AreEqual(typeof(StringMap), eventType.GetPropertyType("mapped"));
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("mapped('a')"));
            Assert.AreEqual(typeof(string[]), eventType.GetPropertyType("indexed"));
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("indexed[0]"));

            Assert.AreEqual(new EventPropertyDescriptor("indexed", typeof(string[]), typeof(string), false, false, true, false, false), eventType.GetPropertyDescriptor("indexed"));
            Assert.AreEqual(new EventPropertyDescriptor("mapped", typeof(StringMap), typeof(string), false, false, false, true, false), eventType.GetPropertyDescriptor("mapped"));
    
            Assert.IsNull(eventType.GetFragmentType("indexed"));
            Assert.IsNull(eventType.GetFragmentType("mapped"));
        }
    
        private void RunAssertionTypeInvalidProp(EPServiceProvider epService, string typeName) {
            EventType eventType = epService.EPAdministrator.Configuration.GetEventType(typeName);
    
            foreach (string prop in Collections.List("xxxx", "myString[0]", "indexed('a')", "indexed.x", "mapped[0]", "mapped.x")) {
                Assert.AreEqual(false, eventType.IsProperty(prop));
                Assert.AreEqual(null, eventType.GetPropertyType(prop));
                Assert.IsNull(eventType.GetPropertyDescriptor(prop));
            }
        }
    
        public class MyIMEvent
        {
            private readonly string[] _indexed;
            private readonly StringMap _mapped;
    
            public MyIMEvent(string[] indexed, StringMap mapped)
            {
                _indexed = indexed;
                _mapped = mapped;
            }

            [PropertyName("indexed")]
            public string[] Indexed => _indexed;
            [PropertyName("mapped")]
            public StringMap Mapped => _mapped;
        }
    }
} // end of namespace
