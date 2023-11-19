///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.Serialization;

namespace com.espertech.esper.common.client.dataflow.core
{
    /// <summary>Indicates an exception instantiating a data flow. </summary>
    public class EPDataFlowInstantiationException : EPException
    {
        /// <summary>Ctor. </summary>
        /// <param name="message">the message</param>
        public EPDataFlowInstantiationException(string message)
            : base(message)
        {
        }

        /// <summary>Ctor. </summary>
        /// <param name="message">the message</param>
        /// <param name="cause">the inner exception</param>
        public EPDataFlowInstantiationException(
            string message,
            Exception cause)
            : base(message, cause)
        {
        }

        /// <summary>Ctor. </summary>
        /// <param name="cause">the inner exception</param>
        public EPDataFlowInstantiationException(Exception cause)
            : base(cause)
        {
        }

        protected EPDataFlowInstantiationException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}