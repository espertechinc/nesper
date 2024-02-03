///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.view.core
{
    public class ViewableDefaultImpl : ViewSupport
    {
        public ViewableDefaultImpl(EventType eventType)
        {
            EventType = eventType;
        }

        public override EventType EventType { get; }

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return CollectionUtil.NULL_EVENT_ITERATOR;
        }
    }
} // end of namespace