///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Text;

using com.espertech.esper.common.@internal.@event.property;

namespace com.espertech.esper.common.@internal.@event.xml
{
    /// <summary>
    ///     Parses event property names and transforms to XPath expressions. Supports
    ///     nested, indexed and mapped event properties.
    /// </summary>
    public class SimpleXMLPropertyParser
    {
        /// <summary>
        ///     Return the xPath corresponding to the given property.
        ///     The propertyName String may be simple, nested, indexed or mapped.
        /// </summary>
        /// <param name="property">is the property</param>
        /// <param name="rootElementName">is the name of the root element for generating the XPath expression</param>
        /// <param name="defaultNamespacePrefix">is the prefix of the default namespace</param>
        /// <param name="isResolvePropertiesAbsolute">
        ///     is true to indicate to resolve XPath properties as absolute propsor relative props
        /// </param>
        /// <returns>xpath expression</returns>
        public static string Walk(
            Property property,
            string rootElementName,
            string defaultNamespacePrefix,
            bool isResolvePropertiesAbsolute)
        {
            var xPathBuf = new StringBuilder();
            xPathBuf.Append('/');
            if (isResolvePropertiesAbsolute) {
                if (defaultNamespacePrefix != null) {
                    xPathBuf.Append(defaultNamespacePrefix);
                    xPathBuf.Append(':');
                }

                xPathBuf.Append(rootElementName);
            }

            if (!(property is NestedProperty nestedProperty)) {
                xPathBuf.Append(MakeProperty(property, defaultNamespacePrefix));
            }
            else {
                foreach (var propertyNested in nestedProperty.Properties) {
                    xPathBuf.Append(MakeProperty(propertyNested, defaultNamespacePrefix));
                }
            }

            return xPathBuf.ToString();
        }

        private static string MakeProperty(
            Property property,
            string defaultNamespacePrefix)
        {
            var prefix = "";
            if (defaultNamespacePrefix != null) {
                prefix = defaultNamespacePrefix + ":";
            }

            var unescapedIdent = property.PropertyNameAtomic;
            if (property is PropertyWithIndex withIndex) {
                var index = withIndex.Index;
                var xPathPosition = index + 1;
                return '/' + prefix + unescapedIdent + "[position() = " + xPathPosition + ']';
            }

            if (property is MappedProperty) {
                var key = ((PropertyWithKey)property).Key;
                return '/' + prefix + unescapedIdent + "[@id='" + key + "']";
            }

            return '/' + prefix + unescapedIdent;
        }
    }
} // end of namespace