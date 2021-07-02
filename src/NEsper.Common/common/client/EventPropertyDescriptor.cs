///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client
{
    /// <summary>
    ///     Descriptor for event property names, property types and access metadata.
    /// </summary>
    public class EventPropertyDescriptor
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="propertyName">name of the property</param>
        /// <param name="propertyType">the property type</param>
        /// <param name="isRequiresIndex">true if the access to property value access requires an integer index value</param>
        /// <param name="isRequiresMapKey">true if the access to property value access requires a string map key</param>
        /// <param name="indexed">true if the property is an indexed property, i.e. type is an array or the property value access requires an integer index value</param>
        /// <param name="mapped">true if the property is a mapped property, i.e. type is an Map or the property value access requires an string map key</param>
        /// <param name="fragment">true if the property value can be represented as an EventBean and property type can be represented as an EventType</param>
        public EventPropertyDescriptor(
            string propertyName,
            Type propertyType,
            bool isRequiresIndex,
            bool isRequiresMapKey,
            bool indexed,
            bool mapped,
            bool fragment)
        {
            if (propertyType == null) {
                throw new ArgumentNullException(nameof(propertyType));
            }

            PropertyName = propertyName;
            PropertyTypeRaw = propertyType; // GetBoxedType
            IsRequiresIndex = isRequiresIndex;
            IsRequiresMapKey = isRequiresMapKey;
            IsIndexed = indexed;
            IsMapped = mapped;
            IsFragment = fragment;
        }

        /// <summary>
        ///     Returns the property name.
        /// </summary>
        /// <returns>
        ///     property name
        /// </returns>
        public string PropertyName { get; }

        /// <summary>
        ///     Returns the property underlying type.
        ///     <para>
        ///         Note that a null values is possible as null values can be selected.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     underlying property type
        /// </returns>
        public Type PropertyType => PropertyTypeRaw.TypeNormalized();

        /// <summary>
        ///     Returns the actual property underlying type.
        ///     <para>
        ///         Note that a null values is possible as null values can be selected.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     underlying property type
        /// </returns>
        public Type PropertyTypeRaw { get; }

        /// <summary>
        ///     Gets property component type, if applicable.
        /// </summary>
        /// <value>The type of the property component.</value>
        public Type PropertyComponentType {
            get {
                var propertyType = PropertyTypeRaw;
                if (propertyType == typeof(string)) {
                    return typeof(char);
                }

                if (propertyType.IsArray) {
                    return propertyType.GetElementType();
                }

                if (propertyType.IsGenericDictionary()) {
                    return propertyType.GetDictionaryValueType();
                }

                if (propertyType.IsGenericList()) {
                    return propertyType.GetCollectionItemType();
                }

                if (propertyType.IsGenericCollection()) {
                    return propertyType.GetCollectionItemType();
                }

                if (propertyType.IsGenericEnumerable()) {
                    return propertyType.GetEnumerableItemType();
                }

                return null;
            }
        }

        /// <summary>
        ///     Returns true to indicate that the property is an indexed property and requires
        ///     an index to access elements of the indexed property. Returns false to indicate
        ///     that the property is not an indexed property or does not require an index for
        ///     property value access.
        ///     <para>
        ///         For object events, a getter-method that takes a single integer
        ///         parameter is considered an indexed property that requires an index for access.
        ///     </para>
        ///     <para>
        ///         A getter-method that returns an array is considered an index property but does
        ///         not require an index for access.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     true to indicate that property value access requires an index value
        /// </returns>
        public bool IsRequiresIndex { get; }

        /// <summary>
        ///     Returns true to indicate that the property is a mapped property and requires a
        ///     map key to access elements of the mapped property. Returns false to indicate that
        ///     the property is not a mapped property or does not require a map key for property
        ///     value access.
        ///     <para>
        ///         For object events, a getter-method that takes a single string parameter
        ///         is considered a mapped property that requires a map key for access.
        ///     </para>
        ///     <para>
        ///         A getter-method that returns a Map is considered a mapped property but does not
        ///         require a map key for access.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     true to indicate that property value access requires an index value
        /// </returns>
        public bool IsRequiresMapKey { get; }

        /// <summary>
        ///     Returns true for indexed properties, returns false for all other property
        ///     styles.
        ///     <para>
        ///         An indexed property is a property returning an array value or a getter-method
        ///         taking a single integer parameter.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     indicator whether this property is an index property
        /// </returns>
        public bool IsIndexed { get; }

        /// <summary>
        ///     Returns true for mapped properties, returns false for all other property styles.
        ///     <para>
        ///         A mapped property is a property returning a Map value or a getter-method taking
        ///         a single string (key) parameter.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     indicator whether this property is a mapped property
        /// </returns>
        public bool IsMapped { get; }

        /// <summary>
        ///     Returns true to indicate that the property value can itself be represented as an
        ///     <seealso cref="EventBean" /> and that the property type can be represented as an
        ///     <seealso cref="EventType" />.
        /// </summary>
        /// <returns>
        ///     indicator whether property is itself a complex data structure representable as a
        ///     nested <seealso cref="EventType" />
        /// </returns>
        public bool IsFragment { get; }

        /// <summary>
        ///     Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        ///     A hash code for the current <see cref="T:System.Object" />.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked {
                var result = PropertyName != null ? PropertyName.GetHashCode() : 0;
                result = (result * 397) ^ (PropertyType != null ? PropertyType.GetHashCode() : 0);
                result = (result * 397) ^ IsRequiresIndex.GetHashCode();
                result = (result * 397) ^ IsRequiresMapKey.GetHashCode();
                result = (result * 397) ^ IsIndexed.GetHashCode();
                result = (result * 397) ^ IsMapped.GetHashCode();
                result = (result * 397) ^ IsFragment.GetHashCode();
                return result;
            }
        }

        /// <summary>
        ///     Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object" /> to compare with the current <see cref="T:System.Object" />.</param>
        /// <returns>
        ///     true if the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">
        ///     The <paramref name="obj" /> parameter is null.
        /// </exception>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != typeof(EventPropertyDescriptor)) {
                return false;
            }

            var that = (EventPropertyDescriptor) obj;

            if (IsFragment != that.IsFragment) {
                return false;
            }

            if (IsIndexed != that.IsIndexed) {
                return false;
            }

            if (IsMapped != that.IsMapped) {
                return false;
            }

            if (IsRequiresIndex != that.IsRequiresIndex) {
                return false;
            }

            if (IsRequiresMapKey != that.IsRequiresMapKey) {
                return false;
            }

            if (!Equals(PropertyName, that.PropertyName)) {
                return false;
            }

            return PropertyType != null ? PropertyType == that.PropertyType : that.PropertyType == null;
        }

        /// <summary>
        ///     Returns a <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
        /// </returns>
        public override string ToString()
        {
            return
                $"Name={PropertyName}; PropertyType={PropertyType}; IsRequiresIndex={IsRequiresIndex}; IsRequiresMapkey={IsRequiresMapKey}; IsIndexed={IsIndexed}; IsMapped={IsMapped}; IsFragment={IsFragment}";
        }
    }
}