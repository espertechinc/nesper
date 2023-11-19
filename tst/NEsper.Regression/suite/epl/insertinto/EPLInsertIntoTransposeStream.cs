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
using com.espertech.esper.compiler.client;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NEsper.Avro.Extensions;

using Newtonsoft.Json.Linq;

using NUnit.Framework;

using static NEsper.Avro.Extensions.TypeBuilder;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;
using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;

namespace com.espertech.esper.regressionlib.suite.epl.insertinto
{
    public class EPLInsertIntoTransposeStream
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithTransposeCreateSchemaPONO(execs);
            WithTransposeMapAndObjectArrayAndOthers(execs);
            WithTransposeFunctionToStreamWithProps(execs);
            WithTransposeFunctionToStream(execs);
            WithTransposeSingleColumnInsert(execs);
            WithTransposeSingleColumnInsertInvalid(execs);
            WithTransposeEventJoinMap(execs);
            WithTransposeEventJoinPONO(execs);
            WithTransposePONOPropertyStream(execs);
            WithInvalidTranspose(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInvalidTranspose(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoInvalidTranspose());
            return execs;
        }

        public static IList<RegressionExecution> WithTransposePONOPropertyStream(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoTransposePONOPropertyStream());
            return execs;
        }

        public static IList<RegressionExecution> WithTransposeEventJoinPONO(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoTransposeEventJoinPONO());
            return execs;
        }

        public static IList<RegressionExecution> WithTransposeEventJoinMap(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoTransposeEventJoinMap());
            return execs;
        }

        public static IList<RegressionExecution> WithTransposeSingleColumnInsertInvalid(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoTransposeSingleColumnInsertInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithTransposeSingleColumnInsert(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoTransposeSingleColumnInsert());
            return execs;
        }

        public static IList<RegressionExecution> WithTransposeFunctionToStream(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoTransposeFunctionToStream());
            return execs;
        }

        public static IList<RegressionExecution> WithTransposeFunctionToStreamWithProps(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoTransposeFunctionToStreamWithProps());
            return execs;
        }

        public static IList<RegressionExecution> WithTransposeMapAndObjectArrayAndOthers(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoTransposeMapAndObjectArrayAndOthers());
            return execs;
        }

        public static IList<RegressionExecution> WithTransposeCreateSchemaPONO(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoTransposeCreateSchemaPONO());
            return execs;
        }

        private class EPLInsertIntoTransposeCreateSchemaPONO : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create schema SupportBeanTwo as " +
                          typeof(SupportBeanTwo).FullName +
                          ";\n" +
                          "on SupportBean event insert into astream select transpose(" +
                          typeof(EPLInsertIntoTransposeStream).FullName +
                          ".MakeSB2Event(event));\n" +
                          "on SupportBean event insert into bstream select transpose(" +
                          typeof(EPLInsertIntoTransposeStream).FullName +
                          ".MakeSB2Event(event));\n" +
                          "@name('a') select * from astream\n;" +
                          "@name('b') select * from bstream\n;";
                env.CompileDeploy(epl).AddListener("a").AddListener("b");

                env.SendEventBean(new SupportBean("E1", 1));

                var fields = new string[] { "stringTwo" };
                env.AssertPropsNew("a", fields, new object[] { "E1" });
                env.AssertPropsNew("b", fields, new object[] { "E1" });

                env.UndeployAll();
            }
        }

        private class EPLInsertIntoTransposeMapAndObjectArrayAndOthers : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
                    RunTransposeMapAndObjectArray(env, rep);
                }
            }

            private static void RunTransposeMapAndObjectArray(
                RegressionEnvironment env,
                EventRepresentationChoice representation)
            {
                var fields = "p0,p1".SplitCsv();
                var path = new RegressionPath();
                var schema = representation.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedMySchema)) +
                             "@public @buseventtype @public create schema MySchema(p0 string, p1 int)";
                env.CompileDeploy(schema, path);

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
                else if (representation.IsJsonEvent() || representation.IsJsonProvidedClassEvent()) {
                    generateFunction = "generateJson";
                }
                else {
                    throw new IllegalStateException("Unrecognized code " + representation);
                }

                var epl = "insert into MySchema select transpose(" +
                          generateFunction +
                          "(TheString, IntPrimitive)) from SupportBean";
                env.CompileDeploy("@name('s0') " + epl, path).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertPropsNew("s0", fields, new object[] { "E1", 1 });

                env.SendEventBean(new SupportBean("E2", 2));
                env.AssertPropsNew("s0", fields, new object[] { "E2", 2 });

                // MySchema already exists, start second statement
                env.CompileDeploy("@name('s1') " + epl, path).AddListener("s1");
                env.UndeployModuleContaining("s0");

                env.SendEventBean(new SupportBean("E3", 3));
                env.AssertPropsNew("s1", fields, new object[] { "E3", 3 });

                env.UndeployAll();
            }
        }

        private class EPLInsertIntoTransposeFunctionToStreamWithProps : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtTextOne =
                    "@public insert into MyStream select 1 as dummy, transpose(custom('O' || TheString, 10)) from SupportBean(TheString like 'I%')";
                env.CompileDeploy(stmtTextOne, path);

                var stmtTextTwo = "@name('s0') select * from MyStream";
                env.CompileDeploy(stmtTextTwo, path).AddListener("s0");

                env.AssertStatement(
                    "s0",
                    statement => Assert.AreEqual(
                        typeof(Pair<object, IDictionary<string, object>>),
                        statement.EventType.UnderlyingType));

                env.SendEventBean(new SupportBean("I1", 1));
                env.AssertEventNew(
                    "s0",
                    result => {
                        var underlying = (Pair<object, IDictionary<string, object>>)result.Underlying;
                        EPAssertionUtil.AssertProps(
                            result,
                            "dummy,TheString,IntPrimitive".SplitCsv(),
                            new object[] { 1, "OI1", 10 });
                        Assert.AreEqual("OI1", ((SupportBean)underlying.First).TheString);
                    });

                env.UndeployAll();
            }
        }

        private class EPLInsertIntoTransposeFunctionToStream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtTextOne =
                    "insert into OtherStream select transpose(custom('O' || TheString, 10)) from SupportBean(TheString like 'I%')";
                env.CompileDeploy("@name('first') @public " + stmtTextOne, path).AddListener("first");

                var stmtTextTwo = "@name('s0') select * from OtherStream(TheString like 'O%')";
                env.CompileDeploy(stmtTextTwo, path).AddListener("s0");

                env.AssertStatement(
                    "s0",
                    statement => Assert.AreEqual(typeof(SupportBean), statement.EventType.UnderlyingType));

                env.SendEventBean(new SupportBean("I1", 1));
                env.AssertEventNew(
                    "s0",
                    result => {
                        EPAssertionUtil.AssertProps(
                            result,
                            "TheString,IntPrimitive".SplitCsv(),
                            new object[] { "OI1", 10 });
                        Assert.AreEqual("OI1", ((SupportBean)result.Underlying).TheString);
                    });

                // try second statement as "OtherStream" now already exists
                env.CompileDeploy("@name('second') " + stmtTextOne).AddListener("second");
                env.UndeployModuleContaining("s0");
                env.SendEventBean(new SupportBean("I2", 2));
                env.AssertPropsNew("second", "TheString,IntPrimitive".SplitCsv(), new object[] { "OI2", 10 });

                env.UndeployAll();
            }
        }

        private class EPLInsertIntoTransposeSingleColumnInsert : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // with transpose and same input and output
                var stmtTextOne =
                    "@name('s0') insert into SupportBean select transpose(customOne('O' || TheString, 10)) from SupportBean(TheString like 'I%')";
                env.CompileDeploy(stmtTextOne).AddListener("s0");
                env.AssertStatement(
                    "s0",
                    statement => Assert.AreEqual(typeof(SupportBean), statement.EventType.UnderlyingType));

                env.SendEventBean(new SupportBean("I1", 1));
                env.AssertEventNew(
                    "s0",
                    resultOne => {
                        EPAssertionUtil.AssertProps(
                            resultOne,
                            "TheString,IntPrimitive".SplitCsv(),
                            new object[] { "OI1", 10 });
                        Assert.AreEqual("OI1", ((SupportBean)resultOne.Underlying).TheString);
                    });
                env.UndeployModuleContaining("s0");

                // with transpose but different input and output (also test ignore column name)
                var stmtTextTwo =
                    "@name('s0') insert into SupportBeanNumeric select transpose(customTwo(IntPrimitive, IntPrimitive+1)) as col1 from SupportBean(TheString like 'I%')";
                env.CompileDeploy(stmtTextTwo).AddListener("s0");
                env.AssertStatement(
                    "s0",
                    statement => Assert.AreEqual(typeof(SupportBeanNumeric), statement.EventType.UnderlyingType));

                env.SendEventBean(new SupportBean("I2", 10));
                env.AssertEventNew(
                    "s0",
                    resultTwo => {
                        EPAssertionUtil.AssertProps(resultTwo, "intOne,intTwo".SplitCsv(), new object[] { 10, 11 });
                        Assert.AreEqual(11, (int)((SupportBeanNumeric)resultTwo.Underlying).IntTwo);
                    });

                env.UndeployAll();
            }
        }

        private class EPLInsertIntoTransposeSingleColumnInsertInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // invalid wrong-bean target
                env.TryInvalidCompile(
                    "insert into SupportBeanNumeric select transpose(customOne('O', 10)) from SupportBean",
                    "Expression-returned value of type '" +
                    typeof(SupportBean).FullName +
                    "' cannot be converted to target event type 'SupportBeanNumeric' with underlying type '" +
                    typeof(SupportBeanNumeric).FullName +
                    "' [insert into SupportBeanNumeric select transpose(customOne('O', 10)) from SupportBean]");

                // invalid additional properties
                env.TryInvalidCompile(
                    "insert into SupportBean select 1 as dummy, transpose(customOne('O', 10)) from SupportBean",
                    "Cannot transpose additional properties in the select-clause to target event type 'SupportBean' with underlying type '" +
                    typeof(SupportBean).FullName +
                    "', the transpose function must occur alone in the select clause [insert into SupportBean select 1 as dummy, transpose(customOne('O', 10)) from SupportBean]");

                // invalid occurs twice
                env.TryInvalidCompile(
                    "insert into SupportBean select transpose(customOne('O', 10)), transpose(customOne('O', 11)) from SupportBean",
                    "A column name must be supplied for all but one stream if multiple streams are selected via the stream.* notation");

                // invalid wrong-type target
                try {
                    var path = new RegressionPath();
                    env.CompileDeploy("@public create map schema SomeOtherStream()", path);
                    env.CompileWCheckedEx(
                        "insert into SomeOtherStream select transpose(customOne('O', 10)) from SupportBean",
                        path);
                    Assert.Fail();
                }
                catch (EPCompileException ex) {
                    Assert.AreEqual(
                        "Expression-returned value of type '" +
                        typeof(SupportBean).FullName +
                        "' cannot be converted to target event type 'SomeOtherStream' with underlying type 'System.Collections.Generic.IDictionary' [insert into SomeOtherStream select transpose(customOne('O', 10)) from SupportBean]",
                        ex.Message);
                }

                env.UndeployAll();

                // invalid two parameters
                env.TryInvalidCompile(
                    "select transpose(customOne('O', 10), customOne('O', 10)) from SupportBean",
                    "Failed to validate select-clause expression 'transpose(customOne(\"O\",10),customO...(46 chars)': The transpose function requires a single parameter expression [select transpose(customOne('O', 10), customOne('O', 10)) from SupportBean]");

                // test not a top-level function or used in where-clause (possible but not useful)
                env.CompileDeploy("select * from SupportBean where transpose(customOne('O', 10)) is not null");
                env.CompileDeploy("select transpose(customOne('O', 10)) is not null from SupportBean");

                // invalid insert of object-array into undefined stream
                env.TryInvalidCompile(
                    "insert into SomeOther select transpose(generateOA('a', 1)) from SupportBean",
                    "Invalid expression return type 'Object[]' for transpose function [insert into SomeOther select transpose(generateOA('a', 1)) from SupportBean]");

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.INVALIDITY);
            }
        }

        private class EPLInsertIntoTransposeEventJoinMap : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtTextOne =
                    "@public insert into MyStreamTE select a, b from AEventTE#keepall as a, BEventTE#keepall as b";
                env.CompileDeploy(stmtTextOne, path);

                var stmtTextTwo = "@name('s0') select a.Id, b.Id from MyStreamTE";
                env.CompileDeploy(stmtTextTwo, path).AddListener("s0");

                var eventOne = MakeMap(new object[][] { new object[] { "Id", "A1" } });
                var eventTwo = MakeMap(new object[][] { new object[] { "Id", "B1" } });
                env.SendEventMap(eventOne, "AEventTE");
                env.SendEventMap(eventTwo, "BEventTE");

                env.AssertPropsNew("s0", "a.Id,b.Id".SplitCsv(), new object[] { "A1", "B1" });

                env.UndeployAll();
            }
        }

        private class EPLInsertIntoTransposeEventJoinPONO : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtTextOne =
                    "@public insert into MyStream2Bean select a.* as a, b.* as b from SupportBean_A#keepall as a, SupportBean_B#keepall as b";
                env.CompileDeploy(stmtTextOne, path);

                var stmtTextTwo = "@name('s0') select a.Id, b.Id from MyStream2Bean";
                env.CompileDeploy(stmtTextTwo, path).AddListener("s0");

                env.SendEventBean(new SupportBean_A("A1"));
                env.SendEventBean(new SupportBean_B("B1"));
                env.AssertPropsNew("s0", "a.Id,b.Id".SplitCsv(), new object[] { "A1", "B1" });

                env.UndeployAll();
            }
        }

        private class EPLInsertIntoTransposePONOPropertyStream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtTextOne =
                    "@public insert into MyStreamComplex select nested as inneritem from SupportBeanComplexProps";
                env.CompileDeploy(stmtTextOne, path);

                var stmtTextTwo = "@name('s0') select inneritem.NestedValue as result from MyStreamComplex";
                env.CompileDeploy(stmtTextTwo, path).AddListener("s0");

                env.SendEventBean(SupportBeanComplexProps.MakeDefaultBean());
                env.AssertPropsNew("s0", "result".SplitCsv(), new object[] { "NestedValue" });

                env.UndeployAll();
            }
        }

        private class EPLInsertIntoInvalidTranspose : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtTextOne = "@public insert into MyStreamComplexMap select nested as inneritem from ComplexMap";
                env.CompileDeploy(stmtTextOne, path);

                env.TryInvalidCompile(
                    path,
                    "select inneritem.NestedValue as result from MyStreamComplexMap",
                    "Failed to validate select-clause expression 'inneritem.NestedValue': Failed to resolve property 'inneritem.NestedValue' (property 'inneritem' is a mapped property and requires keyed access) [select inneritem.NestedValue as result from MyStreamComplexMap]");

                // test invalid unwrap-properties
                env.TryInvalidCompile(
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

                env.TryInvalidCompile(
                    "select transpose(null) from SupportBean",
                    "Invalid expression return type 'null' for transpose function");

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.INVALIDITY);
            }
        }

        public static IDictionary<string, object> LocalGenerateMap(
            string @string,
            int intPrimitive)
        {
            IDictionary<string, object> @out = new Dictionary<string, object>();
            @out.Put("p0", @string);
            @out.Put("p1", intPrimitive);
            return @out;
        }

        public static object[] LocalGenerateOA(
            string @string,
            int intPrimitive)
        {
            return new object[] { @string, intPrimitive };
        }

        public static SupportBeanTwo MakeSB2Event(SupportBean sb)
        {
            return new SupportBeanTwo(sb.TheString, sb.IntPrimitive);
        }

        public static GenericRecord LocalGenerateAvro(
            string @string,
            int intPrimitive)
        {
            var schema = SchemaBuilder.Record("name", RequiredString("p0"), RequiredInt("p1"));
            var record = new GenericRecord(schema);
            record.Put("p0", @string);
            record.Put("p1", intPrimitive);
            return record;
        }

        public static string LocalGenerateJson(
            string @string,
            int intPrimitive)
        {
            var @object = new JObject(
                new JProperty("p0", @string),
                new JProperty("p1", intPrimitive));
            return @object.ToString();
        }

        private static IDictionary<string, object> MakeMap(object[][] entries)
        {
            var result = new Dictionary<string, object>();
            foreach (var entry in entries) {
                result.Put((string)entry[0], entry[1]);
            }

            return result;
        }

        public class E1
        {
            private readonly string id;
            private readonly string otherId;

            public E1(
                string id,
                string otherId)
            {
                this.id = id;
                this.otherId = otherId;
            }

            public string Id => id;

            public string OtherId => otherId;
        }

        public class E2
        {
            private readonly string id;
            private readonly string value;

            public E2(
                string id,
                string value)
            {
                this.id = id;
                this.value = value;
            }

            public string Id => id;

            public string Value => value;
        }

        public class EnrichedE2
        {
            private readonly E2 @event;
            private readonly string otherId;

            public EnrichedE2(
                E2 @event,
                string playerId)
            {
                this.@event = @event;
                this.otherId = playerId;
            }

            public E2 Event => @event;

            public string OtherId => otherId;
        }

        public class MyLocalJsonProvidedMySchema
        {
            public string p0;
            public int p1;
        }
    }
} // end of namespace