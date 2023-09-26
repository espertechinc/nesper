///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.join.exec.composite;

namespace com.espertech.esper.common.@internal.epl.index.composite
{
    public abstract class PropertyCompositeEventTable : EventTable
    {
        internal readonly PropertyCompositeEventTableFactory factory;

        public abstract IDictionary<object, CompositeIndexEntry> Index { get; }

        public abstract CompositeIndexQueryResultPostProcessor PostProcessor { get; }

        public PropertyCompositeEventTable(PropertyCompositeEventTableFactory factory)
        {
            this.factory = factory;
        }

        public void AddRemove(
            EventBean[] newData,
            EventBean[] oldData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            exprEvaluatorContext.InstrumentationProvider.QIndexAddRemove(this, newData, oldData);

            if (newData != null) {
                foreach (var theEvent in newData) {
                    Add(theEvent, exprEvaluatorContext);
                }
            }

            if (oldData != null) {
                foreach (var theEvent in oldData) {
                    Remove(theEvent, exprEvaluatorContext);
                }
            }

            exprEvaluatorContext.InstrumentationProvider.AIndexAddRemove();
        }

        /// <summary>
        /// Add an array of events. Same event instance is not added twice. Event properties should be immutable.
        /// Allow null passed instead of an empty array.
        /// </summary>
        /// <param name="events">to add</param>
        /// <param name="exprEvaluatorContext">evaluator context</param>
        /// <throws>ArgumentException if the event was already existed in the index</throws>
        public void Add(
            EventBean[] events,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (events != null) {
                foreach (var theEvent in events) {
                    Add(theEvent, exprEvaluatorContext);
                }
            }
        }

        /// <summary>
        /// Remove events.
        /// </summary>
        /// <param name="events">to be removed, can be null instead of an empty array.</param>
        /// <param name="exprEvaluatorContext">evaluator context</param>
        /// <throws>ArgumentException when the event could not be removed as its not in the index</throws>
        public void Remove(
            EventBean[] events,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (events != null) {
                foreach (var theEvent in events) {
                    Remove(theEvent, exprEvaluatorContext);
                }
            }
        }

        public override string ToString()
        {
            return ToQueryPlan();
        }

        public string ToQueryPlan()
        {
            return GetType().Name;
        }

        public int? NumberOfEvents => null;

        public EventTableOrganization Organization => factory.Organization;

        public Type[] OptKeyCoercedTypes => factory.OptKeyCoercedTypes;

        public Type[] OptRangeCoercedTypes => factory.OptRangeCoercedTypes;

        public MultiKeyFromObjectArray MultiKeyTransform => factory.TransformFireAndForget;

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public abstract IEnumerator<EventBean> GetEnumerator();

        public abstract Type ProviderClass { get; }
        public abstract int NumKeys { get; }
        object EventTable.Index => Index;

        public abstract void Add(
            EventBean @event,
            ExprEvaluatorContext exprEvaluatorContext);

        public abstract void Remove(
            EventBean @event,
            ExprEvaluatorContext exprEvaluatorContext);

        public abstract bool IsEmpty { get; }

        public abstract void Clear();

        public abstract void Destroy();
    }
} // end of namespace