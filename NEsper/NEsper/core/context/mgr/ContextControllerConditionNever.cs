///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.pattern;

namespace com.espertech.esper.core.context.mgr
{
    /// <summary>
    ///     Context condition used for overlapping and non-overlaping to never-end/terminated.
    /// </summary>
    public class ContextControllerConditionNever : ContextControllerCondition
    {
        public void Activate(
            EventBean optionalTriggerEvent,
            MatchedEventMap priorMatches,
            long timeOffset,
            bool isRecoveringResilient)
        {
        }

        public void Deactivate()
        {
        }

        public bool IsRunning
        {
            get { return true; }
        }

        public long? ExpectedEndTime
        {
            get { return null; }
        }

        public bool IsImmediate
        {
            get { return false; }
        }
    }
} // end of namespace