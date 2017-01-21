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
using com.espertech.esper.events.util;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.events
{
    [TestFixture]
    public class TestEventRendererJSON 
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
    
        [Test]
        public void TestRenderSimple()
        {
            SupportBean bean = new SupportBean();
            bean.TheString = "a\nc>";
            bean.IntPrimitive = 1;
            bean.IntBoxed = 992;
            bean.CharPrimitive = 'x';
            bean.EnumValue = SupportEnum.ENUM_VALUE_1;
            
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            EPStatement statement = _epService.EPAdministrator.CreateEPL("select * from SupportBean");
            _epService.EPRuntime.SendEvent(bean);
            
            String result = _epService.EPRuntime.EventRenderer.RenderJSON("supportBean", statement.First());
    
            //Console.WriteLine(result);
            String valuesOnly = "{ \"BoolBoxed\": null, \"BoolPrimitive\": false, \"ByteBoxed\": null, \"BytePrimitive\": 0, \"CharBoxed\": null, \"CharPrimitive\": \"x\", \"DecimalBoxed\": null, \"DoubleBoxed\": null, \"DoublePrimitive\": 0.0, \"EnumValue\": \"ENUM_VALUE_1\", \"FloatBoxed\": null, \"FloatPrimitive\": 0.0, \"IntBoxed\": 992, \"IntPrimitive\": 1, \"LongBoxed\": null, \"LongPrimitive\": 0, \"ShortBoxed\": null, \"ShortPrimitive\": 0, \"TheString\": \"a\\nc>\", \"This\": { \"BoolBoxed\": null, \"BoolPrimitive\": false, \"ByteBoxed\": null, \"BytePrimitive\": 0, \"CharBoxed\": null, \"CharPrimitive\": \"x\", \"DecimalBoxed\": null, \"DoubleBoxed\": null, \"DoublePrimitive\": 0.0, \"EnumValue\": \"ENUM_VALUE_1\", \"FloatBoxed\": null, \"FloatPrimitive\": 0.0, \"IntBoxed\": 992, \"IntPrimitive\": 1, \"LongBoxed\": null, \"LongPrimitive\": 0, \"ShortBoxed\": null, \"ShortPrimitive\": 0, \"TheString\": \"a\\nc>\" } }";
            String expected = "{ \"supportBean\": " + valuesOnly + " }";
            Assert.AreEqual(RemoveNewline(expected), RemoveNewline(result));
            
            JSONEventRenderer renderer = _epService.EPRuntime.EventRenderer.GetJSONRenderer(statement.EventType);
            String jsonEvent = renderer.Render("supportBean", statement.First());
            Assert.AreEqual(RemoveNewline(expected), RemoveNewline(jsonEvent));
    
            jsonEvent = renderer.Render(statement.First());
            Assert.AreEqual(RemoveNewline(valuesOnly), RemoveNewline(jsonEvent));
        }
    
        [Test]
        public void TestMapAndNestedArray()
        {
            IDictionary<String, Object> defOuter = new LinkedHashMap<String, Object>();
            defOuter.Put("intarr", typeof(int[]));
            defOuter.Put("innersimple", "InnerMap");
            defOuter.Put("innerarray", "InnerMap[]");
            defOuter.Put("prop0", typeof(SupportBean_A));
    
            IDictionary<String, Object> defInner = new LinkedHashMap<String, Object>();
            defInner.Put("stringarr", typeof(string[]));
            defInner.Put("prop1", typeof(String));
    
            _epService.EPAdministrator.Configuration.AddEventType("InnerMap", defInner);
            _epService.EPAdministrator.Configuration.AddEventType("OuterMap", defOuter);
            EPStatement statement = _epService.EPAdministrator.CreateEPL("select * from OuterMap");
    
            IDictionary<String, Object> dataInner = new LinkedHashMap<String, Object>();
            dataInner.Put("stringarr", new String[] {"a", "b"});
            dataInner.Put("prop1", "");
            IDictionary<String, Object> dataInnerTwo = new LinkedHashMap<String, Object>();
            dataInnerTwo.Put("stringarr", new String[0]);
            dataInnerTwo.Put("prop1", "abcdef");
            IDictionary<String, Object> dataOuter = new LinkedHashMap<String, Object>();
            dataOuter.Put("intarr", new int[] {1, 2});
            dataOuter.Put("innersimple", dataInner);
            dataOuter.Put("innerarray", new IDictionary<string, object>[] { dataInner, dataInnerTwo });
            dataOuter.Put("prop0", new SupportBean_A("A1"));
            _epService.EPRuntime.SendEvent(dataOuter, "OuterMap");
    
            String result = _epService.EPRuntime.EventRenderer.RenderJSON("outerMap", statement.First());
    
            //Console.WriteLine(result);
            String expected = "{\n" +
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
            Assert.AreEqual(RemoveNewline(expected), RemoveNewline(result));
        }
    
        [Test]
        public void TestEmptyMap()
        {
            _epService.EPAdministrator.Configuration.AddEventType("EmptyMapEvent", typeof(EmptyMapEvent));
            EPStatement statement = _epService.EPAdministrator.CreateEPL("select * from EmptyMapEvent");
    
            _epService.EPRuntime.SendEvent(new EmptyMapEvent(null));
            String result = _epService.EPRuntime.EventRenderer.RenderJSON("outer", statement.First());
            String expected = "{ \"outer\": { \"Props\": null } }";
            Assert.AreEqual(RemoveNewline(expected), RemoveNewline(result));
    
            _epService.EPRuntime.SendEvent(new EmptyMapEvent(Collections.GetEmptyMap<String, String>()));
            result = _epService.EPRuntime.EventRenderer.RenderJSON("outer", statement.First());
            expected = "{ \"outer\": { \"Props\": {} } }";
            Assert.AreEqual(RemoveNewline(expected), RemoveNewline(result));
    
            _epService.EPRuntime.SendEvent(new EmptyMapEvent(Collections.SingletonMap("a", "b")));
            result = _epService.EPRuntime.EventRenderer.RenderJSON("outer", statement.First());
            expected = "{ \"outer\": { \"Props\": { \"a\": \"b\" } } }";
            Assert.AreEqual(RemoveNewline(expected), RemoveNewline(result));
        }
    
        public static void TestEnquote()
        {
            String[][] testdata =
            {
                    new string[] {"\t", "\"\\t\""},
                    new string[] {"\n", "\"\\n\""},
                    new string[] {"\r", "\"\\r\""},
                    new string[] {((char)0).ToString(), "\"\\u0000\""},
            };
    
            for (int i = 0; i < testdata.Length; i++)
            {
                StringBuilder buf = new StringBuilder();
                OutputValueRendererJSONString.Enquote(testdata[i][0], buf);
                Assert.AreEqual(testdata[i][1], buf.ToString());
            }
        }
    
        private String RemoveNewline(String text)
        {
            return text.RegexReplaceAll("\\s\\s+|\\n|\\r", " ").Trim();
        }
    
        public class EmptyMapEvent
        {
            public EmptyMapEvent(IDictionary<String, String> props)
            {
                Props = props;
            }

            public IDictionary<string, string> Props { get; private set; }
        }
    }
}
