///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace com.espertech.esper.epl.parse
{
    /// <summary>Result of a parse action. </summary>
    public class ParseResult
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="tree">parse tree</param>
        /// <param name="expressionWithoutAnnotations">expression text no annotations, or null if same</param>
        /// <param name="tokenStream">The token stream.</param>
        /// <param name="scripts">The scripts.</param>
        public ParseResult(ITree tree, String expressionWithoutAnnotations, CommonTokenStream tokenStream, IList<string> scripts)
        {
            Tree = tree;
            ExpressionWithoutAnnotations = expressionWithoutAnnotations;
            TokenStream = tokenStream;
            Scripts = scripts;
        }

        /// <summary>
        /// AST.
        /// </summary>
        /// <value>ast</value>
        public ITree Tree { get; private set; }

        /// <summary>
        /// Returns the expression text no annotations.
        /// </summary>
        /// <value>expression text no annotations.</value>
        public string ExpressionWithoutAnnotations { get; private set; }

        /// <summary>
        /// Gets or sets the token stream.
        /// </summary>
        /// <value>The token stream.</value>
        public CommonTokenStream TokenStream { get; private set; }

        /// <summary>
        /// Gets or sets the scripts.
        /// </summary>
        /// <value>The scripts.</value>
        public IList<String> Scripts { get; private set; }
    }
}
