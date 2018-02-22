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
using com.espertech.esper.compat.logging;
using com.espertech.esper.events.util;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.events.render
{
    using Map = IDictionary<string, object>;

    public class ExecEventRenderXML : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.ViewResources.IsIterableUnbound = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionRenderSimple(epService);
            RunAssertionMapAndNestedArray(epService);
            RunAssertionSQLDate(epService);
            RunAssertionEnquote();
        }
    
        private void RunAssertionRenderSimple(EPServiceProvider epService) {
            var bean = new SupportBean();
            bean.TheString = "a\nc";
            bean.IntPrimitive = 1;
            bean.IntBoxed = 992;
            bean.CharPrimitive = 'x';
            bean.EnumValue = SupportEnum.ENUM_VALUE_2;
    
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            EPStatement statement = epService.EPAdministrator.CreateEPL("select * from SupportBean");
            epService.EPRuntime.SendEvent(bean);
    
            string result = epService.EPRuntime.EventRenderer.RenderXML("supportBean", statement.First());
            //Log.Info(result);
            string expected = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                    "<supportBean>\n" +
                    "  <boolPrimitive>false</boolPrimitive>\n" +
                    "  <bytePrimitive>0</bytePrimitive>\n" +
                    "  <charPrimitive>x</charPrimitive>\n" +
                    "  <doublePrimitive>0.0</doublePrimitive>\n" +
                    "  <enumValue>ENUM_VALUE_2</enumValue>\n" +
                    "  <floatPrimitive>0.0</floatPrimitive>\n" +
                    "  <intBoxed>992</intBoxed>\n" +
                    "  <intPrimitive>1</intPrimitive>\n" +
                    "  <longPrimitive>0</longPrimitive>\n" +
                    "  <shortPrimitive>0</shortPrimitive>\n" +
                    "  <theString>a\\u000ac</theString>\n" +
                    "  <this>\n" +
                    "    <boolPrimitive>false</boolPrimitive>\n" +
                    "    <bytePrimitive>0</bytePrimitive>\n" +
                    "    <charPrimitive>x</charPrimitive>\n" +
                    "    <doublePrimitive>0.0</doublePrimitive>\n" +
                    "    <enumValue>ENUM_VALUE_2</enumValue>\n" +
                    "    <floatPrimitive>0.0</floatPrimitive>\n" +
                    "    <intBoxed>992</intBoxed>\n" +
                    "    <intPrimitive>1</intPrimitive>\n" +
                    "    <longPrimitive>0</longPrimitive>\n" +
                    "    <shortPrimitive>0</shortPrimitive>\n" +
                    "    <theString>a\\u000ac</theString>\n" +
                    "  </this>\n" +
                    "</supportBean>";
            Assert.AreEqual(RemoveNewline(expected), RemoveNewline(result));
    
            result = epService.EPRuntime.EventRenderer.RenderXML("supportBean", statement.First(), new XMLRenderingOptions() { IsDefaultAsAttribute = true });
            // Log.Info(result);
            expected = "<?xml version=\"1.0\" encoding=\"UTF-8\"?> <supportBean boolPrimitive=\"false\" bytePrimitive=\"0\" charPrimitive=\"x\" doublePrimitive=\"0.0\" enumValue=\"ENUM_VALUE_2\" floatPrimitive=\"0.0\" intBoxed=\"992\" intPrimitive=\"1\" longPrimitive=\"0\" shortPrimitive=\"0\" theString=\"a\\u000ac\"> <this boolPrimitive=\"false\" bytePrimitive=\"0\" charPrimitive=\"x\" doublePrimitive=\"0.0\" enumValue=\"ENUM_VALUE_2\" floatPrimitive=\"0.0\" intBoxed=\"992\" intPrimitive=\"1\" longPrimitive=\"0\" shortPrimitive=\"0\" theString=\"a\\u000ac\"/> </supportBean>";
            Assert.AreEqual(RemoveNewline(expected), RemoveNewline(result));
    
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
            dataInner.Put("stringarr", new string[]{"a", null});
            dataInner.Put("prop1", "");
            var dataArrayOne = new LinkedHashMap<string, Object>();
            dataArrayOne.Put("stringarr", new string[0]);
            dataArrayOne.Put("prop1", "abcdef");
            var dataArrayTwo = new LinkedHashMap<string, Object>();
            dataArrayTwo.Put("stringarr", new string[]{"R&R", "a>b"});
            dataArrayTwo.Put("prop1", "");
            var dataArrayThree = new LinkedHashMap<string, Object>();
            dataArrayOne.Put("stringarr", null);
            var dataOuter = new LinkedHashMap<string, Object>();
            dataOuter.Put("intarr", new int[]{1, 2});
            dataOuter.Put("innersimple", dataInner);
            dataOuter.Put("innerarray", new Map[]{dataArrayOne, dataArrayTwo, dataArrayThree});
            dataOuter.Put("prop0", new SupportBean_A("A1"));
            epService.EPRuntime.SendEvent(dataOuter, "OuterMap");
    
            string result = epService.EPRuntime.EventRenderer.RenderXML("outerMap", statement.First());
            // Log.Info(result);
            string expected = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
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
    
            result = epService.EPRuntime.EventRenderer.RenderXML("outerMap xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"", statement.First(), new XMLRenderingOptions() { IsDefaultAsAttribute = true });
            // Log.Info(result);
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
    
            statement.Dispose();
        }
    
        private void RunAssertionSQLDate(EPServiceProvider epService) {
            // ESPER-469
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            EPStatement statement = epService.EPAdministrator.CreateEPL("select System.DateTime.Parse(\"2010-01-31\") as mySqlDate from SupportBean");
            epService.EPRuntime.SendEvent(new SupportBean());
    
            EventBean theEvent = statement.First();
            Assert.AreEqual(DateTime.Parse("2010-01-31"), theEvent.Get("mySqlDate"));
            EventPropertyGetter getter = statement.EventType.GetGetter("mySqlDate");
            Assert.AreEqual(DateTime.Parse("2010-01-31"), getter.Get(theEvent));
    
            string result = epService.EPRuntime.EventRenderer.RenderXML("testsqldate", theEvent);
    
            // Log.Info(result);
            string expected = "<?xml version=\"1.0\" encoding=\"UTF-8\"?> <testsqldate> <mySqlDate>2010-01-31</mySqlDate> </testsqldate>";
            Assert.AreEqual(RemoveNewline(expected), RemoveNewline(result));
    
            statement.Dispose();
        }
    
        private void RunAssertionEnquote() {
            var testdata = new string[][]{
                    new string[] {"\"", "&quot;"},
                    new string[] {"'", "&apos;"},
                    new string[] {"&", "&amp;"},
                    new string[] {"<", "&lt;"},
                    new string[] {">", "&gt;"},
                    new string[] {"\0", "\\u0000"},
            };
    
            for (int i = 0; i < testdata.Length; i++) {
                var buf = new StringBuilder();
                OutputValueRendererXMLString.XmlEncode(testdata[i][0], buf, true);
                Assert.AreEqual(testdata[i][1], buf.ToString());
            }
        }
    
        private string RemoveNewline(string text) {
            return text.RegexReplaceAll("\\s\\s+|\\n|\\r", " ").Trim();
        }
    }
} // end of namespace
