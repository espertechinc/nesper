///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.rowregex
{
    /// <summary>
    /// "Prev" state for random access to event history.
    /// </summary>
    public class RegexPartitionStateRandomAccessImpl : RegexPartitionStateRandomAccess
    {
        private readonly RegexPartitionStateRandomAccessGetter _getter;
        private readonly IDictionary<EventBean, EventBean[]> _priorEventMap;
        private readonly RollingEventBuffer _newEvents;
        private EventBean[] _lastNew;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="getter">for access</param>
        public RegexPartitionStateRandomAccessImpl(RegexPartitionStateRandomAccessGetter getter)
        {
            _getter = getter;

            // Construct a rolling buffer of new data for holding max index + 1 (position 1 requires 2 events to keep)
            _newEvents = new RollingEventBuffer(getter.MaxPriorIndex + 1);
            if (!getter.IsUnbound)
            {
                _priorEventMap = new Dictionary<EventBean, EventBean[]>();
            }
            else
            {
                _priorEventMap = null;
            }
        }

        /// <summary>
        /// Add new event.
        /// </summary>
        /// <param name="newEvent">to add</param>
        public void NewEventPrepare(EventBean newEvent)
        {
            // Add new event
            _newEvents.Add(newEvent);

            // Save prior index events in array
            EventBean[] priorEvents = new EventBean[_getter.IndexesRequestedLen];
            for (int j = 0; j < priorEvents.Length; j++)
            {
                int priorIndex = _getter.IndexesRequested[j];
                priorEvents[j] = _newEvents.Get(priorIndex);
            }

            if (_priorEventMap != null)
            {
                _priorEventMap.Put(newEvent, priorEvents);
            }

            _lastNew = priorEvents;
            _getter.RandomAccess = this;
        }

        /// <summary>
        /// Prepare relative to existing event, for iterating.
        /// </summary>
        /// <param name="newEvent">to consider for index</param>
        public void ExistingEventPrepare(EventBean newEvent)
        {
            if (_priorEventMap != null)
            {
                _lastNew = _priorEventMap.Get(newEvent);
            }
            _getter.RandomAccess = this;
        }

        /// <summary>
        /// Returns a previous event. Always immediatly preceded by #newEventPrepare.
        /// </summary>
        /// <param name="assignedRelativeIndex">index</param>
        /// <returns>
        /// event
        /// </returns>
        public EventBean GetPreviousEvent(int assignedRelativeIndex)
        {
            if (_lastNew == null)
            {
                return null;
            }
            return _lastNew[assignedRelativeIndex];
        }

        /// <summary>
        /// Remove events.
        /// </summary>
        /// <param name="oldEvents">to remove</param>
        public void Remove(EventBean[] oldEvents)
        {
            if (oldEvents == null)
            {
                return;
            }
            for (int i = 0; i < oldEvents.Length; i++)
            {
                Remove(oldEvents[i]);
            }
        }

        /// <summary>
        /// Remove event.
        /// </summary>
        /// <param name="oldEvent">to remove</param>
        public void Remove(EventBean oldEvent)
        {
            if (_priorEventMap != null)
            {
                _priorEventMap.Remove(oldEvent);
            }
        }

        /// <summary>
        /// Returns true for empty collection.
        /// </summary>
        /// <value>
        ///   indicator if empty
        /// </value>
        public bool IsEmpty
        {
            get
            {
                if (_priorEventMap != null)
                {
                    _priorEventMap.IsEmpty();
                }
                return true;
            }
        }
    }
}
