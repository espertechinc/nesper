///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;

namespace com.espertech.esper.epl.agg.access
{
    /// <summary>Enum for aggregation multi-function state type. </summary>
    public enum AggregationStateType
    {
        /// <summary>For "first" function. </summary>
        FIRST,
        /// <summary>For "last" function. </summary>
        LAST,
        /// <summary>For "window" function. </summary>
        WINDOW
    }

    public static class AggregationStateTypeExtensions
    {
        public static AggregationStateType? FromString(string text, bool throwException = false)
        {
            var value = EnumHelper.ParseBoxed<AggregationStateType>(text, true);
            if ((value == null) && throwException)
            {
                throw new ArgumentException("illegal value for enumeration");
            }

            return value;
        }
    }
} // end of namespace
