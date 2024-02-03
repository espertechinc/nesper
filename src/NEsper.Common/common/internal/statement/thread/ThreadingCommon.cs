///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.util;

namespace com.espertech.esper.common.@internal.statement.thread
{
    public interface ThreadingCommon
    {
        bool IsInboundThreading { get; }

        void SubmitInbound(
            EventBean @event,
            EPRuntimeEventProcessWrapped runtimeEventSender);
    }
} // end of namespace