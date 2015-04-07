///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client.dataflow
{
    /// <summary>
    /// Context for use with <seealso cref="EPDataFlowOperatorParameterProvider" /> describes the operator and parameters to provide.
    /// </summary>
    public class EPDataFlowOperatorParameterProviderContext
    {
        /// <summary>Ctor. </summary>
        /// <param name="operatorName">operator name</param>
        /// <param name="parameterName">parameter name</param>
        /// <param name="operatorInstance">operator instance</param>
        /// <param name="operatorNum">operator number</param>
        /// <param name="providedValue">value if any was provided as part of the declaration</param>
        /// <param name="dataFlowName">data flow name</param>
        public EPDataFlowOperatorParameterProviderContext(String operatorName,
                                                          String parameterName,
                                                          Object operatorInstance,
                                                          int operatorNum,
                                                          Object providedValue,
                                                          String dataFlowName)
        {
            OperatorName = operatorName;
            ParameterName = parameterName;
            OperatorInstance = operatorInstance;
            OperatorNum = operatorNum;
            ProvidedValue = providedValue;
            DataFlowName = dataFlowName;
        }

        /// <summary>Returns the operator name. </summary>
        /// <value>operator name</value>
        public string OperatorName { get; private set; }

        /// <summary>Returns the parameter name. </summary>
        /// <value>parameter name</value>
        public string ParameterName { get; private set; }

        /// <summary>Returns the operator instance. </summary>
        /// <value>operator instance</value>
        public object OperatorInstance { get; private set; }

        /// <summary>Returns the operator number </summary>
        /// <value>operator num</value>
        public int OperatorNum { get; private set; }

        /// <summary>Returns the parameters declared value, if any </summary>
        /// <value>value</value>
        public object ProvidedValue { get; private set; }

        /// <summary>Returns the data flow name. </summary>
        /// <value>data flow name</value>
        public string DataFlowName { get; private set; }
    }
}