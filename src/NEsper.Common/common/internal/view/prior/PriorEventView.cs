///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.view.core;

namespace com.espertech.esper.common.@internal.view.prior
{
    /// <summary>
    ///     View that provides access to prior events posted by the parent view for use by 'prior' expression nodes.
    /// </summary>
    public class PriorEventView : ViewSupport,
        ViewDataVisitable
    {
        private readonly ViewUpdatedCollection _buffer;
        private Viewable _parent;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="buffer">is handling the actual storage of events for use in the 'prior' expression</param>
        public PriorEventView(ViewUpdatedCollection buffer)
        {
            _buffer = buffer;
        }

        public override Viewable Parent {
            set => _parent = value;
        }

        /// <summary>
        ///     Returns the underlying buffer used for access to prior events.
        /// </summary>
        /// <returns>buffer</returns>
        public ViewUpdatedCollection Buffer => _buffer;

        public override EventType EventType => _parent.EventType;

        public void VisitView(ViewDataVisitor viewDataVisitor)
        {
            viewDataVisitor.VisitPrimary(_buffer, "Prior");
        }

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            _buffer.Update(newData, oldData);
            child.Update(newData, oldData);
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return _parent.GetEnumerator();
        }
    }
} // end of namespace