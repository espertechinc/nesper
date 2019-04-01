///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client
{
    /// <summary>
    /// Interface for event representation. All events have an <seealso cref="EventType"/>.
    /// Events also usually have one or more event properties. This interface allows the querying 
    /// of event type, event property values and the underlying event object.
    /// </summary>
    public interface EventBean
    {
        /// <summary>
        /// Return the <seealso cref="EventType"/> instance that describes the set of properties available for this event.
        /// </summary>
        /// <value>event type</value>
        EventType EventType { get; }

        /// <summary>
        /// Returns the value of an event property for the given property name or property expression.
        /// <para/> Returns null if the property value is null. Throws an exception if the expression is not valid against the event type.
        /// <para/> The method takes a property name or property expression as a parameter. Property expressions may include indexed properties
        /// via the syntax "name[index]", mapped properties via the syntax "name('key')", nested properties via the syntax "outer.inner" or
        /// combinations thereof.
        /// </summary>
        Object this[string property] { get; }

        /// <summary>
        /// Returns the value of an event property for the given property name or property expression.
        /// <para/> Returns null if the property value is null. Throws an exception if the expression is not valid against the event type.
        /// <para/> The method takes a property name or property expression as a parameter. Property expressions may include indexed properties 
        /// via the syntax "name[index]", mapped properties via the syntax "name('key')", nested properties via the syntax "outer.inner" or 
        /// combinations thereof.
        /// </summary>
        /// <param name="propertyExpression">name or expression of the property whose value is to be retrieved</param>
        /// <returns>
        /// the value of a property with the specified name.
        /// </returns>
        /// <throws>PropertyAccessException - if there is no property of the specified name, or the property cannot be accessed</throws>
        Object Get(String propertyExpression);

        /// <summary>
        /// Get the underlying data object to this event wrapper.
        /// </summary>
        /// <value>
        /// 	underlying data object, usually either a Map or a bean instance.
        /// </value>
        object Underlying { get; }

        /// <summary>
        /// Returns event beans or array of event bean for a property name or property expression.
        /// <para/>
        /// For use with properties whose value is itself an event or whose value can be represented as
        /// an event by the underlying event representation.
        /// <para/>
        /// The <seealso cref="EventType"/> of the event bean Instance(s) returned by this method can be
        /// determined by <seealso cref="FragmentEventType" />
        /// 	. Use 
        /// <seealso cref="EventPropertyDescriptor"/> to obtain a list of properties that return fragments from an event type.
        /// <para/> 
        /// Returns null if the property value is null or the property value cannot be represented as a 
        /// fragment by the underlying representation.
        /// <para/> 
        /// The method takes a property name or property expression as a parameter. Property expressions may 
        /// include indexed properties via the syntax "name[index]", mapped properties via the syntax "name('key')", 
        /// nested properties via the syntax "outer.inner" or combinations thereof.
        /// </summary>
        /// <param name="propertyExpression">name or expression of the property whose value is to be presented as an EventBean or array of EventBean</param>
        /// <returns>
        /// the value of a property as an EventBean or array of EventBean
        /// </returns>
        /// <throws>PropertyAccessException - if there is no property of the specified name, or the property cannot be accessed</throws>
        Object GetFragment(String propertyExpression);

        //T GetFragment<T>(string propertyExpression);
    }
}
