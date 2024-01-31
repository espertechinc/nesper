///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
    /// Interface representing an expression for use in select-clauses, where-clauses, having-clauses, order-by clauses and
    /// streams based on filters and pattern filter expressions.
    /// <para />Expressions are organized into a tree-like structure with nodes representing sub-expressions.
    /// <para />Certain types of nodes have certain requirements towards the number or types of nodes that
    /// are expected as sub-expressions to an expression.
    /// </summary>
    [JsonConverter(typeof(JsonConverterAbstract<Expression>))]
    public interface Expression
    {
        /// <summary>
        /// Returns the list of sub-expressions (child expressions) to the current expression node.
        /// </summary>
        /// <returns>child expressions or empty list if there are no child expressions</returns>
        IList<Expression> Children { get; set; }

        /// <summary>
        /// Returns the tree of object name, for use by tools to assign an identifier to an expression.
        /// </summary>
        /// <returns>tree object id</returns>
        string TreeObjectName { get; set; }

        /// <summary>
        /// Returns precedence.
        /// </summary>
        /// <returns>precedence</returns>
        ExpressionPrecedenceEnum Precedence { get; }

        /// <summary>
        /// Write expression considering precedence.
        /// </summary>
        /// <param name="writer">to use</param>
        /// <param name="parentPrecedence">precedence</param>
        void ToEPL(
            TextWriter writer,
            ExpressionPrecedenceEnum parentPrecedence);
    }
} // end of namespace