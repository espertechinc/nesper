///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.hook.enummethod
{
    /// <summary>
    ///     A lambda parameter that assumes a value of the given type and that originates from the state object
    ///     by calling the provided getter-method.
    /// </summary>
    public class EnumMethodLambdaParameterTypeStateGetter : EnumMethodLambdaParameterType
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="type">lambda parameter-assumed value type</param>
        /// <param name="propertyName">getter method name</param>
        public EnumMethodLambdaParameterTypeStateGetter(
            Type type,
            string propertyName)
        {
            Type = type;
            PropertyName = propertyName;
        }

        /// <summary>
        ///     Returns the type of the value the lambda parameter assumes
        /// </summary>
        /// <returns>types</returns>
        public Type Type { get; }

        /// <summary>
        ///     Returns the name of the property that the runtime invokes on the state object to obtain
        ///     the value of the lambda parameter
        /// </summary>
        /// <value>getter method name</value>
        public string PropertyName { get; }
    }
} // end of namespace