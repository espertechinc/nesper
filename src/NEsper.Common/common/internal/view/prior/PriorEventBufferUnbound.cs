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
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.view.access;

namespace com.espertech.esper.common.@internal.view.prior
{
    /// <summary>
    ///     Buffer class for insert stream events only for use with unbound streams that inserts data only, to serve
    ///     up one or more prior events in the insert stream based on an index.
    ///     <para />
    ///     Does not expect or care about the remove stream and simple keeps a rolling buffer of new data events
    ///     up to the maximum prior event we are asking for.
    /// </summary>
    public class PriorEventBufferUnbound : ViewUpdatedCollection,
        RandomAccessByIndex
    {
        private readonly int _maxSize;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="maxPriorIndex">is the highest prior-event index required by any expression</param>
        public PriorEventBufferUnbound(int maxPriorIndex)
        {
            _maxSize = maxPriorIndex + 1;
            NewEvents = new RollingEventBuffer(_maxSize);
        }

        public RollingEventBuffer NewEvents { get; }

        public EventBean GetNewData(int index)
        {
            if (index >= _maxSize) {
                throw new ArgumentException("Index " + index + " not allowed, max size is " + _maxSize);
            }

            return NewEvents.Get(index);
        }

        public EventBean GetOldData(int index)
        {
            return null;
        }

        public EventBean GetNewDataTail(int index)
        {
            // No requirement to index from end of current buffer
            return null;
        }

        public IEnumerator<EventBean> GetWindowEnumerator()
        {
            // no requirement for window iterator support
            return null;
        }

        public ICollection<EventBean> WindowCollectionReadOnly => null;

        public int WindowCount => 0;

        public void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            // Post new data to rolling buffer starting with the oldest
            if (newData != null) {
                for (var i = 0; i < newData.Length; i++) {
                    var newEvent = newData[i];

                    // Add new event
                    NewEvents.Add(newEvent);
                }
            }
        }

        public void Destroy()
        {
            // No action required
        }

        public int NumEventsInsertBuf => NewEvents.Count;
    }
} // end of namespace