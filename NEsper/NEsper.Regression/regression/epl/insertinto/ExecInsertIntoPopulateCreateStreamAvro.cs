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
using NEsper.Avro.Util.Support;

// using static org.apache.avro.SchemaBuilder.*;
// using static org.junit.Assert.*;

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
    
            Schema schema = Record("name").Fields()
                    .RequiredLong("myLong")
                    .Name("myLongArray").Type(Array().Items(Builder().LongType())).NoDefault()
                    .Name("myByteArray").Type("bytes").NoDefault()
                    .Name("myMap").Type(Map().Values().StringBuilder().Prop(AvroConstant.PROP_JAVA_STRING_KEY, AvroConstant.PROP_JAVA_STRING_VALUE).EndString()).NoDefault()
                    .EndRecord();
            epService.EPAdministrator.Configuration.AddEventTypeAvro("AvroExistingType", new ConfigurationEventTypeAvro(schema));
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            statement.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean());
            EventBean @event = listener.AssertOneGetNewAndReset();
            SupportAvroUtil.AvroToJson(@event);
            Assert.AreEqual(1L, @event.Get("myLong"));
            EPAssertionUtil.AssertEqualsExactOrder(new long[]{1L, 2L}, ((Collection<>) @event.Get("myLongArray")).ToArray());
            Assert.IsTrue(Collections.AreEqual(new byte[]{1, 2, 3}, ((ByteBuffer) @event.Get("myByteArray")).Array()));
            Assert.AreEqual("{k1=v1}", ((Map) @event.Get("myMap")).ToString());
    
            statement.Destroy();
        }
    
        private void RunAssertionNewSchema(EPServiceProvider epService) {
    
            string epl = EventRepresentationChoice.AVRO.GetAnnotationText() + " select 1 as myInt," +
                    "{1L, 2L} as myLongArray," +
                    GetType().FullName + ".MakeByteArray() as myByteArray, " +
                    GetType().FullName + ".MakeMapStringString() as myMap " +
                    "from SupportBean";
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            statement.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean());
            EventBean @event = listener.AssertOneGetNewAndReset();
            string json = SupportAvroUtil.AvroToJson(@event);
            Assert.AreEqual(1, @event.Get("myInt"));
            EPAssertionUtil.AssertEqualsExactOrder(new long[]{1L, 2L}, ((Collection) @event.Get("myLongArray")).ToArray());
            Assert.IsTrue(Collections.AreEqual(new byte[]{1, 2, 3}, ((ByteBuffer) @event.Get("myByteArray")).Array()));
            Assert.AreEqual("{k1=v1}", ((Map) @event.Get("myMap")).ToString());
    
            Schema designSchema = Record("name").Fields()
                    .RequiredInt("myInt")
                    .Name("myLongArray").Type(Array().Items(UnionOf().NullType().And().LongType().EndUnion())).NoDefault()
                    .Name("myByteArray").Type("bytes").NoDefault()
                    .Name("myMap").Type(Map().Values().StringBuilder().Prop(AvroConstant.PROP_JAVA_STRING_KEY, AvroConstant.PROP_JAVA_STRING_VALUE).EndString()).NoDefault()
                    .EndRecord();
            Schema assembledSchema = ((AvroEventType) @event.EventType).SchemaAvro;
            string compareMsg = SupportAvroUtil.CompareSchemas(designSchema, assembledSchema);
            Assert.IsNull(compareMsg, compareMsg);
    
            statement.Destroy();
        }
    
        public static byte[] MakeByteArray() {
            return new byte[]{1, 2, 3};
        }
    
        public static IDictionary<string, string> MakeMapStringString() {
            return Collections.SingletonMap("k1", "v1");
        }
    }
} // end of namespace
