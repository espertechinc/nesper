///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.declared.compiletime;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.expression.agg.@base
{
	public class ExprAggregateNodeUtil {
	    public static ExprAggregateNodeParamDesc GetValidatePositionalParams(ExprNode[] childNodes, bool builtinAggregationFunc)
	            {
	        ExprAggregateLocalGroupByDesc optionalLocalGroupBy = null;
	        ExprNode optionalFilter = null;
	        int count = 0;
	        foreach (ExprNode node in childNodes) {
	            if (!IsNonPositionalParameter(node)) {
	                count++;
	            } else {
	                ExprNamedParameterNode namedParameterNode = (ExprNamedParameterNode) node;
	                string paramNameLower = namedParameterNode.ParameterName.ToLowerInvariant();
	                if (paramNameLower.Equals("group_by")) {
	                    optionalLocalGroupBy = new ExprAggregateLocalGroupByDesc(namedParameterNode.ChildNodes);
	                } else if (paramNameLower.Equals("filter")) {
	                    if (namedParameterNode.ChildNodes.Length != 1 | Boxing.GetBoxedType(namedParameterNode.ChildNodes[0].Forge.EvaluationType) != typeof(bool?)) {
	                        throw new ExprValidationException("Filter named parameter requires a single expression returning a boolean-typed value");
	                    }
	                    optionalFilter = namedParameterNode.ChildNodes[0];
	                } else if (builtinAggregationFunc) {
	                    throw new ExprValidationException("Invalid named parameter '" + namedParameterNode.ParameterName + "' (did you mean 'group_by' or 'filter'?)");
	                }
	            }
	        }
	        ExprNode[] positionals = new ExprNode[count];
	        count = 0;
	        foreach (ExprNode node in childNodes) {
	            if (!IsNonPositionalParameter(node)) {
	                positionals[count++] = node;
	            }
	        }
	        return new ExprAggregateNodeParamDesc(positionals, optionalLocalGroupBy, optionalFilter);
	    }

	    public static bool IsNonPositionalParameter(ExprNode node) {
	        return node is ExprNamedParameterNode;
	    }

	    public static void GetAggregatesBottomUp(ExprNode[][] nodes, IList<ExprAggregateNode> aggregateNodes) {
	        if (nodes == null) {
	            return;
	        }
	        foreach (ExprNode[] node in nodes) {
	            GetAggregatesBottomUp(node, aggregateNodes);
	        }
	    }

	    public static void GetAggregatesBottomUp(ExprNode[] nodes, IList<ExprAggregateNode> aggregateNodes) {
	        if (nodes == null) {
	            return;
	        }
	        foreach (ExprNode node in nodes) {
	            GetAggregatesBottomUp(node, aggregateNodes);
	        }
	    }

	    /// <summary>
	    /// Populates into the supplied list all aggregation functions within this expression, if any.
	    /// <para />Populates by going bottom-up such that nested aggregates appear first.
	    /// <para />I.e. sum(volume * sum(price)) would put first A then B into the list with A=sum(price) and B=sum(volume * A)
	    /// </summary>
	    /// <param name="topNode">is the expression node to deep inspect</param>
	    /// <param name="aggregateNodes">is a list of node to populate into</param>
	    public static void GetAggregatesBottomUp(ExprNode topNode, IList<ExprAggregateNode> aggregateNodes) {
	        // Map to hold per level of the node (1 to N depth) of expression node a list of aggregation expr nodes, if any
	        // exist at that level
	        OrderedDictionary<int, IList<ExprAggregateNode>> aggregateExprPerLevel = new OrderedDictionary<int, IList<ExprAggregateNode>>();

	        RecursiveAggregateHandleSpecial(topNode, aggregateExprPerLevel, 1);

	        // Recursively enter all aggregate functions and their level into map
	        RecursiveAggregateEnter(topNode, aggregateExprPerLevel, 1);

	        // Done if none found
	        if (aggregateExprPerLevel.IsEmpty()) {
	            return;
	        }

	        // From the deepest (highest) level to the lowest, add aggregates to list
	        int deepLevel = aggregateExprPerLevel.LastKey();
	        for (int i = deepLevel; i >= 1; i--) {
	            IList<ExprAggregateNode> list = aggregateExprPerLevel.Get(i);
	            if (list == null) {
	                continue;
	            }
	            aggregateNodes.AddAll(list);
	        }
	    }

	    private static void RecursiveAggregateHandleSpecial(ExprNode topNode, IDictionary<int, IList<ExprAggregateNode>> aggregateExprPerLevel, int level) {
	        if (topNode is ExprNodeInnerNodeProvider) {
	            ExprNodeInnerNodeProvider parameterized = (ExprNodeInnerNodeProvider) topNode;
	            IList<ExprNode> additionalNodes = parameterized.AdditionalNodes;
	            foreach (ExprNode additionalNode in additionalNodes) {
	                RecursiveAggregateEnter(additionalNode, aggregateExprPerLevel, level);
	            }
	        }

	        if (topNode is ExprDeclaredNode) {
	            ExprDeclaredNode declared = (ExprDeclaredNode) topNode;
	            RecursiveAggregateEnter(declared.Body, aggregateExprPerLevel, level);
	        }
	    }

	    private static void RecursiveAggregateEnter(ExprNode currentNode, IDictionary<int, IList<ExprAggregateNode>> aggregateExprPerLevel, int currentLevel) {
	        // ask all child nodes to enter themselves
	        foreach (ExprNode node in currentNode.ChildNodes) {
	            RecursiveAggregateHandleSpecial(node, aggregateExprPerLevel, currentLevel + 1);

	            RecursiveAggregateEnter(node, aggregateExprPerLevel, currentLevel + 1);
	        }

	        if (!(currentNode is ExprAggregateNode)) {
	            return;
	        }

	        // Add myself to list, I'm an aggregate function
	        IList<ExprAggregateNode> aggregates = aggregateExprPerLevel.Get(currentLevel);
	        if (aggregates == null) {
	            aggregates = new LinkedList<ExprAggregateNode>();
	            aggregateExprPerLevel.Put(currentLevel, aggregates);
	        }
	        aggregates.Add((ExprAggregateNode) currentNode);
	    }

	    public static int CountPositionalArgs(IList<ExprNode> args) {
	        int count = 0;
	        foreach (ExprNode expr in args) {
	            if (!IsNonPositionalParameter(expr)) {
	                count++;
	            }
	        }
	        return count;
	    }
	}
} // end of namespace