///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.view.core
{
    public class ViewNullEvent : View
    {
        public static readonly ViewNullEvent INSTANCE = new ViewNullEvent();

        private ViewNullEvent()
        {
        }

        public Viewable Parent {
            get { return null; }
            set { }
        }

        public void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
        }

        public View Child {
            get { return null; }
            set { }
        }

        public EventType EventType {
            get { throw new IllegalStateException("Information not available"); }
        }

        public IEnumerator<EventBean> GetEnumerator()
        {
            return EnumerationHelper.Empty<EventBean>();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
} // end of namespace