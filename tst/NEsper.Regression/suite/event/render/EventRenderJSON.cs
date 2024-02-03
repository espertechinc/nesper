///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using com.espertech.esper.common.@internal.@event.render;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using Newtonsoft.Json.Linq;

using NUnit.Framework;
using NUnit.Framework.Legacy;
using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.@event.render
{
    public class EventRenderJSON
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithRenderSimple(execs);
            WithMapAndNestedArray(execs);
            WithEmptyMap(execs);
            WithEnquote(execs);
            WithJsonEventType(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithJsonEventType(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventRenderJsonEventType());
            return execs;
        }

        public static IList<RegressionExecution> WithEnquote(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventRenderEnquote());
            return execs;
        }

        public static IList<RegressionExecution> WithEmptyMap(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventRenderEmptyMap());
            return execs;
        }

        public static IList<RegressionExecution> WithMapAndNestedArray(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventRenderMapAndNestedArray());
            return execs;
        }

        public static IList<RegressionExecution> WithRenderSimple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventRenderRenderSimple());
            return execs;
        }

        private class EventRenderJsonEventType : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@public @buseventtype create json schema MyJsonEvent(p0 string, p1 int);\n" +
                          "@name('s0') select * from MyJsonEvent#keepall;\n";
                env.CompileDeploy(epl).AddListener("s0");
                env.SendEventJson(
                    new JObject(
                        new JProperty("p0", "abc"),
                        new JProperty("p1", 10)).ToString(),
                    "MyJsonEvent");

                var expected = "{\"p0\":\"abc\",\"p1\":10}";
                var expectedWithTitle = "{\"thetitle\":{\"p0\":\"abc\",\"p1\":10}}";
                env.AssertStatement(
                    "s0",
                    statement => {
                        var @event = statement.First();

                        var result = env.Runtime.RenderEventService.RenderJSON("thetitle", @event);
                        ClassicAssert.AreEqual(expectedWithTitle, result);

                        result = env.Runtime.RenderEventService.RenderJSON("thetitle", @event);
                        ClassicAssert.AreEqual(expectedWithTitle, result);

                        var renderer = env.Runtime.RenderEventService.GetJSONRenderer(statement.EventType);
                        result = renderer.Render("thetitle", @event);
                        ClassicAssert.AreEqual(expectedWithTitle, result);
                        result = renderer.Render(@event);
                        ClassicAssert.AreEqual(expected, result);
                    });

                env.UndeployAll();
            }
        }

        private class EventRenderRenderSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var bean = new SupportBean();
                bean.TheString = "a\nc>";
                bean.IntPrimitive = 1;
                bean.IntBoxed = 992;
                bean.CharPrimitive = 'x';
                bean.EnumValue = SupportEnum.ENUM_VALUE_1;

                env.CompileDeploy("@name('s0') select * from SupportBean").AddListener("s0");
                env.SendEventBean(bean);

                env.AssertStatement(
                    "s0",
                    statement => {
                        var result = env.Runtime.RenderEventService.RenderJSON("supportBean", statement.First());

                        //Console.WriteLine(result);
                        var valuesOnly =
                            "{ \"BigInteger\": null, \"BoolBoxed\": null, \"BoolPrimitive\": false, \"ByteBoxed\": null, \"BytePrimitive\": 0, \"CharBoxed\": null, \"CharPrimitive\": \"x\", \"DecimalBoxed\": null, \"DecimalPrimitive\": 0.0, \"DoubleBoxed\": null, \"DoublePrimitive\": 0.0, \"EnumValue\": \"ENUM_VALUE_1\", \"FloatBoxed\": null, \"FloatPrimitive\": 0.0, \"IntBoxed\": 992, \"IntPrimitive\": 1, \"LongBoxed\": null, \"LongPrimitive\": 0, \"ShortBoxed\": null, \"ShortPrimitive\": 0, \"TheString\": \"a\\nc>\", \"This\": { \"BigInteger\": null, \"BoolBoxed\": null, \"BoolPrimitive\": false, \"ByteBoxed\": null, \"BytePrimitive\": 0, \"CharBoxed\": null, \"CharPrimitive\": \"x\", \"DecimalBoxed\": null, \"DecimalPrimitive\": 0.0, \"DoubleBoxed\": null, \"DoublePrimitive\": 0.0, \"EnumValue\": \"ENUM_VALUE_1\", \"FloatBoxed\": null, \"FloatPrimitive\": 0.0, \"IntBoxed\": 992, \"IntPrimitive\": 1, \"LongBoxed\": null, \"LongPrimitive\": 0, \"ShortBoxed\": null, \"ShortPrimitive\": 0, \"TheString\": \"a\\nc>\" } }";
                        var expected = "{ \"supportBean\": " + valuesOnly + " }";
                        ClassicAssert.AreEqual(RemoveNewline(expected), RemoveNewline(result));

                        var renderer = env.Runtime.RenderEventService.GetJSONRenderer(statement.EventType);
                        var jsonEvent = renderer.Render("supportBean", statement.First());
                        ClassicAssert.AreEqual(RemoveNewline(expected), RemoveNewline(jsonEvent));

                        jsonEvent = renderer.Render(statement.First());
                        ClassicAssert.AreEqual(RemoveNewline(valuesOnly), RemoveNewline(jsonEvent));
                    });

                env.UndeployAll();
            }
        }

        private class EventRenderMapAndNestedArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@name('s0') select * from OuterMap").AddListener("s0");

                IDictionary<string, object> dataInner = new LinkedHashMap<string, object>();
                dataInner.Put("stringarr", new string[] { "a", "b" });
                dataInner.Put("prop1", "");
                IDictionary<string, object> dataInnerTwo = new LinkedHashMap<string, object>();
                dataInnerTwo.Put("stringarr", Array.Empty<string>());
                dataInnerTwo.Put("prop1", "abcdef");
                IDictionary<string, object> dataOuter = new LinkedHashMap<string, object>();
                dataOuter.Put("intarr", new int[] { 1, 2 });
                dataOuter.Put("innersimple", dataInner);
                dataOuter.Put("innerarray", new IDictionary<string, object>[] { dataInner, dataInnerTwo });
                dataOuter.Put("prop0", new SupportBean_A("A1"));
                env.SendEventMap(dataOuter, "OuterMap");

                env.AssertThat(
                    () => {
                        var result = env.Runtime.RenderEventService.RenderJSON(
                            "outerMap",
                            env.GetEnumerator("s0").Advance());
                        var expected = "{\n" +
                                       "  \"outerMap\": {\n" +
                                       "    \"intarr\": [1, 2],\n" +
                                       "    \"innerarray\": [{\n" +
                                       "        \"prop1\": \"\",\n" +
                                       "        \"stringarr\": [\"a\", \"b\"]\n" +
                                       "      },\n" +
                                       "      {\n" +
                                       "        \"prop1\": \"abcdef\",\n" +
                                       "        \"stringarr\": []\n" +
                                       "      }],\n" +
                                       "    \"innersimple\": {\n" +
                                       "      \"prop1\": \"\",\n" +
                                       "      \"stringarr\": [\"a\", \"b\"]\n" +
                                       "    },\n" +
                                       "    \"prop0\": {\n" +
                                       "      \"Id\": \"A1\"\n" +
                                       "    }\n" +
                                       "  }\n" +
                                       "}";
                        ClassicAssert.AreEqual(RemoveNewline(expected), RemoveNewline(result));
                    });

                env.UndeployAll();
            }
        }

        private class EventRenderEmptyMap : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@name('s0') select * from EmptyMapEvent");

                env.SendEventBean(new EmptyMapEvent(null));
                env.AssertThat(
                    () => {
                        var result = env.Runtime.RenderEventService.RenderJSON(
                            "outer",
                            env.GetEnumerator("s0").Advance());
                        var expected = "{ \"outer\": { \"Props\": null } }";
                        ClassicAssert.AreEqual(RemoveNewline(expected), RemoveNewline(result));
                    });

                env.SendEventBean(new EmptyMapEvent(Collections.GetEmptyMap<string, string>()));
                env.AssertThat(
                    () => {
                        var result = env.Runtime.RenderEventService.RenderJSON(
                            "outer",
                            env.GetEnumerator("s0").Advance());
                        var expected = "{ \"outer\": { \"Props\": {} } }";
                        ClassicAssert.AreEqual(RemoveNewline(expected), RemoveNewline(result));
                    });

                env.SendEventBean(new EmptyMapEvent(Collections.SingletonMap("a", "b")));
                env.AssertThat(
                    () => {
                        var result = env.Runtime.RenderEventService.RenderJSON(
                            "outer",
                            env.GetEnumerator("s0").Advance());
                        var expected = "{ \"outer\": { \"Props\": { \"a\": \"b\" } } }";
                        ClassicAssert.AreEqual(RemoveNewline(expected), RemoveNewline(result));
                    });

                env.UndeployAll();
            }
        }

        private class EventRenderEnquote : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var testdata = new string[][] {
                    new string[] { "\t", "\"\\t\"" },
                    new string[] { "\n", "\"\\n\"" },
                    new string[] { "\r", "\"\\r\"" },
                    new string[] { Convert.ToString((char)0), "\"\\u0000\"" },
                };

                for (var i = 0; i < testdata.Length; i++) {
                    var buf = new StringBuilder();
                    OutputValueRendererJSONString.Enquote(testdata[i][0], buf);
                    ClassicAssert.AreEqual(testdata[i][1], buf.ToString());
                }
            }
        }

        private static string RemoveNewline(string text)
        {
            return text.RegexReplaceAll("\\s\\s+|\\n|\\r", " ").Trim();
        }

        public class EmptyMapEvent
        {
            public EmptyMapEvent(IDictionary<string, string> props)
            {
                this.Props = props;
            }

            public IDictionary<string, string> Props { get; }
        }
    }
} // end of namespace