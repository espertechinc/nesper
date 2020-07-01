///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.time.node;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.grammar.@internal.generated;

namespace com.espertech.esper.compiler.@internal.parse
{
    public class ASTContextHelper
    {
        public static CreateContextDesc WalkCreateContext(
            EsperEPL2GrammarParser.CreateContextExprContext ctx,
            IDictionary<ITree, ExprNode> astExprNodeMap,
            IDictionary<ITree, EvalForgeNode> astPatternNodeMap,
            PropertyEvalSpec propertyEvalSpec,
            FilterSpecRaw filterSpec)
        {
            var contextName = ctx.name.Text;
            ContextSpec contextDetail;

            var choice = ctx.createContextDetail().createContextChoice();
            if (choice != null)
            {
                contextDetail = WalkChoice(choice, astExprNodeMap, astPatternNodeMap, propertyEvalSpec);
            }
            else
            {
                contextDetail = WalkNested(
                    ctx.createContextDetail().contextContextNested(), astExprNodeMap, astPatternNodeMap, propertyEvalSpec, filterSpec);
            }

            return new CreateContextDesc(contextName, contextDetail);
        }

        private static ContextSpec WalkNested(
            IList<EsperEPL2GrammarParser.ContextContextNestedContext> nestedContexts,
            IDictionary<ITree, ExprNode> astExprNodeMap,
            IDictionary<ITree, EvalForgeNode> astPatternNodeMap,
            PropertyEvalSpec propertyEvalSpec,
            FilterSpecRaw filterSpec)
        {
            IList<CreateContextDesc> contexts = new List<CreateContextDesc>(nestedContexts.Count);
            foreach (var nestedctx in nestedContexts)
            {
                var contextDetail = WalkChoice(nestedctx.createContextChoice(), astExprNodeMap, astPatternNodeMap, propertyEvalSpec);
                var desc = new CreateContextDesc(nestedctx.name.Text, contextDetail);
                contexts.Add(desc);
            }

            return new ContextNested(contexts);
        }

        private static ContextSpec WalkChoice(
            EsperEPL2GrammarParser.CreateContextChoiceContext ctx,
            IDictionary<ITree, ExprNode> astExprNodeMap,
            IDictionary<ITree, EvalForgeNode> astPatternNodeMap,
            PropertyEvalSpec propertyEvalSpec)
        {
            // temporal fixed (start+end) and overlapping (initiated/terminated)
            if (ctx.START() != null || ctx.INITIATED() != null)
            {
                ExprNode[] distinctExpressions = null;
                if (ctx.createContextDistinct() != null)
                {
                    if (ctx.createContextDistinct().expressionList() == null)
                    {
                        distinctExpressions = ExprNodeUtilityQuery.EMPTY_EXPR_ARRAY;
                    }
                    else
                    {
                        distinctExpressions = ASTExprHelper.ExprCollectSubNodesPerNode(
                            ctx.createContextDistinct().expressionList().expression(), astExprNodeMap);
                    }
                }

                ContextSpecCondition startEndpoint;
                if (ctx.START() != null)
                {
                    var immediate = CheckNow(ctx.i);
                    if (immediate)
                    {
                        startEndpoint = ContextSpecConditionImmediate.INSTANCE;
                    }
                    else
                    {
                        startEndpoint = GetContextCondition(ctx.r1, astExprNodeMap, astPatternNodeMap, propertyEvalSpec, false);
                    }
                }
                else
                {
                    var immediate = CheckNow(ctx.i);
                    startEndpoint = GetContextCondition(ctx.r1, astExprNodeMap, astPatternNodeMap, propertyEvalSpec, immediate);
                }

                var overlapping = ctx.INITIATED() != null;
                var endEndpoint = GetContextCondition(ctx.r2, astExprNodeMap, astPatternNodeMap, propertyEvalSpec, false);
                return new ContextSpecInitiatedTerminated(startEndpoint, endEndpoint, overlapping, distinctExpressions);
            }

            // partitioned
            if (ctx.PARTITION() != null)
            {
                IList<EsperEPL2GrammarParser.CreateContextPartitionItemContext> partitions = ctx.createContextPartitionItem();
                IList<ContextSpecKeyedItem> rawSpecs = new List<ContextSpecKeyedItem>();
                foreach (var partition in partitions)
                {
                    var filterSpec = ASTFilterSpecHelper.WalkFilterSpec(partition.eventFilterExpression(), propertyEvalSpec, astExprNodeMap);
                    propertyEvalSpec = null;

                    IList<string> propertyNames = new List<string>();
                    IList<EsperEPL2GrammarParser.ChainableContext> properties = partition.chainable();
                    foreach (var property in properties)
                    {
                        var propertyName = ASTUtil.GetPropertyName(property, 0);
                        propertyNames.Add(propertyName);
                    }

                    ASTExprHelper.ExprCollectSubNodes(partition, 0, astExprNodeMap); // remove expressions

                    rawSpecs.Add(
                        new ContextSpecKeyedItem(
                            filterSpec, propertyNames, partition.keywordAllowedIdent() == null ? null : partition.keywordAllowedIdent().GetText()));
                }

                IList<ContextSpecConditionFilter> optionalInit = null;
                if (ctx.createContextPartitionInit() != null)
                {
                    optionalInit = GetContextPartitionInit(ctx.createContextPartitionInit().createContextFilter(), astExprNodeMap);
                }

                ContextSpecCondition optionalTermination = null;
                if (ctx.createContextPartitionTerm() != null)
                {
                    optionalTermination = GetContextCondition(
                        ctx.createContextPartitionTerm().createContextRangePoint(), astExprNodeMap, astPatternNodeMap, propertyEvalSpec, false);
                }

                return new ContextSpecKeyed(rawSpecs, optionalInit, optionalTermination);
            }

            if (ctx.COALESCE() != null)
            {
                // hash
                IList<EsperEPL2GrammarParser.CreateContextCoalesceItemContext> coalesces = ctx.createContextCoalesceItem();
                IList<ContextSpecHashItem> rawSpecs = new List<ContextSpecHashItem>(coalesces.Count);
                foreach (var coalesce in coalesces)
                {
                    var chain = ASTChainSpecHelper.GetChainables(coalesce.chainable(), astExprNodeMap);
                    var func = chain[0];
                    var filterSpec = ASTFilterSpecHelper.WalkFilterSpec(coalesce.eventFilterExpression(), propertyEvalSpec, astExprNodeMap);
                    propertyEvalSpec = null;
                    rawSpecs.Add(new ContextSpecHashItem(func, filterSpec));
                }

                var granularity = ctx.g.Text;
                if (!granularity.ToLowerInvariant().Equals("granularity"))
                {
                    throw ASTWalkException.From("Expected 'granularity' keyword after list of coalesce items, found '" + granularity + "' instead");
                }

                var num = ASTConstantHelper.Parse(ctx.number());
                var preallocateStr = ctx.p?.Text;
                if (preallocateStr != null && !preallocateStr.ToLowerInvariant().Equals("preallocate"))
                {
                    throw ASTWalkException.From(
                        "Expected 'preallocate' keyword after list of coalesce items, found '" + preallocateStr + "' instead");
                }

                if (!num.GetType().IsNumericNonFP() || num.GetType().GetBoxedType() == typeof(long?))
                {
                    throw ASTWalkException.From("Granularity provided must be an int-type number, received " + num.GetType() + " instead");
                }

                return new ContextSpecHash(rawSpecs, num.AsInt32(), preallocateStr != null);
            }

            // categorized
            if (ctx.createContextGroupItem() != null)
            {
                IList<EsperEPL2GrammarParser.CreateContextGroupItemContext> grps = ctx.createContextGroupItem();
                IList<ContextSpecCategoryItem> items = new List<ContextSpecCategoryItem>();
                foreach (var grp in grps)
                {
                    var exprNode = ASTExprHelper.ExprCollectSubNodes(grp, 0, astExprNodeMap)[0];
                    var name = grp.i.Text;
                    items.Add(new ContextSpecCategoryItem(exprNode, name));
                }

                var filterSpec = ASTFilterSpecHelper.WalkFilterSpec(ctx.eventFilterExpression(), propertyEvalSpec, astExprNodeMap);
                return new ContextSpecCategory(items, filterSpec);
            }

            throw new IllegalStateException("Unrecognized context detail type");
        }

        private static IList<ContextSpecConditionFilter> GetContextPartitionInit(
            IList<EsperEPL2GrammarParser.CreateContextFilterContext> ctxs,
            IDictionary<ITree, ExprNode> astExprNodeMap)
        {
            IList<ContextSpecConditionFilter> filters = new List<ContextSpecConditionFilter>(ctxs.Count);
            foreach (var ctx in ctxs)
            {
                filters.Add(GetContextDetailConditionFilter(ctx, null, astExprNodeMap));
            }

            return filters;
        }

        private static ContextSpecCondition GetContextCondition(
            EsperEPL2GrammarParser.CreateContextRangePointContext ctx,
            IDictionary<ITree, ExprNode> astExprNodeMap,
            IDictionary<ITree, EvalForgeNode> astPatternNodeMap,
            PropertyEvalSpec propertyEvalSpec,
            bool immediate)
        {
            if (ctx == null)
            {
                return ContextSpecConditionNever.INSTANCE;
            }

            if (ctx.crontabLimitParameterSetList() != null) {
                var crontabs = new List<IList<ExprNode>>();
                foreach (var crontabCtx in ctx.crontabLimitParameterSetList().crontabLimitParameterSet()) {
                    var crontab = ASTExprHelper.ExprCollectSubNodes(crontabCtx, 0, astExprNodeMap);
                    crontabs.Add(crontab);
                }
                return new ContextSpecConditionCrontab(crontabs, immediate);
            }

            if (ctx.patternInclusionExpression() != null)
            {
                var evalNode = ASTExprHelper.PatternGetRemoveTopNode(ctx.patternInclusionExpression(), astPatternNodeMap);
                var inclusive = false;
                if (ctx.i != null)
                {
                    var ident = ctx.i.Text;
                    if (ident != null && !ident.ToLowerInvariant().Equals("inclusive"))
                    {
                        throw ASTWalkException.From("Expected 'inclusive' keyword after '@', found '" + ident + "' instead");
                    }

                    inclusive = true;
                }

                return new ContextSpecConditionPattern(evalNode, inclusive, immediate);
            }

            if (ctx.createContextFilter() != null)
            {
                if (immediate)
                {
                    throw ASTWalkException.From("Invalid use of 'now' with initiated-by stream, this combination is not supported");
                }

                return GetContextDetailConditionFilter(ctx.createContextFilter(), propertyEvalSpec, astExprNodeMap);
            }

            if (ctx.AFTER() != null)
            {
                var timePeriod = (ExprTimePeriod) ASTExprHelper.ExprCollectSubNodes(ctx.timePeriod(), 0, astExprNodeMap)[0];
                return new ContextSpecConditionTimePeriod(timePeriod, immediate);
            }

            throw new IllegalStateException("Unrecognized child type");
        }

        private static ContextSpecConditionFilter GetContextDetailConditionFilter(
            EsperEPL2GrammarParser.CreateContextFilterContext ctx,
            PropertyEvalSpec propertyEvalSpec,
            IDictionary<ITree, ExprNode> astExprNodeMap)
        {
            var filterSpecRaw = ASTFilterSpecHelper.WalkFilterSpec(ctx.eventFilterExpression(), propertyEvalSpec, astExprNodeMap);
            var asName = ctx.i?.Text;
            return new ContextSpecConditionFilter(filterSpecRaw, asName);
        }

        private static bool CheckNow(IToken i)
        {
            if (i == null)
            {
                return false;
            }

            var ident = i.Text;
            if (!ident.ToLowerInvariant().Equals("now"))
            {
                throw ASTWalkException.From("Expected 'now' keyword after '@', found '" + ident + "' instead");
            }

            return true;
        }
    }
} // end of namespace