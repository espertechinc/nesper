///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.collection
{
    /// <summary>
    ///     Event buffer of a given size provides a random access API into the most current event to prior events
    ///     up to the given size. Oldest events roll out of the buffer first.
    ///     <para>
    ///         Backed by a fixed-size array that is filled forward, then rolls back to the beginning
    ///         keeping track of the current position.
    ///     </para>
    /// </summary>
    public class RollingEventBuffer
    {
        /// <summary>Ctor.</summary>
        /// <param name="size">is the maximum number of events in buffer</param>
        public RollingEventBuffer(int size)
        {
            if (size <= 0) {
                throw new ArgumentException("Minimum buffer size is 1");
            }

            NextFreeIndex = 0;
            Buffer = new EventBean[size];
        }

        public EventBean this[int index] => Get(index);

        public int Count => Buffer.Length;

        internal EventBean[] Buffer { get; set; }

        public int NextFreeIndex { get; set; }

        /// <summary>Add events to the buffer.</summary>
        /// <param name="events">to add</param>
        public void Add(EventBean[] events)
        {
            if (events == null) {
                return;
            }

            for (var i = 0; i < events.Length; i++) {
                Add(events[i]);
            }
        }

        /// <summary>Add an event to the buffer.</summary>
        /// <param name="@event">to add</param>
        public void Add(EventBean @event)
        {
            Buffer[NextFreeIndex] = @event;
            NextFreeIndex++;

            if (NextFreeIndex == Buffer.Length) {
                NextFreeIndex = 0;
            }
        }

        /// <summary>
        ///     Get an event prior to the last event posted given a number of events before the last.
        ///     <para>
        ///         Thus index 0 returns the last event added, index 1 returns the prior to the last event added
        ///         up to the maximum buffer size.
        ///     </para>
        /// </summary>
        /// <param name="index">prior event index from zero to max size</param>
        /// <returns>prior event at given index</returns>
        public EventBean Get(int index)
        {
            if (index >= Buffer.Length) {
                throw new ArgumentException("Invalid index " + index + " for size " + Buffer.Length);
            }

            // The newest event is at (nextFreeIndex + 1)
            var newest = NextFreeIndex - 1;
            var relative = newest - index;
            if (relative < 0) {
                relative += Buffer.Length;
            }

            return Buffer[relative];
        }
    }
} // End of namespace