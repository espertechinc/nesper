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
    /// <summary>
    ///     Builds a filter specification from filter AST nodes.
    /// </summary>
    public class ASTFilterSpecHelper
    {
        public static FilterSpecRaw WalkFilterSpec(
            EsperEPL2GrammarParser.EventFilterExpressionContext ctx,
            PropertyEvalSpec propertyEvalSpec,
            IDictionary<ITree, ExprNode> astExprNodeMap)
        {
            var eventName = ASTUtil.UnescapeClassIdent(ctx.classIdentifier());
            var exprNodes = ctx.expressionList() != null
                ? ASTExprHelper.ExprCollectSubNodes(ctx.expressionList(), 0, astExprNodeMap)
                : new List<ExprNode>(1);
            return new FilterSpecRaw(eventName, exprNodes, propertyEvalSpec);
        }
    }
} // end of namespace