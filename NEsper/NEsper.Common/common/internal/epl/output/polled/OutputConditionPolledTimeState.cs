///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.output.polled
{
    public sealed class OutputConditionPolledTimeState : OutputConditionPolledState
    {
        public OutputConditionPolledTimeState(long? lastUpdate)
        {
            LastUpdate = lastUpdate;
        }

        public long? LastUpdate { get; set; }
    }
} // end of namespace