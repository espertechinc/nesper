///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.baseagg;

namespace com.espertech.esper.epl.agg.service
{
    public class AggregationServiceAggExpressionDesc
    {
        /// <summary>Ctor. </summary>
        /// <param name="aggregationNode">expression</param>
        /// <param name="factory">method factory</param>
        public AggregationServiceAggExpressionDesc(ExprAggregateNode aggregationNode, AggregationMethodFactory factory)
        {
            AggregationNode = aggregationNode;
            Factory = factory;
        }

        /// <summary>Returns the equivalent aggregation functions. </summary>
        /// <value>list of agg nodes</value>
        public IList<ExprAggregateNode> EquivalentNodes { get; private set; }

        /// <summary>Returns the expression. </summary>
        /// <value>expression</value>
        public ExprAggregateNode AggregationNode { get; private set; }

        /// <summary>Returns the method factory. </summary>
        /// <value>factory</value>
        public AggregationMethodFactory Factory { get; private set; }

        /// <summary>Returns the column number assigned. </summary>
        /// <value>column number</value>
        public int? ColumnNum { get; set; }

        /// <summary>Add an equivalent aggregation function node </summary>
        /// <param name="aggNodeToAdd">node to add</param>
        public void AddEquivalent(ExprAggregateNode aggNodeToAdd)
        {
            if (EquivalentNodes == null) {
                EquivalentNodes = new List<ExprAggregateNode>();
            }
            EquivalentNodes.Add(aggNodeToAdd);
        }

        /// <summary>Assigns a future to the expression </summary>
        /// <param name="service">the future</param>
        public void AssignFuture(AggregationResultFuture service)
        {
            var columnNum = ColumnNum.GetValueOrDefault();

            AggregationNode.SetAggregationResultFuture(service, columnNum);
            if (EquivalentNodes == null) {
                return;
            }
            foreach (ExprAggregateNode equivalentAggNode in EquivalentNodes)
            {
                equivalentAggNode.SetAggregationResultFuture(service, columnNum);
            }
        }
    }
}
