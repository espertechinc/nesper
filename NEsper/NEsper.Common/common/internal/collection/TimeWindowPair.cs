///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.collection
{
    public class TimeWindowPair
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimeWindowPair"/> class.
        /// </summary>
        /// <param name="timestamp">The timestamp.</param>
        /// <param name="eventHolder">The event holder.</param>
        public TimeWindowPair(
            long timestamp,
            object eventHolder)
        {
            Timestamp = timestamp;
            EventHolder = eventHolder;
        }

        /// <summary>
        /// Gets or sets the timestamp.
        /// </summary>
        /// <value>The timestamp.</value>
        public long Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the event holder.
        /// </summary>
        /// <value>The event holder.</value>
        public object EventHolder { get; set; }
    }
}