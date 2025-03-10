///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Xml;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.util
{
    public class DOMUtil
    {
        public static void ParseRequiredBoolean(
            XmlElement element,
            string name,
            Consumer<bool> func)
        {
            var str = GetRequiredAttribute(element, name);
            var b = ParseBoolean(name, str);
            func.Invoke(b);
        }

        public static void ParseOptionalBoolean(
            XmlElement element,
            string name,
            Consumer<bool> func)
        {
            var str = GetOptionalAttribute(element, name);
            if (str != null) {
                var b = ParseBoolean(name, str);
                func.Invoke(b);
            }
        }

        public static void ParseOptionalInteger(
            XmlElement element,
            string name,
            Consumer<int> func)
        {
            var str = GetOptionalAttribute(element, name);
            if (str != null) {
                func.Invoke(ParseInteger(name, str));
            }
        }

        public static void ParseRequiredAttribute(
            XmlElement element,
            string name,
            Consumer<string> func)
        {
            var str = GetRequiredAttribute(element, name);
            if (str != null) {
                func.Invoke(str);
            }
        }

        public static string GetRequiredAttribute(
            XmlNode node,
            string key)
        {
            var valueNode = node?.Attributes?.GetNamedItem(key);
            if (valueNode == null) {
                var name = node.LocalName;
                throw new ConfigurationException(
                    $"Required attribute by name '{key}' not found for element '{name}'");
            }

            return valueNode.InnerText;
        }

        public static string GetOptionalAttribute(
            XmlNode node,
            string key)
        {
            var valueNode = node?.Attributes?.GetNamedItem(key);
            return valueNode?.InnerText;
        }

        public static Properties GetProperties(
            XmlElement element,
            string propElementName)
        {
            var properties = new Properties();
            foreach (var subElement in DOMElementEnumerator.For(element.ChildNodes)) {
                if (subElement.Name.Equals(propElementName)) {
                    var name = GetRequiredAttribute(subElement, "name");
                    var value = GetRequiredAttribute(subElement, "value");
                    properties.Put(name, value);
                }
            }

            return properties;
        }

        private static bool ParseBoolean(
            string name,
            string str)
        {
            try {
                return bool.Parse(str);
            }
            catch (Exception t) {
                throw new ConfigurationException(
                    $"Failed to parse value for '{name}' value '{str}' as boolean: {t.Message}",
                    t);
            }
        }

        private static int ParseInteger(
            string name,
            string str)
        {
            try {
                return int.Parse(str);
            }
            catch (Exception t) {
                throw new ConfigurationException(
                    $"Failed to parse value for '{name}' value '{str}' as integer: {t.Message}",
                    t);
            }
        }
    }
} // end of namespace