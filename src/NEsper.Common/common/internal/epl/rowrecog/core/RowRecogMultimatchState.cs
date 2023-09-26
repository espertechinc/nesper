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

namespace com.espertech.esper.common.@internal.epl.rowrecog.core
{
    /// <summary>
    ///     State holder for matches, backed by an array, for fast copying and writing.
    /// </summary>
    public class RowRecogMultimatchState
    {
        public RowRecogMultimatchState(
            int count,
            EventBean[] events)
        {
            Count = count;
            Buffer = events;
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name = "theEvent">first event to hold</param>
        public RowRecogMultimatchState(EventBean theEvent)
        {
            Buffer = new EventBean[3];
            Add(theEvent);
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name = "state">to copy</param>
        public RowRecogMultimatchState(RowRecogMultimatchState state)
        {
            var copyArray = new EventBean[state.Buffer.Length];
            Array.Copy(state.Buffer, 0, copyArray, 0, state.Count);
            Count = state.Count;
            Buffer = copyArray;
        }

        /// <summary>
        ///     Returns the count of events.
        /// </summary>
        /// <returns>count</returns>
        public int Count { get; private set; }

        /// <summary>
        ///     Returns the raw buffer.
        /// </summary>
        /// <value>buffer</value>
        public EventBean[] Buffer { get; private set; }

        /// <summary>
        ///     Add an event.
        /// </summary>
        /// <param name = "theEvent">to add</param>
        public void Add(EventBean theEvent)
        {
            if (Count == Buffer.Length) {
                var buf = new EventBean[Buffer.Length * 2];
                Array.Copy(Buffer, 0, buf, 0, Buffer.Length);
                Buffer = buf;
            }

            Buffer[Count++] = theEvent;
        }

        /// <summary>
        ///     Determines if an event is in the collection.
        /// </summary>
        /// <param name = "theEvent">to check</param>
        /// <returns>indicator</returns>
        public bool ContainsEvent(EventBean theEvent)
        {
            for (var i = 0; i < Count; i++) {
                if (Buffer[i].Equals(theEvent)) {
                    return true;
                }
            }

            return false;
        }

        public EventBean[] ShrinkEventArray {
            get {
                if (Count == 0) {
                    return CollectionUtil.EVENTBEANARRAY_EMPTY;
                }

                if (Count == Buffer.Length) {
                    return Buffer;
                }

                var array = new EventBean[Count];
                Array.Copy(Buffer, 0, array, 0, Count);
                Buffer = array; // we hold on to the result, avoiding future shrinking
                return array;
            }
        }
    }
} // end of namespace