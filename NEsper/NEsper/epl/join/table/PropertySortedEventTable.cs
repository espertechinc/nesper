///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.@join.exec.@base;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.join.table
{
    /// <summary>
    /// Index that organizes events by the event property values into a single TreeMap sortable non-nested index
    /// with Object keys that store the property values.
    /// </summary>
    public abstract class PropertySortedEventTable : EventTable
    {
        protected readonly EventPropertyGetter _propertyGetter;
        protected readonly EventTableOrganization _organization;

        public abstract ISet<EventBean> LookupRange(object keyStart, bool includeStart, object keyEnd, bool includeEnd, bool allowRangeReversal);
        public abstract ICollection<EventBean> LookupRangeColl(object keyStart, bool includeStart, object keyEnd, bool includeEnd, bool allowRangeReversal);
        public abstract ISet<EventBean> LookupRangeInverted(object keyStart, bool includeStart, object keyEnd, bool includeEnd);
        public abstract ICollection<EventBean> LookupRangeInvertedColl(object keyStart, bool includeStart, object keyEnd, bool includeEnd);
        public abstract ISet<EventBean> LookupLess(object keyStart);
        public abstract ICollection<EventBean> LookupLessThenColl(object keyStart);
        public abstract ISet<EventBean> LookupLessEqual(object keyStart);
        public abstract ICollection<EventBean> LookupLessEqualColl(object keyStart);
        public abstract ISet<EventBean> LookupGreaterEqual(object keyStart);
        public abstract ICollection<EventBean> LookupGreaterEqualColl(object keyStart);
        public abstract ISet<EventBean> LookupGreater(object keyStart);
        public abstract ICollection<EventBean> LookupGreaterColl(object keyStart);
        public abstract ISet<EventBean> LookupConstants(RangeIndexLookupValue lookupValueBase);

        public abstract void Add(EventBean @event, ExprEvaluatorContext exprEvaluatorContext);
        public abstract void Remove(EventBean @event, ExprEvaluatorContext exprEvaluatorContext);
        public abstract bool IsEmpty();
        public abstract void Clear();
        public abstract void Destroy();

        public abstract IEnumerator<EventBean> GetEnumerator();

        /// <summary>
        /// Ctor.
        /// </summary>
        public PropertySortedEventTable(EventPropertyGetter propertyGetter, EventTableOrganization organization)
        {
            this._propertyGetter = propertyGetter;
            this._organization = organization;
        }

        /// <summary>
        /// Determine multikey for index access.
        /// </summary>
        /// <param name="theEvent">to get properties from for key</param>
        /// <returns>multi key</returns>
        protected internal object GetIndexedValue(EventBean theEvent)
        {
            return _propertyGetter.Get(theEvent);
        }

        public void AddRemove(EventBean[] newData, EventBean[] oldData, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QIndexAddRemove(this, newData, oldData); }
            if (newData != null)
            {
                foreach (var theEvent in newData)
                {
                    Add(theEvent, exprEvaluatorContext);
                }
            }
            if (oldData != null)
            {
                foreach (var theEvent in oldData)
                {
                    Remove(theEvent, exprEvaluatorContext);
                }
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AIndexAddRemove(); }
        }

        /// <summary>
        /// Add an array of events. Same event instance is not added twice. Event properties should be immutable.
        /// Allow null passed instead of an empty array.
        /// </summary>
        /// <param name="events">to add</param>
        /// <param name="exprEvaluatorContext">The expr evaluator context.</param>
        /// <throws>ArgumentException if the event was already existed in the index</throws>
        public void Add(EventBean[] events, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (events != null)
            {

                if (InstrumentationHelper.ENABLED && events.Length > 0)
                {
                    InstrumentationHelper.Get().QIndexAdd(this, events);
                    foreach (var theEvent in events)
                    {
                        Add(theEvent, exprEvaluatorContext);
                    }
                    InstrumentationHelper.Get().AIndexAdd();
                    return;
                }

                foreach (var theEvent in events)
                {
                    Add(theEvent, exprEvaluatorContext);
                }
            }
        }

        /// <summary>
        /// Remove events.
        /// </summary>
        /// <param name="events">to be removed, can be null instead of an empty array.</param>
        /// <param name="exprEvaluatorContext">The expr evaluator context.</param>
        /// <throws>ArgumentException when the event could not be removed as its not in the index</throws>
        public void Remove(EventBean[] events, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (events != null)
            {

                if (InstrumentationHelper.ENABLED && events.Length > 0)
                {
                    InstrumentationHelper.Get().QIndexRemove(this, events);
                    foreach (var theEvent in events)
                    {
                        Remove(theEvent, exprEvaluatorContext);
                    }
                    InstrumentationHelper.Get().AIndexRemove();
                    return;
                }

                foreach (var theEvent in events)
                {
                    Remove(theEvent, exprEvaluatorContext);
                }
            }
        }

        public virtual int? NumberOfEvents
        {
            get { return null; }
        }

        internal static ISet<EventBean> Normalize(IDictionary<object, ISet<EventBean>> submap)
        {
            if (submap.Count == 0)
            {
                return null;
            }
            if (submap.Count == 1)
            {
                return submap.Get(submap.Keys.First());
            }
            ISet<EventBean> result = new LinkedHashSet<EventBean>();
            foreach (var entry in submap)
            {
                result.AddAll(entry.Value);
            }
            return result;
        }

        internal static ICollection<EventBean> NormalizeCollection(IDictionary<object, ISet<EventBean>> submap)
        {
            if (submap.Count == 0)
            {
                return null;
            }
            if (submap.Count == 1)
            {
                return submap.Get(submap.Keys.First());
            }
            var result = new ArrayDeque<EventBean>();
            foreach (var entry in submap)
            {
                result.AddAll(entry.Value);
            }
            return result;
        }

        internal static ICollection<EventBean> NormalizeCollection(IDictionary<object, ISet<EventBean>> submapOne, IDictionary<object, ISet<EventBean>> submapTwo)
        {
            if (submapOne.Count == 0)
            {
                return NormalizeCollection(submapTwo);
            }
            if (submapTwo.Count == 0)
            {
                return NormalizeCollection(submapOne);
            }
            var result = new ArrayDeque<EventBean>();
            foreach (var entry in submapOne)
            {
                result.AddAll(entry.Value);
            }
            foreach (var entry in submapTwo)
            {
                result.AddAll(entry.Value);
            }
            return result;
        }

        internal static ISet<EventBean> Normalize(IDictionary<object, ISet<EventBean>> submapOne, IDictionary<object, ISet<EventBean>> submapTwo)
        {
            if (submapOne.Count == 0)
            {
                return Normalize(submapTwo);
            }
            if (submapTwo.Count == 0)
            {
                return Normalize(submapOne);
            }
            ISet<EventBean> result = new LinkedHashSet<EventBean>();
            foreach (var entry in submapOne)
            {
                result.AddAll(entry.Value);
            }
            foreach (var entry in submapTwo)
            {
                result.AddAll(entry.Value);
            }
            return result;
        }

        public string ToQueryPlan()
        {
            return this.GetType().Name +
                    " streamNum=" + _organization.StreamNum +
                    " _propertyGetter=" + _propertyGetter;
        }

        public EventTableOrganization Organization
        {
            get { return _organization; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public abstract Type ProviderClass { get; }
        public abstract int NumKeys { get; }
        public abstract object Index { get; }
    }
} // end of namespace
