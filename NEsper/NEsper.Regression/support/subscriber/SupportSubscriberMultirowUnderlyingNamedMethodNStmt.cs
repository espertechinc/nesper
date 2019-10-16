///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.subscriber
{
    public class SupportSubscriberMultirowUnderlyingNamedMethodNStmt : SupportSubscriberMultirowUnderlyingBase
    {
        public SupportSubscriberMultirowUnderlyingNamedMethodNStmt() : base(false)
        {
        }

        public void SomeNewDataMayHaveArrived(
            object[] newData,
            object[] oldData)
        {
            AddIndication(newData, oldData);
        }
    }
} // end of namespace