///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.annotation
{
    /// <summary>
    /// Enumeration for the different types of statement-processing hooks (callbacks) that can be provided for a statement.
    /// </summary>
    public enum HookType
    {
        /// <summary>
        /// For use when installing a callback for converting SQL input parameters or column output values.
        /// </summary>
        SQLCOL,

        /// <summary>
        /// For use when installing a callback for converting SQL row results to an object.
        /// </summary>
        SQLROW,

        /// <summary>
        /// For internal use, query planning reporting.
        /// </summary>
        INTERNAL_QUERY_PLAN,

        /// <summary>
        /// For internal use, group rollup plan reporting.
        /// </summary>
        INTERNAL_GROUPROLLUP_PLAN,

        /// <summary>
        /// For internal use, aggregation level reporting.
        /// </summary>
        INTERNAL_AGGLOCALLEVEL,

        /// <summary>
        /// For internal use, pattern objects visibility.
        /// </summary>
        INTERNAL_PATTERNCOMPILE,

        /// <summary>
        /// For internal use, filter spec reporting.
        /// </summary>
        INTERNAL_FILTERSPEC,

        /// <summary>
        /// For internal use, statement spec compiled reporting.
        /// </summary>
        INTERNAL_COMPILE,

        /// <summary>
        /// For internal use, EPCompiled loading
        /// </summary>
        INTERNAL_COMPILEDLOAD
    }
}