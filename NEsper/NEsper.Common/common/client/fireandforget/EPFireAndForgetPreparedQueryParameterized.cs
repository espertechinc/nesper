///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.fireandforget
{
    /// <summary>
    /// Parameter holder for parameterized on-demand queries that are prepared with substitution parameters and that
    /// can be executed efficiently multiple times with different actual values for parameters.
    /// <para />A pre-compiled query can only be executed when actual values for all
    /// substitution parameters are set.
    /// </summary>
    public interface EPFireAndForgetPreparedQueryParameterized
    {
        /// <summary>
        /// Sets the value of the designated parameter using the given object.
        /// </summary>
        /// <param name="parameterIndex">the first parameter is 1, the second is 2, ...</param>
        /// <param name="value">the object containing the input parameter value</param>
        /// <throws>EPException if the substitution parameter could not be located</throws>
        void SetObject(
            int parameterIndex,
            object value);

        /// <summary>
        /// Sets the value of the designated parameter using the given object.
        /// </summary>
        /// <param name="parameterName">the name of the parameter</param>
        /// <param name="value">the object containing the input parameter value</param>
        /// <throws>EPException if the substitution parameter could not be set</throws>
        void SetObject(
            string parameterName,
            object value);
    }
} // end of namespace