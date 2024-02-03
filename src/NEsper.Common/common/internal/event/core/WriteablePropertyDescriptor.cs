///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

namespace com.espertech.esper.common.@internal.@event.core
{
    /// <summary>
    ///     Descriptor for writable properties.
    /// </summary>
    public class WriteablePropertyDescriptor
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="propertyName">name of property</param>
        /// <param name="type">type</param>
        /// <param name="writeMember">optional write methods</param>
        /// <param name="fragment">whether the property is itself an event or array of events</param>
        public WriteablePropertyDescriptor(
            string propertyName,
            Type type,
            MemberInfo writeMember,
            bool fragment)
        {
            PropertyName = propertyName;
            PropertyType = type;
            WriteMember = writeMember;
            IsFragment = fragment;
        }

        /// <summary>
        ///     Returns property name.
        /// </summary>
        /// <value>property name</value>
        public string PropertyName { get; }

        /// <summary>
        ///     Returns property type.
        /// </summary>
        /// <value>property type</value>
        public Type PropertyType { get; }

        /// <summary>
        ///     Returns write member.
        /// </summary>
        /// <value>write member</value>
        public MemberInfo WriteMember { get; }

        public bool IsFragment { get; }

        protected bool Equals(WriteablePropertyDescriptor other)
        {
            return PropertyName == other.PropertyName && Equals(PropertyType, other.PropertyType);
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

            return Equals((WriteablePropertyDescriptor)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(PropertyName, PropertyType);
        }
    }
} // end of namespace