///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using Newtonsoft.Json.Linq;

using static com.espertech.esper.regressionlib.support.json.SupportJsonEventTypeUtil;

namespace com.espertech.esper.regressionlib.suite.@event.json
{
    public class EventJsonTypingCoreWrite
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithWriteBasicType(execs);
            WithWriteBasicTypeArray(execs);
            WithWriteBasicTypeArray2Dim(execs);
            WithWriteEnumType(execs);
            WithWriteDecimalBigInt(execs);
            WithWriteObjectType(execs);
            WithWriteObjectArrayType(execs);
            WithWriteMapType(execs);
            WithParseDynamicPropJsonTypes(execs);
            WithWriteDynamicPropMixedOjectArray(execs);
            WithWriteDynamicPropNestedArray(execs);
            WithWriteDynamicPropNumberFormat(execs);
            WithWriteNested(execs);
            WithWriteNestedArray(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithWriteNestedArray(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonTypingWriteNestedArray());
            return execs;
        }

        public static IList<RegressionExecution> WithWriteNested(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonTypingWriteNested());
            return execs;
        }

        public static IList<RegressionExecution> WithWriteDynamicPropNumberFormat(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonTypingWriteDynamicPropNumberFormat());
            return execs;
        }

        public static IList<RegressionExecution> WithWriteDynamicPropNestedArray(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonTypingWriteDynamicPropNestedArray());
            return execs;
        }

        public static IList<RegressionExecution> WithWriteDynamicPropMixedOjectArray(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonTypingWriteDynamicPropMixedOjectArray());
            return execs;
        }

        public static IList<RegressionExecution> WithParseDynamicPropJsonTypes(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonTypingParseDynamicPropJsonTypes());
            return execs;
        }

        public static IList<RegressionExecution> WithWriteMapType(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonTypingWriteMapType());
            return execs;
        }

        public static IList<RegressionExecution> WithWriteObjectArrayType(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonTypingWriteObjectArrayType());
            return execs;
        }

        public static IList<RegressionExecution> WithWriteObjectType(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonTypingWriteObjectType());
            return execs;
        }

        public static IList<RegressionExecution> WithWriteDecimalBigInt(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonTypingWriteDecimalBigInt());
            return execs;
        }

        public static IList<RegressionExecution> WithWriteEnumType(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonTypingWriteEnumType());
            return execs;
        }

        public static IList<RegressionExecution> WithWriteBasicTypeArray2Dim(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonTypingWriteBasicTypeArray2Dim());
            return execs;
        }

        public static IList<RegressionExecution> WithWriteBasicTypeArray(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonTypingWriteBasicTypeArray());
            return execs;
        }

        public static IList<RegressionExecution> WithWriteBasicType(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonTypingWriteBasicType());
            return execs;
        }

        private class EventJsonTypingWriteNested : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public create json schema Book(BookId string, Price decimal);\n", path);
                env.CompileDeploy("@public create json schema Shelf(shelfId string, Book Book);\n", path);
                env.CompileDeploy("@public create json schema Isle(isleId string, shelf Shelf);\n", path);
                env.CompileDeploy(
                    "@public @buseventtype create json schema Library(libraryId string, isle Isle);\n",
                    path);
                env.CompileDeploy("@name('s0') select * from Library;\n", path).AddListener("s0");
                string json;

                json = "{\n" +
                       "  \"libraryId\": \"L\",\n" +
                       "  \"isle\": {\n" +
                       "    \"isleId\": \"I1\",\n" +
                       "    \"shelf\": {\n" +
                       "      \"shelfId\": \"S11\",\n" +
                       "      \"Book\": {\n" +
                       "        \"BookId\": \"B111\",\n" +
                       "        \"Price\": 20\n" +
                       "      }\n" +
                       "    }\n" +
                       "  }\n" +
                       "}";
                SendAssertLibrary(env, json, "L", "I1", "S11", "B111");

                json = "{\n" +
                       "  \"libraryId\": \"L\",\n" +
                       "  \"isle\": null\n" +
                       "}";
                SendAssertLibrary(env, json, "L", null, null, null);

                json = "{\n" +
                       "  \"libraryId\": \"L\",\n" +
                       "  \"isle\": {\n" +
                       "    \"isleId\": \"I1\",\n" +
                       "    \"shelf\": null\n" +
                       "  }\n" +
                       "}";
                SendAssertLibrary(env, json, "L", "I1", null, null);

                json = "{\n" +
                       "  \"libraryId\": \"L\",\n" +
                       "  \"isle\": {\n" +
                       "    \"isleId\": \"I1\",\n" +
                       "    \"shelf\": {\n" +
                       "      \"shelfId\": \"S11\",\n" +
                       "      \"Book\": null\n" +
                       "    }\n" +
                       "  }\n" +
                       "}";
                SendAssertLibrary(env, json, "L", "I1", "S11", null);

                env.UndeployAll();
            }

            private void SendAssertLibrary(
                RegressionEnvironment env,
                string json,
                string libraryId,
                string isleId,
                string shelfId,
                string bookId)
            {
                env.SendEventJson(json, "Library");
                env.AssertEventNew(
                    "s0",
                    @event => {
                        AssertJsonWrite(json, @event);
                        EPAssertionUtil.AssertProps(
                            @event,
                            "libraryId,isle.isleId,isle.shelf.shelfId,isle.shelf.Book.BookId".SplitCsv(),
                            new object[] { libraryId, isleId, shelfId, bookId });
                    });
            }
        }

        private class EventJsonTypingWriteNestedArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public create json schema Book(BookId string, Price decimal);\n", path);
                env.CompileDeploy("@public create json schema Shelf(shelfId string, Books Book[]);\n", path);
                env.CompileDeploy("@public create json schema Isle(isleId string, shelfs Shelf[]);\n", path);
                env.CompileDeploy(
                    "@public @buseventtype create json schema Library(libraryId string, isles Isle[]);\n",
                    path);
                env.CompileDeploy("@name('s0') select * from Library#keepall;\n", path).AddListener("s0");

                var jsonOne = "{\n" +
                              "  \"libraryId\": \"L1\",\n" +
                              "  \"isles\": [\n" +
                              "    {\n" +
                              "      \"isleId\": \"I1\",\n" +
                              "      \"shelfs\": [\n" +
                              "        {\n" +
                              "          \"shelfId\": \"S1\",\n" +
                              "          \"Books\": [\n" +
                              "            {\n" +
                              "              \"BookId\": \"B1\",\n" +
                              "              \"Price\": 10\n" +
                              "            }\n" +
                              "          ]\n" +
                              "        }\n" +
                              "      ]\n" +
                              "    }\n" +
                              "  ]\n" +
                              "}";
                env.SendEventJson(jsonOne, "Library");
                env.AssertEventNew("s0", @event => AssertJsonWrite(jsonOne, @event));

                var book111 = BuildBook("B111", 20);
                var shelf11 = BuildShelf("S11", book111);
                var isle1 = BuildIsle("I1", shelf11);
                var libraryOne = BuildLibrary("L1", isle1);
                var jsonTwo = libraryOne.ToString();
                env.SendEventJson(jsonTwo, "Library");
                env.AssertEventNew("s0", @event => AssertJsonWrite(jsonTwo, @event));

                var book112 = BuildBook("B112", 21);
                ((JArray)shelf11.Get("Books")).Add(book112);
                var shelf12 = BuildShelf("S12", book111, book112);
                var isle2 = BuildIsle("I2", shelf11, shelf12);
                var libraryTwo = BuildLibrary("L", isle1, isle2);
                var jsonThree = libraryTwo.ToString();
                env.SendEventJson(jsonThree, "Library");
                env.AssertEventNew("s0", @event => AssertJsonWrite(jsonThree, @event));

                env.Milestone(0);

                env.AssertIterator(
                    "s0",
                    en => {
                        AssertJsonWrite(jsonOne, en.Advance());
                        AssertJsonWrite(jsonTwo, en.Advance());
                        AssertJsonWrite(jsonThree, en.Advance());
                    });

                env.UndeployAll();
            }
        }

        private class EventJsonTypingWriteBasicType : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@public @buseventtype create json schema JsonEvent (" +
                          "c0 string, c1 char, c2 character, c3 bool, c4 boolean, " +
                          "c5 byte, c6 short, c7 int, c8 integer, c9 long, c10 double, c11 float, c12 null);\n" +
                          "@name('s0') select * from JsonEvent#keepall;\n";
                env.CompileDeploy(epl).AddListener("s0");

                var jsonOne = "{\n" +
                              "  \"c0\": \"abc\",\n" +
                              "  \"c1\": \"x\",\n" +
                              "  \"c2\": \"z\",\n" +
                              "  \"c3\": true,\n" +
                              "  \"c4\": false,\n" +
                              "  \"c5\": 1,\n" +
                              "  \"c6\": 10,\n" +
                              "  \"c7\": 11,\n" +
                              "  \"c8\": 12,\n" +
                              "  \"c9\": 13,\n" +
                              "  \"c10\": 14.0,\n" +
                              "  \"c11\": 1500.0,\n" +
                              "  \"c12\": null\n" +
                              "}";
                env.SendEventJson(jsonOne, "JsonEvent");
                env.AssertEventNew("s0", @event => AssertJsonWrite(jsonOne, @event));

                var jsonTwo = "{\n" +
                              "  \"c0\": null,\n" +
                              "  \"c1\": null,\n" +
                              "  \"c2\": null,\n" +
                              "  \"c3\": null,\n" +
                              "  \"c4\": null,\n" +
                              "  \"c5\": null,\n" +
                              "  \"c6\": null,\n" +
                              "  \"c7\": null,\n" +
                              "  \"c8\": null,\n" +
                              "  \"c9\": null,\n" +
                              "  \"c10\": null,\n" +
                              "  \"c11\": null,\n" +
                              "  \"c12\": null\n" +
                              "}";
                env.SendEventJson(jsonTwo, "JsonEvent");
                env.AssertEventNew("s0", @event => AssertJsonWrite(jsonTwo, @event));

                env.Milestone(0);

                env.AssertIterator(
                    "s0",
                    it => {
                        AssertJsonWrite(jsonOne, it.Advance());
                        AssertJsonWrite(jsonTwo, it.Advance());
                    });

                env.UndeployAll();
            }
        }

        private class EventJsonTypingWriteBasicTypeArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@public @buseventtype create json schema JsonEvent (" +
                          "c0 string[], " +
                          "c1 char[], c2 char[primitive], " +
                          "c3 bool[], c4 boolean[primitive], " +
                          "c5 byte[], c6 byte[primitive], " +
                          "c7 short[], c8 short[primitive], " +
                          "c9 int[], c10 int[primitive], " +
                          "c11 long[], c12 long[primitive], " +
                          "c13 double[], c14 double[primitive], " +
                          "c15 float[], c16 float[primitive]);\n" +
                          "@name('s0') select * from JsonEvent#keepall;\n";
                env.CompileDeploy(epl).AddListener("s0");

                var jsonOne = "{ \"c0\": [\"abc\", \"def\"],\n" +
                              "\"c1\": [\"x\", \"z\"],\n" +
                              "\"c2\": [\"x\", \"y\"],\n" +
                              "\"c3\": [true, false],\n" +
                              "\"c4\": [false, true],\n" +
                              "\"c5\": [10, 11],\n" +
                              "\"c6\": [12, 13],\n" +
                              "\"c7\": [20, 21],\n" +
                              "\"c8\": [22, 23],\n" +
                              "\"c9\": [30, 31],\n" +
                              "\"c10\": [32, 33],\n" +
                              "\"c11\": [40, 41],\n" +
                              "\"c12\": [42, 43],\n" +
                              "\"c13\": [50.0, 51.0],\n" +
                              "\"c14\": [52.0, 53.0],\n" +
                              "\"c15\": [60.0, 61.0],\n" +
                              "\"c16\": [62.0, 63.0]" +
                              "}\n";
                env.SendEventJson(jsonOne, "JsonEvent");
                env.AssertEventNew("s0", @event => AssertJsonWrite(jsonOne, @event));

                var jsonTwo = "{ \"c0\": [],\n" +
                              "\"c1\": [],\n" +
                              "\"c2\": [],\n" +
                              "\"c3\": [],\n" +
                              "\"c4\": [],\n" +
                              "\"c5\": [],\n" +
                              "\"c6\": [],\n" +
                              "\"c7\": [],\n" +
                              "\"c8\": [],\n" +
                              "\"c9\": [],\n" +
                              "\"c10\": [],\n" +
                              "\"c11\": [],\n" +
                              "\"c12\": [],\n" +
                              "\"c13\": [],\n" +
                              "\"c14\": [],\n" +
                              "\"c15\": [],\n" +
                              "\"c16\": []" +
                              "}\n";
                env.SendEventJson(jsonTwo, "JsonEvent");
                env.AssertEventNew("s0", @event => AssertJsonWrite(jsonTwo, @event));

                var jsonThree = "{ \"c0\": null,\n" +
                                "\"c1\": null,\n" +
                                "\"c2\": null,\n" +
                                "\"c3\": null,\n" +
                                "\"c4\": null,\n" +
                                "\"c5\": null,\n" +
                                "\"c6\": null,\n" +
                                "\"c7\": null,\n" +
                                "\"c8\": null,\n" +
                                "\"c9\": null,\n" +
                                "\"c10\": null,\n" +
                                "\"c11\": null,\n" +
                                "\"c12\": null,\n" +
                                "\"c13\": null,\n" +
                                "\"c14\": null,\n" +
                                "\"c15\": null,\n" +
                                "\"c16\": null" +
                                "}\n";
                env.SendEventJson(jsonThree, "JsonEvent");
                env.AssertEventNew("s0", @event => AssertJsonWrite(jsonThree, @event));

                var jsonFour = "{ \"c0\": [null, \"def\", null],\n" +
                               "\"c1\": [\"x\", null],\n" +
                               "\"c2\": [\"x\"],\n" +
                               "\"c3\": [true, null, false],\n" +
                               "\"c4\": [true],\n" +
                               "\"c5\": [null, null, null],\n" +
                               "\"c6\": [12],\n" +
                               "\"c7\": [20, 21, null],\n" +
                               "\"c8\": [23],\n" +
                               "\"c9\": [null, 30, null, 31, null, 32],\n" +
                               "\"c10\": [32],\n" +
                               "\"c11\": [null, 40, 41, null],\n" +
                               "\"c12\": [42],\n" +
                               "\"c13\": [null, null, 51.0],\n" +
                               "\"c14\": [52.0],\n" +
                               "\"c15\": [null],\n" +
                               "\"c16\": [63.0]" +
                               "}\n";
                env.SendEventJson(jsonFour, "JsonEvent");
                env.AssertEventNew("s0", @event => AssertJsonWrite(jsonFour, @event));

                env.Milestone(0);

                env.AssertIterator(
                    "s0",
                    it => {
                        AssertJsonWrite(jsonOne, it.Advance());
                        AssertJsonWrite(jsonTwo, it.Advance());
                        AssertJsonWrite(jsonThree, it.Advance());
                        AssertJsonWrite(jsonFour, it.Advance());
                    });

                env.UndeployAll();
            }
        }

        private class EventJsonTypingWriteBasicTypeArray2Dim : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@public @buseventtype create json schema JsonEvent (" +
                          "c0 string[][], " +
                          "c1 char[][], c2 char[primitive][], " +
                          "c3 bool[][], c4 boolean[primitive][], " +
                          "c5 byte[][], c6 byte[primitive][], " +
                          "c7 short[][], c8 short[primitive][], " +
                          "c9 int[][], c10 int[primitive][], " +
                          "c11 long[][], c12 long[primitive][], " +
                          "c13 double[][], c14 double[primitive][], " +
                          "c15 float[][], c16 float[primitive][]);\n" +
                          "@name('s0') select * from JsonEvent#keepall;\n";
                env.CompileDeploy(epl).AddListener("s0");

                var jsonOne = "{ \"c0\": [[\"a\", \"b\"],[\"c\"]],\n" +
                              "\"c1\": [[\"x\", \"z\"],[\"n\"]],\n" +
                              "\"c2\": [[\"x\"], [\"y\", \"z\"]],\n" +
                              "\"c3\": [[], [true, false], []],\n" +
                              "\"c4\": [[false, true]],\n" +
                              "\"c5\": [[10], [11]],\n" +
                              "\"c6\": [[12, 13]],\n" +
                              "\"c7\": [[20, 21], [22, 23]],\n" +
                              "\"c8\": [[22], [23], []],\n" +
                              "\"c9\": [[], [], [30, 31]],\n" +
                              "\"c10\": [[32], [33, 34]],\n" +
                              "\"c11\": [[40], [], [41]],\n" +
                              "\"c12\": [[42, 43], [44]],\n" +
                              "\"c13\": [[50.0], [51.0, 52.0], [53.0]],\n" +
                              "\"c14\": [[54.0], [55.0, 56.0]],\n" +
                              "\"c15\": [[60.0, 61.0], []],\n" +
                              "\"c16\": [[62.0], [63.0]]" +
                              "}\n";
                env.SendEventJson(jsonOne, "JsonEvent");
                env.AssertEventNew("s0", @event => AssertJsonWrite(jsonOne, @event));

                var jsonTwo = "{ \"c0\": [],\n" +
                              "\"c1\": [],\n" +
                              "\"c2\": [],\n" +
                              "\"c3\": [],\n" +
                              "\"c4\": [],\n" +
                              "\"c5\": [],\n" +
                              "\"c6\": [],\n" +
                              "\"c7\": [],\n" +
                              "\"c8\": [],\n" +
                              "\"c9\": [],\n" +
                              "\"c10\": [],\n" +
                              "\"c11\": [],\n" +
                              "\"c12\": [],\n" +
                              "\"c13\": [],\n" +
                              "\"c14\": [],\n" +
                              "\"c15\": [],\n" +
                              "\"c16\": []" +
                              "}\n";
                env.SendEventJson(jsonTwo, "JsonEvent");
                env.AssertEventNew("s0", @event => AssertJsonWrite(jsonTwo, @event));

                var jsonThree = "{ \"c0\": null,\n" +
                                "\"c1\": null,\n" +
                                "\"c2\": null,\n" +
                                "\"c3\": null,\n" +
                                "\"c4\": null,\n" +
                                "\"c5\": null,\n" +
                                "\"c6\": null,\n" +
                                "\"c7\": null,\n" +
                                "\"c8\": null,\n" +
                                "\"c9\": null,\n" +
                                "\"c10\": null,\n" +
                                "\"c11\": null,\n" +
                                "\"c12\": null,\n" +
                                "\"c13\": null,\n" +
                                "\"c14\": null,\n" +
                                "\"c15\": null,\n" +
                                "\"c16\": null" +
                                "}\n";
                env.SendEventJson(jsonThree, "JsonEvent");
                env.AssertEventNew("s0", @event => AssertJsonWrite(jsonThree, @event));

                var jsonFour = "{ \"c0\": [[null, \"a\"]],\n" +
                               "\"c1\": [[null], [\"x\"]],\n" +
                               "\"c2\": [null, [\"x\"]],\n" +
                               "\"c3\": [[null], [true]],\n" +
                               "\"c4\": [[true], null],\n" +
                               "\"c5\": [null, null],\n" +
                               "\"c6\": [null, [12, 13]],\n" +
                               "\"c7\": [[21], null],\n" +
                               "\"c8\": [null, [23], null],\n" +
                               "\"c9\": [[30], null, [31]],\n" +
                               "\"c10\": [[]],\n" +
                               "\"c11\": [[], []],\n" +
                               "\"c12\": [[42]],\n" +
                               "\"c13\": [null, []],\n" +
                               "\"c14\": [[], null],\n" +
                               "\"c15\": [[null]],\n" +
                               "\"c16\": [[63.0]]" +
                               "}\n";
                env.SendEventJson(jsonFour, "JsonEvent");
                env.AssertEventNew("s0", @event => AssertJsonWrite(jsonFour, @event));

                env.Milestone(0);

                env.AssertIterator(
                    "s0",
                    it => {
                        AssertJsonWrite(jsonOne, it.Advance());
                        AssertJsonWrite(jsonTwo, it.Advance());
                        AssertJsonWrite(jsonThree, it.Advance());
                        AssertJsonWrite(jsonFour, it.Advance());
                    });

                env.UndeployAll();
            }
        }

        private class EventJsonTypingWriteEnumType : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@public @buseventtype create json schema JsonEvent (c0 SupportEnum, c1 SupportEnum[], c2 SupportEnum[][]);\n" +
                    "@name('s0') select * from JsonEvent#keepall;\n";
                env.CompileDeploy(epl).AddListener("s0");

                var jsonOne =
                    "{\"c0\": \"ENUM_VALUE_2\", \"c1\": [\"ENUM_VALUE_2\", \"ENUM_VALUE_1\"], \"c2\": [[\"ENUM_VALUE_2\"], [\"ENUM_VALUE_1\", \"ENUM_VALUE_3\"]]}";
                env.SendEventJson(jsonOne, "JsonEvent");
                env.AssertEventNew("s0", @event => AssertJsonWrite(jsonOne, @event));

                var jsonTwo = "{\"c0\": null, \"c1\": null, \"c2\": null}";
                env.SendEventJson(jsonTwo, "JsonEvent");
                env.AssertEventNew("s0", @event => AssertJsonWrite(jsonTwo, @event));

                var jsonThree = "{\"c0\": null, \"c1\": [], \"c2\": [[]]}";
                env.SendEventJson(jsonThree, "JsonEvent");
                env.AssertEventNew("s0", @event => AssertJsonWrite(jsonThree, @event));

                env.Milestone(0);

                env.AssertIterator(
                    "s0",
                    it => {
                        AssertJsonWrite(jsonOne, it.Advance());
                        AssertJsonWrite(jsonTwo, it.Advance());
                        AssertJsonWrite(jsonThree, it.Advance());
                    });

                env.UndeployAll();
            }
        }

        private class EventJsonTypingWriteDecimalBigInt : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@public @buseventtype create json schema JsonEvent (c0 BigInteger, c1 decimal," +
                          "c2 BigInteger[], c3 decimal[], c4 BigInteger[][], c5 decimal[][]);\n" +
                          "@name('s0') select * from JsonEvent#keepall;\n";
                env.CompileDeploy(epl).AddListener("s0");

                var jsonOne = "{\"c0\": 123456789123456789123456789, \"c1\": 123456789123456789123456789.1," +
                              "\"c2\": [123456789123456789123456789], \"c3\": [123456789123456789123456789.1]," +
                              "\"c4\": [[123456789123456789123456789]], \"c5\": [[123456789123456789123456789.1]]" +
                              "}";
                env.SendEventJson(jsonOne, "JsonEvent");
                env.AssertEventNew("s0", @event => AssertJsonWrite(jsonOne, @event));

                var jsonTwo = "{\"c0\": null, \"c1\": null, \"c2\": null, \"c3\": null, \"c4\": null, \"c5\": null}";
                env.SendEventJson(jsonTwo, "JsonEvent");
                env.AssertEventNew("s0", @event => AssertJsonWrite(jsonTwo, @event));

                env.Milestone(0);

                env.AssertIterator(
                    "s0",
                    it => {
                        AssertJsonWrite(jsonOne, it.Advance());
                        AssertJsonWrite(jsonTwo, it.Advance());
                    });

                env.UndeployAll();
            }
        }

        private class EventJsonTypingWriteObjectType : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@public @buseventtype create json schema JsonEvent (c0 Object);\n" +
                          "@name('s0') select * from JsonEvent#keepall;\n";
                env.CompileDeploy(epl).AddListener("s0");
                var namesAndTypes = new object[][] { new object[] { "c0", typeof(object) } };
                env.AssertStatement(
                    "s0",
                    statement => SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                        namesAndTypes,
                        statement.EventType,
                        SupportEventTypeAssertionEnum.NAME,
                        SupportEventTypeAssertionEnum.TYPE));

                var jsons = new string[] {
                    "{\"c0\": 1}",
                    "{\"c0\": 1.0}",
                    "{\"c0\": null}",
                    "{\"c0\": true}",
                    "{\"c0\": false}",
                    "{\"c0\": \"abc\"}",
                    "{\"c0\": [\"abc\"]}",
                    "{\"c0\": []}",
                    "{\"c0\": [\"abc\", 2]}",
                    "{\"c0\": [[\"abc\"], [5.0]]}",
                    "{\"c0\": {\"c1\": 10}}",
                    "{\"c0\": {\"c1\": 10, \"c2\": \"abc\"}}",
                };
                foreach (var json in jsons) {
                    env.SendEventJson(json, "JsonEvent");
                    env.AssertEventNew("s0", @event => AssertJsonWrite(json, @event));
                }

                env.Milestone(0);

                env.AssertIterator(
                    "s0",
                    it => {
                        foreach (var json in jsons) {
                            AssertJsonWrite(json, it.Advance());
                        }
                    });

                env.UndeployAll();
            }
        }

        private class EventJsonTypingWriteObjectArrayType : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@public @buseventtype create json schema JsonEvent (c0 Object[]);\n" +
                          "@name('s0') select * from JsonEvent#keepall;\n";
                env.CompileDeploy(epl).AddListener("s0");
                var namesAndTypes = new object[][] { new object[] { "c0", typeof(object[]) } };
                env.AssertStatement(
                    "s0",
                    statement => SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                        namesAndTypes,
                        statement.EventType,
                        SupportEventTypeAssertionEnum.NAME,
                        SupportEventTypeAssertionEnum.TYPE));

                var jsons = new string[] {
                    "{\"c0\": []}",
                    "{\"c0\": [1.0]}",
                    "{\"c0\": [null]}",
                    "{\"c0\": [true]}",
                    "{\"c0\": [false]}",
                    "{\"c0\": [\"abc\"]}",
                    "{\"c0\": [[\"abc\"]]}",
                    "{\"c0\": [[]]}",
                    "{\"c0\": [[\"abc\", 2]]}",
                    "{\"c0\": [[[\"abc\"], [5.0]]]}",
                    "{\"c0\": [{\"c1\": 10}]}",
                    "{\"c0\": [{\"c1\": 10, \"c2\": \"abc\"}]}",
                };
                foreach (var json in jsons) {
                    env.SendEventJson(json, "JsonEvent");
                    env.AssertEventNew("s0", @event => AssertJsonWrite(json, @event));
                }

                env.Milestone(0);

                env.AssertIterator(
                    "s0",
                    it => {
                        foreach (var json in jsons) {
                            AssertJsonWrite(json, it.Advance());
                        }
                    });

                env.UndeployAll();
            }
        }

        private class EventJsonTypingWriteMapType : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@public @buseventtype create json schema JsonEvent (c0 Map);\n" +
                          "@name('s0') select * from JsonEvent#keepall;\n";
                env.CompileDeploy(epl).AddListener("s0");
                var namesAndTypes = new object[][] { new object[] { "c0", typeof(IDictionary<string, object>) } };
                env.AssertStatement(
                    "s0",
                    statement => SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                        namesAndTypes,
                        statement.EventType,
                        SupportEventTypeAssertionEnum.NAME,
                        SupportEventTypeAssertionEnum.TYPE));

                var jsons = new string[] {
                    "{\"c0\": {\"c1\" : 10}}",
                    "{\"c0\": {\"c1\": [\"c2\", 20]}}",
                };
                foreach (var json in jsons) {
                    env.SendEventJson(json, "JsonEvent");
                    env.AssertEventNew("s0", @event => AssertJsonWrite(json, @event));
                }

                env.Milestone(0);

                env.AssertIterator(
                    "s0",
                    it => {
                        foreach (var json in jsons) {
                            AssertJsonWrite(json, it.Advance());
                        }
                    });

                env.UndeployAll();
            }
        }

        private class EventJsonTypingParseDynamicPropJsonTypes : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                        "@JsonSchema(Dynamic=true) @public @buseventtype create json schema JsonEvent();\n" +
                        "@name('s0') select * from JsonEvent#keepall")
                    .AddListener("s0");

                var jsonOne = "{\n" +
                              "  \"a_string\": \"abc\",\n" +
                              "  \"a_number\": 1,\n" +
                              "  \"a_boolean\": true,\n" +
                              "  \"a_null\": null,\n" +
                              "  \"a_object\": {\n" +
                              "    \"value\": \"def\"\n" +
                              "  },\n" +
                              "  \"a_array\": [\n" +
                              "    \"a\",\n" +
                              "    \"b\"\n" +
                              "  ]\n" +
                              "}";
                env.SendEventJson(jsonOne, "JsonEvent");
                env.AssertEventNew("s0", @event => AssertJsonWrite(jsonOne, @event));

                var jsonTwo = "{}";
                env.SendEventJson(jsonTwo, "JsonEvent");
                env.AssertEventNew("s0", @event => AssertJsonWrite(jsonTwo, @event));

                var jsonThree = "{\"a_boolean\": false}";
                env.SendEventJson(jsonThree, "JsonEvent");
                env.AssertEventNew("s0", @event => AssertJsonWrite(jsonThree, @event));

                env.Milestone(0);

                env.AssertIterator(
                    "s0",
                    it => {
                        AssertJsonWrite(jsonOne, it.Advance());
                        AssertJsonWrite(jsonTwo, it.Advance());
                        AssertJsonWrite(jsonThree, it.Advance());
                    });

                env.UndeployAll();
            }
        }

        private class EventJsonTypingWriteDynamicPropMixedOjectArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                        "@JsonSchema(Dynamic=true) @public @buseventtype create json schema JsonEvent();\n" +
                        "@name('s0') select * from JsonEvent#keepall")
                    .AddListener("s0");

                var json = "{\n" +
                           "  \"a_array\": [\n" +
                           "    \"a\",\n" +
                           "     1,\n" +
                           "    {\n" +
                           "      \"value\": \"def\"\n" +
                           "    },\n" +
                           "    false,\n" +
                           "    null\n" +
                           "  ]\n" +
                           "}";
                env.SendEventJson(json, "JsonEvent");
                env.AssertEventNew("s0", @event => AssertJsonWrite(json, @event));

                env.Milestone(0);

                env.AssertIterator("s0", it => AssertJsonWrite(json, it.Advance()));

                env.UndeployAll();
            }
        }

        private class EventJsonTypingWriteDynamicPropNestedArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                        "@JsonSchema(Dynamic=true) @public @buseventtype create json schema JsonEvent();\n" +
                        "@name('s0') select * from JsonEvent#keepall")
                    .AddListener("s0");

                var jsonOne = "{\n" +
                              "  \"a_array\": [\n" +
                              "    [1,2],\n" +
                              "    [[3,4], 5]" +
                              "  ]\n" +
                              "}";
                env.SendEventJson(jsonOne, "JsonEvent");
                env.AssertEventNew("s0", @event => AssertJsonWrite(jsonOne, @event));

                var jsonTwo = "{\n" +
                              "  \"a_array\": [\n" +
                              "    [6, [ [7,8], [9], []]]\n" +
                              "  ]\n" +
                              "}";
                env.SendEventJson(jsonTwo, "JsonEvent");
                env.AssertEventNew("s0", @event => AssertJsonWrite(jsonTwo, @event));

                env.Milestone(0);

                env.AssertIterator(
                    "s0",
                    it => {
                        AssertJsonWrite(jsonOne, it.Advance());
                        AssertJsonWrite(jsonTwo, it.Advance());
                    });

                env.UndeployAll();
            }
        }

        private class EventJsonTypingWriteDynamicPropNumberFormat : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                        "@JsonSchema(Dynamic=true) @public @buseventtype create json schema JsonEvent();\n" +
                        "@name('s0') select * from JsonEvent#keepall")
                    .AddListener("s0");

                var json = "{ \"num1\": 42, \"num2\": 42.0, \"num3\": 43.0}";
                env.SendEventJson(json, "JsonEvent");
                env.AssertEventNew("s0", @event => AssertJsonWrite(json, @event));

                env.Milestone(0);

                env.AssertIterator("s0", en => AssertJsonWrite(json, en.Advance()));

                env.UndeployAll();
            }
        }

        private static JObject BuildBook(
            string bookId,
            int price)
        {
            var book = new JObject();
            book.Add("BookId", bookId);
            book.Add("Price", price);
            return book;
        }

        private static JObject BuildShelf(
            string shelfId,
            params JObject[] books)
        {
            var shelf = new JObject();
            shelf.Add("shelfId", shelfId);
            shelf.Add("Books", ArrayOfObjects(books));
            return shelf;
        }

        private static JObject BuildIsle(
            string isleId,
            params JObject[] shelfs)
        {
            var shelf = new JObject();
            shelf.Add("isleId", isleId);
            shelf.Add("shelfs", ArrayOfObjects(shelfs));
            return shelf;
        }

        private static JObject BuildLibrary(
            string libraryId,
            params JObject[] isles)
        {
            var shelf = new JObject();
            shelf.Add("libraryId", libraryId);
            shelf.Add("isles", ArrayOfObjects(isles));
            return shelf;
        }

        private static JArray ArrayOfObjects(JObject[] objects)
        {
            var array = new JArray();
            for (var i = 0; i < objects.Length; i++) {
                array.Add(objects[i]);
            }

            return array;
        }
    }
} // end of namespace