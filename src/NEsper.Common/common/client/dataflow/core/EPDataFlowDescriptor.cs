///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.dataflow.core
{
    /// <summary>
    /// Data flow descriptor.
    /// </summary>
    public class EPDataFlowDescriptor
    {
        private readonly string dataFlowName;
        private readonly string statementName;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="dataFlowName">data flow name</param>
        /// <param name="statementName">statement name</param>
        public EPDataFlowDescriptor(
            string dataFlowName,
            string statementName)
        {
            this.dataFlowName = dataFlowName;
            this.statementName = statementName;
        }

        /// <summary>
        /// Returns the data flow name.
        /// </summary>
        /// <returns>name</returns>
        public string DataFlowName {
            get => dataFlowName;
        }

        /// <summary>
        /// Returns the statement name.
        /// </summary>
        /// <returns>statement name.</returns>
        public string StatementName {
            get => statementName;
        }
    }
} // end of namespace