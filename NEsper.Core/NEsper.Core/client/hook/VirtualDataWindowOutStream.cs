///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.client.hook
{
    /// <summary>
    /// For use with virtual data windows, handles any insert stream and remove stream events that a virtual data window may post to consuming statements.
    /// </summary>
    public interface VirtualDataWindowOutStream
    {
        /// <summary>
        /// Post insert stream (new data) and remove stream (old data) events.
        /// </summary>
        /// <param name="newData">insert stream, or null if no insert stream events</param>
        /// <param name="oldData">remove stream, or null if no remove stream events</param>
        void Update(EventBean[] newData, EventBean[] oldData);
    }
}
