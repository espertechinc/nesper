///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;

namespace com.espertech.esper.events.vaevent
{
    /// <summary>
    /// An event bean that represents multiple potentially disparate underlying events and presents a unified face
    /// across each such types or even any type.
    /// </summary>
    public class VariantEventBean
        : EventBean
        , VariantEvent
    {
        private readonly VariantEventType _variantEventType;
        private readonly EventBean _underlyingEventBean;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="variantEventType">the event type</param>
        /// <param name="underlying">the event</param>
        public VariantEventBean(VariantEventType variantEventType, EventBean underlying)
        {
            _variantEventType = variantEventType;
            _underlyingEventBean = underlying;
        }

        public EventType EventType
        {
            get { return _variantEventType; }
        }

        public Object Get(string property)
        {
            EventPropertyGetter getter = _variantEventType.GetGetter(property);
            if (getter == null)
            {
                return null;
            }
            return getter.Get(this);
        }

        public object this[string property]
        {
            get { return Get(property); }
        }

        public object Underlying
        {
            get { return _underlyingEventBean.Underlying; }
        }

        /// <summary>
        /// Returns the underlying event.
        /// </summary>
        /// <value>underlying event</value>
        public EventBean UnderlyingEventBean
        {
            get { return _underlyingEventBean; }
        }

        public Object GetFragment(string propertyExpression)
        {
            EventPropertyGetter getter = _variantEventType.GetGetter(propertyExpression);
            if (getter == null)
            {
                throw PropertyAccessException.NotAValidProperty(propertyExpression);
            }
            return getter.GetFragment(this);
        }
    }
} // end of namespace
