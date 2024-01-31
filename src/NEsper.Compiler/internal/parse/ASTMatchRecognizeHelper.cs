///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.rowrecog.expr;
using com.espertech.esper.grammar.@internal.generated;

namespace com.espertech.esper.compiler.@internal.parse
{
    /// <summary>
    ///     Helper class for walking the match-recognize AST.
    /// </summary>
    public class ASTMatchRecognizeHelper
    {
        private const string MESSAGE =
            "Match-recognize AFTER clause must be either AFTER MATCH SKIP TO LAST ROW or AFTER MATCH SKIP TO NEXT ROW or AFTER MATCH SKIP TO CURRENT ROW";

        public static MatchRecognizeSkipEnum ParseSkip(
            CommonTokenStream tokenStream,
            EsperEPL2GrammarParser.MatchRecogMatchesAfterSkipContext ctx)
        {
            if (!string.Equals(ctx.i1.GetText(), "MATCH", StringComparison.InvariantCultureIgnoreCase) ||
                !string.Equals(ctx.i2.GetText(), "SKIP", StringComparison.InvariantCultureIgnoreCase) ||
                !string.Equals(ctx.i5.GetText(), "ROW", StringComparison.InvariantCultureIgnoreCase)
            )
            {
                throw ASTWalkException.From(MESSAGE, tokenStream, ctx);
            }

            var i3 = ctx.i3.GetText();
            if (!string.Equals(i3, "TO", StringComparison.InvariantCultureIgnoreCase) &&
                !string.Equals(i3, "PAST", StringComparison.InvariantCultureIgnoreCase))
            {
                throw ASTWalkException.From(MESSAGE, tokenStream, ctx);
            }

            var i4 = ctx.i4.GetText();
            if (string.Equals(i4, "LAST", StringComparison.InvariantCultureIgnoreCase))
            {
                return MatchRecognizeSkipEnum.PAST_LAST_ROW;
            }

            if (string.Equals(i4, "NEXT", StringComparison.InvariantCultureIgnoreCase))
            {
                return MatchRecognizeSkipEnum.TO_NEXT_ROW;
            }

            if (string.Equals(i4, "CURRENT", StringComparison.InvariantCultureIgnoreCase))
            {
                return MatchRecognizeSkipEnum.TO_CURRENT_ROW;
            }

            throw ASTWalkException.From(MESSAGE);
        }

        public static RowRecogExprRepeatDesc WalkOptionalRepeat(
            EsperEPL2GrammarParser.MatchRecogPatternRepeatContext ctx,
            IDictionary<ITree, ExprNode> astExprNodeMap)
        {
            if (ctx == null)
            {
                return null;
            }

            var e1 = ctx.e1 == null ? null : ASTExprHelper.ExprCollectSubNodes(ctx.e1, 0, astExprNodeMap)[0];
            var e2 = ctx.e2 == null ? null : ASTExprHelper.ExprCollectSubNodes(ctx.e2, 0, astExprNodeMap)[0];

            if (ctx.comma == null && ctx.e1 != null)
            {
                return new RowRecogExprRepeatDesc(null, null, e1);
            }

            if (e1 == null && e2 == null)
            {
                throw ASTWalkException.From("Invalid match-recognize quantifier '" + ctx.GetText() + "', expecting an expression");
            }

            return new RowRecogExprRepeatDesc(e1, e2, null);
        }
    }
} // end of namespace