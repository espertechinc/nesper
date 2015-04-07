///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.events;

namespace com.espertech.esper.client
{
    /// <summary>
    /// This interface provides metadata on events.
    /// <para/>
    /// The interface exposes events as organizations of named values. The contract is that any event in the system must have a 
    /// name-based way of accessing sub-data within its event type. A simple example is an object: the names can be property names,
    /// and those properties can have still more properties beneath them. Another example is a Map structure. Here string names 
    /// can referto data objects.
    /// <para/> 
    /// The interface presents an immutable view of events. There are no methods to change property values. Events by definition are
    /// an observation of a past occurrance or state change and may not be modified.
    /// <para/>
    /// Information on the super-types (superclass and interfaces implemented by events) is also available, for PONO events as well
    /// as for Map event types that has supertypes.
    /// <para/>
    /// Implementations provide metadata on the properties that an implementation itself provides.
    /// <para/>
    /// Implementations also allow property expressioms that may use nested, indexed, mapped or a combination of these as a syntax
    /// to access property types and values.
    /// <para/>
    /// Implementations in addition may provide a means to access property values as event fragments, which are typed events themselves.
    /// </summary>
    public interface EventType
    {
        /// <summary>
        /// Get the type of an event property.
        /// <para/>
        /// Returns null if the property name or property expression is not valid against the event type. Can also return null
        /// if a select-clause selects a constant null value. <para/> The method takes a property name or property expression as
        /// a parameter. Property expressions may include indexed properties via the syntax "name[index]", mapped properties via the syntax
        /// "name('key')", nested properties via the syntax "outer.inner" or combinations thereof.
        /// <para/>
        /// Returns unboxed (such as 'typeof(int)') as well as boxed 'typeof(int?)' type.
        /// </summary>
        /// <param name="propertyExpression">is the property name or property expression</param>
        /// <returns>
        /// type of the property, the unboxed or the boxed type.
        /// </returns>
        Type GetPropertyType(String propertyExpression);

        /// <summary>
        /// Check that the given property name or property expression is valid for this event type, ie. that the property exists
        /// on the event type.
        /// <para/> 
        /// The method takes a property name or property expression as a parameter. Property expressions may include indexed
        /// properties via the syntax "name[index]", mapped properties via the syntax "name('key')", nested properties via the
        /// syntax "outer.inner" or combinations thereof.
        /// </summary>
        /// <param name="propertyExpression">is the property name or property expression to check</param>
        /// <returns>true if exists, false if not</returns>
        bool IsProperty(String propertyExpression);

        /// <summary>
        /// Get the getter of an event property or property expression:
        /// Getters are useful when an application receives events of the same event type multiple times and requires fast access
        /// to an event property or nested, indexed or mapped property.
        /// <para/>
        /// Returns null if the property name or property expression is not valid against the event type.
        /// <para/> 
        /// The method takes a property name or property expression as a parameter. Property expressions may include indexed 
        /// properties via the syntax "name[index]", mapped properties via the syntax "name('key')", nested properties via the
        /// syntax "outer.inner" or combinations thereof.
        /// </summary>
        /// <param name="propertyExpression">is the property name or property expression</param>
        /// <returns>
        /// a getter that can be used to obtain property values for event instances of the same event type
        /// </returns>
        EventPropertyGetter GetGetter(String propertyExpression);

        /// <summary>
        /// Returns the event type of the fragment that is the value of a property name or property expression.
        /// <para/>
        /// Returns null if the property name or property expression is not valid or does not return a fragment for the event type.
        /// <para/>
        /// The <seealso cref="EventPropertyDescriptor" /> provides a flag that indicates which properties provide fragment events.
        /// <para/>
        /// This is useful for navigating properties that are itself events or other well-defined types that the underlying event
        /// representation may represent as an event type. It is up to each event representation to determine what properties can
        /// be represented as event types themselves.
        /// <para/>
        /// The method takes a property name or property expression as a parameter. Property expressions may include indexed properties
        /// via the syntax "name[index]", mapped properties via the syntax "name('key')", nested properties via the syntax "outer.inner"
        /// or combinations thereof.
        /// <para/>
        /// The underlying event representation may not support providing fragments or therefore fragment event types for any or all
        /// properties, in which case the method returns null.
        /// <para/>
        /// Use the <seealso cref="PropertyDescriptors" /> method to obtain a list of properties for which a fragment event type may be
        /// retrieved by this method.
        /// </summary>
        /// <param name="propertyExpression">is the name of the property to return the fragment event type</param>
        /// <returns>fragment event type of the property</returns>
        FragmentEventType GetFragmentType(String propertyExpression);

        /// <summary>
        /// Get the class that represents the type of the event type.
        /// Returns an event class if the schema represents a POCO event type.
        /// Returns IDictionary is the schema represents a collection of values in a Map.
        /// </summary>
        /// <returns>type of the event object</returns>
        Type UnderlyingType { get; }

        /// <summary>
        /// Get the property names for the event type.
        /// <para>
        /// Note that properties do not have a defined order. Your application should not rely on the order of properties returned by this method.
        /// </para>
        /// <para>
        /// The method does not return property names of inner or nested types.
        /// </para>
        /// </summary>
        /// <returns>
        /// A string array containing the property names of this typed event data object.
        /// </returns>
        string[] PropertyNames { get; }

        /// <summary>Get property descriptors for the event type.
        /// <para>
        /// Note that properties do not have a defined order. Your application should not rely 
        /// on the order of properties returned by this method.
        /// </para>
        /// <para>
        /// The method does not return property information of inner or nested types.
        /// </para>
        ///  </summary>
        /// <returns>descriptors for all known properties of the event type.</returns>
        IList<EventPropertyDescriptor> PropertyDescriptors { get; }

        /// <summary>
        /// Get the property descriptor for a given property of the event, or null if a property by that name was not found.
        /// <para>
        /// The property name parameter does accept a property expression. It therefore does not allow the indexed,
        /// mapped or nested property expression syntax and only returns the descriptor for the event type's known
        /// properties.
        /// </para>
        /// <para> 
        /// The method does not return property information of inner or nested types.
        /// </para>
        /// <para>
        /// For returning a property descriptor for nested, indexed or mapped properties 
        /// <seealso cref="EventTypeUtility" />
        /// </para>
        /// </summary>
        /// <param name="propertyName">property name</param>
        /// <returns>descriptor for the named property</returns>
        EventPropertyDescriptor GetPropertyDescriptor(String propertyName);

        /// <summary>
        /// Returns an enumeration of event types that are super to this event type, from which this event type inherited event properties.
        /// <para/>
        /// For POCO instances underlying the event this method returns the event types for all superclasses extended by
        /// the POCO and all interfaces implemented by the POCO.
        /// </summary>
        /// <value>The super types.</value>
        /// <returns>an array of event types</returns>
        EventType[] SuperTypes { get; }

        /// <summary>
        /// Returns enumerator over all super types to event type, going up the hierarchy and including all interfaces (and their
        /// extended interfaces) and superclasses as EventType instances.
        /// </summary>
        /// <value>The deep super types.</value>
        EventType[] DeepSuperTypes { get; }

        /// <summary>
        /// Returns the type name or null if no type name is assigned.
        /// <para/> A type name is available for application-configured event types and for event types that represent events of a stream populated by insert-into.
        /// <para/> No type name is available for anonymous statement-specific event type.
        /// </summary>
        /// <value>The name.</value>
        /// <returns>type name or null if none assigned</returns>
        string Name { get; }
   
        /// <summary>Get the getter of an event property that is a mapped event property: Getters are useful when an application receives events of the same event type multiple times and requires fast access to a mapped property. <para /> Returns null if the property name is not valid against the event type or the property is not a mapped property. <para /> The method takes a mapped property name (and not a property expression) as a parameter. </summary>
        /// <param name="mappedPropertyName">is the property name</param>
        /// <returns>a getter that can be used to obtain property values for event instances of the same event type</returns>
        EventPropertyGetterMapped GetGetterMapped(String mappedPropertyName);

        /// <summary>Get the getter of an event property that is a indexed event property: Getters are useful when an application receives events of the same event type multiple times and requires fast access to a indexed property. <para /> Returns null if the property name is not valid against the event type or the property is not an indexed property. <para /> The method takes a indexed property name (and not a property expression) as a parameter. </summary>
        /// <param name="indexedPropertyName">is the property name</param>
        /// <returns>a getter that can be used to obtain property values for event instances of the same event type</returns>
        EventPropertyGetterIndexed GetGetterIndexed(string indexedPropertyName);

        /// <summary>Returns the event type id assigned to the event type. </summary>
        /// <value>event type id</value>
        int EventTypeId { get; }

        /// <summary>Returns the property name of the property providing the start timestamp value. </summary>
        /// <value>start timestamp property name</value>
        string StartTimestampPropertyName { get; }

        /// <summary>Returns the property name of the property providing the end timestamp value. </summary>
        /// <value>end timestamp property name</value>
        string EndTimestampPropertyName { get; }
    }
}
