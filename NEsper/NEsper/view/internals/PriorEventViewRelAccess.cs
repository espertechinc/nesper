///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.view.window;

namespace com.espertech.esper.view.internals
{
    public class PriorEventViewRelAccess : RelativeAccessByEventNIndex
    {
        private readonly RelativeAccessByEventNIndex _buffer;
        private readonly int _relativeIndex;
    
        /// <summary>Ctor. </summary>
        /// <param name="buffer">is the buffer to acces</param>
        /// <param name="relativeIndex">is the index to pull out</param>
        public PriorEventViewRelAccess(RelativeAccessByEventNIndex buffer, int relativeIndex)
        {
            _buffer = buffer;
            _relativeIndex = relativeIndex;
        }
    
        public EventBean GetRelativeToEvent(EventBean theEvent, int prevIndex)
        {
            return _buffer.GetRelativeToEvent(theEvent, _relativeIndex);
        }
    
        public EventBean GetRelativeToEnd(int index)
        {
            // No requirement to index from end of current buffer
            return null;
        }
    
        public IEnumerator<EventBean> GetWindowToEvent()
        {
            return null;
        }
    
        public ICollection<EventBean> GetWindowToEventCollReadOnly() {
            return null;
        }
    
        public int GetWindowToEventCount()
        {
            return 0;
        }
    }
}
