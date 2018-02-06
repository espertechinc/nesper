///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

using Avro.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regression.events;
using com.espertech.esper.util.support;

using NUnit.Framework;

namespace com.espertech.esper.supportregression.events
{
	public class SupportEventInfra
    {
	    public const string MAP_TYPENAME = "MyMapEvent";
	    public const string OA_TYPENAME = "MyObjectArrayEvent";
	    public const string XML_TYPENAME = "MyXMLEvent";
	    public const string AVRO_TYPENAME = "MyAvroEvent";

	    public static void AssertValuesMayConvert(EventBean eventBean, string[] propertyNames, ValueWithExistsFlag[] expected, Func<object, object> optionalValueConversion)
        {
	        SupportEventTypeAssertionUtil.AssertConsistency(eventBean);
	        var receivedValues = new object[propertyNames.Length];
	        var expectedValues = new object[propertyNames.Length];
	        for (var i = 0; i < receivedValues.Length; i++) {
	            var value = eventBean.Get(propertyNames[i]);
	            if (optionalValueConversion != null) {
	                value = optionalValueConversion.Invoke(value);
	            }
	            receivedValues[i] = value;
	            expectedValues[i] = expected[i].GetValue();
	        }
	        EPAssertionUtil.AssertEqualsExactOrder(expectedValues, receivedValues);

	        for (var i = 0; i < receivedValues.Length; i++) {
	            var exists = (bool) eventBean.Get("exists_" + propertyNames[i]);
	            Assert.AreEqual(expected[i].IsExists(), exists, "Assertion failed for property 'exists_" + propertyNames[i] + "'");
	        }
	    }

	    public static void AssertValueMayConvert(EventBean eventBean, string propertyName, ValueWithExistsFlag expected, Func<object, object> optionalValueConversion)
        {
	        SupportEventTypeAssertionUtil.AssertConsistency(eventBean);
	        var value = eventBean.Get(propertyName);
	        if (optionalValueConversion != null) {
	            value = optionalValueConversion.Invoke(value);
	        }
	        Assert.AreEqual(expected.GetValue(), value);
	        Assert.AreEqual(expected.IsExists(), eventBean.Get("exists_" + propertyName));
	    }

	    public  static LinkedHashMap<string, object> TwoEntryMap(string keyOne, object valueOne, string keyTwo, object valueTwo)
        {
	        var map = new LinkedHashMap<string, object>();
	        map.Put(keyOne, valueOne);
	        map.Put(keyTwo, valueTwo);
	        return map;
	    }

	    public static readonly FunctionSendEvent FMAP =
	        (epService, @event) => epService.EPRuntime.SendEvent((IDictionary<string, object>) @event, MAP_TYPENAME);

	    public static readonly FunctionSendEvent FOA = (epService, @event) =>
	        epService.EPRuntime.SendEvent((object[]) @event, OA_TYPENAME);

	    public static readonly FunctionSendEvent FBEAN = (epService, @event) =>
	        epService.EPRuntime.SendEvent(@event);

	    public static readonly FunctionSendEvent FAVRO = (epService, @event) => {
	        var record = (GenericRecord) @event;
	        //GenericData.Get().Validate(record.Schema, record);
	        epService.EPRuntime.SendEventAvro(@event, AVRO_TYPENAME);
	    };

	    public static readonly FunctionSendEventWType FMAPWTYPE =
	        (epService, @event, typeName) => epService.EPRuntime.SendEvent((IDictionary<string, object>) @event, typeName);

	    public static readonly FunctionSendEventWType FOAWTYPE = (epService, @event, typeName) =>
	        epService.EPRuntime.SendEvent((object[]) @event, typeName);

	    public static readonly FunctionSendEventWType FBEANWTYPE = (epService, @event, typeName) =>
	        epService.EPRuntime.SendEvent(@event);

	    public static readonly FunctionSendEventWType FAVROWTYPE = (epService, @event, typeName) => {
	        var record = (GenericRecord) @event;
	        //GenericData.Get().Validate(record.Schema, record);
	        epService.EPRuntime.SendEventAvro(@event, typeName);
	    };

	    public static readonly FunctionSendEvent FXML = (epService, @event) => {
	        string xml;
	        if (@event.ToString().Contains("<myevent")) {
	            xml = @event.ToString();
	        } else {
	            xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
	            "<myevent>\n" +
	            "  " + @event + "\n" +
	            "</myevent>\n";
	        }
	        try {
	            SupportXML.SendEvent(epService.EPRuntime, xml);
	        } catch (Exception e) {
	            throw new EPException(e);
	        }
	    };

	    public static Func<object, object> XML_TO_VALUE = (@in) =>
	    {
	        if (@in == null)
	        {
	            return null;
	        }
	        else if (@in is XmlAttribute)
	        {
	            return ((XmlAttribute) @in).Value;
	        }
            else if (@in is XAttribute)
            {
                return ((XAttribute) @in).Value;
            }
            else if (@in is XmlNode)
            {
                return ((XmlNode)@in).InnerText;
            }
            else if (@in is XElement)
            {
                String.Concat(((XElement) @in).Nodes());
	        }
	        return "unknown xml value";
	    };
	}

    public delegate void FunctionSendEvent(EPServiceProvider epService, object value);
    public delegate void FunctionSendEventWType(EPServiceProvider epService, object value, string typeName);
} // end of namespace
