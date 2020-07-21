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

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.json.util;
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

using Newtonsoft.Json.Linq;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.support.@event.SupportEventInfra;

namespace com.espertech.esper.regressionlib.suite.epl.insertinto
{
	public class EPLInsertIntoPopulateSingleColByMethodCall : RegressionExecution
	{
		public void Run(RegressionEnvironment env)
		{
			RegressionPath path = new RegressionPath();

			// Bean
			RunAssertionConversionImplicitType(
				env,
				path,
				"Bean",
				"SupportBean",
				"convertEvent",
				typeof(BeanEventType),
				typeof(SupportBean),
				"SupportMarketDataBean",
				new SupportMarketDataBean("ACME", 0, 0L, null),
				FBEANWTYPE,
				"theString".SplitCsv(),
				new object[] {"ACME"});

			// Map
			IDictionary<string, object> mapEventOne = new Dictionary<string, object>();
			mapEventOne.Put("one", "1");
			mapEventOne.Put("two", "2");
			RunAssertionConversionImplicitType(
				env,
				path,
				"Map",
				"MapOne",
				"convertEventMap",
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
				path,
				"MapOne",
				"convertEventMap",
				"MapTwo",
				typeof(MappedEventBean),
				typeof(Dictionary<string, object>),
				mapEventTwo,
				FMAPWTYPE,
				"one,two".SplitCsv(),
				new object[] {"3", "|4|"});

			// Object-Array
			RunAssertionConversionImplicitType(
				env,
				path,
				"OA",
				"OAOne",
				"convertEventObjectArray",
				typeof(WrapperEventType),
				typeof(object[]),
				"OATwo",
				new object[] {"1", "2"},
				FOAWTYPE,
				"one,two".SplitCsv(),
				new object[] {"1", "|2|"});
			RunAssertionConversionConfiguredType(
				env,
				path,
				"OAOne",
				"convertEventObjectArray",
				"OATwo",
				typeof(ObjectArrayBackedEventBean),
				typeof(object[]),
				new object[] {"3", "4"},
				FOAWTYPE,
				"one,two".SplitCsv(),
				new object[] {"3", "|4|"});

			// Avro
			var rowOne = new GenericRecord(
				AvroSchemaUtil.ResolveAvroSchema(env.Runtime.EventTypeService.GetEventTypePreconfigured("AvroOne")).AsRecordSchema());
			rowOne.Put("one", "1");
			rowOne.Put("two", "2");
			RunAssertionConversionImplicitType(
				env,
				path,
				"Avro",
				"AvroOne",
				"convertEventAvro",
				typeof(WrapperEventType),
				typeof(GenericRecord),
				"AvroTwo",
				rowOne,
				FAVROWTYPE,
				"one,two".SplitCsv(),
				new object[] {"1", "|2|"});

			var avroSchema = AvroSchemaUtil
				.ResolveAvroSchema(env.Runtime.EventTypeService.GetEventTypePreconfigured("AvroTwo"))
				.AsRecordSchema();
			var rowTwo = new GenericRecord(avroSchema);
			rowTwo.Put("one", "3");
			rowTwo.Put("two", "4");
			RunAssertionConversionConfiguredType(
				env,
				path,
				"AvroOne",
				"convertEventAvro",
				"AvroTwo",
				typeof(AvroGenericDataBackedEventBean),
				typeof(GenericRecord),
				rowTwo,
				FAVROWTYPE,
				"one,two".SplitCsv(),
				new object[] {"3", "|4|"});

			// Json
			env.CompileDeploy(
				"@buseventtype @public create json schema JsonOne(one string, two string);\n" +
				"@buseventtype @public create json schema JsonTwo(one string, two string);\n",
				path);
			var jsonOne = new JObject();
			jsonOne.Add("one", "1");
			jsonOne.Add("two", "2");
			RunAssertionConversionImplicitType(
				env,
				path,
				"Json",
				"JsonOne",
				"convertEventJson",
				typeof(WrapperEventType),
				typeof(JsonEventObject),
				"JsonTwo",
				jsonOne.ToString(),
				FJSONWTYPE,
				"one,two".SplitCsv(),
				new object[] {"1", "|2|"});

			var jsonTwo = new JObject();
			jsonTwo.Add("one", "3");
			jsonTwo.Add("two", "4");
			RunAssertionConversionConfiguredType(
				env,
				path,
				"JsonOne",
				"convertEventJson",
				"JsonTwo",
				typeof(object),
				typeof(object),
				jsonTwo.ToString(),
				FJSONWTYPE,
				"one,two".SplitCsv(),
				new object[] {"3", "|4|"});

			env.UndeployAll();
		}

		private static void RunAssertionConversionImplicitType(
			RegressionEnvironment env,
			RegressionPath path,
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
			string streamName = prefix + "_Stream";
			string textOne = "@name('s1') insert into " + streamName + " select * from " + typeNameOrigin;
			string textTwo = "@name('s2') insert into " +
			                 streamName +
			                 " select " +
			                 typeof(SupportStaticMethodLib).FullName +
			                 "." +
			                 functionName +
			                 "(s0) from " +
			                 typeNameEvent +
			                 " as s0";

			env.CompileDeploy(textOne, path).AddListener("s1");
			EventType type = env.Statement("s1").EventType;
			Assert.IsTrue(TypeHelper.IsSubclassOrImplementsInterface(type.UnderlyingType, underlyingType));

			env.CompileDeploy(textTwo, path).AddListener("s2");
			type = env.Statement("s2").EventType;
			Assert.IsTrue(TypeHelper.IsSubclassOrImplementsInterface(type.UnderlyingType, underlyingType));

			sendEvent.Invoke(env, @event, typeNameEvent);

			EventBean theEvent = env.Listener("s2").AssertOneGetNewAndReset();
			Assert.IsTrue(TypeHelper.IsSubclassOrImplementsInterface(theEvent.EventType.GetType(), eventTypeType));
			Assert.IsTrue(TypeHelper.IsSubclassOrImplementsInterface(theEvent.Underlying.GetType(), underlyingType));
			EPAssertionUtil.AssertProps(theEvent, propertyName, propertyValues);

			env.UndeployModuleContaining("s2");
			env.UndeployModuleContaining("s1");
		}

		private static void RunAssertionConversionConfiguredType(
			RegressionEnvironment env,
			RegressionPath path,
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
			env.CompileDeploy(
				"@name('insert') insert into " +
				typeNameTarget +
				" select " +
				typeof(SupportStaticMethodLib).FullName +
				"." +
				functionName +
				"(s0) from " +
				typeNameOrigin +
				" as s0",
				path);
			env.CompileDeploy("@name('s0') select * from " + typeNameTarget, path).AddListener("s0");

			sendEvent.Invoke(env, @event, typeNameOrigin);

			EventBean eventBean = env.Listener("s0").AssertOneGetNewAndReset();
			Assert.IsTrue(TypeHelper.IsSubclassOrImplementsInterface(eventBean.Underlying.GetType(), underlyingType));
			Assert.IsTrue(TypeHelper.IsSubclassOrImplementsInterface(eventBean.GetType(), eventBeanType));
			EPAssertionUtil.AssertProps(eventBean, propertyName, propertyValues);

			env.UndeployModuleContaining("s0");
			env.UndeployModuleContaining("insert");
		}
	}
} // end of namespace
