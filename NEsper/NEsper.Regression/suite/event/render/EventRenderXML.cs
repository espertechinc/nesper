///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

using com.espertech.esper.common.client.render;
using com.espertech.esper.common.@internal.@event.render;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.@event.render
{
    public class EventRenderXML
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EventRenderRenderSimple());
            execs.Add(new EventRenderMapAndNestedArray());
            execs.Add(new EventRenderSQLDate());
            execs.Add(new EventRenderEnquote());
            return execs;
        }

        private static string RemoveNewline(string text)
        {
            return text.RegexReplaceAll("\\s\\s+|\\n|\\r", " ").Trim();
        }

        internal class EventRenderRenderSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var bean = new SupportBean();
                bean.TheString = "a\nc";
                bean.IntPrimitive = 1;
                bean.IntBoxed = 992;
                bean.CharPrimitive = 'x';
                bean.EnumValue = SupportEnum.ENUM_VALUE_2;

                env.CompileDeploy("@Name('s0') select * from SupportBean");
                env.SendEventBean(bean);

                var result = env.Runtime.RenderEventService.RenderXML("supportBean", env.GetEnumerator("s0").Advance());
                //System.out.println(result);
                var expected = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                               "<supportBean>\n" +
                               "  <BoolPrimitive>false</BoolPrimitive>\n" +
                               "  <BytePrimitive>0</BytePrimitive>\n" +
                               "  <CharPrimitive>x</CharPrimitive>\n" +
                               "  <DoublePrimitive>0.0</DoublePrimitive>\n" +
                               "  <EnumValue>ENUM_VALUE_2</EnumValue>\n" +
                               "  <FloatPrimitive>0.0</FloatPrimitive>\n" +
                               "  <IntBoxed>992</IntBoxed>\n" +
                               "  <IntPrimitive>1</IntPrimitive>\n" +
                               "  <longPrimitive>0</LongPrimitive>\n" +
                               "  <ShortPrimitive>0</ShortPrimitive>\n" +
                               "  <TheString>a\\u000ac</TheString>\n" +
                               "  <this>\n" +
                               "    <BoolPrimitive>false</BoolPrimitive>\n" +
                               "    <BytePrimitive>0</BytePrimitive>\n" +
                               "    <CharPrimitive>x</CharPrimitive>\n" +
                               "    <DoublePrimitive>0.0</DoublePrimitive>\n" +
                               "    <EnumValue>ENUM_VALUE_2</EnumValue>\n" +
                               "    <FloatPrimitive>0.0</FloatPrimitive>\n" +
                               "    <IntBoxed>992</IntBoxed>\n" +
                               "    <IntPrimitive>1</IntPrimitive>\n" +
                               "    <LongPrimitive>0</LongPrimitive>\n" +
                               "    <ShortPrimitive>0</ShortPrimitive>\n" +
                               "    <TheString>a\\u000ac</TheString>\n" +
                               "  </this>\n" +
                               "</supportBean>";
                Assert.AreEqual(RemoveNewline(expected), RemoveNewline(result));

                result = env.Runtime.RenderEventService.RenderXML(
                    "supportBean",
                    env.GetEnumerator("s0").Advance(),
                    new XMLRenderingOptions().SetIsDefaultAsAttribute(true));
                // System.out.println(result);
                expected =
                    "<?xml version=\"1.0\" encoding=\"UTF-8\"?> <supportBean BoolPrimitive=\"false\" BytePrimitive=\"0\" CharPrimitive=\"x\" DoublePrimitive=\"0.0\" EnumValue=\"ENUM_VALUE_2\" FloatPrimitive=\"0.0\" IntBoxed=\"992\" IntPrimitive=\"1\" LongPrimitive=\"0\" ShortPrimitive=\"0\" TheString=\"a\\u000ac\"> <this BoolPrimitive=\"false\" BytePrimitive=\"0\" CharPrimitive=\"x\" DoublePrimitive=\"0.0\" EnumValue=\"ENUM_VALUE_2\" FloatPrimitive=\"0.0\" IntBoxed=\"992\" IntPrimitive=\"1\" LongPrimitive=\"0\" ShortPrimitive=\"0\" TheString=\"a\\u000ac\"/> </supportBean>";
                Assert.AreEqual(RemoveNewline(expected), RemoveNewline(result));

                env.UndeployAll();
            }
        }

        internal class EventRenderMapAndNestedArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@Name('s0') select * from OuterMap").AddListener("s0");

                IDictionary<string, object> dataInner = new LinkedHashMap<string, object>();
                dataInner.Put("stringarr", new[] {"a", null});
                dataInner.Put("prop1", "");
                IDictionary<string, object> dataArrayOne = new LinkedHashMap<string, object>();
                dataArrayOne.Put("stringarr", new string[0]);
                dataArrayOne.Put("prop1", "abcdef");
                IDictionary<string, object> dataArrayTwo = new LinkedHashMap<string, object>();
                dataArrayTwo.Put("stringarr", new[] {"R&R", "a>b"});
                dataArrayTwo.Put("prop1", "");
                IDictionary<string, object> dataArrayThree = new LinkedHashMap<string, object>();
                dataArrayOne.Put("stringarr", null);
                IDictionary<string, object> dataOuter = new LinkedHashMap<string, object>();
                dataOuter.Put("intarr", new[] {1, 2});
                dataOuter.Put("innersimple", dataInner);
                dataOuter.Put("innerarray", new[] {dataArrayOne, dataArrayTwo, dataArrayThree});
                dataOuter.Put("prop0", new SupportBean_A("A1"));
                env.SendEventMap(dataOuter, "OuterMap");

                var result = env.Runtime.RenderEventService.RenderXML("outerMap", env.GetEnumerator("s0").Advance());
                // System.out.println(result);
                var expected = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                               "<outerMap>\n" +
                               "  <intarr>1</intarr>\n" +
                               "  <intarr>2</intarr>\n" +
                               "  <innersimple>\n" +
                               "    <prop1></prop1>\n" +
                               "    <stringarr>a</stringarr>\n" +
                               "  </innersimple>\n" +
                               "  <innerarray>\n" +
                               "    <prop1>abcdef</prop1>\n" +
                               "  </innerarray>\n" +
                               "  <innerarray>\n" +
                               "    <prop1></prop1>\n" +
                               "    <stringarr>R&amp;R</stringarr>\n" +
                               "    <stringarr>a&gt;b</stringarr>\n" +
                               "  </innerarray>\n" +
                               "  <innerarray>\n" +
                               "  </innerarray>\n" +
                               "  <prop0>\n" +
                               "    <id>A1</id>\n" +
                               "  </prop0>\n" +
                               "</outerMap>";
                Assert.AreEqual(RemoveNewline(expected), RemoveNewline(result));

                result = env.Runtime.RenderEventService.RenderXML(
                    "outerMap xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"",
                    env.GetEnumerator("s0").Advance(),
                    new XMLRenderingOptions().SetIsDefaultAsAttribute(true));
                // System.out.println(result);
                expected = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                           "<outerMap xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">\n" +
                           "  <intarr>1</intarr>\n" +
                           "  <intarr>2</intarr>\n" +
                           "  <innersimple prop1=\"\">\n" +
                           "    <stringarr>a</stringarr>\n" +
                           "  </innersimple>\n" +
                           "  <innerarray prop1=\"abcdef\"/>\n" +
                           "  <innerarray prop1=\"\">\n" +
                           "    <stringarr>R&amp;R</stringarr>\n" +
                           "    <stringarr>a&gt;b</stringarr>\n" +
                           "  </innerarray>\n" +
                           "  <innerarray/>\n" +
                           "  <prop0 id=\"A1\"/>\n" +
                           "</outerMap>";
                Assert.AreEqual(RemoveNewline(expected), RemoveNewline(result));

                env.UndeployAll();
            }
        }

        internal class EventRenderSQLDate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // ESPER-469
                env.CompileDeploy(
                    "@Name('s0') select System.DateTime.Parse(\"2010-01-31\") as mySqlDate from SupportBean");
                env.SendEventBean(new SupportBean());

                var theEvent = env.GetEnumerator("s0").Advance();
                Assert.AreEqual(DateTime.Parse("2010-01-31"), theEvent.Get("mySqlDate"));
                var getter = env.Statement("s0").EventType.GetGetter("mySqlDate");
                Assert.AreEqual(DateTime.Parse("2010-01-31"), getter.Get(theEvent));

                var result = env.Runtime.RenderEventService.RenderXML("testsqldate", theEvent);

                // System.out.println(result);
                var expected = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                               "<testsqldate>" +
                               "<mySqlDate>2010-01-31</mySqlDate>" +
                               "</testsqldate>";
                Assert.AreEqual(RemoveNewline(expected), RemoveNewline(result));

                env.UndeployAll();
            }
        }

        internal class EventRenderEnquote : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[][] testdata = {
                    new[] {"\"", "&quot;"},
                    new[] {"'", "&apos;"},
                    new[] {"&", "&amp;"},
                    new[] {"<", "&lt;"},
                    new[] {">", "&gt;"},
                    new[] {Convert.ToString((char) 0), "\\u0000"}
                };

                for (var i = 0; i < testdata.Length; i++) {
                    var buf = new StringBuilder();
                    OutputValueRendererXMLString.XmlEncode(testdata[i][0], buf, true);
                    Assert.AreEqual(testdata[i][1], buf.ToString());
                }
            }
        }
    }
} // end of namespace