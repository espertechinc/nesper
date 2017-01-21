///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using Antlr4.Runtime.Tree;

using com.espertech.esper.epl.generated;

namespace com.espertech.esper.epl.parse
{
    /// <summary>
    /// Implementations can invoke a parse rule of their choice on the parser.
    /// </summary>
    /// <param name="parser">parser to invoke parse rule on</param>

    public delegate ITree ParseRuleSelector(EsperEPL2GrammarParser parser);
}
