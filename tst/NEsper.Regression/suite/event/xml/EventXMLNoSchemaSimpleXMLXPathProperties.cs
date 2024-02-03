///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;
using NUnit.Framework.Legacy;
using static com.espertech.esper.regressionlib.support.util.SupportXML;

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
    public class EventXMLNoSchemaSimpleXMLXPathProperties
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

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
#if REGRESSION_EXECUTIONS
            WithPreconfig(execs);
            With(CreateSchema)(execs);
#endif
            return execs;
        }

        public static IList<RegressionExecution> WithCreateSchema(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventXMLNoSchemaSimpleXMLXPathPropertiesCreateSchema());
            return execs;
        }

        public static IList<RegressionExecution> WithPreconfig(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventXMLNoSchemaSimpleXMLXPathPropertiesPreconfig());
            return execs;
        }

        public class EventXMLNoSchemaSimpleXMLXPathPropertiesPreconfig : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunAssertion(env, "TestXMLNoSchemaTypeWMoreXPath", new RegressionPath());
            }
        }

        public class EventXMLNoSchemaSimpleXMLXPathPropertiesCreateSchema : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@public @buseventtype " +
                          "@XMLSchema(RootElementName='myevent'," +
                          "  XPathFunctionResolver='" +
                          typeof(SupportXPathFunctionResolver).MaskTypeName() +
                          "'," +
                          "  XPathVariableResolver='" +
                          typeof(SupportXPathVariableResolver).MaskTypeName() +
                          "')" +
                          "@XMLSchemaField(Name='xpathElement1', XPath='/myevent/element1', Type='STRING')" +
                          "@XMLSchemaField(Name='xpathCountE21', XPath='count(/myevent/element2/element21)', Type='NUMBER')" +
                          "@XMLSchemaField(Name='xpathAttrString', XPath='/myevent/element3/@attrString', Type='STRING')" +
                          "@XMLSchemaField(Name='xpathAttrNum', XPath='/myevent/element3/@attrNum', Type='NUMBER')" +
                          "@XMLSchemaField(Name='xpathAttrBool', XPath='/myevent/element3/@attrBool', Type='BOOLEAN')" +
                          "@XMLSchemaField(Name='stringCastLong', XPath='/myevent/element3/@attrNum', Type='STRING', CastToType='long')" +
                          "@XMLSchemaField(Name='stringCastDouble', XPath='/myevent/element3/@attrNum', Type='STRING', CastToType='double')" +
                          "@XMLSchemaField(Name='numCastInt', XPath='/myevent/element3/@attrNum', Type='NUMBER', CastToType='int')" +
                          "create xml schema MyEventCreateSchema()";
                var path = new RegressionPath();
                env.CompileDeploy(epl, path);
                RunAssertion(env, "MyEventCreateSchema", path);
            }
        }

        private static void RunAssertion(
            RegressionEnvironment env,
            string eventTypeName,
            RegressionPath path)
        {
            // assert type metadata
            env.AssertThat(
                () => {
                    var type = env.Runtime.EventTypeService.GetEventTypePreconfigured(eventTypeName);
                    ClassicAssert.AreEqual(EventTypeApplicationType.XML, type.Metadata.ApplicationType);

                    SupportEventPropUtil.AssertPropsEquals(
                        type.PropertyDescriptors.ToArray(),
                        new SupportEventPropDesc("xpathElement1", typeof(string)),
                        new SupportEventPropDesc("xpathCountE21", typeof(double?)),
                        new SupportEventPropDesc("xpathAttrString", typeof(string)),
                        new SupportEventPropDesc("xpathAttrNum", typeof(double?)),
                        new SupportEventPropDesc("xpathAttrBool", typeof(bool?)),
                        new SupportEventPropDesc("stringCastLong", typeof(long)),
                        new SupportEventPropDesc("stringCastDouble", typeof(double)),
                        new SupportEventPropDesc("numCastInt", typeof(int)));
                });

            var stmt =
                "@name('s0') select xpathElement1, xpathCountE21, xpathAttrString, xpathAttrNum, xpathAttrBool," +
                "stringCastLong," +
                "stringCastDouble," +
                "numCastInt " +
                "from " +
                eventTypeName +
                "#length(100)";
            env.CompileDeploy(stmt, path).AddListener("s0");

            // Generate document with the specified in element1 to confirm we have independent events
            SendEvent(env, "EventA", eventTypeName);
            AssertDataSimpleXPath(env, "EventA");

            SendEvent(env, "EventB", eventTypeName);
            AssertDataSimpleXPath(env, "EventB");

            env.UndeployAll();
        }

        internal static void AssertDataSimpleXPath(
            RegressionEnvironment env,
            string element1)
        {
            env.AssertListener(
                "s0",
                listener => {
                    ClassicAssert.NotNull(listener.LastNewData);
                    var theEvent = listener.LastNewData[0];

                    ClassicAssert.AreEqual(element1, theEvent.Get("xpathElement1"));
                    ClassicAssert.AreEqual(2.0, theEvent.Get("xpathCountE21"));
                    ClassicAssert.AreEqual("VAL3", theEvent.Get("xpathAttrString"));
                    ClassicAssert.AreEqual(5d, theEvent.Get("xpathAttrNum"));
                    ClassicAssert.AreEqual(true, theEvent.Get("xpathAttrBool"));
                    ClassicAssert.AreEqual(5L, theEvent.Get("stringCastLong"));
                    ClassicAssert.AreEqual(5d, theEvent.Get("stringCastDouble"));
                    ClassicAssert.AreEqual(5, theEvent.Get("numCastInt"));
                });
        }

        public static void SendEvent(
            RegressionEnvironment env,
            string value,
            string typeName)
        {
            var xml = XML_NOSCHEMAEVENT.Replace("VAL1", value);
            Log.Debug(".SendEvent value=" + value);
            SendXMLEvent(env, xml, typeName);
        }
    }
} // end of namespace