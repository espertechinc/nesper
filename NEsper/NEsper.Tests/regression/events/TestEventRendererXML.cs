///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    using Map = IDictionary<string, object>;

    [TestFixture]
    public class TestEventRendererXML
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
            bean.TheString = "a\nc";
            bean.IntPrimitive = 1;
            bean.IntBoxed = 992;
            bean.CharPrimitive = 'x';
            bean.EnumValue = SupportEnum.ENUM_VALUE_2;

            _epService.EPAdministrator.Configuration.AddEventType("SupportBean", typeof(SupportBean));
            EPStatement statement = _epService.EPAdministrator.CreateEPL("select * from SupportBean");
            _epService.EPRuntime.SendEvent(bean);

            String result = _epService.EPRuntime.EventRenderer.RenderXML("supportBean", statement.FirstOrDefault());
            //Console.Out.WriteLine(result);
            String expected = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                    "<supportBean>\n" +
                    "  <BoolPrimitive>false</BoolPrimitive>\n" +
                    "  <BytePrimitive>0</BytePrimitive>\n" +
                    "  <CharPrimitive>x</CharPrimitive>\n" +
                    "  <DoublePrimitive>0.0</DoublePrimitive>\n" +
                    "  <EnumValue>ENUM_VALUE_2</EnumValue>\n" +
                    "  <FloatPrimitive>0.0</FloatPrimitive>\n" +
                    "  <IntBoxed>992</IntBoxed>\n" +
                    "  <IntPrimitive>1</IntPrimitive>\n" +
                    "  <LongPrimitive>0</LongPrimitive>\n" +
                    "  <ShortPrimitive>0</ShortPrimitive>\n" +
                    "  <TheString>a\\u000ac</TheString>\n" +
                    "  <This>\n" +
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
                    "  </This>\n" +
                    "</supportBean>";
            Assert.AreEqual(RemoveNewline(expected), RemoveNewline(result));

            result = _epService.EPRuntime.EventRenderer.RenderXML("supportBean", statement.FirstOrDefault(), new XMLRenderingOptions { IsDefaultAsAttribute = true });
            // Console.Out.WriteLine(result);
            expected = "<?xml version=\"1.0\" encoding=\"UTF-8\"?> <supportBean BoolPrimitive=\"false\" BytePrimitive=\"0\" CharPrimitive=\"x\" DoublePrimitive=\"0.0\" EnumValue=\"ENUM_VALUE_2\" FloatPrimitive=\"0.0\" IntBoxed=\"992\" IntPrimitive=\"1\" LongPrimitive=\"0\" ShortPrimitive=\"0\" TheString=\"a\\u000ac\"> <This BoolPrimitive=\"false\" BytePrimitive=\"0\" CharPrimitive=\"x\" DoublePrimitive=\"0.0\" EnumValue=\"ENUM_VALUE_2\" FloatPrimitive=\"0.0\" IntBoxed=\"992\" IntPrimitive=\"1\" LongPrimitive=\"0\" ShortPrimitive=\"0\" TheString=\"a\\u000ac\"/> </supportBean>";
            Assert.AreEqual(RemoveNewline(expected), RemoveNewline(result));
        }

        [Test]
        public void TestMapAndNestedArray()
        {
            IDictionary<String, Object> defOuter = new LinkedHashMap<String, Object>();
            defOuter["intarr"] = typeof(int[]);
            defOuter["innersimple"] = "InnerMap";
            defOuter["innerarray"] = "InnerMap[]";
            defOuter["prop0"] = typeof(SupportBean_A);

            IDictionary<String, Object> defInner = new LinkedHashMap<String, Object>();
            defInner["stringarr"] = typeof(String[]);
            defInner["prop1"] = typeof(string);

            _epService.EPAdministrator.Configuration.AddEventType("InnerMap", defInner);
            _epService.EPAdministrator.Configuration.AddEventType("OuterMap", defOuter);
            EPStatement statement = _epService.EPAdministrator.CreateEPL("select * from OuterMap");

            IDictionary<String, Object> dataInner = new LinkedHashMap<String, Object>();
            dataInner["stringarr"] = new String[] { "a", null };
            dataInner["prop1"] = "";
            IDictionary<String, Object> dataArrayOne = new LinkedHashMap<String, Object>();
            dataArrayOne["stringarr"] = new String[0];
            dataArrayOne["prop1"] = "abcdef";
            IDictionary<String, Object> dataArrayTwo = new LinkedHashMap<String, Object>();
            dataArrayTwo["stringarr"] = new String[] { "R&R", "a>b" };
            dataArrayTwo["prop1"] = "";
            IDictionary<String, Object> dataArrayThree = new LinkedHashMap<String, Object>();
            dataArrayOne["stringarr"] = null;
            IDictionary<String, Object> dataOuter = new LinkedHashMap<String, Object>();
            dataOuter["intarr"] = new int[] { 1, 2 };
            dataOuter["innersimple"] = dataInner;
            dataOuter["innerarray"] = new Map[] { dataArrayOne, dataArrayTwo, dataArrayThree };
            dataOuter["prop0"] = new SupportBean_A("A1");
            _epService.EPRuntime.SendEvent(dataOuter, "OuterMap");

            String result = _epService.EPRuntime.EventRenderer.RenderXML("outerMap", statement.FirstOrDefault());
            // Console.Out.WriteLine(result);
            String expected = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                    "<outerMap>\n" +
                    "  <intarr>1</intarr>\n" +
                    "  <intarr>2</intarr>\n" +
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
                    "  <innersimple>\n" +
                    "    <prop1></prop1>\n" +
                    "    <stringarr>a</stringarr>\n" +
                    "  </innersimple>\n" +
                    "  <prop0>\n" +
                    "    <Id>A1</Id>\n" +
                    "  </prop0>\n" +
                    "</outerMap>";
            Assert.AreEqual(RemoveNewline(expected), RemoveNewline(result));

            result =
                _epService.EPRuntime.EventRenderer.RenderXML(
                    "outerMap xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"", statement.FirstOrDefault(),
                    new XMLRenderingOptions { IsDefaultAsAttribute = true });
            // Console.Out.WriteLine(result);
            expected = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                    "<outerMap xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">\n" +
                    "  <intarr>1</intarr>\n" +
                    "  <intarr>2</intarr>\n" +
                    "  <innerarray prop1=\"abcdef\"/>\n" +
                    "  <innerarray prop1=\"\">\n" +
                    "    <stringarr>R&amp;R</stringarr>\n" +
                    "    <stringarr>a&gt;b</stringarr>\n" +
                    "  </innerarray>\n" +
                    "  <innerarray/>\n" +
                    "  <innersimple prop1=\"\">\n" +
                    "    <stringarr>a</stringarr>\n" +
                    "  </innersimple>\n" +
                    "  <prop0 Id=\"A1\"/>\n" +
                    "</outerMap>";
            Assert.AreEqual(RemoveNewline(expected), RemoveNewline(result));
        }

        [Test]
        public void TestSQLDate()
        {
            // ESPER-469
            _epService.EPAdministrator.Configuration.AddEventType("SupportBean", typeof(SupportBean));
            EPStatement statement = _epService.EPAdministrator.CreateEPL("select DateTime.Parse(\"2010-01-31\") as mySqlDate from SupportBean");
            _epService.EPRuntime.SendEvent(new SupportBean());

            EventBean theEvent = statement.FirstOrDefault();
            Assert.AreEqual(DateTime.Parse("2010-01-31"), theEvent.Get("mySqlDate"));
            EventPropertyGetter getter = statement.EventType.GetGetter("mySqlDate");
            Assert.AreEqual(DateTime.Parse("2010-01-31"), getter.Get(theEvent));

            String result = _epService.EPRuntime.EventRenderer.RenderXML("testsqldate", theEvent);

            // Console.Out.WriteLine(result);
            String expected = "<?xml version=\"1.0\" encoding=\"UTF-8\"?> <testsqldate> <mySqlDate>2010-01-31</mySqlDate> </testsqldate>";
            Assert.AreEqual(RemoveNewline(expected), RemoveNewline(result));
        }

        public static void TestEnquote()
        {
            var testdata = new[]
            {
                    new[] {"\"", "&quot;"},
                    new[]{"'", "&apos;"},
                    new[]{"&", "&amp;"},
                    new[]{"<", "&lt;"},
                    new[]{">", "&gt;"},
                    new[]{Char.ToString((char)0), "\\u0000"},
            };

            for (int i = 0; i < testdata.Length; i++)
            {
                StringBuilder buf = new StringBuilder();
                OutputValueRendererXMLString.XmlEncode(testdata[i][0], buf, true);
                Assert.AreEqual(testdata[i][1], buf.ToString());
            }
        }

        private String RemoveNewline(String text)
        {
            return text.RegexReplaceAll("\\s\\s+|\\n|\\r", " ").Trim();
        }
    }
}
