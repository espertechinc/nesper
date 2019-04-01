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
using com.espertech.esper.compat.collections;
using com.espertech.esper.events;
using com.espertech.esper.events.bean;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.events;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

using static com.espertech.esper.supportregression.events.SupportEventInfra;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl.insertinto
{
    using Map = IDictionary<string, object>;

    public class ExecInsertIntoPopulateSingleColByMethodCall : RegressionExecution
    {
        public override void Run(EPServiceProvider epService)
        {
            // define Bean
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType<SupportMarketDataBean>();
    
            // define Map
            var mapTypeInfo = new Dictionary<string, object>();
            mapTypeInfo.Put("one", typeof(string));
            mapTypeInfo.Put("two", typeof(string));
            epService.EPAdministrator.Configuration.AddEventType("MapOne", mapTypeInfo);
            epService.EPAdministrator.Configuration.AddEventType("MapTwo", mapTypeInfo);
    
            // define OA
            string[] props = {"one", "two"};
            object[] types = {typeof(string), typeof(string)};
            epService.EPAdministrator.Configuration.AddEventType("OAOne", props, types);
            epService.EPAdministrator.Configuration.AddEventType("OATwo", props, types);
    
            // define Avro
            var schema = SchemaBuilder.Record("name",
                TypeBuilder.RequiredString("one"),
                TypeBuilder.RequiredString("two"));
            epService.EPAdministrator.Configuration.AddEventTypeAvro("AvroOne", new ConfigurationEventTypeAvro(schema));
            epService.EPAdministrator.Configuration.AddEventTypeAvro("AvroTwo", new ConfigurationEventTypeAvro(schema));
    
            // Bean
            RunAssertionConversionImplicitType(epService, "Bean", typeof(SupportBean).Name, "ConvertEvent", typeof(BeanEventType), typeof(SupportBean),
                    typeof(SupportMarketDataBean).FullName, new SupportMarketDataBean("ACME", 0, 0L, null), FBEANWTYPE, "TheString".Split(','), new object[]{"ACME"});
    
            // Map
            var mapEventOne = new Dictionary<string, object>();
            mapEventOne.Put("one", "1");
            mapEventOne.Put("two", "2");
            RunAssertionConversionImplicitType(epService, "Map", "MapOne", "ConvertEventMap", typeof(WrapperEventType), typeof(Map),
                    "MapTwo", mapEventOne, FMAPWTYPE, "one,two".Split(','), new object[]{"1", "|2|"});
    
            var mapEventTwo = new Dictionary<string, object>();
            mapEventTwo.Put("one", "3");
            mapEventTwo.Put("two", "4");
            RunAssertionConversionConfiguredType(epService, "MapOne", "ConvertEventMap", "MapTwo", typeof(MappedEventBean), typeof(Dictionary<string, object>), mapEventTwo, FMAPWTYPE, "one,two".Split(','), new object[]{"3", "|4|"});
    
            // Object-Array
            RunAssertionConversionImplicitType(epService, "OA", "OAOne", "ConvertEventObjectArray", typeof(WrapperEventType), typeof(object[]),
                    "OATwo", new object[]{"1", "2"}, FOAWTYPE, "one,two".Split(','), new object[]{"1", "|2|"});
            RunAssertionConversionConfiguredType(epService, "OAOne", "ConvertEventObjectArray", "OATwo", typeof(ObjectArrayBackedEventBean), typeof(object[]), new object[]{"3", "4"}, FOAWTYPE, "one,two".Split(','), new object[]{"3", "|4|"});
    
            // Avro
            var rowOne = new GenericRecord(schema);
            rowOne.Put("one", "1");
            rowOne.Put("two", "2");
            RunAssertionConversionImplicitType(epService, "Avro", "AvroOne", "ConvertEventAvro", typeof(WrapperEventType), typeof(GenericRecord),
                    "AvroTwo", rowOne, FAVROWTYPE, "one,two".Split(','), new object[]{"1", "|2|"});
    
            var rowTwo = new GenericRecord(schema);
            rowTwo.Put("one", "3");
            rowTwo.Put("two", "4");
            RunAssertionConversionConfiguredType(epService, "AvroOne", "ConvertEventAvro", "AvroTwo", typeof(AvroGenericDataBackedEventBean), typeof(GenericRecord), rowTwo, FAVROWTYPE, "one,two".Split(','), new object[]{"3", "|4|"});
        }
    
        private void RunAssertionConversionImplicitType(EPServiceProvider epService, string prefix,
                                                        string typeNameOrigin,
                                                        string functionName,
                                                        Type eventTypeType,
                                                        Type underlyingType,
                                                        string typeNameEvent,
                                                        object @event,
                                                        FunctionSendEventWType sendEvent,
                                                        string[] propertyName,
                                                        object[] propertyValues) {
            var streamName = prefix + "_Stream";
            var textOne = "insert into " + streamName + " select * from " + typeNameOrigin;
            var textTwo = "insert into " + streamName + " select " + typeof(SupportStaticMethodLib).FullName + "." + functionName + "(s0) from " + typeNameEvent + " as s0";
    
            var stmtOne = epService.EPAdministrator.CreateEPL(textOne);
            var listenerOne = new SupportUpdateListener();
            stmtOne.Events += listenerOne.Update;
            var type = stmtOne.EventType;
            Assert.AreEqual(underlyingType, type.UnderlyingType);
    
            var stmtTwo = epService.EPAdministrator.CreateEPL(textTwo);
            var listenerTwo = new SupportUpdateListener();
            stmtTwo.Events += listenerTwo.Update;
            type = stmtTwo.EventType;
            Assert.AreEqual(underlyingType, type.UnderlyingType);
    
            sendEvent.Invoke(epService, @event, typeNameEvent);
    
            var theEvent = listenerTwo.AssertOneGetNewAndReset();
            Assert.IsTrue(TypeHelper.IsSubclassOrImplementsInterface(theEvent.EventType.GetType(), eventTypeType));
            Assert.IsTrue(TypeHelper.IsSubclassOrImplementsInterface(theEvent.Underlying.GetType(), underlyingType));
            EPAssertionUtil.AssertProps(theEvent, propertyName, propertyValues);
    
            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionConversionConfiguredType(
            EPServiceProvider epService,
            string typeNameTarget,
            string functionName,
            string typeNameOrigin,
            Type eventBeanType,
            Type underlyingType,
            object @event,
            FunctionSendEventWType sendEvent,
            string[] propertyName,
            object[] propertyValues)
        {
            // test native
            epService.EPAdministrator.CreateEPL("insert into " + typeNameTarget + " select " + typeof(SupportStaticMethodLib).FullName + "." + functionName + "(s0) from " + typeNameOrigin + " as s0");
            var stmt = epService.EPAdministrator.CreateEPL("select * from " + typeNameTarget);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            sendEvent.Invoke(epService, @event, typeNameOrigin);
    
            var eventBean = listener.AssertOneGetNewAndReset();
            Assert.IsTrue(TypeHelper.IsSubclassOrImplementsInterface(eventBean.Underlying.GetType(), underlyingType));
            Assert.IsTrue(TypeHelper.IsSubclassOrImplementsInterface(eventBean.GetType(), eventBeanType));
            EPAssertionUtil.AssertProps(eventBean, propertyName, propertyValues);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    }
} // end of namespace
