///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

namespace com.espertech.esper.common.@internal.@event.core
{
    /// <summary>Descriptor for writable properties. </summary>
    public class WriteablePropertyDescriptor
    {
        /// <summary>Ctor. </summary>
        /// <param name="propertyName">name of property</param>
        /// <param name="type">type</param>
        /// <param name="writeMember">optional write methods</param>
        public WriteablePropertyDescriptor(
            string propertyName,
            Type type,
            MemberInfo writeMember)
        {
            PropertyName = propertyName;
            PropertyType = type;
            WriteMember = writeMember;
        }

        /// <summary>Returns property name. </summary>
        /// <value>property name</value>
        public string PropertyName { get; }

        /// <summary>Returns property type. </summary>
        /// <value>property type</value>
        public Type PropertyType { get; }

        /// <summary>Returns write member. </summary>
        /// <value>write member</value>
        public MemberInfo WriteMember { get; }

        public bool Equals(WriteablePropertyDescriptor other)
        {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(other.PropertyName, PropertyName);
        }

        /// <summary>
        ///     Determines whether the specified <see cref="T:System.Object" /> is equal to the current
        ///     <see cref="T:System.Object" />.
        /// </summary>
        /// <returns>
        ///     true if the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />;
        ///     otherwise, false.
        /// </returns>
        /// <param name="obj">
        ///     The <see cref="T:System.Object" /> to compare with the current <see cref="T:System.Object" />.
        /// </param>
        /// <exception cref="T:System.NullReferenceException">
        ///     The <paramref name="obj" /> parameter is null.
        /// </exception>
        /// <filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != typeof(WriteablePropertyDescriptor)) {
                return false;
            }

            return Equals((WriteablePropertyDescriptor) obj);
        }

        /// <summary>
        ///     Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        ///     A hash code for the current <see cref="T:System.Object" />.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            return PropertyName != null ? PropertyName.GetHashCode() : 0;
        }
    }
}