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
using System.Text;

using com.espertech.esper.client;
using com.espertech.esper.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.events
{
    [TestFixture]
    public class TestEventRenderer
    {
        private EPServiceProvider _epService;

        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.ViewResourcesConfig.IsIterableUnbound = true;
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
        }

        private String RemoveNewline(String text)
        {
            return text.RegexReplaceAll("\\s\\s+|\\n|\\r", " ").Trim();
        }

        public class MyRendererEvent
        {
            public MyRendererEvent(String id, Object[][] someProperties)
            {
                Id = id;
                SomeProperties = someProperties;
            }

            public string Id { get; private set; }

            public object[][] SomeProperties { get; private set; }

            public IDictionary<string, object> MappedProperty
            {
                get { return Collections.SingletonDataMap("key", "value"); }
            }
        }


        public class MyRenderer : EventPropertyRenderer
        {
            static MyRenderer()
            {
                Contexts = new List<EventPropertyRendererContext>();
            }

            public static List<EventPropertyRendererContext> Contexts { get; set; }

            #region EventPropertyRenderer Members

            public void Render(EventPropertyRendererContext context)
            {
                if (context.PropertyName.Equals("SomeProperties"))
                {
                    var value = (Object[]) context.PropertyValue;

                    StringBuilder builder = context.StringBuilder;

                    if (context.IsJsonFormatted)
                    {
                        context.StringBuilder.Append("\"");
                    }
                    String delimiter = "";

                    for (int i = 0; i < value.Length; i++)
                    {
                        builder.Append(delimiter);
                        builder.Append("index#");
                        builder.Append(Convert.ToString(i));
                        builder.Append("=");
                        builder.Append(value[i]);
                        delimiter = ";";
                    }
                    if (context.IsJsonFormatted)
                    {
                        context.StringBuilder.Append("\"");
                    }
                }
                else
                {
                    context.DefaultRenderer.Render(context.PropertyValue,
                                                   context.StringBuilder);
                }

                Contexts.Add(context.Copy());
            }

            #endregion
        }

        [Test]
        public void TestObjectArray()
        {
            String[] props = {
                "p0", "p1", "p2", "p3", "p4"
            };
            
            Object[] types = {
                typeof (string), typeof (int), typeof (SupportBean_S0),
                typeof (long), typeof (double)
            };

            _epService.EPAdministrator.Configuration.AddEventType(
                "MyObjectArrayType", props, types);

            Object[] values = {
                "abc", 1, new SupportBean_S0(1, "p00"), 2L, 3d
            };

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                "select * from MyObjectArrayType");

            _epService.EPRuntime.SendEvent(values, "MyObjectArrayType");

            String json = _epService.EPRuntime.EventRenderer.RenderJSON(
                "MyEvent", stmt.First());
            String expectedJson =
                "{ \"MyEvent\": { \"p1\": 1, \"p3\": 2, \"p4\": 3.0, \"p0\": \"abc\", \"p2\": { \"Id\": 1, \"P00\": \"p00\", \"P01\": null, \"P02\": null, \"P03\": null } } }";

            Assert.AreEqual(RemoveNewline(expectedJson), RemoveNewline(json));

            String xmlOne = _epService.EPRuntime.EventRenderer.RenderXML(
                "MyEvent", stmt.First());
            String expected =
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?> <MyEvent> <p1>1</p1> <p3>2</p3> <p4>3.0</p4> <p0>abc</p0> <p2> <Id>1</Id> <P00>p00</P00> </p2> </MyEvent>";

            Assert.AreEqual(RemoveNewline(expected), RemoveNewline(xmlOne));
        }

        [Test]
        public void TestObjectMap()
        {
            _epService.EPAdministrator.Configuration.AddEventType(
                "SupportBeanRendererOne", typeof (SupportBeanRendererOne));
            _epService.EPAdministrator.Configuration.AddEventType(
                "SupportBeanRendererThree", typeof (SupportBeanRendererThree));

            var beanOne = new SupportBeanRendererOne();
            IDictionary<String, Object> otherMap = new LinkedHashMap<String, Object>();

            otherMap["abc"] = "def";
            otherMap["def"] = 123;
            otherMap["efg"] = null;
            otherMap.Put(null, 1234);
            beanOne.StringObjectMap = otherMap;

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                "select * from SupportBeanRendererOne");

            _epService.EPRuntime.SendEvent(beanOne);

            String json = _epService.EPRuntime.EventRenderer.RenderJSON(
                "MyEvent", stmt.First());
            String expectedJson =
                "{ \"MyEvent\": { \"StringObjectMap\": { \"abc\": \"def\", \"def\": 123, \"efg\": null } } }";

            Assert.AreEqual(RemoveNewline(expectedJson), RemoveNewline(json));

            String xmlOne = _epService.EPRuntime.EventRenderer.RenderXML(
                "MyEvent", stmt.First());
            String expected = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n"
                              + "<MyEvent>\n" + "  <StringObjectMap>\n"
                              + "    <abc>def<abc>\n" + "    <def>123<def>\n"
                              + "    <efg><efg>\n" + "  </StringObjectMap>\n" + "</MyEvent>";

            Assert.AreEqual(RemoveNewline(expected), RemoveNewline(xmlOne));

            var opt = new XMLRenderingOptions();

            opt.IsDefaultAsAttribute = true;
            String xmlTwo = _epService.EPRuntime.EventRenderer.RenderXML(
                "MyEvent", stmt.First(), opt);
            String expectedTwo = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n"
                                 + "<MyEvent>\n"
                                 + "  <StringObjectMap abc=\"def\" def=\"123\"/>\n"
                                 + "</MyEvent>";

            Assert.AreEqual(RemoveNewline(expectedTwo), RemoveNewline(xmlTwo));

            // try the same Map only undeclared
            var beanThree = new SupportBeanRendererThree();

            beanThree.StringObjectMap = otherMap;
            stmt = _epService.EPAdministrator.CreateEPL(
                "select * from SupportBeanRendererThree");
            _epService.EPRuntime.SendEvent(beanThree);
            json = _epService.EPRuntime.EventRenderer.RenderJSON("MyEvent", stmt.First());
            Assert.AreEqual(RemoveNewline(expectedJson), RemoveNewline(json));
        }

        [Test]
        public void TestPropertyCustomRenderer()
        {
            _epService.EPAdministrator.Configuration.AddEventType(
                typeof (MyRendererEvent));

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                "select * from MyRendererEvent");

            _epService.EPRuntime.SendEvent(
                new MyRendererEvent("id1", new Object[][]
                {
                    new Object[] {1, "x"},
                    new Object[] {2, "y"}
                }));

            MyRenderer.Contexts.Clear();
            var jsonOptions = new JSONRenderingOptions();

            jsonOptions.Renderer = new MyRenderer();
            String json = _epService.EPRuntime.EventRenderer.RenderJSON(
                "MyEvent", stmt.First(), jsonOptions);

            Assert.AreEqual(4, MyRenderer.Contexts.Count);
            List<EventPropertyRendererContext> contexts = MyRenderer.Contexts;
            EventPropertyRendererContext context = contexts[2];

            Assert.NotNull(context.DefaultRenderer);
            Assert.AreEqual(1, (int) context.IndexedPropertyIndex);
            Assert.AreEqual(typeof (MyRendererEvent).Name, context.EventType.Name);
            Assert.AreEqual("SomeProperties", context.PropertyName);

            String expectedJson =
                "{ \"MyEvent\": { \"Id\": \"id1\", \"SomeProperties\": [\"index#0=1;index#1=x\", \"index#0=2;index#1=y\"], \"MappedProperty\": { \"key\": \"value\" } } }";

            Assert.AreEqual(RemoveNewline(expectedJson), RemoveNewline(json));

            MyRenderer.Contexts.Clear();
            var xmlOptions = new XMLRenderingOptions();

            xmlOptions.Renderer = new MyRenderer();
            String xmlOne = _epService.EPRuntime.EventRenderer.RenderXML(
                "MyEvent", stmt.First(), xmlOptions);
            String expected =
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?> <MyEvent> <Id>id1</Id> <SomeProperties>index#0=1;index#1=x</SomeProperties> <SomeProperties>index#0=2;index#1=y</SomeProperties> <MappedProperty> <key>value<key> </MappedProperty> </MyEvent>";

            Assert.AreEqual(4, MyRenderer.Contexts.Count);
            Assert.AreEqual(RemoveNewline(expected), RemoveNewline(xmlOne));
        }
    }
}