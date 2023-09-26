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

namespace com.espertech.esper.common.@internal.epl.index.hash
{
    public abstract class PropertyHashedEventTable : EventTable
    {
        internal readonly PropertyHashedEventTableFactory Factory;

        public PropertyHashedEventTable(PropertyHashedEventTableFactory factory)
        {
            Factory = factory;
        }

        public abstract void Add(
            EventBean @event,
            ExprEvaluatorContext exprEvaluatorContext);

        public abstract void Remove(
            EventBean @event,
            ExprEvaluatorContext exprEvaluatorContext);

        public virtual void AddRemove(
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
        ///     Add an array of events. Same event instance is not added twice. Event properties should be immutable.
        ///     Allow null passed instead of an empty array.
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
        ///     Remove events.
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

        public EventTableOrganization Organization => Factory.Organization;

        public string ToQueryPlan()
        {
            return Factory.ToQueryPlan();
        }

        public MultiKeyFromObjectArray MultiKeyTransform => Factory.MultiKeyTransform;

        public abstract ISet<EventBean> Lookup(object key);
        public abstract ISet<EventBean> LookupFAF(object key);

        /// <summary>
        ///     Determine multikey for index access.
        /// </summary>
        /// <param name="theEvent">to get properties from for key</param>
        /// <returns>multi key</returns>
        protected object GetKey(EventBean theEvent)
        {
            return Factory.PropertyGetter.Get(theEvent);
        }

        public override string ToString()
        {
            return ToQueryPlan();
        }

        public abstract Type ProviderClass { get; }
        public abstract int? NumberOfEvents { get; }
        public abstract int NumKeys { get; }
        public abstract object Index { get; }

        public abstract bool IsEmpty { get; }
        public abstract void Clear();
        public abstract void Destroy();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public abstract IEnumerator<EventBean> GetEnumerator();
    }
} // end of namespace