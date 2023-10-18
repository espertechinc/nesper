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
using System.Text;

using Avro;
using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.json.util;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.module;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;

using Microsoft.CodeAnalysis;

using NEsper.Avro.Extensions;

using NUnit.Framework;

using static com.espertech.esper.common.@internal.support.SupportEventPropUtil; // assertPropEquals;
//using static com.espertech.esper.regressionlib.support.events.SupportGenericColUtil;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compiler.client;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;

using Newtonsoft.Json.Linq;

using static com.espertech.esper.regressionlib.support.events.SupportGenericColUtil;

using Exception = System.Exception;

namespace com.espertech.esper.regressionlib.suite.epl.other {
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
			execs.Add(new EPLOtherCreateSchemaTypeParameterized());
			return execs;
		}

		internal class EPLOtherCreateSchemaTypeParameterized : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				TryAssertionTypeParameterized(env, GetSchema(EventRepresentationChoice.OBJECTARRAY), "MyEvent");
				TryAssertionTypeParameterized(env, GetSchema(EventRepresentationChoice.MAP), "MyEvent");

				var beanSchema = "@name('schema') create schema MyEvent as " +
				                 typeof(MyLocalSchemaTypeParamEvent<>).FullName +
				                 ";\n";
				TryAssertionTypeParameterized(env, beanSchema, "MyEvent");

				TryAssertionTypeParameterized(env, null, "MyPreconfiguredParameterizeTypeMap");
			}

			private string GetSchema(EventRepresentationChoice rep)
			{
				var buf = new StringBuilder();
				buf.Append("@name('schema') ").Append(rep.GetAnnotationText()).Append("create schema MyEvent(");
				var delimiter = "";
				foreach (var pair in NAMESANDTYPES) {
					buf
						.Append(delimiter)
						.Append(pair.Name)
						.Append(" ")
						.Append(pair.TypeName);
					delimiter = ",";
				}

				buf.Append(");\n");
				return buf.ToString();
			}

			private void TryAssertionTypeParameterized(
				RegressionEnvironment env,
				string schemaEPL,
				string eventTypeName)
			{
				var epl = schemaEPL == null ? "" : schemaEPL;
				epl += "@name('s0') select " + AllNames() + " from " + eventTypeName + ";\n";
				env.CompileDeploy(epl);

				if (schemaEPL != null) {
					env.AssertStatement("schema", statement => AssertPropertyTypes(statement.EventType));
				}

				env.AssertStatement("s0", statement => AssertPropertyTypes(statement.EventType));

				env.UndeployAll();
			}
		}

		internal class EPLOtherCreateSchemaCopyFromDeepWithValueObject : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "create schema SchemaA (account string, foo " +
				          typeof(MyLocalValueObject).Name +
				          ");\n" +
				          "create schema SchemaB (symbol string) copyfrom SchemaA;\n" +
				          "create schema SchemaC () copyfrom SchemaB;\n" +
				          "create schema SchemaD () copyfrom SchemaB;\n" +
				          "insert into SchemaD select account, " +
				          typeof(EPLOtherCreateSchema).Name +
				          ".getLocalValueObject() as foo, symbol from SchemaC;\n";
				env.CompileDeploy(epl).UndeployAll();
			}
		}

		internal class EPLOtherCreateSchemaBeanImport : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				env.CompileDeploy("create schema MyEvent as Rectangle");

				env.TryInvalidCompile(
					"create schema MyEvent as XXUnknown",
					"Could not load class by name 'XXUnknown', please check imports");

				env.UndeployAll();
			}
		}

		internal class EPLOtherCreateSchemaSameCRC : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				try {
					env.CompileDeploy(
						"@Public @buseventtype create schema b5a7b602ab754d7ab30fb42c4fb28d82();\n" +
						"@Public @buseventtype create schema d19f2e9e82d14b96be4fa12b8a27ee9f();",
						new RegressionPath());
				}
				catch (Exception ex) {
					Assert.AreEqual(
						"Test failed due to exception: Event type by name 'd19f2e9e82d14b96be4fa12b8a27ee9f' has a public crc32 id overlap with event type by name 'b5a7b602ab754d7ab30fb42c4fb28d82', please consider renaming either of these types",
						ex.Message);
				}
			}

			public ISet<RegressionFlag> Flags()
			{
				return Collections.Set(RegressionFlag.INVALIDITY);
			}
		}

		internal class EPLOtherCreateSchemaPathSimple : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "@name('schema') create schema SimpleSchema(p0 string, p1 int);" +
				          "@name('s0') select * from SimpleSchema;\n" +
				          "insert into SimpleSchema select theString as p0, intPrimitive as p1 from SupportBean;\n";
				env.CompileDeploy(epl).AddListener("s0");
				env.AssertStatement(
					"schema",
					statement => {
						Assert.AreEqual(
							StatementType.CREATE_SCHEMA,
							statement.GetProperty(StatementProperty.STATEMENTTYPE));
						Assert.AreEqual("SimpleSchema", statement.GetProperty(StatementProperty.CREATEOBJECTNAME));
					});

				env.SendEventBean(new SupportBean("a", 20));
				env.AssertPropsNew("s0", "p0,p1".Split(","), new object[] { "a", 20 });

				env.AssertThat(() => Assert.IsNull(env.Runtime.EventTypeService.GetBusEventType("SimpleSchema")));

				env.UndeployAll();
			}
		}

		internal class EPLOtherCreateSchemaPublicSimple : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "@buseventtype @public create schema MySchema as (p0 string, p1 int);\n" +
				          "@name('s0') select p0, p1 from MySchema;\n";
				env.CompileDeploy(epl, new RegressionPath()).AddListener("s0");

				env.SendEventMap(CollectionUtil.BuildMap("p0", "a", "p1", 20), "MySchema");
				env.AssertPropsNew("s0", "p0,p1".Split(","), new object[] { "a", 20 });

				env.AssertThat(
					() => {
						var eventType = env.Runtime.EventTypeService.GetBusEventType("MySchema");
						Assert.AreEqual("MySchema", eventType.Name);
					});

				env.UndeployAll();
			}
		}

		internal class EPLOtherCreateSchemaCopyFromOrderObjectArray : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				var epl =
					"@name('s1') @public @buseventtype create objectarray schema MyEventOne(p0 string, p1 double);\n " +
					"@Public create objectarray schema MyEventTwo(p2 string) copyfrom MyEventOne;\n";
				env.CompileDeploy(epl, path);

				env.AssertThat(
					() => {
						var type = env.Runtime.EventTypeService.GetEventType(env.DeploymentId("s1"), "MyEventTwo");
						EPAssertionUtil.AssertEqualsExactOrder("p0,p1,p2".Split(","), type.PropertyNames);
					});

				epl = "insert into MyEventTwo select 'abc' as p2, s.* from MyEventOne as s;\n" +
				      "@name('s0') select p0, p1, p2 from MyEventTwo;\n";
				env.CompileDeploy(epl, path).AddListener("s0");

				env.SendEventObjectArray(new object[] { "E1", 10d }, "MyEventOne");
				env.AssertPropsNew("s0", "p0,p1,p2".Split(","), new object[] { "E1", 10d, "abc" });

				env.UndeployAll();
			}
		}

		internal class EPLOtherCreateSchemaArrayPrimitiveType : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				TryAssertionSchemaArrayPrimitiveType(env, true);
				TryAssertionSchemaArrayPrimitiveType(env, false);

				env.TryInvalidCompile(
					"create schema Invalid (x dummy[primitive])",
					"Type 'dummy' is not a primitive type [create schema Invalid (x dummy[primitive])]");
				env.TryInvalidCompile(
					"create schema Invalid (x int[dummy])",
					"Invalid array keyword 'dummy', expected 'primitive'");
				env.TryInvalidCompile(
					"create schema Invalid (x int<string>[primitive])",
					"Cannot use the 'primitive' keyword with type parameters");
			}

			private static void TryAssertionSchemaArrayPrimitiveType(
				RegressionEnvironment env,
				bool soda)
			{
				CompileDeployWExport(
					"@name('schema') @public @buseventtype create schema MySchema as (c0 int[primitive], c1 int[])",
					soda,
					env);
				var expectedType = new object[][]
					{ new object[] { "c0", typeof(int[]) }, new object[] { "c1", typeof(int?[]) } };
				env.AssertStatement(
					"schema",
					statement => SupportEventTypeAssertionUtil.AssertEventTypeProperties(
						expectedType,
						statement.EventType,
						SupportEventTypeAssertionEnum.NAME,
						SupportEventTypeAssertionEnum.TYPE));
				env.UndeployAll();
			}

			public ISet<RegressionFlag> Flags()
			{
				return Collections.Set(RegressionFlag.INVALIDITY);
			}
		}

		internal class EPLOtherCreateSchemaWithEventType : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var theEvent = new SupportBeanSourceEvent(
					new SupportBean("E1", 1),
					new SupportBean_S0[] { new SupportBean_S0(2) });

				// test schema
				env.CompileDeploy(
					"@name('schema') create schema MySchema (bean SupportBean, beanarray SupportBean_S0[])");
				env.AssertStatement(
					"schema",
					statement => {
						var stmtSchemaType = statement.EventType;
						AssertPropEquals(
							new SupportEventPropDesc("bean", typeof(SupportBean)).WithFragment(),
							stmtSchemaType.GetPropertyDescriptor("bean"));
						AssertPropEquals(
							new SupportEventPropDesc("beanarray", typeof(SupportBean_S0[])).WithIndexed()
								.WithFragment(),
							stmtSchemaType.GetPropertyDescriptor("beanarray"));
					});

				env.CompileDeploy(
						"@name('s0') insert into MySchema select sb as bean, s0Arr as beanarray from SupportBeanSourceEvent")
					.AddListener("s0");
				env.SendEventBean(theEvent);
				env.AssertPropsNew("s0", "bean.theString,beanarray[0].id".Split(","), new object[] { "E1", 2 });
				env.UndeployModuleContaining("s0");

				// test named window
				var path = new RegressionPath();
				env.CompileDeploy(
						"@name('window') @public create window MyWindow#keepall as (bean SupportBean, beanarray SupportBean_S0[])",
						path)
					.AddListener("window");
				env.AssertStatement(
					"window",
					statement => {
						var stmtWindowType = statement.EventType;
						AssertPropEquals(
							new SupportEventPropDesc("bean", typeof(SupportBean)).WithFragment(),
							stmtWindowType.GetPropertyDescriptor("bean"));
						AssertPropEquals(
							new SupportEventPropDesc("beanarray", typeof(SupportBean_S0[])).WithIndexed()
								.WithFragment(),
							stmtWindowType.GetPropertyDescriptor("beanarray"));
					});

				env.CompileDeploy(
					"@name('windowInsertOne') insert into MyWindow select sb as bean, s0Arr as beanarray from SupportBeanSourceEvent",
					path);
				env.SendEventBean(theEvent);
				env.AssertPropsNew("window", "bean.theString,beanarray[0].id".Split(","), new object[] { "E1", 2 });
				env.UndeployModuleContaining("windowInsertOne");

				// insert pattern to named window
				env.CompileDeploy(
					"@name('windowInsertOne') insert into MyWindow select sb as bean, s0Arr as beanarray from pattern [sb=SupportBean -> s0Arr=SupportBean_S0 until SupportBean_S0(id=0)]",
					path);
				env.SendEventBean(new SupportBean("E2", 2));
				env.SendEventBean(new SupportBean_S0(10, "S0_1"));
				env.SendEventBean(new SupportBean_S0(20, "S0_2"));
				env.SendEventBean(new SupportBean_S0(0, "S0_3"));
				env.AssertPropsNew(
					"window",
					"bean.theString,beanarray[0].id,beanarray[1].id".Split(","),
					new object[] { "E2", 10, 20 });
				env.UndeployModuleContaining("windowInsertOne");

				// test configured Map type
				env.CompileDeploy(
						"@name('s0') insert into MyConfiguredMap select sb as bean, s0Arr as beanarray from SupportBeanSourceEvent")
					.AddListener("s0");
				env.SendEventBean(theEvent);
				env.AssertPropsNew("s0", "bean.theString,beanarray[0].id".Split(","), new object[] { "E1", 2 });

				env.UndeployAll();
			}
		}

		internal class EPLOtherCreateSchemaCopyProperties : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
					TryAssertionSchemaCopyProperties(env, rep);
				}
			}

			private static void TryAssertionSchemaCopyProperties(
				RegressionEnvironment env,
				EventRepresentationChoice eventRepresentationEnum)
			{
				var path = new RegressionPath();
				var epl =
					eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedBaseOne)) +
					" @public @buseventtype create schema BaseOne (prop1 String, prop2 int);\n" +
					eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedBaseTwo)) +
					" @public @buseventtype create schema BaseTwo (prop3 long);\n" +
					eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedE1)) +
					" @public @buseventtype create schema E1 () copyfrom BaseOne;\n";
				env.CompileDeploy(epl, path);

				env.CompileDeploy("@name('s0') select * from E1", path).AddListener("s0");
				env.AssertStatement(
					"s0",
					statement => {
						Assert.IsTrue(eventRepresentationEnum.MatchesClass(statement.EventType.UnderlyingType));
						Assert.AreEqual(typeof(string), statement.EventType.GetPropertyType("prop1"));
						Assert.AreEqual(
							typeof(int?),
							Boxing.GetBoxedType(statement.EventType.GetPropertyType("prop2")));
					});

				if (eventRepresentationEnum.IsObjectArrayEvent()) {
					env.SendEventObjectArray(new object[] { "v1", 2 }, "E1");
				}
				else if (eventRepresentationEnum.IsMapEvent()) {
					IDictionary<string, object> @event = new LinkedHashMap<string, object>();
					@event.Put("prop1", "v1");
					@event.Put("prop2", 2);
					env.SendEventMap(@event, "E1");
				}
				else if (eventRepresentationEnum.IsAvroEvent()) {
					var @event = new GenericRecord(
						SchemaBuilder.Record(
							"name",
							TypeBuilder.RequiredString("prop1"),
							TypeBuilder.RequiredInt("prop2")));
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

				env.AssertPropsNew("s0", "prop1,prop2".Split(","), new object[] { "v1", 2 });
				env.UndeployModuleContaining("s0");

				// test two copy-from types
				env.CompileDeploy(
					eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedE2)) +
					" @public create schema E2 () copyfrom BaseOne, BaseTwo",
					path);
				env.CompileDeploy("@name('s0') select * from E2", path);
				env.AssertStatement(
					"s0",
					statement => {
						var stmtEventType = statement.EventType;
						Assert.AreEqual(typeof(string), stmtEventType.GetPropertyType("prop1"));
						Assert.AreEqual(typeof(int?), Boxing.GetBoxedType(stmtEventType.GetPropertyType("prop2")));
						Assert.AreEqual(typeof(long?), Boxing.GetBoxedType(stmtEventType.GetPropertyType("prop3")));
					});
				env.UndeployModuleContaining("s0");

				// test API-defined type
				if (!eventRepresentationEnum.IsAvroEvent() ||
				    eventRepresentationEnum.IsObjectArrayEvent() ||
				    eventRepresentationEnum.IsJsonEvent()) {
					env.CompileDeploy("@Public create schema MyType(a string, b string, c BaseOne, d BaseTwo[])", path);
				}
				else if (eventRepresentationEnum.IsJsonProvidedClassEvent()) {
					env.CompileDeploy(
						"@JsonSchema(className='" +
						typeof(MyLocalJsonProvidedMyType).FullName +
						"') @public create json schema MyType(a string, b string, c BaseOne, d BaseTwo[])",
						path);
				}
				else {
					env.CompileDeploy(
						"@Public create avro schema MyType(a string, b string, c BaseOne, d BaseTwo[])",
						path);
				}

				env.CompileDeploy(
					eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedE3)) +
					" @public create schema E3(e long, f BaseOne) copyfrom MyType",
					path);
				env.CompileDeploy("@name('s0') select * from E3", path);
				env.AssertStatement(
					"s0",
					statement => {
						Assert.AreEqual(typeof(string), statement.EventType.GetPropertyType("a"));
						Assert.AreEqual(typeof(string), statement.EventType.GetPropertyType("b"));
						if (eventRepresentationEnum.IsObjectArrayEvent()) {
							Assert.AreEqual(typeof(object[]), statement.EventType.GetPropertyType("c"));
							Assert.AreEqual(typeof(object[][]), statement.EventType.GetPropertyType("d"));
							Assert.AreEqual(typeof(object[]), statement.EventType.GetPropertyType("f"));
						}
						else if (eventRepresentationEnum.IsMapEvent()) {
							Assert.AreEqual(
								typeof(IDictionary<string, object>),
								statement.EventType.GetPropertyType("c"));
							Assert.AreEqual(
								typeof(IDictionary<string, object>[]),
								statement.EventType.GetPropertyType("d"));
							Assert.AreEqual(
								typeof(IDictionary<string, object>),
								statement.EventType.GetPropertyType("f"));
						}
						else if (eventRepresentationEnum.IsAvroEvent()) {
							Assert.AreEqual(typeof(GenericRecord), statement.EventType.GetPropertyType("c"));
							Assert.AreEqual(typeof(ICollection<object>), statement.EventType.GetPropertyType("d"));
							Assert.AreEqual(typeof(GenericRecord), statement.EventType.GetPropertyType("f"));
						}
						else if (eventRepresentationEnum.IsJsonEvent()) {
							Assert.IsTrue(
								TypeHelper.IsSubclassOrImplementsInterface(
									statement.EventType.GetPropertyType("c"),
									typeof(JsonEventObject)));
							Assert.IsTrue(
								TypeHelper.IsSubclassOrImplementsInterface(
									statement.EventType.GetPropertyType("d").GetElementType(),
									typeof(JsonEventObject)));
							Assert.IsTrue(
								TypeHelper.IsSubclassOrImplementsInterface(
									statement.EventType.GetPropertyType("f"),
									typeof(JsonEventObject)));
						}
						else if (eventRepresentationEnum.IsJsonProvidedClassEvent()) {
							Assert.AreEqual(
								typeof(MyLocalJsonProvidedBaseOne),
								statement.EventType.GetPropertyType("c"));
							Assert.AreEqual(
								typeof(MyLocalJsonProvidedBaseTwo[]),
								statement.EventType.GetPropertyType("d"));
							Assert.AreEqual(
								typeof(MyLocalJsonProvidedBaseOne),
								statement.EventType.GetPropertyType("f"));
						}
						else {
							Assert.Fail();
						}

						Assert.AreEqual(typeof(long?), Boxing.GetBoxedType(statement.EventType.GetPropertyType("e")));
					});

				// invalid tests
				var prefix = eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedDummy));
				env.TryInvalidCompile(
					path,
					prefix + " create schema E4(a long) copyFrom MyType",
					"Duplicate column name 'a' [");
				env.TryInvalidCompile(
					path,
					prefix + " create schema E4(c BaseTwo) copyFrom MyType",
					"Duplicate column name 'c' [");
				env.TryInvalidCompile(
					path,
					prefix + " create schema E4(c BaseTwo) copyFrom XYZ",
					"Type by name 'XYZ' could not be located [");
				env.TryInvalidCompile(
					path,
					prefix + " create schema E4 as " + typeof(SupportBean).Name + " copyFrom XYZ",
					"Copy-from types are not allowed with class-provided types [");
				env.TryInvalidCompile(
					path,
					prefix + " create variant schema E4(c BaseTwo) copyFrom XYZ",
					"Copy-from types are not allowed with variant types [");

				// test SODA
				prefix = eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedE2));
				var createEPL = prefix + " create schema EX as () copyFrom BaseOne, BaseTwo";
				env.EplToModelCompileDeploy(createEPL, path).UndeployAll();
			}
		}

		internal class EPLOtherCreateSchemaConfiguredNotRemoved : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{

				var path = new RegressionPath();
				env.CompileDeploy("@name('s1') @public create schema ABCType(col1 int, col2 int)", path);
				var deploymentIdS1 = env.DeploymentId("s1");
				Assert.IsNotNull(env.Runtime.EventTypeService.GetEventType(deploymentIdS1, "ABCType"));
				env.UndeployAll();

				Assert.IsNull(env.Runtime.EventTypeService.GetEventType(deploymentIdS1, "ABCType"));

				AssertTypeExistsPreconfigured(env, "SupportBean");
				AssertTypeExistsPreconfigured(env, "MapTypeEmpty");
				AssertTypeExistsPreconfigured(env, "TestXMLNoSchemaType");
			}

			public ISet<RegressionFlag> Flags()
			{
				return Collections.Set(RegressionFlag.RUNTIMEOPS);
			}
		}

		internal class EPLOtherCreateSchemaInvalid : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
					TryAssertionInvalid(env, rep);
				}

				env.TryInvalidCompile(
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
					? "Nestable type configuration encountered an unexpected property type name 'xxxx' for property 'col1', expected System.Type or System.Collections.Generic.IDictionary or the name of a previously-declared event type ["
					: "Type definition encountered an unexpected property type name 'xxxx' for property 'col1', expected the name of a previously-declared Avro type";
				var prefix = eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedDummy));
				env.TryInvalidCompile(prefix + " create schema MyEventType as (col1 xxxx)", expectedOne);

				env.TryInvalidCompile(
					prefix + " create schema MyEventType as (col1 int, col1 string)",
					"Duplicate column name 'col1' [");

				var path = new RegressionPath();
				env.CompileDeploy(prefix + " @public create schema MyEventType as (col1 string)", path);
				var expectedTwo = "Event type named 'MyEventType' has already been declared";
				env.TryInvalidCompile(path, "create schema MyEventType as (col1 string, col2 string)", expectedTwo);

				env.TryInvalidCompile(
					prefix + " create schema MyEventTypeT1 as () inherit ABC",
					"Expected 'inherits', 'starttimestamp', 'endtimestamp' or 'copyfrom' keyword after create-schema clause but encountered 'inherit' [");

				env.TryInvalidCompile(
					prefix + " create schema MyEventTypeT2 as () inherits ABC",
					"Supertype by name 'ABC' could not be found [");

				env.TryInvalidCompile(
					prefix + " create schema MyEventTypeT3 as () inherits",
					"Incorrect syntax near end-of-input expecting an identifier but found end-of-input at line 1 column ");

				env.UndeployAll();
			}
		}

		internal class EPLOtherCreateSchemaAvroSchemaWAnnotation : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				Schema schema = SchemaBuilder.Union(TypeBuilder.IntType(), TypeBuilder.StringType());
				var epl = "@AvroSchemaField(name='carId',schema='" +
				          schema.ToString() +
				          "') create avro schema MyEvent(carId object)";
				env.CompileDeploy(epl);
				env.UndeployAll();
			}
		}

		internal class EPLOtherCreateSchemaColDefPlain : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
					TryAssertionColDefPlain(env, rep);
				}

				// test property classname, either simple or fully-qualified.
				var path = new RegressionPath();
				env.CompileDeploy(
					"@name('create') create schema MySchema (f1 TimeSpan, f2 System.Drawing.PointF, f3 EventHandler, f4 null)",
					path);

				env.AssertStatement(
					"create",
					statement => {
						var eventType = statement.EventType;
						Assert.AreEqual(typeof(TimeSpan?), eventType.GetPropertyType("f1"));
						Assert.AreEqual(typeof(PointF?), eventType.GetPropertyType("f2"));
						Assert.AreEqual(typeof(EventHandler), eventType.GetPropertyType("f3"));
						Assert.AreEqual(null, eventType.GetPropertyType("f4"));
					});

				env.UndeployAll();
			}
		}

		internal class EPLOtherCreateSchemaModelPONO : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				var schema = "@name('c1') @public @buseventtype create schema SupportBeanOne as " +
				             typeof(SupportBean_ST0).Name +
				             ";\n" +
				             "@name('c2') @public @buseventtype create schema SupportBeanTwo as " +
				             typeof(SupportBean_ST0).Name +
				             ";\n";
				env.CompileDeploy(schema, path);

				env.AssertStatement(
					"c1",
					statement => Assert.AreEqual(typeof(SupportBean_ST0), statement.EventType.UnderlyingType));
				env.AssertStatement(
					"c2",
					statement => Assert.AreEqual(typeof(SupportBean_ST0), statement.EventType.UnderlyingType));

				env.CompileDeploy("@name('s0') select * from SupportBeanOne", path).AddListener("s0");
				env.AssertStatement(
					"s0",
					statement => Assert.AreEqual(typeof(SupportBean_ST0), statement.EventType.UnderlyingType));

				env.CompileDeploy("@name('s1') select * from SupportBeanTwo", path).AddListener("s1");
				env.AssertStatement(
					"s1",
					statement => Assert.AreEqual(typeof(SupportBean_ST0), statement.EventType.UnderlyingType));

				env.SendEventBean(new SupportBean_ST0("E1", 2), "SupportBeanOne");
				env.AssertPropsNew("s0", "id,p00".Split(","), new object[] { "E1", 2 });
				env.AssertListenerNotInvoked("s1");

				env.SendEventBean(new SupportBean_ST0("E2", 3), "SupportBeanTwo");
				env.AssertPropsNew("s1", "id,p00".Split(","), new object[] { "E2", 3 });
				env.AssertListenerNotInvoked("s0");

				// assert type information
				env.AssertStatement(
					"s0",
					statement => Assert.AreEqual(EventTypeTypeClass.STREAM, statement.EventType.Metadata.TypeClass));

				// test keyword
				env.TryInvalidCompile(
					"create schema MySchemaInvalid as com.mycompany.event.ABC",
					"Could not load class by name 'com.mycompany.event.ABC', please check imports");
				env.TryInvalidCompile(
					"create schema MySchemaInvalid as com.mycompany.events.ABC",
					"Could not load class by name 'com.mycompany.events.ABC', please check imports");

				env.UndeployAll();
			}
		}

		internal class EPLOtherCreateSchemaNestableMapArray : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
					TryAssertionNestableMapArray(env, rep);
				}
			}
		}

		internal class EPLOtherCreateSchemaInherit : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				env.CompileDeploy("@Public create schema MyParentType as (col1 int, col2 string)", path);
				env.CompileDeploy(
					"@name('child') @public create schema MyChildTypeOne (col3 int) inherits MyParentType",
					path);

				env.AssertStatement(
					"child",
					statement => {
						var childType = statement.EventType;
						Assert.AreEqual(typeof(int?), childType.GetPropertyType("col1"));
						Assert.AreEqual(typeof(string), childType.GetPropertyType("col2"));
						Assert.AreEqual(typeof(int?), childType.GetPropertyType("col3"));
					});

				env.CompileDeploy("@Public create schema MyChildTypeTwo as (col4 boolean)", path);
				var createText =
					"@name('childchild') @public create schema MyChildChildType as (col5 short, col6 long) inherits MyChildTypeOne, MyChildTypeTwo";
				var model = env.EplToModel(createText);
				Assert.AreEqual(createText, model.ToEPL());
				env.CompileDeploy(model, path);
				env.AssertStatement(
					"childchild",
					statement => {
						var stmtChildChildType = statement.EventType;
						Assert.AreEqual(typeof(bool?), stmtChildChildType.GetPropertyType("col4"));
						Assert.AreEqual(typeof(int?), stmtChildChildType.GetPropertyType("col3"));
						Assert.AreEqual(typeof(short?), stmtChildChildType.GetPropertyType("col5"));
					});

				env.CompileDeploy(
					"@name('cc2') create schema MyChildChildTypeTwo () inherits MyChildTypeOne, MyChildTypeTwo",
					path);
				env.AssertStatement(
					"cc2",
					statement => {
						var eventTypeCC2 = statement.EventType;
						Assert.AreEqual(typeof(bool?), eventTypeCC2.GetPropertyType("col4"));
						Assert.AreEqual(typeof(int?), eventTypeCC2.GetPropertyType("col3"));
					});

				env.UndeployAll();
			}
		}

		internal class EPLOtherCreateSchemaVariantType : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				var epl = "@Public create schema MyTypeZero as (col1 int, col2 string);\n" +
				          "@Public create schema MyTypeOne as (col1 int, col3 string, col4 int);\n" +
				          "@Public create schema MyTypeTwo as (col1 int, col4 boolean, col5 short)";
				env.CompileDeploy(epl, path);

				// try predefined
				env.CompileDeploy(
					"@name('predef') @public create variant schema MyVariantPredef as MyTypeZero, MyTypeOne",
					path);
				env.AssertStatement(
					"predef",
					statement => {
						var variantTypePredef = statement.EventType;
						Assert.AreEqual(typeof(int?), variantTypePredef.GetPropertyType("col1"));
						Assert.AreEqual(1, variantTypePredef.PropertyDescriptors.Count);
					});

				env.CompileDeploy("@Public insert into MyVariantPredef select * from MyTypeZero", path);
				env.CompileDeploy("@Public insert into MyVariantPredef select * from MyTypeOne", path);
				env.TryInvalidCompile(
					path,
					"insert into MyVariantPredef select * from MyTypeTwo",
					"Selected event type is not a valid event type of the variant stream 'MyVariantPredef' [insert into MyVariantPredef select * from MyTypeTwo]");

				// try predefined with any
				var createText =
					"@name('predef_any') @public create variant schema MyVariantAnyModel as MyTypeZero, MyTypeOne, *";
				var model = env.EplToModel(createText);
				Assert.AreEqual(createText, model.ToEPL());
				env.CompileDeploy(model, path);
				env.AssertStatement(
					"predef_any",
					statement => {
						var predefAnyType = statement.EventType;
						Assert.AreEqual(4, predefAnyType.PropertyDescriptors.Count);
						Assert.AreEqual(typeof(object), predefAnyType.GetPropertyType("col1"));
						Assert.AreEqual(typeof(object), predefAnyType.GetPropertyType("col2"));
						Assert.AreEqual(typeof(object), predefAnyType.GetPropertyType("col3"));
						Assert.AreEqual(typeof(object), predefAnyType.GetPropertyType("col4"));
					});

				// try "any"
				env.CompileDeploy("@name('any') @public create variant schema MyVariantAny as *", path);
				env.AssertStatement(
					"any",
					statement => {
						var variantTypeAny = statement.EventType;
						Assert.AreEqual(0, variantTypeAny.PropertyDescriptors.Count);
					});

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
				eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedMyEventTypeCol1To4)) +
				" @public create schema MyEventType as (col1 string, col2 int, col3col4 int)",
				path);
			env.AssertStatement("create", statement => AssertTypeColDef(statement.EventType));
			env.CompileDeploy(
				"@name('select') " +
				eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedMyEventTypeCol1To4)) +
				" select * from MyEventType",
				path);
			env.AssertStatement("select", statement => AssertTypeColDef(statement.EventType));
			env.UndeployAll();

			// destroy and create differently
			env.CompileDeploy(
				"@name('create') " +
				eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedMyEventTypCol34)) +
				" @public create schema MyEventType as (col3 string, col4 int)");
			env.AssertStatement(
				"create",
				statement => {
					Assert.AreEqual(typeof(int?), Boxing.GetBoxedType(statement.EventType.GetPropertyType("col4")));
					Assert.AreEqual(2, statement.EventType.PropertyDescriptors.Count);
				});
			env.UndeployAll();

			// destroy and create differently
			path.Clear();
			var schemaEPL = "@name('create') " +
			                eventRepresentationEnum.GetAnnotationTextWJsonProvided(
				                typeof(MyLocalJsonProvidedMyEventTypCol56)) +
			                " @public @buseventtype create schema MyEventType as (col5 string, col6 int)";
			env.CompileDeploy(schemaEPL, path);

			env.AssertStatement(
				"create",
				statement => {
					Assert.IsTrue(eventRepresentationEnum.MatchesClass(statement.EventType.UnderlyingType));
					Assert.AreEqual(typeof(int?), Boxing.GetBoxedType(statement.EventType.GetPropertyType("col6")));
					Assert.AreEqual(2, statement.EventType.PropertyDescriptors.Count);
				});

			env.CompileDeploy(
					"@name('select') " +
					eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedMyEventTypCol56)) +
					" select * from MyEventType",
					path)
				.AddListener("select");
			env.AssertStatement(
				"select",
				statement => Assert.IsTrue(eventRepresentationEnum.MatchesClass(statement.EventType.UnderlyingType)));

			// send event
			if (eventRepresentationEnum.IsMapEvent()) {
				IDictionary<string, object> data = new LinkedHashMap<string, object>();
				data.Put("col5", "abc");
				data.Put("col6", 1);
				env.SendEventMap(data, "MyEventType");
			}
			else if (eventRepresentationEnum.IsObjectArrayEvent()) {
				env.SendEventObjectArray(new object[] { "abc", 1 }, "MyEventType");
			}
			else if (eventRepresentationEnum.IsAvroEvent()) {
				var schema = env.RuntimeAvroSchemaByDeployment("create", "MyEventType");
				var @event = new GenericRecord(schema.AsRecordSchema());
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

			env.AssertPropsNew("select", "col5,col6".Split(","), new object[] { "abc", 1 });

			// assert type information
			env.AssertStatement(
				"select",
				statement => {
					var type = statement.EventType;
					Assert.AreEqual(EventTypeTypeClass.STREAM, type.Metadata.TypeClass);
					Assert.AreEqual(type.Name, type.Metadata.Name);
				});

			// test non-enum create-schema
			var epl =
				eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedMyEventTypeTwo)) +
				" @name('c2') create schema MyEventTypeTwo as (col1 string, col2 int, col3col4 int)";
			env.CompileDeploy(epl);
			env.AssertStatement(
				"c2",
				statement => {
					AssertTypeColDef(env.Statement("c2").EventType);
					Assert.IsTrue(eventRepresentationEnum.MatchesClass(statement.EventType.UnderlyingType));
				});
			env.UndeployModuleContaining("c2");

			env.EplToModelCompileDeploy(epl);
			env.AssertStatement(
				"c2",
				statement => {
					AssertTypeColDef(statement.EventType);
					Assert.IsTrue(eventRepresentationEnum.MatchesClass(statement.EventType.UnderlyingType));
				});

			env.UndeployAll();
		}

		private static void TryAssertionNestableMapArray(
			RegressionEnvironment env,
			EventRepresentationChoice eventRepresentationEnum)
		{
			var path = new RegressionPath();
			var schema =
				"@name('innerType') " +
				eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedNestableArray)) +
				" @public create schema MyInnerType as (inn1 string[], inn2 int[]);\n" +
				"@name('outerType') " +
				eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedNestableOuter)) +
				" @public @buseventtype create schema MyOuterType as (col1 MyInnerType, col2 MyInnerType[]);\n";
			env.CompileDeploy(schema, path);

			env.AssertStatement(
				"innerType",
				statement => {
					var innerType = statement.EventType;
					Assert.AreEqual(
						eventRepresentationEnum.IsAvroEvent() ? typeof(ICollection<object>) : typeof(string[]),
						innerType.GetPropertyType("inn1"));
					Assert.IsTrue(innerType.GetPropertyDescriptor("inn1").IsIndexed);
					Assert.AreEqual(
						eventRepresentationEnum.IsAvroEvent() ? typeof(ICollection<object>) : typeof(int?[]),
						innerType.GetPropertyType("inn2"));
					Assert.IsTrue(innerType.GetPropertyDescriptor("inn2").IsIndexed);
					Assert.IsTrue(eventRepresentationEnum.MatchesClass(innerType.UnderlyingType));
				});
			env.AssertStatement(
				"outerType",
				statement => {
					var type = statement.EventType.GetFragmentType("col1");
					Assert.IsFalse(type.IsIndexed);
					Assert.AreEqual(false, type.IsNative);
					type = statement.EventType.GetFragmentType("col2");
					Assert.IsTrue(type.IsIndexed);
					Assert.AreEqual(false, type.IsNative);
				});

			env.CompileDeploy("@name('s0') select * from MyOuterType", path).AddListener("s0");
			env.AssertStatement(
				"s0",
				statement => Assert.IsTrue(eventRepresentationEnum.MatchesClass(statement.EventType.UnderlyingType)));

			if (eventRepresentationEnum.IsObjectArrayEvent()) {
				var innerData = new object[] { "abc,def".Split(","), new int[] { 1, 2 } };
				var outerData = new object[] { innerData, new object[] { innerData, innerData } };
				env.SendEventObjectArray(outerData, "MyOuterType");
			}
			else if (eventRepresentationEnum.IsMapEvent()) {
				IDictionary<string, object> innerData = new Dictionary<string, object>();
				innerData.Put("inn1", "abc,def".Split(","));
				innerData.Put("inn2", new int[] { 1, 2 });
				IDictionary<string, object> outerData = new Dictionary<string, object>();
				outerData.Put("col1", innerData);
				outerData.Put("col2", new IDictionary<string, object>[] { innerData, innerData });
				env.SendEventMap(outerData, "MyOuterType");
			}
			else if (eventRepresentationEnum.IsAvroEvent()) {
				var innerSchema = env.RuntimeAvroSchemaByDeployment("innerType", "MyInnerType");
				var outerSchema = env.RuntimeAvroSchemaByDeployment("outerType", "MyOuterType");
				var innerData = new GenericRecord(innerSchema.AsRecordSchema());
				innerData.Put("inn1", Arrays.AsList("abc", "def"));
				innerData.Put("inn2", Arrays.AsList(1, 2));
				var outerData = new GenericRecord(outerSchema.AsRecordSchema());
				outerData.Put("col1", innerData);
				outerData.Put("col2", Arrays.AsList(innerData, innerData));
				env.SendEventAvro(outerData, "MyOuterType");
			}
			else if (eventRepresentationEnum.IsJsonEvent() || eventRepresentationEnum.IsJsonProvidedClassEvent()) {
				var inn1 = new JArray("abc", "def");
				var inn2 = new JArray(1, 2);
				var inn = new JObject("inn1", "inn2");
				var outer = new JObject(new JProperty("col1", inn));
				var col2 = new JArray(inn, inn);
				outer.Add("col2", col2);
				env.SendEventJson(outer.ToString(), "MyOuterType");
			}
			else {
				Assert.Fail();
			}

			env.AssertPropsNew("s0", "col1.inn1[1],col2[1].inn2[1]".Split(","), new object[] { "def", 2 });

			env.UndeployAll();
		}

		private static void CompileDeployWExport(
			string epl,
			bool soda,
			RegressionEnvironment env)
		{
			EPCompiled compiled;
			try {
				if (!soda) {
					compiled = env.Compile(epl);
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
			}
			catch (Exception ex) {
				throw new EPRuntimeException(ex);
			}

			env.Deploy(compiled);
		}

		private static void AssertTypeExistsPreconfigured(
			RegressionEnvironment env,
			string typeName)
		{
			Assert.IsNotNull(env.Runtime.EventTypeService.GetEventTypePreconfigured(typeName));
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
		internal class MyLocalJsonProvidedMyEventTypeCol1To4
		{
			public string col1;
			public int col2;
			public int col3col4;
		}

		[Serializable]
		internal class MyLocalJsonProvidedMyEventTypCol34
		{
			public string col3;
			public int col4;
		}

		[Serializable]
		internal class MyLocalJsonProvidedMyEventTypCol56
		{
			public string col5;
			public int col6;
		}

		[Serializable]
		internal class MyLocalJsonProvidedNestableArray
		{
			public string[] inn1;
			public int?[] inn2;
		}

		[Serializable]
		internal class MyLocalJsonProvidedNestableOuter
		{
			public MyLocalJsonProvidedNestableArray col1;
			public MyLocalJsonProvidedNestableArray[] col2;
		}

		[Serializable]
		internal class MyLocalJsonProvidedBaseOne
		{
			public string prop1;
			public int prop2;
		}

		[Serializable]
		internal class MyLocalJsonProvidedBaseTwo
		{
			public long prop3;
		}

		[Serializable]
		internal class MyLocalJsonProvidedE1
		{
			public string prop1;
			public int prop2;
		}

		[Serializable]
		internal class MyLocalJsonProvidedE2
		{
			public string prop1;
			public int prop2;
			public long prop3;
		}

		[Serializable]
		internal class MyLocalJsonProvidedE3
		{
			public string a;
			public string b;
			public MyLocalJsonProvidedBaseOne c;
			public MyLocalJsonProvidedBaseTwo[] d;
			public MyLocalJsonProvidedBaseOne f;
			public long e;
		}

		[Serializable]
		internal class MyLocalJsonProvidedDummy
		{
			public string col1;
		}

		[Serializable]
		internal class MyLocalJsonProvidedMyType
		{
			public string a;
			public string b;
			public MyLocalJsonProvidedBaseOne c;
			public MyLocalJsonProvidedBaseTwo[] d;
		}

		[Serializable]
		internal class MyLocalJsonProvidedMyEventTypeTwo
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

		internal class MyLocalSchemaTypeParamEvent<T>
		{
			private System.Collections.Generic.IList<string> listOfString;
			private System.Collections.Generic.IList<Optional<int?>> listOfOptionalInteger;
			private IDictionary<string, int?> mapOfStringAndInteger;
			private IList<string>[] listArrayOfString;
			IList<string[]> listOfStringArray;
			IList<string>[][] listArray2DimOfString;
			IList<string[][]> listOfStringArray2Dim;
			IList<T> listOfT;

			public IList<string> ListOfString => listOfString;

			public IList<Optional<int?>> ListOfOptionalInteger => listOfOptionalInteger;

			public IDictionary<string, int?> MapOfStringAndInteger => mapOfStringAndInteger;

			public IList<string>[] ListArrayOfString => listArrayOfString;

			public IList<string[]> ListOfStringArray => listOfStringArray;

			public IList<string>[][] ListArray2DimOfString => listArray2DimOfString;

			public IList<string[][]> ListOfStringArray2Dim => listOfStringArray2Dim;

			public IList<T> ListOfT => listOfT;
		}
		// ReSharper restore InconsistentNaming
		// ReSharper restore UnusedMember.Global
	}
} // end of namespace