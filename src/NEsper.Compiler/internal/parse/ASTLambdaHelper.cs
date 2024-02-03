///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using Antlr4.Runtime.Tree;

using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.collections;
using com.espertech.esper.grammar.@internal.generated;

namespace com.espertech.esper.compiler.@internal.parse
{
	public class ASTLambdaHelper
	{
		public static IList<ExprNode> GetExprNodesLibFunc(
			EsperEPL2GrammarParser.LibFunctionArgsContext ctx,
			IDictionary<ITree, ExprNode> astExprNodeMap)
		{
			IList<EsperEPL2GrammarParser.LibFunctionArgItemContext> args = ctx?.libFunctionArgItem();
			if (args == null || args.IsEmpty()) {
				return EmptyList<ExprNode>.Instance;
			}

			IList<ExprNode> parameters = new List<ExprNode>(args.Count);
			foreach (EsperEPL2GrammarParser.LibFunctionArgItemContext arg in args) {
				if (arg.expressionLambdaDecl() != null) {
					IList<string> lambdaparams = GetLambdaGoesParams(arg.expressionLambdaDecl());
					ExprLambdaGoesNode goes = new ExprLambdaGoesNode(lambdaparams);
					ExprNode lambdaExpr = ASTExprHelper.ExprCollectSubNodes(arg.expressionWithNamed(), 0, astExprNodeMap)[0];
					goes.AddChildNode(lambdaExpr);
					parameters.Add(goes);
				}
				else {
					IList<ExprNode> @params = ASTExprHelper.ExprCollectSubNodes(arg.expressionWithNamed(), 0, astExprNodeMap);
					ExprNode parameter = @params[0];
					parameters.Add(parameter);
				}
			}

			return parameters;
		}

		public static IList<string> GetLambdaGoesParams(EsperEPL2GrammarParser.ExpressionLambdaDeclContext ctx)
		{
			IList<string> parameters;
			if (ctx.i != null) {
				parameters = new List<string>(1);
				parameters.Add(ctx.i.GetText());
			}
			else {
				parameters = ASTUtil.GetIdentList(ctx.columnListKeywordAllowed());
			}

			return parameters;
		}
	}
} // end of namespace
