///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.view.access;

namespace com.espertech.esper.common.@internal.view.prior
{
    public class PriorEventViewRelAccess : RelativeAccessByEventNIndex
    {
        private readonly RelativeAccessByEventNIndex buffer;
        private readonly int relativeIndex;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="buffer">is the buffer to acces</param>
        /// <param name="relativeIndex">is the index to pull out</param>
        public PriorEventViewRelAccess(
            RelativeAccessByEventNIndex buffer,
            int relativeIndex)
        {
            this.buffer = buffer;
            this.relativeIndex = relativeIndex;
        }

        public EventBean GetRelativeToEvent(
            EventBean theEvent,
            int prevIndex)
        {
            return buffer.GetRelativeToEvent(theEvent, relativeIndex);
        }

        public EventBean GetRelativeToEnd(int index)
        {
            // No requirement to index from end of current buffer
            return null;
        }

        public IEnumerator<EventBean> WindowToEvent => null;

        public ICollection<EventBean> WindowToEventCollReadOnly => null;

        public int WindowToEventCount => 0;
    }
} // end of namespace