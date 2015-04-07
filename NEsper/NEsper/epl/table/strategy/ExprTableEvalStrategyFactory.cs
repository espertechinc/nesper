///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.table;
using com.espertech.esper.epl.table.mgmt;

namespace com.espertech.esper.epl.table.strategy
{
    public class ExprTableEvalStrategyFactory
    {
        public static ExprEvaluator GetTableAccessEvalStrategy(ExprNode exprNode, string tableName, int streamNum, TableMetadataColumnAggregation agg)
        {
            if (!agg.Factory.IsAccessAggregation) {
                return new ExprTableExprEvaluatorMethod(exprNode, tableName, agg.ColumnName, streamNum, agg.Factory.ResultType, agg.MethodOffset);
            }
            else {
                return new ExprTableExprEvaluatorAccess(exprNode, tableName, agg.ColumnName, streamNum, agg.Factory.ResultType, agg.AccessAccessorSlotPair, agg.OptionalEventType);
            }
        }
    
        public static ExprTableAccessEvalStrategy GetTableAccessEvalStrategy(bool writesToTables, ExprTableAccessNode tableNode, TableStateInstance state, TableMetadata tableMetadata)
        {
            var groupKeyEvals = tableNode.GroupKeyEvaluators;
    
            TableStateInstanceUngrouped ungrouped;
            TableStateInstanceGroupBy grouped;
            ILockable @lock;
            if (state is TableStateInstanceUngrouped) {
                ungrouped = (TableStateInstanceUngrouped) state;
                grouped = null;
                @lock = writesToTables ? ungrouped.TableLevelRWLock.WriteLock : ungrouped.TableLevelRWLock.ReadLock;
            }
            else {
                grouped = (TableStateInstanceGroupBy) state;
                ungrouped = null;
                @lock = writesToTables ? grouped.TableLevelRWLock.WriteLock : grouped.TableLevelRWLock.ReadLock;
            }
    
            // handle sub-property access
            if (tableNode is ExprTableAccessNodeSubprop) {
                var subprop = (ExprTableAccessNodeSubprop) tableNode;
                var column = tableMetadata.TableColumns.Get(subprop.SubpropName);
                return GetTableAccessSubprop(@lock, subprop, column, grouped, ungrouped);
            }
    
            // handle top-level access
            if (tableNode is ExprTableAccessNodeTopLevel) {
                if (ungrouped != null) {
                    return new ExprTableEvalStrategyUngroupedTopLevel(@lock, ungrouped.EventReference, tableMetadata.TableColumns);
                }
                if (tableNode.GroupKeyEvaluators.Length > 1) {
                    return new ExprTableEvalStrategyGroupByTopLevelMulti(@lock, grouped.Rows, tableMetadata.TableColumns, groupKeyEvals);
                }
                return new ExprTableEvalStrategyGroupByTopLevelSingle(@lock, grouped.Rows, tableMetadata.TableColumns, groupKeyEvals[0]);
            }
    
            // handle "keys" function access
            if (tableNode is ExprTableAccessNodeKeys) {
                return new ExprTableEvalStrategyGroupByKeys(@lock, grouped.Rows);
            }
    
            // handle access-aggregator accessors
            if (tableNode is ExprTableAccessNodeSubpropAccessor) {
                var accessorProvider = (ExprTableAccessNodeSubpropAccessor) tableNode;
                var column = (TableMetadataColumnAggregation) tableMetadata.TableColumns.Get(accessorProvider.SubpropName);
                if (ungrouped != null) {
                    var pairX = column.AccessAccessorSlotPair;
                    return new ExprTableEvalStrategyUngroupedAccess(@lock, ungrouped.EventReference, pairX.Slot, accessorProvider.Accessor);
                }
    
                var pair = new AggregationAccessorSlotPair(column.AccessAccessorSlotPair.Slot, accessorProvider.Accessor);
                if (tableNode.GroupKeyEvaluators.Length > 1) {
                    return new ExprTableEvalStrategyGroupByAccessMulti(@lock, grouped.Rows, pair, groupKeyEvals);
                }
                return new ExprTableEvalStrategyGroupByAccessSingle(@lock, grouped.Rows, pair, groupKeyEvals[0]);
            }
    
            throw new IllegalStateException("Unrecognized table access node " + tableNode);
        }
    
        private static ExprTableAccessEvalStrategy GetTableAccessSubprop(ILockable @lock, ExprTableAccessNodeSubprop subprop, TableMetadataColumn column, TableStateInstanceGroupBy grouped, TableStateInstanceUngrouped ungrouped) {
    
            if (column is TableMetadataColumnPlain) {
                var plain = (TableMetadataColumnPlain) column;
                if (ungrouped != null) {
                    return new ExprTableEvalStrategyUngroupedProp(@lock, ungrouped.EventReference, plain.IndexPlain, subprop.OptionalPropertyEnumEvaluator);
                }
                if (subprop.GroupKeyEvaluators.Length > 1) {
                    return new ExprTableEvalStrategyGroupByPropMulti(@lock, grouped.Rows, plain.IndexPlain, subprop.OptionalPropertyEnumEvaluator, subprop.GroupKeyEvaluators);
                }
                return new ExprTableEvalStrategyGroupByPropSingle(@lock, grouped.Rows, plain.IndexPlain, subprop.OptionalPropertyEnumEvaluator, subprop.GroupKeyEvaluators[0]);
            }
    
            var aggcol = (TableMetadataColumnAggregation) column;
            if (ungrouped != null) {
                if (!aggcol.Factory.IsAccessAggregation) {
                    return new ExprTableEvalStrategyUngroupedMethod(@lock, ungrouped.EventReference, aggcol.MethodOffset);
                }
                var pair = aggcol.AccessAccessorSlotPair;
                return new ExprTableEvalStrategyUngroupedAccess(@lock, ungrouped.EventReference, pair.Slot, pair.Accessor);
            }
    
            var columnAggregation = (TableMetadataColumnAggregation) column;
            if (!columnAggregation.Factory.IsAccessAggregation) {
                if (subprop.GroupKeyEvaluators.Length > 1) {
                    return new ExprTableEvalStrategyGroupByMethodMulti(@lock, grouped.Rows, columnAggregation.MethodOffset, subprop.GroupKeyEvaluators);
                }
                return new ExprTableEvalStrategyGroupByMethodSingle(@lock, grouped.Rows, columnAggregation.MethodOffset, subprop.GroupKeyEvaluators[0]);
            }
            if (subprop.GroupKeyEvaluators.Length > 1) {
                return new ExprTableEvalStrategyGroupByAccessMulti(@lock, grouped.Rows, columnAggregation.AccessAccessorSlotPair, subprop.GroupKeyEvaluators);
            }
            return new ExprTableEvalStrategyGroupByAccessSingle(@lock, grouped.Rows, columnAggregation.AccessAccessorSlotPair, subprop.GroupKeyEvaluators[0]);
        }
    }
}
