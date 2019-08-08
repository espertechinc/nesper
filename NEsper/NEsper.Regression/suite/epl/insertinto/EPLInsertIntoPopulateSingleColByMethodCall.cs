///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Avro.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.epl;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.support.@event.SupportEventInfra;

namespace com.espertech.esper.regressionlib.suite.epl.insertinto
{
    public class EPLInsertIntoPopulateSingleColByMethodCall : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            // Bean
            RunAssertionConversionImplicitType(
                env,
                "Bean",
                "SupportBean",
                "ConvertEvent",
                typeof(BeanEventType),
                typeof(SupportBean),
                "SupportMarketDataBean",
                new SupportMarketDataBean("ACME", 0, 0L, null),
                FBEANWTYPE,
                "TheString".SplitCsv(),
                new object[] {"ACME"});

            // Map
            IDictionary<string, object> mapEventOne = new Dictionary<string, object>();
            mapEventOne.Put("one", "1");
            mapEventOne.Put("two", "2");
            RunAssertionConversionImplicitType(
                env,
                "Map",
                "MapOne",
                "ConvertEventMap",
                typeof(WrapperEventType),
                typeof(IDictionary<string, object>),
                "MapTwo",
                mapEventOne,
                FMAPWTYPE,
                "one,two".SplitCsv(),
                new object[] {"1", "|2|"});

            IDictionary<string, object> mapEventTwo = new Dictionary<string, object>();
            mapEventTwo.Put("one", "3");
            mapEventTwo.Put("two", "4");
            RunAssertionConversionConfiguredType(
                env,
                "MapOne",
                "ConvertEventMap",
                "MapTwo",
                typeof(MappedEventBean),
                typeof(IDictionary<string, object>),
                mapEventTwo,
                FMAPWTYPE,
                "one,two".SplitCsv(),
                new object[] {"3", "|4|"});

            // Object-Array
            RunAssertionConversionImplicitType(
                env,
                "OA",
                "OAOne",
                "ConvertEventObjectArray",
                typeof(WrapperEventType),
                typeof(object[]),
                "OATwo",
                new object[] {"1", "2"},
                FOAWTYPE,
                "one,two".SplitCsv(),
                new object[] {"1", "|2|"});
            RunAssertionConversionConfiguredType(
                env,
                "OAOne",
                "ConvertEventObjectArray",
                "OATwo",
                typeof(ObjectArrayBackedEventBean),
                typeof(object[]),
                new object[] {"3", "4"},
                FOAWTYPE,
                "one,two".SplitCsv(),
                new object[] {"3", "|4|"});

            // Avro
            var rowOne = new GenericRecord(
                AvroSchemaUtil.ResolveAvroSchema(env.Runtime.EventTypeService.GetEventTypePreconfigured("AvroOne"))
                    .AsRecordSchema());
            rowOne.Put("one", "1");
            rowOne.Put("two", "2");
            RunAssertionConversionImplicitType(
                env,
                "Avro",
                "AvroOne",
                "ConvertEventAvro",
                typeof(WrapperEventType),
                typeof(GenericRecord),
                "AvroTwo",
                rowOne,
                FAVROWTYPE,
                "one,two".SplitCsv(),
                new object[] {"1", "|2|"});

            var rowTwo = new GenericRecord(
                AvroSchemaUtil.ResolveAvroSchema(env.Runtime.EventTypeService.GetEventTypePreconfigured("AvroTwo"))
                    .AsRecordSchema());
            rowTwo.Put("one", "3");
            rowTwo.Put("two", "4");
            RunAssertionConversionConfiguredType(
                env,
                "AvroOne",
                "ConvertEventAvro",
                "AvroTwo",
                typeof(AvroGenericDataBackedEventBean),
                typeof(GenericRecord),
                rowTwo,
                FAVROWTYPE,
                "one,two".SplitCsv(),
                new object[] {"3", "|4|"});
        }

        private static void RunAssertionConversionImplicitType(
            RegressionEnvironment env,
            string prefix,
            string typeNameOrigin,
            string functionName,
            Type eventTypeType,
            Type underlyingType,
            string typeNameEvent,
            object @event,
            FunctionSendEventWType sendEvent,
            string[] propertyName,
            object[] propertyValues)
        {
            var streamName = prefix + "_Stream";
            var textOne = "@Name('s1') insert into " + streamName + " select * from " + typeNameOrigin;
            var textTwo = "@Name('s2') insert into " +
                          streamName +
                          " select " +
                          typeof(SupportStaticMethodLib).FullName +
                          "." +
                          functionName +
                          "(s0) from " +
                          typeNameEvent +
                          " as s0";

            var path = new RegressionPath();
            env.CompileDeploy(textOne, path).AddListener("s1");
            var type = env.Statement("s1").EventType;
            Assert.AreEqual(underlyingType, type.UnderlyingType);

            env.CompileDeploy(textTwo, path).AddListener("s2");
            type = env.Statement("s2").EventType;
            Assert.AreEqual(underlyingType, type.UnderlyingType);

            sendEvent.Invoke(env, @event, typeNameEvent);

            var theEvent = env.Listener("s2").AssertOneGetNewAndReset();
            Assert.IsTrue(TypeHelper.IsSubclassOrImplementsInterface(theEvent.EventType.GetType(), eventTypeType));
            Assert.IsTrue(TypeHelper.IsSubclassOrImplementsInterface(theEvent.Underlying.GetType(), underlyingType));
            EPAssertionUtil.AssertProps(theEvent, propertyName, propertyValues);

            env.UndeployAll();
        }

        private static void RunAssertionConversionConfiguredType(
            RegressionEnvironment env,
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
            var typeName = typeof(SupportStaticMethodLib).FullName;
            env.CompileDeploy(
                $"insert into {typeNameTarget} select {typeName}.{functionName}(s0) from {typeNameOrigin} as s0");
            env.CompileDeploy($"@Name('s0') select * from {typeNameTarget}").AddListener("s0");

            sendEvent.Invoke(env, @event, typeNameOrigin);

            var eventBean = env.Listener("s0").AssertOneGetNewAndReset();
            Assert.IsTrue(TypeHelper.IsSubclassOrImplementsInterface(eventBean.Underlying.GetType(), underlyingType));
            Assert.IsTrue(TypeHelper.IsSubclassOrImplementsInterface(eventBean.GetType(), eventBeanType));
            EPAssertionUtil.AssertProps(eventBean, propertyName, propertyValues);

            env.UndeployAll();
        }
    }
} // end of namespace