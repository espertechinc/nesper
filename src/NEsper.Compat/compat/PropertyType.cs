///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.compat
{
    /// <summary> Enumeration of property types.</summary>
    [Flags]
    public enum PropertyType
    {
        UNDEFINED = 0,

        /// <summary> Simple property.</summary>
        SIMPLE = 1,

        /// <summary> Indexed property.</summary>
        INDEXED = 2,

        /// <summary> Mapped property.</summary>
        MAPPED = 4
    }

    public static class PropertyTypeExtensions
    {
        public static bool IsSimple(this PropertyType propertyType)
        {
            return (propertyType & PropertyType.SIMPLE) == PropertyType.SIMPLE;
        }

        public static bool IsIndexed(this PropertyType propertyType)
        {
            return (propertyType & PropertyType.INDEXED) == PropertyType.INDEXED;
        }

        public static bool IsMapped(this PropertyType propertyType)
        {
            return (propertyType & PropertyType.MAPPED) == PropertyType.MAPPED;
        }

        public static bool IsUndefined(this PropertyType propertyType)
        {
            return propertyType == PropertyType.UNDEFINED;
        }
    }
}