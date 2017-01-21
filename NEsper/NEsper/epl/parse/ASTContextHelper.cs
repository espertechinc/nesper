///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

using com.espertech.esper.compat;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.epl.generated;
using com.espertech.esper.epl.spec;
using com.espertech.esper.pattern;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.parse
{
    public class ASTContextHelper
    {
        public static CreateContextDesc WalkCreateContext(EsperEPL2GrammarParser.CreateContextExprContext ctx, IDictionary<ITree, ExprNode> astExprNodeMap, IDictionary<ITree, EvalFactoryNode> astPatternNodeMap, PropertyEvalSpec propertyEvalSpec, FilterSpecRaw filterSpec)
        {
            String contextName = ctx.name.Text;
            ContextDetail contextDetail;
    
            EsperEPL2GrammarParser.CreateContextChoiceContext choice = ctx.createContextDetail().createContextChoice();
            if (choice != null) {
                contextDetail = WalkChoice(choice, astExprNodeMap, astPatternNodeMap, propertyEvalSpec, filterSpec);
            }
            else {
                contextDetail = WalkNested(ctx.createContextDetail().contextContextNested(), astExprNodeMap, astPatternNodeMap, propertyEvalSpec, filterSpec);
            }
            return new CreateContextDesc(contextName, contextDetail);
        }

        private static ContextDetail WalkNested(IList<EsperEPL2GrammarParser.ContextContextNestedContext> nestedContexts, IDictionary<ITree, ExprNode> astExprNodeMap, IDictionary<ITree, EvalFactoryNode> astPatternNodeMap, PropertyEvalSpec propertyEvalSpec, FilterSpecRaw filterSpec)
        {
            IList<CreateContextDesc> contexts = new List<CreateContextDesc>(nestedContexts.Count);
            foreach (EsperEPL2GrammarParser.ContextContextNestedContext nestedctx in nestedContexts) {
                ContextDetail contextDetail = WalkChoice(nestedctx.createContextChoice(), astExprNodeMap, astPatternNodeMap, propertyEvalSpec, filterSpec);
                CreateContextDesc desc = new CreateContextDesc(nestedctx.name.Text, contextDetail);
                contexts.Add(desc);
            }
            return new ContextDetailNested(contexts);
        }
    
        private static ContextDetail WalkChoice(EsperEPL2GrammarParser.CreateContextChoiceContext ctx, IDictionary<ITree, ExprNode> astExprNodeMap, IDictionary<ITree, EvalFactoryNode> astPatternNodeMap, PropertyEvalSpec propertyEvalSpec, FilterSpecRaw filterSpec) {
    
            // temporal fixed (start+end) and overlapping (initiated/terminated)
            if (ctx.START() != null || ctx.INITIATED() != null) {
    
                ExprNode[] distinctExpressions = null;
                if (ctx.createContextDistinct() != null) {
                    if (ctx.createContextDistinct().expressionList() == null) {
                        distinctExpressions = new ExprNode[0];
                    }
                    else {
                        distinctExpressions = ASTExprHelper.ExprCollectSubNodesPerNode(ctx.createContextDistinct().expressionList().expression(), astExprNodeMap);
                    }
                }
    
                ContextDetailCondition startEndpoint;
                if (ctx.START() != null) {
                    bool immediate = CheckNow(ctx.i);
                    if (immediate) {
                        startEndpoint = new ContextDetailConditionImmediate();
                    }
                    else {
                        startEndpoint = GetContextCondition(ctx.r1, astExprNodeMap, astPatternNodeMap, propertyEvalSpec, false);
                    }
                }
                else {
                    bool immediate = CheckNow(ctx.i);
                    startEndpoint = GetContextCondition(ctx.r1, astExprNodeMap, astPatternNodeMap, propertyEvalSpec, immediate);
                }
    
                bool overlapping = ctx.INITIATED() != null;
                ContextDetailCondition endEndpoint = GetContextCondition(ctx.r2, astExprNodeMap, astPatternNodeMap, propertyEvalSpec, false);
                return new ContextDetailInitiatedTerminated(startEndpoint, endEndpoint, overlapping, distinctExpressions);
            }
    
            // partitioned
            if (ctx.PARTITION() != null){
                IList<EsperEPL2GrammarParser.CreateContextPartitionItemContext> partitions = ctx.createContextPartitionItem();
                IList<ContextDetailPartitionItem> rawSpecs = new List<ContextDetailPartitionItem>();
                foreach (EsperEPL2GrammarParser.CreateContextPartitionItemContext partition in partitions) {
    
                    filterSpec = ASTFilterSpecHelper.WalkFilterSpec(partition.eventFilterExpression(), propertyEvalSpec, astExprNodeMap);
                    propertyEvalSpec = null;
    
                    IList<String> propertyNames = new List<String>();
                    IList<EsperEPL2GrammarParser.EventPropertyContext> properties = partition.eventProperty();
                    foreach (EsperEPL2GrammarParser.EventPropertyContext property in properties) {
                        String propertyName = ASTUtil.GetPropertyName(property, 0);
                        propertyNames.Add(propertyName);
                    }
                    ASTExprHelper.ExprCollectSubNodes(partition, 0, astExprNodeMap); // remove expressions
    
                    rawSpecs.Add(new ContextDetailPartitionItem(filterSpec, propertyNames));
                }
                return new ContextDetailPartitioned(rawSpecs);
            }
    
            // hash
            else if (ctx.COALESCE() != null){
                IList<EsperEPL2GrammarParser.CreateContextCoalesceItemContext> coalesces = ctx.createContextCoalesceItem();
                IList<ContextDetailHashItem> rawSpecs = new List<ContextDetailHashItem>(coalesces.Count);
                foreach (EsperEPL2GrammarParser.CreateContextCoalesceItemContext coalesce in coalesces) {
                    ExprChainedSpec func = ASTLibFunctionHelper.GetLibFunctionChainSpec(coalesce.libFunctionNoClass(), astExprNodeMap);
                    filterSpec = ASTFilterSpecHelper.WalkFilterSpec(coalesce.eventFilterExpression(), propertyEvalSpec, astExprNodeMap);
                    propertyEvalSpec = null;
                    rawSpecs.Add(new ContextDetailHashItem(func, filterSpec));
                }
    
                String granularity = ctx.g.Text;
                if (!granularity.ToLower().Equals("granularity")) {
                    throw ASTWalkException.From("Expected 'granularity' keyword after list of coalesce items, found '" + granularity + "' instead");
                }
                var num = ASTConstantHelper.Parse(ctx.number());
                String preallocateStr = ctx.p != null ? ctx.p.Text : null;
                if (preallocateStr != null && !preallocateStr.ToLower().Equals("preallocate")) {
                    throw ASTWalkException.From("Expected 'preallocate' keyword after list of coalesce items, found '" + preallocateStr + "' instead");
                }
                if (!num.GetType().IsNumericNonFP() || num.GetType().GetBoxedType() == typeof(long?))
                {
                    throw ASTWalkException.From("Granularity provided must be an int-type number, received " + num.GetType() + " instead");
                }
    
                return new ContextDetailHash(rawSpecs, num.AsInt(), preallocateStr != null);
            }
    
            // categorized
            if (ctx.createContextGroupItem() != null){
                IList<EsperEPL2GrammarParser.CreateContextGroupItemContext> grps = ctx.createContextGroupItem();
                IList<ContextDetailCategoryItem> items = new List<ContextDetailCategoryItem>();
                foreach (EsperEPL2GrammarParser.CreateContextGroupItemContext grp in grps) {
                    ExprNode exprNode = ASTExprHelper.ExprCollectSubNodes(grp, 0, astExprNodeMap)[0];
                    String name = grp.i.Text;
                    items.Add(new ContextDetailCategoryItem(exprNode, name));
                }
                filterSpec = ASTFilterSpecHelper.WalkFilterSpec(ctx.eventFilterExpression(), propertyEvalSpec, astExprNodeMap);
                return new ContextDetailCategory(items, filterSpec);
            }
    
            throw new IllegalStateException("Unrecognized context detail type");
        }

        private static ContextDetailCondition GetContextCondition(EsperEPL2GrammarParser.CreateContextRangePointContext ctx, IDictionary<ITree, ExprNode> astExprNodeMap, IDictionary<ITree, EvalFactoryNode> astPatternNodeMap, PropertyEvalSpec propertyEvalSpec, bool immediate)
        {
            if (ctx.crontabLimitParameterSet() != null) {
                IList<ExprNode> crontab = ASTExprHelper.ExprCollectSubNodes(ctx.crontabLimitParameterSet(), 0, astExprNodeMap);
                return new ContextDetailConditionCrontab(crontab, immediate);
            }
            else if (ctx.patternInclusionExpression() != null) {
                EvalFactoryNode evalNode = ASTExprHelper.PatternGetRemoveTopNode(ctx.patternInclusionExpression(), astPatternNodeMap);
                bool inclusive = false;
                if (ctx.i != null) {
                    String ident = ctx.i.Text;
                    if (ident != null && ident.ToLower() != "inclusive") {
                        throw ASTWalkException.From("Expected 'inclusive' keyword after '@', found '" + ident + "' instead");
                    }
                    inclusive = true;
                }
                return new ContextDetailConditionPattern(evalNode, inclusive, immediate);
            }
            else if (ctx.createContextFilter() != null) {
                FilterSpecRaw filterSpecRaw = ASTFilterSpecHelper.WalkFilterSpec(ctx.createContextFilter().eventFilterExpression(), propertyEvalSpec, astExprNodeMap);
                String asName = ctx.createContextFilter().i != null ? ctx.createContextFilter().i.Text : null;
                if (immediate) {
                    throw ASTWalkException.From("Invalid use of 'now' with initiated-by stream, this combination is not supported");
                }
                return new ContextDetailConditionFilter(filterSpecRaw, asName);
            }
            else if (ctx.AFTER() != null) {
                ExprTimePeriod timePeriod = (ExprTimePeriod) ASTExprHelper.ExprCollectSubNodes(ctx.timePeriod(), 0, astExprNodeMap)[0];
                return new ContextDetailConditionTimePeriod(timePeriod, immediate);
            }
            else {
                throw new IllegalStateException("Unrecognized child type");
            }
        }
    
        private static bool CheckNow(IToken i) {
            if (i == null) {
                return false;
            }
            String ident = i.Text;
            if (ident.ToLower() != "now") {
                throw ASTWalkException.From("Expected 'now' keyword after '@', found '" + ident + "' instead");
            }
            return true;
        }
    }
}
