///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Antlr4.Runtime.Tree;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.generated;
using com.espertech.esper.epl.spec;

namespace com.espertech.esper.epl.parse
{
    public class ASTIndexHelper
    {
        public static CreateIndexDesc Walk(EsperEPL2GrammarParser.CreateIndexExprContext ctx, IDictionary<ITree, ExprNode> astExprNodeMap)
        {
            var indexName = ctx.n.Text;
            var windowName = ctx.w.Text;
    
            var unique = false;
            if (ctx.u != null)
            {
                string ident = ctx.u.Text;
                if (ident.ToLowerInvariant().Trim() == "unique")
                {
                    unique = true;
                }
                else
                {
                    throw ASTWalkException.From("Invalid keyword '" + ident + "' in create-index encountered, expected 'unique'");
                }
            }
    
            var columns = new List<CreateIndexItem>();
            var cols = ctx.createIndexColumnList().createIndexColumn();
            foreach (var col in cols)
            {
                CreateIndexItem item = Walk(col, astExprNodeMap);
                columns.Add(item);
            }
            return new CreateIndexDesc(unique, indexName, windowName, columns);
        }
    
        private static CreateIndexItem Walk(EsperEPL2GrammarParser.CreateIndexColumnContext col, IDictionary<ITree, ExprNode> astExprNodeMap)
        {
            var expressions = Collections.GetEmptyList<ExprNode>();
            if (col.i != null) {
                expressions = ASTExprHelper.ExprCollectSubNodes(col.i, 0, astExprNodeMap);
            } else if (col.expression() != null) {
                expressions = ASTExprHelper.ExprCollectSubNodes(col.expression(), 0, astExprNodeMap);
            }

            var type = CreateIndexType.HASH.GetNameInvariant();
            if (col.t != null) {
                type = col.t.Text;
            }
    
            var parameters = Collections.GetEmptyList<ExprNode>();
            if (col.p != null) {
                parameters = ASTExprHelper.ExprCollectSubNodes(col.p, 0, astExprNodeMap);
            }
            return new CreateIndexItem(expressions, type, parameters);
        }
    }
} // end of namespace
