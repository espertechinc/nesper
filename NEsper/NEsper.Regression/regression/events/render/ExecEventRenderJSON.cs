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
using System.Net.Mime;
using System.Text;
using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.events.util;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.events.render
{
    using Map = IDictionary<string, object>;

    public class ExecEventRenderJSON : RegressionExecution
    {
        public override void Configure(Configuration configuration)
        {
            configuration.EngineDefaults.ViewResources.IsIterableUnbound = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionRenderSimple(epService);
            RunAssertionMapAndNestedArray(epService);
            RunAssertionEmptyMap(epService);
            RunAssertionEnquote();
        }
    
        private void RunAssertionRenderSimple(EPServiceProvider epService) {
            var bean = new SupportBean();
            bean.TheString = "a\nc>";
            bean.IntPrimitive = 1;
            bean.IntBoxed = 992;
            bean.CharPrimitive = 'x';
            bean.EnumValue = SupportEnum.ENUM_VALUE_1;
    
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            EPStatement statement = epService.EPAdministrator.CreateEPL("select * from SupportBean");
            epService.EPRuntime.SendEvent(bean);
    
            string result = epService.EPRuntime.EventRenderer.RenderJSON("supportBean", statement.First());
    
            //Log.Info(result);
            string valuesOnly = "{ \"BoolBoxed\": null, \"BoolPrimitive\": false, \"ByteBoxed\": null, \"BytePrimitive\": 0, \"CharBoxed\": null, \"CharPrimitive\": \"x\", \"DecimalBoxed\": null, \"DoubleBoxed\": null, \"DoublePrimitive\": 0.0, \"EnumValue\": \"ENUM_VALUE_1\", \"FloatBoxed\": null, \"FloatPrimitive\": 0.0, \"IntBoxed\": 992, \"IntPrimitive\": 1, \"LongBoxed\": null, \"LongPrimitive\": 0, \"ShortBoxed\": null, \"ShortPrimitive\": 0, \"TheString\": \"a\\nc>\", \"This\": { \"BoolBoxed\": null, \"BoolPrimitive\": false, \"ByteBoxed\": null, \"BytePrimitive\": 0, \"CharBoxed\": null, \"CharPrimitive\": \"x\", \"DecimalBoxed\": null, \"DoubleBoxed\": null, \"DoublePrimitive\": 0.0, \"EnumValue\": \"ENUM_VALUE_1\", \"FloatBoxed\": null, \"FloatPrimitive\": 0.0, \"IntBoxed\": 992, \"IntPrimitive\": 1, \"LongBoxed\": null, \"LongPrimitive\": 0, \"ShortBoxed\": null, \"ShortPrimitive\": 0, \"TheString\": \"a\\nc>\" } }";
            string expected = "{ \"supportBean\": " + valuesOnly + " }";
            Assert.AreEqual(RemoveNewline(expected), RemoveNewline(result));
    
            JSONEventRenderer renderer = epService.EPRuntime.EventRenderer.GetJSONRenderer(statement.EventType);
            string jsonEvent = renderer.Render("supportBean", statement.First());
            Assert.AreEqual(RemoveNewline(expected), RemoveNewline(jsonEvent));
    
            jsonEvent = renderer.Render(statement.First());
            Assert.AreEqual(RemoveNewline(valuesOnly), RemoveNewline(jsonEvent));
    
            statement.Dispose();
        }
    
        private void RunAssertionMapAndNestedArray(EPServiceProvider epService) {
            var defOuter = new LinkedHashMap<string, Object>();
            defOuter.Put("intarr", typeof(int[]));
            defOuter.Put("innersimple", "InnerMap");
            defOuter.Put("innerarray", "InnerMap[]");
            defOuter.Put("prop0", typeof(SupportBean_A));
    
            var defInner = new LinkedHashMap<string, Object>();
            defInner.Put("stringarr", typeof(string[]));
            defInner.Put("prop1", typeof(string));
    
            epService.EPAdministrator.Configuration.AddEventType("InnerMap", defInner);
            epService.EPAdministrator.Configuration.AddEventType("OuterMap", defOuter);
            EPStatement statement = epService.EPAdministrator.CreateEPL("select * from OuterMap");
    
            var dataInner = new LinkedHashMap<string, Object>();
            dataInner.Put("stringarr", new string[]{"a", "b"});
            dataInner.Put("prop1", "");
            var dataInnerTwo = new LinkedHashMap<string, Object>();
            dataInnerTwo.Put("stringarr", new string[0]);
            dataInnerTwo.Put("prop1", "abcdef");
            var dataOuter = new LinkedHashMap<string, Object>();
            dataOuter.Put("intarr", new int[]{1, 2});
            dataOuter.Put("innersimple", dataInner);
            dataOuter.Put("innerarray", new Map[]{dataInner, dataInnerTwo});
            dataOuter.Put("prop0", new SupportBean_A("A1"));
            epService.EPRuntime.SendEvent(dataOuter, "OuterMap");
    
            string result = epService.EPRuntime.EventRenderer.RenderJSON("outerMap", statement.First());
    
            //Log.Info(result);
            string expected = "{\n" +
                    "  \"outerMap\": {\n" +
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
                    "    \"intarr\": [1, 2],\n" +
                    "    \"prop0\": {\n" +
                    "      \"Id\": \"A1\"\n" +
                    "    }\n" +
                    "  }\n" +
                    "}";
            Assert.AreEqual(RemoveNewline(expected), RemoveNewline(result));
    
            statement.Dispose();
        }
    
        private void RunAssertionEmptyMap(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("EmptyMapEvent", typeof(EmptyMapEvent));
            EPStatement statement = epService.EPAdministrator.CreateEPL("select * from EmptyMapEvent");
    
            epService.EPRuntime.SendEvent(new EmptyMapEvent(null));
            string result = epService.EPRuntime.EventRenderer.RenderJSON("outer", statement.First());
            string expected = "{ \"outer\": { \"props\": null } }";
            Assert.AreEqual(RemoveNewline(expected), RemoveNewline(result));
    
            epService.EPRuntime.SendEvent(new EmptyMapEvent(Collections.GetEmptyMap<string, string>()));
            result = epService.EPRuntime.EventRenderer.RenderJSON("outer", statement.First());
            expected = "{ \"outer\": { \"props\": {} } }";
            Assert.AreEqual(RemoveNewline(expected), RemoveNewline(result));
    
            epService.EPRuntime.SendEvent(new EmptyMapEvent(Collections.SingletonMap("a", "b")));
            result = epService.EPRuntime.EventRenderer.RenderJSON("outer", statement.First());
            expected = "{ \"outer\": { \"props\": { \"a\": \"b\" } } }";
            Assert.AreEqual(RemoveNewline(expected), RemoveNewline(result));
    
            statement.Dispose();
        }
    
        private void RunAssertionEnquote() {
            var testdata = new string[][]{
                new string[]{"\t", "\"\\t\""},
                new string[]{"\n", "\"\\n\""},
                new string[]{"\r", "\"\\r\""},
                new string[]{"\0", "\"\\u0000\""},
            };
    
            for (int i = 0; i < testdata.Length; i++) {
                var buf = new StringBuilder();
                OutputValueRendererJSONString.Enquote(testdata[i][0], buf);
                Assert.AreEqual(testdata[i][1], buf.ToString());
            }
        }
    
        private string RemoveNewline(string text) {
            return text.RegexReplaceAll("\\s\\s+|\\n|\\r", " ").Trim();
        }
    
        public class EmptyMapEvent {
            [PropertyName("props")]
            public IDictionary<string, string> Props { get; }
            public EmptyMapEvent(IDictionary<string, string> props) {
                this.Props = props;
            }
        }
    }
} // end of namespace
