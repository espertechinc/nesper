///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.view.core
{
    public abstract class ViewSupport : View
    {
        private View _child;
        private Viewable _parent;

        public virtual Viewable Parent {
            get => _parent;
            set => _parent = value;
        }

        public virtual View Child {
            get => _child;
            set => _child = value;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public abstract IEnumerator<EventBean> GetEnumerator();
        public abstract EventType EventType { get; }

        public abstract void Update(
            EventBean[] newData,
            EventBean[] oldData);
    }
} // end of namespace