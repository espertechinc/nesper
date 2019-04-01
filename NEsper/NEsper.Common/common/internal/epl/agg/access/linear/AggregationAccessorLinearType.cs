///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;

namespace com.espertech.esper.common.@internal.epl.agg.access.linear
{
    /// <summary>
    ///     Enum for aggregation multi-function state type.
    /// </summary>
    public class AggregationAccessorLinearType
    {
        /// <summary>
        ///     For "first" function.
        /// </summary>
        public static readonly AggregationAccessorLinearType FIRST = new AggregationAccessorLinearType("FIRST");

        /// <summary>
        ///     For "last" function.
        /// </summary>
        public static readonly AggregationAccessorLinearType LAST = new AggregationAccessorLinearType("LAST");

        /// <summary>
        ///     For "window" function.
        /// </summary>
        public static readonly AggregationAccessorLinearType WINDOW = new AggregationAccessorLinearType("WINDOW");

        public static AggregationAccessorLinearType[] Values = new AggregationAccessorLinearType[] {
            FIRST,
            LAST,
            WINDOW
        };

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Returns the enumeration value associated with the string text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static AggregationAccessorLinearType FromString(string text)
        {
            string compare = text.Trim().ToUpperInvariant();
            return Values.FirstOrDefault(type => 
                string.Equals(text, type.Name, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregationAccessorLinearType"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        private AggregationAccessorLinearType(string name)
        {
            Name = name;
        }
    }
} // end of namespace