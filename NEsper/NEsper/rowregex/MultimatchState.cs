///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.util;

namespace com.espertech.esper.rowregex
{
    /// <summary>
    /// State holder for matches, backed by an array, for fast copying and writing.
    /// </summary>
    public class MultimatchState
    {
        private int _count;
        private EventBean[] _events;
    
        /// <summary>Ctor. </summary>
        /// <param name="theEvent">first event to hold</param>
        public MultimatchState(EventBean theEvent)
        {
            _events = new EventBean[3];
            Add(theEvent);
        }
    
        /// <summary>Ctor. </summary>
        /// <param name="state">to copy</param>
        public MultimatchState(MultimatchState state)
        {
            var copyArray = new EventBean[state.Buffer.Length];
            Array.Copy(state.Buffer, 0, copyArray, 0, state.Count);
    
            _count = state.Count;
            _events = copyArray;
        }
    
        /// <summary>Add an event. </summary>
        /// <param name="theEvent">to add</param>
        public void Add(EventBean theEvent)
        {
            if (_count == _events.Length)
            {
                var buf = new EventBean[_events.Length * 2];
                Array.Copy(_events, 0, buf, 0, _events.Length);
                _events = buf;
            }
            _events[_count++] = theEvent;
        }

        /// <summary>Returns the count of events. </summary>
        /// <value>count</value>
        public int Count
        {
            get { return _count; }
        }

        /// <summary>Returns the raw buffer. </summary>
        /// <value>buffer</value>
        public EventBean[] Buffer
        {
            get { return _events; }
        }

        /// <summary>Determines if an event is in the collection. </summary>
        /// <param name="theEvent">to check</param>
        /// <returns>indicator</returns>
        public bool ContainsEvent(EventBean theEvent)
        {
            for (var i = 0; i < _count; i++)
            {
                if (_events[i] == theEvent)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns the buffer sized to only the contained events, and shrinks the event array unless it is empty
        /// </summary>
        /// <returns>events</returns>
        public EventBean[] GetShrinkEventArray()
        {
            if (_count == 0) {
                return CollectionUtil.EVENTBEANARRAY_EMPTY;
            }
            if (_count == _events.Length) {
                return _events;
            }
            var array = new EventBean[_count];
            Array.Copy(_events, 0, array, 0, _count);
            _events = array; // we hold on to the result, avoiding future shrinking
            return array;
        }
    }
}
