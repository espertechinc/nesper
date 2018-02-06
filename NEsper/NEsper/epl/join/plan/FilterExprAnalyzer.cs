///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.dot;
using com.espertech.esper.epl.expression.ops;
using com.espertech.esper.epl.join.util;
using com.espertech.esper.filter;
using com.espertech.esper.type;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.join.plan
{
    /// <summary>
    /// Analyzes a filter expression and builds a query graph model. The 'equals', 'and',
    /// 'between' and relational operators expressions in the filter expression are extracted 
    /// and placed in the query graph model as navigable relationships (by key and index 
    /// properties as well as ranges) between streams.
    /// </summary>
    public class FilterExprAnalyzer
    {
        /// <summary>
        /// Analyzes filter expression to build query graph model.
        /// </summary>
        /// <param name="topNode">filter top node</param>
        /// <param name="queryGraph">model containing relationships between streams, to be written to</param>
        /// <param name="isOuterJoin">if set to <c>true</c> [is outer join].</param>
        public static void Analyze(ExprNode topNode, QueryGraph queryGraph, bool isOuterJoin)
        {
            // Analyze relationships between streams. Relationships are properties in AND and EQUALS nodes of joins.
            if (topNode is ExprEqualsNode)
            {
                var equalsNode = (ExprEqualsNode)topNode;
                if (!equalsNode.IsNotEquals)
                {
                    AnalyzeEqualsNode(equalsNode, queryGraph, isOuterJoin);
                }
            }
            else if (topNode is ExprAndNode)
            {
                var andNode = (ExprAndNode)topNode;
                AnalyzeAndNode(andNode, queryGraph, isOuterJoin);
            }
            else if (topNode is ExprBetweenNode)
            {
                var betweenNode = (ExprBetweenNode)topNode;
                AnalyzeBetweenNode(betweenNode, queryGraph);
            }
            else if (topNode is ExprRelationalOpNode)
            {
                var relNode = (ExprRelationalOpNode)topNode;
                AnalyzeRelationalOpNode(relNode, queryGraph);
            }
            else if (topNode is FilterExprAnalyzerAffectorProvider)
            {
                var provider = (FilterExprAnalyzerAffectorProvider)topNode;
                AnalyzeAffectorProvider(provider, queryGraph, isOuterJoin);
            }
            else if (topNode is ExprInNode)
            {
                ExprInNode inNode = (ExprInNode)topNode;
                AnalyzeInNode(inNode, queryGraph);
            }
            else if (topNode is ExprOrNode)
            {
                ExprNode rewritten = FilterSpecCompilerMakeParamUtil.RewriteOrToInIfApplicable(topNode);
                if (rewritten is ExprInNode)
                {
                    var inNode = (ExprInNode)rewritten;
                    AnalyzeInNode(inNode, queryGraph);
                }
            }
        }

        private static void AnalyzeInNode(ExprInNode inNode, QueryGraph queryGraph)
        {
            if (inNode.IsNotIn)
            {
                return;
            }

            // direction of lookup is value-set (keys) to single-expression (single index)
            AnalyzeInNodeSingleIndex(inNode, queryGraph);

            // direction of lookup is single-expression (key) to value-set  (multi index)
            AnalyzeInNodeMultiIndex(inNode, queryGraph);
        }

        private static void AnalyzeInNodeMultiIndex(ExprInNode inNode, QueryGraph queryGraph)
        {
            ExprNode[] setExpressions = GetInNodeSetExpressions(inNode);
            if (setExpressions.Length == 0)
            {
                return;
            }

            var perStreamExprs = new Dictionary<int?, IList<ExprNode>>();
            foreach (ExprNode exprNodeSet in setExpressions)
            {
                if (!(exprNodeSet is ExprIdentNode))
                {
                    continue;
                }
                var setIdent = (ExprIdentNode)exprNodeSet;
                AddToList(setIdent.StreamId, setIdent, perStreamExprs);
            }
            if (perStreamExprs.IsEmpty())
            {
                return;
            }

            var testExpr = inNode.ChildNodes[0];
            var testExprType = testExpr.ExprEvaluator.ReturnType.GetBoxedType();
            if (perStreamExprs.Count > 1)
            {
                return;
            }
            var entry = perStreamExprs.First();
            ExprNode[] exprNodes = ExprNodeUtility.ToArray(entry.Value);
            foreach (ExprNode node in exprNodes)
            {
                var exprType = node.ExprEvaluator.ReturnType;
                if (exprType.GetBoxedType() != testExprType)
                {
                    return;
                }
            }

            int? testStreamNum;
            int setStream = entry.Key.Value;
            if (!(testExpr is ExprIdentNode))
            {
                EligibilityDesc eligibility = EligibilityUtil.VerifyInputStream(testExpr, setStream);
                if (!eligibility.Eligibility.IsEligible())
                {
                    return;
                }
                if (eligibility.Eligibility == Eligibility.REQUIRE_ONE && setStream == eligibility.StreamNum)
                {
                    return;
                }
                testStreamNum = eligibility.StreamNum;
            }
            else
            {
                testStreamNum = ((ExprIdentNode)testExpr).StreamId;
            }

            if (testStreamNum == null)
            {
                queryGraph.AddInSetMultiIndexUnkeyed(testExpr, setStream, exprNodes);
            }
            else
            {
                if (testStreamNum.Equals(entry.Key))
                {
                    return;
                }
                queryGraph.AddInSetMultiIndex(testStreamNum.Value, testExpr, setStream, exprNodes);
            }
        }

        private static void AnalyzeInNodeSingleIndex(ExprInNode inNode, QueryGraph queryGraph)
        {
            if (!(inNode.ChildNodes[0] is ExprIdentNode))
            {
                return;
            }
            var testIdent = (ExprIdentNode)inNode.ChildNodes[0];
            var testIdentClass = TypeHelper.GetBoxedType(testIdent.ExprEvaluator.ReturnType);
            int indexedStream = testIdent.StreamId;

            ExprNode[] setExpressions = GetInNodeSetExpressions(inNode);
            if (setExpressions.Length == 0)
            {
                return;
            }

            var perStreamExprs = new LinkedHashMap<int?, IList<ExprNode>>();

            foreach (ExprNode exprNodeSet in setExpressions)
            {
                if (exprNodeSet.ExprEvaluator.ReturnType.GetBoxedType() != testIdentClass)
                {
                    continue;
                }
                if (exprNodeSet is ExprIdentNode)
                {
                    var setIdent = (ExprIdentNode)exprNodeSet;
                    AddToList(setIdent.StreamId, setIdent, perStreamExprs);
                }
                else
                {
                    EligibilityDesc eligibility = EligibilityUtil.VerifyInputStream(exprNodeSet, indexedStream);
                    if (!eligibility.Eligibility.IsEligible())
                    {
                        continue;
                    }
                    AddToList(eligibility.StreamNum, exprNodeSet, perStreamExprs);
                }
            }

            foreach (var entry in perStreamExprs)
            {
                ExprNode[] exprNodes = ExprNodeUtility.ToArray(entry.Value);
                if (entry.Key == null)
                {
                    queryGraph.AddInSetSingleIndexUnkeyed(testIdent.StreamId, testIdent, exprNodes);
                    continue;
                }
                if (entry.Key.Value != indexedStream)
                {
                    queryGraph.AddInSetSingleIndex(testIdent.StreamId, testIdent, entry.Key.Value, exprNodes);
                }
            }
        }

        private static void AddToList(int? streamIdAllowNull, ExprNode expr, IDictionary<int?, IList<ExprNode>> perStreamExpression)
        {
            var perStream = perStreamExpression.Get(streamIdAllowNull);
            if (perStream == null)
            {
                perStream = new List<ExprNode>();
                perStreamExpression.Put(streamIdAllowNull, perStream);
            }
            perStream.Add(expr);
        }

        private static ExprNode[] GetInNodeSetExpressions(ExprInNode inNode)
        {
            var setExpressions = new ExprNode[inNode.ChildNodes.Count - 1];
            var count = 0;
            for (int i = 1; i < inNode.ChildNodes.Count; i++)
            {
                setExpressions[count++] = inNode.ChildNodes[i];
            }
            return setExpressions;
        }

        private static void AnalyzeAffectorProvider(FilterExprAnalyzerAffectorProvider provider, QueryGraph queryGraph, bool isOuterJoin)
        {
            var affector = provider.GetAffector(isOuterJoin);
            if (affector == null)
            {
                return;
            }
            affector.Apply(queryGraph);
        }

        private static void AnalyzeRelationalOpNode(ExprRelationalOpNode relNode, QueryGraph queryGraph)
        {
            if (((relNode.ChildNodes[0] is ExprIdentNode)) &&
                 ((relNode.ChildNodes[1] is ExprIdentNode)))
            {
                var identNodeLeft = (ExprIdentNode)relNode.ChildNodes[0];
                var identNodeRight = (ExprIdentNode)relNode.ChildNodes[1];

                if (identNodeLeft.StreamId != identNodeRight.StreamId)
                {
                    queryGraph.AddRelationalOpStrict(
                        identNodeLeft.StreamId, identNodeLeft,
                        identNodeRight.StreamId, identNodeRight,
                        relNode.RelationalOpEnum);
                }
                return;
            }

            var indexedStream = -1;
            ExprIdentNode indexedPropExpr = null;
            ExprNode exprNodeNoIdent = null;
            RelationalOpEnum relop = relNode.RelationalOpEnum;

            if (relNode.ChildNodes[0] is ExprIdentNode)
            {
                indexedPropExpr = (ExprIdentNode)relNode.ChildNodes[0];
                indexedStream = indexedPropExpr.StreamId;
                exprNodeNoIdent = relNode.ChildNodes[1];
            }
            else if (relNode.ChildNodes[1] is ExprIdentNode)
            {
                indexedPropExpr = (ExprIdentNode)relNode.ChildNodes[1];
                indexedStream = indexedPropExpr.StreamId;
                exprNodeNoIdent = relNode.ChildNodes[0];
                relop = relop.Reversed();
            }
            if (indexedStream == -1)
            {
                return;     // require property of right/left side of equals
            }

            var eligibility = EligibilityUtil.VerifyInputStream(exprNodeNoIdent, indexedStream);
            if (!eligibility.Eligibility.IsEligible())
            {
                return;
            }

            queryGraph.AddRelationalOp(indexedStream, indexedPropExpr, eligibility.StreamNum, exprNodeNoIdent, relop);
        }

        private static void AnalyzeBetweenNode(ExprBetweenNode betweenNode, QueryGraph queryGraph)
        {
            RangeFilterAnalyzer.Apply(betweenNode.ChildNodes[0], betweenNode.ChildNodes[1], betweenNode.ChildNodes[2],
                    betweenNode.IsLowEndpointIncluded, betweenNode.IsHighEndpointIncluded, betweenNode.IsNotBetween,
                    queryGraph);
        }

        /// <summary>
        /// Analye EQUALS (=) node.
        /// </summary>
        /// <param name="equalsNode">node to analyze</param>
        /// <param name="queryGraph">store relationships between stream properties</param>
        /// <param name="isOuterJoin">if set to <c>true</c> [is outer join].</param>
        internal static void AnalyzeEqualsNode(ExprEqualsNode equalsNode, QueryGraph queryGraph, bool isOuterJoin)
        {
            if ((equalsNode.ChildNodes[0] is ExprIdentNode) &&
                 (equalsNode.ChildNodes[1] is ExprIdentNode))
            {
                var identNodeLeft = (ExprIdentNode)equalsNode.ChildNodes[0];
                var identNodeRight = (ExprIdentNode)equalsNode.ChildNodes[1];

                if (identNodeLeft.StreamId != identNodeRight.StreamId)
                {
                    queryGraph.AddStrictEquals(identNodeLeft.StreamId, identNodeLeft.ResolvedPropertyName, identNodeLeft,
                            identNodeRight.StreamId, identNodeRight.ResolvedPropertyName, identNodeRight);
                }

                return;
            }
            if (isOuterJoin)
            {      // outerjoins don't use constants or one-way expression-derived information to evaluate join
                return;
            }

            // handle constant-compare or transformation case
            var indexedStream = -1;
            ExprIdentNode indexedPropExpr = null;
            ExprNode exprNodeNoIdent = null;

            if (equalsNode.ChildNodes[0] is ExprIdentNode)
            {
                indexedPropExpr = (ExprIdentNode)equalsNode.ChildNodes[0];
                indexedStream = indexedPropExpr.StreamId;
                exprNodeNoIdent = equalsNode.ChildNodes[1];
            }
            else if (equalsNode.ChildNodes[1] is ExprIdentNode)
            {
                indexedPropExpr = (ExprIdentNode)equalsNode.ChildNodes[1];
                indexedStream = indexedPropExpr.StreamId;
                exprNodeNoIdent = equalsNode.ChildNodes[0];
            }
            if (indexedStream == -1)
            {
                return;     // require property of right/left side of equals
            }

            var eligibility = EligibilityUtil.VerifyInputStream(exprNodeNoIdent, indexedStream);
            if (!eligibility.Eligibility.IsEligible())
            {
                return;
            }

            if (eligibility.Eligibility == Eligibility.REQUIRE_NONE)
            {
                queryGraph.AddUnkeyedExpression(indexedStream, indexedPropExpr, exprNodeNoIdent);
            }
            else
            {
                queryGraph.AddKeyedExpression(indexedStream, indexedPropExpr, eligibility.StreamNum.Value, exprNodeNoIdent);
            }
        }

        /// <summary>
        /// Analyze the AND-node.
        /// </summary>
        /// <param name="andNode">node to analyze</param>
        /// <param name="queryGraph">to store relationships between stream properties</param>
        /// <param name="isOuterJoin">if set to <c>true</c> [is outer join].</param>
        internal static void AnalyzeAndNode(ExprAndNode andNode, QueryGraph queryGraph, bool isOuterJoin)
        {
            foreach (var childNode in andNode.ChildNodes)
            {
                Analyze(childNode, queryGraph, isOuterJoin);
            }
        }
    }
}
