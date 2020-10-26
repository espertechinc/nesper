///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.agg.access.linear
{
    /// <summary>
    ///     Enum for aggregation multi-function state type.
    /// </summary>
    public enum AggregationAccessorLinearType
    {
        /// <summary>
        ///     For "first" function.
        /// </summary>
        FIRST,

        /// <summary>
        ///     For "last" function.
        /// </summary>
        LAST,

        /// <summary>
        ///     For "window" function.
        /// </summary>
        WINDOW
    }

    public static class AggregationAccessorLinearTypeExtensions
    {
        /// <summary>
        /// Returns the enumeration value associated with the string text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static AggregationAccessorLinearType? FromString(string text)
        {
            return EnumHelper.ParseBoxed<AggregationAccessorLinearType>(text, true);
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public static string GetName(this AggregationAccessorLinearType value)
        {
            switch (value)
            {
                case AggregationAccessorLinearType.FIRST:
                    return "FIRST";
                case AggregationAccessorLinearType.LAST:
                    return "LAST";
                case AggregationAccessorLinearType.WINDOW:
                    return "WINDOW";
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }
} // end of namespace