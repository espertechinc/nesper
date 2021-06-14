///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.compat
{
    public class TypePair
    {
        private readonly Type typeA;
        private readonly Type typeB;

        /// <summary>
        /// Gets the type A.
        /// </summary>
        /// <value>The type A.</value>
        public Type TypeA => typeA;

        /// <summary>
        /// Gets the type B.
        /// </summary>
        /// <value>The type B.</value>
        public Type TypeB => typeB;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypePair"/> class.
        /// </summary>
        /// <param name="typeA">The type A.</param>
        /// <param name="typeB">The type B.</param>
        public TypePair(Type typeA, Type typeB)
        {
            this.typeA = typeA;
            this.typeB = typeB;
        }

        /// <summary>
        /// Performs the underlying equality comparison.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        public bool Equals(TypePair obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj.typeA, typeA) && Equals(obj.typeB, typeB);
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
            if (obj.GetType() != typeof(TypePair)) return false;
            return Equals((TypePair)obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((typeA != null ? typeA.GetHashCode() : 0) * 397) ^ (typeB != null ? typeB.GetHashCode() : 0);
            }
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format("TypeA: {0}, TypeB: {1}", typeA, typeB);
        }
    }
}
