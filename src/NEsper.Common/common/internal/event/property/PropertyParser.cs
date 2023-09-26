///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.propertyparser;

namespace com.espertech.esper.common.@internal.@event.property
{
    public class PropertyParser
    {
        public static Property ParseAndWalkLaxToSimple(string propertyName)
        {
            try {
                return PropertyParserNoDep.ParseAndWalkLaxToSimple(propertyName, false);
            }
            catch (PropertyAccessException) {
                return new SimpleProperty(propertyName);
            }
        }

        public static string UnescapeBacktickForProperty(string unescapedPropertyName)
        {
            if (unescapedPropertyName.StartsWith("`") && unescapedPropertyName.EndsWith("`")) {
                return unescapedPropertyName.Substring(1, unescapedPropertyName.Length - 2);
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

            if (!(prop is NestedProperty nestedProperty)) {
                return false;
            }

            foreach (var property in nestedProperty.Properties) {
                if (IsPropertyDynamic(property)) {
                    return true;
                }
            }

            return false;
        }
    }
} // end of namespace