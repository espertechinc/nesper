///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.view;

namespace com.espertech.esper.supportunit.view
{
    public abstract class SupportBaseView : ViewSupport
    {
        private EventType _eventType;
        private bool _isInvoked;
        private EventBean[] _lastNewData;
        private EventBean[] _lastOldData;

        /// <summary>
        /// Default constructor since views are also beans.
        /// </summary>
        protected SupportBaseView()
        {
        }

        protected SupportBaseView(EventType eventType)
        {
            _eventType = eventType;
        }

        public override EventType EventType
        {
            get { return _eventType; }
        }

        public void SetEventType(EventType value)
        {
            _eventType = value;
        }

        public EventBean[] LastNewData
        {
            get { return _lastNewData; }
            set { _lastNewData = value; }
        }

        public EventBean[] LastOldData
        {
            get { return _lastOldData; }
            set { _lastOldData = value; }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return EnumerationHelper.Empty<EventBean>();
        }

        public void ClearLastNewData()
        {
            _lastNewData = null;
        }

        public void ClearLastOldData()
        {
            _lastOldData = null;
        }

        public bool IsInvoked
        {
            set { _isInvoked = value; }
            get { return _isInvoked; }
        }

        public bool GetAndClearIsInvoked()
        {
            bool invoked = _isInvoked;
            _isInvoked = false;
            return invoked;
        }

        public void Reset()
        {
            _isInvoked = false;
            _lastNewData = null;
            _lastOldData = null;
        }
    }
}
