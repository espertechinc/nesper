///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using Antlr4.Runtime.Tree;

using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage1.specmapper;
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.epl.expression.chain;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.dot.walk;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.grammar.@internal.generated;

namespace com.espertech.esper.compiler.@internal.parse
{
	public class ASTChainableHelper
	{
		public static void ProcessChainable(
			EsperEPL2GrammarParser.ChainableContext ctx,
			IDictionary<ITree, ExprNode> astExprNodeMap,
			ContextCompileTimeDescriptor contextCompileTimeDescriptor,
			StatementSpecMapEnv mapEnv,
			StatementSpecRaw statementSpec,
			ExpressionDeclDesc expressionDeclarations,
			LazyAllocatedMap<HashableMultiKey, AggregationMultiFunctionForge> plugInAggregations,
			IList<ExpressionScriptProvided> scriptExpressions)
		{
			// we first convert the event property into chain spec
			IList<Chainable> chain = ASTChainSpecHelper.GetChainables(ctx, astExprNodeMap);

			// process chain
			StatementSpecMapContext mapContext = new StatementSpecMapContext(contextCompileTimeDescriptor, mapEnv, plugInAggregations, scriptExpressions);
			mapContext.AddExpressionDeclarations(expressionDeclarations);
			ExprNode node = ChainableWalkHelper.ProcessDot(false, true, chain, mapContext);
			astExprNodeMap.Put(ctx, node);
			mapContext.AddTo(statementSpec);
		}
	}
} // end of namespace
