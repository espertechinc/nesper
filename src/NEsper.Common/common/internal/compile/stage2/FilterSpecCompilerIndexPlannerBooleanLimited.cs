///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using Antlr4.Runtime.Sharpen;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.chain;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.filter;
using com.espertech.esper.common.@internal.epl.expression.visitor;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.compile.stage2.FilterSpecCompilerIndexPlannerHelper; //getMatchEventConvertor;

//hasLevelOrHint;

namespace com.espertech.esper.common.@internal.compile.stage2
{
    public class FilterSpecCompilerIndexPlannerBooleanLimited
    {
        internal static FilterSpecParamForge HandleBooleanLimited(
            ExprNode constituent,
            IDictionary<string, Pair<EventType, string>> taggedEventTypes,
            IDictionary<string, Pair<EventType, string>> arrayEventTypes,
            ISet<string> allTagNamesOrdered,
            StreamTypeService streamTypeService,
            StatementRawInfo raw,
            StatementCompileTimeServices services)
        {
            if (!HasLevelOrHint(FilterSpecCompilerIndexPlannerHint.BOOLCOMPOSITE, raw, services)) {
                return null;
            }

            // prequalify
            var prequalified = Prequalify(constituent);
            if (!prequalified) {
                return null;
            }

            // determine rewrite
            var desc = FindRewrite(constituent);
            if (desc == null) {
                return null;
            }

            // there is no value expression, i.e. "select * from SupportBean(theString = intPrimitive)"
            if (desc is RewriteDescriptorNoValueExpr) {
                var reboolExpression = ExprNodeUtilityPrint.ToExpressionStringMinPrecedence(constituent, new ExprNodeRenderableFlags(false));
                var lookupable = new ExprFilterSpecLookupableForge(reboolExpression, null, constituent.Forge, null, true, null);
                return new FilterSpecParamValueNullForge(lookupable, FilterOperator.REBOOL);
            }

            // there is no value expression, i.e. "select * from SupportBean(theString regexp 'abc')"
            var withValueExpr = (RewriteDescriptorWithValueExpr) desc;
            ExprNode valueExpression = withValueExpr.ValueExpression;
            var valueExpressionType = valueExpression.Forge.EvaluationType;
            var replacement = new ExprFilterReboolValueNode(valueExpressionType);
            ExprNodeUtilityModify.ReplaceChildNode(withValueExpr.ValueExpressionParent, valueExpression, replacement);
            var validationContext = new ExprValidationContextBuilder(streamTypeService, raw, services).WithIsFilterExpression(true).Build();
            var rebool = ExprNodeUtilityValidate.GetValidatedSubtree(ExprNodeOrigin.FILTER, constituent, validationContext);
            DataInputOutputSerdeForge serde = services.SerdeResolver.SerdeForFilter(valueExpressionType, raw);
            var convertor = GetMatchEventConvertor(valueExpression, taggedEventTypes, arrayEventTypes, allTagNamesOrdered);

            var reboolExpressionX = ExprNodeUtilityPrint.ToExpressionStringMinPrecedence(constituent, new ExprNodeRenderableFlags(false));
            var lookupableX = new ExprFilterSpecLookupableForge(reboolExpressionX, null, rebool.Forge, valueExpressionType, true, serde);
            return new FilterSpecParamValueLimitedExprForge(lookupableX, FilterOperator.REBOOL, valueExpression, convertor, null);
        }

        private static bool Prequalify(ExprNode constituent)
        {
            var prequalify = new FilterSpecExprNodeVisitorBooleanLimitedExprPrequalify();
            constituent.Accept(prequalify);
            if (!prequalify.IsLimited) {
                return false;
            }

            var streamRefVisitor = new ExprNodeIdentifierAndStreamRefVisitor(false);
            constituent.Accept(streamRefVisitor);
            var hasStreamRefZero = false;
            foreach (var @ref in streamRefVisitor.Refs) {
                if (@ref.StreamNum == 0) {
                    hasStreamRefZero = true;
                    break;
                }
            }

            return hasStreamRefZero && !streamRefVisitor.HasWildcardOrStreamAlias;
        }

        private static RewriteDescriptor FindRewrite(ExprNode parent)
        {
            IList<ExprNodeWithParentPair> valueExpressions = FindValueExpressionsDeepMayNull(parent);
            if (valueExpressions == null) {
                return new RewriteDescriptorNoValueExpr();
            }

            if (valueExpressions.Count == 1) {
                ExprNodeWithParentPair pair = valueExpressions[0];
                return new RewriteDescriptorWithValueExpr(pair.Node, pair.Parent);
            }

            // find a single value expression that is non-deploy-time-constant
            IList<ExprNodeWithParentPair> nonConstants = new List<ExprNodeWithParentPair>(valueExpressions.Count);
            foreach (ExprNodeWithParentPair expr in valueExpressions) {
                if (!expr.Node.Forge.ForgeConstantType.IsConstant) {
                    nonConstants.Add(expr);
                }
            }

            if (nonConstants.Count == 1) {
                ExprNodeWithParentPair pair = nonConstants[0];
                return new RewriteDescriptorWithValueExpr(pair.Node, pair.Parent);
            }

            // we are not handling multiple non-constant value expressions
            return null;
        }

        private static IList<ExprNodeWithParentPair> FindValueExpressionsDeepMayNull(ExprNode parent)
        {
            AtomicReference<IList<ExprNodeWithParentPair>> pairs = new AtomicReference<IList<ExprNodeWithParentPair>>();
            FindValueExpressionsDeepRecursive(parent, pairs);
            return pairs.Get();
        }

        private static void FindValueExpressionsDeepRecursive(
            ExprNode parent,
            AtomicReference<IList<ExprNodeWithParentPair>> pairsRef)
        {
            foreach (var child in parent.ChildNodes) {
                FindValueExpr(child, parent, pairsRef);
            }

            if (parent is ExprNodeWithChainSpec) {
                ExprNodeWithChainSpec chainableNode = (ExprNodeWithChainSpec) parent;
                foreach (Chainable chainable in chainableNode.ChainSpec) {
                    foreach (ExprNode param in chainable.GetParametersOrEmpty()) {
                        FindValueExpr(param, parent, pairsRef);
                    }
                }
            }
        }

        private static void FindValueExpr(
            ExprNode child,
            ExprNode parent,
            AtomicReference<IList<ExprNodeWithParentPair>> pairsRef)
        {
            var valueVisitor = new FilterSpecExprNodeVisitorValueLimitedExpr();
            child.Accept(valueVisitor);

            // not by itself a value expression, but itself it may decompose into some value expressions
            if (!valueVisitor.IsLimited) {
                FindValueExpressionsDeepRecursive(child, pairsRef);
                return;
            }

            // add value expression, don't traverse child
            IList<ExprNodeWithParentPair> pairs = pairsRef.Get();
            if (pairs == null) {
                pairs = new List<ExprNodeWithParentPair>(2);
                pairsRef.Set(pairs);
            }

            pairs.Add(new ExprNodeWithParentPair(child, parent));
        }

        private abstract class RewriteDescriptor
        {
        }

        private class RewriteDescriptorNoValueExpr : RewriteDescriptor
        {
        }

        private class RewriteDescriptorWithValueExpr : RewriteDescriptor
        {
            public RewriteDescriptorWithValueExpr(
                ExprNode valueExpression,
                ExprNode valueExpressionParent)
            {
                ValueExpression = valueExpression;
                ValueExpressionParent = valueExpressionParent;
            }

            public ExprNode ValueExpression { get; }

            public ExprNode ValueExpressionParent { get; }
        }
    }
} // end of namespace