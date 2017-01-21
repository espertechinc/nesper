///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.events.xml
{
    /// <summary>
    /// Pair of namespace and name.
    /// </summary>
    public class NamespaceNamePair
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="namespace">namespace</param>
        /// <param name="name">name</param>
        public NamespaceNamePair(String @namespace, String name)
        {
            Namespace = @namespace;
            Name = name;
        }

        /// <summary>
        /// Returns the name.
        /// </summary>
        /// <returns>
        /// name part
        /// </returns>
        public string Name { get; private set; }

        /// <summary>
        /// Returns the namespace.
        /// </summary>
        /// <returns>
        /// namespace part
        /// </returns>
        public string Namespace { get; private set; }

        public override String ToString()
        {
            return Namespace + " " + Name;
        }
    
        public bool Equals(NamespaceNamePair obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj.Namespace, Namespace) && Equals(obj.Name, Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (NamespaceNamePair)) return false;
            return Equals((NamespaceNamePair) obj);
        }

        public override int GetHashCode()
        {
            unchecked {
                return ((Namespace != null ? Namespace.GetHashCode() : 0)*397) ^ (Name != null ? Name.GetHashCode() : 0);
            }
        }
    }
}
