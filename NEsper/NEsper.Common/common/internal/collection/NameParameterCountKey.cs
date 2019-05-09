///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.collection
{
    /// <summary>
    ///     A hash key that include a name and a count, wherein the combination of name and count defines the uniqueness.
    /// </summary>
    public class NameParameterCountKey
    {
        public NameParameterCountKey(
            string name,
            int count)
        {
            Name = name;
            Count = count;
        }

        public string Name { get; }

        public int Count { get; }

        public bool Equals(NameParameterCountKey other)
        {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return other.Count == Count && Equals(other.Name, Name);
        }

        /// <summary>
        ///     Determines whether the specified <see cref="T:System.Object" /> is equal to the current
        ///     <see cref="T:System.Object" />.
        /// </summary>
        /// <returns>
        ///     true if the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />;
        ///     otherwise, false.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object" /> to compare with the current <see cref="T:System.Object" />. </param>
        /// <filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != typeof(NameParameterCountKey)) {
                return false;
            }

            return Equals((NameParameterCountKey) obj);
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
            unchecked {
                return (Count * 397) ^ (Name != null ? Name.GetHashCode() : 0);
            }
        }
    }
}