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
    /// An event bean that represents multiple potentially disparate underlying events and presents
    /// a unified face across each such types or even any type.
    /// </summary>
    public class VariantEventBean : EventBean, VariantEvent
    {
        private readonly VariantEventType variantEventType;
        private readonly EventBean underlyingEventBean;
    
        /// <summary>Ctor. </summary>
        /// <param name="variantEventType">the event type</param>
        /// <param name="underlying">the event</param>
        public VariantEventBean(VariantEventType variantEventType, EventBean underlying)
        {
            this.variantEventType = variantEventType;
            this.underlyingEventBean = underlying;
        }

        /// <summary>
        /// Return the <see cref="EventType"/> instance that describes the set of properties available for this event.
        /// </summary>
        /// <value></value>
        /// <returns> event type
        /// </returns>
        public EventType EventType
        {
            get { return variantEventType; }
        }

        /// <summary>
        /// Returns the value of an event property.
        /// </summary>
        /// <value></value>
        /// <returns> the value of a simple property with the specified name.
        /// </returns>
        /// <throws>  PropertyAccessException - if there is no property of the specified name, or the property cannot be accessed </throws>
        public Object this[String property]
        {
            get
            {
                EventPropertyGetter getter = variantEventType.GetGetter(property);
                return getter != null ? getter.Get(this) : null;
            }
        }


        /// <summary>
        /// Returns the value of an event property.  This method is a proxy of the indexer.
        /// </summary>
        /// <param name="property">name of the property whose value is to be retrieved</param>
        /// <returns>
        /// the value of a simple property with the specified name.
        /// </returns>
        /// <throws>  PropertyAccessException - if there is no property of the specified name, or the property cannot be accessed </throws>
        public object Get(string property)
        {
            return this[property];
        }

        /// <summary>
        /// Get the underlying data object to this event wrapper.
        /// </summary>
        /// <value></value>
        /// <returns> underlying data object, usually either a Map or a bean instance.
        /// </returns>
        public object Underlying
        {
            get { return underlyingEventBean.Underlying; }
        }

        /// <summary>Returns the underlying event. </summary>
        /// <returns>underlying event</returns>
        public EventBean UnderlyingEventBean
        {
            get { return underlyingEventBean; }
        }

        public Object GetFragment(String propertyExpression)
        {
            EventPropertyGetter getter = variantEventType.GetGetter(propertyExpression);
            if (getter == null)
            {
                throw new PropertyAccessException("Property named '" + propertyExpression + "' is not a valid property name for this type");
            }
            return getter.GetFragment(this);
        }
    }
}
