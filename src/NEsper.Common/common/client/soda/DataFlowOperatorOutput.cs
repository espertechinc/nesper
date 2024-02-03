///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>Represents an output port of an operator. </summary>
    public class DataFlowOperatorOutput
    {
        /// <summary>Ctor. </summary>
        public DataFlowOperatorOutput()
        {
        }

        /// <summary>Ctor. </summary>
        /// <param name="streamName">output stream name</param>
        /// <param name="typeInfo">type information</param>
        public DataFlowOperatorOutput(
            string streamName,
            IList<DataFlowOperatorOutputType> typeInfo)
        {
            StreamName = streamName;
            TypeInfo = typeInfo;
        }

        /// <summary>Returns the output stream name. </summary>
        /// <value>stream name.</value>
        public string StreamName { get; set; }

        /// <summary>Returns output port type information </summary>
        /// <value>type INFO</value>
        public IList<DataFlowOperatorOutputType> TypeInfo { get; set; }
    }
}