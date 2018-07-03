///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.threading;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.filter
{
    /// <summary>
    /// This class is responsible for changes to <seealso cref="EventTypeIndex"/> for addition 
    /// and removal of filters. It delegates the work to make modifications to the filter parameter 
    /// tree to an <seealso cref="IndexTreeBuilder"/>. It enforces a policy that a filter 
    /// callback can only be added once.
    /// </summary>
    public class EventTypeIndexBuilder
    {
        private readonly IDictionary<FilterHandle, EventTypeIndexBuilderValueIndexesPair> _isolatableCallbacks;
        private readonly ILockable _callbacksLock;
        private readonly EventTypeIndex _eventTypeIndex;

        /// <summary>
        /// Constructor - takes the event type index to manipulate as its parameter.
        /// </summary>
        /// <param name="lockManager">The lock manager.</param>
        /// <param name="eventTypeIndex">index to manipulate</param>
        /// <param name="allowIsolation">if set to <c>true</c> [allow isolation].</param>
        public EventTypeIndexBuilder(
            ILockManager lockManager,
            EventTypeIndex eventTypeIndex,
            bool allowIsolation)
        {
            _eventTypeIndex = eventTypeIndex;
            _callbacksLock = lockManager.CreateLock(GetType());
            _isolatableCallbacks = allowIsolation ? new Dictionary<FilterHandle, EventTypeIndexBuilderValueIndexesPair>() : null;
        }

        /// <summary>
        /// Dispose the service.
        /// </summary>
        public void Destroy()
        {
            _eventTypeIndex.Dispose();
            if (_isolatableCallbacks != null)
                _isolatableCallbacks.Clear();
        }

        /// <summary>
        /// Add a filter to the event type index structure, and to the filter subtree.
        /// Throws an IllegalStateException exception if the callback is already registered.
        /// </summary>
        /// <param name="filterValueSet">is the filter information</param>
        /// <param name="filterCallback">is the callback</param>
        /// <param name="lockFactory">The lock factory.</param>
        /// <returns></returns>
        /// <exception cref="IllegalStateException">Callback for filter specification already exists in collection</exception>
        public FilterServiceEntry Add(FilterValueSet filterValueSet, FilterHandle filterCallback, FilterServiceGranularLockFactory lockFactory)
        {
            using (Instrument.With(
                i => i.QFilterAdd(filterValueSet, filterCallback),
                i => i.AFilterAdd()))
            {
                var eventType = filterValueSet.EventType;
    
                // Check if a filter tree exists for this event type
                var rootNode = _eventTypeIndex.Get(eventType);
    
                // Make sure we have a root node
                if (rootNode == null)
                {
                    using(_callbacksLock.Acquire())
                    {
                        rootNode = _eventTypeIndex.Get(eventType);
                        if (rootNode == null) {
                            rootNode = new FilterHandleSetNode(lockFactory.ObtainNew());
                            _eventTypeIndex.Add(eventType, rootNode);
                        }
                    }
                }
    
                // GetInstance add to tree
                var path = IndexTreeBuilder.Add(filterValueSet, filterCallback, rootNode, lockFactory);
                var pathArray = path.Select(p => p.ToArray()).ToArray();
                var pair = new EventTypeIndexBuilderValueIndexesPair(filterValueSet, pathArray);
    
                // for non-isolatable callbacks the consumer keeps track of tree location
                if (_isolatableCallbacks == null) {
                    return pair;
                }

                // for isolatable callbacks this class is keeping track of tree location
                using (_callbacksLock.Acquire()) {
                    _isolatableCallbacks.Put(filterCallback, pair);
                }

                return null;
            }
        }

        /// <summary>
        /// Remove a filter callback from the given index node.
        /// </summary>
        /// <param name="filterCallback">is the callback to remove</param>
        /// <param name="filterServiceEntry">The filter service entry.</param>
        public void Remove(FilterHandle filterCallback, FilterServiceEntry filterServiceEntry)
        {
            EventTypeIndexBuilderValueIndexesPair pair;
            if (_isolatableCallbacks != null)
            {
                using (_callbacksLock.Acquire())
                {
                    pair = _isolatableCallbacks.Delete(filterCallback);
                }
                if (pair == null)
                {
                    return;
                }
            }
            else
            {
                pair = (EventTypeIndexBuilderValueIndexesPair) filterServiceEntry;
            }

            using (Instrument.With(
                i => i.QFilterRemove(filterCallback, pair),
                i => i.AFilterRemove()))
            {
                var eventType = pair.FilterValueSet.EventType;
                var rootNode = _eventTypeIndex.Get(eventType);

                // GetInstance remove from tree
                if (rootNode != null)
                {
                    pair.IndexPairs.ForEach(
                        indexPair => IndexTreeBuilder.Remove(eventType, filterCallback, indexPair, rootNode));
                }
            }
        }

        /// <summary>
        /// Returns filters for the statement ids.
        /// </summary>
        /// <param name="statementIds">ids to take</param>
        /// <returns>set of filters for taken statements</returns>
        public FilterSet Take(ICollection<int> statementIds)
        {
            if (_isolatableCallbacks == null)
            {
                throw new EPException("Operation not supported, please enable isolation in the engine configuration");
            }

            IList<FilterSetEntry> list = new List<FilterSetEntry>();
            using (_callbacksLock.Acquire())
            {
                foreach (var entry in _isolatableCallbacks)
                {
                    var pair = entry.Value;
                    if (statementIds.Contains(entry.Key.StatementId))
                    {
                        list.Add(new FilterSetEntry(entry.Key, pair.FilterValueSet));
    
                        var eventType = pair.FilterValueSet.EventType;
                        var rootNode = _eventTypeIndex.Get(eventType);
    
                        // GetInstance remove from tree
                        pair.IndexPairs.ForEvery(
                            indexPair => IndexTreeBuilder.Remove(eventType, entry.Key, indexPair, rootNode));
                    }
                }
                
                foreach (var removed in list)
                {
                    _isolatableCallbacks.Remove(removed.Handle);
                }
            }
    
            return new FilterSet(list);
        }

        /// <summary>
        /// Add the filters, from previously-taken filters.
        /// </summary>
        /// <param name="filterSet">to add</param>
        /// <param name="lockFactory">The lock factory.</param>
        public void Apply(FilterSet filterSet, FilterServiceGranularLockFactory lockFactory)
        {
            foreach (var entry in filterSet.Filters)
            {
                Add(entry.FilterValueSet, entry.Handle, lockFactory);
            }
        }

        public bool IsSupportsTakeApply
        {
            get { return _isolatableCallbacks != null; }
        }
    }
}
