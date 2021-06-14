///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.view.core
{
    public class ViewNoop : View
    {
        public static readonly ViewNoop INSTANCE = new ViewNoop();

        private ViewNoop()
        {
        }

        public Viewable Parent {
            get => null;
            set { }
        }

        public void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
        }

        public View Child {
            get => null;
            set { }
        }

        public EventType EventType => throw new IllegalStateException("Information not available");

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<EventBean> GetEnumerator()
        {
            return CollectionUtil.NULL_EVENT_ITERATOR;
        }
    }
} // end of namespace