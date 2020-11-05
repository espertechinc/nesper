///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.dataflow.core
{
    /// <summary>
    /// Thrown to indicate a data flow execution exception.
    /// </summary>
    public class EPDataFlowExecutionException : EPException
    {
        public string DataFlowName { get; private set; }

        /// <summary>Ctor. </summary>
        /// <param name="message">error message</param>
        /// <param name="dataFlowName">data flow name</param>
        public EPDataFlowExecutionException(
            string message,
            string dataFlowName)
            : base(message)
        {
            DataFlowName = dataFlowName;
        }

        /// <summary>Ctor. </summary>
        /// <param name="message">error message</param>
        /// <param name="cause">cuase</param>
        /// <param name="dataFlowName">data flow name</param>
        public EPDataFlowExecutionException(
            string message,
            Exception cause,
            string dataFlowName)
            : base(message, cause)
        {
            DataFlowName = dataFlowName;
        }

        /// <summary>Ctor. </summary>
        /// <param name="cause">cuase</param>
        /// <param name="dataFlowName">data flow name</param>
        public EPDataFlowExecutionException(
            Exception cause,
            string dataFlowName)
            : base(cause)
        {
            DataFlowName = dataFlowName;
        }
    }
}