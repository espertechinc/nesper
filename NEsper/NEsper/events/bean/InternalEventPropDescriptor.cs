///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.events.property;
using com.espertech.esper.util;

namespace com.espertech.esper.events.bean
{
    /// <summary>
    /// Encapsulates the event property information available after introspecting an
    /// event's class members for getter methods.
    /// </summary>
    public class InternalEventPropDescriptor
    {
        private readonly String propertyName;
        private readonly MethodInfo readMethod;
        private readonly FieldInfo accessorField;
        private readonly EventPropertyType? propertyType;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="propertyName">name of property, from getter method</param>
        /// <param name="readMethod">read method to get value</param>
        /// <param name="propertyType">type of property</param>
        public InternalEventPropDescriptor(String propertyName, MethodInfo readMethod, EventPropertyType? propertyType)
        {
            this.propertyName = propertyName;
            this.readMethod = readMethod;
            this.propertyType = propertyType;
        }
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="propertyName">name of property, from getter method</param>
        /// <param name="accessorField">field to get value from</param>
        /// <param name="propertyType">type of property</param>
        public InternalEventPropDescriptor(String propertyName, FieldInfo accessorField, EventPropertyType? propertyType)
        {
            this.propertyName = propertyName;
            this.accessorField = accessorField;
            this.propertyType = propertyType;
        }

        /// <summary>
        /// Gets the declaring type for the property.
        /// </summary>
        /// <value>The type of the declaring.</value>
        public Type DeclaringType
        {
            get
            {
                if (readMethod != null)
                    return readMethod.DeclaringType;
                if (accessorField != null)
                    return accessorField.DeclaringType;
                return null;
            }
        }

        /// <summary>
        /// Return the property name, for mapped and indexed properties this is just the
        /// property name without parantheses or brackets.
        /// </summary>
        /// <returns>
        /// property name
        /// </returns>
        public string PropertyName
        {
            get { return propertyName; }
        }

        /// <summary>
        /// Returns an enum indicating the type of property: simple, mapped, indexed.
        /// </summary>
        /// <returns>
        /// enum with property type info
        /// </returns>
        public EventPropertyType? PropertyType
        {
            get { return propertyType; }
        }

        /// <summary>
        /// Returns the read method. Can return null if the property is backed by a field..
        /// </summary>
        /// <returns>
        /// read method of null if field property
        /// </returns>
        public MethodInfo ReadMethod
        {
            get { return readMethod; }
        }

        /// <summary>
        /// Returns the accessor field. Can return null if the property is backed by a
        /// method.
        /// </summary>
        /// <returns>
        /// accessor field of null if method property
        /// </returns>
        public FieldInfo AccessorField
        {
            get { return accessorField; }
        }

        /// <summary>
        /// Returns the type of the underlying method or field of the event property.
        /// </summary>
        /// <returns>
        /// return type
        /// </returns>
        public Type ReturnType
        {
            get
            {
                if (readMethod != null)
                    return readMethod.ReturnType;
                return accessorField.FieldType;
            }
        }

        /// <summary>
        /// Returns the type of the underlying method or field of the event property.
        /// </summary>
        /// <returns>
        /// return type
        /// </returns>
        public GenericPropertyDesc GetReturnTypeGeneric()
        {
            if (readMethod != null)
            {
                return new GenericPropertyDesc(
                    readMethod.ReturnType, 
                    TypeHelper.GetGenericReturnType(readMethod, true));
            }

            return new GenericPropertyDesc(
                accessorField.FieldType, 
                TypeHelper.GetGenericFieldType(accessorField, true));
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format(
                "PropertyName: {0}, ReadMethod: {1}, AccessorField: {2}, EventPropertyType: {3}",
                propertyName,
                readMethod, 
                accessorField, 
                propertyType);
        }

        public bool Equals(InternalEventPropDescriptor obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return 
                Equals(obj.propertyName, propertyName) && 
                Equals(obj.readMethod, readMethod) && 
                Equals(obj.accessorField, accessorField) && 
                Equals(obj.propertyType, propertyType);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">
        /// The <paramref name="obj"/> parameter is null.
        /// </exception>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (InternalEventPropDescriptor)) return false;
            return Equals((InternalEventPropDescriptor) obj);
        }
    
        public override int GetHashCode()
        {
            unchecked {
                int result = (propertyName != null ? propertyName.GetHashCode() : 0);
                result = (result*397) ^ (readMethod != null ? readMethod.GetHashCode() : 0);
                result = (result*397) ^ (accessorField != null ? accessorField.GetHashCode() : 0);
                result = (result*397) ^ propertyType.GetHashCode();
                return result;
            }
        }
    }
}
