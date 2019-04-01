///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.@event.variant
{
    /// <summary>
    ///     Descriptor for a variant stream property.
    /// </summary>
    public class VariantPropertyDesc
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="propertyType">type or null if not exists</param>
        /// <param name="getter">the getter or null if not exists</param>
        /// <param name="property">the boolean indicating whether it exists or not</param>
        public VariantPropertyDesc(Type propertyType, EventPropertyGetterSPI getter, bool property)
        {
            PropertyType = propertyType;
            Getter = getter;
            IsProperty = property;
        }

        /// <summary>
        ///     True if the property exists, false if not.
        /// </summary>
        /// <returns>indicator whether property exists</returns>
        public bool IsProperty { get; }

        /// <summary>
        ///     Returns the property type.
        /// </summary>
        /// <returns>property type</returns>
        public Type PropertyType { get; }

        /// <summary>
        ///     Returns the getter for the property.
        /// </summary>
        /// <returns>property getter</returns>
        public EventPropertyGetterSPI Getter { get; }
    }
} // end of namespace