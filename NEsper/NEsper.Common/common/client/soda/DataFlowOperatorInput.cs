///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>Represents an input port of an operator. </summary>
    [Serializable]
    public class DataFlowOperatorInput
    {
        /// <summary>Ctor. </summary>
        public DataFlowOperatorInput()
        {
        }

        /// <summary>Ctor. </summary>
        /// <param name="inputStreamNames">names of input streams for the same port</param>
        /// <param name="optionalAsName">optional alias</param>
        public DataFlowOperatorInput(
            IList<String> inputStreamNames,
            String optionalAsName)
        {
            InputStreamNames = inputStreamNames;
            OptionalAsName = optionalAsName;
        }

        /// <summary>Returns the input stream names. </summary>
        /// <value>input stream names</value>
        public IList<string> InputStreamNames { get; set; }

        /// <summary>Returns the alias name. </summary>
        /// <value>alias</value>
        public string OptionalAsName { get; set; }
    }
}