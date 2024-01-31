///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.dataflow.interfaces;

namespace com.espertech.esper.common.client.dataflow.core
{
    /// <summary>
    ///     Context for use with <seealso cref="EPDataFlowOperatorParameterProvider" /> describes the operator and parameters
    ///     to provide.
    /// </summary>
    public class EPDataFlowOperatorParameterProviderContext
    {
        private readonly DataFlowOperatorFactory factory;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="initializeContext">context</param>
        /// <param name="parameterName">parameter name</param>
        public EPDataFlowOperatorParameterProviderContext(
            DataFlowOpInitializeContext initializeContext,
            string parameterName)
        {
            OperatorName = initializeContext.OperatorName;
            ParameterName = parameterName;
            factory = initializeContext.DataFlowOperatorFactory;
            OperatorNum = initializeContext.OperatorNumber;
            DataFlowName = initializeContext.DataFlowName;
        }

        /// <summary>
        ///     Returns the operator name.
        /// </summary>
        /// <returns>operator name</returns>
        public string OperatorName { get; }

        /// <summary>
        ///     Returns the parameter name.
        /// </summary>
        /// <returns>parameter name</returns>
        public string ParameterName { get; }

        /// <summary>
        ///     Returns the operator instance.
        /// </summary>
        /// <returns>operator instance</returns>
        public object Factory => factory;

        /// <summary>
        ///     Returns the operator number
        /// </summary>
        /// <returns>operator num</returns>
        public int OperatorNum { get; }

        /// <summary>
        ///     Returns the data flow name.
        /// </summary>
        /// <returns>data flow name</returns>
        public string DataFlowName { get; }
    }
} // end of namespace