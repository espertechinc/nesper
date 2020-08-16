///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Drawing;

using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.json.util;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.module;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compiler.client;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;

using Newtonsoft.Json.Linq;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

using static NEsper.Avro.Extensions.TypeBuilder;

namespace com.espertech.esper.regressionlib.suite.epl.other
{
	public class EPLOtherCreateSchema
	{

		public static IList<RegressionExecution> Executions()
		{
			IList<RegressionExecution> execs = new List<RegressionExecution>();
			execs.Add(new EPLOtherCreateSchemaPathSimple());
			execs.Add(new EPLOtherCreateSchemaPublicSimple());
			execs.Add(new EPLOtherCreateSchemaArrayPrimitiveType());
			execs.Add(new EPLOtherCreateSchemaCopyProperties());
			execs.Add(new EPLOtherCreateSchemaConfiguredNotRemoved());
			execs.Add(new EPLOtherCreateSchemaAvroSchemaWAnnotation());
			execs.Add(new EPLOtherCreateSchemaColDefPlain());
			execs.Add(new EPLOtherCreateSchemaModelPONO());
			execs.Add(new EPLOtherCreateSchemaNestableMapArray());
			execs.Add(new EPLOtherCreateSchemaInherit());
			execs.Add(new EPLOtherCreateSchemaCopyFromOrderObjectArray());
			execs.Add(new EPLOtherCreateSchemaInvalid());
			execs.Add(new EPLOtherCreateSchemaWithEventType());
			execs.Add(new EPLOtherCreateSchemaVariantType());
			execs.Add(new EPLOtherCreateSchemaSameCRC());
			execs.Add(new EPLOtherCreateSchemaBeanImport());
			execs.Add(new EPLOtherCreateSchemaCopyFromDeepWithValueObject());
			return execs;
		}

		public class EPLOtherCreateSchemaCopyFromDeepWithValueObject : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "create schema SchemaA (account string, foo " +
				          typeof(MyLocalValueObject).FullName +
				          ");\n" +
				          "create schema SchemaB (symbol string) copyfrom SchemaA;\n" +
				          "create schema SchemaC () copyfrom SchemaB;\n" +
				          "create schema SchemaD () copyfrom SchemaB;\n" +
				          "insert into SchemaD select account, " +
				          typeof(EPLOtherCreateSchema).FullName +
				          ".getLocalValueObject() as foo, symbol from SchemaC;\n";
				env.CompileDeploy(epl).UndeployAll();
			}
		}

		public class EPLOtherCreateSchemaBeanImport : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				env.CompileDeploy("create schema MyEvent as Rectangle");

				TryInvalidCompile(env, "create schema MyEvent as XXUnknown", "Could not load class by name 'XXUnknown', please check imports");

				env.UndeployAll();
			}
		}

		public class EPLOtherCreateSchemaSameCRC : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				try {
					env.CompileDeployWBusPublicType(
						"create schema b5a7b602ab754d7ab30fb42c4fb28d82();\n" +
						"create schema d19f2e9e82d14b96be4fa12b8a27ee9f();",
						new RegressionPath());
				}
				catch (Exception ex) {
					Assert.AreEqual(
						"Test failed due to exception: Event type by name 'd19f2e9e82d14b96be4fa12b8a27ee9f' has a public crc32 id overlap with event type by name 'b5a7b602ab754d7ab30fb42c4fb28d82', please consider renaming either of these types",
						ex.Message);
				}
			}
		}

		public class EPLOtherCreateSchemaPathSimple : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "@name('schema') create schema SimpleSchema(p0 string, p1 int);" +
				          "@name('s0') select * from SimpleSchema;\n" +
				          "insert into SimpleSchema select theString as p0, intPrimitive as p1 from SupportBean;\n";
				env.CompileDeploy(epl).AddListener("s0");
				Assert.AreEqual(StatementType.CREATE_SCHEMA, env.Statement("schema").GetProperty(StatementProperty.STATEMENTTYPE));
				Assert.AreEqual("SimpleSchema", env.Statement("schema").GetProperty(StatementProperty.CREATEOBJECTNAME));

				env.SendEventBean(new SupportBean("a", 20));
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), "p0,p1".SplitCsv(), new object[] {"a", 20});

				Assert.IsNull(env.Runtime.EventTypeService.GetBusEventType("SimpleSchema"));

				env.UndeployAll();
			}
		}

		public class EPLOtherCreateSchemaPublicSimple : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "create schema MySchema as (p0 string, p1 int);\n" +
				          "@name('s0') select p0, p1 from MySchema;\n";
				env.CompileDeployWBusPublicType(epl, new RegressionPath()).AddListener("s0");

				env.SendEventMap(CollectionUtil.BuildMap("p0", "a", "p1", 20), "MySchema");
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), "p0,p1".SplitCsv(), new object[] {"a", 20});

				var eventType = env.Runtime.EventTypeService.GetBusEventType("MySchema");
				Assert.AreEqual("MySchema", eventType.Name);

				env.UndeployAll();
			}
		}

		public class EPLOtherCreateSchemaCopyFromOrderObjectArray : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				var epl = "@name('s1') create objectarray schema MyEventOne(p0 string, p1 double);\n " +
				          "create objectarray schema MyEventTwo(p2 string) copyfrom MyEventOne;\n";
				env.CompileDeployWBusPublicType(epl, path);

				var type = env.Runtime.EventTypeService.GetEventType(env.DeploymentId("s1"), "MyEventTwo");
				EPAssertionUtil.AssertEqualsExactOrder("p0,p1,p2".SplitCsv(), type.PropertyNames);

				epl = "insert into MyEventTwo select 'abc' as p2, s.* from MyEventOne as s;\n" +
				      "@name('s0') select p0, p1, p2 from MyEventTwo;\n";
				env.CompileDeploy(epl, path).AddListener("s0");

				env.SendEventObjectArray(new object[] {"E1", 10d}, "MyEventOne");
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), "p0,p1,p2".SplitCsv(), new object[] {"E1", 10d, "abc"});

				env.UndeployAll();
			}
		}

		public class EPLOtherCreateSchemaArrayPrimitiveType : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				TryAssertionSchemaArrayPrimitiveType(env, true);
				TryAssertionSchemaArrayPrimitiveType(env, false);

				TryInvalidCompile(
					env,
					"create schema Invalid (x dummy[primitive])",
					"Type 'dummy' is not a primitive type [create schema Invalid (x dummy[primitive])]");
				TryInvalidCompile(
					env,
					"create schema Invalid (x int[dummy])",
					"Invalid array keyword 'dummy', expected 'primitive'");
			}

			private static void TryAssertionSchemaArrayPrimitiveType(
				RegressionEnvironment env,
				bool soda)
			{
				CompileDeployWExport("@name('schema') create schema MySchema as (c0 int[primitive], c1 int[])", soda, env);
				var expectedType = new[] {
					new object[] {"c0", typeof(int[])}, 
					new object[] {"c1", typeof(int?[])}};
				SupportEventTypeAssertionUtil.AssertEventTypeProperties(
					expectedType,
					GetTypeStmt(env, "schema"),
					SupportEventTypeAssertionEnum.NAME,
					SupportEventTypeAssertionEnum.TYPE);
				env.UndeployAll();
			}
		}

		public class EPLOtherCreateSchemaWithEventType : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var theEvent = new SupportBeanSourceEvent(new SupportBean("E1", 1), new[] {new SupportBean_S0(2)});

				// test schema
				env.CompileDeploy("@name('schema') create schema MySchema (bean SupportBean, beanarray SupportBean_S0[])");
				var stmtSchemaType = env.Statement("schema").EventType;
				Assert.AreEqual(
					new EventPropertyDescriptor("bean", typeof(SupportBean), null, false, false, false, false, true),
					stmtSchemaType.GetPropertyDescriptor("bean"));
				Assert.AreEqual(
					new EventPropertyDescriptor("beanarray", typeof(SupportBean_S0[]), typeof(SupportBean_S0), false, false, true, false, true),
					stmtSchemaType.GetPropertyDescriptor("beanarray"));

				env.CompileDeploy("@name('s0') insert into MySchema select sb as bean, s0Arr as beanarray from SupportBeanSourceEvent").AddListener("s0");
				env.SendEventBean(theEvent);
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), "bean.theString,beanarray[0].id".SplitCsv(), new object[] {"E1", 2});
				env.UndeployModuleContaining("s0");

				// test named window
				var path = new RegressionPath();
				env.CompileDeploy("@name('window') create window MyWindow#keepall as (bean SupportBean, beanarray SupportBean_S0[])", path)
					.AddListener("window");
				var stmtWindowType = env.Statement("window").EventType;
				Assert.AreEqual(
					new EventPropertyDescriptor("bean", typeof(SupportBean), null, false, false, false, false, true),
					stmtWindowType.GetPropertyDescriptor("bean"));
				Assert.AreEqual(
					new EventPropertyDescriptor("beanarray", typeof(SupportBean_S0[]), typeof(SupportBean_S0), false, false, true, false, true),
					stmtWindowType.GetPropertyDescriptor("beanarray"));

				env.CompileDeploy("@name('windowInsertOne') insert into MyWindow select sb as bean, s0Arr as beanarray from SupportBeanSourceEvent", path);
				env.SendEventBean(theEvent);
				EPAssertionUtil.AssertProps(
					env.Listener("window").AssertOneGetNewAndReset(),
					"bean.theString,beanarray[0].id".SplitCsv(),
					new object[] {"E1", 2});
				env.UndeployModuleContaining("windowInsertOne");

				// insert pattern to named window
				env.CompileDeploy(
					"@name('windowInsertOne') insert into MyWindow select sb as bean, s0Arr as beanarray from pattern [sb=SupportBean -> s0Arr=SupportBean_S0 until SupportBean_S0(id=0)]",
					path);
				env.SendEventBean(new SupportBean("E2", 2));
				env.SendEventBean(new SupportBean_S0(10, "S0_1"));
				env.SendEventBean(new SupportBean_S0(20, "S0_2"));
				env.SendEventBean(new SupportBean_S0(0, "S0_3"));
				EPAssertionUtil.AssertProps(
					env.Listener("window").AssertOneGetNewAndReset(),
					"bean.theString,beanarray[0].id,beanarray[1].id".SplitCsv(),
					new object[] {"E2", 10, 20});
				env.UndeployModuleContaining("windowInsertOne");

				// test configured Map type
				env.CompileDeploy("@name('s0') insert into MyConfiguredMap select sb as bean, s0Arr as beanarray from SupportBeanSourceEvent")
					.AddListener("s0");
				env.SendEventBean(theEvent);
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), "bean.theString,beanarray[0].id".SplitCsv(), new object[] {"E1", 2});

				env.UndeployAll();
			}
		}

		public class EPLOtherCreateSchemaCopyProperties : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
					TryAssertionSchemaCopyProperties(env, rep);
				}
			}

			private static void TryAssertionSchemaCopyProperties(
				RegressionEnvironment env,
				EventRepresentationChoice eventRepresentationEnum)
			{
				var path = new RegressionPath();
				var epl =
					eventRepresentationEnum.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedBaseOne>() +
					" create schema BaseOne (prop1 String, prop2 int);\n" +
					eventRepresentationEnum.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedBaseTwo>() +
					" create schema BaseTwo (prop3 long);\n" +
					eventRepresentationEnum.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedE1>() +
					" create schema E1 () copyfrom BaseOne;\n";
				env.CompileDeployWBusPublicType(epl, path);

				env.CompileDeploy("@name('s0') select * from E1", path).AddListener("s0");
				Assert.IsTrue(eventRepresentationEnum.MatchesClass(env.Statement("s0").EventType.UnderlyingType));
				Assert.AreEqual(typeof(string), env.Statement("s0").EventType.GetPropertyType("prop1"));
				Assert.AreEqual(typeof(int?), Boxing.GetBoxedType(env.Statement("s0").EventType.GetPropertyType("prop2")));

				if (eventRepresentationEnum.IsObjectArrayEvent()) {
					env.SendEventObjectArray(new object[] {"v1", 2}, "E1");
				}
				else if (eventRepresentationEnum.IsMapEvent()) {
					IDictionary<string, object> @event = new LinkedHashMap<string, object>();
					@event.Put("prop1", "v1");
					@event.Put("prop2", 2);
					env.SendEventMap(@event, "E1");
				}
				else if (eventRepresentationEnum.IsAvroEvent()) {
					var @event = new GenericRecord(SchemaBuilder.Record("name", RequiredString("prop1"), RequiredInt("prop2")));
					@event.Put("prop1", "v1");
					@event.Put("prop2", 2);
					env.SendEventAvro(@event, "E1");
				}
				else if (eventRepresentationEnum.IsJsonEvent() || eventRepresentationEnum.IsJsonProvidedClassEvent()) {
					var @object = new JObject();
					@object.Add("prop1", "v1");
					@object.Add("prop2", 2);
					env.SendEventJson(@object.ToString(), "E1");
				}
				else {
					Assert.Fail();
				}

				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), "prop1,prop2".SplitCsv(), new object[] {"v1", 2});
				env.UndeployModuleContaining("s0");

				// test two copy-from types
				env.CompileDeploy(
					eventRepresentationEnum.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedE2>() + " create schema E2 () copyfrom BaseOne, BaseTwo",
					path);
				env.CompileDeploy("@name('s0') select * from E2", path);
				var stmtEventType = env.Statement("s0").EventType;
				Assert.AreEqual(typeof(string), stmtEventType.GetPropertyType("prop1"));
				Assert.AreEqual(typeof(int?), Boxing.GetBoxedType(stmtEventType.GetPropertyType("prop2")));
				Assert.AreEqual(typeof(long?), Boxing.GetBoxedType(stmtEventType.GetPropertyType("prop3")));
				env.UndeployModuleContaining("s0");

				// test API-defined type
				if (!eventRepresentationEnum.IsAvroEvent() || eventRepresentationEnum.IsObjectArrayEvent() || eventRepresentationEnum.IsJsonEvent()) {
					env.CompileDeploy("create schema MyType(a string, b string, c BaseOne, d BaseTwo[])", path);
				}
				else if (eventRepresentationEnum.IsJsonProvidedClassEvent()) {
					env.CompileDeploy(
						"@JsonSchema(className='" +
						typeof(MyLocalJsonProvidedMyType).FullName +
						"') create json schema MyType(a string, b string, c BaseOne, d BaseTwo[])",
						path);
				}
				else {
					env.CompileDeploy("create avro schema MyType(a string, b string, c BaseOne, d BaseTwo[])", path);
				}

				env.CompileDeploy(
					eventRepresentationEnum.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedE3>() +
					" create schema E3(e long, f BaseOne) copyfrom MyType",
					path);
				env.CompileDeploy("@name('s0') select * from E3", path);
				var stmtThree = env.Statement("s0");
				Assert.AreEqual(typeof(string), stmtThree.EventType.GetPropertyType("a"));
				Assert.AreEqual(typeof(string), stmtThree.EventType.GetPropertyType("b"));
				if (eventRepresentationEnum.IsObjectArrayEvent()) {
					Assert.AreEqual(typeof(object[]), stmtThree.EventType.GetPropertyType("c"));
					Assert.AreEqual(typeof(object[][]), stmtThree.EventType.GetPropertyType("d"));
					Assert.AreEqual(typeof(object[]), stmtThree.EventType.GetPropertyType("f"));
				}
				else if (eventRepresentationEnum.IsMapEvent()) {
					Assert.AreEqual(typeof(IDictionary<string, object>), stmtThree.EventType.GetPropertyType("c"));
					Assert.AreEqual(typeof(IDictionary<string, object>[]), stmtThree.EventType.GetPropertyType("d"));
					Assert.AreEqual(typeof(IDictionary<string, object>), stmtThree.EventType.GetPropertyType("f"));
				}
				else if (eventRepresentationEnum.IsAvroEvent()) {
					Assert.AreEqual(typeof(GenericRecord), stmtThree.EventType.GetPropertyType("c"));
					Assert.AreEqual(typeof(GenericRecord[]), stmtThree.EventType.GetPropertyType("d"));
					Assert.AreEqual(typeof(GenericRecord), stmtThree.EventType.GetPropertyType("f"));
				}
				else if (eventRepresentationEnum.IsJsonEvent()) {
					Assert.IsTrue(TypeHelper.IsSubclassOrImplementsInterface(stmtThree.EventType.GetPropertyType("c"), typeof(JsonEventObject)));
					Assert.IsTrue(
						TypeHelper.IsSubclassOrImplementsInterface(stmtThree.EventType.GetPropertyType("d").GetElementType(), typeof(JsonEventObject)));
					Assert.IsTrue(TypeHelper.IsSubclassOrImplementsInterface(stmtThree.EventType.GetPropertyType("f"), typeof(JsonEventObject)));
				}
				else if (eventRepresentationEnum.IsJsonProvidedClassEvent()) {
					Assert.AreEqual(typeof(MyLocalJsonProvidedBaseOne), stmtThree.EventType.GetPropertyType("c"));
					Assert.AreEqual(typeof(MyLocalJsonProvidedBaseTwo[]), stmtThree.EventType.GetPropertyType("d"));
					Assert.AreEqual(typeof(MyLocalJsonProvidedBaseOne), stmtThree.EventType.GetPropertyType("f"));
				}
				else {
					Assert.Fail();
				}

				Assert.AreEqual(typeof(long?), Boxing.GetBoxedType(stmtThree.EventType.GetPropertyType("e")));

				// invalid tests
				var prefix = eventRepresentationEnum.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedDummy>();
				TryInvalidCompile(
					env,
					path,
					prefix + " create schema E4(a long) copyFrom MyType",
					"Duplicate column name 'a' [");
				TryInvalidCompile(
					env,
					path,
					prefix + " create schema E4(c BaseTwo) copyFrom MyType",
					"Duplicate column name 'c' [");
				TryInvalidCompile(
					env,
					path,
					prefix + " create schema E4(c BaseTwo) copyFrom XYZ",
					"Type by name 'XYZ' could not be located [");
				TryInvalidCompile(
					env,
					path,
					prefix + " create schema E4 as " + typeof(SupportBean).FullName + " copyFrom XYZ",
					"Copy-from types are not allowed with class-provided types [");
				TryInvalidCompile(
					env,
					path,
					prefix + " create variant schema E4(c BaseTwo) copyFrom XYZ",
					"Copy-from types are not allowed with variant types [");

				// test SODA
				prefix = eventRepresentationEnum.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedE2>();
				var createEPL = prefix + " create schema EX as () copyFrom BaseOne, BaseTwo";
				env.EplToModelCompileDeploy(createEPL, path).UndeployAll();
			}
		}

		public class EPLOtherCreateSchemaConfiguredNotRemoved : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{

				var path = new RegressionPath();
				env.CompileDeploy("@name('s1') create schema ABCType(col1 int, col2 int)", path);
				var deploymentIdS1 = env.DeploymentId("s1");
				Assert.IsNotNull(env.Runtime.EventTypeService.GetEventType(deploymentIdS1, "ABCType"));
				env.UndeployAll();
				Assert.IsNull(env.Runtime.EventTypeService.GetEventType(deploymentIdS1, "ABCType"));

				AssertTypeExistsPreconfigured(env, "SupportBean");
				AssertTypeExistsPreconfigured(env, "MapTypeEmpty");
				AssertTypeExistsPreconfigured(env, "TestXMLNoSchemaType");
			}
		}

		public class EPLOtherCreateSchemaInvalid : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
					TryAssertionInvalid(env, rep);
				}

				TryInvalidCompile(
					env,
					"create objectarray schema A();\n" +
					"create objectarray schema B();\n" +
					"create objectarray schema InvalidOA () inherits A, B;\n",
					"Object-array event types only allow a single supertype");
			}

			private static void TryAssertionInvalid(
				RegressionEnvironment env,
				EventRepresentationChoice eventRepresentationEnum)
			{
				var expectedOne = !eventRepresentationEnum.IsAvroEvent()
					? "Nestable type configuration encountered an unexpected property type name 'xxxx' for property 'col1', expected System.Type or java.util.Map or the name of a previously-declared event type ["
					: "Type definition encountered an unexpected property type name 'xxxx' for property 'col1', expected the name of a previously-declared Avro type";
				var prefix = eventRepresentationEnum.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedDummy>();
				TryInvalidCompile(env, $"{prefix} create schema MyEventType as (col1 xxxx)", expectedOne);

				TryInvalidCompile(
					env,
					$"{prefix} create schema MyEventType as (col1 int, col1 string)",
					"Duplicate column name 'col1' [");

				var path = new RegressionPath();
				env.CompileDeploy($"{prefix} create schema MyEventType as (col1 string)", path);
				var expectedTwo = "Event type named 'MyEventType' has already been declared";
				TryInvalidCompile(env, path, "create schema MyEventType as (col1 string, col2 string)", expectedTwo);

				TryInvalidCompile(
					env,
					$"{prefix} create schema MyEventTypeT1 as () inherit ABC",
					"Expected 'inherits', 'starttimestamp', 'endtimestamp' or 'copyfrom' keyword after create-schema clause but encountered 'inherit' [");

				TryInvalidCompile(
					env,
					$"{prefix} create schema MyEventTypeT2 as () inherits ABC",
					"Supertype by name 'ABC' could not be found [");

				TryInvalidCompile(
					env,
					$"{prefix} create schema MyEventTypeT3 as () inherits",
					"Incorrect syntax near end-of-input expecting an identifier but found end-of-input at line 1 column ");

				env.UndeployAll();
			}
		}

		public class EPLOtherCreateSchemaAvroSchemaWAnnotation : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var schema = SchemaBuilder.Union(IntType(), StringType());
				var epl = $"@AvroSchemaField(name='carId',schema='{schema}') create avro schema MyEvent(carId object)";
				env.CompileDeploy(epl);
				env.UndeployAll();
			}
		}

		public class EPLOtherCreateSchemaColDefPlain : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
					TryAssertionColDefPlain(env, rep);
				}

				// test property classname, either simple or fully-qualified.
				var path = new RegressionPath();
				env.CompileDeploy("@name('create') create schema MySchema (f1 TimeSpan, f2 System.Drawing.PointF, f3 EventHandler, f4 null)", path);

				var eventType = env.Runtime.EventTypeService.GetEventType(env.DeploymentId("create"), "MySchema");
				Assert.AreEqual(typeof(TimeSpan?), eventType.GetPropertyType("f1"));
				Assert.AreEqual(typeof(PointF?), eventType.GetPropertyType("f2"));
				Assert.AreEqual(typeof(EventHandler), eventType.GetPropertyType("f3"));
				Assert.AreEqual(null, eventType.GetPropertyType("f4"));

				env.UndeployAll();
			}
		}

		public class EPLOtherCreateSchemaModelPONO : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				var schema = "@name('c1') create schema SupportBeanOne as " +
				             typeof(SupportBean_ST0).FullName +
				             ";\n" +
				             "@name('c2') create schema SupportBeanTwo as " +
				             typeof(SupportBean_ST0).FullName +
				             ";\n";
				env.CompileDeployWBusPublicType(schema, path);

				Assert.AreEqual(typeof(SupportBean_ST0), env.Statement("c1").EventType.UnderlyingType);
				Assert.AreEqual(typeof(SupportBean_ST0), env.Statement("c2").EventType.UnderlyingType);

				env.CompileDeploy("@name('s0') select * from SupportBeanOne", path).AddListener("s0");
				Assert.AreEqual(typeof(SupportBean_ST0), env.Statement("s0").EventType.UnderlyingType);

				env.CompileDeploy("@name('s1') select * from SupportBeanTwo", path).AddListener("s1");
				Assert.AreEqual(typeof(SupportBean_ST0), env.Statement("s1").EventType.UnderlyingType);

				env.SendEventBean(new SupportBean_ST0("E1", 2), "SupportBeanOne");
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), "id,p00".SplitCsv(), new object[] {"E1", 2});
				Assert.IsFalse(env.Listener("s1").IsInvoked);

				env.SendEventBean(new SupportBean_ST0("E2", 3), "SupportBeanTwo");
				EPAssertionUtil.AssertProps(env.Listener("s1").AssertOneGetNewAndReset(), "id,p00".SplitCsv(), new object[] {"E2", 3});
				Assert.IsFalse(env.Listener("s0").IsInvoked);

				// assert type information
				var type = env.Statement("s0").EventType;
				Assert.AreEqual(EventTypeTypeClass.STREAM, type.Metadata.TypeClass);

				// test keyword
				TryInvalidCompile(
					env,
					"create schema MySchemaInvalid as com.mycompany.event.ABC",
					"Could not load class by name 'com.mycompany.event.ABC', please check imports");
				TryInvalidCompile(
					env,
					"create schema MySchemaInvalid as com.mycompany.events.ABC",
					"Could not load class by name 'com.mycompany.events.ABC', please check imports");

				env.UndeployAll();
			}
		}

		public class EPLOtherCreateSchemaNestableMapArray : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
					TryAssertionNestableMapArray(env, rep);
				}
			}
		}

		public class EPLOtherCreateSchemaInherit : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				env.CompileDeploy("create schema MyParentType as (col1 int, col2 string)", path);
				env.CompileDeploy("@name('child') create schema MyChildTypeOne (col3 int) inherits MyParentType", path);

				var childType = env.Statement("child").EventType;
				Assert.AreEqual(typeof(int?), childType.GetPropertyType("col1"));
				Assert.AreEqual(typeof(string), childType.GetPropertyType("col2"));
				Assert.AreEqual(typeof(int?), childType.GetPropertyType("col3"));

				env.CompileDeploy("create schema MyChildTypeTwo as (col4 boolean)", path);
				var createText = "@name('childchild') create schema MyChildChildType as (col5 short, col6 long) inherits MyChildTypeOne, MyChildTypeTwo";
				var model = env.EplToModel(createText);
				Assert.AreEqual(createText, model.ToEPL());
				env.CompileDeploy(model, path);
				var stmtChildChildType = env.Statement("childchild").EventType;
				Assert.AreEqual(typeof(bool?), stmtChildChildType.GetPropertyType("col4"));
				Assert.AreEqual(typeof(int?), stmtChildChildType.GetPropertyType("col3"));
				Assert.AreEqual(typeof(short?), stmtChildChildType.GetPropertyType("col5"));

				env.CompileDeploy("@name('cc2') create schema MyChildChildTypeTwo () inherits MyChildTypeOne, MyChildTypeTwo", path);
				var eventTypeCC2 = env.Statement("cc2").EventType;
				Assert.AreEqual(typeof(bool?), eventTypeCC2.GetPropertyType("col4"));
				Assert.AreEqual(typeof(int?), eventTypeCC2.GetPropertyType("col3"));

				env.UndeployAll();
			}
		}

		public class EPLOtherCreateSchemaVariantType : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				var epl = "create schema MyTypeZero as (col1 int, col2 string);\n" +
				          "create schema MyTypeOne as (col1 int, col3 string, col4 int);\n" +
				          "create schema MyTypeTwo as (col1 int, col4 boolean, col5 short)";
				env.CompileDeploy(epl, path);

				// try predefined
				env.CompileDeploy("@name('predef') create variant schema MyVariantPredef as MyTypeZero, MyTypeOne", path);
				var variantTypePredef = env.Statement("predef").EventType;
				Assert.AreEqual(typeof(int?), variantTypePredef.GetPropertyType("col1"));
				Assert.AreEqual(1, variantTypePredef.PropertyDescriptors.Count);

				env.CompileDeploy("insert into MyVariantPredef select * from MyTypeZero", path);
				env.CompileDeploy("insert into MyVariantPredef select * from MyTypeOne", path);
				TryInvalidCompile(
					env,
					path,
					"insert into MyVariantPredef select * from MyTypeTwo",
					"Selected event type is not a valid event type of the variant stream 'MyVariantPredef' [insert into MyVariantPredef select * from MyTypeTwo]");

				// try predefined with any
				var createText = "@name('predef_any') create variant schema MyVariantAnyModel as MyTypeZero, MyTypeOne, *";
				var model = env.EplToModel(createText);
				Assert.AreEqual(createText, model.ToEPL());
				env.CompileDeploy(model, path);
				var predefAnyType = env.Statement("predef_any").EventType;
				Assert.AreEqual(4, predefAnyType.PropertyDescriptors.Count);
				Assert.AreEqual(typeof(object), predefAnyType.GetPropertyType("col1"));
				Assert.AreEqual(typeof(object), predefAnyType.GetPropertyType("col2"));
				Assert.AreEqual(typeof(object), predefAnyType.GetPropertyType("col3"));
				Assert.AreEqual(typeof(object), predefAnyType.GetPropertyType("col4"));

				// try "any"
				env.CompileDeploy("@name('any') create variant schema MyVariantAny as *", path);
				var variantTypeAny = env.Statement("any").EventType;
				Assert.AreEqual(0, variantTypeAny.PropertyDescriptors.Count);

				env.CompileDeploy("insert into MyVariantAny select * from MyTypeZero", path);
				env.CompileDeploy("insert into MyVariantAny select * from MyTypeOne", path);
				env.CompileDeploy("insert into MyVariantAny select * from MyTypeTwo", path);

				env.UndeployAll();
			}
		}

		private static void TryAssertionColDefPlain(
			RegressionEnvironment env,
			EventRepresentationChoice eventRepresentationEnum)
		{
			var path = new RegressionPath();
			env.CompileDeploy(
				"@name('create') " +
				eventRepresentationEnum.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedMyEventTypeCol1To4>() +
				" create schema MyEventType as (col1 string, col2 int, col3col4 int)",
				path);
			AssertTypeColDef(env.Statement("create").EventType);
			env.CompileDeploy(
				"@name('select') " +
				eventRepresentationEnum.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedMyEventTypeCol1To4>() +
				" select * from MyEventType",
				path);
			AssertTypeColDef(env.Statement("select").EventType);
			env.UndeployAll();

			// destroy and create differently
			env.CompileDeploy(
				"@name('create') " +
				eventRepresentationEnum.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedMyEventTypCol34>() +
				" create schema MyEventType as (col3 string, col4 int)");
			Assert.AreEqual(typeof(int?), Boxing.GetBoxedType(env.Statement("create").EventType.GetPropertyType("col4")));
			Assert.AreEqual(2, env.Statement("create").EventType.PropertyDescriptors.Count);
			env.UndeployAll();

			// destroy and create differently
			path.Clear();
			var schemaEPL = "@name('create') " +
			                eventRepresentationEnum.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedMyEventTypCol56>() +
			                " create schema MyEventType as (col5 string, col6 int)";
			env.CompileDeployWBusPublicType(schemaEPL, path);

			Assert.IsTrue(eventRepresentationEnum.MatchesClass(env.Statement("create").EventType.UnderlyingType));
			Assert.AreEqual(typeof(int?), Boxing.GetBoxedType(env.Statement("create").EventType.GetPropertyType("col6")));
			Assert.AreEqual(2, env.Statement("create").EventType.PropertyDescriptors.Count);
			env.CompileDeploy(
					"@name('select') " +
					eventRepresentationEnum.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedMyEventTypCol56>() +
					" select * from MyEventType",
					path)
				.AddListener("select");
			Assert.IsTrue(eventRepresentationEnum.MatchesClass(env.Statement("select").EventType.UnderlyingType));

			// send event
			if (eventRepresentationEnum.IsMapEvent()) {
				IDictionary<string, object> data = new LinkedHashMap<string, object>();
				data.Put("col5", "abc");
				data.Put("col6", 1);
				env.SendEventMap(data, "MyEventType");
			}
			else if (eventRepresentationEnum.IsObjectArrayEvent()) {
				env.SendEventObjectArray(new object[] {"abc", 1}, "MyEventType");
			}
			else if (eventRepresentationEnum.IsAvroEvent()) {
				var avroType = env.Runtime.EventTypeService.GetEventType(env.DeploymentId("create"), "MyEventType");
				var schema = AvroSchemaUtil.ResolveAvroSchema(avroType).AsRecordSchema();
				var @event = new GenericRecord(schema);
				@event.Put("col5", "abc");
				@event.Put("col6", 1);
				env.SendEventAvro(@event, "MyEventType");
			}
			else if (eventRepresentationEnum.IsJsonEvent() || eventRepresentationEnum.IsJsonProvidedClassEvent()) {
				var @object = new JObject();
				@object.Add("col5", "abc");
				@object.Add("col6", 1);
				env.SendEventJson(@object.ToString(), "MyEventType");
			}
			else {
				Assert.Fail();
			}

			EPAssertionUtil.AssertProps(env.Listener("select").AssertOneGetNewAndReset(), "col5,col6".SplitCsv(), new object[] {"abc", 1});

			// assert type information
			var type = env.Statement("select").EventType;
			Assert.AreEqual(EventTypeTypeClass.STREAM, type.Metadata.TypeClass);
			Assert.AreEqual(type.Name, type.Metadata.Name);

			// test non-enum create-schema
			var epl = eventRepresentationEnum.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedMyEventTypeTwo>() +
			          " @name('c2') create schema MyEventTypeTwo as (col1 string, col2 int, col3col4 int)";
			env.CompileDeploy(epl);
			AssertTypeColDef(env.Statement("c2").EventType);
			Assert.IsTrue(eventRepresentationEnum.MatchesClass(env.Statement("c2").EventType.UnderlyingType));
			env.UndeployModuleContaining("c2");

			env.EplToModelCompileDeploy(epl);
			AssertTypeColDef(env.Statement("c2").EventType);
			Assert.IsTrue(eventRepresentationEnum.MatchesClass(env.Statement("c2").EventType.UnderlyingType));

			env.UndeployAll();
		}

		private static void TryAssertionNestableMapArray(
			RegressionEnvironment env,
			EventRepresentationChoice eventRepresentationEnum)
		{
			var path = new RegressionPath();
			var schema =
				"@name('innerType') " +
				eventRepresentationEnum.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedNestableArray>() +
				" create schema MyInnerType as (inn1 string[], inn2 int[]);\n" +
				"@name('outerType') " +
				eventRepresentationEnum.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedNestableOuter>() +
				" create schema MyOuterType as (col1 MyInnerType, col2 MyInnerType[]);\n";
			env.CompileDeployWBusPublicType(schema, path);

			var innerType = env.Statement("innerType").EventType;
			Assert.AreEqual(eventRepresentationEnum.IsAvroEvent() ? typeof(GenericRecord[]) : typeof(string[]), innerType.GetPropertyType("inn1"));
			Assert.IsTrue(innerType.GetPropertyDescriptor("inn1").IsIndexed);
			Assert.AreEqual(eventRepresentationEnum.IsAvroEvent() ? typeof(GenericRecord[]) : typeof(int?[]), innerType.GetPropertyType("inn2"));
			Assert.IsTrue(innerType.GetPropertyDescriptor("inn2").IsIndexed);
			Assert.IsTrue(eventRepresentationEnum.MatchesClass(innerType.UnderlyingType));

			var outerType = env.Statement("outerType").EventType;
			var type = outerType.GetFragmentType("col1");
			Assert.IsFalse(type.IsIndexed);
			Assert.AreEqual(false, type.IsNative);
			type = outerType.GetFragmentType("col2");
			Assert.IsTrue(type.IsIndexed);
			Assert.AreEqual(false, type.IsNative);

			env.CompileDeploy("@name('s0') select * from MyOuterType", path).AddListener("s0");
			Assert.IsTrue(eventRepresentationEnum.MatchesClass(env.Statement("s0").EventType.UnderlyingType));

			if (eventRepresentationEnum.IsObjectArrayEvent()) {
				var innerData = new object[] {"abc,def".SplitCsv(), new[] {1, 2}};
				var outerData = new object[] {innerData, new object[] {innerData, innerData}};
				env.SendEventObjectArray(outerData, "MyOuterType");
			}
			else if (eventRepresentationEnum.IsMapEvent()) {
				var innerData = new Dictionary<string, object>();
				innerData.Put("inn1", "abc,def".SplitCsv());
				innerData.Put("inn2", new[] {1, 2});
				var outerData = new Dictionary<string, object>();
				outerData.Put("col1", innerData);
				outerData.Put("col2", new[] {innerData, innerData});
				env.SendEventMap(outerData, "MyOuterType");
			}
			else if (eventRepresentationEnum.IsAvroEvent()) {
				var innerData = new GenericRecord(SupportAvroUtil.GetAvroSchema(innerType).AsRecordSchema());
				innerData.Put("inn1", Arrays.AsList("abc", "def"));
				innerData.Put("inn2", Arrays.AsList(1, 2));
				var outerData = new GenericRecord(SupportAvroUtil.GetAvroSchema(outerType).AsRecordSchema());
				outerData.Put("col1", innerData);
				outerData.Put("col2", Arrays.AsList(innerData, innerData));
				env.SendEventAvro(outerData, "MyOuterType");
			}
			else if (eventRepresentationEnum.IsJsonEvent() || eventRepresentationEnum.IsJsonProvidedClassEvent()) {
				var inn1 = new JArray(new JValue("abc"), new JValue("def"));
				var inn2 = new JArray(new JValue(1), new JValue(2));
				var inn = new JObject(new JProperty("inn1", inn1), new JProperty("inn2", inn2));
				var outer = new JObject(new JProperty("col1", inn));
				var col2 = new JArray(inn, inn);
				outer.Add("col2", col2);
				env.SendEventJson(outer.ToString(), "MyOuterType");
			}
			else {
				Assert.Fail();
			}

			EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), "col1.inn1[1],col2[1].inn2[1]".SplitCsv(), new object[] {"def", 2});

			env.UndeployAll();
		}

		private static void CompileDeployWExport(
			string epl,
			bool soda,
			RegressionEnvironment env)
		{
			EPCompiled compiled;
			if (!soda) {
				compiled = env.CompileWBusPublicType(epl);
			}
			else {
				var model = env.EplToModel(epl);
				var module = new Module();
				module.Items.Add(new ModuleItem(model));

				var args = new CompilerArguments();
				args.Configuration = env.Configuration;
				args.Options
					.SetAccessModifierEventType(ctx => NameAccessModifier.PUBLIC)
					.SetBusModifierEventType(ctx => EventTypeBusModifier.BUS);
				compiled = env.Compiler.Compile(module, args);
			}

			env.Deploy(compiled);
		}

		private static void AssertTypeExistsPreconfigured(
			RegressionEnvironment env,
			string typeName)
		{
			Assert.IsNotNull(env.Runtime.EventTypeService.GetEventTypePreconfigured(typeName));
		}

		private static EventType GetTypeStmt(
			RegressionEnvironment env,
			string statementName)
		{
			return env.Statement(statementName).EventType;
		}

		private static void AssertTypeColDef(EventType eventType)
		{
			Assert.AreEqual(typeof(string), eventType.GetPropertyType("col1"));
			Assert.AreEqual(typeof(int?), Boxing.GetBoxedType(eventType.GetPropertyType("col2")));
			Assert.AreEqual(typeof(int?), Boxing.GetBoxedType(eventType.GetPropertyType("col3col4")));
			Assert.AreEqual(3, eventType.PropertyDescriptors.Count);
		}

		// ReSharper disable UnusedMember.Global
		// ReSharper disable InconsistentNaming
		[Serializable]
		public class MyLocalJsonProvidedMyEventTypeCol1To4
		{
			public string col1;
			public int col2;
			public int col3col4;
		}

		[Serializable]
		public class MyLocalJsonProvidedMyEventTypCol34
		{
			public string col3;
			public int col4;
		}

		[Serializable]
		public class MyLocalJsonProvidedMyEventTypCol56
		{
			public string col5;
			public int col6;
		}

		[Serializable]
		public class MyLocalJsonProvidedNestableArray
		{
			public string[] inn1;
			public int?[] inn2;
		}

		[Serializable]
		public class MyLocalJsonProvidedNestableOuter
		{
			public MyLocalJsonProvidedNestableArray col1;
			public MyLocalJsonProvidedNestableArray[] col2;
		}

		[Serializable]
		public class MyLocalJsonProvidedBaseOne
		{
			public string prop1;
			public int prop2;
		}

		[Serializable]
		public class MyLocalJsonProvidedBaseTwo
		{
			public long prop3;
		}

		[Serializable]
		public class MyLocalJsonProvidedE1
		{
			public string prop1;
			public int prop2;
		}

		[Serializable]
		public class MyLocalJsonProvidedE2
		{
			public string prop1;
			public int prop2;
			public long prop3;
		}

		[Serializable]
		public class MyLocalJsonProvidedE3
		{
			public string a;
			public string b;
			public MyLocalJsonProvidedBaseOne c;
			public MyLocalJsonProvidedBaseTwo[] d;
			public MyLocalJsonProvidedBaseOne f;
			public long e;
		}

		[Serializable]
		public class MyLocalJsonProvidedDummy
		{
			public string col1;
		}

		[Serializable]
		public class MyLocalJsonProvidedMyType
		{
			public string a;
			public string b;
			public MyLocalJsonProvidedBaseOne c;
			public MyLocalJsonProvidedBaseTwo[] d;
		}

		[Serializable]
		public class MyLocalJsonProvidedMyEventTypeTwo
		{
			public string col1;
			public int col2;
			public int col3col4;
		}

		public static MyLocalValueObject GetLocalValueObject()
		{
			return new MyLocalValueObject();
		}

		public class MyLocalValueObject
		{
		}
		// ReSharper restore InconsistentNaming
		// ReSharper restore UnusedMember.Global
	}
} // end of namespace
