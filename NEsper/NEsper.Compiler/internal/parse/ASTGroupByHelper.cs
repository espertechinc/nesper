///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using Antlr4.Runtime.Tree;

using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.grammar.@internal.generated;

namespace com.espertech.esper.compiler.@internal.parse
{
    public class ASTGroupByHelper
    {
        public static void WalkGroupBy(
            EsperEPL2GrammarParser.GroupByListExprContext ctx,
            IDictionary<ITree, ExprNode> astExprNodeMap,
            IList<GroupByClauseElement> groupByExpressions)
        {
            IList<EsperEPL2GrammarParser.GroupByListChoiceContext> choices = ctx.groupByListChoice();
            foreach (var choice in choices)
            {
                var element = WalkChoice(choice, astExprNodeMap);
                groupByExpressions.Add(element);
            }
        }

        private static GroupByClauseElement WalkChoice(
            EsperEPL2GrammarParser.GroupByListChoiceContext choice,
            IDictionary<ITree, ExprNode> astExprNodeMap)
        {
            if (choice.e1 != null)
            {
                var expr = ASTExprHelper.ExprCollectSubNodes(choice.e1, 0, astExprNodeMap)[0];
                return new GroupByClauseElementExpr(expr);
            }

            if (choice.groupByCubeOrRollup() != null)
            {
                return WalkCubeOrRollup(choice.groupByCubeOrRollup(), astExprNodeMap);
            }

            return WalkGroupingSets(choice.groupByGroupingSets().groupBySetsChoice(), astExprNodeMap);
        }

        private static GroupByClauseElement WalkCubeOrRollup(
            EsperEPL2GrammarParser.GroupByCubeOrRollupContext ctx,
            IDictionary<ITree, ExprNode> astExprNodeMap)
        {
            var cube = ctx.CUBE() != null;
            var combinables = WalkCombinables(ctx.groupByCombinableExpr(), astExprNodeMap);
            return new GroupByClauseElementRollupOrCube(cube, combinables);
        }

        private static IList<GroupByClauseElement> WalkCombinables(
            IList<EsperEPL2GrammarParser.GroupByCombinableExprContext> ctxs,
            IDictionary<ITree, ExprNode> astExprNodeMap)
        {
            IList<GroupByClauseElement> elements = new List<GroupByClauseElement>();
            foreach (var ctx in ctxs)
            {
                elements.Add(WalkCombinable(ctx, astExprNodeMap));
            }

            return elements;
        }

        private static GroupByClauseElement WalkCombinable(
            EsperEPL2GrammarParser.GroupByCombinableExprContext ctx,
            IDictionary<ITree, ExprNode> astExprNodeMap)
        {
            if (ctx.e1 != null && ctx.LPAREN() == null)
            {
                ExprNode expr = ASTExprHelper.ExprCollectSubNodes(ctx.e1, 0, astExprNodeMap)[0];
                return new GroupByClauseElementExpr(expr);
            }

            var combined = ASTExprHelper.ExprCollectSubNodes(ctx, 0, astExprNodeMap);
            return new GroupByClauseElementCombinedExpr(combined);
        }

        private static GroupByClauseElementGroupingSet WalkGroupingSets(
            IList<EsperEPL2GrammarParser.GroupBySetsChoiceContext> ctxs,
            IDictionary<ITree, ExprNode> astExprNodeMap)
        {
            IList<GroupByClauseElement> elements = new List<GroupByClauseElement>();
            foreach (var ctx in ctxs)
            {
                if (ctx.groupByCubeOrRollup() != null)
                {
                    elements.Add(WalkCubeOrRollup(ctx.groupByCubeOrRollup(), astExprNodeMap));
                }
                else
                {
                    elements.Add(WalkCombinable(ctx.groupByCombinableExpr(), astExprNodeMap));
                }
            }

            return new GroupByClauseElementGroupingSet(elements);
        }
    }
} // end of namespace