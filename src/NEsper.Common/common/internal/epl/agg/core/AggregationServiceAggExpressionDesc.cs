///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.expression.agg.@base;

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    public class AggregationServiceAggExpressionDesc
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="aggregationNode">expression</param>
        /// <param name="factory">method factory</param>
        public AggregationServiceAggExpressionDesc(
            ExprAggregateNode aggregationNode,
            AggregationForgeFactory factory)
        {
            AggregationNode = aggregationNode;
            Factory = factory;
        }

        /// <summary>
        ///     Returns the equivalent aggregation functions.
        /// </summary>
        /// <returns>list of agg nodes</returns>
        public IList<ExprAggregateNode> EquivalentNodes { get; private set; }

        /// <summary>
        ///     Returns the method factory.
        /// </summary>
        /// <returns>factory</returns>
        public AggregationForgeFactory Factory { get; }

        /// <summary>
        ///     Returns the expression.
        /// </summary>
        /// <returns>expression</returns>
        public ExprAggregateNode AggregationNode { get; }

        /// <summary>
        ///     Assigns a column number.
        /// </summary>
        /// <param name="value">column number</param>
        public void SetColumnNum(int value)
        {
            AggregationNode.Column = value;
            if (EquivalentNodes != null) {
                foreach (var node in EquivalentNodes) {
                    node.Column = value;
                }
            }
        }

        /// <summary>
        ///     Add an equivalent aggregation function node
        /// </summary>
        /// <param name="aggNodeToAdd">node to add</param>
        public void AddEquivalent(ExprAggregateNode aggNodeToAdd)
        {
            if (EquivalentNodes == null) {
                EquivalentNodes = new List<ExprAggregateNode>();
            }

            EquivalentNodes.Add(aggNodeToAdd);
        }
    }
} // end of namespace