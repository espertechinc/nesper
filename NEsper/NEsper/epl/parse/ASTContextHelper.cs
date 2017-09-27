///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.epl.generated;
using com.espertech.esper.epl.spec;
using com.espertech.esper.pattern;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.parse
{
    public class ASTContextHelper
    {
        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static CreateContextDesc WalkCreateContext(
            EsperEPL2GrammarParser.CreateContextExprContext ctx,
            IDictionary<ITree, ExprNode> astExprNodeMap,
            IDictionary<ITree, EvalFactoryNode> astPatternNodeMap,
            PropertyEvalSpec propertyEvalSpec,
            FilterSpecRaw filterSpec)
        {
            var contextName = ctx.name.Text;
            ContextDetail contextDetail;

            var choice = ctx.createContextDetail().createContextChoice();
            if (choice != null)
            {
                contextDetail = WalkChoice(choice, astExprNodeMap, astPatternNodeMap, propertyEvalSpec, filterSpec);
            }
            else
            {
                contextDetail = WalkNested(
                    ctx.createContextDetail().contextContextNested(), astExprNodeMap, astPatternNodeMap,
                    propertyEvalSpec, filterSpec);
            }
            return new CreateContextDesc(contextName, contextDetail);
        }

        private static ContextDetail WalkNested(
            IList<EsperEPL2GrammarParser.ContextContextNestedContext> nestedContexts,
            IDictionary<ITree, ExprNode> astExprNodeMap,
            IDictionary<ITree, EvalFactoryNode> astPatternNodeMap,
            PropertyEvalSpec propertyEvalSpec,
            FilterSpecRaw filterSpec)
        {
            var contexts = new List<CreateContextDesc>(nestedContexts.Count);
            foreach (var nestedctx in nestedContexts)
            {
                var contextDetail = WalkChoice(
                    nestedctx.createContextChoice(), astExprNodeMap, astPatternNodeMap, propertyEvalSpec, filterSpec);
                var desc = new CreateContextDesc(nestedctx.name.Text, contextDetail);
                contexts.Add(desc);
            }
            return new ContextDetailNested(contexts);
        }

        private static ContextDetail WalkChoice(
            EsperEPL2GrammarParser.CreateContextChoiceContext ctx,
            IDictionary<ITree, ExprNode> astExprNodeMap,
            IDictionary<ITree, EvalFactoryNode> astPatternNodeMap,
            PropertyEvalSpec propertyEvalSpec,
            FilterSpecRaw filterSpec)
        {

            // temporal fixed (start+end) and overlapping (initiated/terminated)
            if (ctx.START() != null || ctx.INITIATED() != null)
            {

                ExprNode[] distinctExpressions = null;
                if (ctx.createContextDistinct() != null)
                {
                    if (ctx.createContextDistinct().expressionList() == null)
                    {
                        distinctExpressions = new ExprNode[0];
                    }
                    else
                    {
                        distinctExpressions =
                            ASTExprHelper.ExprCollectSubNodesPerNode(
                                ctx.createContextDistinct().expressionList().expression(), astExprNodeMap);
                    }
                }

                ContextDetailCondition startEndpoint;
                if (ctx.START() != null)
                {
                    var immediate = CheckNow(ctx.i);
                    if (immediate)
                    {
                        startEndpoint = ContextDetailConditionImmediate.INSTANCE;
                    }
                    else
                    {
                        startEndpoint = GetContextCondition(
                            ctx.r1, astExprNodeMap, astPatternNodeMap, propertyEvalSpec, false);
                    }
                }
                else
                {
                    var immediate = CheckNow(ctx.i);
                    startEndpoint = GetContextCondition(
                        ctx.r1, astExprNodeMap, astPatternNodeMap, propertyEvalSpec, immediate);
                }

                var overlapping = ctx.INITIATED() != null;
                var endEndpoint = GetContextCondition(
                    ctx.r2, astExprNodeMap, astPatternNodeMap, propertyEvalSpec, false);
                return new ContextDetailInitiatedTerminated(
                    startEndpoint, endEndpoint, overlapping, distinctExpressions);
            }

            // partitioned
            if (ctx.PARTITION() != null)
            {
                IList<EsperEPL2GrammarParser.CreateContextPartitionItemContext> partitions =
                    ctx.createContextPartitionItem();
                var rawSpecs = new List<ContextDetailPartitionItem>();
                foreach (var partition in partitions)
                {

                    filterSpec = ASTFilterSpecHelper.WalkFilterSpec(
                        partition.eventFilterExpression(), propertyEvalSpec, astExprNodeMap);
                    propertyEvalSpec = null;

                    var propertyNames = new List<string>();
                    IList<EsperEPL2GrammarParser.EventPropertyContext> properties = partition.eventProperty();
                    foreach (var property in properties)
                    {
                        var propertyName = ASTUtil.GetPropertyName(property, 0);
                        propertyNames.Add(propertyName);
                    }
                    ASTExprHelper.ExprCollectSubNodes(partition, 0, astExprNodeMap); // remove expressions

                    rawSpecs.Add(new ContextDetailPartitionItem(filterSpec, propertyNames));
                }
                return new ContextDetailPartitioned(rawSpecs);
            }
            else if (ctx.COALESCE() != null)
            {
                // hash
                IList<EsperEPL2GrammarParser.CreateContextCoalesceItemContext> coalesces =
                    ctx.createContextCoalesceItem();
                var rawSpecs = new List<ContextDetailHashItem>(coalesces.Count);
                foreach (var coalesce in coalesces)
                {
                    var func = ASTLibFunctionHelper.GetLibFunctionChainSpec(
                        coalesce.libFunctionNoClass(), astExprNodeMap);
                    filterSpec = ASTFilterSpecHelper.WalkFilterSpec(
                        coalesce.eventFilterExpression(), propertyEvalSpec, astExprNodeMap);
                    propertyEvalSpec = null;
                    rawSpecs.Add(new ContextDetailHashItem(func, filterSpec));
                }

                var granularity = ctx.g.Text;
                if (!granularity.ToLowerInvariant().Equals("granularity"))
                {
                    throw ASTWalkException.From(
                        "Expected 'granularity' keyword after list of coalesce items, found '" + granularity +
                        "' instead");
                }
                var num = ASTConstantHelper.Parse(ctx.number());
                var preallocateStr = ctx.p != null ? ctx.p.Text : null;
                if (preallocateStr != null && !preallocateStr.ToLowerInvariant().Equals("preallocate"))
                {
                    throw ASTWalkException.From(
                        "Expected 'preallocate' keyword after list of coalesce items, found '" + preallocateStr +
                        "' instead");
                }
                if (!TypeHelper.IsNumericNonFP(num.GetType()) || TypeHelper.GetBoxedType(num.GetType()) == typeof (long?))
                {
                    throw ASTWalkException.From(
                        "Granularity provided must be an int-type number, received " + num.GetType() + " instead");
                }

                return new ContextDetailHash(rawSpecs, num.AsInt(), preallocateStr != null);
            }

            // categorized
            if (ctx.createContextGroupItem() != null)
            {
                IList<EsperEPL2GrammarParser.CreateContextGroupItemContext> grps = ctx.createContextGroupItem();
                var items = new List<ContextDetailCategoryItem>();
                foreach (var grp in grps)
                {
                    var exprNode = ASTExprHelper.ExprCollectSubNodes(grp, 0, astExprNodeMap)[0];
                    var name = grp.i.Text;
                    items.Add(new ContextDetailCategoryItem(exprNode, name));
                }
                filterSpec = ASTFilterSpecHelper.WalkFilterSpec(
                    ctx.eventFilterExpression(), propertyEvalSpec, astExprNodeMap);
                return new ContextDetailCategory(items, filterSpec);
            }

            throw new IllegalStateException("Unrecognized context detail type");
        }

        private static ContextDetailCondition GetContextCondition(
            EsperEPL2GrammarParser.CreateContextRangePointContext ctx,
            IDictionary<ITree, ExprNode> astExprNodeMap,
            IDictionary<ITree, EvalFactoryNode> astPatternNodeMap,
            PropertyEvalSpec propertyEvalSpec,
            bool immediate)
        {
            if (ctx == null)
            {
                return ContextDetailConditionNever.INSTANCE;
            }
            if (ctx.crontabLimitParameterSet() != null)
            {
                var crontab = ASTExprHelper.ExprCollectSubNodes(
                    ctx.crontabLimitParameterSet(), 0, astExprNodeMap);
                return new ContextDetailConditionCrontab(crontab, immediate);
            }
            else if (ctx.patternInclusionExpression() != null)
            {
                var evalNode = ASTExprHelper.PatternGetRemoveTopNode(
                    ctx.patternInclusionExpression(), astPatternNodeMap);
                var inclusive = false;
                if (ctx.i != null)
                {
                    var ident = ctx.i.Text;
                    if (ident != null && !ident.ToLowerInvariant().Equals("inclusive"))
                    {
                        throw ASTWalkException.From(
                            "Expected 'inclusive' keyword after '@', found '" + ident + "' instead");
                    }
                    inclusive = true;
                }
                return new ContextDetailConditionPattern(evalNode, inclusive, immediate);
            }
            else if (ctx.createContextFilter() != null)
            {
                var filterSpecRaw =
                    ASTFilterSpecHelper.WalkFilterSpec(
                        ctx.createContextFilter().eventFilterExpression(), propertyEvalSpec, astExprNodeMap);
                var asName = ctx.createContextFilter().i != null ? ctx.createContextFilter().i.Text : null;
                if (immediate)
                {
                    throw ASTWalkException.From(
                        "Invalid use of 'now' with initiated-by stream, this combination is not supported");
                }
                return new ContextDetailConditionFilter(filterSpecRaw, asName);
            }
            else if (ctx.AFTER() != null)
            {
                var timePeriod =
                    (ExprTimePeriod) ASTExprHelper.ExprCollectSubNodes(ctx.timePeriod(), 0, astExprNodeMap)[0];
                return new ContextDetailConditionTimePeriod(timePeriod, immediate);
            }
            else
            {
                throw new IllegalStateException("Unrecognized child type");
            }
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
