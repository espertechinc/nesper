///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.spec;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.core
{
    /// <summary>
    /// An order-by processor that sorts events according to the expressions
    /// in the order_by clause.
    /// </summary>
    public class OrderByProcessorFactoryImpl : OrderByProcessorFactory
    {
    	private readonly OrderByElement[] _orderBy;
    	private readonly ExprEvaluator[] _groupByNodes;
    	private readonly bool _needsGroupByKeys;
    	private readonly IComparer<object> _comparator;
    
    	/// <summary>
    	/// Ctor.
    	/// </summary>
    	/// <param name="orderByList">the nodes that generate the keys to sort events on
    	/// </param>
    	/// <param name="groupByNodes">generate the keys for determining aggregation groups
    	/// </param>
    	/// <param name="needsGroupByKeys">indicates whether this processor needs to have individual
    	/// group by keys to evaluate the sort condition successfully
    	/// </param>
    	/// <param name="isSortUsingCollator">for string value sorting using compare or Collator</param>
    	/// <throws><seealso cref="ExprValidationException" /> when order-by items don't divulge a type</throws>
    	public OrderByProcessorFactoryImpl(
            IList<OrderByItem> orderByList,
            ExprNode[] groupByNodes,
            bool needsGroupByKeys,
            bool isSortUsingCollator)
        {
    		_orderBy = ToElementArray(orderByList);
    		_groupByNodes = ExprNodeUtility.GetEvaluators(groupByNodes);
    		_needsGroupByKeys = needsGroupByKeys;
            _comparator = GetComparator(_orderBy, isSortUsingCollator);
        }
    
        public OrderByProcessor Instantiate(AggregationService aggregationService, AgentInstanceContext agentInstanceContext) {
            return new OrderByProcessorImpl(this, aggregationService);
        }

        public OrderByElement[] OrderBy
        {
            get { return _orderBy; }
        }

        public ExprEvaluator[] GroupByNodes
        {
            get { return _groupByNodes; }
        }

        public bool IsNeedsGroupByKeys
        {
            get { return _needsGroupByKeys; }
        }

        public IComparer<object> Comparator
        {
            get { return _comparator; }
        }

        /// <summary>
        /// Returns a comparator for order items that may sort string values using Collator.
        /// </summary>
        /// <param name="orderBy">order-by items</param>
        /// <param name="isSortUsingCollator">true for Collator string sorting</param>
        /// <returns>comparator</returns>
        /// <throws><seealso cref="ExprValidationException" /> if the return type of order items cannot be determined</throws>
        internal static IComparer<Object> GetComparator(OrderByElement[] orderBy, bool isSortUsingCollator) 
        {
            var evaluators = new ExprEvaluator[orderBy.Length];
            var descending = new bool[orderBy.Length];
            for (int i = 0; i < orderBy.Length; i++) {
                evaluators[i] = orderBy[i].Expr;
                descending[i] = orderBy[i].IsDescending;
            }
            return CollectionUtil.GetComparator(evaluators, isSortUsingCollator, descending);
        }
    
        private OrderByElement[] ToElementArray(IList<OrderByItem> orderByList)
        {
            var elements = new OrderByElement[orderByList.Count];
            var count = 0;
            foreach (var item in orderByList) {
                elements[count++] = new OrderByElement(item.ExprNode, item.ExprNode.ExprEvaluator, item.IsDescending);
            }
            return elements;
        }
    }
}
