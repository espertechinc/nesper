///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.dataflow.core
{
    /// <summary>
    /// Context for use with <seealso cref="EPDataFlowExceptionHandler"/>
    /// </summary>
    public class EPDataFlowExceptionContext
    {
        /// <summary>Ctor. </summary>
        /// <param name="dataFlowName">data flow name</param>
        /// <param name="operatorName">operator name</param>
        /// <param name="operatorNumber">operator number</param>
        /// <param name="operatorPrettyPrint">pretty-print of operator</param>
        /// <param name="throwable">cause</param>
        public EPDataFlowExceptionContext(
            string dataFlowName,
            string operatorName,
            object operatorNumber,
            object operatorPrettyPrint,
            Exception throwable)
        {
            DataFlowName = dataFlowName;
            OperatorName = operatorName;
            OperatorNumber = operatorNumber;
            OperatorPrettyPrint = operatorPrettyPrint;
            Exception = throwable;
        }

        /// <summary>Returns the data flow name. </summary>
        /// <value>data flow name</value>
        public string DataFlowName { get; private set; }

        /// <summary>Returns the operator name. </summary>
        /// <value>operator name</value>
        public string OperatorName { get; private set; }

        /// <summary>Returns the cause. </summary>
        /// <value>cause</value>
        public Exception Exception { get; private set; }

        /// <summary>Returns the operator number. </summary>
        /// <value>operator num</value>
        public object OperatorNumber { get; private set; }

        /// <summary>Returns the pretty-print for the operator. </summary>
        /// <value>operator string</value>
        public object OperatorPrettyPrint { get; private set; }
    }
}