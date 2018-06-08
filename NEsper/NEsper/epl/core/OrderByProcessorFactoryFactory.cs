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
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.variable;

namespace com.espertech.esper.epl.core
{
    /// <summary>
    /// Factory for <seealso cref="com.espertech.esper.epl.core.OrderByProcessor" /> processors.
    /// </summary>
    public class OrderByProcessorFactoryFactory
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Returns processor for order-by clauses.
        /// </summary>
        /// <param name="selectionList">is a list of select expressions</param>
        /// <param name="groupByNodes">is a list of group-by expressions</param>
        /// <param name="orderByList">is a list of order-by expressions</param>
        /// <param name="rowLimitSpec">specification for row limit, or null if no row limit is defined</param>
        /// <param name="variableService">for retrieving variable state for use with row limiting</param>
        /// <param name="isSortUsingCollator">for string value sorting using compare or Collator</param>
        /// <param name="optionalContextName">context name</param>
        /// <exception cref="com.espertech.esper.epl.expression.core.ExprValidationException">when validation of expressions fails</exception>
        /// <returns>ordering processor instance</returns>
        public static OrderByProcessorFactory GetProcessor(
            IList<SelectClauseExprCompiledSpec> selectionList,
            ExprNode[] groupByNodes,
            IList<OrderByItem> orderByList,
            RowLimitSpec rowLimitSpec,
            VariableService variableService,
            bool isSortUsingCollator,
            string optionalContextName)
        {
            // Get the order by expression nodes
            var orderByNodes = orderByList.Select(element => element.ExprNode).ToList();

            // No order-by clause
            if (orderByList.IsEmpty()) {
                Log.Debug(".GetProcessor Using no OrderByProcessor");
                if (rowLimitSpec != null) {
                    var rowLimitProcessorFactory = new RowLimitProcessorFactory(rowLimitSpec, variableService, optionalContextName);
                    return new OrderByProcessorRowLimitOnlyFactory(rowLimitProcessorFactory);
                }
                return null;
            }
    
            // Determine aggregate functions used in select, if any
            var selectAggNodes = new List<ExprAggregateNode>();
            foreach (SelectClauseExprCompiledSpec element in selectionList) {
                ExprAggregateNodeUtil.GetAggregatesBottomUp(element.SelectExpression, selectAggNodes);
            }
    
            // Get all the aggregate functions occuring in the order-by clause
            var orderAggNodes = new List<ExprAggregateNode>();
            foreach (ExprNode orderByNode in orderByNodes) {
                ExprAggregateNodeUtil.GetAggregatesBottomUp(orderByNode, orderAggNodes);
            }
    
            ValidateOrderByAggregates(selectAggNodes, orderAggNodes);
    
            // Tell the order-by processor whether to compute group-by
            // keys if they are not present
            bool needsGroupByKeys = !selectionList.IsEmpty() && !orderAggNodes.IsEmpty();
    
            Log.Debug(".getProcessor Using OrderByProcessorImpl");
            var orderByProcessorFactory = new OrderByProcessorFactoryImpl(orderByList, groupByNodes, needsGroupByKeys, isSortUsingCollator);
            if (rowLimitSpec == null) {
                return orderByProcessorFactory;
            } else {
                var rowLimitProcessorFactory = new RowLimitProcessorFactory(rowLimitSpec, variableService, optionalContextName);
                return new OrderByProcessorOrderedLimitFactory(orderByProcessorFactory, rowLimitProcessorFactory);
            }
        }
    
        private static void ValidateOrderByAggregates(IList<ExprAggregateNode> selectAggNodes, IList<ExprAggregateNode> orderAggNodes)
        {
            // Check that the order-by clause doesn't contain
            // any aggregate functions not in the select expression
            foreach (ExprAggregateNode orderAgg in orderAggNodes) {
                bool inSelect = false;
                foreach (ExprAggregateNode selectAgg in selectAggNodes) {
                    if (ExprNodeUtility.DeepEquals(selectAgg, orderAgg, false)) {
                        inSelect = true;
                        break;
                    }
                }
                if (!inSelect) {
                    throw new ExprValidationException("Aggregate functions in the order-by clause must also occur in the select expression");
                }
            }
        }
    }
} // end of namespace
