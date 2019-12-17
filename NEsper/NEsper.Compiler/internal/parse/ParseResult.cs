///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace com.espertech.esper.compiler.@internal.parse
{
    /// <summary>
    ///     Result of a parse action.
    /// </summary>
    public class ParseResult
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="tree">parse tree</param>
        /// <param name="expressionWithoutAnnotations">expression text no annotations, or null if same</param>
        /// <param name="scripts">script list</param>
        /// <param name="tokenStream">tokens</param>
        public ParseResult(
            ITree tree,
            string expressionWithoutAnnotations,
            CommonTokenStream tokenStream,
            IList<string> scripts)
        {
            Tree = tree;
            ExpressionWithoutAnnotations = expressionWithoutAnnotations;
            TokenStream = tokenStream;
            Scripts = scripts;
        }

        /// <summary>
        ///     AST.
        /// </summary>
        /// <returns>ast</returns>
        public ITree Tree { get; }

        /// <summary>
        ///     Returns the expression text no annotations.
        /// </summary>
        /// <returns>expression text no annotations.</returns>
        public string ExpressionWithoutAnnotations { get; }

        public CommonTokenStream TokenStream { get; }

        public IList<string> Scripts { get; }
    }
} // end of namespace