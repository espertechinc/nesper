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
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.index.hash
{
    public class PropertyHashedEventTableUnadorned : PropertyHashedEventTable
    {
        private readonly IDictionary<object, ISet<EventBean>> _propertyIndex;

        public PropertyHashedEventTableUnadorned(PropertyHashedEventTableFactory factory)
            : base(factory)
        {
            _propertyIndex = new Dictionary<object, ISet<EventBean>>(AsymmetricEqualityComparer.Instance)
                .WithNullKeySupport();
        }

        public override bool IsEmpty => _propertyIndex.IsEmpty();

        public override int? NumberOfEvents => null;

        public override int NumKeys => _propertyIndex.Count;

        public override object Index => _propertyIndex;

        public override Type ProviderClass => typeof(PropertyHashedEventTable);

        /// <summary>
        ///     Returns the set of events that have the same property value as the given event.
        /// </summary>
        /// <param name="key">to compare against</param>
        /// <returns>set of events with property value, or null if none found (never returns zero-sized set)</returns>
        public override ISet<EventBean> Lookup(object key)
        {
            return _propertyIndex.Get(key);
        }

        public override void Add(
            EventBean theEvent,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var key = GetKey(theEvent);

            var events = _propertyIndex.Get(key);
            if (events == null) {
                events = new LinkedHashSet<EventBean>();
                _propertyIndex.Put(key, events);
            }

            events.Add(theEvent);
        }

        public override void Remove(
            EventBean theEvent,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var key = GetKey(theEvent);

            var events = _propertyIndex.Get(key);
            if (events == null) {
                return;
            }

            if (!events.Remove(theEvent)) {
                // Not an error, its possible that an old-data event is artificial (such as for statistics) and
                // thus did not correspond to a new-data event raised earlier.
                return;
            }

            if (events.IsEmpty()) {
                _propertyIndex.Remove(key);
            }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return PropertyHashedEventTableEnumerator.For(_propertyIndex);
        }

        public override void Clear()
        {
            _propertyIndex.Clear();
        }

        public override void Destroy()
        {
            Clear();
        }
    }
} // end of namespace