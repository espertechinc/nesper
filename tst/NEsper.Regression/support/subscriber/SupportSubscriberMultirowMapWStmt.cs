///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.runtime.client;

namespace com.espertech.esper.regressionlib.support.subscriber
{
    public class SupportSubscriberMultirowMapWStmt : SupportSubscriberMultirowMapBase
    {
        public SupportSubscriberMultirowMapWStmt() : base(true)
        {
        }

        public void Update(
            EPStatement stmt,
            IDictionary<string, object>[] newEvents,
            IDictionary<string, object>[] oldEvents)
        {
            AddIndication(stmt, newEvents, oldEvents);
        }
    }
} // end of namespace