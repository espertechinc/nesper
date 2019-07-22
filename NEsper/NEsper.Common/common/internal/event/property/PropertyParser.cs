///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.IO;

using com.espertech.esper.common.@internal.@event.propertyparser;

namespace com.espertech.esper.common.@internal.@event.property
{
    public class PropertyParser
    {
        public static Property ParseAndWalkLaxToSimple(string propertyName)
        {
            return PropertyParserNoDep.ParseAndWalkLaxToSimple(propertyName, false);
        }

        public static string UnescapeBacktickForProperty(string unescapedPropertyName)
        {
            if (unescapedPropertyName.StartsWith("`") && unescapedPropertyName.EndsWith("`")) {
                return unescapedPropertyName.Substring(1, unescapedPropertyName.Length - 1);
            }

            if (!unescapedPropertyName.Contains("`")) {
                return unescapedPropertyName;
            }

            // parse and render
            var property = ParseAndWalkLaxToSimple(unescapedPropertyName);
            if (property is NestedProperty) {
                var writer = new StringWriter();
                property.ToPropertyEPL(writer);
                return writer.ToString();
            }

            return unescapedPropertyName;
        }

        public static Property ParseAndWalk(
            string propertyNested,
            bool isRootedDynamic)
        {
            return PropertyParserNoDep.ParseAndWalkLaxToSimple(propertyNested, isRootedDynamic);
        }

        public static bool IsPropertyDynamic(Property prop)
        {
            if (prop is DynamicProperty) {
                return true;
            }

            if (!(prop is NestedProperty)) {
                return false;
            }

            var nestedProperty = (NestedProperty) prop;
            foreach (var property in nestedProperty.Properties) {
                if (IsPropertyDynamic(property)) {
                    return true;
                }
            }

            return false;
        }
    }
} // end of namespace