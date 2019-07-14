///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.runtime.client;

namespace com.espertech.esper.regressionlib.support.subscriber
{
    public class SupportSubscriberMultirowUnderlyingNamedMethodWStmt : SupportSubscriberMultirowUnderlyingBase
    {
        public SupportSubscriberMultirowUnderlyingNamedMethodWStmt() : base(true)
        {
        }

        public void SomeNewDataMayHaveArrived(
            EPStatement stmt,
            object[] newData,
            object[] oldData)
        {
            AddIndication(stmt, newData, oldData);
        }
    }
} // end of namespace