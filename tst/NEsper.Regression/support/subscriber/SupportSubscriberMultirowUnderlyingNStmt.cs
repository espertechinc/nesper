///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.support;

namespace com.espertech.esper.regressionlib.support.subscriber
{
    public class SupportSubscriberMultirowUnderlyingNStmt : SupportSubscriberMultirowUnderlyingBase
    {
        public SupportSubscriberMultirowUnderlyingNStmt() : base(false)
        {
        }

        public void Update(
            SupportBean[] newEvents,
            SupportBean[] oldEvents)
        {
            AddIndication(newEvents, oldEvents);
        }
    }
} // end of namespace