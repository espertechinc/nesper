///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.pattern;

namespace com.espertech.esper.core.context.mgr
{
    public interface ContextControllerCondition
    {
        void Activate(EventBean optionalTriggeringEvent, MatchedEventMap priorMatches, long timeOffset, bool isRecoveringReslient);
        void Deactivate();
        bool IsRunning { get; }
        long? ExpectedEndTime { get; }
        bool IsImmediate { get; }
    }
}
