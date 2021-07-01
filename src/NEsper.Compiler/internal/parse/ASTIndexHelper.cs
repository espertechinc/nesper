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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.grammar.@internal.generated;

namespace com.espertech.esper.compiler.@internal.parse
{
    public class ASTIndexHelper
    {
        public static CreateIndexDesc Walk(
            EsperEPL2GrammarParser.CreateIndexExprContext ctx,
            IDictionary<ITree, ExprNode> astExprNodeMap)
        {
            var indexName = ctx.n.Text;
            var windowName = ctx.w.Text;

            var unique = false;
            if (ctx.u != null)
            {
                var ident = ctx.u.Text;
                if (ident.ToLowerInvariant().Trim().Equals("unique"))
                {
                    unique = true;
                }
                else
                {
                    throw ASTWalkException.From("Invalid keyword '" + ident + "' in create-index encountered, expected 'unique'");
                }
            }

            IList<CreateIndexItem> columns = new List<CreateIndexItem>();
            IList<EsperEPL2GrammarParser.CreateIndexColumnContext> cols = ctx.createIndexColumnList().createIndexColumn();
            foreach (var col in cols)
            {
                var item = Walk(col, astExprNodeMap);
                columns.Add(item);
            }

            return new CreateIndexDesc(unique, indexName, windowName, columns);
        }

        private static CreateIndexItem Walk(
            EsperEPL2GrammarParser.CreateIndexColumnContext col,
            IDictionary<ITree, ExprNode> astExprNodeMap)
        {
            IList<ExprNode> expressions = new EmptyList<ExprNode>();
            if (col.i != null)
            {
                expressions = ASTExprHelper.ExprCollectSubNodes(col.i, 0, astExprNodeMap);
            }
            else if (col.expression() != null)
            {
                expressions = ASTExprHelper.ExprCollectSubNodes(col.expression(), 0, astExprNodeMap);
            }

            string type = CreateIndexType.HASH.GetName().ToLowerInvariant();
            if (col.t != null)
            {
                type = col.t.Text;
            }

            IList<ExprNode> parameters = new EmptyList<ExprNode>();
            if (col.p != null)
            {
                parameters = ASTExprHelper.ExprCollectSubNodes(col.p, 0, astExprNodeMap);
            }

            return new CreateIndexItem(expressions, type, parameters);
        }
    }
} // end of namespace