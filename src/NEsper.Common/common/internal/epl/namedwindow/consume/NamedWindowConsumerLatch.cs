///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.context.util;

namespace com.espertech.esper.common.@internal.epl.namedwindow.consume
{
    public abstract class NamedWindowConsumerLatch
    {
        public NamedWindowConsumerLatch(
            NamedWindowDeltaData deltaData,
            IDictionary<EPStatementAgentInstanceHandle, IList<NamedWindowConsumerView>> dispatchTo)
        {
            DeltaData = deltaData;
            DispatchTo = dispatchTo;
        }

        public abstract NamedWindowConsumerLatch Earlier { get; }

        public NamedWindowDeltaData DeltaData { get; }

        public IDictionary<EPStatementAgentInstanceHandle, IList<NamedWindowConsumerView>> DispatchTo { get; }

        public abstract void Await();

        public abstract void Done();
    }
} // end of namespace