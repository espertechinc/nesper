///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.IO;
using System.Linq;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage1.specmapper;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.agg.rollup
{
    public class GroupByExpressionHelper
    {
        public static GroupByClauseExpressions GetGroupByRollupExpressions(
            IList<GroupByClauseElement> groupByElements,
            SelectClauseSpecRaw selectClauseSpec,
            ExprNode optionalHavingNode,
            IList<OrderByItem> orderByList,
            ExpressionCopier expressionCopier)
        {
            if (groupByElements == null || groupByElements.Count == 0) {
                return null;
            }

            // walk group-by-elements, determine group-by expressions and rollup nodes
            var groupByExpressionInfo = GroupByToRollupNodes(groupByElements);

            // obtain expression nodes, collect unique nodes and assign index
            IList<ExprNode> distinctGroupByExpressions = new List<ExprNode>();
            IDictionary<ExprNode, int> expressionToIndex = new Dictionary<ExprNode, int>();
            foreach (var exprNode in groupByExpressionInfo.Expressions) {
                var found = false;
                for (var i = 0; i < distinctGroupByExpressions.Count; i++) {
                    ExprNode other = distinctGroupByExpressions[i];
                    // find same expression
                    if (ExprNodeUtilityCompare.DeepEquals(exprNode, other, false)) {
                        expressionToIndex.Put(exprNode, i);
                        found = true;
                        break;
                    }
                }

                // not seen before
                if (!found) {
                    expressionToIndex.Put(exprNode, distinctGroupByExpressions.Count);
                    distinctGroupByExpressions.Add(exprNode);
                }
            }

            // determine rollup, validate it is either (not both)
            var hasGroupingSet = false;
            var hasRollup = false;
            foreach (var element in groupByElements) {
                if (element is GroupByClauseElementGroupingSet) {
                    hasGroupingSet = true;
                }

                if (element is GroupByClauseElementRollupOrCube) {
                    hasRollup = true;
                }
            }

            // no-rollup or grouping-sets means simply validate
            ExprNode[] groupByExpressions = distinctGroupByExpressions.ToArray();
            if (!hasRollup && !hasGroupingSet) {
                return new GroupByClauseExpressions(groupByExpressions);
            }

            // evaluate rollup node roots
            var nodes = groupByExpressionInfo.Nodes;
            var perNodeCombinations = new object[nodes.Count][];
            var context = new GroupByRollupEvalContext(expressionToIndex);
            try {
                for (var i = 0; i < nodes.Count; i++) {
                    GroupByRollupNodeBase node = nodes[i];
                    var combinations = node.Evaluate(context);
                    perNodeCombinations[i] = new object[combinations.Count];
                    for (var j = 0; j < combinations.Count; j++) {
                        perNodeCombinations[i][j] = combinations[j];
                    }
                }
            }
            catch (GroupByRollupDuplicateException ex) {
                if (ex.Indexes.Length == 0) {
                    throw new ExprValidationException(
                        "Failed to validate the group-by clause, found duplicate specification of the overall grouping '()'");
                }

                var writer = new StringWriter();
                var delimiter = "";
                for (var i = 0; i < ex.Indexes.Length; i++) {
                    writer.Write(delimiter);
                    writer.Write(
                        ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(groupByExpressions[ex.Indexes[i]]));
                    delimiter = ", ";
                }

                throw new ExprValidationException(
                    "Failed to validate the group-by clause, found duplicate specification of expressions (" + writer +
                    ")");
            }

            // enumerate combinations building an index list
            var combinationEnumeration = new CombinationEnumeration(perNodeCombinations);
            ISet<int> combination = new SortedSet<int>();
            ISet<MultiKeyInt> indexList = new LinkedHashSet<MultiKeyInt>();
            while (combinationEnumeration.MoveNext()) {
                combination.Clear();
                object[] combinationOA = combinationEnumeration.Current;
                foreach (var indexes in combinationOA) {
                    var indexarr = (int[]) indexes;
                    foreach (var anIndex in indexarr) {
                        combination.Add(anIndex);
                    }
                }

                var indexArr = CollectionUtil.IntArray(combination);
                indexList.Add(new MultiKeyInt(indexArr));
            }

            // obtain rollup levels
            var rollupLevels = new int[indexList.Count][];
            var count = 0;
            foreach (var mk in indexList) {
                rollupLevels[count++] = mk.Keys;
            }

            var numberOfLevels = rollupLevels.Length;
            if (numberOfLevels == 1 && rollupLevels[0].Length == 0) {
                throw new ExprValidationException(
                    "Failed to validate the group-by clause, the overall grouping '()' cannot be the only grouping");
            }

            // obtain select-expression copies for rewrite
            var expressions = selectClauseSpec.SelectExprList;
            var selects = new ExprNode[numberOfLevels][];
            for (var i = 0; i < numberOfLevels; i++) {
                selects[i] = new ExprNode[expressions.Count];
                for (var j = 0; j < expressions.Count; j++) {
                    SelectClauseElementRaw selectRaw = expressions[j];
                    if (!(selectRaw is SelectClauseExprRawSpec)) {
                        throw new ExprValidationException(
                            "Group-by with rollup requires that the select-clause does not use wildcard");
                    }

                    var compiled = (SelectClauseExprRawSpec) selectRaw;
                    selects[i][j] = CopyVisitExpression(compiled.SelectExpression, expressionCopier);
                }
            }

            // obtain having-expression copies for rewrite
            ExprNode[] optHavingNodeCopy = null;
            if (optionalHavingNode != null) {
                optHavingNodeCopy = new ExprNode[numberOfLevels];
                for (var i = 0; i < numberOfLevels; i++) {
                    optHavingNodeCopy[i] = CopyVisitExpression(optionalHavingNode, expressionCopier);
                }
            }

            // obtain orderby-expression copies for rewrite
            ExprNode[][] optOrderByCopy = null;
            if (orderByList != null && orderByList.Count > 0) {
                optOrderByCopy = new ExprNode[numberOfLevels][];
                for (var i = 0; i < numberOfLevels; i++) {
                    optOrderByCopy[i] = new ExprNode[orderByList.Count];
                    for (var j = 0; j < orderByList.Count; j++) {
                        OrderByItem element = orderByList[j];
                        optOrderByCopy[i][j] = CopyVisitExpression(element.ExprNode, expressionCopier);
                    }
                }
            }

            return new GroupByClauseExpressions(
                groupByExpressions, rollupLevels, selects, optHavingNodeCopy, optOrderByCopy);
        }

        private static GroupByExpressionInfo GroupByToRollupNodes(IList<GroupByClauseElement> groupByExpressions)
        {
            IList<GroupByRollupNodeBase> parents = new List<GroupByRollupNodeBase>(groupByExpressions.Count);
            IList<ExprNode> exprNodes = new List<ExprNode>();

            foreach (var element in groupByExpressions) {
                GroupByRollupNodeBase parent;
                if (element is GroupByClauseElementExpr) {
                    var expr = (GroupByClauseElementExpr) element;
                    exprNodes.Add(expr.Expr);
                    parent = new GroupByRollupNodeSingleExpr(expr.Expr);
                }
                else if (element is GroupByClauseElementRollupOrCube) {
                    var spec = (GroupByClauseElementRollupOrCube) element;
                    parent = new GroupByRollupNodeRollupOrCube(spec.IsCube);
                    GroupByAddRollup(spec, parent, exprNodes);
                }
                else if (element is GroupByClauseElementGroupingSet) {
                    var spec = (GroupByClauseElementGroupingSet) element;
                    parent = new GroupByRollupNodeGroupingSet();
                    foreach (var groupElement in spec.Elements) {
                        if (groupElement is GroupByClauseElementExpr) {
                            var single = (GroupByClauseElementExpr) groupElement;
                            exprNodes.Add(single.Expr);
                            parent.Add(new GroupByRollupNodeSingleExpr(single.Expr));
                        }

                        if (groupElement is GroupByClauseElementCombinedExpr) {
                            var combined = (GroupByClauseElementCombinedExpr) groupElement;
                            exprNodes.AddAll(combined.Expressions);
                            parent.Add(new GroupByRollupNodeCombinedExpr(combined.Expressions));
                        }

                        if (groupElement is GroupByClauseElementRollupOrCube) {
                            var rollup = (GroupByClauseElementRollupOrCube) groupElement;
                            var node = new GroupByRollupNodeRollupOrCube(rollup.IsCube);
                            GroupByAddRollup(rollup, node, exprNodes);
                            parent.Add(node);
                        }
                    }
                }
                else {
                    throw new IllegalStateException("Unexpected group-by clause element " + element);
                }

                parents.Add(parent);
            }

            return new GroupByExpressionInfo(exprNodes, parents);
        }

        private static void GroupByAddRollup(
            GroupByClauseElementRollupOrCube spec,
            GroupByRollupNodeBase parent,
            IList<ExprNode> exprNodes)
        {
            foreach (var rolledUp in spec.RollupExpressions) {
                if (rolledUp is GroupByClauseElementExpr) {
                    var expr = (GroupByClauseElementExpr) rolledUp;
                    exprNodes.Add(expr.Expr);
                    parent.Add(new GroupByRollupNodeSingleExpr(expr.Expr));
                }
                else {
                    var combined = (GroupByClauseElementCombinedExpr) rolledUp;
                    exprNodes.AddAll(combined.Expressions);
                    parent.Add(new GroupByRollupNodeCombinedExpr(combined.Expressions));
                }
            }
        }

        private static ExprNode CopyVisitExpression(
            ExprNode expression,
            ExpressionCopier expressionCopier)
        {
            return expressionCopier.Copy(expression);
        }

        internal class GroupByExpressionInfo
        {
            internal GroupByExpressionInfo(
                IList<ExprNode> expressions,
                IList<GroupByRollupNodeBase> nodes)
            {
                Expressions = expressions;
                Nodes = nodes;
            }

            public IList<ExprNode> Expressions { get; }

            public IList<GroupByRollupNodeBase> Nodes { get; }
        }
    }
} // end of namespace