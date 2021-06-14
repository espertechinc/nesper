///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.runtime.client;

namespace com.espertech.esper.regressionlib.support.subscriber
{
    public class SupportSubscriberMultirowUnderlyingWStmt : SupportSubscriberMultirowUnderlyingBase
    {
        public SupportSubscriberMultirowUnderlyingWStmt() : base(true)
        {
        }

        public void Update(
            EPStatement stmt,
            SupportBean[] newEvents,
            SupportBean[] oldEvents)
        {
            AddIndication(stmt, newEvents, oldEvents);
        }
    }
} // end of namespace