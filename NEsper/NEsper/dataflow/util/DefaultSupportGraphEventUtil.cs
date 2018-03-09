///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.core.service;
using com.espertech.esper.events;
using com.espertech.esper.events.arr;
using com.espertech.esper.events.bean;
using com.espertech.esper.events.map;
using com.espertech.esper.events.xml;

namespace com.espertech.esper.dataflow.util
{
    public class DefaultSupportGraphEventUtil
    {
        public static readonly String CLASSLOADER_SCHEMA_URI = "regression/threeProperties.xsd";

        public static void AddTypeConfiguration(EPServiceProviderSPI epService)
        {
            var propertyTypes = new LinkedHashMap<String, Object>();
            propertyTypes.Put("myDouble", typeof(Double));
            propertyTypes.Put("myInt", typeof(int));
            propertyTypes.Put("myString", typeof(String));
            epService.EPAdministrator.Configuration.AddEventType("MyMapEvent", propertyTypes);
            epService.EPAdministrator.Configuration.AddEventType("MyOAEvent", "myDouble,myInt,myString".Split(','), new Object[] { typeof(double), typeof(int), typeof(string) });
            epService.EPAdministrator.Configuration.AddEventType(typeof(MyEvent));
            epService.EPAdministrator.Configuration.AddEventType("MyXMLEvent", GetConfig(
                epService.ServicesContext.ResourceManager));
        }

        public static SendableEvent[] XMLEventsSendable
        {
            get
            {
                Object[] xmlEvents = XMLEvents;
                SendableEvent[] xmls = new SendableEvent[xmlEvents.Length];
                for (int i = 0; i < xmlEvents.Length; i++)
                {
                    xmls[i] = new SendableEventXML((XmlNode) xmlEvents[i]);
                }
                return xmls;
            }
        }

        public static SendableEvent[] OAEventsSendable
        {
            get
            {
                Object[] oaEvents = OAEvents;
                SendableEvent[] oas = new SendableEvent[oaEvents.Length];
                for (int i = 0; i < oaEvents.Length; i++)
                {
                    oas[i] = new SendableEventObjectArray((Object[]) oaEvents[i], "MyOAEvent");
                }
                return oas;
            }
        }

        public static SendableEvent[] MapEventsSendable
        {
            get
            {
                Object[] mapEvents = MapEvents;
                SendableEvent[] sendables = new SendableEvent[mapEvents.Length];
                for (int i = 0; i < mapEvents.Length; i++)
                {
                    sendables[i] = new SendableEventMap((IDictionary<String, Object>) mapEvents[i], "MyMapEvent");
                }
                return sendables;
            }
        }

        public static SendableEvent[] PonoEventsSendable
        {
            get
            {
                Object[] ponoEvents = PonoEvents;
                SendableEvent[] sendables = new SendableEvent[ponoEvents.Length];
                for (int i = 0; i < ponoEvents.Length; i++)
                {
                    sendables[i] = new SendableEventBean(ponoEvents[i]);
                }
                return sendables;
            }
        }

        public static object[] XMLEvents
        {
            get { return new Object[] {MakeXMLEvent(1.1d, 1, "one"), MakeXMLEvent(2.2d, 2, "two")}; }
        }

        public static object[] OAEvents
        {
            get { return new Object[] {new Object[] {1.1d, 1, "one"}, new Object[] {2.2d, 2, "two"}}; }
        }

        public static object[] MapEvents
        {
            get { return new Object[] {MakeMapEvent(1.1, 1, "one"), MakeMapEvent(2.2d, 2, "two")}; }
        }

        public static object[] PonoEvents
        {
            get { return new Object[] {new MyEvent(1.1d, 1, "one"), new MyEvent(2.2d, 2, "two")}; }
        }

        private static ConfigurationEventTypeXMLDOM GetConfig(IResourceManager resourceManager)
        {
            ConfigurationEventTypeXMLDOM eventTypeMeta = new ConfigurationEventTypeXMLDOM();
            eventTypeMeta.RootElementName = "rootelement";
            var schemaStream = resourceManager.GetResourceAsStream(CLASSLOADER_SCHEMA_URI);
            if (schemaStream == null)
            {
                throw new IllegalStateException("Failed to load schema '" + CLASSLOADER_SCHEMA_URI + "'");
            }

            var reader = new StreamReader(schemaStream);
            var schemaText = reader.ReadToEnd();
            eventTypeMeta.SchemaText = schemaText;
            return eventTypeMeta;
        }

        private static XmlNode MakeXMLEvent(double myDouble, int myInt, String myString)
        {
            String xml = "<rootelement myDouble=\"VAL_DBL\" myInt=\"VAL_INT\" myString=\"VAL_STR\" />";
            xml = xml.Replace("VAL_DBL", Convert.ToString(myDouble));
            xml = xml.Replace("VAL_INT", Convert.ToString(myInt));
            xml = xml.Replace("VAL_STR", myString);

            try
            {
                XmlDocument document = new XmlDocument();
                document.LoadXml(xml);
                return document.DocumentElement;
            }
            catch (Exception e)
            {
                throw new Exception("Failed to parse '" + xml + "' as XML: " + e.Message, e);
            }
        }

        private static IDictionary<String, Object> MakeMapEvent(double myDouble, int myInt, String myString)
        {
            IDictionary<String, Object> map = new Dictionary<String, Object>();
            map.Put("myDouble", myDouble);
            map.Put("myInt", myInt);
            map.Put("myString", myString);
            return map;
        }

        public class MyEvent
        {
            public int MyInt { get; private set; }

            public double MyDouble { get; private set; }

            public string MyString { get; private set; }

            public MyEvent(double myDouble, int myInt, String myString)
            {
                MyDouble = myDouble;
                MyInt = myInt;
                MyString = myString;
            }
        }
    }
}
