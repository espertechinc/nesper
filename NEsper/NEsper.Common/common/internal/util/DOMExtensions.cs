///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Xml;
using com.espertech.esper.common.client.configuration;

namespace com.espertech.esper.common.@internal.util
{
    public static class DOMExtensions
    {
        public static void WhenOptionalAttribute(
            XmlNode node,
            string key,
            Action<string> action)
        {
            var valueNode = node.Attributes.GetNamedItem(key);
            if (valueNode != null) {
                action.Invoke(valueNode.InnerText);
            }
        }

        public static string GetOptionalAttribute(
            XmlNode node,
            string key)
        {
            var valueNode = node.Attributes.GetNamedItem(key);
            if (valueNode != null) {
                return valueNode.InnerText;
            }

            return null;
        }

        public static string GetRequiredAttribute(
            XmlNode node,
            string key)
        {
            var valueNode = node.Attributes.GetNamedItem(key);
            if (valueNode == null) {
                var name = string.IsNullOrEmpty(node.Name)
                    ? node.LocalName
                    : node.Name;
                throw new ConfigurationException(
                    "Required attribute by name '" + key + "' not found for element '" + name + "'");
            }

            return valueNode.InnerText;
        }
    }
}