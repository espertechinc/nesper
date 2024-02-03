///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    /// <summary>
    ///     This class is responsible for changes to <seealso cref="EventTypeIndex" /> for addition and removal of filters.
    ///     It delegates the work to make modifications to the filter parameter tree to an
    ///     <seealso cref="IndexTreeBuilderAdd" /> and <seealso cref="IndexTreeBuilderRemove" />.
    ///     It enforces a policy that a filter callback can only be added once.
    /// </summary>
    public class EventTypeIndexBuilder
    {
        private readonly ILockable callbacksLock;
        private readonly EventTypeIndex eventTypeIndex;

        /// <summary>
        ///     Constructor - takes the event type index to manipulate as its parameter.
        /// </summary>
        /// <param name="eventTypeIndex">index to manipulate</param>
        public EventTypeIndexBuilder(
            EventTypeIndex eventTypeIndex)
        {
            this.eventTypeIndex = eventTypeIndex;
            callbacksLock = new MonitorSlimLock(LockConstants.DefaultTimeout);
        }

        public bool IsSupportsTakeApply => false;

        public IDictionary<EventTypeIdPair, IDictionary<int, IList<FilterItem[]>>> Get(ISet<int> statementIds)
        {
            return eventTypeIndex.GetTraverseStatement(statementIds);
        }

        /// <summary>
        ///     Destroy the service.
        /// </summary>
        public void Destroy()
        {
            eventTypeIndex.Destroy();
        }

        /// <summary>
        ///     Add a filter to the event type index structure, and to the filter subtree.
        ///     Throws an IllegalStateException exception if the callback is already registered.
        /// </summary>
        /// <param name="valueSet">is the filter information</param>
        /// <param name="filterCallback">is the callback</param>
        /// <param name="lockFactory">lock factory</param>
        /// <param name="eventType">event type</param>
        public void Add(
            EventType eventType,
            FilterValueSetParam[][] valueSet,
            FilterHandle filterCallback,
            FilterServiceGranularLockFactory lockFactory)
        {
            // Check if a filter tree exists for this event type
            var rootNode = eventTypeIndex.Get(eventType);

            // Make sure we have a root node
            if (rootNode == null) {
                using (callbacksLock.Acquire()) {
                    rootNode = eventTypeIndex.Get(eventType);
                    if (rootNode == null) {
                        rootNode = new FilterHandleSetNode(lockFactory.ObtainNew());
                        eventTypeIndex.Add(eventType, rootNode);
                    }
                }
            }

            // Now add to tree
            IndexTreeBuilderAdd.Add(valueSet, filterCallback, rootNode, lockFactory);
        }

        /// <summary>
        ///     Remove a filter callback from the given index node.
        /// </summary>
        /// <param name="eventType">event type</param>
        /// <param name="valueSet">value set</param>
        /// <param name="filterCallback">is the callback to remove</param>
        public void Remove(
            FilterHandle filterCallback,
            EventType eventType,
            FilterValueSetParam[][] valueSet)
        {
            var rootNode = eventTypeIndex.Get(eventType);
            if (rootNode != null) {
                if (valueSet.Length == 0) {
                    IndexTreeBuilderRemove.Remove(eventType, filterCallback, FilterSpecParam.EMPTY_VALUE_ARRAY, rootNode);
                }
                else {
                    for (var i = 0; i < valueSet.Length; i++) {
                        IndexTreeBuilderRemove.Remove(eventType, filterCallback, valueSet[i], rootNode);
                    }
                }
            }
        }
    }
} // end of namespace