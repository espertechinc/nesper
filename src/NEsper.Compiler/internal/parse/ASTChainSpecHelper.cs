///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Antlr4.Runtime.Tree;

using com.espertech.esper.common.@internal.epl.expression.chain;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.grammar.@internal.generated;

namespace com.espertech.esper.compiler.@internal.parse
{
	public class ASTChainSpecHelper
	{
		public static bool HasChain(EsperEPL2GrammarParser.ChainableElementsContext ctx)
		{
			return ctx != null && !ctx.chainableAtomicWithOpt().IsEmpty();
		}

		public static IList<Chainable> GetChainables(
			EsperEPL2GrammarParser.ChainableContext ctx,
			IDictionary<ITree, ExprNode> astExprNodeMap)
		{
			var chain = new List<Chainable>();

			// handle root
			var root = ctx.chainableRootWithOpt();
			var optionalRoot = root.q != null;
			var prop = root.chainableWithArgs();
			chain.Add(GetChainable(prop, optionalRoot, astExprNodeMap));
			AddChainablesInternal(ctx.chainableElements(), astExprNodeMap, chain);
			if (chain.IsEmpty()) {
				throw new ArgumentException("Empty chain");
			}

			return chain;
		}

		public static IList<Chainable> GetChainables(
			EsperEPL2GrammarParser.ChainableElementsContext ctx,
			IDictionary<ITree, ExprNode> astExprNodeMap)
		{
			IList<Chainable> chain = new List<Chainable>();
			AddChainablesInternal(ctx, astExprNodeMap, chain);
			return chain;
		}

		public static void AddChainablesInternal(
			EsperEPL2GrammarParser.ChainableElementsContext ctx,
			IDictionary<ITree, ExprNode> astExprNodeMap,
			IList<Chainable> chain)
		{
			foreach (var context in ctx.chainableAtomicWithOpt()) {
				var optionalChainable = context.q != null;
				var atomic = context.chainableAtomic();
				Chainable chainable;
				if (atomic.chainableArray() != null) {
					var @params = ASTExprHelper.ExprCollectSubNodes(atomic.chainableArray(), 0, astExprNodeMap);
					chainable = new ChainableArray(false, optionalChainable, @params);
				}
				else {
					chainable = GetChainable(context.chainableAtomic().chainableWithArgs(), optionalChainable, astExprNodeMap);
				}

				chain.Add(chainable);
			}
		}

		private static Chainable GetChainable(
			EsperEPL2GrammarParser.ChainableWithArgsContext ctx,
			bool optional,
			IDictionary<ITree, ExprNode> astExprNodeMap)
		{
			var distinct = ctx.libFunctionArgs() != null && ctx.libFunctionArgs().DISTINCT() != null;
			var nameUnescaped = ctx.chainableIdent().GetText();
			var name = StringValue.RemoveTicks(nameUnescaped);
			if (ctx.lp == null) {
				return new ChainableName(distinct, optional, name, nameUnescaped);
			}

			IList<ExprNode> @params = ASTLambdaHelper.GetExprNodesLibFunc(ctx.libFunctionArgs(), astExprNodeMap);
			return new ChainableCall(distinct, optional, name, nameUnescaped, @params);
		}
	}
} // end of namespace
