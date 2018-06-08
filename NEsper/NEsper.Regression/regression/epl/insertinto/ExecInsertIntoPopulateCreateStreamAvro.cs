///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avro;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;

using static NEsper.Avro.Extensions.TypeBuilder;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl.insertinto
{
    using Map = IDictionary<string, object>;

    public class ExecInsertIntoPopulateCreateStreamAvro : RegressionExecution {
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            RunAssertionCompatExisting(epService);
            RunAssertionNewSchema(epService);
        }
    
        private void RunAssertionCompatExisting(EPServiceProvider epService) {
    
            string epl = "insert into AvroExistingType select 1 as myLong," +
                    "{1L, 2L} as myLongArray," +
                    GetType().FullName + ".MakeByteArray() as myByteArray, " +
                    GetType().FullName + ".MakeMapStringString() as myMap " +
                    "from SupportBean";
    
            var schema = SchemaBuilder.Record(
                "name",
                RequiredLong("myLong"),
                Field("myLongArray", Array(LongType())),
                Field("myByteArray", BytesType()),
                Field("myMap", Map(StringType(Property(AvroConstant.PROP_STRING_KEY, AvroConstant.PROP_STRING_VALUE))))
            );
            epService.EPAdministrator.Configuration.AddEventTypeAvro("AvroExistingType", new ConfigurationEventTypeAvro(schema));
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean());
            EventBean @event = listener.AssertOneGetNewAndReset();
            SupportAvroUtil.AvroToJson(@event);
            Assert.AreEqual(1L, @event.Get("myLong"));
            EPAssertionUtil.AssertEqualsExactOrder(new long[] {1L, 2L}, @event.Get("myLongArray").UnwrapIntoArray<long>());
            Assert.IsTrue(Collections.AreEqual(new byte[]{1, 2, 3}, @event.Get("myByteArray").UnwrapIntoArray<byte>()));
            Assert.AreEqual("[k1=v1]", @event.Get("myMap").UnwrapStringDictionary().Render());
    
            statement.Dispose();
        }
    
        private void RunAssertionNewSchema(EPServiceProvider epService) {
    
            string epl = EventRepresentationChoice.AVRO.GetAnnotationText() + " select 1 as myInt," +
                    "{1L, 2L} as myLongArray," +
                    GetType().FullName + ".MakeByteArray() as myByteArray, " +
                    GetType().FullName + ".MakeMapStringString() as myMap " +
                    "from SupportBean";
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean());
            EventBean @event = listener.AssertOneGetNewAndReset();
            string json = SupportAvroUtil.AvroToJson(@event);
            Assert.AreEqual(1, @event.Get("myInt"));
            EPAssertionUtil.AssertEqualsExactOrder(new long[] {1L, 2L}, @event.Get("myLongArray").UnwrapIntoArray<long>());
            Assert.IsTrue(Collections.AreEqual(new byte[]{1, 2, 3}, @event.Get("myByteArray").UnwrapIntoArray<byte>()));
            Assert.AreEqual("[k1=v1]", @event.Get("myMap").UnwrapStringDictionary().Render());
    
            var designSchema = SchemaBuilder.Record(
                "name",
                RequiredInt("myInt"),
                Field("myLongArray", Array(Union(NullType(), LongType()))),
                Field("myByteArray", BytesType()),
                Field("myMap", Map(StringType(Property(AvroConstant.PROP_STRING_KEY, AvroConstant.PROP_STRING_VALUE))))
            );
            Schema assembledSchema = ((AvroEventType) @event.EventType).SchemaAvro;
            string compareMsg = SupportAvroUtil.CompareSchemas(designSchema, assembledSchema);
            Assert.IsNull(compareMsg, compareMsg);
    
            statement.Dispose();
        }
    
        public static byte[] MakeByteArray() {
            return new byte[]{1, 2, 3};
        }
    
        public static IDictionary<string, string> MakeMapStringString() {
            return Collections.SingletonMap("k1", "v1");
        }
    }
} // end of namespace
