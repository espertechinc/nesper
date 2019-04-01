///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.util;

namespace com.espertech.esper.view.internals
{
    public class NoopView
        : ViewSupport
        , DataWindowView
        , CloneableView
    {
        private readonly NoopViewFactory _viewFactory;

        public NoopView(NoopViewFactory viewFactory)
        {
            _viewFactory = viewFactory;
        }

        public ViewFactory ViewFactory
        {
            get { return _viewFactory; }
        }

        public View CloneView()
        {
            return _viewFactory.MakeView(null);
        }

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
        }

        public override EventType EventType
        {
            get { return _viewFactory.EventType; }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return CollectionUtil.NULL_EVENT_ITERATOR;
        }

        public void VisitView(ViewDataVisitor viewDataVisitor)
        {
        }
    }
} // end of namespace
