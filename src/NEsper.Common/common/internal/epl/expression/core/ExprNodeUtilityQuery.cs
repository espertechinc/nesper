///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.chain;
using com.espertech.esper.common.@internal.epl.expression.declared.compiletime;
using com.espertech.esper.common.@internal.epl.expression.visitor;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public class ExprNodeUtilityQuery
    {
        public static readonly ExprNode[] EMPTY_EXPR_ARRAY = new ExprNode[0];
        public static readonly ExprForge[] EMPTY_FORGE_ARRAY = new ExprForge[0];

        public static ExprForge[] ForgesForProperties(
            IList<EventType> eventTypes,
            String[] propertyNames,
            int[] keyStreamNums)
        {
            ExprForge[] forges = new ExprForge[propertyNames.Length];
            for (int i = 0; i < propertyNames.Length; i++) {
                ExprIdentNodeImpl node = new ExprIdentNodeImpl(eventTypes[keyStreamNums[i]], propertyNames[i], keyStreamNums[i]);
                forges[i] = node.Forge;
            }

            return forges;
        }

        public static bool IsConstant(ExprNode exprNode)
        {
            return exprNode.Forge.ForgeConstantType.IsConstant;
        }

        public static ISet<string> GetPropertyNamesIfAllProps(IList<ExprNode> expressions)
        {
            foreach (var expression in expressions) {
                if (!(expression is ExprIdentNode)) {
                    return null;
                }
            }

            ISet<string> uniquePropertyNames = new HashSet<string>();
            foreach (var expression in expressions) {
                var identNode = (ExprIdentNode) expression;
                uniquePropertyNames.Add(identNode.UnresolvedPropertyName);
            }

            return uniquePropertyNames;
        }

        public static IList<Pair<ExprNode, ExprNode>> FindExpression(
            ExprNode selectExpression,
            ExprNode searchExpression)
        {
            IList<Pair<ExprNode, ExprNode>> pairs = new List<Pair<ExprNode, ExprNode>>();
            if (ExprNodeUtilityCompare.DeepEquals(selectExpression, searchExpression, false)) {
                pairs.Add(new Pair<ExprNode, ExprNode>(null, selectExpression));
                return pairs;
            }

            FindExpressionChildRecursive(selectExpression, searchExpression, pairs);
            return pairs;
        }

        private static void FindExpressionChildRecursive(
            ExprNode parent,
            ExprNode searchExpression,
            IList<Pair<ExprNode, ExprNode>> pairs)
        {
            foreach (var child in parent.ChildNodes) {
                if (ExprNodeUtilityCompare.DeepEquals(child, searchExpression, false)) {
                    pairs.Add(new Pair<ExprNode, ExprNode>(parent, child));
                    continue;
                }

                FindExpressionChildRecursive(child, searchExpression, pairs);
            }
        }

        public static string[] GetIdentResolvedPropertyNames(IList<ExprNode> nodes)
        {
            var propertyNames = new string[nodes.Count];
            for (var i = 0; i < propertyNames.Length; i++) {
                if (!(nodes[i] is ExprIdentNode)) {
                    throw new ArgumentException("Expressions are not ident nodes");
                }

                propertyNames[i] = ((ExprIdentNode) nodes[i]).ResolvedPropertyName;
            }

            return propertyNames;
        }

        public static Type[] GetExprResultTypes(IList<ExprForge> nodes)
        {
            var types = new Type[nodes.Count];
            for (var i = 0; i < types.Length; i++) {
                types[i] = nodes[i].EvaluationType;
            }

            return types;
        }

        public static ExprNode[] ToArray(ICollection<ExprNode> expressions)
        {
            if (expressions.IsEmpty()) {
                return EMPTY_EXPR_ARRAY;
            }

            return expressions.ToArray();
        }

        public static ExprForge[] GetForges(IList<ExprNode> exprNodes)
        {
            if (exprNodes == null) {
                return null;
            }

            var forge = new ExprForge[exprNodes.Count];
            for (var i = 0; i < exprNodes.Count; i++) {
                var node = exprNodes[i];
                if (node != null) {
                    forge[i] = node.Forge;
                }
            }

            return forge;
        }

        public static ExprEvaluator[] GetEvaluatorsNoCompile(IList<ExprForge> forges)
        {
            if (forges == null) {
                return null;
            }

            var eval = new ExprEvaluator[forges.Count];
            for (var i = 0; i < forges.Count; i++) {
                var forge = forges[i];
                if (forge != null) {
                    eval[i] = forge.ExprEvaluator;
                }
            }

            return eval;
        }

        public static ExprEvaluator[] GetEvaluatorsNoCompile(IList<ExprNode> childNodes)
        {
            var eval = new ExprEvaluator[childNodes.Count];
            for (var i = 0; i < childNodes.Count; i++) {
                eval[i] = childNodes[i].Forge.ExprEvaluator;
            }

            return eval;
        }

        public static Type[] GetExprResultTypes(IList<ExprNode> expressions)
        {
            var returnTypes = new Type[expressions.Count];
            for (var i = 0; i < expressions.Count; i++) {
                returnTypes[i] = expressions[i].Forge.EvaluationType;
            }

            return returnTypes;
        }

        public static void AcceptParams(
            ExprNodeVisitor visitor,
            IList<ExprNode> @params)
        {
            foreach (var param in @params) {
                param.Accept(visitor);
            }
        }

        public static void AcceptParams(
            ExprNodeVisitorWithParent visitor,
            IList<ExprNode> @params)
        {
            foreach (var param in @params) {
                param.Accept(visitor);
            }
        }

        public static void AcceptParams(
            ExprNodeVisitorWithParent visitor,
            IList<ExprNode> @params,
            ExprNode parent)
        {
            foreach (var param in @params) {
                param.AcceptChildnodes(visitor, parent);
            }
        }

        public static string[] GetPropertiesPerExpressionExpectSingle(IList<ExprNode> exprNodes)
        {
            var indexedProperties = new string[exprNodes.Count];
            for (var i = 0; i < exprNodes.Count; i++) {
                var visitor = new ExprNodeIdentifierVisitor(true);
                exprNodes[i].Accept(visitor);
                if (visitor.ExprProperties.Count != 1) {
                    throw new IllegalStateException("Failed to find indexed property");
                }

                indexedProperties[i] = visitor.ExprProperties.First().Second;
            }

            return indexedProperties;
        }

        public static bool IsExpressionsAllPropsOnly(IList<ExprNode> exprNodes)
        {
            for (var i = 0; i < exprNodes.Count; i++) {
                if (!(exprNodes[i] is ExprIdentNode)) {
                    return false;
                }
            }

            return true;
        }

        public static ISet<int> GetIdentStreamNumbers(ExprNode child)
        {
            ISet<int> streams = new HashSet<int>();
            var visitor = new ExprNodeIdentifierCollectVisitor();
            child.Accept(visitor);
            foreach (var node in visitor.ExprProperties) {
                streams.Add(node.StreamId);
            }

            return streams;
        }

        public static IList<Pair<int, string>> GetExpressionProperties(
            ExprNode exprNode,
            bool visitAggregateNodes)
        {
            var visitor = new ExprNodeIdentifierVisitor(visitAggregateNodes);
            exprNode.Accept(visitor);
            return visitor.ExprProperties;
        }

        public static bool IsAllConstants(IList<ExprNode> parameters)
        {
            foreach (var node in parameters) {
                if (!node.Forge.ForgeConstantType.IsCompileTimeConstant) {
                    return false;
                }
            }

            return true;
        }

        public static bool HasStreamSelect(IList<ExprNode> exprNodes)
        {
            var visitor = new ExprNodeStreamSelectVisitor(false);
            foreach (var node in exprNodes) {
                node.Accept(visitor);
                if (visitor.HasStreamSelect) {
                    return true;
                }
            }

            return false;
        }

        public static IList<ExprNode> CollectChainParameters(IList<Chainable> chainSpec)
        {
            IList<ExprNode> result = new List<ExprNode>();
            foreach (var chainElement in chainSpec) {
                chainElement.AddParametersTo(result);
            }

            return result;
        }

        public static void AcceptChain(
            ExprNodeVisitor visitor,
            IList<Chainable> chainSpec)
        {
            foreach (var chain in chainSpec) {
                chain.Accept(visitor);
            }
        }

        public static void AcceptChain(
            ExprNodeVisitorWithParent visitor,
            IList<Chainable> chainSpec)
        {
            foreach (var chain in chainSpec) {
                chain.Accept(visitor);
            }
        }

        public static void AcceptChain(
            ExprNodeVisitorWithParent visitor,
            IList<Chainable> chainSpec,
            ExprNode parent)
        {
            foreach (var chain in chainSpec) {
                chain.Accept(visitor, parent);
            }
        }

        public static IDictionary<ExprDeclaredNode, IList<ExprDeclaredNode>> GetDeclaredExpressionCallHierarchy(
            ExprDeclaredNode[] declaredExpressions)
        {
            var visitor = new ExprNodeSubselectDeclaredDotVisitor();
            IDictionary<ExprDeclaredNode, IList<ExprDeclaredNode>> calledToCallerMap =
                new Dictionary<ExprDeclaredNode, IList<ExprDeclaredNode>>();
            foreach (var node in declaredExpressions) {
                visitor.Reset();
                node.AcceptNoVisitParams(visitor);
                foreach (var called in visitor.DeclaredExpressions) {
                    if (called == node) {
                        continue;
                    }

                    var callers = calledToCallerMap.Get(called);
                    if (callers == null) {
                        callers = new List<ExprDeclaredNode>(2);
                        calledToCallerMap.Put(called, callers);
                    }

                    callers.Add(node);
                }

                if (!calledToCallerMap.ContainsKey(node)) {
                    calledToCallerMap.Put(node, Collections.GetEmptyList<ExprDeclaredNode>());
                }
            }

            return calledToCallerMap;
        }
    }
} // end of namespace