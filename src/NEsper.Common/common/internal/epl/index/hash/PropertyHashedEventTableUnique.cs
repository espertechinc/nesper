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
        private readonly IDictionary<object, EventBean> _propertyIndex;

        public PropertyHashedEventTableUnique(PropertyHashedEventTableFactory factory)
            : base(factory)
        {
            _propertyIndex = new Dictionary<object, EventBean>(AsymmetricEqualityComparer.Instance)
                .WithNullKeySupport();
        }

        public override ISet<EventBean> Lookup(object key)
        {
            return _propertyIndex.TryGetValue(key, out var @event)
                ? Collections.SingletonSet(@event)
                : null;
        }

        public override ISet<EventBean> LookupFAF(object key)
        {
            return Lookup(key);
        }

        public override int NumKeys => _propertyIndex.Count;

        public override object Index => _propertyIndex;

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
                foreach (var theEvent in oldData) {
                    Remove(theEvent, exprEvaluatorContext);
                }
            }

            if (newData != null) {
                foreach (var theEvent in newData) {
                    Add(theEvent, exprEvaluatorContext);
                }
            }

            exprEvaluatorContext.InstrumentationProvider.AIndexAddRemove();
        }

        public override void Add(
            EventBean theEvent,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var key = GetKey(theEvent);

            var existing = _propertyIndex.Push(key, theEvent);
            if (existing != null && !existing.Equals(theEvent)) {
                throw HandleUniqueIndexViolation(Factory.Organization.IndexName, key);
            }
        }

        public override void Remove(
            EventBean theEvent,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var key = GetKey(theEvent);
            _propertyIndex.Remove(key);
        }

        public override bool IsEmpty => _propertyIndex.IsEmpty();

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return _propertyIndex.Values.GetEnumerator();
        }

        public override void Clear()
        {
            _propertyIndex.Clear();
        }

        public override void Destroy()
        {
            Clear();
        }

        public override string ToString()
        {
            return ToQueryPlan();
        }

        public override int? NumberOfEvents => _propertyIndex.Count;

        public ISet<EventBean> AllValues()
        {
            if (_propertyIndex.IsEmpty()) {
                return Collections.GetEmptySet<EventBean>();
            }

            return new HashSet<EventBean>(_propertyIndex.Values);
        }

        public override Type ProviderClass => typeof(PropertyHashedEventTableUnique);

        public IDictionary<object, EventBean> PropertyIndex => _propertyIndex;

        public static EPException HandleUniqueIndexViolation(
            string indexName,
            object key)
        {
            var indexNameDisplay = indexName == null ? "" : " '" + indexName + "'";
            throw new EPException(
                "Unique index violation, index" +
                indexNameDisplay +
                " is a unique index and key '" +
                key +
                "' already exists");
        }
    }
} // end of namespace