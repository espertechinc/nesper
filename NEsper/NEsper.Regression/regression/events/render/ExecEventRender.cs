///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.events.render
{
    public class ExecEventRender : RegressionExecution
    {
        public override void Configure(Configuration configuration)
        {
            configuration.EngineDefaults.ViewResources.IsIterableUnbound = true;
        }

        public override void Run(EPServiceProvider epService)
        {
            RunAssertionPropertyCustomRenderer(epService);
            RunAssertionObjectArray(epService);
            RunAssertionPOJOMap(epService);
        }

        private void RunAssertionPropertyCustomRenderer(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddEventType(typeof(MyRendererEvent));

            var stmt = epService.EPAdministrator.CreateEPL("select * from MyRendererEvent");
            epService.EPRuntime.SendEvent(
                new MyRendererEvent("id1", new[] {new object[] {1, "x"}, new object[] {2, "y"}}));

            MyRenderer.Contexts.Clear();
            var jsonOptions = new JSONRenderingOptions();
            jsonOptions.Renderer = new MyRenderer();
            var json = epService.EPRuntime.EventRenderer.RenderJSON("MyEvent", stmt.First(), jsonOptions);
            Assert.AreEqual(4, MyRenderer.Contexts.Count);
            var contexts = MyRenderer.Contexts;
            var contextsIndexed = contexts
                .Where(c => c.PropertyName == "SomeProperties")
                .ToList();

            Assert.That(contextsIndexed.Count, Is.Not.Zero);
            Assert.That(contextsIndexed.Count(c => c.IndexedPropertyIndex == 0), Is.EqualTo(1));
            Assert.That(contextsIndexed.Count(c => c.IndexedPropertyIndex == 1), Is.EqualTo(1));
            foreach (var context in contextsIndexed) {
                Assert.AreEqual(typeof(MyRendererEvent).Name, context.EventType.Name);
            }

            var expectedJson =
                "{ \"MyEvent\": { \"Id\": \"id1\", \"MappedProperty\": { \"key\": \"value\" }, \"SomeProperties\": [\"index#0=1;index#1=x\", \"index#0=2;index#1=y\"] } }";
            Assert.AreEqual(RemoveNewline(expectedJson), RemoveNewline(json));

            MyRenderer.Contexts.Clear();
            var xmlOptions = new XMLRenderingOptions();
            xmlOptions.Renderer = new MyRenderer();
            var xmlOne = epService.EPRuntime.EventRenderer.RenderXML("MyEvent", stmt.First(), xmlOptions);
            var expected =
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?> <MyEvent> <Id>id1</Id> <SomeProperties>index#0=1;index#1=x</SomeProperties> <SomeProperties>index#0=2;index#1=y</SomeProperties> <MappedProperty> <key>value</key> </MappedProperty> </MyEvent>";
            Assert.AreEqual(4, MyRenderer.Contexts.Count);
            Assert.AreEqual(RemoveNewline(expected), RemoveNewline(xmlOne));

            stmt.Dispose();
        }

        private void RunAssertionObjectArray(EPServiceProvider epService)
        {
            string[] props = {"p0", "p1", "p2", "p3", "p4"};
            object[] types = {typeof(string), typeof(int), typeof(SupportBean_S0), typeof(long), typeof(double?)};
            epService.EPAdministrator.Configuration.AddEventType("MyObjectArrayType", props, types);

            object[] values = {"abc", 1, new SupportBean_S0(1, "p00"), 2L, 3d};
            var stmt = epService.EPAdministrator.CreateEPL("select * from MyObjectArrayType");
            epService.EPRuntime.SendEvent(values, "MyObjectArrayType");

            var json = epService.EPRuntime.EventRenderer.RenderJSON("MyEvent", stmt.First());
            var expectedJson =
                "{ \"MyEvent\": { \"p0\": \"abc\", \"p1\": 1, \"p2\": { \"Id\": 1, \"P00\": \"p00\", \"P01\": null, \"P02\": null, \"P03\": null }, \"p3\": 2, \"p4\": 3.0 } }";
            Assert.AreEqual(RemoveNewline(expectedJson), RemoveNewline(json));

            var xmlOne = epService.EPRuntime.EventRenderer.RenderXML("MyEvent", stmt.First());
            var expected =
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?> <MyEvent> <p1>1</p1> <p3>2</p3> <p4>3.0</p4> <p0>abc</p0> <p2> <Id>1</Id> <P00>p00</P00> </p2> </MyEvent>";
            Assert.AreEqual(RemoveNewline(expected), RemoveNewline(xmlOne));

            stmt.Dispose();
        }

        private void RunAssertionPOJOMap(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddEventType(
                "SupportBeanRendererOne", typeof(SupportBeanRendererOne));
            epService.EPAdministrator.Configuration.AddEventType(
                "SupportBeanRendererThree", typeof(SupportBeanRendererThree));

            var beanOne = new SupportBeanRendererOne();
            var otherMap = new LinkedHashMap<string, object>();
            otherMap.Put("abc", "def");
            otherMap.Put("def", 123);
            otherMap.Put("efg", null);
            otherMap.Put(null, 1234);
            beanOne.StringObjectMap = otherMap;

            var stmt = epService.EPAdministrator.CreateEPL("select * from SupportBeanRendererOne");
            epService.EPRuntime.SendEvent(beanOne);

            var json = epService.EPRuntime.EventRenderer.RenderJSON("MyEvent", stmt.First());
            var expectedJson =
                "{ \"MyEvent\": { \"StringObjectMap\": { \"abc\": \"def\", \"def\": 123, \"efg\": null } } }";
            Assert.AreEqual(RemoveNewline(expectedJson), RemoveNewline(json));

            var xmlOne = epService.EPRuntime.EventRenderer.RenderXML("MyEvent", stmt.First());
            var expected = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                           "<MyEvent>\n" +
                           "  <StringObjectMap>\n" +
                           "    <abc>def</abc>\n" +
                           "    <def>123</def>\n" +
                           "    <efg></efg>\n" +
                           "  </StringObjectMap>\n" +
                           "</MyEvent>";
            Assert.AreEqual(RemoveNewline(expected), RemoveNewline(xmlOne));

            var opt = new XMLRenderingOptions();
            opt.IsDefaultAsAttribute = true;
            var xmlTwo = epService.EPRuntime.EventRenderer.RenderXML("MyEvent", stmt.First(), opt);
            var expectedTwo = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                              "<MyEvent>\n" +
                              "  <StringObjectMap abc=\"def\" def=\"123\"/>\n" +
                              "</MyEvent>";
            Assert.AreEqual(RemoveNewline(expectedTwo), RemoveNewline(xmlTwo));

            // try the same Map only undeclared
            var beanThree = new SupportBeanRendererThree();
            beanThree.StringObjectMap = otherMap;
            stmt = epService.EPAdministrator.CreateEPL("select * from SupportBeanRendererThree");
            epService.EPRuntime.SendEvent(beanThree);
            json = epService.EPRuntime.EventRenderer.RenderJSON("MyEvent", stmt.First());
            Assert.AreEqual(RemoveNewline(expectedJson), RemoveNewline(json));

            stmt.Dispose();
        }

        private string RemoveNewline(string text)
        {
            return text.RegexReplaceAll("\\s\\s+|\\n|\\r", " ").Trim();
        }

        public class MyRendererEvent
        {
            public MyRendererEvent(string id, object[][] someProperties)
            {
                Id = id;
                SomeProperties = someProperties;
            }

            public string Id { get; }

            public object[][] SomeProperties { get; }

            public IDictionary<string, object> MappedProperty => Collections.SingletonDataMap("key", "value");
        }

        public class MyRenderer : EventPropertyRenderer
        {
            public static List<EventPropertyRendererContext> Contexts { get; set; } =
                new List<EventPropertyRendererContext>();

            public void Render(EventPropertyRendererContext context)
            {
                if (context.PropertyName.Equals("SomeProperties")) {
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