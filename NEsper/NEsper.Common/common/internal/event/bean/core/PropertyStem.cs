///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.property;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.@event.bean.core
{
    /// <summary>
    ///     Encapsulates the event property information available after introspecting an event's class members
    ///     for getter methods.
    /// </summary>
    public class PropertyStem
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="propertyName">name of property, from getter method</param>
        /// <param name="readMethod">read method to get value</param>
        /// <param name="propertyType">type of property</param>
        public PropertyStem(
            string propertyName,
            MethodInfo readMethod,
            PropertyType? propertyType)
        {
            PropertyName = propertyName;
            ReadMethod = readMethod;
            PropertyType = propertyType;
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="propertyName">name of property, from getter method</param>
        /// <param name="accessorField">field to get value from</param>
        /// <param name="propertyType">type of property</param>
        public PropertyStem(
            string propertyName,
            FieldInfo accessorField,
            PropertyType propertyType)
        {
            PropertyName = propertyName;
            AccessorField = accessorField;
            PropertyType = propertyType;
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="propertyName">name of property, from getter method</param>
        /// <param name="accessorProp">property to get value from</param>
        /// <param name="propertyType">type of property</param>
        public PropertyStem(
            string propertyName,
            PropertyInfo accessorProp,
            PropertyType propertyType)
        {
            PropertyName = propertyName;
            AccessorProp = accessorProp;
            PropertyType = propertyType;
        }

        /// <summary>
        ///     Return the property name, for mapped and indexed properties this is just the property name
        ///     without parentheses or brackets.
        /// </summary>
        /// <returns>property name</returns>
        public string PropertyName { get; }

        /// <summary>
        ///     Returns an enum indicating the type of property: simple, mapped, indexed.
        /// </summary>
        /// <returns>enum with property type info</returns>
        public PropertyType? PropertyType { get; }

        /// <summary>
        ///     Returns the read method. Can return null if the property is backed by a field..
        /// </summary>
        /// <returns>read method of null if field property</returns>
        public MethodInfo ReadMethod { get; }

        /// <summary>
        ///     Returns the accessor field. Can return null if the property is backed by a method.
        /// </summary>
        /// <returns>accessor field of null if method property</returns>
        public FieldInfo AccessorField { get; }

        /// <summary>
        ///     Returns the accessor property. Can return null if the property exists.
        /// </summary>
        /// <returns>accessor property of null if property exists</returns>
        public PropertyInfo AccessorProp { get; }

        /// <summary>
        ///     Returns the type of the underlying method or field of the event property.
        /// </summary>
        /// <value>return type</value>
        public Type ReturnType {
            get {
                if (ReadMethod != null) {
                    return ReadMethod.ReturnType;
                }
                else if (AccessorProp != null) {
                    return AccessorProp.PropertyType;
                }
                else {
                    return AccessorField.FieldType;
                }
            }
        }

        /// <summary>
        /// Gets the declaring type for the property.
        /// </summary>
        /// <value>The type of the declaring.</value>
        public Type DeclaringType {
            get {
                if (ReadMethod != null) {
                    return ReadMethod.DeclaringType;
                }
                else if (AccessorProp != null) {
                    return AccessorProp.DeclaringType;
                }
                else if (AccessorField != null) {
                    return AccessorField.DeclaringType;
                }

                return null;
            }
        }

        /// <summary>
        ///     Returns the type of the underlying method or field of the event property.
        /// </summary>
        /// <value>return type</value>
        public GenericPropertyDesc ReturnTypeGeneric {
            get {
                if (ReadMethod != null) {
                    return new GenericPropertyDesc(
                        ReadMethod.ReturnType,
                        TypeHelper.GetGenericReturnType(ReadMethod, true));
                }
                else if (AccessorProp != null) {
                    return new GenericPropertyDesc(
                        AccessorProp.PropertyType,
                        TypeHelper.GetGenericPropertyType(AccessorProp, true));
                }
                else {
                    return new GenericPropertyDesc(
                        AccessorField.FieldType,
                        TypeHelper.GetGenericFieldType(AccessorField, true));
                }
            }
        }

        protected bool Equals(PropertyStem other)
        {
            return string.Equals(PropertyName, other.PropertyName)
                   && PropertyType == other.PropertyType 
                   && Equals(ReadMethod, other.ReadMethod) 
                   && Equals(AccessorField, other.AccessorField) 
                   && Equals(AccessorProp, other.AccessorProp);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != this.GetType()) {
                return false;
            }

            return Equals((PropertyStem) obj);
        }

        public override int GetHashCode()
        {
            unchecked {
                var hashCode = (PropertyName != null ? PropertyName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ PropertyType.GetHashCode();
                hashCode = (hashCode * 397) ^ (ReadMethod != null ? ReadMethod.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (AccessorField != null ? AccessorField.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (AccessorProp != null ? AccessorProp.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
} // end of namespace