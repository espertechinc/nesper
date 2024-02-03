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
    ///     For use with lambda parameters, the descriptor identifies a specific lambda parameter.
    ///     <para />
    ///     For instance <code>mymethod(1, (v, i) =&gt; 2)</code> the parameter number is 1 amd the lambda parameter number
    ///     is zero for "v" and one for "i".
    /// </summary>
    public class EnumMethodLambdaParameterDescriptor
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="parameterNumber">overall parameter number</param>
        /// <param name="lambdaParameterNumber">lambda parameter number</param>
        public EnumMethodLambdaParameterDescriptor(
            int parameterNumber,
            int lambdaParameterNumber)
        {
            ParameterNumber = parameterNumber;
            LambdaParameterNumber = lambdaParameterNumber;
        }

        /// <summary>
        ///     Returns the overall parameter number.
        /// </summary>
        /// <value>number</value>
        public int ParameterNumber { get; }

        /// <summary>
        ///     Returns the lambda parameter number.
        /// </summary>
        /// <value>number</value>
        public int LambdaParameterNumber { get; }
    }
} // end of namespace