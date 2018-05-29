///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.view
{
    public class ViewableDefaultImpl : Viewable
    {
        private readonly EventType _eventType;
    
        public ViewableDefaultImpl(EventType eventType)
        {
            _eventType = eventType;
        }
    
        public View AddView(View view)
        {
            return null;
        }

        public View[] Views
        {
            get { return ViewSupport.EMPTY_VIEW_ARRAY; }
        }

        public bool RemoveView(View view)
        {
            return false;
        }
    
        public void RemoveAllViews()
        {
        }

        public bool HasViews
        {
            get { return false; }
        }

        public EventType EventType
        {
            get { return _eventType; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<EventBean> GetEnumerator()
        {
            return EnumerationHelper.Empty<EventBean>();
            //return ((IEnumerable<EventBean>) null).GetEnumerator();
        }
    }
}
