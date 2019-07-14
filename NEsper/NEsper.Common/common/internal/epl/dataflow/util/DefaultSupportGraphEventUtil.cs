///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.common.@internal.@event.xml;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;

namespace com.espertech.esper.common.@internal.epl.dataflow.util
{
    public class DefaultSupportGraphEventUtil
    {
        public const string CLASSLOADER_SCHEMA_URI = "regression/threeProperties.xsd";

        public static readonly string EVENTTYPENAME = typeof(MyDefaultSupportGraphEvent).GetSimpleName();

        public static void AddTypeConfiguration(Configuration configuration)
        {
            var container = configuration.Container;
            var propertyTypes = new Dictionary<string, object>();
            propertyTypes.Put("myDouble", typeof(double?));
            propertyTypes.Put("myInt", typeof(int?));
            propertyTypes.Put("myString", typeof(string));
            configuration.Common.AddEventType("MyMapEvent", propertyTypes);
            configuration.Common.AddEventType(
                "MyOAEvent",
                "myDouble,myInt,myString".SplitCsv(),
                new object[] {
                    typeof(double?), typeof(int?), typeof(string)
                });
            configuration.Common.AddEventType(typeof(MyDefaultSupportGraphEvent));
            configuration.Common.AddEventType("MyXMLEvent", GetConfig(container.ResourceManager()));
        }

        public static SendableEvent[] GetXMLEventsSendable()
        {
            var xmlEvents = GetXMLEvents();
            var xmls = new SendableEvent[xmlEvents.Length];
            for (var i = 0; i < xmlEvents.Length; i++) {
                xmls[i] = new SendableEventXML((XmlNode) xmlEvents[i], "MyXMLEvent");
            }

            return xmls;
        }

        public static SendableEvent[] GetOAEventsSendable()
        {
            var oaEvents = GetOAEvents();
            var oas = new SendableEvent[oaEvents.Length];
            for (var i = 0; i < oaEvents.Length; i++) {
                oas[i] = new SendableEventObjectArray((object[]) oaEvents[i], "MyOAEvent");
            }

            return oas;
        }

        public static SendableEvent[] GetMapEventsSendable()
        {
            var mapEvents = GetMapEvents();
            var sendables = new SendableEvent[mapEvents.Length];
            for (var i = 0; i < mapEvents.Length; i++) {
                sendables[i] = new SendableEventMap((IDictionary<string, object>) mapEvents[i], "MyMapEvent");
            }

            return sendables;
        }

        public static SendableEvent[] GetPONOEventsSendable()
        {
            var pojoEvents = GetPONOEvents();
            var sendables = new SendableEvent[pojoEvents.Length];
            for (var i = 0; i < pojoEvents.Length; i++) {
                sendables[i] = new SendableEventBean(pojoEvents[i], EVENTTYPENAME);
            }

            return sendables;
        }

        public static object[] GetXMLEvents()
        {
            return new object[] {MakeXMLEvent(1.1d, 1, "one"), MakeXMLEvent(2.2d, 2, "two")};
        }

        public static object[] GetOAEvents()
        {
            return new object[] {new object[] {1.1d, 1, "one"}, new object[] {2.2d, 2, "two"}};
        }

        public static object[] GetMapEvents()
        {
            return new object[] {MakeMapEvent(1.1, 1, "one"), MakeMapEvent(2.2d, 2, "two")};
        }

        public static object[] GetPONOEvents()
        {
            return new object[]
                {new MyDefaultSupportGraphEvent(1.1d, 1, "one"), new MyDefaultSupportGraphEvent(2.2d, 2, "two")};
        }

        private static ConfigurationCommonEventTypeXMLDOM GetConfig(IResourceManager resourceManager)
        {
            var eventTypeMeta = new ConfigurationCommonEventTypeXMLDOM();
            eventTypeMeta.RootElementName = "rootelement";

            var schemaStream = resourceManager.GetResourceAsStream(CLASSLOADER_SCHEMA_URI);
            if (schemaStream == null) {
                throw new IllegalStateException("Failed to load schema '" + CLASSLOADER_SCHEMA_URI + "'");
            }

            var reader = new StreamReader(schemaStream);
            eventTypeMeta.SchemaText = reader.ReadToEnd();
            return eventTypeMeta;
        }

        private static XmlNode MakeXMLEvent(
            double myDouble,
            int myInt,
            string myString)
        {
            var xml = "<rootelement myDouble=\"VAL_DBL\" myInt=\"VAL_INT\" myString=\"VAL_STR\" />";
            xml = xml.RegexReplaceAll("VAL_DBL", Convert.ToString(myDouble));
            xml = xml.RegexReplaceAll("VAL_INT", Convert.ToString(myInt));
            xml = xml.RegexReplaceAll("VAL_STR", myString);

            try {
                var document = new XmlDocument();
                document.LoadXml(xml);
                return document.DocumentElement;
            }
            catch (Exception e) {
                throw new EPRuntimeException("Failed to parse '" + xml + "' as XML: " + e.Message, e);
            }
        }

        private static IDictionary<string, object> MakeMapEvent(
            double myDouble,
            int myInt,
            string myString)
        {
            IDictionary<string, object> map = new Dictionary<string, object>();
            map.Put("myDouble", myDouble);
            map.Put("myInt", myInt);
            map.Put("myString", myString);
            return map;
        }

        public class MyDefaultSupportGraphEvent
        {
            public MyDefaultSupportGraphEvent(
                double myDouble,
                int myInt,
                string myString)
            {
                MyDouble = myDouble;
                MyInt = myInt;
                MyString = myString;
            }

            public int MyInt { get; }

            public double MyDouble { get; }

            public string MyString { get; }
        }
    }
} // end of namespace