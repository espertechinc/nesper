///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;

using com.espertech.esper.common.@internal.util.serde;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    ///     Interface representing a pattern expression.
    ///     <para />
    ///     Pattern expressions are organized into a tree-like structure with nodes representing sub-expressions (composite).
    ///     <para />
    ///     Certain types of nodes have certain requirements towards the number or types of nodes that
    ///     are expected as pattern sub-expressions to an pattern expression.
    /// </summary>
    [JsonConverter(typeof(JsonConverterAbstract<PatternExpr>))]
    public interface PatternExpr
    {
        /// <summary>
        ///     Returns the list of pattern sub-expressions (child expressions) to the current pattern expression node.
        /// </summary>
        /// <returns>pattern child expressions or empty list if there are no child expressions</returns>
        IList<PatternExpr> Children { get; set; }

        /// <summary>
        ///     Returns the precedence.
        /// </summary>
        /// <returns>precedence</returns>
        PatternExprPrecedenceEnum Precedence { get; }

        /// <summary>
        ///     Returns the id for the pattern expression, for use by tools.
        /// </summary>
        /// <returns>id</returns>
        string TreeObjectName { get; set; }

        /// <summary>
        ///     Renders the pattern expression and all it's child expressions, in full tree depth, as a string in
        ///     language syntax.
        /// </summary>
        /// <param name="writer">is the output to use</param>
        /// <param name="parentPrecedence">precedence</param>
        /// <param name="formatter">formatter</param>
        void ToEPL(
            TextWriter writer,
            PatternExprPrecedenceEnum parentPrecedence,
            EPStatementFormatter formatter);
    }
} // end of namespace