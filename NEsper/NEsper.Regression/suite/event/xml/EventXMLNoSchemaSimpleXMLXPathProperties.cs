///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.support.util.SupportXML;

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
    public class EventXMLNoSchemaSimpleXMLXPathProperties : RegressionExecution
    {
        protected const string XML_NOSCHEMAEVENT =
            "<myevent>\n" +
            "  <element1>VAL1</element1>\n" +
            "  <element2>\n" +
            "    <element21 id=\"e21_1\">VAL21-1</element21>\n" +
            "    <element21 id=\"e21_2\">VAL21-2</element21>\n" +
            "  </element2>\n" +
            "  <element3 attrString=\"VAL3\" attrNum=\"5\" attrBool=\"true\"/>\n" +
            "  <element4><element41>VAL4-1</element41></element4>\n" +
            "</myevent>";

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void Run(RegressionEnvironment env)
        {
            // assert type metadata
            var type = env.Runtime.EventTypeService.GetEventTypePreconfigured("TestXMLNoSchemaTypeWMoreXPath");
            Assert.AreEqual(EventTypeApplicationType.XML, type.Metadata.ApplicationType);

            EPAssertionUtil.AssertEqualsAnyOrder(
                new object[] {
                    new EventPropertyDescriptor(
                        "xpathElement1",
                        typeof(string),
                        null,
                        false,
                        false,
                        false,
                        false,
                        false),
                    new EventPropertyDescriptor(
                        "xpathCountE21",
                        typeof(double?),
                        null,
                        false,
                        false,
                        false,
                        false,
                        false),
                    new EventPropertyDescriptor(
                        "xpathAttrString",
                        typeof(string),
                        null,
                        false,
                        false,
                        false,
                        false,
                        false),
                    new EventPropertyDescriptor(
                        "xpathAttrNum",
                        typeof(double?),
                        null,
                        false,
                        false,
                        false,
                        false,
                        false),
                    new EventPropertyDescriptor(
                        "xpathAttrBool",
                        typeof(bool?),
                        null,
                        false,
                        false,
                        false,
                        false,
                        false),
                    new EventPropertyDescriptor(
                        "stringCastLong",
                        typeof(long?),
                        null,
                        false,
                        false,
                        false,
                        false,
                        false),
                    new EventPropertyDescriptor(
                        "stringCastDouble",
                        typeof(double?),
                        null,
                        false,
                        false,
                        false,
                        false,
                        false),
                    new EventPropertyDescriptor("numCastInt", typeof(int?), null, false, false, false, false, false)
                },
                type.PropertyDescriptors);

            var stmt =
                "@Name('s0') select xpathElement1, xpathCountE21, xpathAttrString, xpathAttrNum, xpathAttrBool," +
                "stringCastLong," +
                "stringCastDouble," +
                "numCastInt " +
                "from TestXMLNoSchemaTypeWMoreXPath#length(100)";
            env.CompileDeploy(stmt).AddListener("s0");

            // Generate document with the specified in element1 to confirm we have independent events
            SendEvent(env, "EventA", "TestXMLNoSchemaTypeWMoreXPath");
            AssertDataSimpleXPath(env, "EventA");

            SendEvent(env, "EventB", "TestXMLNoSchemaTypeWMoreXPath");
            AssertDataSimpleXPath(env, "EventB");

            env.UndeployAll();
        }

        internal static void AssertDataSimpleXPath(
            RegressionEnvironment env,
            string element1)
        {
            Assert.IsNotNull(env.Listener("s0").LastNewData);
            var theEvent = env.Listener("s0").LastNewData[0];

            Assert.AreEqual(element1, theEvent.Get("xpathElement1"));
            Assert.AreEqual(2.0, theEvent.Get("xpathCountE21"));
            Assert.AreEqual("VAL3", theEvent.Get("xpathAttrString"));
            Assert.AreEqual(5d, theEvent.Get("xpathAttrNum"));
            Assert.AreEqual(true, theEvent.Get("xpathAttrBool"));
            Assert.AreEqual(5L, theEvent.Get("stringCastLong"));
            Assert.AreEqual(5d, theEvent.Get("stringCastDouble"));
            Assert.AreEqual(5, theEvent.Get("numCastInt"));
        }

        public static void SendEvent(
            RegressionEnvironment env,
            string value,
            string typeName)
        {
            var xml = XML_NOSCHEMAEVENT.Replace("VAL1", value);
            log.Debug(".SendEvent value=" + value);
            SendXMLEvent(env, xml, typeName);
        }
    }
} // end of namespace