///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.ComponentModel;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.fafquery;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.table.strategy;
using com.espertech.esper.epl.virtualdw;
using com.espertech.esper.filter;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.core.start
{
    public class FireAndForgetInstanceTable : FireAndForgetInstance
    {
        private readonly TableStateInstance _instance;
    
        public FireAndForgetInstanceTable(TableStateInstance instance)
        {
            _instance = instance;
        }
    
        public override EventBean[] ProcessInsert(EPPreparedExecuteIUDSingleStreamExecInsert insert)
        {
            ExprTableEvalLockUtil.ObtainLockUnless(_instance.TableLevelRWLock.WriteLock, insert.Services.TableService.TableExprEvaluatorContext);
            var theEvent = insert.InsertHelper.Process(new EventBean[0], true, true, insert.ExprEvaluatorContext);
            var aggs = _instance.TableMetadata.RowFactory.MakeAggs(insert.ExprEvaluatorContext.AgentInstanceId, null, null, _instance.AggregationServicePassThru);
            ((object[]) theEvent.Underlying)[0] = aggs;
            _instance.AddEvent(theEvent);
            return CollectionUtil.EVENTBEANARRAY_EMPTY;
        }
    
        public override EventBean[] ProcessDelete(EPPreparedExecuteIUDSingleStreamExecDelete delete) 
        {
            ExprTableEvalLockUtil.ObtainLockUnless(_instance.TableLevelRWLock.WriteLock, delete.Services.TableService.TableExprEvaluatorContext);
    
            if (delete.OptionalWhereClause == null) {
                _instance.ClearInstance();
                return CollectionUtil.EVENTBEANARRAY_EMPTY;
            }
    
            var found = SnapshotAndApplyFilter(delete.QueryGraph, delete.Annotations, delete.OptionalWhereClause, _instance.AgentInstanceContext);
            foreach (var @event in found) {
                _instance.DeleteEvent(@event);
            }
            return CollectionUtil.EVENTBEANARRAY_EMPTY;
        }
    
        public override EventBean[] ProcessUpdate(EPPreparedExecuteIUDSingleStreamExecUpdate update)
        {
            ExprTableEvalLockUtil.ObtainLockUnless(_instance.TableLevelRWLock.WriteLock, update.Services.TableService.TableExprEvaluatorContext);
            var events = SnapshotAndApplyFilter(update.QueryGraph, update.Annotations, update.OptionalWhereClause, _instance.AgentInstanceContext);
    
            if (events != null && events.IsEmpty()) {
                return CollectionUtil.EVENTBEANARRAY_EMPTY;
            }
    
            var eventsPerStream = new EventBean[3];
            if (events == null) {
                update.TableUpdateStrategy.UpdateTable(_instance.EventCollection, _instance, eventsPerStream, _instance.AgentInstanceContext);
            }
            else {
                update.TableUpdateStrategy.UpdateTable(events, _instance, eventsPerStream, _instance.AgentInstanceContext);
            }
            return CollectionUtil.EVENTBEANARRAY_EMPTY;
        }
    
        public override ICollection<EventBean> SnapshotBestEffort(EPPreparedExecuteMethodQuery query, QueryGraph queryGraph, Attribute[] annotations)
        {
            ExprTableEvalLockUtil.ObtainLockUnless(_instance.TableLevelRWLock.ReadLock, query.AgentInstanceContext);
            var events = SnapshotNullWhenNoIndex(queryGraph, annotations, null, null);
            if (events != null) {
                return events;
            }
            return _instance.EventCollection;
        }
    
        private ICollection<EventBean> SnapshotAndApplyFilter(QueryGraph queryGraph, Attribute[] annotations, ExprNode filterExpr, AgentInstanceContext agentInstanceContext)
        {
            var indexedResult = SnapshotNullWhenNoIndex(queryGraph, annotations, null, null);
            if (indexedResult != null) {
                if (indexedResult.IsEmpty() || filterExpr == null) {
                    return indexedResult;
                }
                var dequeX = new ArrayDeque<EventBean>(Math.Min(indexedResult.Count, 16));
                ExprNodeUtility.ApplyFilterExpressionIterable(indexedResult.GetEnumerator(), filterExpr.ExprEvaluator, agentInstanceContext, dequeX);
                return dequeX;
            }
    
            // fall back to window operator if snapshot doesn't resolve successfully
            var sourceCollection = _instance.EventCollection;
            var it = sourceCollection.GetEnumerator();
            if (it.MoveNext() == false) {
                return Collections.GetEmptyList<EventBean>();
            }
            var deque = new ArrayDeque<EventBean>(sourceCollection.Count);
            if (filterExpr != null) {
                ExprNodeUtility.ApplyFilterExpressionIterable(sourceCollection.GetEnumerator(), filterExpr.ExprEvaluator, agentInstanceContext, deque);
            }
            else
            {
                do
                {
                    deque.Add(it.Current);
                } while (it.MoveNext());
            }
            return deque;
        }
    
        /// <summary>
        /// Returns null when a filter cannot be applied, and a collection iterator must be used instead.
        /// Returns best-effort matching events otherwise which should still be run through any filter expressions.
        /// </summary>
        private ICollection<EventBean> SnapshotNullWhenNoIndex(QueryGraph queryGraph, Attribute[] annotations, ExprNode optionalWhereClause, AgentInstanceContext agentInstanceContext)
        {
            // return null when filter cannot be applies
            return FireAndForgetQueryExec.Snapshot(queryGraph, annotations, null,
                    _instance.IndexRepository, _instance.TableMetadata.IsQueryPlanLogging,
                    TableServiceImpl.QueryPlanLog, _instance.TableMetadata.TableName,
                    _instance.AgentInstanceContext);
        }

        public override AgentInstanceContext AgentInstanceContext
        {
            get { return _instance.AgentInstanceContext; }
        }

        public override Viewable TailViewInstance
        {
            get { return null; }
        }

        public override VirtualDWView VirtualDataWindow
        {
            get { return null; }
        }
    }
}
