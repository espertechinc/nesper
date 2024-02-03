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
    /// <summary>Indicates cancellation of a data flow instance. </summary>
    public class EPDataFlowCancellationException : EPException
    {
        public string DataFlowName { get; private set; }

        /// <summary>Ctor. </summary>
        /// <param name="message">cancel message</param>
        /// <param name="dataFlowName">data flow name</param>
        public EPDataFlowCancellationException(
            string message,
            string dataFlowName)
            : base(message)
        {
            DataFlowName = dataFlowName;
        }

        /// <summary>Ctor. </summary>
        /// <param name="message">cancel message</param>
        /// <param name="cause">cause</param>
        /// <param name="dataFlowName">data flow name</param>
        public EPDataFlowCancellationException(
            string message,
            Exception cause,
            string dataFlowName)
            : base(message, cause)
        {
            DataFlowName = dataFlowName;
        }

        /// <summary>Ctor. </summary>
        /// <param name="cause">cause</param>
        /// <param name="dataFlowName">data flow name</param>
        public EPDataFlowCancellationException(
            Exception cause,
            string dataFlowName)
            : base(cause)
        {
            DataFlowName = dataFlowName;
        }
    }
}