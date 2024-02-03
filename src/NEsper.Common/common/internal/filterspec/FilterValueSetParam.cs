///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.filterspec
{
    /// <summary>
    ///     This interface represents one filter parameter in an filter specification.
    ///     <para />
    ///     Each filtering parameter has a lookup-able and operator type, and a value to filter for.
    /// </summary>
    public interface FilterValueSetParam
    {
        /// <summary>
        ///     Returns the lookup-able for the filter parameter.
        /// </summary>
        /// <value>lookup-able</value>
        ExprFilterSpecLookupable Lookupable { get; }

        /// <summary>
        ///     Returns the filter operator type.
        /// </summary>
        /// <returns>filter operator type</returns>
        FilterOperator FilterOperator { get; }

        /// <summary>
        ///     Return the filter parameter constant to filter for.
        /// </summary>
        /// <returns>filter parameter constant's value</returns>
        object FilterForValue { get; }

        void AppendTo(TextWriter writer);
    }

    public class FilterValueSetParamConstants
    {
        public static readonly FilterValueSetParam[][] EMPTY = Array.Empty<FilterValueSetParam[]>();
    }
} // end of namespace