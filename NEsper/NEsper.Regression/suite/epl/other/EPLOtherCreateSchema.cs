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

using Avro;
using Avro.Generic;

using com.espertech.esper.common.client;
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

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

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
            execs.Add(new EPLOtherCreateSchemaModelPOJO());
            execs.Add(new EPLOtherCreateSchemaNestableMapArray());
            execs.Add(new EPLOtherCreateSchemaInherit());
            execs.Add(new EPLOtherCreateSchemaCopyFromOrderObjectArray());
            execs.Add(new EPLOtherCreateSchemaInvalid());
            execs.Add(new EPLOtherCreateSchemaWithEventType());
            execs.Add(new EPLOtherCreateSchemaVariantType());
            execs.Add(new EPLOtherCreateSchemaSameCRC());
            return execs;
        }

        private static void TryAssertionColDefPlain(
            RegressionEnvironment env,
            EventRepresentationChoice eventRepresentationEnum)
        {
            var path = new RegressionPath();
            env.CompileDeploy(
                "@Name('create') " +
                eventRepresentationEnum.GetAnnotationText() +
                " create schema MyEventType as (col1 string, col2 int, col3_col4 int)",
                path);
            AssertTypeColDef(env.Statement("create").EventType);
            env.CompileDeploy(
                "@Name('select') " + eventRepresentationEnum.GetAnnotationText() + " select * from MyEventType",
                path);
            AssertTypeColDef(env.Statement("select").EventType);
            env.UndeployAll();

            // destroy and create differently
            env.CompileDeploy(
                "@Name('create') " +
                eventRepresentationEnum.GetAnnotationText() +
                " create schema MyEventType as (col3 string, col4 int)");
            Assert.AreEqual(typeof(int?), env.Statement("create").EventType.GetPropertyType("col4").GetBoxedType());
            Assert.AreEqual(2, env.Statement("create").EventType.PropertyDescriptors.Length);
            env.UndeployAll();

            // destroy and create differently
            path.Clear();
            var schemaEPL = "@Name('create') " +
                            eventRepresentationEnum.GetAnnotationText() +
                            " create schema MyEventType as (col5 string, col6 int)";
            env.CompileDeployWBusPublicType(schemaEPL, path);

            Assert.IsTrue(eventRepresentationEnum.MatchesClass(env.Statement("create").EventType.UnderlyingType));
            Assert.AreEqual(typeof(int?), env.Statement("create").EventType.GetPropertyType("col6").GetBoxedType());
            Assert.AreEqual(2, env.Statement("create").EventType.PropertyDescriptors.Length);
            env.CompileDeploy(
                    "@Name('select') " + eventRepresentationEnum.GetAnnotationText() + " select * from MyEventType",
                    path)
                .AddListener("select");
            Assert.IsTrue(eventRepresentationEnum.MatchesClass(env.Statement("select").EventType.UnderlyingType));

            // send event
            if (eventRepresentationEnum.IsMapEvent()) {
                IDictionary<string, object> data = new Dictionary<string, object>();
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

            EPAssertionUtil.AssertProps(
                env.Listener("select").AssertOneGetNewAndReset(),
                "col5,col6".SplitCsv(),
                new object[] {"abc", 1});

            // assert type information
            var type = env.Statement("select").EventType;
            Assert.AreEqual(EventTypeTypeClass.STREAM, type.Metadata.TypeClass);
            Assert.AreEqual(type.Name, type.Metadata.Name);

            // test non-enum create-schema
            var epl = "@Name('c2') create" +
                      eventRepresentationEnum.GetOutputTypeCreateSchemaName() +
                      " schema MyEventTypeTwo as (col1 string, col2 int, col3_col4 int)";
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
                "@Name('innerType') " +
                eventRepresentationEnum.GetAnnotationText() +
                " create schema MyInnerType as (inn1 string[], inn2 int[]);\n" +
                "@Name('outerType') " +
                eventRepresentationEnum.GetAnnotationText() +
                " create schema MyOuterType as (col1 MyInnerType, col2 MyInnerType[]);\n";
            env.CompileDeployWBusPublicType(schema, path);

            var innerType = env.Statement("innerType").EventType;
            Assert.AreEqual(
                eventRepresentationEnum.IsAvroEvent() ? typeof(ICollection<object>) : typeof(string[]),
                innerType.GetPropertyType("inn1"));
            Assert.IsTrue(innerType.GetPropertyDescriptor("inn1").IsIndexed);
            Assert.AreEqual(
                eventRepresentationEnum.IsAvroEvent() ? typeof(ICollection<object>) : typeof(int?[]),
                innerType.GetPropertyType("inn2"));
            Assert.IsTrue(innerType.GetPropertyDescriptor("inn2").IsIndexed);
            Assert.IsTrue(eventRepresentationEnum.MatchesClass(innerType.UnderlyingType));

            var outerType = env.Statement("outerType").EventType;
            var type = outerType.GetFragmentType("col1");
            Assert.AreEqual("MyInnerType", type.FragmentType.Name);
            Assert.IsFalse(type.IsIndexed);
            Assert.IsFalse(type.IsNative);
            type = outerType.GetFragmentType("col2");
            Assert.AreEqual("MyInnerType", type.FragmentType.Name);
            Assert.IsTrue(type.IsIndexed);
            Assert.IsFalse(type.IsNative);

            env.CompileDeploy("@Name('s0') select * from MyOuterType", path).AddListener("s0");
            Assert.IsTrue(eventRepresentationEnum.MatchesClass(env.Statement("s0").EventType.UnderlyingType));

            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                object[] innerData = {"abc,def".SplitCsv(), new[] {1, 2}};
                object[] outerData = {
                    innerData,
                    new object[] {innerData, innerData}
                };
                env.SendEventObjectArray(outerData, "MyOuterType");
            }
            else if (eventRepresentationEnum.IsMapEvent()) {
                IDictionary<string, object> innerData = new Dictionary<string, object>();
                innerData.Put("inn1", "abc,def".SplitCsv());
                innerData.Put("inn2", new[] {1, 2});
                IDictionary<string, object> outerData = new Dictionary<string, object>();
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
            else {
                Assert.Fail();
            }

            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                "col1.inn1[1],col2[1].inn2[1]".SplitCsv(),
                new object[] {"def", 2});

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
                    compiled = EPCompilerProvider.Compiler.Compile(module, args);
                }
            }
            catch (Exception t) {
                throw new EPException(t);
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
            Assert.AreEqual(typeof(int?), eventType.GetPropertyType("col2").GetBoxedType());
            Assert.AreEqual(typeof(int?), eventType.GetPropertyType("col3_col4").GetBoxedType());
            Assert.AreEqual(3, eventType.PropertyDescriptors.Length);
        }

        internal class EPLOtherCreateSchemaSameCRC : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                try {
                    env.CompileDeployWBusPublicType(
                        "create schema b5a7b602ab754d7ab30fb42c4fb28d82();\n" +
                        "create schema d19f2e9e82d14b96be4fa12b8a27ee9f();",
                        new RegressionPath());
                }
                catch (Exception t) {
                    Assert.AreEqual(
                        "Test failed due to exception: Event type by name 'd19f2e9e82d14b96be4fa12b8a27ee9f' has a public crc32 id overlap with event type by name 'b5a7b602ab754d7ab30fb42c4fb28d82', please consider renaming either of these types",
                        t.Message);
                }
            }
        }

        internal class EPLOtherCreateSchemaPathSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('schema') create schema SimpleSchema(p0 string, p1 int);" +
                          "@Name('s0') select * from SimpleSchema;\n" +
                          "insert into SimpleSchema select TheString as p0, IntPrimitive as p1 from SupportBean;\n";
                env.CompileDeploy(epl).AddListener("s0");
                Assert.AreEqual(
                    StatementType.CREATE_SCHEMA,
                    env.Statement("schema").GetProperty(StatementProperty.STATEMENTTYPE));

                env.SendEventBean(new SupportBean("a", 20));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "p0,p1".SplitCsv(),
                    new object[] {"a", 20});

                env.UndeployAll();
            }
        }

        internal class EPLOtherCreateSchemaPublicSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create schema MySchema as (p0 string, p1 int);\n" +
                          "@Name('s0') select p0, p1 from MySchema;\n";
                env.CompileDeployWBusPublicType(epl, new RegressionPath()).AddListener("s0");

                env.SendEventMap(CollectionUtil.BuildMap("p0", "a", "p1", 20), "MySchema");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "p0,p1".SplitCsv(),
                    new object[] {"a", 20});

                env.UndeployAll();
            }
        }

        internal class EPLOtherCreateSchemaCopyFromOrderObjectArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "@Name('s1') create objectarray schema MyEventOne(p0 string, p1 double);\n " +
                          "create objectarray schema MyEventTwo(p2 string) copyfrom MyEventOne;\n";
                env.CompileDeployWBusPublicType(epl, path);

                var type = env.Runtime.EventTypeService.GetEventType(env.DeploymentId("s1"), "MyEventTwo");
                EPAssertionUtil.AssertEqualsExactOrder("p0,p1,p2".SplitCsv(), type.PropertyNames);

                epl = "insert into MyEventTwo select 'abc' as p2, s.* from MyEventOne as s;\n" +
                      "@Name('s0') select p0, p1, p2 from MyEventTwo;\n";
                env.CompileDeploy(epl, path).AddListener("s0");

                env.SendEventObjectArray(new object[] {"E1", 10d}, "MyEventOne");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "p0,p1,p2".SplitCsv(),
                    new object[] {"E1", 10d, "abc"});

                env.UndeployAll();
            }
        }

        internal class EPLOtherCreateSchemaArrayPrimitiveType : RegressionExecution
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
                CompileDeployWExport(
                    "@Name('schema') create schema MySchema as (c0 int[primitive], c1 int[])",
                    soda,
                    env);
                object[][] expectedType = {new object[] {"c0", typeof(int[])}, new object[] {"c1", typeof(int?[])}};
                SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                    expectedType,
                    GetTypeStmt(env, "schema"),
                    SupportEventTypeAssertionEnum.NAME,
                    SupportEventTypeAssertionEnum.TYPE);
                env.UndeployAll();
            }
        }

        internal class EPLOtherCreateSchemaWithEventType : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var theEvent = new SupportBeanSourceEvent(new SupportBean("E1", 1), new[] {new SupportBean_S0(2)});

                // test schema
                env.CompileDeploy(
                    "@Name('schema') create schema MySchema (bean SupportBean, beanarray SupportBean_S0[])");
                var stmtSchemaType = env.Statement("schema").EventType;
                Assert.AreEqual(
                    new EventPropertyDescriptor("bean", typeof(SupportBean), null, false, false, false, false, true),
                    stmtSchemaType.GetPropertyDescriptor("bean"));
                Assert.AreEqual(
                    new EventPropertyDescriptor(
                        "beanarray",
                        typeof(SupportBean_S0[]),
                        typeof(SupportBean_S0),
                        false,
                        false,
                        true,
                        false,
                        true),
                    stmtSchemaType.GetPropertyDescriptor("beanarray"));

                env.CompileDeploy(
                        "@Name('s0') insert into MySchema select sb as bean, s0Arr as beanarray from SupportBeanSourceEvent")
                    .AddListener("s0");
                env.SendEventBean(theEvent);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "bean.TheString,beanarray[0].id".SplitCsv(),
                    new object[] {"E1", 2});
                env.UndeployModuleContaining("s0");

                // test named window
                var path = new RegressionPath();
                env.CompileDeploy(
                        "@Name('window') create window MyWindow#keepall as (bean SupportBean, beanarray SupportBean_S0[])",
                        path)
                    .AddListener("window");
                var stmtWindowType = env.Statement("window").EventType;
                Assert.AreEqual(
                    new EventPropertyDescriptor("bean", typeof(SupportBean), null, false, false, false, false, true),
                    stmtWindowType.GetPropertyDescriptor("bean"));
                Assert.AreEqual(
                    new EventPropertyDescriptor(
                        "beanarray",
                        typeof(SupportBean_S0[]),
                        typeof(SupportBean_S0),
                        false,
                        false,
                        true,
                        false,
                        true),
                    stmtWindowType.GetPropertyDescriptor("beanarray"));

                env.CompileDeploy(
                    "@Name('windowInsertOne') insert into MyWindow select sb as bean, s0Arr as beanarray from SupportBeanSourceEvent",
                    path);
                env.SendEventBean(theEvent);
                EPAssertionUtil.AssertProps(
                    env.Listener("window").AssertOneGetNewAndReset(),
                    "bean.TheString,beanarray[0].id".SplitCsv(),
                    new object[] {"E1", 2});
                env.UndeployModuleContaining("windowInsertOne");

                // insert pattern to named window
                env.CompileDeploy(
                    "@Name('windowInsertOne') insert into MyWindow select sb as bean, s0Arr as beanarray from pattern [sb=SupportBean => s0Arr=SupportBean_S0 until SupportBean_S0(id=0)]",
                    path);
                env.SendEventBean(new SupportBean("E2", 2));
                env.SendEventBean(new SupportBean_S0(10, "S0_1"));
                env.SendEventBean(new SupportBean_S0(20, "S0_2"));
                env.SendEventBean(new SupportBean_S0(0, "S0_3"));
                EPAssertionUtil.AssertProps(
                    env.Listener("window").AssertOneGetNewAndReset(),
                    "bean.TheString,beanarray[0].id,beanarray[1].id".SplitCsv(),
                    new object[] {"E2", 10, 20});
                env.UndeployModuleContaining("windowInsertOne");

                // test configured Map type
                env.CompileDeploy(
                        "@Name('s0') insert into MyConfiguredMap select sb as bean, s0Arr as beanarray from SupportBeanSourceEvent")
                    .AddListener("s0");
                env.SendEventBean(theEvent);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "bean.TheString,beanarray[0].id".SplitCsv(),
                    new object[] {"E1", 2});

                env.UndeployAll();
            }
        }

        internal class EPLOtherCreateSchemaCopyProperties : RegressionExecution
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
                    eventRepresentationEnum.GetAnnotationText() +
                    " create schema BaseOne (prop1 String, prop2 int);\n" +
                    eventRepresentationEnum.GetAnnotationText() +
                    " create schema BaseTwo (prop3 long);\n" +
                    eventRepresentationEnum.GetAnnotationText() +
                    " create schema E1 () copyfrom BaseOne;\n";
                env.CompileDeployWBusPublicType(epl, path);

                env.CompileDeploy("@Name('s0') select * from E1", path).AddListener("s0");
                Assert.IsTrue(eventRepresentationEnum.MatchesClass(env.Statement("s0").EventType.UnderlyingType));
                Assert.AreEqual(typeof(string), env.Statement("s0").EventType.GetPropertyType("prop1"));
                Assert.AreEqual(typeof(int?), env.Statement("s0").EventType.GetPropertyType("prop2").GetBoxedType());

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
                    var @event = new GenericRecord(
                        SchemaBuilder.Record(
                            "name",
                            TypeBuilder.RequiredString("prop1"),
                            TypeBuilder.RequiredInt("prop2")));
                    @event.Put("prop1", "v1");
                    @event.Put("prop2", 2);
                    env.SendEventAvro(@event, "E1");
                }
                else {
                    Assert.Fail();
                }

                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "prop1,prop2".SplitCsv(),
                    new object[] {"v1", 2});
                env.UndeployModuleContaining("s0");

                // test two copy-from types
                env.CompileDeploy(
                    eventRepresentationEnum.GetAnnotationText() + " create schema E2 () copyfrom BaseOne, BaseTwo",
                    path);
                env.CompileDeploy("@Name('s0') select * from E2", path);
                var stmtEventType = env.Statement("s0").EventType;
                Assert.AreEqual(typeof(string), stmtEventType.GetPropertyType("prop1"));
                Assert.AreEqual(typeof(int?), stmtEventType.GetPropertyType("prop2").GetBoxedType());
                Assert.AreEqual(typeof(long?), stmtEventType.GetPropertyType("prop3").GetBoxedType());
                env.UndeployModuleContaining("s0");

                // test API-defined type
                if (eventRepresentationEnum.IsMapEvent() || eventRepresentationEnum.IsObjectArrayEvent()) {
                    env.CompileDeploy(
                        "create " +
                        eventRepresentationEnum.GetOutputTypeCreateSchemaName() +
                        " schema MyType(a string, b string, c BaseOne, d BaseTwo[])",
                        path);
                }
                else {
                    env.CompileDeploy("create avro schema MyType(a string, b string, c BaseOne, d BaseTwo[])", path);
                }

                env.CompileDeploy(
                    eventRepresentationEnum.GetAnnotationText() +
                    " create schema E3(e long, f BaseOne) copyfrom MyType",
                    path);
                env.CompileDeploy("@Name('s0') select * from E3", path);
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
                    Assert.AreEqual(typeof(ICollection<object>), stmtThree.EventType.GetPropertyType("d"));
                    Assert.AreEqual(typeof(GenericRecord), stmtThree.EventType.GetPropertyType("f"));
                }
                else {
                    Assert.Fail();
                }

                Assert.AreEqual(typeof(long?), stmtThree.EventType.GetPropertyType("e").GetBoxedType());

                // invalid tests
                TryInvalidCompile(
                    env,
                    path,
                    eventRepresentationEnum.GetAnnotationText() + " create schema E4(a long) copyFrom MyType",
                    "Duplicate column name 'a' [");
                TryInvalidCompile(
                    env,
                    path,
                    eventRepresentationEnum.GetAnnotationText() + " create schema E4(c BaseTwo) copyFrom MyType",
                    "Duplicate column name 'c' [");
                TryInvalidCompile(
                    env,
                    path,
                    eventRepresentationEnum.GetAnnotationText() + " create schema E4(c BaseTwo) copyFrom XYZ",
                    "Type by name 'XYZ' could not be located [");
                TryInvalidCompile(
                    env,
                    path,
                    eventRepresentationEnum.GetAnnotationText() +
                    " create schema E4 as " +
                    typeof(SupportBean).Name +
                    " copyFrom XYZ",
                    "Copy-from types are not allowed with class-provided types [");
                TryInvalidCompile(
                    env,
                    path,
                    eventRepresentationEnum.GetAnnotationText() + " create variant schema E4(c BaseTwo) copyFrom XYZ",
                    "Copy-from types are not allowed with variant types [");

                // test SODA
                var createEPL = eventRepresentationEnum.GetAnnotationText() +
                                " create schema EX as () copyFrom BaseOne, BaseTwo";
                env.EplToModelCompileDeploy(createEPL, path).UndeployAll();
            }
        }

        internal class EPLOtherCreateSchemaConfiguredNotRemoved : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@Name('s1') create schema ABCType(col1 int, col2 int)", path);
                var deploymentIdS1 = env.DeploymentId("s1");
                Assert.IsNotNull(env.Runtime.EventTypeService.GetEventType(deploymentIdS1, "ABCType"));
                env.UndeployAll();
                Assert.IsNull(env.Runtime.EventTypeService.GetEventType(deploymentIdS1, "ABCType"));

                AssertTypeExistsPreconfigured(env, "SupportBean");
                AssertTypeExistsPreconfigured(env, "MapTypeEmpty");
                AssertTypeExistsPreconfigured(env, "TestXMLNoSchemaType");
            }
        }

        internal class EPLOtherCreateSchemaInvalid : RegressionExecution
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
                    ? "Nestable type configuration encountered an unexpected property type name 'xxxx' for property 'col1', expected System.Class or java.util.Map or the name of a previously-declared Map or ObjectArray type ["
                    : "Type definition encountered an unexpected property type name 'xxxx' for property 'col1', expected the name of a previously-declared Avro type";
                TryInvalidCompile(
                    env,
                    eventRepresentationEnum.GetAnnotationText() + " create schema MyEventType as (col1 xxxx)",
                    expectedOne);

                TryInvalidCompile(
                    env,
                    eventRepresentationEnum.GetAnnotationText() +
                    " create schema MyEventType as (col1 int, col1 string)",
                    "Duplicate column name 'col1' [");

                var path = new RegressionPath();
                env.CompileDeploy(
                    eventRepresentationEnum.GetAnnotationText() + " create schema MyEventType as (col1 string)",
                    path);
                var expectedTwo = "Event type named 'MyEventType' has already been declared";
                TryInvalidCompile(env, path, "create schema MyEventType as (col1 string, col2 string)", expectedTwo);

                TryInvalidCompile(
                    env,
                    eventRepresentationEnum.GetAnnotationText() + " create schema MyEventTypeT1 as () inherit ABC",
                    "Expected 'inherits', 'starttimestamp', 'endtimestamp' or 'copyfrom' keyword after create-schema clause but encountered 'inherit' [");

                TryInvalidCompile(
                    env,
                    eventRepresentationEnum.GetAnnotationText() + " create schema MyEventTypeT2 as () inherits ABC",
                    "Supertype by name 'ABC' could not be found [");

                TryInvalidCompile(
                    env,
                    eventRepresentationEnum.GetAnnotationText() + " create schema MyEventTypeT3 as () inherits",
                    "Incorrect syntax near end-of-input expecting an identifier but found end-of-input at line 1 column ");

                env.UndeployAll();
            }
        }

        internal class EPLOtherCreateSchemaAvroSchemaWAnnotation : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                Schema schema = SchemaBuilder.Union(
                    TypeBuilder.IntType(),
                    TypeBuilder.StringType());
                var epl = $"@AvroSchemaField(name='carId',schema='{schema}') create avro schema MyEvent(carId object)";
                env.CompileDeploy(epl);
                env.UndeployAll();
            }
        }

        internal class EPLOtherCreateSchemaColDefPlain : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                    TryAssertionColDefPlain(env, rep);
                }

                // test property classname, either simple or fully-qualified.
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@Name('create') create schema MySchema (f1 TimeSpan, f2 System.Drawing.PointF, f3 EventHandler, f4 null)",
                    path);

                var eventType = env.Runtime.EventTypeService.GetEventType(env.DeploymentId("create"), "MySchema");
                Assert.AreEqual(typeof(TimeSpan), eventType.GetPropertyType("f1"));
                Assert.AreEqual(typeof(PointF), eventType.GetPropertyType("f2"));
                Assert.AreEqual(typeof(EventHandler), eventType.GetPropertyType("f3"));
                Assert.AreEqual(null, eventType.GetPropertyType("f4"));

                env.UndeployAll();
            }
        }

        internal class EPLOtherCreateSchemaModelPOJO : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var schema = "@Name('c1') create schema SupportBeanOne as " +
                             typeof(SupportBean_ST0).Name +
                             ";\n" +
                             "@Name('c2') create schema SupportBeanTwo as " +
                             typeof(SupportBean_ST0).Name +
                             ";\n";
                env.CompileDeployWBusPublicType(schema, path);

                Assert.AreEqual(typeof(SupportBean_ST0), env.Statement("c1").EventType.UnderlyingType);
                Assert.AreEqual(typeof(SupportBean_ST0), env.Statement("c2").EventType.UnderlyingType);

                env.CompileDeploy("@Name('s0') select * from SupportBeanOne", path).AddListener("s0");
                Assert.AreEqual(typeof(SupportBean_ST0), env.Statement("s0").EventType.UnderlyingType);

                env.CompileDeploy("@Name('s1') select * from SupportBeanTwo", path).AddListener("s1");
                Assert.AreEqual(typeof(SupportBean_ST0), env.Statement("s1").EventType.UnderlyingType);

                env.SendEventBean(new SupportBean_ST0("E1", 2), "SupportBeanOne");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "id,p00".SplitCsv(),
                    new object[] {"E1", 2});
                Assert.IsFalse(env.Listener("s1").IsInvoked);

                env.SendEventBean(new SupportBean_ST0("E2", 3), "SupportBeanTwo");
                EPAssertionUtil.AssertProps(
                    env.Listener("s1").AssertOneGetNewAndReset(),
                    "id,p00".SplitCsv(),
                    new object[] {"E2", 3});
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

        internal class EPLOtherCreateSchemaNestableMapArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                    TryAssertionNestableMapArray(env, rep);
                }
            }
        }

        internal class EPLOtherCreateSchemaInherit : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create schema MyParentType as (col1 int, col2 string)", path);
                env.CompileDeploy("@Name('child') create schema MyChildTypeOne (col3 int) inherits MyParentType", path);

                var childType = env.Statement("child").EventType;
                Assert.AreEqual(typeof(int?), childType.GetPropertyType("col1"));
                Assert.AreEqual(typeof(string), childType.GetPropertyType("col2"));
                Assert.AreEqual(typeof(int?), childType.GetPropertyType("col3"));

                env.CompileDeploy("create schema MyChildTypeTwo as (col4 boolean)", path);
                var createText =
                    "@Name('childchild') create schema MyChildChildType as (col5 short, col6 long) inherits MyChildTypeOne, MyChildTypeTwo";
                var model = env.EplToModel(createText);
                Assert.AreEqual(createText, model.ToEPL());
                env.CompileDeploy(model, path);
                var stmtChildChildType = env.Statement("childchild").EventType;
                Assert.AreEqual(typeof(bool?), stmtChildChildType.GetPropertyType("col4"));
                Assert.AreEqual(typeof(int?), stmtChildChildType.GetPropertyType("col3"));
                Assert.AreEqual(typeof(short?), stmtChildChildType.GetPropertyType("col5"));

                env.CompileDeploy(
                    "@Name('cc2') create schema MyChildChildTypeTwo () inherits MyChildTypeOne, MyChildTypeTwo",
                    path);
                var eventTypeCC2 = env.Statement("cc2").EventType;
                Assert.AreEqual(typeof(bool?), eventTypeCC2.GetPropertyType("col4"));
                Assert.AreEqual(typeof(int?), eventTypeCC2.GetPropertyType("col3"));

                env.UndeployAll();
            }
        }

        internal class EPLOtherCreateSchemaVariantType : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "create schema MyTypeZero as (col1 int, col2 string);\n" +
                          "create schema MyTypeOne as (col1 int, col3 string, col4 int);\n" +
                          "create schema MyTypeTwo as (col1 int, col4 boolean, col5 short)";
                env.CompileDeploy(epl, path);

                // try predefined
                env.CompileDeploy(
                    "@Name('predef') create variant schema MyVariantPredef as MyTypeZero, MyTypeOne",
                    path);
                var variantTypePredef = env.Statement("predef").EventType;
                Assert.AreEqual(typeof(int?), variantTypePredef.GetPropertyType("col1"));
                Assert.AreEqual(1, variantTypePredef.PropertyDescriptors.Length);

                env.CompileDeploy("insert into MyVariantPredef select * from MyTypeZero", path);
                env.CompileDeploy("insert into MyVariantPredef select * from MyTypeOne", path);
                TryInvalidCompile(
                    env,
                    path,
                    "insert into MyVariantPredef select * from MyTypeTwo",
                    "Selected event type is not a valid event type of the variant stream 'MyVariantPredef' [insert into MyVariantPredef select * from MyTypeTwo]");

                // try predefined with any
                var createText =
                    "@Name('predef_any') create variant schema MyVariantAnyModel as MyTypeZero, MyTypeOne, *";
                var model = env.EplToModel(createText);
                Assert.AreEqual(createText, model.ToEPL());
                env.CompileDeploy(model, path);
                var predefAnyType = env.Statement("predef_any").EventType;
                Assert.AreEqual(4, predefAnyType.PropertyDescriptors.Length);
                Assert.AreEqual(typeof(object), predefAnyType.GetPropertyType("col1"));
                Assert.AreEqual(typeof(object), predefAnyType.GetPropertyType("col2"));
                Assert.AreEqual(typeof(object), predefAnyType.GetPropertyType("col3"));
                Assert.AreEqual(typeof(object), predefAnyType.GetPropertyType("col4"));

                // try "any"
                env.CompileDeploy("@Name('any') create variant schema MyVariantAny as *", path);
                var variantTypeAny = env.Statement("any").EventType;
                Assert.AreEqual(0, variantTypeAny.PropertyDescriptors.Length);

                env.CompileDeploy("insert into MyVariantAny select * from MyTypeZero", path);
                env.CompileDeploy("insert into MyVariantAny select * from MyTypeOne", path);
                env.CompileDeploy("insert into MyVariantAny select * from MyTypeTwo", path);

                env.UndeployAll();
            }
        }
    }
} // end of namespace