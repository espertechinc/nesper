///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.ops;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.compile.stage2.FilterSpecCompilerIndexPlannerHelper;

namespace com.espertech.esper.common.@internal.compile.stage2
{
	public class FilterSpecCompilerIndexPlannerWidthWithConditions
	{
		internal static FilterSpecPlanForge PlanRemainingNodesWithConditions(
			FilterSpecParaForgeMap overallExpressions,
			FilterSpecCompilerArgs args,
			int filterServiceMaxFilterWidth,
			ExprNode topLevelNegator)
		{
			var unassigned = overallExpressions.UnassignedExpressions;
			var orNodes = new List<ExprOrNode>(unassigned.Count);

			foreach (var node in unassigned) {
				if (node is ExprOrNode) {
					orNodes.Add((ExprOrNode) node);
				}
			}

			var expressionsWithoutOr = new FilterSpecParaForgeMap();
			expressionsWithoutOr.Add(overallExpressions);

			// first dimension: or-node index
			// second dimension: or child node index
			var countOr = 0;
			var sizeFactorized = 1;
			var sizePerOr = new int[orNodes.Count];
			var orChildNodes = new OrChildNode[orNodes.Count][];
			var hasControl = false;
			foreach (var orNode in orNodes) {
				expressionsWithoutOr.RemoveNode(orNode);

				// get value-nodes and non-value nodes
				var nonValueNodes = GetNonValueChildNodes(orNode);
				var valueNodes = new List<ExprNode>(Arrays.AsList(orNode.ChildNodes));
				valueNodes.RemoveAll(nonValueNodes);
				var singleValueNode = ExprNodeUtilityMake.ConnectExpressionsByLogicalOrWhenNeeded(valueNodes);

				// get all child nodes; last one is confirm if present
				IList<ExprNode> allChildNodes = new List<ExprNode>(nonValueNodes);
				if (singleValueNode != null) {
					allChildNodes.Add(singleValueNode);
				}

				var len = allChildNodes.Count;
				orChildNodes[countOr] = new OrChildNode[len];

				for (var i = 0; i < len; i++) {
					var child = allChildNodes[i];
					if (child == singleValueNode) {
						hasControl = true;
						orChildNodes[countOr][i] = new OrChildNodeV(singleValueNode);
					}
					else {
						var map = new FilterSpecParaForgeMap();
						var nodes = Collections.SingletonList(child);
						var confirm = DecomposePopulateConsolidate(map, true, nodes, args);
						if (confirm == null) {
							orChildNodes[countOr][i] = new OrChildNodeNV(child, map);
						}
						else {
							hasControl = true;
							orChildNodes[countOr][i] = new OrChildNodeNVNegated(child, map, confirm);
						}
					}
				}

				sizePerOr[countOr] = len;
				sizeFactorized = sizeFactorized * len;
				countOr++;
			}

			// compute permutations
			var permutations = new CombPermutationTriplets[sizeFactorized];
			var combinationEnumeration = CombinationEnumeration.FromZeroBasedRanges(sizePerOr);
			var count = 0;
			foreach (var permutation in combinationEnumeration) {
				permutations[count] = ComputePermutation(expressionsWithoutOr, permutation, orChildNodes, hasControl, args);
				count++;
			}

			// Remove any permutations that only have a control-confirm
			var result = new List<FilterSpecPlanPathForge>(sizeFactorized);
			var pathControlConfirm = new List<ExprNode>();
			foreach (var permutation in permutations) {
				if (permutation.Triplets.Length > 0) {
					result.Add(new FilterSpecPlanPathForge(permutation.Triplets, permutation.NegateCondition));
				}
				else {
					pathControlConfirm.Add(permutation.NegateCondition);
				}
			}

			if (result.Count > filterServiceMaxFilterWidth) {
				return null;
			}

			var pathArray = result.ToArray();
			var topLevelConfirmer = ExprNodeUtilityMake.ConnectExpressionsByLogicalOrWhenNeeded(pathControlConfirm);

			// determine when the path-negate condition is the same as the root confirm-expression
			if (topLevelConfirmer != null) {
				var not = new ExprNotNode();
				not.AddChildNode(topLevelConfirmer);
				foreach (var path in pathArray) {
					if (ExprNodeUtilityCompare.DeepEquals(not, path.PathNegate, true)) {
						path.PathNegate = null;
					}
				}
			}

			var convertor = new MatchedEventConvertorForge(
				args.taggedEventTypes,
				args.arrayEventTypes,
				args.allTagNamesOrdered,
				null,
				true);
			return new FilterSpecPlanForge(pathArray, topLevelConfirmer, topLevelNegator, convertor);
		}

		private static IList<ExprNode> GetNonValueChildNodes(ExprOrNode orNode)
		{
			IList<ExprNode> childNodes = new List<ExprNode>(orNode.ChildNodes.Length);
			foreach (var node in orNode.ChildNodes) {
				var visitor = new FilterSpecExprNodeVisitorValueLimitedExpr();
				node.Accept(visitor);
				if (!visitor.IsLimited) {
					childNodes.Add(node);
				}
			}

			return childNodes;
		}

		private static CombPermutationTriplets ComputePermutation(
			FilterSpecParaForgeMap filterParamExprMap,
			object[] permutation,
			OrChildNode[][] orChildNodes,
			bool hasControl,
			FilterSpecCompilerArgs args)
		{
			var mapAll = new FilterSpecParaForgeMap();
			mapAll.Add(filterParamExprMap);

			// combine
			IList<ExprNode> nvPerOr = new List<ExprNode>(permutation.Length);
			IList<ExprNode> negatingPath = new List<ExprNode>(permutation.Length);
			for (var orNodeNum = 0; orNodeNum < permutation.Length; orNodeNum++) {
				var orChildNodeNum = permutation[orNodeNum].AsInt32();
				var current = orChildNodes[orNodeNum][orChildNodeNum];
				if (current is OrChildNodeNV) {
					var nv = (OrChildNodeNV) current;
					mapAll.Add(nv.Map);
					if (current is OrChildNodeNVNegated) {
						negatingPath.Add(((OrChildNodeNVNegated) current).Control);
					}
				}
				else {
					var v = (OrChildNodeV) current;
					negatingPath.Add(v.Node);
				}

				var orChildNodesForCurrent = orChildNodes[orNodeNum];
				foreach (var other in orChildNodesForCurrent) {
					if (current == other) {
						continue;
					}

					if (other is OrChildNodeV) {
						var v = (OrChildNodeV) other;
						var not = new ExprNotNode();
						not.AddChildNode(v.Node);
						nvPerOr.Add(not);
					}
				}
			}

			// consolidate across
			FilterSpecCompilerConsolidateUtil.Consolidate(mapAll, args.statementRawInfo.StatementName);

			IList<FilterSpecPlanPathTripletForge> triplets = new List<FilterSpecPlanPathTripletForge>(mapAll.Triplets);
			var countUnassigned = mapAll.CountUnassignedExpressions();
			if (countUnassigned != 0) {
				var triplet = MakeRemainingNode(mapAll.UnassignedExpressions, args);
				triplets.Add(triplet);
			}

			// without conditions we are done
			var tripletsArray = triplets.ToArray();
			if (!hasControl) {
				return new CombPermutationTriplets(tripletsArray, null);
			}

			var negatingNode = ExprNodeUtilityMake.ConnectExpressionsByLogicalAndWhenNeeded(negatingPath);
			var excluded = ExprNodeUtilityMake.ConnectExpressionsByLogicalAndWhenNeeded(nvPerOr);
			var merged = ExprNodeUtilityMake.ConnectExpressionsByLogicalAndWhenNeeded(negatingNode, excluded);
			return new CombPermutationTriplets(tripletsArray, merged);
		}

		private class CombPermutationTriplets
		{
			public CombPermutationTriplets(
				FilterSpecPlanPathTripletForge[] triplets,
				ExprNode negateCondition)
			{
				Triplets = triplets;
				NegateCondition = negateCondition;
			}

			public FilterSpecPlanPathTripletForge[] Triplets { get; }

			public ExprNode NegateCondition { get; }
		}

		private interface OrChildNode
		{
		}

		private class OrChildNodeV : OrChildNode
		{
			public OrChildNodeV(ExprNode node)
			{
				Node = node;
			}

			public ExprNode Node { get; }
		}

		private class OrChildNodeNV : OrChildNode
		{
			public OrChildNodeNV(
				ExprNode node,
				FilterSpecParaForgeMap map)
			{
				Node = node;
				Map = map;
			}

			public ExprNode Node { get; }

			public FilterSpecParaForgeMap Map { get; }
		}

		private class OrChildNodeNVNegated : OrChildNodeNV
		{
			public OrChildNodeNVNegated(
				ExprNode node,
				FilterSpecParaForgeMap map,
				ExprNode control) : base(node, map)
			{
				Control = control;
			}

			public ExprNode Control { get; }
		}
	}
} // end of namespace
