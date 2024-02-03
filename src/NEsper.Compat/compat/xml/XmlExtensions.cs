///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Xml;

namespace com.espertech.esper.compat.xml
{
    public static class XmlExtensions
    {
        /// <summary>
        /// Gets the declaration (if any) from the document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns></returns>
        public static XmlDeclaration GetDeclaration(this XmlDocument document)
        {
            foreach (var node in document.ChildNodes)
            {
                if ( node is XmlDeclaration ) {
                    return (XmlDeclaration) node;
                }
            }

            return null;
        }

        public static void WhenOptionalAttribute(this XmlNode node, String key, Action<string> action)
        {
            var valueNode = node.Attributes?.GetNamedItem(key);
            if (valueNode != null)
            {
                action.Invoke(valueNode.InnerText);
            }
        }

        public static String GetOptionalAttribute(this XmlNode node, String key)
        {
            var valueNode = node.Attributes?.GetNamedItem(key);
            return valueNode?.InnerText;
        }

        public static String GetRequiredAttribute(this XmlNode node, String key)
        {
            var valueNode = node.Attributes?.GetNamedItem(key);
            if (valueNode == null)
            {
                var name = String.IsNullOrEmpty(node.Name)
                    ? node.LocalName
                    : node.Name;
                throw new XmlException(
                    "Required attribute by name '" + key + "' not found for element '" + name + "'");
            }
            return valueNode.InnerText;
        }

        public static IList<XmlElement> CreateElementList(this XmlNodeList nodeList)
        {
            return new List<XmlElement>(CreateElementEnumerable(nodeList));
        }

        public static IEnumerable<XmlElement> CreateElementEnumerable(this XmlNodeList nodeList)
        {
            foreach (XmlNode node in nodeList)
            {
                if (node is XmlElement)
                {
                    yield return node as XmlElement;
                }
            }
        }
    }
}
