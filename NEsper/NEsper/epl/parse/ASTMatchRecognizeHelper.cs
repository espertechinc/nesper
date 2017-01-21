///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.generated;
using com.espertech.esper.epl.spec;
using com.espertech.esper.rowregex;

namespace com.espertech.esper.epl.parse
{
    /// <summary>
    /// Helper class for walking the match-recognize AST.
    /// </summary>
    public class ASTMatchRecognizeHelper
    {
        private const String Message = "Match-recognize AFTER clause must be either AFTER MATCH SKIP TO LAST ROW or AFTER MATCH SKIP TO NEXT ROW or AFTER MATCH SKIP TO CURRENT ROW";
    
        public static MatchRecognizeSkipEnum ParseSkip(CommonTokenStream tokenStream, EsperEPL2GrammarParser.MatchRecogMatchesAfterSkipContext ctx)
        {
            if ((!ctx.i1.GetText().ToUpper().Equals("MATCH")) ||
                (!ctx.i2.GetText().ToUpper().Equals("SKIP")) ||
                (!ctx.i5.GetText().ToUpper().Equals("ROW"))
                )
            {
                throw ASTWalkException.From(Message, tokenStream, ctx);
            }
    
            if ((!ctx.i3.GetText().ToUpper().Equals("TO")) &&
                (!ctx.i3.GetText().ToUpper().Equals("PAST"))
                )
            {
                throw ASTWalkException.From(Message, tokenStream, ctx);
            }
    
            if (ctx.i4.GetText().ToUpper().Equals("LAST"))
            {
                return MatchRecognizeSkipEnum.PAST_LAST_ROW;
            }
            else if (ctx.i4.GetText().ToUpper().Equals("NEXT"))
            {
                return MatchRecognizeSkipEnum.TO_NEXT_ROW;
            }
            else if (ctx.i4.GetText().ToUpper().Equals("CURRENT"))
            {
                return MatchRecognizeSkipEnum.TO_CURRENT_ROW;
            }
            throw ASTWalkException.From(Message);
        }

        public static RowRegexExprRepeatDesc walkOptionalRepeat(EsperEPL2GrammarParser.MatchRecogPatternRepeatContext ctx, IDictionary<ITree, ExprNode> astExprNodeMap)
        {
            if (ctx == null)
            {
                return null;
            }

            ExprNode e1 = ctx.e1 == null ? null : ASTExprHelper.ExprCollectSubNodes(ctx.e1, 0, astExprNodeMap)[0];
            ExprNode e2 = ctx.e2 == null ? null : ASTExprHelper.ExprCollectSubNodes(ctx.e2, 0, astExprNodeMap)[0];

            if (ctx.comma == null && ctx.e1 != null)
            {
                return new RowRegexExprRepeatDesc(null, null, e1);
            }

            if (e1 == null && e2 == null)
            {
                throw ASTWalkException.From("Invalid match-recognize quantifier '" + ctx.GetText() + "', expecting an expression");
            }

            return new RowRegexExprRepeatDesc(e1, e2, null);
        }
    }
}
