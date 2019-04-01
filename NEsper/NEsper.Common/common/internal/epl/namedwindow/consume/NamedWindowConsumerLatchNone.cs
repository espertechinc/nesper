///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Threading;

using com.espertech.esper.common.@internal.context.util;

namespace com.espertech.esper.common.@internal.epl.namedwindow.consume
{
    /// <summary>
    /// A no-latch implementation of a latch for use in guaranteeing delivery between
    /// a named window delta result and consumable by another statement.
    /// </summary>
    public class NamedWindowConsumerLatchNone : NamedWindowConsumerLatch
    {
        public NamedWindowConsumerLatchNone(
            NamedWindowDeltaData deltaData,
            IDictionary<EPStatementAgentInstanceHandle, IList<NamedWindowConsumerView>> dispatchTo)
            : base(deltaData, dispatchTo)
        {
        }

        public override void Await()
        {
        }

        public Thread CurrentThread
        {
            get => Thread.CurrentThread;
        }

        public override void Done()
        {
        }

        public override NamedWindowConsumerLatch Earlier
        {
            get => null;
        }
    }
} // end of namespace