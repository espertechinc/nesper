///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.IO;

namespace com.espertech.esper.client.soda
{
	/// <summary>
	/// Interface representing an expression for use in select-clauses, where-clauses, having-clauses, order-by clauses and
	/// streams based on filters and pattern filter expressions.
	/// <para>
	/// Expressions are organized into a tree-like structure with nodes representing sub-expressions.
	/// </para>
	/// <para>
	/// Certain types of nodes have certain requirements towards the number or types of nodes that
	/// are expected as sub-expressions to an expression.
	/// </para>
	/// </summary>
	public interface Expression
	{
	    /// <summary>
	    /// Returns the list of sub-expressions (child expressions) to the current expression node.
	    /// </summary>
	    /// <returns>child expressions or empty list if there are no child expressions</returns>
        IList<Expression> Children { get; set; }

        /// <summary>
        /// Gets or sets the name of the tree object.
        /// </summary>
        /// <value>The name of the tree object.</value>
        string TreeObjectName { get; set; }

        /// <summary>
        /// Gets the Precedence.
        /// </summary>
        /// <value>The Precedence.</value>
        ExpressionPrecedenceEnum Precedence { get; }

        /// <summary>
        /// Renders the expressions and all it's child expression, in full tree depth, as a string in
        /// language syntax.
        /// </summary>
        /// <param name="writer">is the output to use</param>
        /// <param name="parentPrecedence">The parent Precedence.</param>
	    void ToEPL(TextWriter writer, ExpressionPrecedenceEnum parentPrecedence);
	}
} // End of namespace
