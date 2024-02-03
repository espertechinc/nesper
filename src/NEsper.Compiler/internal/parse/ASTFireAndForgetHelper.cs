///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using Antlr4.Runtime.Tree;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.grammar.@internal.generated;

namespace com.espertech.esper.compiler.@internal.parse
{
	public class ASTFireAndForgetHelper
	{
		public static IList<IList<ExprNode>> WalkInsertInto(
			EsperEPL2GrammarParser.FafInsertContext ctx,
			IDictionary<ITree, ExprNode> astExprNodeMap)
		{
			var insertRowContexts = ctx.fafInsertRow();
			var values = new List<IList<ExprNode>>(insertRowContexts.Length);
			foreach (EsperEPL2GrammarParser.FafInsertRowContext rowCtx in insertRowContexts) {
				values.Add(WalkInsertIntoRow(rowCtx, astExprNodeMap));
			}

			return values;
		}

		private static IList<ExprNode> WalkInsertIntoRow(
			EsperEPL2GrammarParser.FafInsertRowContext ctx,
			IDictionary<ITree, ExprNode> astExprNodeMap)
		{
			var expressionList = ctx.expressionList().expression();
			var result = new List<ExprNode>(expressionList.Length);
			foreach (var valueExpr in expressionList) {
				var expr = ASTExprHelper.ExprCollectSubNodes(valueExpr, 0, astExprNodeMap)[0];
				result.Add(expr);
			}

			return result;
		}
	}
} // end of namespace
