///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.hook.enummethod
{
    /// <summary>
    ///     A lambda parameter that assumes the value itself.
    /// </summary>
    public class EnumMethodLambdaParameterTypeValue : EnumMethodLambdaParameterType
    {
        /// <summary>
        ///     Instance.
        /// </summary>
        public static readonly EnumMethodLambdaParameterTypeValue INSTANCE = new EnumMethodLambdaParameterTypeValue();

        private EnumMethodLambdaParameterTypeValue()
        {
        }
    }
} // end of namespace