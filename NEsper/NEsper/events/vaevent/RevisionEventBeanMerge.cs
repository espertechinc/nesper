///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.util;

namespace com.espertech.esper.events.vaevent
{
    /// <summary>
    /// Merge-event for event revisions.
    /// </summary>
    public class RevisionEventBeanMerge : EventBean
    {
        private readonly RevisionEventType _revisionEventType;
        private NullableObject<Object>[] _overlay;

        /// <summary>Ctor. </summary>
        /// <param name="revisionEventType">type</param>
        /// <param name="underlyingFull">event wrapped</param>
        public RevisionEventBeanMerge(RevisionEventType revisionEventType, EventBean underlyingFull)
        {
            _revisionEventType = revisionEventType;
            UnderlyingFullOrDelta = underlyingFull;
        }

        /// <summary>Returns overlay values. </summary>
        /// <value>overlay</value>
        public NullableObject<Object>[] Overlay
        {
            get { return _overlay; }
            set { _overlay = value; }
        }

        /// <summary>Returns flag indicated latest or not. </summary>
        /// <value>latest flag</value>
        public bool IsLatest { get; set; }

        /// <summary>Returns the key. </summary>
        /// <value>key</value>
        public object Key { get; set; }

        /// <summary>Returns last base event. </summary>
        /// <value>base event</value>
        public EventBean LastBaseEvent { get; set; }

        public EventType EventType
        {
            get { return _revisionEventType; }
        }

        public object this[string property]
        {
            get { return Get(property); }
        }

        public Object Get(String property)
        {
            var getter = _revisionEventType.GetGetter(property);
            if (getter == null)
            {
                return null;
            }
            return getter.Get(this);
        }

        public object Underlying
        {
            get { return typeof (RevisionEventBeanMerge); }
        }

        /// <summary>Returns wrapped event </summary>
        /// <value>event</value>
        public EventBean UnderlyingFullOrDelta { get; private set; }

        /// <summary>Returns base event value. </summary>
        /// <param name="parameters">supplies getter</param>
        /// <returns>value</returns>
        public Object GetBaseEventValue(RevisionGetterParameters parameters)
        {
            return parameters.BaseGetter.Get(LastBaseEvent);
        }
    
        /// <summary>Returns a versioned value. </summary>
        /// <param name="parameters">getter and indexes</param>
        /// <returns>value</returns>
        public Object GetVersionedValue(RevisionGetterParameters parameters)
        {
            int propertyNumber = parameters.PropertyNumber;
    
            if (_overlay != null)
            {
                var value = _overlay[propertyNumber];
                if (value != null)
                {
                    return value.Value;
                }
            }
    
            var getter = parameters.BaseGetter;
            if (getter == null)
            {
                return null;  // The property was added by a delta event and only exists on a delta
            }
            if (LastBaseEvent != null) {
                return getter.Get(LastBaseEvent);
            }
            return null;
        }
    
        public Object GetFragment(String propertyExpression)
        {
            var getter = _revisionEventType.GetGetter(propertyExpression);
            if (getter == null)
            {
                return null;
            }
            return getter.GetFragment(this);
        }
    }
}
