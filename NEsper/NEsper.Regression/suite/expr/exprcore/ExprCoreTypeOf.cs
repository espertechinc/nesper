///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using Avro.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
    public class ExprCoreTypeOf
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ExprCoreTypeOfFragment());
            execs.Add(new ExprCoreTypeOfNamedUnnamedPONO());
            execs.Add(new ExprCoreTypeOfInvalid());
            execs.Add(new ExprCoreTypeOfDynamicProps());
            execs.Add(new ExprCoreTypeOfVariantStream());
            return execs;
        }

        private static void TryAssertionVariantStream(
            RegressionEnvironment env,
            EventRepresentationChoice eventRepresentationEnum)
        {
            var eplSchemas =
                eventRepresentationEnum.GetAnnotationText() +
                " create schema EventOne as (key string);\n" +
                eventRepresentationEnum.GetAnnotationText() +
                " create schema EventTwo as (key string);\n" +
                eventRepresentationEnum.GetAnnotationText() +
                " create schema S0 as " +
                typeof(SupportBean_S0).Name +
                ";\n" +
                eventRepresentationEnum.GetAnnotationText() +
                " create variant schema VarSchema as *;\n";
            var path = new RegressionPath();
            env.CompileDeployWBusPublicType(eplSchemas, path);

            env.CompileDeploy("insert into VarSchema select * from EventOne", path);
            env.CompileDeploy("insert into VarSchema select * from EventTwo", path);
            env.CompileDeploy("insert into VarSchema select * from S0", path);
            env.CompileDeploy("insert into VarSchema select * from SupportBean", path);

            var stmtText = "@Name('s0') select typeof(A) as t0 from VarSchema as A";
            env.CompileDeploy(stmtText, path).AddListener("s0");

            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                env.SendEventObjectArray(new object[] {"value"}, "EventOne");
            }
            else if (eventRepresentationEnum.IsMapEvent()) {
                env.SendEventMap(Collections.SingletonDataMap("key", "value"), "EventOne");
            }
            else if (eventRepresentationEnum.IsAvroEvent()) {
                var record = new GenericRecord(
                    SchemaBuilder.Record("EventOne", TypeBuilder.RequiredString("key")));
                record.Put("key", "value");
                env.SendEventAvro(record, "EventOne");
            }
            else {
                Assert.Fail();
            }

            Assert.AreEqual("EventOne", env.Listener("s0").AssertOneGetNewAndReset().Get("t0"));

            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                env.SendEventObjectArray(new object[] {"value"}, "EventTwo");
            }
            else if (eventRepresentationEnum.IsMapEvent()) {
                env.SendEventMap(Collections.SingletonDataMap("key", "value"), "EventTwo");
            }
            else if (eventRepresentationEnum.IsAvroEvent()) {
                var record = new GenericRecord(
                    SchemaBuilder.Record("EventTwo", TypeBuilder.RequiredString("key")));
                record.Put("key", "value");
                env.SendEventAvro(record, "EventTwo");
            }
            else {
                Assert.Fail();
            }

            Assert.AreEqual("EventTwo", env.Listener("s0").AssertOneGetNewAndReset().Get("t0"));

            env.SendEventBean(new SupportBean_S0(1), "S0");
            Assert.AreEqual("S0", env.Listener("s0").AssertOneGetNewAndReset().Get("t0"));

            env.SendEventBean(new SupportBean());
            Assert.AreEqual("SupportBean", env.Listener("s0").AssertOneGetNewAndReset().Get("t0"));

            env.UndeployModuleContaining("s0");
            env.CompileDeploy(
                    "@Name('s0') select * from VarSchema match_recognize(\n" +
                    "  measures A as a, B as b\n" +
                    "  pattern (A B)\n" +
                    "  define A as typeof(A) = \"EventOne\",\n" +
                    "         B as typeof(B) = \"EventTwo\"\n" +
                    "  )",
                    path)
                .AddListener("s0");

            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                env.SendEventObjectArray(new object[] {"value"}, "EventOne");
                env.SendEventObjectArray(new object[] {"value"}, "EventTwo");
            }
            else if (eventRepresentationEnum.IsMapEvent()) {
                env.SendEventMap(Collections.SingletonDataMap("key", "value"), "EventOne");
                env.SendEventMap(Collections.SingletonDataMap("key", "value"), "EventTwo");
            }
            else if (eventRepresentationEnum.IsAvroEvent()) {
                var schema = SchemaBuilder.Record("EventTwo", TypeBuilder.RequiredString("key"));
                var eventOne = new GenericRecord(schema);
                eventOne.Put("key", "value");
                var eventTwo = new GenericRecord(schema);
                eventTwo.Put("key", "value");
                env.SendEventAvro(eventOne, "EventOne");
                env.SendEventAvro(eventTwo, "EventTwo");
            }
            else {
                Assert.Fail();
            }

            Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());

            env.UndeployAll();
        }

        private static void TryAssertionFragment(
            RegressionEnvironment env,
            EventRepresentationChoice eventRepresentationEnum)
        {
            var epl = eventRepresentationEnum.GetAnnotationText() +
                      " create schema InnerSchema as (key string);\n" +
                      eventRepresentationEnum.GetAnnotationText() +
                      " create schema MySchema as (inside InnerSchema, insidearr InnerSchema[]);\n" +
                      eventRepresentationEnum.GetAnnotationText() +
                      " @name('s0') select typeof(s0.inside) as t0, typeof(s0.insidearr) as t1 from MySchema as s0;\n";
            string[] fields = {"t0", "t1"};
            env.CompileDeployWBusPublicType(epl, new RegressionPath()).AddListener("s0");
            var deploymentId = env.DeploymentId("s0");

            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                env.SendEventObjectArray(new object[2], "MySchema");
            }
            else if (eventRepresentationEnum.IsMapEvent()) {
                env.SendEventMap(new Dictionary<string, object>(), "MySchema");
            }
            else if (eventRepresentationEnum.IsAvroEvent()) {
                var schema = SupportAvroUtil
                    .GetAvroSchema(env.Runtime.EventTypeService.GetEventType(deploymentId, "MySchema"))
                    .AsRecordSchema();
                env.SendEventAvro(new GenericRecord(schema), "MySchema");
            }
            else {
                Assert.Fail();
            }

            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {null, null});

            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                env.SendEventObjectArray(new object[] {new object[2], null}, "MySchema");
            }
            else if (eventRepresentationEnum.IsMapEvent()) {
                IDictionary<string, object> theEvent = new Dictionary<string, object>();
                theEvent.Put("inside", new Dictionary<string, object>());
                env.SendEventMap(theEvent, "MySchema");
            }
            else if (eventRepresentationEnum.IsAvroEvent()) {
                var mySchema = SupportAvroUtil
                    .GetAvroSchema(env.Runtime.EventTypeService.GetEventType(deploymentId, "MySchema"))
                    .AsRecordSchema();
                var innerSchema = SupportAvroUtil
                    .GetAvroSchema(env.Runtime.EventTypeService.GetEventType(deploymentId, "InnerSchema"))
                    .AsRecordSchema();
                var @event = new GenericRecord(mySchema);
                @event.Put("inside", new GenericRecord(innerSchema));
                env.SendEventAvro(@event, "MySchema");
            }
            else {
                Assert.Fail();
            }

            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"InnerSchema", null});

            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                env.SendEventObjectArray(new object[] {null, new object[2][]}, "MySchema");
            }
            else if (eventRepresentationEnum.IsMapEvent()) {
                IDictionary<string, object> theEvent = new Dictionary<string, object>();
                theEvent.Put("insidearr", new IDictionary<string, object>[0]);
                env.SendEventMap(theEvent, "MySchema");
            }
            else if (eventRepresentationEnum.IsAvroEvent()) {
                var mySchema = SupportAvroUtil
                    .GetAvroSchema(env.Runtime.EventTypeService.GetEventType(deploymentId, "MySchema"))
                    .AsRecordSchema();
                var @event = new GenericRecord(mySchema);
                @event.Put("insidearr", new EmptyList<object>());
                env.SendEventAvro(@event, "MySchema");
            }
            else {
                Assert.Fail();
            }

            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {null, "InnerSchema[]"});

            env.UndeployAll();
        }

        private static void TryAssertionDynamicProps(RegressionEnvironment env)
        {
            string[] fields = {"typeof(prop?)", "typeof(key)"};

            SendSchemaEvent(env, 1, "E1");
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"Integer", "String"});

            SendSchemaEvent(env, "test", "E2");
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"String", "String"});

            SendSchemaEvent(env, null, "E3");
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {null, "String"});
        }

        private static void SendSchemaEvent(
            RegressionEnvironment env,
            object prop,
            string key)
        {
            IDictionary<string, object> theEvent = new Dictionary<string, object>();
            theEvent.Put("prop", prop);
            theEvent.Put("key", key);
            env.SendEventMap(theEvent, "MyDynoPropSchema");
        }

        internal class ExprCoreTypeOfInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select typeof(xx) from SupportBean",
                    "Failed to validate select-clause expression 'typeof(xx)': Property named 'xx' is not valid in any stream [select typeof(xx) from SupportBean]");
            }
        }

        internal class ExprCoreTypeOfDynamicProps : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var schema = EventRepresentationChoice.MAP.GetAnnotationText() +
                             " create schema MyDynoPropSchema as (key string);\n";
                env.CompileDeployWBusPublicType(schema, path);

                var epl = "@Name('s0') select typeof(prop?), typeof(key) from MyDynoPropSchema as s0";
                env.CompileDeploy(epl, path).AddListener("s0");

                TryAssertionDynamicProps(env);

                env.UndeployModuleContaining("s0");

                env.EplToModelCompileDeploy(epl, path).AddListener("s0");

                TryAssertionDynamicProps(env);

                env.UndeployAll();
            }
        }

        internal class ExprCoreTypeOfVariantStream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                    TryAssertionVariantStream(env, rep);
                }
            }
        }

        internal class ExprCoreTypeOfNamedUnnamedPONO : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // test name-provided or no-name-provided
                var epl = "@Name('s0') select typeof(A) as t0 from ISupportA as A";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new ISupportAImpl(null, null));
                Assert.AreEqual(typeof(ISupportAImpl).Name, env.Listener("s0").AssertOneGetNewAndReset().Get("t0"));

                env.SendEventBean(new ISupportABCImpl(null, null, null, null));
                Assert.AreEqual("ISupportABCImpl", env.Listener("s0").AssertOneGetNewAndReset().Get("t0"));

                env.UndeployAll();
            }
        }

        internal class ExprCoreTypeOfFragment : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                    TryAssertionFragment(env, rep);
                }
            }
        }
    }
} // end of namespace