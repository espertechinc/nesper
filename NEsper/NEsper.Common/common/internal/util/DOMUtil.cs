///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Xml;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.util
{
	public class DOMUtil {
	    public static void ParseRequiredBoolean(XmlElement element, string name, Consumer<Boolean> func) {
	        string str = GetRequiredAttribute(element, name);
	        bool b = ParseBoolean(name, str);
	        func.Accept(b);
	    }

	    public static void ParseOptionalBoolean(XmlElement element, string name, Consumer<Boolean> func) {
	        string str = GetOptionalAttribute(element, name);
	        if (str != null) {
	            bool b = ParseBoolean(name, str);
	            func.Accept(b);
	        }
	    }

	    public static string GetRequiredAttribute(XmlNode node, string key) {
	        XmlNode valueNode = node.Attributes.GetNamedItem(key);
	        if (valueNode == null) {
	            string name = node.LocalName;
	            if (name == null) {
	                name = node.NodeName;
	            }
	            throw new ConfigurationException("Required attribute by name '" + key + "' not found for element '" + name + "'");
	        }
	        return valueNode.TextContent;
	    }

	    public static string GetOptionalAttribute(XmlNode node, string key) {
	        XmlNode valueNode = node.Attributes.GetNamedItem(key);
	        if (valueNode != null) {
	            return valueNode.TextContent;
	        }
	        return null;
	    }

	    public static Properties GetProperties(XmlElement element, string propElementName) {
	        Properties properties = new Properties();
	        DOMElementEnumerator nodeEnumerator = new DOMElementEnumerator(element.ChildNodes);
	        while (nodeEnumerator.HasNext) {
	            XmlElement subElement = nodeEnumerator.Next();
	            if (subElement.NodeName.Equals(propElementName)) {
	                string name = GetRequiredAttribute(subElement, "name");
	                string value = GetRequiredAttribute(subElement, "value");
	                properties.Put(name, value);
	            }
	        }
	        return properties;
	    }

	    private static bool ParseBoolean(string name, string str) {
	        try {
	            return Boolean.Parse(str);
	        } catch (Throwable t) {
	            throw new ConfigurationException("Failed to parse value for '" + name + "' value '" + str + "' as boolean: " + t.Message, t);
	        }
	    }
	}
} // end of namespace