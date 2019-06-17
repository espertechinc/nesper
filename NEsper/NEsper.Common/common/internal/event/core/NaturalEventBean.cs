///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.@event.core
{
    /// <summary>
    ///     An event that is carries multiple representations of event properties: A
    ///     synthetic representation that is designed for delivery as <seealso cref="EventBean" /> to
    ///     client <seealso cref="UpdateListener" /> code, and a
    ///     natural representation as a bunch of Object-type properties for fast delivery to
    ///     client subscriber objects via method call.
    /// </summary>
    public class NaturalEventBean : EventBean,
        DecoratingEventBean
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="eventBeanType">the event type of the synthetic event</param>
        /// <param name="natural">the properties of the event</param>
        /// <param name="optionalSynthetic">the event bean that is the synthetic event, or null if no synthetic is packed in</param>
        public NaturalEventBean(
            EventType eventBeanType,
            object[] natural,
            EventBean optionalSynthetic)
        {
            EventType = eventBeanType;
            Natural = natural;
            OptionalSynthetic = optionalSynthetic;
        }

        /// <summary>
        ///     Returns the column object result representation.
        /// </summary>
        /// <returns>
        ///     select column values
        /// </returns>
        public object[] Natural { get; }

        /// <summary>
        ///     Returns the synthetic event that can be attached.
        /// </summary>
        /// <returns>
        ///     synthetic if attached, or null if none attached
        /// </returns>
        public EventBean OptionalSynthetic { get; }

        public EventBean UnderlyingEvent => ((DecoratingEventBean) OptionalSynthetic).UnderlyingEvent;

        public IDictionary<string, object> DecoratingProperties =>
            ((DecoratingEventBean) OptionalSynthetic).DecoratingProperties;

        public EventType EventType { get; }

        #region Implementation of EventBean

        public object this[string property] => Get(property);

        #endregion

        public object Get(string property)
        {
            if (OptionalSynthetic != null) {
                return OptionalSynthetic.Get(property);
            }

            throw new PropertyAccessException(
                "Property access not allowed for natural events without the synthetic event present");
        }

        public object Underlying => OptionalSynthetic != null
            ? OptionalSynthetic.Underlying
            : Natural;

        public object GetFragment(string propertyExpression)
        {
            if (OptionalSynthetic != null) {
                return OptionalSynthetic.GetFragment(propertyExpression);
            }

            throw new PropertyAccessException(
                "Property access not allowed for natural events without the synthetic event present");
        }
    }
}