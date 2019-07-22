///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.@event.xml
{
    /// <summary>
    ///     Pair of namespace and name.
    /// </summary>
    public class NamespaceNamePair
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="namespace">namespace</param>
        /// <param name="name">name</param>
        public NamespaceNamePair(
            string @namespace,
            string name)
        {
            Namespace = @namespace;
            Name = name;
        }

        /// <summary>
        ///     Returns the name.
        /// </summary>
        /// <returns>name part</returns>
        public string Name { get; }

        /// <summary>
        ///     Returns the namespace.
        /// </summary>
        /// <returns>namespace part</returns>
        public string Namespace { get; }

        protected bool Equals(NamespaceNamePair other)
        {
            return string.Equals(Name, other.Name) && string.Equals(Namespace, other.Namespace);
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

            return Equals((NamespaceNamePair) obj);
        }

        public override int GetHashCode()
        {
            unchecked {
                return ((Name != null ? Name.GetHashCode() : 0) * 397) ^
                       (Namespace != null ? Namespace.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return $"{nameof(Name)}: {Name}, {nameof(Namespace)}: {Namespace}";
        }
    }
} // end of namespace