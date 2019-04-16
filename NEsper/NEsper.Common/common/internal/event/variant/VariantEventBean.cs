///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.@event.variant
{
    /// <summary>
    ///     An event bean that represents multiple potentially disparate underlying events and presents a unified face
    ///     across each such types or even any type.
    /// </summary>
    public class VariantEventBean : EventBean,
        VariantEvent
    {
        private readonly VariantEventType variantEventType;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="variantEventType">the event type</param>
        /// <param name="underlying">the event</param>
        public VariantEventBean(
            VariantEventType variantEventType,
            EventBean underlying)
        {
            this.variantEventType = variantEventType;
            UnderlyingEventBean = underlying;
        }

        public EventType EventType => variantEventType;

        public object Get(string property)
        {
            var getter = variantEventType.GetGetter(property);
            return getter?.Get(this);
        }

        public object this[string property] => Get(property);

        public object Underlying => UnderlyingEventBean.Underlying;

        public object GetFragment(string propertyExpression)
        {
            var getter = variantEventType.GetGetter(propertyExpression);
            if (getter == null) {
                throw PropertyAccessException.NotAValidProperty(propertyExpression);
            }

            return getter.GetFragment(this);
        }

        /// <summary>
        ///     Returns the underlying event.
        /// </summary>
        /// <returns>underlying event</returns>
        public EventBean UnderlyingEventBean { get; }
    }
} // end of namespace