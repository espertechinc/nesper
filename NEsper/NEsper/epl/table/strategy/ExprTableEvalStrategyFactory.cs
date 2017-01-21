///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.table;
using com.espertech.esper.epl.table.mgmt;

namespace com.espertech.esper.epl.table.strategy
{
    public class ExprTableEvalStrategyFactory
    {
        public static ExprEvaluator GetTableAccessEvalStrategy(
            ExprNode exprNode,
            string tableName,
            int streamNum,
            TableMetadataColumnAggregation agg)
        {
            if (!agg.Factory.IsAccessAggregation)
            {
                return new ExprTableExprEvaluatorMethod(
                    exprNode, tableName, agg.ColumnName, streamNum, agg.Factory.ResultType, agg.MethodOffset);
            }
            else
            {
                return new ExprTableExprEvaluatorAccess(
                    exprNode, tableName, agg.ColumnName, streamNum, agg.Factory.ResultType, agg.AccessAccessorSlotPair,
                    agg.OptionalEventType);
            }
        }

        public static ExprTableAccessEvalStrategy GetTableAccessEvalStrategy(
            ExprTableAccessNode tableNode,
            TableAndLockProvider provider,
            TableMetadata tableMetadata)
        {
            var groupKeyEvals = tableNode.GroupKeyEvaluators;

            TableAndLockProviderUngrouped ungrouped;
            TableAndLockProviderGrouped grouped;
            if (provider is TableAndLockProviderUngrouped)
            {
                ungrouped = (TableAndLockProviderUngrouped) provider;
                grouped = null;
            }
            else
            {
                grouped = (TableAndLockProviderGrouped) provider;
                ungrouped = null;
            }

            // handle sub-property access
            if (tableNode is ExprTableAccessNodeSubprop)
            {
                var subprop = (ExprTableAccessNodeSubprop) tableNode;
                var column = tableMetadata.TableColumns.Get(subprop.SubpropName);
                return GetTableAccessSubprop(subprop, column, ungrouped, grouped);
            }

            // handle top-level access
            if (tableNode is ExprTableAccessNodeTopLevel)
            {
                if (ungrouped != null)
                {
                    return new ExprTableEvalStrategyUngroupedTopLevel(ungrouped, tableMetadata.TableColumns);
                }
                if (tableNode.GroupKeyEvaluators.Length > 1)
                {
                    return new ExprTableEvalStrategyGroupByTopLevelMulti(
                        grouped, tableMetadata.TableColumns, groupKeyEvals);
                }
                return new ExprTableEvalStrategyGroupByTopLevelSingle(
                    grouped, tableMetadata.TableColumns, groupKeyEvals[0]);
            }

            // handle "keys" function access
            if (tableNode is ExprTableAccessNodeKeys)
            {
                return new ExprTableEvalStrategyGroupByKeys(grouped);
            }

            // handle access-aggregator accessors
            if (tableNode is ExprTableAccessNodeSubpropAccessor)
            {
                var accessorProvider = (ExprTableAccessNodeSubpropAccessor) tableNode;
                var column =
                    (TableMetadataColumnAggregation) tableMetadata.TableColumns.Get(accessorProvider.SubpropName);
                if (ungrouped != null)
                {
                    var pairX = column.AccessAccessorSlotPair;
                    return new ExprTableEvalStrategyUngroupedAccess(ungrouped, pairX.Slot, accessorProvider.Accessor);
                }

                var pair = new AggregationAccessorSlotPair(
                    column.AccessAccessorSlotPair.Slot, accessorProvider.Accessor);
                if (tableNode.GroupKeyEvaluators.Length > 1)
                {
                    return new ExprTableEvalStrategyGroupByAccessMulti(grouped, pair, groupKeyEvals);
                }
                return new ExprTableEvalStrategyGroupByAccessSingle(grouped, pair, groupKeyEvals[0]);
            }

            throw new IllegalStateException("Unrecognized table access node " + tableNode);
        }

        private static ExprTableAccessEvalStrategy GetTableAccessSubprop(
            ExprTableAccessNodeSubprop subprop,
            TableMetadataColumn column,
            TableAndLockProviderUngrouped ungrouped,
            TableAndLockProviderGrouped grouped)
        {
            if (column is TableMetadataColumnPlain)
            {
                var plain = (TableMetadataColumnPlain) column;
                if (ungrouped != null)
                {
                    return new ExprTableEvalStrategyUngroupedProp(
                        ungrouped, plain.IndexPlain, subprop.OptionalPropertyEnumEvaluator);
                }
                if (subprop.GroupKeyEvaluators.Length > 1)
                {
                    return new ExprTableEvalStrategyGroupByPropMulti(
                        grouped, plain.IndexPlain, subprop.OptionalPropertyEnumEvaluator, subprop.GroupKeyEvaluators);
                }
                return new ExprTableEvalStrategyGroupByPropSingle(
                    grouped, plain.IndexPlain, subprop.OptionalPropertyEnumEvaluator, subprop.GroupKeyEvaluators[0]);
            }

            var aggcol = (TableMetadataColumnAggregation) column;
            if (ungrouped != null)
            {
                if (!aggcol.Factory.IsAccessAggregation)
                {
                    return new ExprTableEvalStrategyUngroupedMethod(ungrouped, aggcol.MethodOffset);
                }
                var pair = aggcol.AccessAccessorSlotPair;
                return new ExprTableEvalStrategyUngroupedAccess(ungrouped, pair.Slot, pair.Accessor);
            }

            var columnAggregation = (TableMetadataColumnAggregation) column;
            if (!columnAggregation.Factory.IsAccessAggregation)
            {
                if (subprop.GroupKeyEvaluators.Length > 1)
                {
                    return new ExprTableEvalStrategyGroupByMethodMulti(
                        grouped, columnAggregation.MethodOffset, subprop.GroupKeyEvaluators);
                }
                return new ExprTableEvalStrategyGroupByMethodSingle(
                    grouped, columnAggregation.MethodOffset, subprop.GroupKeyEvaluators[0]);
            }
            if (subprop.GroupKeyEvaluators.Length > 1)
            {
                return new ExprTableEvalStrategyGroupByAccessMulti(
                    grouped, columnAggregation.AccessAccessorSlotPair, subprop.GroupKeyEvaluators);
            }
            return new ExprTableEvalStrategyGroupByAccessSingle(
                grouped, columnAggregation.AccessAccessorSlotPair, subprop.GroupKeyEvaluators[0]);
        }
    }
} // end of namespace
