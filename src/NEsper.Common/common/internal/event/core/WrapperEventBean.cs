///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.core
{
    using DataMap = IDictionary<string, object>;

    /// <summary>
    ///     Event bean that wraps another event bean adding additional properties.
    ///     <para>
    ///         This can be useful for classes for which the statement adds derived values retaining the original class.
    ///     </para>
    ///     <para>
    ///         The event type of such events is always <see cref="WrapperEventType" />. Additional properties are stored in a
    ///         Map.
    ///     </para>
    /// </summary>
    public class WrapperEventBean : EventBeanSPI,
        DecoratingEventBean
    {
        /// <summary>Ctor.</summary>
        /// <param name="theEvent">is the wrapped event</param>
        /// <param name="properties">
        ///     is zero or more property values that embellish the wrapped event
        /// </param>
        /// <param name="eventType">is the <see cref="WrapperEventType" />.</param>
        public WrapperEventBean(
            EventBean theEvent,
            DataMap properties,
            EventType eventType)
        {
            UnderlyingEvent = theEvent;
            UnderlyingMap = properties;
            EventType = eventType;
        }

        /// <summary>
        ///     Returns the underlying map storing the additional properties, if any.
        /// </summary>
        /// <returns>event property IDictionary</returns>
        public DataMap UnderlyingMap { get; private set; }

        /// <summary>
        ///     Returns decorating properties.
        /// </summary>
        /// <value></value>
        /// <returns>property name and values</returns>
        public DataMap DecoratingProperties => UnderlyingMap;

        /// <summary>Returns the wrapped event.</summary>
        /// <returns>wrapped event</returns>
        public EventBean UnderlyingEvent { get; private set; }

        /// <summary>
        ///     Returns the value of an event property.
        /// </summary>
        /// <value></value>
        /// <returns>
        ///     the value of a simple property with the specified name.
        /// </returns>
        /// <throws>  PropertyAccessException - if there is no property of the specified name, or the property cannot be accessed </throws>
        public object this[string property] => GetInternal(property);

        /// <summary>
        ///     Returns the value of an event property.  This method is a proxy of the indexer.
        /// </summary>
        /// <param name="property">name of the property whose value is to be retrieved</param>
        /// <returns>
        ///     the value of a simple property with the specified name.
        /// </returns>
        /// <throws>  PropertyAccessException - if there is no property of the specified name, or the property cannot be accessed </throws>
        public virtual object Get(string property)
        {
            return GetInternal(property);
        }

        /// <summary>
        ///     Return the <see cref="EventType" /> instance that describes the set of properties available for this event.
        /// </summary>
        /// <value></value>
        /// <returns>
        ///     event type
        /// </returns>
        public EventType EventType { get; }

        /// <summary>
        ///     Get the underlying data object to this event wrapper.
        /// </summary>
        /// <value></value>
        /// <returns>
        ///     underlying data object, usually either a Map or a bean instance.
        /// </returns>
        public object Underlying {
            get {
                // If wrapper is simply for the underlying with no additional properties, then return the underlying type
                if (UnderlyingMap.Count == 0) {
                    return UnderlyingEvent.Underlying;
                }

                return new Pair<object, DataMap>(UnderlyingEvent.Underlying, UnderlyingMap);
            }
            set {
                var type = (WrapperEventType)EventType;
                UnderlyingEvent = EventTypeUtility.GetShellForType(type.UnderlyingEventType);
                if (value is Pair<object, IDictionary<string, object>> pair) {
                    ((EventBeanSPI) UnderlyingEvent).Underlying = pair.First;
                    UnderlyingMap = pair.Second;
                }
                else {
                    ((EventBeanSPI) UnderlyingEvent).Underlying = value;
                    UnderlyingMap = Collections.GetEmptyDataMap();
                }
            }
        }

        public object GetFragment(string propertyExpression)
        {
            var getter = EventType.GetGetter(propertyExpression);
            if (getter == null) {
                throw PropertyAccessException.NotAValidProperty(propertyExpression);
            }

            return getter.GetFragment(this);
        }

        /// <summary>
        ///     Internal getter implementation.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        /// <exception cref="PropertyAccessException">Property named '" + property + "' is not a valid property name for this type</exception>
        private object GetInternal(string property)
        {
            var getter = EventType.GetGetter(property);
            if (getter == null) {
                throw new PropertyAccessException(
                    "Property named '" + property + "' is not a valid property name for this type");
            }

            return getter.Get(this);
        }

        /// <summary>
        ///     Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override string ToString()
        {
            return
                "WrapperEventBean " +
                "[event=" +
                UnderlyingEvent +
                "] " +
                "[properties=" +
                UnderlyingMap +
                "]";
        }
    }
} // End of namespace