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
            WithPropertyCustomRenderer(execs);
            WithObjectArray(execs);
            WithPONOMap(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithPONOMap(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventRenderPONOMap());
            return execs;
        }

        public static IList<RegressionExecution> WithObjectArray(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventRenderObjectArray());
            return execs;
        }

        public static IList<RegressionExecution> WithPropertyCustomRenderer(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventRenderPropertyCustomRenderer());
            return execs;
        }

        private class EventRenderPropertyCustomRenderer : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@name('s0') select * from MyRendererEvent");
                env.SendEventBean(
                    new MyRendererEvent("id1", new object[][] { new object[] { 1, "x" }, new object[] { 2, "y" } }));

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
                Assert.AreEqual(1, (int)context.IndexedPropertyIndex);
                Assert.AreEqual(nameof(MyRendererEvent), context.EventType.Name);
                Assert.AreEqual("someProperties", context.PropertyName);

                var expectedJson =
                    "{ \"MyEvent\": { \"Id\": \"id1\", \"someProperties\": [\"index#0=1;index#1=x\", \"index#0=2;index#1=y\"], \"mappedProperty\": { \"key\": \"value\" } } }";
                Assert.AreEqual(RemoveNewline(expectedJson), RemoveNewline(json));

                MyRenderer.Contexts.Clear();
                var xmlOptions = new XMLRenderingOptions();
                xmlOptions.Renderer = new MyRenderer();
                var xmlOne = env.Runtime.RenderEventService.RenderXML(
                    "MyEvent",
                    env.GetEnumerator("s0").Advance(),
                    xmlOptions);
                var expected =
                    "<?xml version=\"1.0\" encoding=\"UTF-8\"?> <MyEvent> <id>id1</Id> <someProperties>index#0=1;index#1=x</someProperties> <someProperties>index#0=2;index#1=y</someProperties> <mappedProperty> <key>value</key> </mappedProperty> </MyEvent>";
                Assert.AreEqual(4, MyRenderer.Contexts.Count);
                Assert.AreEqual(RemoveNewline(expected), RemoveNewline(xmlOne));

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.SERDEREQUIRED);
            }
        }

        private class EventRenderObjectArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                object[] values = { "abc", 1, new SupportBean_S0(1, "P00"), 2L, 3d };
                env.CompileDeploy("@name('s0') select * from MyObjectArrayType");
                env.SendEventObjectArray(values, "MyObjectArrayType");

                env.AssertThat(
                    () => {
                        var json = env.Runtime.RenderEventService.RenderJSON(
                            "MyEvent",
                            env.GetEnumerator("s0").Advance());
                        var expectedJson =
                            "{ \"MyEvent\": { \"p0\": \"abc\", \"p1\": 1, \"p3\": 2, \"p4\": 3.0, \"p2\": { \"Id\": 1, \"P00\": \"P00\", \"P01\": null, \"P02\": null, \"P03\": null } } }";
                        Assert.AreEqual(RemoveNewline(expectedJson), RemoveNewline(json));
                    });

                env.AssertThat(
                    () => {
                        var xmlOne = env.Runtime.RenderEventService.RenderXML(
                            "MyEvent",
                            env.GetEnumerator("s0").Advance());
                        var expected =
                            "<?xml version=\"1.0\" encoding=\"UTF-8\"?> <MyEvent> <p0>abc</p0> <p1>1</p1> <p3>2</p3> <p4>3.0</p4> <p2> <id>1</Id> <P00>P00</P00> </p2> </MyEvent>";
                        Assert.AreEqual(RemoveNewline(expected), RemoveNewline(xmlOne));
                    });

                env.UndeployAll();
            }
        }

        private class EventRenderPONOMap : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED);
            }

            public void Run(RegressionEnvironment env)
            {
                var beanOne = new SupportBeanRendererOne();
                IDictionary<string, object> otherMap = new LinkedHashMap<string, object>();
                otherMap.Put("abc", "def");
                otherMap.Put("def", 123);
                otherMap.Put("efg", null);
                otherMap.Put(null, 1234);
                beanOne.StringObjectMap = otherMap;

                env.CompileDeploy("@name('s0') select * from SupportBeanRendererOne");
                env.SendEventBean(beanOne);

                var expectedJson =
                    "{ \"MyEvent\": { \"stringObjectMap\": { \"abc\": \"def\", \"def\": 123, \"efg\": null } } }";
                env.AssertThat(
                    () => {
                        var json = env.Runtime.RenderEventService.RenderJSON(
                            "MyEvent",
                            env.GetEnumerator("s0").Advance());
                        Assert.AreEqual(RemoveNewline(expectedJson), RemoveNewline(json));
                    });

                env.AssertThat(
                    () => {
                        var xmlOne = env.Runtime.RenderEventService.RenderXML(
                            "MyEvent",
                            env.GetEnumerator("s0").Advance());
                        var expected = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                                       "<MyEvent>\n" +
                                       "  <stringObjectMap>\n" +
                                       "    <abc>def</abc>\n" +
                                       "    <def>123</def>\n" +
                                       "    <efg></efg>\n" +
                                       "  </stringObjectMap>\n" +
                                       "</MyEvent>";
                        Assert.AreEqual(RemoveNewline(expected), RemoveNewline(xmlOne));
                    });

                env.AssertThat(
                    () => {
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
                    });
                env.UndeployModuleContaining("s0");

                // try the same Map only undeclared
                var beanThree = new SupportBeanRendererThree();
                beanThree.StringObjectMap = otherMap;
                env.CompileDeploy("@name('s0') select * from SupportBeanRendererThree");
                env.SendEventBean(beanThree);
                env.AssertIterator(
                    "s0",
                    iterator => {
                        var json = env.Runtime.RenderEventService.RenderJSON("MyEvent", iterator.Advance());
                        Assert.AreEqual(RemoveNewline(expectedJson), RemoveNewline(json));
                    });

                env.UndeployAll();
            }
        }

        private static string RemoveNewline(string text)
        {
            return text.RegexReplaceAll("\\s\\s+|\\n|\\r", " ").Trim();
        }

        public class MyRendererEvent
        {
            private readonly string id;
            private readonly object[][] someProperties;

            public MyRendererEvent(
                string id,
                object[][] someProperties)
            {
                this.id = id;
                this.someProperties = someProperties;
            }

            public string Id => id;

            public object[][] SomeProperties => someProperties;

            public IDictionary<string, object> MappedProperty => Collections.SingletonDataMap("key", "value");
        }

        public class MyRenderer : EventPropertyRenderer
        {
            private static IList<EventPropertyRendererContext> contexts = new List<EventPropertyRendererContext>();

            public void Render(EventPropertyRendererContext context)
            {
                if (context.PropertyName.Equals("someProperties")) {
                    var value = (object[])context.PropertyValue;

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

                contexts.Add(context.Copy());
            }

            public static IList<EventPropertyRendererContext> Contexts {
                get => MyRenderer.contexts;
                set => MyRenderer.contexts = value;
            }
        }
    }
} // end of namespace