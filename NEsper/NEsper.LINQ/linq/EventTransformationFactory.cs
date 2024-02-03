///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.magic;

namespace com.espertech.esper.runtime.client.linq
{
    /// <summary>
    /// Creates event transformation.
    /// </summary>
    public class EventTransformationFactory
    {
        /// <summary>
        /// Returns the defaults the transformation from an eventBean to a type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Func<EventBean, T> DefaultTransformation<T>()
        {
            if (typeof(T) == typeof(EventBean))
            {
                return eventBean => (T) eventBean;
            }
            if (typeof(T).IsInterface || typeof(T).IsAbstract)
            {
                throw new ArgumentException(
                    "Can not create default transformation for interfaces or abstract classes");
            }

            var magicType = MagicType.GetCachedType(typeof(T));
            return eventBean => DefaultTransformation<T>(magicType, eventBean);
        }

        /// <summary>
        /// Defaults the transformation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="magicType">Type of the magic.</param>
        /// <param name="eventBean">The event bean.</param>
        /// <returns></returns>
        private static T DefaultTransformation<T>(MagicType magicType, EventBean eventBean)
        {
            if (eventBean.Underlying is T)
            {
                return (T) eventBean.Underlying;
            }

            var eventType = eventBean.EventType;
            var instance = (T) Activator.CreateInstance(typeof(T));

            foreach (var propertyName in eventType.PropertyNames)
            {
                var magicProperty = magicType.ResolveProperty(propertyName, PropertyResolutionStyle.CASE_SENSITIVE);
                if (magicProperty != null)
                {
                    var propertyValue = eventBean.Get(propertyName);
                    magicProperty.SetFunction(instance, propertyValue);
                }
            }

            return instance;
        }
    }
}