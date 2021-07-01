///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using static com.espertech.esper.common.@internal.bytecodemodel.util.CodegenFieldSharableComparator.
    CodegenSharableSerdeName;

namespace com.espertech.esper.common.@internal.epl.resultset.order
{
    /// <summary>
    ///     Factory for <seealso cref="OrderByProcessor" /> processors.
    /// </summary>
    public class OrderByProcessorFactoryFactory
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static OrderByProcessorFactoryForge GetProcessor(
            IList<SelectClauseExprCompiledSpec> selectionList,
            IList<OrderByItem> orderByList,
            RowLimitSpec rowLimitSpec,
            VariableCompileTimeResolver variableCompileTimeResolver,
            bool isSortUsingCollator,
            string optionalContextName,
            OrderByElementForge[][] orderByRollup)
        {
            // Get the order by expression nodes
            IList<ExprNode> orderByNodes = new List<ExprNode>();
            foreach (var element in orderByList) {
                orderByNodes.Add(element.ExprNode);
            }

            // No order-by clause
            if (orderByList.IsEmpty()) {
                Log.Debug(".getProcessor Using no OrderByProcessor");
                if (rowLimitSpec != null) {
                    var rowLimitProcessorFactory = new RowLimitProcessorFactoryForge(
                        rowLimitSpec,
                        variableCompileTimeResolver,
                        optionalContextName);
                    return new OrderByProcessorRowLimitOnlyForge(rowLimitProcessorFactory);
                }

                return null;
            }

            // Determine aggregate functions used in select, if any
            IList<ExprAggregateNode> selectAggNodes = new List<ExprAggregateNode>();
            foreach (var element in selectionList) {
                ExprAggregateNodeUtil.GetAggregatesBottomUp(element.SelectExpression, selectAggNodes);
            }

            // Get all the aggregate functions occuring in the order-by clause
            IList<ExprAggregateNode> orderAggNodes = new List<ExprAggregateNode>();
            foreach (var orderByNode in orderByNodes) {
                ExprAggregateNodeUtil.GetAggregatesBottomUp(orderByNode, orderAggNodes);
            }

            ValidateOrderByAggregates(selectAggNodes, orderAggNodes);

            // Tell the order-by processor whether to compute group-by
            // keys if they are not present
            var needsGroupByKeys = !selectionList.IsEmpty() && !orderAggNodes.IsEmpty();

            Log.Debug(".getProcessor Using OrderByProcessorImpl");
            var elements = ToElementArray(orderByList);
            var comparator = GetComparator(elements, isSortUsingCollator);
            var orderByProcessorForge = new OrderByProcessorForgeImpl(
                elements,
                needsGroupByKeys,
                orderByRollup,
                comparator);
            if (rowLimitSpec == null) {
                return orderByProcessorForge;
            }

            {
                var rowLimitProcessorFactory = new RowLimitProcessorFactoryForge(
                    rowLimitSpec,
                    variableCompileTimeResolver,
                    optionalContextName);
                return new OrderByProcessorOrderedLimitForge(orderByProcessorForge, rowLimitProcessorFactory);
            }
        }

        private static void ValidateOrderByAggregates(
            IList<ExprAggregateNode> selectAggNodes,
            IList<ExprAggregateNode> orderAggNodes)
        {
            // Check that the order-by clause doesn't contain
            // any aggregate functions not in the select expression
            foreach (var orderAgg in orderAggNodes) {
                var inSelect = false;
                foreach (var selectAgg in selectAggNodes) {
                    if (ExprNodeUtilityCompare.DeepEquals(selectAgg, orderAgg, false)) {
                        inSelect = true;
                        break;
                    }
                }

                if (!inSelect) {
                    throw new ExprValidationException(
                        "Aggregate functions in the order-by clause must also occur in the select expression");
                }
            }
        }

        private static CodegenFieldSharable GetComparator(
            OrderByElementForge[] orderBy,
            bool isSortUsingCollator)
        {
            var nodes = new ExprNode[orderBy.Length];
            var descending = new bool[orderBy.Length];
            for (var i = 0; i < orderBy.Length; i++) {
                nodes[i] = orderBy[i].ExprNode;
                descending[i] = orderBy[i].IsDescending();
            }

            var types = ExprNodeUtilityQuery.GetExprResultTypes(nodes);
            return new CodegenFieldSharableComparator(
                COMPARATORHASHABLEMULTIKEYS,
                types,
                isSortUsingCollator,
                descending);
        }

        private static OrderByElementForge[] ToElementArray(IList<OrderByItem> orderByList)
        {
            var elements = new OrderByElementForge[orderByList.Count];
            var count = 0;
            foreach (var item in orderByList) {
                elements[count++] = new OrderByElementForge(item.ExprNode, item.IsDescending);
            }

            return elements;
        }
    }
} // end of namespace