///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.IO;
using System.Text.Json.Serialization;

using com.espertech.esper.common.@internal.util.serde;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// Base interface for group-by clause expressions, covers all possible combinations
    /// of expressions, parenthesis-expression-combinations, rollup, cube and grouping sets
    /// and their parameters.
    /// </summary>
    [JsonConverter(typeof(JsonConverterAbstract<GroupByClauseExpression>))]
    public interface GroupByClauseExpression
    {
        /// <summary>RenderAny group by expression </summary>
        /// <param name="writer">to render to</param>
        void ToEPL(TextWriter writer);
    }
}