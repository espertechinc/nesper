///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.hook.enummethod
{
    /// <summary>
    ///     A lambda parameter that assumes an index value.
    /// </summary>
    public class EnumMethodLambdaParameterTypeIndex : EnumMethodLambdaParameterType
    {
        /// <summary>
        ///     Instance.
        /// </summary>
        public static readonly EnumMethodLambdaParameterTypeIndex INSTANCE = new EnumMethodLambdaParameterTypeIndex();

        private EnumMethodLambdaParameterTypeIndex()
        {
        }
    }
} // end of namespace