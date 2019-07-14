///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.render;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.render
{
    public class EventRender
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EventRenderPropertyCustomRenderer());
            execs.Add(new EventRenderObjectArray());
            execs.Add(new EventRenderPOJOMap());
            return execs;
        }

        private static string RemoveNewline(string text)
        {
            return text.RegexReplaceAll("\\s\\s+|\\n|\\r", " ").Trim();
        }

        internal class EventRenderPropertyCustomRenderer : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@Name('s0') select * from MyRendererEvent");
                env.SendEventBean(
                    new MyRendererEvent(
                        "id1",
                        new[] {new object[] {1, "x"}, new object[] {2, "y"}}));

                MyRenderer.Contexts.Clear();
                var jsonOptions = new JSONRenderingOptions();
                jsonOptions.Renderer = new MyRenderer();
                var json = env.Runtime.RenderEventService.RenderJSON(
                    "MyEvent",
                    env.GetEnumerator("s0").Advance(),
                    jsonOptions);
                Assert.AreEqual(4, MyRenderer.Contexts.Count);
                var contexts = MyRenderer.Contexts;
                var context = contexts[2];
                Assert.IsNotNull(context.DefaultRenderer);
                Assert.AreEqual(1, (int) context.IndexedPropertyIndex);
                Assert.AreEqual(typeof(MyRendererEvent).Name, context.EventType.Name);
                Assert.AreEqual("someProperties", context.PropertyName);

                var expectedJson =
                    "{ \"MyEvent\": { \"id\": \"id1\", \"someProperties\": [\"index#0=1;index#1=x\", \"index#0=2;index#1=y\"], \"mappedProperty\": { \"key\": \"value\" } } }";
                Assert.AreEqual(RemoveNewline(expectedJson), RemoveNewline(json));

                MyRenderer.Contexts.Clear();
                var xmlOptions = new XMLRenderingOptions();
                xmlOptions.Renderer = new MyRenderer();
                var xmlOne = env.Runtime.RenderEventService.RenderXML(
                    "MyEvent",
                    env.GetEnumerator("s0").Advance(),
                    xmlOptions);
                var expected =
                    "<?xml version=\"1.0\" encoding=\"UTF-8\"?> <MyEvent> <id>id1</id> <someProperties>index#0=1;index#1=x</someProperties> <someProperties>index#0=2;index#1=y</someProperties> <mappedProperty> <key>value</key> </mappedProperty> </MyEvent>";
                Assert.AreEqual(4, MyRenderer.Contexts.Count);
                Assert.AreEqual(RemoveNewline(expected), RemoveNewline(xmlOne));

                env.UndeployAll();
            }
        }

        internal class EventRenderObjectArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                object[] values = {"abc", 1, new SupportBean_S0(1, "p00"), 2L, 3d};
                env.CompileDeploy("@Name('s0') select * from MyObjectArrayType");
                env.SendEventObjectArray(values, "MyObjectArrayType");

                var json = env.Runtime.RenderEventService.RenderJSON("MyEvent", env.GetEnumerator("s0").Advance());
                var expectedJson =
                    "{ \"MyEvent\": { \"p0\": \"abc\", \"p1\": 1, \"p3\": 2, \"p4\": 3.0, \"p2\": { \"id\": 1, \"p00\": \"p00\", \"p01\": null, \"p02\": null, \"p03\": null } } }";
                Assert.AreEqual(RemoveNewline(expectedJson), RemoveNewline(json));

                var xmlOne = env.Runtime.RenderEventService.RenderXML("MyEvent", env.GetEnumerator("s0").Advance());
                var expected =
                    "<?xml version=\"1.0\" encoding=\"UTF-8\"?> <MyEvent> <p0>abc</p0> <p1>1</p1> <p3>2</p3> <p4>3.0</p4> <p2> <id>1</id> <p00>p00</p00> </p2> </MyEvent>";
                Assert.AreEqual(RemoveNewline(expected), RemoveNewline(xmlOne));

                env.UndeployAll();
            }
        }

        internal class EventRenderPOJOMap : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var beanOne = new SupportBeanRendererOne();
                IDictionary<string, object> otherMap = new LinkedHashMap<string, object>();
                otherMap.Put("abc", "def");
                otherMap.Put("def", 123);
                otherMap.Put("efg", null);
                otherMap.Put(null, 1234);
                beanOne.StringObjectMap = otherMap;

                env.CompileDeploy("@Name('s0') select * from SupportBeanRendererOne");
                env.SendEventBean(beanOne);

                var json = env.Runtime.RenderEventService.RenderJSON("MyEvent", env.GetEnumerator("s0").Advance());
                var expectedJson =
                    "{ \"MyEvent\": { \"stringObjectMap\": { \"abc\": \"def\", \"def\": 123, \"efg\": null } } }";
                Assert.AreEqual(RemoveNewline(expectedJson), RemoveNewline(json));

                var xmlOne = env.Runtime.RenderEventService.RenderXML("MyEvent", env.GetEnumerator("s0").Advance());
                var expected = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                               "<MyEvent>\n" +
                               "  <stringObjectMap>\n" +
                               "    <abc>def</abc>\n" +
                               "    <def>123</def>\n" +
                               "    <efg></efg>\n" +
                               "  </stringObjectMap>\n" +
                               "</MyEvent>";
                Assert.AreEqual(RemoveNewline(expected), RemoveNewline(xmlOne));

                var opt = new XMLRenderingOptions();
                opt.IsDefaultAsAttribute = true;
                var xmlTwo = env.Runtime.RenderEventService.RenderXML(
                    "MyEvent",
                    env.GetEnumerator("s0").Advance(),
                    opt);
                var expectedTwo = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                                  "<MyEvent>\n" +
                                  "  <stringObjectMap abc=\"def\" def=\"123\"/>\n" +
                                  "</MyEvent>";
                Assert.AreEqual(RemoveNewline(expectedTwo), RemoveNewline(xmlTwo));
                env.UndeployModuleContaining("s0");

                // try the same Map only undeclared
                var beanThree = new SupportBeanRendererThree();
                beanThree.StringObjectMap = otherMap;
                env.CompileDeploy("@Name('s0') select * from SupportBeanRendererThree");
                env.SendEventBean(beanThree);
                json = env.Runtime.RenderEventService.RenderJSON("MyEvent", env.GetEnumerator("s0").Advance());
                Assert.AreEqual(RemoveNewline(expectedJson), RemoveNewline(json));

                env.UndeployAll();
            }
        }

        public class MyRendererEvent
        {
            public MyRendererEvent(
                string id,
                object[][] someProperties)
            {
                Id = id;
                SomeProperties = someProperties;
            }

            public string Id { get; }

            public object[][] SomeProperties { get; }

            public IDictionary<string, object> MappedProperty =>
                Collections.SingletonMap<string, object>("key", "value");
        }

        public class MyRenderer : EventPropertyRenderer
        {
            public static IList<EventPropertyRendererContext> Contexts { get; set; } =
                new List<EventPropertyRendererContext>();

            public void Render(EventPropertyRendererContext context)
            {
                if (context.PropertyName.Equals("someProperties")) {
                    var value = (object[]) context.PropertyValue;

                    var builder = context.StringBuilder;
                    if (context.IsJsonFormatted) {
                        context.StringBuilder.Append("\"");
                    }

                    var delimiter = "";
                    for (var i = 0; i < value.Length; i++) {
                        builder.Append(delimiter);
                        builder.Append("index#");
                        builder.Append(Convert.ToString(i));
                        builder.Append("=");
                        builder.Append(value[i]);
                        delimiter = ";";
                    }

                    if (context.IsJsonFormatted) {
                        context.StringBuilder.Append("\"");
                    }
                }
                else {
                    context.DefaultRenderer.Render(context.PropertyValue, context.StringBuilder);
                }

                Contexts.Add(context.Copy());
            }
        }
    }
} // end of namespace