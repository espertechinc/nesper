///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.context.util
{
    public class ContextMergeViewForwarding : ContextMergeView
    {
        public ContextMergeViewForwarding(EventType eventType) : base(eventType)
        {
        }

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            Child.Update(newData, oldData);
        }
    }
} // end of namespace