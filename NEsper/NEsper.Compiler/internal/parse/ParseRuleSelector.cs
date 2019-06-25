///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using Antlr4.Runtime.Tree;

using com.espertech.esper.compiler.@internal.generated;

namespace com.espertech.esper.compiler.@internal.parse
{
    /// <summary>
    /// For selection of the parse rule to use.
    /// </summary>
    public interface ParseRuleSelector
    {
        /// <summary>
        /// Implementations can invoke a parse rule of their choice on the parser.
        /// </summary>
        /// <param name="parser">to invoke parse rule on</param>
        /// <returns>the AST tree as a result of the parsing</returns>
        /// <throws>RecognitionException is a parse exception</throws>
        ITree InvokeParseRule(EsperEPL2GrammarParser parser);
    }
} // end of namespace