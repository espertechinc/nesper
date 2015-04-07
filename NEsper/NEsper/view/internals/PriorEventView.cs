///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;

namespace com.espertech.esper.view.internals
{
    /// <summary>
    /// View that provides access to prior events posted by the _parent view for use by 'prior' expression nodes.
    /// </summary>
    public class PriorEventView
        : ViewSupport,
            ViewDataVisitable
    {
        private Viewable _parent;
        private readonly ViewUpdatedCollection _buffer;

        /// <summary>Ctor. </summary>
        /// <param name="buffer">is handling the actual storage of events for use in the 'prior' expression</param>
        public PriorEventView(ViewUpdatedCollection buffer)
        {
            _buffer = buffer;
        }

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            _buffer.Update(newData, oldData);
            UpdateChildren(newData, oldData);
        }

        public override Viewable Parent
        {
            set { _parent = value; }
        }

        /// <summary>Returns the underlying buffer used for access to prior events. </summary>
        /// <value>buffer</value>
        public ViewUpdatedCollection Buffer
        {
            get { return _buffer; }
        }

        public override EventType EventType
        {
            get { return _parent.EventType; }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return _parent.GetEnumerator();
        }

        public void VisitView(ViewDataVisitor viewDataVisitor)
        {
            viewDataVisitor.VisitPrimary(_buffer, "Prior");
        }
    }
}