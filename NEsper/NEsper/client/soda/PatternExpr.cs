///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
    /// Interface representing a pattern expression.
    /// <para />
    /// Pattern expressions are organized into a tree-like structure with nodes representing sub-expressions (composite).
    /// <para /> 
    /// Certain types of nodes have certain requirements towards the number or types of nodes that are expected as pattern
    /// sub-expressions to an pattern expression.
    /// </summary>
    public interface PatternExpr
    {
        /// <summary>Returns the list of pattern sub-expressions (child expressions) to the current pattern expression node. </summary>
        /// <value>pattern child expressions or empty list if there are no child expressions</value>
        List<PatternExpr> Children { get; set; }

        /// <summary>Returns the Precedence. </summary>
        /// <value>Precedence</value>
        PatternExprPrecedenceEnum Precedence { get; }

        /// <summary>Renders the pattern expression and all it's child expressions, in full tree depth, as a string in language syntax. </summary>
        /// <param name="writer">is the output to use</param>
        /// <param name="parentPrecedence">Precedence</param>
        /// <param name="formatter">formatter</param>
        void ToEPL(TextWriter writer, PatternExprPrecedenceEnum parentPrecedence, EPStatementFormatter formatter);

        /// <summary>Returns the id for the pattern expression, for use by tools. </summary>
        /// <value>id</value>
        string TreeObjectName { get; set; }
    }
}
