///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.events.property
{
    /// <summary>
    /// Descriptor for a type and its generic type, if any.
    /// </summary>
    public class GenericPropertyDesc
    {
        static GenericPropertyDesc()
        {
            ObjectGeneric = new GenericPropertyDesc(typeof(Object));
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="type">the type</param>
        /// <param name="generic">its generic type parameter, if any</param>
        public GenericPropertyDesc(Type type, Type generic)
        {
            PropertyType = type;
            GenericType = generic;
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="type">the type</param>
        public GenericPropertyDesc(Type type)
        {
            PropertyType = type;
            GenericType = null;
        }

        /// <summary>
        /// Returns the type.
        /// </summary>
        /// <returns>
        /// type
        /// </returns>
        public Type PropertyType { get; private set; }

        /// <summary>
        /// Returns the generic parameter, or null if none.
        /// </summary>
        /// <returns>
        /// generic parameter
        /// </returns>
        public Type GenericType { get; private set; }

        /// <summary>
        /// typeof(Object) type.
        /// </summary>
        /// <returns>
        /// type descriptor
        /// </returns>
        public static GenericPropertyDesc ObjectGeneric { get; private set; }
    }
}
