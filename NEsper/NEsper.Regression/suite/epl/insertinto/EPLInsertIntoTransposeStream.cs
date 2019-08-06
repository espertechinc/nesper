///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.collection;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compiler.client;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;
using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;

namespace com.espertech.esper.regressionlib.suite.epl.insertinto
{
    public class EPLInsertIntoTransposeStream
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoTransposeMapAndObjectArray());
            execs.Add(new EPLInsertIntoTransposeFunctionToStreamWithProps());
            execs.Add(new EPLInsertIntoTransposeFunctionToStream());
            execs.Add(new EPLInsertIntoTransposeSingleColumnInsert());
            execs.Add(new EPLInsertIntoTransposeEventJoinMap());
            execs.Add(new EPLInsertIntoTransposeEventJoinPONO());
            execs.Add(new EPLInsertIntoTransposePONOPropertyStream());
            execs.Add(new EPLInsertIntoInvalidTranspose());
            return execs;
        }

        private static IDictionary<string, object> MakeMap(object[][] entries)
        {
            var result = new Dictionary<string, object>();
            foreach (var entry in entries) {
                result.Put((string) entry[0], entry[1]);
            }

            return result;
        }

        internal class EPLInsertIntoTransposeMapAndObjectArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                    RunTransposeMapAndObjectArray(env, rep);
                }
            }

            private static void RunTransposeMapAndObjectArray(
                RegressionEnvironment env,
                EventRepresentationChoice representation)
            {
                var fields = "p0,p1".SplitCsv();
                var path = new RegressionPath();
                var schema = "create " +
                             representation.GetOutputTypeCreateSchemaName() +
                             " schema MySchema(p0 string, p1 int)";
                env.CompileDeployWBusPublicType(schema, path);

                string generateFunction;
                if (representation.IsObjectArrayEvent()) {
                    generateFunction = "generateOA";
                }
                else if (representation.IsMapEvent()) {
                    generateFunction = "generateMap";
                }
                else if (representation.IsAvroEvent()) {
                    generateFunction = "generateAvro";
                }
                else {
                    throw new IllegalStateException("Unrecognized code " + representation);
                }

                var epl = "insert into MySchema select transpose(" +
                          generateFunction +
                          "(TheString, IntPrimitive)) from SupportBean";
                env.CompileDeploy("@Name('s0') " + epl, path).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 1});

                env.SendEventBean(new SupportBean("E2", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 2});

                // MySchema already exists, start second statement
                env.CompileDeploy("@Name('s1') " + epl, path).AddListener("s1");
                env.UndeployModuleContaining("s0");

                env.SendEventBean(new SupportBean("E3", 3));
                EPAssertionUtil.AssertProps(
                    env.Listener("s1").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3", 3});

                env.UndeployAll();
            }
        }

        internal class EPLInsertIntoTransposeFunctionToStreamWithProps : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtTextOne =
                    "insert into MyStream select 1 as dummy, transpose(custom('O' || TheString, 10)) from SupportBean(TheString like 'I%')";
                env.CompileDeploy(stmtTextOne, path);

                var stmtTextTwo = "@Name('s0') select * from MyStream";
                env.CompileDeploy(stmtTextTwo, path).AddListener("s0");

                var type = env.Statement("s0").EventType;
                Assert.AreEqual(typeof(Pair<,>), type.UnderlyingType);

                env.SendEventBean(new SupportBean("I1", 1));
                var result = env.Listener("s0").AssertOneGetNewAndReset();
                var underlying = (Pair<SupportBean, object>) result.Underlying;
                EPAssertionUtil.AssertProps(
                    result,
                    "dummy,TheString,IntPrimitive".SplitCsv(),
                    new object[] {1, "OI1", 10});
                Assert.AreEqual("OI1", underlying.First.TheString);

                env.UndeployAll();
            }
        }

        internal class EPLInsertIntoTransposeFunctionToStream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtTextOne =
                    "insert into OtherStream select transpose(custom('O' || TheString, 10)) from SupportBean(TheString like 'I%')";
                env.CompileDeploy("@Name('first') " + stmtTextOne, path).AddListener("first");

                var stmtTextTwo = "@Name('s0') select * from OtherStream(TheString like 'O%')";
                env.CompileDeploy(stmtTextTwo, path).AddListener("s0");

                var type = env.Statement("s0").EventType;
                Assert.AreEqual(typeof(SupportBean), type.UnderlyingType);

                env.SendEventBean(new SupportBean("I1", 1));
                var result = env.Listener("s0").AssertOneGetNewAndReset();
                EPAssertionUtil.AssertProps(
                    result,
                    "theString,IntPrimitive".SplitCsv(),
                    new object[] {"OI1", 10});
                Assert.AreEqual("OI1", ((SupportBean) result.Underlying).TheString);

                // try second statement as "OtherStream" now already exists
                env.CompileDeploy("@Name('second') " + stmtTextOne).AddListener("second");
                env.UndeployModuleContaining("s0");
                env.SendEventBean(new SupportBean("I2", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("second").AssertOneGetNewAndReset(),
                    "theString,IntPrimitive".SplitCsv(),
                    new object[] {"OI2", 10});

                env.UndeployAll();
            }
        }

        internal class EPLInsertIntoTransposeSingleColumnInsert : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // with transpose and same input and output
                var stmtTextOne =
                    "@Name('s0') insert into SupportBean select transpose(customOne('O' || TheString, 10)) from SupportBean(TheString like 'I%')";
                env.CompileDeploy(stmtTextOne).AddListener("s0");
                Assert.AreEqual(typeof(SupportBean), env.Statement("s0").EventType.UnderlyingType);

                env.SendEventBean(new SupportBean("I1", 1));
                var resultOne = env.Listener("s0").AssertOneGetNewAndReset();
                EPAssertionUtil.AssertProps(
                    resultOne,
                    "theString,IntPrimitive".SplitCsv(),
                    new object[] {"OI1", 10});
                Assert.AreEqual("OI1", ((SupportBean) resultOne.Underlying).TheString);
                env.UndeployModuleContaining("s0");

                // with transpose but different input and output (also test ignore column name)
                var stmtTextTwo =
                    "@Name('s0') insert into SupportBeanNumeric select transpose(customTwo(IntPrimitive, IntPrimitive+1)) as col1 from SupportBean(TheString like 'I%')";
                env.CompileDeploy(stmtTextTwo).AddListener("s0");
                Assert.AreEqual(typeof(SupportBeanNumeric), env.Statement("s0").EventType.UnderlyingType);

                env.SendEventBean(new SupportBean("I2", 10));
                var resultTwo = env.Listener("s0").AssertOneGetNewAndReset();
                EPAssertionUtil.AssertProps(
                    resultTwo,
                    "intOne,intTwo".SplitCsv(),
                    new object[] {10, 11});
                Assert.AreEqual(11, (int) ((SupportBeanNumeric) resultTwo.Underlying).IntTwo);
                env.UndeployModuleContaining("s0");

                // invalid wrong-bean target
                TryInvalidCompile(
                    env,
                    "insert into SupportBeanNumeric select transpose(customOne('O', 10)) from SupportBean",
                    "Expression-returned value of type '" +
                    typeof(SupportBean).Name +
                    "' cannot be converted to target event type 'SupportBeanNumeric' with underlying type '" +
                    typeof(SupportBeanNumeric).Name +
                    "' [insert into SupportBeanNumeric select transpose(customOne('O', 10)) from SupportBean]");

                // invalid additional properties
                TryInvalidCompile(
                    env,
                    "insert into SupportBean select 1 as dummy, transpose(customOne('O', 10)) from SupportBean",
                    "Cannot transpose additional properties in the select-clause to target event type 'SupportBean' with underlying type '" +
                    typeof(SupportBean).Name +
                    "', the transpose function must occur alone in the select clause [insert into SupportBean select 1 as dummy, transpose(customOne('O', 10)) from SupportBean]");

                // invalid occurs twice
                TryInvalidCompile(
                    env,
                    "insert into SupportBean select transpose(customOne('O', 10)), transpose(customOne('O', 11)) from SupportBean",
                    "A column name must be supplied for all but one stream if multiple streams are selected via the stream.* notation");

                // invalid wrong-type target
                try {
                    var path = new RegressionPath();
                    env.CompileDeploy("create map schema SomeOtherStream()", path);
                    env.CompileWCheckedEx(
                        "insert into SomeOtherStream select transpose(customOne('O', 10)) from SupportBean",
                        path);
                    Assert.Fail();
                }
                catch (EPCompileException ex) {
                    Assert.AreEqual(
                        "Expression-returned value of type '" +
                        typeof(SupportBean).Name +
                        "' cannot be converted to target event type 'SomeOtherStream' with underlying type 'java.util.Map' [insert into SomeOtherStream select transpose(customOne('O', 10)) from SupportBean]",
                        ex.Message);
                }

                env.UndeployAll();

                // invalid two parameters
                TryInvalidCompile(
                    env,
                    "select transpose(customOne('O', 10), customOne('O', 10)) from SupportBean",
                    "Failed to validate select-clause expression 'transpose(customOne(\"O\",10),customO...(46 chars)': The transpose function requires a single parameter expression [select transpose(customOne('O', 10), customOne('O', 10)) from SupportBean]");

                // test not a top-level function or used in where-clause (possible but not useful)
                env.CompileDeploy("select * from SupportBean where transpose(customOne('O', 10)) is not null");
                env.CompileDeploy("select transpose(customOne('O', 10)) is not null from SupportBean");

                // invalid insert of object-array into undefined stream
                TryInvalidCompile(
                    env,
                    "insert into SomeOther select transpose(generateOA('a', 1)) from SupportBean",
                    "Invalid expression return type '[LSystem.Object;' for transpose function [insert into SomeOther select transpose(generateOA('a', 1)) from SupportBean]");

                env.UndeployAll();
            }
        }

        internal class EPLInsertIntoTransposeEventJoinMap : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtTextOne =
                    "insert into MyStreamTE select a, b from AEventTE#keepall as a, BEventTE#keepall as b";
                env.CompileDeploy(stmtTextOne, path);

                var stmtTextTwo = "@Name('s0') select a.Id, b.Id from MyStreamTE";
                env.CompileDeploy(stmtTextTwo, path).AddListener("s0");

                var eventOne = MakeMap(new[] {new object[] {"Id", "A1"}});
                var eventTwo = MakeMap(new[] {new object[] {"Id", "B1"}});
                env.SendEventMap(eventOne, "AEventTE");
                env.SendEventMap(eventTwo, "BEventTE");

                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "a.Id,b.Id".SplitCsv(),
                    new object[] {"A1", "B1"});

                env.UndeployAll();
            }
        }

        internal class EPLInsertIntoTransposeEventJoinPONO : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtTextOne =
                    "insert into MyStream2Bean select a.* as a, b.* as b from SupportBean_A#keepall as a, SupportBean_B#keepall as b";
                env.CompileDeploy(stmtTextOne, path);

                var stmtTextTwo = "@Name('s0') select a.Id, b.Id from MyStream2Bean";
                env.CompileDeploy(stmtTextTwo, path).AddListener("s0");

                env.SendEventBean(new SupportBean_A("A1"));
                env.SendEventBean(new SupportBean_B("B1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "a.Id,b.Id".SplitCsv(),
                    new object[] {"A1", "B1"});

                env.UndeployAll();
            }
        }

        internal class EPLInsertIntoTransposePONOPropertyStream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtTextOne = "insert into MyStreamComplex select nested as inneritem from SupportBeanComplexProps";
                env.CompileDeploy(stmtTextOne, path);

                var stmtTextTwo = "@Name('s0') select inneritem.NestedValue as result from MyStreamComplex";
                env.CompileDeploy(stmtTextTwo, path).AddListener("s0");

                env.SendEventBean(SupportBeanComplexProps.MakeDefaultBean());
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "result".SplitCsv(),
                    new object[] {"NestedValue"});

                env.UndeployAll();
            }
        }

        internal class EPLInsertIntoInvalidTranspose : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtTextOne = "insert into MyStreamComplexMap select nested as inneritem from ComplexMap";
                env.CompileDeploy(stmtTextOne, path);

                TryInvalidCompile(
                    env,
                    path,
                    "select inneritem.NestedValue as result from MyStreamComplexMap",
                    "Failed to validate select-clause expression 'inneritem.NestedValue': Failed to resolve property 'inneritem.NestedValue' (property 'inneritem' is a mapped property and requires keyed access) [select inneritem.NestedValue as result from MyStreamComplexMap]");

                // test invalid unwrap-properties
                TryInvalidCompile(
                    env,
                    "create schema E1 as " +
                    typeof(E1).FullName +
                    ";\n" +
                    "create schema E2 as " +
                    typeof(E2).FullName +
                    ";\n" +
                    "create schema EnrichedE2 as " +
                    typeof(EnrichedE2).FullName +
                    ";\n" +
                    "insert into EnrichedE2 " +
                    "select e2.* as event, e1.otherId as playerId " +
                    "from E1#length(20) as e1, E2#length(1) as e2 " +
                    "where e1.Id = e2.Id ",
                    "The 'e2.* as event' syntax is not allowed when inserting into an existing bean event type, use the 'e2 as event' syntax instead");

                env.UndeployAll();
            }
        }

        [Serializable]
        public class E1
        {
            public E1(
                string id,
                string otherId)
            {
                Id = id;
                OtherId = otherId;
            }

            public string Id { get; }

            public string OtherId { get; }
        }

        [Serializable]
        public class E2
        {
            public E2(
                string id,
                string value)
            {
                Id = id;
                Value = value;
            }

            public string Id { get; }

            public string Value { get; }
        }

        [Serializable]
        public class EnrichedE2
        {
            public EnrichedE2(
                E2 @event,
                string playerId)
            {
                Event = @event;
                OtherId = playerId;
            }

            public string OtherId { get; }

            public E2 Event { get; }
        }
    }
} // end of namespace