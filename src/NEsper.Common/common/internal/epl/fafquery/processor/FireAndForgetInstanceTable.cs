///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.fafquery.querymethod;
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.fafquery.processor
{
    public class FireAndForgetInstanceTable : FireAndForgetInstance
    {
        private readonly TableInstance instance;

        public FireAndForgetInstanceTable(TableInstance instance)
        {
            this.instance = instance;
        }

        public override AgentInstanceContext AgentInstanceContext => instance.AgentInstanceContext;

        public override Viewable TailViewInstance => null;

        public override EventBean[] ProcessInsert(FAFQueryMethodIUDInsertInto insert)
        {
            TableEvalLockUtil.ObtainLockUnless(
                instance.TableLevelRWLock.WriteLock,
                instance.AgentInstanceContext.TableExprEvaluatorContext);

            var eventBeansZero = Array.Empty<EventBean>();

            var inserted = new EventBean[insert.InsertHelpers.Length];
            for (var i = 0; i < insert.InsertHelpers.Length; i++) {
                inserted[i] = insert.InsertHelpers[i]
                    .Process(eventBeansZero, true, true, instance.AgentInstanceContext);
            }

            try {
                foreach (var @event in inserted) {
                    var aggs = instance.Table.AggregationRowFactory.Make();
                    ((object[])@event.Underlying)[0] = aggs;
                    instance.AddEvent(@event);
                }
            }
            catch (EPException) {
                foreach (var @event in inserted) {
                    instance.DeleteEvent(@event);
                }

                throw;
            }

            return CollectionUtil.EVENTBEANARRAY_EMPTY;
        }

        public override EventBean[] ProcessDelete(FAFQueryMethodIUDDelete delete)
        {
            TableEvalLockUtil.ObtainLockUnless(
                instance.TableLevelRWLock.WriteLock,
                instance.AgentInstanceContext.TableExprEvaluatorContext);

            if (delete.OptionalWhereClause == null) {
                instance.ClearInstance();
                return CollectionUtil.EVENTBEANARRAY_EMPTY;
            }

            var found = SnapshotAndApplyFilter(
                delete.QueryGraph,
                delete.Annotations,
                delete.OptionalWhereClause,
                instance.AgentInstanceContext);
            foreach (var @event in found) {
                instance.DeleteEvent(@event);
            }

            return CollectionUtil.EVENTBEANARRAY_EMPTY;
        }

        public override EventBean[] ProcessUpdate(FAFQueryMethodIUDUpdate update)
        {
            TableEvalLockUtil.ObtainLockUnless(
                instance.TableLevelRWLock.WriteLock,
                instance.AgentInstanceContext.TableExprEvaluatorContext);
            var events = SnapshotAndApplyFilter(
                update.QueryGraph,
                update.Annotations,
                update.OptionalWhereClause,
                instance.AgentInstanceContext);

            if (events != null && events.IsEmpty()) {
                return CollectionUtil.EVENTBEANARRAY_EMPTY;
            }

            var eventsPerStream = new EventBean[3];
            if (events == null) {
                update.TableUpdateStrategy.UpdateTable(
                    instance.EventCollection,
                    instance,
                    eventsPerStream,
                    instance.AgentInstanceContext);
            }
            else {
                update.TableUpdateStrategy.UpdateTable(
                    events,
                    instance,
                    eventsPerStream,
                    instance.AgentInstanceContext);
            }

            return CollectionUtil.EVENTBEANARRAY_EMPTY;
        }

        public override ICollection<EventBean> SnapshotBestEffort(
            QueryGraph queryGraph,
            Attribute[] annotations)
        {
            TableEvalLockUtil.ObtainLockUnless(instance.TableLevelRWLock.ReadLock, instance.AgentInstanceContext);
            var events = SnapshotNullWhenNoIndex(queryGraph, annotations, null, null);
            if (events != null) {
                return events;
            }

            return instance.EventCollection;
        }

        /// <summary>
        ///     Returns null when a filter cannot be applied, and a collection iterator must be used instead.
        ///     Returns best-effort matching events otherwise which should still be run through any filter expressions.
        /// </summary>
        private ICollection<EventBean> SnapshotNullWhenNoIndex(
            QueryGraph queryGraph,
            Attribute[] annotations,
            ExprNode optionalWhereClause,
            AgentInstanceContext agentInstanceContext)
        {
            // return null when filter cannot be applies
            return FireAndForgetQueryExec.Snapshot(
                queryGraph,
                annotations,
                null,
                instance.IndexRepository,
                instance.Table.Name,
                instance.AgentInstanceContext);
        }

        private ICollection<EventBean> SnapshotAndApplyFilter(
            QueryGraph queryGraph,
            Attribute[] annotations,
            ExprEvaluator filterExpr,
            AgentInstanceContext agentInstanceContext)
        {
            var indexedResult = SnapshotNullWhenNoIndex(queryGraph, annotations, null, null);
            if (indexedResult != null) {
                if (indexedResult.IsEmpty() || filterExpr == null) {
                    return indexedResult;
                }

                var dequeX = new ArrayDeque<EventBean>(Math.Min(indexedResult.Count, 16));
                ExprNodeUtilityEvaluate.ApplyFilterExpressionIterable(
                    indexedResult.GetEnumerator(),
                    filterExpr,
                    agentInstanceContext,
                    dequeX);
                return dequeX;
            }

            // fall back to window operator if snapshot doesn't resolve successfully
            var sourceCollection = instance.EventCollection;
            using (var enumerator = sourceCollection.GetEnumerator()) {
                if (!enumerator.MoveNext()) {
                    return EmptyList<EventBean>.Instance;
                }

                var deque = new ArrayDeque<EventBean>(sourceCollection.Count);
                if (filterExpr != null) {
                    ExprNodeUtilityEvaluate.ApplyFilterExpressionIterable(
                        sourceCollection.GetEnumerator(),
                        filterExpr,
                        agentInstanceContext,
                        deque);
                }
                else {
                    do {
                        deque.Add(enumerator.Current);
                    } while (enumerator.MoveNext());
                }

                return deque;
            }
        }
    }
} // end of namespace