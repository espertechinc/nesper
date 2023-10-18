///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.IO;
using System.Xml;

using com.espertech.esper.common.client;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client;

namespace com.espertech.esper.regressionlib.support.util
{
    public class SupportXML
    {
        private const string XML =
            "<simpleEvent xmlns=\"samples:schemas:simpleSchema\" xmlns:ss=\"samples:schemas:simpleSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"samples:schemas:simpleSchema\n" +
            "simpleSchema.xsd\">\n" +
            "\t<nested1 attr1=\"SAMPLE_ATTR1\">\n" +
            "\t\t<prop1>SAMPLE_V1</prop1>\n" +
            "\t\t<prop2>true</prop2>\n" +
            "\t\t<nested2>\n" +
            "\t\t\t<prop3>3</prop3>\n" +
            "\t\t\t<prop3>4</prop3>\n" +
            "\t\t\t<prop3>5</prop3>\n" +
            "\t\t</nested2>\n" +
            "\t</nested1>\n" +
            "\t<prop4 ss:attr2=\"true\">SAMPLE_V6</prop4>\n" +
            "\t<nested3>\n" +
            "\t\t<nested4 id=\"a\">\n" +
            "\t\t\t<prop5>SAMPLE_V7</prop5>\n" +
            "\t\t\t<prop5>SAMPLE_V8</prop5>\n" +
            "\t\t</nested4>\n" +
            "\t\t<nested4 id=\"b\">\n" +
            "\t\t\t<prop5>SAMPLE_V9</prop5>\n" +
            "\t\t</nested4>\n" +
            "\t\t<nested4 id=\"c\">\n" +
            "\t\t\t<prop5>SAMPLE_V10</prop5>\n" +
            "\t\t\t<prop5>SAMPLE_V11</prop5>\n" +
            "\t\t</nested4>\n" +
            "\t</nested3>\n" +
            "</simpleEvent>";

        public static XmlDocument MakeDefaultEvent(string value)
        {
            var xml = XML.Replace("VAL1", value);
            var simpleDoc = new XmlDocument();
            simpleDoc.LoadXml(xml);
            return simpleDoc;
        }

        public static XmlDocument SendEvent(
            EventSender sender,
            string xml)
        {
            var simpleDoc = GetDocument(xml);
            sender.SendEvent(simpleDoc);
            return simpleDoc;
        }

        public static XmlDocument GetDocument()
        {
            return GetDocument(XML);
        }


        public static XmlDocument SendXMLEvent(
            RegressionEnvironment env,
            string xml,
            string typeName)
        {
            var simpleDoc = GetDocument(xml);
            env.SendEventXMLDOM(simpleDoc, typeName);
            return simpleDoc;
        }

        public static XmlDocument GetDocument(string xml)
        {
            var document = new XmlDocument();
            document.LoadXml(xml);
            return document;
        }

        public static XmlDocument GetDocument(Stream stream)
        {
            var document = new XmlDocument();
            document.Load(stream);
            return document;
        }

        public static string Serialize(XmlDocument doc)
        {
            return doc.OuterXml;
        }
    }
} // end of namespace