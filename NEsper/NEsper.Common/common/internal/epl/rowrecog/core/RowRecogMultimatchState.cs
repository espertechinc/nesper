///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.rowrecog.core
{
	/// <summary>
	/// State holder for matches, backed by an array, for fast copying and writing.
	/// </summary>
	public class RowRecogMultimatchState {
	    private int count;
	    private EventBean[] events;

	    public RowRecogMultimatchState(int count, EventBean[] events) {
	        this.count = count;
	        this.events = events;
	    }

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="theEvent">first event to hold</param>
	    public RowRecogMultimatchState(EventBean theEvent) {
	        events = new EventBean[3];
	        Add(theEvent);
	    }

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="state">to copy</param>
	    public RowRecogMultimatchState(RowRecogMultimatchState state) {
	        EventBean[] copyArray = new EventBean[state.Buffer.Length];
	        Array.Copy(state.Buffer, 0, copyArray, 0, state.Count);

	        count = state.Count;
	        events = copyArray;
	    }

	    /// <summary>
	    /// Add an event.
	    /// </summary>
	    /// <param name="theEvent">to add</param>
	    public void Add(EventBean theEvent) {
	        if (count == events.Length) {
	            EventBean[] buf = new EventBean[events.Length * 2];
	            Array.Copy(events, 0, buf, 0, events.Length);
	            events = buf;
	        }
	        events[count++] = theEvent;
	    }

	    /// <summary>
	    /// Returns the count of events.
	    /// </summary>
	    /// <returns>count</returns>
	    public int Count {
	        get => count;	    }

	    /// <summary>
	    /// Returns the raw buffer.
	    /// </summary>
	    /// <returns>buffer</returns>
	    public EventBean[] GetBuffer() {
	        return events;
	    }

	    /// <summary>
	    /// Determines if an event is in the collection.
	    /// </summary>
	    /// <param name="theEvent">to check</param>
	    /// <returns>indicator</returns>
	    public bool ContainsEvent(EventBean theEvent) {
	        for (int i = 0; i < count; i++) {
	            if (events[i].Equals(theEvent)) {
	                return true;
	            }
	        }
	        return false;
	    }

	    /// <summary>
	    /// Returns the buffer sized to only the contained events, and shrinks the event array unless it is empty
	    /// </summary>
	    /// <returns>events</returns>
	    public EventBean[] GetShrinkEventArray() {
	        if (count == 0) {
	            return CollectionUtil.EVENTBEANARRAY_EMPTY;
	        }
	        if (count == events.Length) {
	            return events;
	        }
	        EventBean[] array = new EventBean[count];
	        Array.Copy(events, 0, array, 0, count);
	        events = array; // we hold on to the result, avoiding future shrinking
	        return array;
	    }
	}
} // end of namespace