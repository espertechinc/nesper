///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.dataflow.annotations
{
    /// <summary>
    /// Annotation for use with data flow operator forges to provide output type information
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class OutputTypeAttribute : Attribute
    {
        public String Name { get; set; }
        public Type Type { get; set; }
        public String TypeName { get; set; }
        public int Port { get; set; }

        public OutputTypeAttribute(string name)
        {
            Name = name;
        }

        public OutputTypeAttribute(
            string name,
            Type type)
        {
            Name = name;
            Type = type;
        }

        public OutputTypeAttribute()
        {
            Name = string.Empty;
            TypeName = string.Empty;
            Type = typeof(OutputTypeAttribute);
        }

        public override object TypeId {
            get { return this; }
        }

        public bool Equals(OutputTypeAttribute other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return base.Equals(other) && Equals(other.Name, Name) && Equals(other.Type, Type) && Equals(other.TypeName, TypeName);
        }

        /// <summary>
        /// Returns a value that indicates whether this instance is equal to a specified object.
        /// </summary>
        /// <returns>
        /// true if <paramref name="obj"/> equals the type and value of this instance; otherwise, false.
        /// </returns>
        /// <param name="obj">An <see cref="T:System.Object"/> to compare with this instance or null. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return Equals(obj as OutputTypeAttribute);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer hash code.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            unchecked {
                int result = base.GetHashCode();
                result = (result * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                result = (result * 397) ^ (Type != null ? Type.GetHashCode() : 0);
                result = (result * 397) ^ (TypeName != null ? TypeName.GetHashCode() : 0);
                return result;
            }
        }
    }
} // end of namespace