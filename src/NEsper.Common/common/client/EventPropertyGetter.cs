///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client
{
    /// <summary>
    /// Get property values from an event instance for a given event property. Instances that
    /// implement this interface are usually bound to a particular <seealso cref="EventType"/>
    /// and cannot be used to access <seealso cref="EventBean"/> instances of a different type.
    /// </summary>
    public interface EventPropertyGetter : EventPropertyValueGetter
    {
#if INHERITED
        /// <summary>
        /// Return the value for the property in the event object specified when the instance was obtained. Useful for 
        /// fast access to event properties. Throws a PropertyAccessException if the getter instance doesn't match the 
        /// EventType it was obtained from, and to indicate other property access problems.
        /// </summary>
        /// <param name="eventBean">is the event to get the value of a property from</param>
        /// <returns>value of property in event</returns>
        /// <throws>PropertyAccessException to indicate that property access failed</throws>
        Object Get(EventBean eventBean);
#endif

        /// <summary>
        /// Returns true if the property exists, or false if the type does not have such a property.
        /// <para/>
        /// Useful for dynamic properties of the syntax "property?" and the dynamic nested/indexed/mapped versions.
        /// Dynamic nested properties follow the syntax "property?.Nested" which is equivalent to "property?.Nested?". 
        /// If any of the properties in the path of a dynamic nested property return null, the dynamic nested property 
        /// does not exists and the method returns false. <para/> For non-dynamic properties, this method always returns
        /// true since a getter would not be available unless
        /// </summary>
        /// <param name="eventBean">is the event to check if the dynamic property exists</param>
        /// <returns>
        /// indictor whether the property exists, always true for non-dynamic (default) properties
        /// </returns>
        bool IsExistsProperty(EventBean eventBean);

        /// <summary>
        /// Returns <seealso cref="EventBean"/> or array of <seealso cref="EventBean"/> for a property name or
        /// property expression. <para/> For use with properties whose value is itself an event or whose value can
        /// be represented as an event by the underlying event representation. <para/> The <seealso cref="EventType"/> of
        /// the <seealso cref="EventBean"/> Instance(s) returned by this method can be determined by
        /// <seealso cref="EventType.GetFragmentType(String)"/>.  Use <seealso cref="EventPropertyDescriptor"/> to obtain a list of
        /// properties that return fragments from an event type. <para/> Returns null if the property value is null or the
        /// property value cannot be represented as a fragment by the underlying representation.
        /// </summary>
        /// <param name="eventBean">is the event to get the fragment value of a property</param>
        /// <returns>
        /// the value of a property as an EventBean or array of EventBean
        /// </returns>
        /// <throws>PropertyAccessException - if there is no property of the specified name, or the property cannot be accessed</throws>
        object GetFragment(EventBean eventBean);
    }

    public class ProxyEventPropertyGetter : EventPropertyGetter
    {
        public Func<EventBean, object> ProcGet { get; set; }
        public Func<EventBean, object> ProcGetFragment { get; set; }
        public Func<EventBean, bool> ProcIsExistsProperty { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyEventPropertyGetter"/> class.
        /// </summary>
        public ProxyEventPropertyGetter()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyEventPropertyGetter"/> class.
        /// </summary>
        /// <param name="pMethodGet">The _p method get.</param>
        /// <param name="pMethodGetFragment">The _p method get fragment.</param>
        /// <param name="pMethodIsExistsProperty">The _p method is exists property.</param>
        public ProxyEventPropertyGetter(
            Func<EventBean, object> pMethodGet,
            Func<EventBean, object> pMethodGetFragment,
            Func<EventBean, bool> pMethodIsExistsProperty)
        {
            ProcGet = pMethodGet;
            ProcGetFragment = pMethodGetFragment;
            ProcIsExistsProperty = pMethodIsExistsProperty;
        }

        /// <summary>
        /// Return the value for the property in the event object specified when the
        /// instance was obtained. Useful for fast access to event properties. Throws a
        /// PropertyAccessException if the getter instance doesn't match the EventType it was obtained
        /// from, and to indicate other property access problems.
        /// </summary>
        /// <param name="eventBean">is the event to get the value of a property from</param>
        /// <returns>
        /// value of property in event
        /// </returns>
        /// <throws>PropertyAccessException to indicate that property access failed</throws>
        public object Get(EventBean eventBean)
        {
            return ProcGet.Invoke(eventBean);
        }

        /// <summary>
        /// Returns true if the property exists, or false if the type does not have such a
        /// property.
        /// <para/>
        /// Useful for dynamic properties of the syntax "property?" and the dynamic
        /// nested/indexed/mapped versions. Dynamic nested properties follow the syntax
        /// "property?.Nested" which is equivalent to "property?.Nested?". If any of the properties in
        /// the path of a dynamic nested property return null, the dynamic nested property does
        /// not exists and the method returns false.
        /// <para/>
        /// For non-dynamic properties, this method always returns true since a getter
        /// would not be available unless
        /// </summary>
        /// <param name="eventBean">is the event to check if the dynamic property exists</param>
        /// <returns>
        /// indictor whether the property exists, always true for non-dynamic (default)
        /// properties
        /// </returns>
        public bool IsExistsProperty(EventBean eventBean)
        {
            return ProcIsExistsProperty.Invoke(eventBean);
        }

        /// <summary>
        /// Returns <seealso cref="EventBean"/> or array of <seealso cref="EventBean"/> for
        /// a property name or property expression.
        /// <para/>
        /// For use with properties whose value is itself an event or whose value can be
        /// represented as an event by the underlying event representation.
        /// <para/>
        /// The <seealso cref="EventType"/> of the <seealso cref="EventBean"/> Instance(s)
        /// returned by this method can be determined by<seealso cref="EventType.GetFragmentType" />.
        /// Use <seealso cref="EventPropertyDescriptor"/> to obtain a list of
        /// properties that return fragments from an event type.
        /// <para/>
        /// Returns null if the property value is null or the property value cannot be
        /// represented as a fragment by the underlying representation.
        /// </summary>
        /// <param name="eventBean">is the event to get the fragment value of a property</param>
        /// <returns>
        /// the value of a property as an EventBean or array of EventBean
        /// </returns>
        /// <throws>PropertyAccessException - if there is no property of the specified name, or the property cannot be accessed</throws>
        public object GetFragment(EventBean eventBean)
        {
            return ProcGetFragment.Invoke(eventBean);
        }
    }
}