///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client
{
    /// <summary>
    /// Descriptor for event property names, property types and access metadata.
    /// </summary>
    public class EventPropertyDescriptor
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="propertyName">name of the property</param>
        /// <param name="propertyType">the property type</param>
        /// <param name="requiresIndex">true if the access to property value access requires an integer index value</param>
        /// <param name="requiresMapkey">true if the access to property value access requires a string map key</param>
        /// <param name="indexed">true if the property is an indexed property, i.e. type is an array or the property value access requires an integer index value</param>
        /// <param name="mapped">true if the property is a mapped property, i.e. type is an Map or the property value access requires an string map key</param>
        /// <param name="fragment">true if the property value can be represented as an EventBean and property type can be represented as an EventType</param>
        public EventPropertyDescriptor(
            string propertyName,
            Type propertyType,
            bool requiresIndex,
            bool requiresMapkey,
            bool indexed,
            bool mapped,
            bool fragment)
        {
            PropertyName = propertyName;
            PropertyType = propertyType ?? typeof(object);
            //PropertyType = propertyType ?? throw new ArgumentException("Null property type");
            IsRequiresIndex = requiresIndex;
            IsRequiresMapkey = requiresMapkey;
            IsIndexed = indexed;
            IsMapped = mapped;
            IsFragment = fragment;
        }

        /// <summary>
        /// Returns the property name.
        /// </summary>
        /// <value>property name</value>
        public string PropertyName { get; }

        /// <summary>
        /// Returns the property underlying type.
        /// <para>
        /// Note that a null values is possible as null values can be selected.
        /// Use {@link #getPropertyType()} for access to type parameters.
        /// </para>
        /// </summary>
        /// <value>underlying property type</value>
        public Type PropertyType { get; }

        /// <summary>
        /// Returns the component type, if applicable.
        /// This is applicable only to arrays and collections, queues and iterators.
        /// Returns null if not applicable.
        /// </summary>
        /// <value>component type</value>
        public Type PropertyComponentType {
            get {
                if (PropertyType == null) {
                    return null;
                }

                var type = PropertyType;
                if (type.IsArray) {
                    return type.GetElementType();
                }

                if (type.IsGenericDictionary()) {
                    return type.GetDictionaryValueType();
                }

                if (type.IsGenericList()) {
                    return type.GetListType();
                }

                if (type.IsGenericCollection() ||
                    type.IsGenericEnumerable()) {
                    return type.GetComponentType();
                }

                return null;
            }
        }

        /// <summary>
        /// Returns true to indicate that the property is an indexed property and requires an
        /// index to access elements of the indexed property. Returns false to indicate that the
        /// property is not an indexed property or does not require an index for property value access.
        /// <para>
        /// For object-style events, a getter-method that takes a single integer parameter
        /// is considered an indexed property that requires an index for access.
        /// </para>
        /// <para>
        /// A getter-method that returns an array is considered an index property but does not
        /// require an index for access.
        /// </para>
        /// </summary>
        /// <value>
        ///     true to indicate that property value access requires an index value
        /// </value>
        public bool IsRequiresIndex { get; }

        /// <summary>
        /// Returns true to indicate that the property is a mapped property and requires a
        /// map key to access elements of the mapped property. Returns false to indicate that the
        /// property is not a mapped property or does not require a map key for property value access.
        /// <para>
        /// For object-style events, a getter-method that takes a single string parameter
        /// is considered a mapped property that requires a map key for access.
        /// </para>
        /// <para>
        /// A getter-method that returns a Map is considered a mapped property but does not
        /// require a map key for access.
        /// </para>
        /// </summary>
        /// <value>true to indicate that property value access requires an index value</value>
        public bool IsRequiresMapkey { get; }

        /// <summary>
        /// Returns true for indexed properties, returns false for all other property styles.
        /// <para>
        /// An indexed property is a property returning an array value or a getter-method taking a
        /// single integer parameter.
        /// </para>
        /// </summary>
        /// <value>indicator whether this property is an index property</value>
        public bool IsIndexed { get; }

        /// <summary>
        /// Returns true for mapped properties, returns false for all other property styles.
        /// <para>
        /// A mapped property is a property returning a Map value or a getter-method taking a
        /// single string (key) parameter.
        /// </para>
        /// </summary>
        /// <value>indicator whether this property is a mapped property</value>
        public bool IsMapped { get; }

        /// <summary>
        /// Returns true to indicate that the property value can itself be represented as an <seealso cref="EventBean" />
        /// and that the property type can be represented as an <seealso cref="EventType" />.
        /// </summary>
        /// <value>indicator whether property is itself a complex data structure representable as a nested <seealso cref="EventType" /></value>
        public bool IsFragment { get; }

        protected bool Equals(EventPropertyDescriptor other)
        {
            return PropertyName == other.PropertyName &&
                   Equals(PropertyType, other.PropertyType) &&
                   IsRequiresIndex == other.IsRequiresIndex &&
                   IsRequiresMapkey == other.IsRequiresMapkey &&
                   IsIndexed == other.IsIndexed &&
                   IsMapped == other.IsMapped &&
                   IsFragment == other.IsFragment;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((EventPropertyDescriptor)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                PropertyName,
                PropertyType,
                IsRequiresIndex,
                IsRequiresMapkey,
                IsIndexed,
                IsMapped,
                IsFragment);
        }

        public override string ToString()
        {
            return
                "Name " +
                PropertyName +
                " PropertyType " +
                PropertyType.CleanName() +
                " IsRequiresIndex " +
                IsRequiresIndex +
                " IsRequiresMapkey " +
                IsRequiresMapkey +
                " IsIndexed " +
                IsIndexed +
                " IsMapped " +
                IsMapped +
                " IsFragment " +
                IsFragment;
        }
    }
} // end of namespace