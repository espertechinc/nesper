///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Xml;

using Avro.Generic;

using com.espertech.esper.collection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compiler.client;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.runtime.client.scopetest;

using NEsper.Avro.Extensions;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

using SupportBean_N = com.espertech.esper.common.@internal.support.SupportBean_N;
using SupportBeanComplexProps = com.espertech.esper.common.@internal.support.SupportBeanComplexProps;

namespace com.espertech.esper.regressionlib.suite.epl.insertinto
{
    public class EPLInsertIntoPopulateUnderlying
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoCtor());
            execs.Add(new EPLInsertIntoCtorWithPattern());
            execs.Add(new EPLInsertIntoBeanJoin());
            execs.Add(new EPLInsertIntoPopulateBeanSimple());
            execs.Add(new EPLInsertIntoBeanWildcard());
            execs.Add(new EPLInsertIntoPopulateBeanObjects());
            execs.Add(new EPLInsertIntoPopulateUnderlyingSimple());
            execs.Add(new EPLInsertIntoCharSequenceCompat());
            execs.Add(new EPLInsertIntoBeanFactoryMethod());
            execs.Add(new EPLInsertIntoArrayPONOInsert());
            execs.Add(new EPLInsertIntoArrayMapInsert());
            execs.Add(new EPLInsertIntoWindowAggregationAtEventBean());
            execs.Add(new EPLInsertIntoInvalid());
            return execs;
        }

        private static void TryAssertionArrayMapInsert(
            RegressionEnvironment env,
            EventRepresentationChoice eventRepresentationEnum)
        {
            var path = new RegressionPath();
            var schema =
                eventRepresentationEnum.GetAnnotationText() +
                " create schema EventOne(Id string);\n" +
                eventRepresentationEnum.GetAnnotationText() +
                " create schema EventTwo(Id string, val int);\n" +
                eventRepresentationEnum.GetAnnotationText() +
                " create schema FinalEventValId (startEvent EventOne, endEvent EventTwo[]);\n" +
                eventRepresentationEnum.GetAnnotationText() +
                " create schema FinalEventInvalidNonArray (startEvent EventOne, endEvent EventTwo);\n" +
                eventRepresentationEnum.GetAnnotationText() +
                " create schema FinalEventInvalidArray (startEvent EventOne, endEvent EventTwo);\n";
            env.CompileDeployWBusPublicType(schema, path);

            env.AdvanceTime(0);

            // Test valid case of array insert
            var validEpl =
                "@Name('s0') INSERT INTO FinalEventValId SELECT s as startEvent, e as endEvent FROM PATTERN [" +
                "every s=EventOne -> e=EventTwo(Id=s.Id) until timer:interval(10 sec)]";
            env.CompileDeploy(validEpl, path).AddListener("s0");

            SendEventOne(env, eventRepresentationEnum, "G1");
            SendEventTwo(env, eventRepresentationEnum, "G1", 2);
            SendEventTwo(env, eventRepresentationEnum, "G1", 3);
            env.AdvanceTime(10000);

            EventBean startEventOne;
            EventBean endEventOne;
            EventBean endEventTwo;
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                var outArray = (object[]) env.Listener("s0").AssertOneGetNewAndReset().Underlying;
                startEventOne = (EventBean) outArray[0];
                endEventOne = ((EventBean[]) outArray[1])[0];
                endEventTwo = ((EventBean[]) outArray[1])[1];
            }
            else if (eventRepresentationEnum.IsMapEvent()) {
                var outMap = (IDictionary<string, object>) env.Listener("s0").AssertOneGetNewAndReset().Underlying;
                startEventOne = (EventBean) outMap.Get("startEvent");
                endEventOne = ((EventBean[]) outMap.Get("endEvent"))[0];
                endEventTwo = ((EventBean[]) outMap.Get("endEvent"))[1];
            }
            else if (eventRepresentationEnum.IsAvroEvent()) {
                var received = env.Listener("s0").AssertOneGetNewAndReset();
                startEventOne = (EventBean) received.GetFragment("startEvent");
                var endEvents = (EventBean[]) received.GetFragment("endEvent");
                endEventOne = endEvents[0];
                endEventTwo = endEvents[1];
            }
            else {
                throw new IllegalStateException("Unrecognized enum " + eventRepresentationEnum);
            }

            Assert.AreEqual("G1", startEventOne.Get("Id"));
            Assert.AreEqual(2, endEventOne.Get("val"));
            Assert.AreEqual(3, endEventTwo.Get("val"));

            // Test invalid case of non-array destination insert
            var invalidEpl =
                "INSERT INTO FinalEventInvalidNonArray SELECT s as startEvent, e as endEvent FROM PATTERN [" +
                "every s=EventOne -> e=EventTwo(Id=s.Id) until timer:interval(10 sec)]";
            try {
                env.CompileWCheckedEx(invalidEpl, path);
                Assert.Fail();
            }
            catch (EPCompileException ex) {
                string expected;
                if (eventRepresentationEnum.IsAvroEvent()) {
                    expected =
                        "Property 'endEvent' is incompatible, expecting an array of compatible schema 'EventTwo' but received schema 'EventTwo'";
                }
                else {
                    expected =
                        "Event type named 'FinalEventInvalidNonArray' has already been declared with differing column name or type information: Type by name 'FinalEventInvalidNonArray' in property 'endEvent' expected event type 'EventTwo' but receives event type array 'EventTwo'";
                }

                AssertMessage(ex, expected);
            }

            // Test invalid case of array destination insert from non-array var
            invalidEpl = "INSERT INTO FinalEventInvalidArray SELECT s as startEvent, e as endEvent FROM PATTERN [" +
                         "every s=EventOne -> e=EventTwo(Id=s.Id) until timer:interval(10 sec)]";
            try {
                env.CompileWCheckedEx(invalidEpl, path);
                Assert.Fail();
            }
            catch (EPCompileException ex) {
                string expected;
                if (eventRepresentationEnum.IsAvroEvent()) {
                    expected =
                        "Property 'endEvent' is incompatible, expecting an array of compatible schema 'EventTwo' but received schema 'EventTwo'";
                }
                else {
                    expected =
                        "Event type named 'FinalEventInvalidArray' has already been declared with differing column name or type information: Type by name 'FinalEventInvalidArray' in property 'endEvent' expected event type 'EventTwo' but receives event type array 'EventTwo'";
                }

                AssertMessage(ex, expected);
            }

            env.UndeployAll();
        }

        private static void SendEventTwo(
            RegressionEnvironment env,
            EventRepresentationChoice eventRepresentationEnum,
            string id,
            int val)
        {
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                env.SendEventObjectArray(new object[] {id, val}, "EventTwo");
            }
            else if (eventRepresentationEnum.IsMapEvent()) {
                IDictionary<string, object> theEvent = new LinkedHashMap<string, object>();
                theEvent.Put("Id", id);
                theEvent.Put("val", val);
                env.SendEventMap(theEvent, "EventTwo");
            }
            else if (eventRepresentationEnum.IsAvroEvent()) {
                var schema = SchemaBuilder.Record(
                    "name",
                    TypeBuilder.RequiredString("Id"),
                    TypeBuilder.RequiredInt("val"));
                var record = new GenericRecord(schema);
                record.Put("Id", id);
                record.Put("val", val);
                env.SendEventAvro(record, "EventTwo");
            }
            else {
                Assert.Fail();
            }
        }

        private static void SendEventOne(
            RegressionEnvironment env,
            EventRepresentationChoice eventRepresentationEnum,
            string id)
        {
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                env.SendEventObjectArray(new object[] {id}, "EventOne");
            }
            else if (eventRepresentationEnum.IsMapEvent()) {
                IDictionary<string, object> theEvent = new LinkedHashMap<string, object>();
                theEvent.Put("Id", id);
                env.SendEventMap(theEvent, "EventOne");
            }
            else if (eventRepresentationEnum.IsAvroEvent()) {
                var schema = SchemaBuilder.Record("name", TypeBuilder.RequiredString("Id"));
                var record = new GenericRecord(schema);
                record.Put("Id", id);
                env.SendEventAvro(record, "EventOne");
            }
            else {
                Assert.Fail();
            }
        }

        private static void SendReceiveTwo(
            RegressionEnvironment env,
            SupportListener listener,
            string theString,
            int? intBoxed)
        {
            var bean = new SupportBean(theString, -1);
            bean.IntBoxed = intBoxed;
            env.SendEventBean(bean);
            var theEvent = (SupportBeanCtorOne) env.Listener("s0").AssertOneGetNewAndReset().Underlying;
            Assert.AreEqual(theString, theEvent.TheString);
            Assert.AreEqual(null, theEvent.IntBoxed);
            Assert.AreEqual(intBoxed, (int?) theEvent.IntPrimitive);
        }

        private static void SendReceive(
            RegressionEnvironment env,
            SupportListener listener,
            string theString,
            int intPrimitive,
            bool boolPrimitive,
            int? intBoxed)
        {
            var bean = new SupportBean(theString, intPrimitive);
            bean.BoolPrimitive = boolPrimitive;
            bean.IntBoxed = intBoxed;
            env.SendEventBean(bean);
            var theEvent = (SupportBeanCtorOne) env.Listener("s0").AssertOneGetNewAndReset().Underlying;
            Assert.AreEqual(theString, theEvent.TheString);
            Assert.AreEqual(intBoxed, theEvent.IntBoxed);
            Assert.AreEqual(boolPrimitive, theEvent.BoolPrimitive);
            Assert.AreEqual(intPrimitive, theEvent.IntPrimitive);
        }

        private static void TryAssertionPopulateUnderlying(
            RegressionEnvironment env,
            string typeName)
        {
            env.CompileDeploy("@Name('select') select * from " + typeName);

            var stmtTextOne = "@Name('s0') insert into " +
                              typeName +
                              " select IntPrimitive as intVal, TheString as stringVal, DoubleBoxed as doubleVal from SupportBean";
            env.CompileDeploy(stmtTextOne).AddListener("s0");

            Assert.AreSame(env.Statement("select").EventType, env.Statement("s0").EventType);

            var bean = new SupportBean();
            bean.IntPrimitive = 1000;
            bean.TheString = "E1";
            bean.DoubleBoxed = 1001d;
            env.SendEventBean(bean);

            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                new [] { "intVal","stringVal","doubleVal" },
                new object[] {1000, "E1", 1001d});
            env.UndeployAll();
        }

        internal class EPLInsertIntoWindowAggregationAtEventBean : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                        "@Name('s0') insert into SupportBeanArrayEvent select window(*) @eventbean from SupportBean#keepall")
                    .AddListener("s0");

                var e1 = new SupportBean("E1", 1);
                env.SendEventBean(e1);
                AssertMyEventTargetWithArray(env.Listener("s0").AssertOneGetNewAndReset(), e1);

                var e2 = new SupportBean("E2", 2);
                env.SendEventBean(e2);
                AssertMyEventTargetWithArray(env.Listener("s0").AssertOneGetNewAndReset(), e1, e2);

                env.UndeployAll();
            }

            private static void AssertMyEventTargetWithArray(
                EventBean eventBean,
                params SupportBean[] beans)
            {
                var und = (SupportBeanArrayEvent) eventBean.Underlying;
                EPAssertionUtil.AssertEqualsExactOrder(und.Array, beans);
            }
        }

        internal class EPLInsertIntoCtor : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // simple type and null values
                var eplOne =
                    "@Name('s0') insert into SupportBeanCtorOne select TheString, IntBoxed, IntPrimitive, BoolPrimitive from SupportBean";
                env.CompileDeploy(eplOne).AddListener("s0");

                SendReceive(env, env.Listener("s0"), "E1", 2, true, 100);
                SendReceive(env, env.Listener("s0"), "E2", 3, false, 101);
                SendReceive(env, env.Listener("s0"), null, 4, true, null);
                env.UndeployModuleContaining("s0");

                // boxable type and null values
                var eplTwo =
                    "@Name('s0') insert into SupportBeanCtorOne select TheString, null, IntBoxed from SupportBean";
                env.CompileDeploy(eplTwo).AddListener("s0");
                SendReceiveTwo(env, env.Listener("s0"), "E1", 100);
                env.UndeployModuleContaining("s0");

                // test join wildcard
                var eplThree =
                    "@Name('s0') insert into SupportBeanCtorTwo select * from SupportBean_ST0#lastevent, SupportBean_ST1#lastevent";
                env.CompileDeploy(eplThree).AddListener("s0");

                env.SendEventBean(new SupportBean_ST0("ST0", 1));
                env.SendEventBean(new SupportBean_ST1("ST1", 2));
                var theEvent = (SupportBeanCtorTwo) env.Listener("s0").AssertOneGetNewAndReset().Underlying;
                Assert.IsNotNull(theEvent.St0);
                Assert.IsNotNull(theEvent.St1);
                env.UndeployModuleContaining("s0");

                // test (should not use column names)
                var eplFour =
                    "@Name('s0') insert into SupportBeanCtorOne(TheString, IntPrimitive) select 'E1', 5 from SupportBean";
                env.CompileDeploy(eplFour).AddListener("s0");
                env.SendEventBean(new SupportBean("x", -1));
                var eventOne = (SupportBeanCtorOne) env.Listener("s0").AssertOneGetNewAndReset().Underlying;
                Assert.AreEqual("E1", eventOne.TheString);
                Assert.AreEqual(99, eventOne.IntPrimitive);
                Assert.AreEqual((int?) 5, eventOne.IntBoxed);

                // test Ctor accepting same types
                env.UndeployAll();
                var epl =
                    "@Name('s0') insert into SupportEventWithCtorSameType select c1,c2 from SupportBean(TheString='b1')#lastevent as c1, SupportBean(TheString='b2')#lastevent as c2";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("b1", 1));
                env.SendEventBean(new SupportBean("b2", 2));
                var result = (SupportEventWithCtorSameType) env.Listener("s0").AssertOneGetNewAndReset().Underlying;
                Assert.AreEqual(1, result.B1.IntPrimitive);
                Assert.AreEqual(2, result.B2.IntPrimitive);

                env.UndeployAll();
            }
        }

        internal class EPLInsertIntoCtorWithPattern : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Test valid case of array insert
                var epl = "@Name('s0') insert into SupportBeanCtorThree select s, e FROM PATTERN [" +
                          "every s=SupportBean_ST0 -> [2] e=SupportBean_ST1]";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean_ST0("E0", 1));
                env.SendEventBean(new SupportBean_ST1("E1", 2));
                env.SendEventBean(new SupportBean_ST1("E2", 3));
                var three = (SupportBeanCtorThree) env.Listener("s0").AssertOneGetNewAndReset().Underlying;
                Assert.AreEqual("E0", three.St0.Id);
                Assert.AreEqual(2, three.St1.Length);
                Assert.AreEqual("E1", three.St1[0].Id);
                Assert.AreEqual("E2", three.St1[1].Id);

                env.UndeployAll();
            }
        }

        internal class EPLInsertIntoBeanJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var n1 = new SupportBean_N(1, 10, 100d, 1000d, true, true);
                // test wildcard
                var stmtTextOne =
                    "@Name('s0') insert into SupportBeanObject select * from SupportBean_N#lastevent as one, SupportBean_S0#lastevent as two";
                env.CompileDeploy(stmtTextOne).AddListener("s0");

                env.SendEventBean(n1);
                var s01 = new SupportBean_S0(1);
                env.SendEventBean(s01);
                var theEvent = (SupportBeanObject) env.Listener("s0").AssertOneGetNewAndReset().Underlying;
                Assert.AreSame(n1, theEvent.One);
                Assert.AreSame(s01, theEvent.Two);
                env.UndeployModuleContaining("s0");

                // test select stream names
                stmtTextOne =
                    "@Name('s0') insert into SupportBeanObject select one, two from SupportBean_N#lastevent as one, SupportBean_S0#lastevent as two";
                env.CompileDeploy(stmtTextOne).AddListener("s0");

                env.SendEventBean(n1);
                env.SendEventBean(s01);
                theEvent = (SupportBeanObject) env.Listener("s0").AssertOneGetNewAndReset().Underlying;
                Assert.AreSame(n1, theEvent.One);
                Assert.AreSame(s01, theEvent.Two);
                env.UndeployModuleContaining("s0");

                // test fully-qualified class name as target
                stmtTextOne =
                    "@Name('s0') insert into SupportBeanObject select one, two from SupportBean_N#lastevent as one, SupportBean_S0#lastevent as two";
                env.CompileDeploy(stmtTextOne).AddListener("s0");

                env.SendEventBean(n1);
                env.SendEventBean(s01);
                theEvent = (SupportBeanObject) env.Listener("s0").AssertOneGetNewAndReset().Underlying;
                Assert.AreSame(n1, theEvent.One);
                Assert.AreSame(s01, theEvent.Two);
                env.UndeployModuleContaining("s0");

                // test local class and auto-import
                stmtTextOne = "@Name('s0') insert into " +
                              typeof(EPLInsertIntoPopulateUnderlying).Name +
                              "$MyLocalTarget select 1 as value from SupportBean_N";
                env.CompileDeploy(stmtTextOne).AddListener("s0");
                env.SendEventBean(n1);
                var eventLocal = (MyLocalTarget) env.Listener("s0").AssertOneGetNewAndReset().Underlying;
                Assert.AreEqual(1, eventLocal.Value);
                env.UndeployAll();
            }
        }

        internal class EPLInsertIntoInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "insert into SupportBeanCtorOne select 1 from SupportBean";
                TryInvalidCompile(
                    env,
                    text,
                    "Failed to find a suitable constructor for class '" +
                    typeof(SupportBeanCtorOne).Name +
                    "': Could not find constructor in class '" +
                    typeof(SupportBeanCtorOne).Name +
                    "' with matching parameter number and expected parameter type(s) 'int'");

                text = "insert into SupportBean(IntPrimitive) select 1L from SupportBean";
                TryInvalidCompile(
                    env,
                    text,
                    "Invalid assignment of column 'IntPrimitive' of type 'long' to event property 'IntPrimitive' typed as 'int', column and parameter types mismatch [insert into SupportBean(IntPrimitive) select 1L from SupportBean]");

                text = "insert into SupportBean(IntPrimitive) select null from SupportBean";
                TryInvalidCompile(
                    env,
                    text,
                    "Invalid assignment of column 'IntPrimitive' of null type to event property 'IntPrimitive' typed as 'int', nullable type mismatch [insert into SupportBean(IntPrimitive) select null from SupportBean]");

                text = "insert into SupportBeanReadOnly select 'a' as geom from SupportBean";
                TryInvalidCompile(
                    env,
                    text,
                    "Failed to find a suitable constructor for class '" +
                    typeof(SupportBeanReadOnly).Name +
                    "': Could not find constructor in class '" +
                    typeof(SupportBeanReadOnly).Name +
                    "' with matching parameter number and expected parameter type(s) 'String' (nearest matching constructor taking no parameters) [insert into SupportBeanReadOnly select 'a' as geom from SupportBean]");

                text = "insert into SupportBean select 3 as dummyField from SupportBean";
                TryInvalidCompile(
                    env,
                    text,
                    "Column 'dummyField' could not be assigned to any of the properties of the underlying type (missing column names, event property, setter method or constructor?) [insert into SupportBean select 3 as dummyField from SupportBean]");

                text = "insert into SupportBean select 3 from SupportBean";
                TryInvalidCompile(
                    env,
                    text,
                    "Column '3' could not be assigned to any of the properties of the underlying type (missing column names, event property, setter method or constructor?) [insert into SupportBean select 3 from SupportBean]");

                text = "insert into SupportBeanInterfaceProps(isa) select isbImpl from MyMap";
                TryInvalidCompile(
                    env,
                    text,
                    "Invalid assignment of column 'isa' of type '" +
                    typeof(ISupportBImpl).Name +
                    "' to event property 'isa' typed as '" +
                    typeof(ISupportA).Name +
                    "', column and parameter types mismatch [insert into SupportBeanInterfaceProps(isa) select isbImpl from MyMap]");

                text = "insert into SupportBeanInterfaceProps(isg) select isabImpl from MyMap";
                TryInvalidCompile(
                    env,
                    text,
                    "Invalid assignment of column 'isg' of type '" +
                    typeof(ISupportBaseABImpl).Name +
                    "' to event property 'isg' typed as '" +
                    typeof(ISupportAImplSuperG).Name +
                    "', column and parameter types mismatch [insert into SupportBeanInterfaceProps(isg) select isabImpl from MyMap]");

                text = "insert into SupportBean(dummy) select 3 from SupportBean";
                TryInvalidCompile(
                    env,
                    text,
                    "Column 'dummy' could not be assigned to any of the properties of the underlying type (missing column names, event property, setter method or constructor?) [insert into SupportBean(dummy) select 3 from SupportBean]");

                text = "insert into SupportBeanReadOnly(side) select 'E1' from MyMap";
                TryInvalidCompile(
                    env,
                    text,
                    "Failed to find a suitable constructor for class '" +
                    typeof(SupportBeanReadOnly).Name +
                    "': Could not find constructor in class '" +
                    typeof(SupportBeanReadOnly).Name +
                    "' with matching parameter number and expected parameter type(s) 'String' (nearest matching constructor taking no parameters) [insert into SupportBeanReadOnly(side) select 'E1' from MyMap]");

                var path = new RegressionPath();
                env.CompileDeploy("insert into ABCStream select *, 1+1 from SupportBean", path);
                text = "insert into ABCStream(string) select 'E1' from MyMap";
                TryInvalidCompile(
                    env,
                    path,
                    text,
                    "Event type named 'ABCStream' has already been declared with differing column name or type information: Type by name 'ABCStream' is not a compatible type (target type underlying is '" +
                    typeof(Pair<object, object>).Name +
                    "') [insert into ABCStream(string) select 'E1' from MyMap]");

                text = "insert into xmltype select 1 from SupportBean";
                TryInvalidCompile(
                    env,
                    text,
                    "Event type named 'xmltype' has already been declared with differing column name or type information: Type by name 'xmltype' is not a compatible type (target type underlying is '" +
                    typeof(XmlNode).Name +
                    "') [insert into xmltype select 1 from SupportBean]");

                text = "insert into MyMap(dummy) select 1 from SupportBean";
                TryInvalidCompile(
                    env,
                    text,
                    "Event type named 'MyMap' has already been declared with differing column name or type information: Type by name 'MyMap' expects 10 properties but receives 1 properties [insert into MyMap(dummy) select 1 from SupportBean]");

                // setter throws exception
                var stmtTextOne = "@Name('s0') insert into SupportBeanErrorTestingTwo(value) select 'E1' from MyMap";
                env.CompileDeploy(stmtTextOne).AddListener("s0");

                try {
                    env.SendEventMap(new Dictionary<string, object>(), "MyMap");
                    Assert.Fail();
                }
                catch (EPException ex) {
                    // expected
                }

                env.UndeployAll();

                // surprise - wrong type than defined
                stmtTextOne = "@Name('s0') insert into SupportBean(IntPrimitive) select anint from MyMap";
                env.CompileDeploy(stmtTextOne).AddListener("s0");
                env.Listener("s0").Reset();
                IDictionary<string, object> map = new Dictionary<string, object>();
                map.Put("anint", "notAnInt");
                try {
                    env.SendEventBean(map, "MyMap");
                    Assert.AreEqual(0, env.Listener("s0").AssertOneGetNewAndReset().Get("IntPrimitive"));
                }
                catch (Exception ex) {
                    // an exception is possible and up to the implementation.
                }

                // ctor throws exception
                env.UndeployAll();
                var stmtTextThree = "@Name('s0') insert into SupportBeanCtorOne select 'E1' from SupportBean";
                env.CompileDeploy(stmtTextThree).AddListener("s0");
                try {
                    env.SendEventBean(new SupportBean("E1", 1));
                    Assert.Fail(); // rethrowing handler registered
                }
                catch (Exception ex) {
                    // expected
                }

                // allow automatic cast of same-type event
                path.Clear();
                env.CompileDeploy("create schema MapOneA as (prop1 string)", path);
                env.CompileDeploy("create schema MapTwoA as (prop1 string)", path);
                env.CompileDeploy("insert into MapOneA select * from MapTwoA", path);

                env.UndeployAll();
            }
        }

        internal class EPLInsertIntoPopulateBeanSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // test select column names
                var stmtTextOne = "@Name('i1') insert into SupportBean select " +
                                  "'E1' as TheString, 1 as IntPrimitive, 2 as IntBoxed, 3L as LongPrimitive," +
                                  "null as LongBoxed, true as BoolPrimitive, " +
                                  "'x' as CharPrimitive, 0xA as BytePrimitive, " +
                                  "8.0f as FloatPrimitive, 9.0d as DoublePrimitive, " +
                                  "0x05 as ShortPrimitive, SupportEnum.ENUM_VALUE_2 as EnumValue " +
                                  " from MyMap";
                env.CompileDeploy(stmtTextOne);

                var stmtTextTwo = "@Name('s0') select * from SupportBean";
                env.CompileDeploy(stmtTextTwo).AddListener("s0");

                env.SendEventMap(new Dictionary<string, object>(), "MyMap");
                var received = (SupportBean) env.Listener("s0").AssertOneGetNewAndReset().Underlying;
                Assert.AreEqual("E1", received.TheString);
                SupportBean.Compare(
                    received,
                    "IntPrimitive,IntBoxed,LongPrimitive,LongBoxed,BoolPrimitive,CharPrimitive,BytePrimitive,FloatPrimitive,DoublePrimitive,ShortPrimitive,EnumValue"
                        .SplitCsv(),
                    new object[] {1, 2, 3L, null, true, 'x', (byte) 10, 8f, 9d, (short) 5, SupportEnum.ENUM_VALUE_2});

                // test insert-into column names
                env.UndeployModuleContaining("s0");
                env.UndeployModuleContaining("i1");

                stmtTextOne = "@Name('s0') insert into SupportBean(TheString, IntPrimitive, IntBoxed, LongPrimitive," +
                              "LongBoxed, BoolPrimitive, CharPrimitive, BytePrimitive, FloatPrimitive, DoublePrimitive, " +
                              "ShortPrimitive, EnumValue) select " +
                              "'E1', 1, 2, 3L," +
                              "null, true, " +
                              "'x', 0xA, " +
                              "8.0f, 9.0d, " +
                              "0x05 as ShortPrimitive, SupportEnum.ENUM_VALUE_2 " +
                              " from MyMap";
                env.CompileDeploy(stmtTextOne).AddListener("s0");

                env.SendEventMap(new Dictionary<string, object>(), "MyMap");
                received = (SupportBean) env.Listener("s0").AssertOneGetNewAndReset().Underlying;
                Assert.AreEqual("E1", received.TheString);
                SupportBean.Compare(
                    received,
                    "IntPrimitive,IntBoxed,LongPrimitive,LongBoxed,BoolPrimitive,CharPrimitive,BytePrimitive,FloatPrimitive,DoublePrimitive,ShortPrimitive,EnumValue"
                        .SplitCsv(),
                    new object[] {1, 2, 3L, null, true, 'x', (byte) 10, 8f, 9d, (short) 5, SupportEnum.ENUM_VALUE_2});

                // test convert Integer boxed to Long boxed
                env.UndeployModuleContaining("s0");
                stmtTextOne =
                    "@Name('s0') insert into SupportBean(LongBoxed, DoubleBoxed) select IntBoxed, FloatBoxed from MyMap";
                env.CompileDeploy(stmtTextOne).AddListener("s0");

                IDictionary<string, object> vals = new Dictionary<string, object>();
                vals.Put("IntBoxed", 4);
                vals.Put("FloatBoxed", 0f);
                env.SendEventMap(vals, "MyMap");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "LongBoxed","DoubleBoxed" },
                    new object[] {4L, 0d});
                env.UndeployAll();

                // test new-to-map conversion
                env.CompileDeploy(
                        "@Name('s0') insert into MyEventWithMapFieldSetter(Id, themap) " +
                        "select 'test' as Id, new {somefield = TheString} as themap from SupportBean")
                    .AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertPropsMap(
                    (IDictionary<string, object>) env.Listener("s0").AssertOneGetNew().Get("themap"),
                    new [] { "somefield" },
                    "E1");

                env.UndeployAll();
            }
        }

        internal class EPLInsertIntoBeanWildcard : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtTextOne = "@Name('s0') insert into SupportBean select * from MySupportMap";
                env.CompileDeploy(stmtTextOne).AddListener("s0");

                IDictionary<string, object> vals = new Dictionary<string, object>();
                vals.Put("IntPrimitive", 4);
                vals.Put("LongBoxed", 100L);
                vals.Put("TheString", "E1");
                vals.Put("BoolPrimitive", true);

                env.SendEventMap(vals, "MySupportMap");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "IntPrimitive","LongBoxed","TheString","BoolPrimitive" },
                    new object[] {4, 100L, "E1", true});

                env.UndeployAll();
            }
        }

        internal class EPLInsertIntoPopulateBeanObjects : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // arrays and maps
                var stmtTextOne =
                    "@Name('s0') insert into SupportBeanComplexProps(ArrayProperty,ObjectArray,mapProperty) select " +
                    "IntArr,{10,20,30},mapProp" +
                    " from MyMap as m";
                env.CompileDeploy(stmtTextOne).AddListener("s0");

                IDictionary<string, object> mymapVals = new Dictionary<string, object>();
                mymapVals.Put("IntArr", new[] {-1, -2});
                IDictionary<string, object> inner = new Dictionary<string, object>();
                inner.Put("mykey", "myval");
                mymapVals.Put("mapProp", inner);
                env.SendEventMap(mymapVals, "MyMap");
                var theEvent = (SupportBeanComplexProps) env.Listener("s0").AssertOneGetNewAndReset().Underlying;
                Assert.AreEqual(-2, theEvent.ArrayProperty[1]);
                Assert.AreEqual(20, theEvent.ObjectArray[1]);
                Assert.AreEqual("myval", theEvent.MapProperty.Get("mykey"));
                env.UndeployModuleContaining("s0");

                // inheritance
                stmtTextOne = "@Name('s0') insert into SupportBeanInterfaceProps(isa,isg) select " +
                              "isaImpl,isgImpl" +
                              " from MyMap";
                env.CompileDeploy(stmtTextOne).AddListener("s0");

                mymapVals = new Dictionary<string, object>();
                mymapVals.Put("mapProp", inner);
                env.SendEventMap(mymapVals, "MyMap");
                Assert.IsTrue(env.Listener("s0").AssertOneGetNewAndReset().Underlying is SupportBeanInterfaceProps);
                Assert.AreEqual(typeof(SupportBeanInterfaceProps), env.Statement("s0").EventType.UnderlyingType);
                env.UndeployModuleContaining("s0");

                // object values from Map same type
                stmtTextOne = "@Name('s0') insert into SupportBeanComplexProps(nested) select nested from MyMap";
                env.CompileDeploy(stmtTextOne).AddListener("s0");

                mymapVals = new Dictionary<string, object>();
                mymapVals.Put("nested", new SupportBeanComplexProps.SupportBeanSpecialGetterNested("111", "222"));
                env.SendEventMap(mymapVals, "MyMap");
                var eventThree = (SupportBeanComplexProps) env.Listener("s0").AssertOneGetNewAndReset().Underlying;
                Assert.AreEqual("111", eventThree.Nested.NestedValue);
                env.UndeployModuleContaining("s0");

                // object to Object
                stmtTextOne =
                    "@Name('s0') insert into SupportBeanArrayCollMap(AnyObject) select Nested from SupportBeanComplexProps";
                env.CompileDeploy(stmtTextOne).AddListener("s0");

                env.SendEventBean(SupportBeanComplexProps.MakeDefaultBean());
                var eventFour = (SupportBeanArrayCollMap) env.Listener("s0").AssertOneGetNewAndReset().Underlying;
                Assert.AreEqual(
                    "NestedValue",
                    ((SupportBeanComplexProps.SupportBeanSpecialGetterNested) eventFour.AnyObject).NestedValue);
                env.UndeployModuleContaining("s0");

                // test null value
                var stmtTextThree =
                    "@Name('s0') insert into SupportBean select 'B' as TheString, IntBoxed as IntPrimitive from SupportBean(TheString='A')";
                env.CompileDeploy(stmtTextThree).AddListener("s0");

                env.SendEventBean(new SupportBean("A", 0));
                var received = (SupportBean) env.Listener("s0").AssertOneGetNewAndReset().Underlying;
                Assert.AreEqual(0, received.IntPrimitive);

                var bean = new SupportBean("A", 1);
                bean.IntBoxed = 20;
                env.SendEventBean(bean);
                received = (SupportBean) env.Listener("s0").AssertOneGetNewAndReset().Underlying;
                Assert.AreEqual(20, received.IntPrimitive);

                env.UndeployAll();
            }
        }

        internal class EPLInsertIntoPopulateUnderlyingSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryAssertionPopulateUnderlying(env, "MyMapType");
                TryAssertionPopulateUnderlying(env, "MyOAType");
                TryAssertionPopulateUnderlying(env, "MyAvroType");
            }
        }

        internal class EPLInsertIntoCharSequenceCompat : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                    var path = new RegressionPath();
                    env.CompileDeploy(
                        "create " +
                        rep.GetOutputTypeCreateSchemaName() +
                        " schema ConcreteType as (value System.CharSequence)",
                        path);
                    env.CompileDeploy("insert into ConcreteType select \"Test\" as value from SupportBean", path);
                    env.UndeployAll();
                }
            }
        }

        internal class EPLInsertIntoBeanFactoryMethod : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // test factory method on the same event class
                var stmtTextOne = "@Name('s0') insert into SupportBeanString select 'abc' as TheString from MyMap";
                env.CompileDeploy(stmtTextOne).AddListener("s0");

                var subscriber = new SupportSubscriber();
                env.Statement("s0").Subscriber = subscriber;

                env.SendEventMap(new Dictionary<string, object>(), "MyMap");
                Assert.AreEqual("abc", env.Listener("s0").AssertOneGetNewAndReset().Get("TheString"));
                Assert.AreEqual("abc", subscriber.AssertOneGetNewAndReset());
                env.UndeployModuleContaining("s0");

                // test factory method fully-qualified
                stmtTextOne = "@Name('s0') insert into SupportSensorEvent(Id, type, device, measurement, confIdence)" +
                              "select 2, 'A01', 'DHC1000', 100, 5 from MyMap";
                env.CompileDeploy(stmtTextOne).AddListener("s0");

                env.SendEventMap(new Dictionary<string, object>(), "MyMap");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "id","type","device","measurement","confIdence" },
                    new object[] {2, "A01", "DHC1000", 100.0, 5.0});

                Assert.That(
                    () => TypeHelper.Instantiate(typeof(SupportBeanString)),
                    Throws.InstanceOf<TypeInstantiationException>());

                Assert.That(
                    () => TypeHelper.Instantiate(typeof(SupportSensorEvent)),
                    Throws.InstanceOf<TypeInstantiationException>());

                env.UndeployAll();
            }
        }

        internal class EPLInsertIntoArrayPONOInsert : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "create schema FinalEventInvalidNonArray as " +
                          typeof(FinalEventInvalidNonArray).Name +
                          ";\n" +
                          "create schema FinalEventInvalidArray as " +
                          typeof(FinalEventInvalidArray).Name +
                          ";\n" +
                          "create schema FinalEventValId as " +
                          typeof(FinalEventValid).Name +
                          ";\n";
                env.CompileDeploy(epl, path);
                env.AdvanceTime(0);

                // Test valid case of array insert
                var validEpl =
                    "@Name('s0') INSERT INTO FinalEventValId SELECT s as startEvent, e as endEvent FROM PATTERN [" +
                    "every s=SupportBean_S0 -> e=SupportBean(TheString=s.P00) until timer:interval(10 sec)]";
                env.CompileDeploy(validEpl, path).AddListener("s0");

                env.SendEventBean(new SupportBean_S0(1, "G1"));
                env.SendEventBean(new SupportBean("G1", 2));
                env.SendEventBean(new SupportBean("G1", 3));
                env.AdvanceTime(10000);

                var outEvent = (FinalEventValid) env.Listener("s0").AssertOneGetNewAndReset().Underlying;
                Assert.AreEqual(1, outEvent.StartEvent.Id);
                Assert.AreEqual("G1", outEvent.StartEvent.P00);
                Assert.AreEqual(2, outEvent.EndEvent.Length);
                Assert.AreEqual(2, outEvent.EndEvent[0].IntPrimitive);
                Assert.AreEqual(3, outEvent.EndEvent[1].IntPrimitive);

                // Test invalid case of non-array destination insert
                var invalidEpl =
                    "INSERT INTO FinalEventInvalidNonArray SELECT s as startEvent, e as endEvent FROM PATTERN [" +
                    "every s=SupportBean_S0 -> e=SupportBean(TheString=s.P00) until timer:interval(10 sec)]";
                TryInvalidCompile(
                    env,
                    path,
                    invalidEpl,
                    "Invalid assignment of column 'endEvent' of type '" +
                    typeof(SupportBean).Name +
                    "[]' to event property 'endEvent' typed as '" +
                    typeof(SupportBean).Name +
                    "', column and parameter types mismatch [INSERT INTO FinalEventInvalidNonArray SELECT s as startEvent, e as endEvent FROM PATTERN [every s=SupportBean_S0 -> e=SupportBean(TheString=s.P00) until timer:interval(10 sec)]]");

                // Test invalid case of array destination insert from non-array var
                var invalidEplTwo =
                    "INSERT INTO FinalEventInvalidArray SELECT s as startEvent, e as endEvent FROM PATTERN [" +
                    "every s=SupportBean_S0 -> e=SupportBean(TheString=s.P00) until timer:interval(10 sec)]";
                TryInvalidCompile(
                    env,
                    path,
                    invalidEplTwo,
                    "Invalid assignment of column 'startEvent' of type '" +
                    typeof(SupportBean_S0).Name +
                    "' to event property 'startEvent' typed as '" +
                    typeof(SupportBean_S0).Name +
                    "[]', column and parameter types mismatch [INSERT INTO FinalEventInvalidArray SELECT s as startEvent, e as endEvent FROM PATTERN [every s=SupportBean_S0 -> e=SupportBean(TheString=s.P00) until timer:interval(10 sec)]]");

                env.UndeployAll();
            }
        }

        internal class EPLInsertIntoArrayMapInsert : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                    TryAssertionArrayMapInsert(env, rep);
                }
            }
        }

        public class FinalEventInvalidNonArray
        {
            public SupportBean EndEvent { get; set; }

            public SupportBean_S0 StartEvent { get; set; }
        }

        public class FinalEventInvalidArray
        {
            public SupportBean[] EndEvent { get; set; }

            public SupportBean_S0[] StartEvent { get; set; }
        }

        public class FinalEventValid
        {
            public SupportBean[] EndEvent { get; set; }

            public SupportBean_S0 StartEvent { get; set; }
        }

        public class MyLocalTarget
        {
            public int _value;

            public int Value {
                get => _value;
                set => _value = value;
            }

            public void SetValue(int value)
            {
                _value = value;
            }
        }
    }
} // end of namespace