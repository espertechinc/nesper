///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using XLR8.CGLib;

namespace com.espertech.esper.events.bean
{
    /// <summary>
    /// Provides method information for dynamic (unchecked) properties of each class for
    /// use in obtaining property values.
    /// </summary>
    public class DynamicPropertyDescriptor
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="clazz">the class to match when looking for a method</param>
        /// <param name="method">the fast method to call</param>
        /// <param name="hasParameters">true if the method takes parameters</param>
        public DynamicPropertyDescriptor(Type clazz, FastMethod method, bool hasParameters)
        {
            Clazz = clazz;
            Method = method;
            HasParameters = hasParameters;
        }

        /// <summary>
        /// Returns the class for the method.
        /// </summary>
        /// <returns>
        /// class to match on
        /// </returns>
        public Type Clazz { get; private set; }

        /// <summary>
        /// Returns the method to invoke.
        /// </summary>
        /// <returns>
        /// method to invoke
        /// </returns>
        public FastMethod Method { get; private set; }

        /// <summary>
        /// Returns true if the method takes parameters.
        /// </summary>
        /// <returns>
        /// indicator if parameters are required
        /// </returns>
        public bool HasParameters { get; private set; }
    }
}
