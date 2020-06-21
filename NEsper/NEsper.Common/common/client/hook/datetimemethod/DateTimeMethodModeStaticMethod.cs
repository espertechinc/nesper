///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.hook.datetimemethod
{
    /// <summary>
    ///     Provides information about the public static method that implements the logic for the date-time method.
    /// </summary>
    public class DateTimeMethodModeStaticMethod : DateTimeMethodMode
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="clazz">class</param>
        /// <param name="methodName">method</param>
        public DateTimeMethodModeStaticMethod(
            Type clazz,
            string methodName)
        {
            Clazz = clazz;
            MethodName = methodName;
        }

        /// <summary>
        ///     Returns the class
        /// </summary>
        /// <value>class</value>
        public Type Clazz { get; }

        /// <summary>
        ///     Returns the method
        /// </summary>
        /// <value>method</value>
        public string MethodName { get; }
    }
} // end of namespace