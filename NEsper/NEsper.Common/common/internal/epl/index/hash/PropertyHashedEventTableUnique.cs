///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.index.hash
{
    /// <summary>
    /// Unique index.
    /// </summary>
    public class PropertyHashedEventTableUnique : PropertyHashedEventTable,
        EventTableAsSet
    {
        private readonly IDictionary<object, EventBean> propertyIndex;

        public PropertyHashedEventTableUnique(PropertyHashedEventTableFactory factory)
            : base(factory)
        {
            propertyIndex = new Dictionary<object, EventBean>();
        }

        public override ISet<EventBean> Lookup(object key)
        {
            EventBean @event = propertyIndex.Get(key);
            if (@event != null) {
                return Collections.SingletonSet(@event);
            }

            return null;
        }

        public override int NumKeys {
            get => propertyIndex.Count;
        }

        public override object Index {
            get => propertyIndex;
        }

        /// <summary>
        /// Remove then add events.
        /// </summary>
        /// <param name="newData">to add</param>
        /// <param name="oldData">to remove</param>
        /// <param name="exprEvaluatorContext">evaluator context</param>
        public override void AddRemove(
            EventBean[] newData,
            EventBean[] oldData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            exprEvaluatorContext.InstrumentationProvider.QIndexAddRemove(this, newData, oldData);

            if (oldData != null) {
                foreach (EventBean theEvent in oldData) {
                    Remove(theEvent, exprEvaluatorContext);
                }
            }

            if (newData != null) {
                foreach (EventBean theEvent in newData) {
                    Add(theEvent, exprEvaluatorContext);
                }
            }

            exprEvaluatorContext.InstrumentationProvider.AIndexAddRemove();
        }

        public override void Add(
            EventBean theEvent,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            object key = GetKey(theEvent);

            var existing = propertyIndex.Push(key, theEvent);
            if (existing != null && !existing.Equals(theEvent)) {
                throw HandleUniqueIndexViolation(base.Factory.Organization.IndexName, key);
            }
        }

        public override void Remove(
            EventBean theEvent,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            object key = GetKey(theEvent);
            propertyIndex.Remove(key);
        }

        public override bool IsEmpty {
            get => propertyIndex.IsEmpty();
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return propertyIndex.Values.GetEnumerator();
        }

        public override void Clear()
        {
            propertyIndex.Clear();
        }

        public override void Destroy()
        {
            Clear();
        }

        public override string ToString()
        {
            return ToQueryPlan();
        }

        public override int? NumberOfEvents {
            get => propertyIndex.Count;
        }

        public ISet<EventBean> AllValues()
        {
            if (propertyIndex.IsEmpty()) {
                return Collections.GetEmptySet<EventBean>();
            }

            return new HashSet<EventBean>(propertyIndex.Values);
        }

        public override Type ProviderClass {
            get => typeof(PropertyHashedEventTableUnique);
        }

        public IDictionary<object, EventBean> PropertyIndex {
            get { return propertyIndex; }
        }

        public static EPException HandleUniqueIndexViolation(
            string indexName,
            object key)
        {
            string indexNameDisplay = indexName == null ? "" : " '" + indexName + "'";
            throw new EPException("Unique index violation, index" + indexNameDisplay + " is a unique index and key '" + key + "' already exists");
        }
    }
} // end of namespace