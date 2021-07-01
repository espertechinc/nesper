///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.context.controller.initterm
{
    public class ContextControllerInitTermPartitionKey
    {
        public ContextControllerInitTermPartitionKey()
        {
        }

        public ContextControllerInitTermPartitionKey(
            EventBean triggeringEvent,
            IDictionary<string, object> triggeringPattern,
            long startTime,
            long? expectedEndTime)
        {
            TriggeringEvent = triggeringEvent;
            TriggeringPattern = triggeringPattern;
            StartTime = startTime;
            ExpectedEndTime = expectedEndTime;
        }

        public EventBean TriggeringEvent { get; set; }

        public IDictionary<string, object> TriggeringPattern { set; get; }

        public long StartTime { get; set; }

        public long? ExpectedEndTime { get; set; }
    }
} // end of namespace