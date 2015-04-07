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
    /// <summary>Data flow descriptor. </summary>
    public class EPDataFlowDescriptor
    {
        /// <summary>Ctor. </summary>
        /// <param name="dataFlowName">data flow name</param>
        /// <param name="statementState">statement state</param>
        /// <param name="statementName">statement name</param>
        public EPDataFlowDescriptor(String dataFlowName, EPStatementState statementState, String statementName)
        {
            DataFlowName = dataFlowName;
            StatementState = statementState;
            StatementName = statementName;
        }

        /// <summary>Returns the data flow name. </summary>
        /// <value>name</value>
        public string DataFlowName { get; private set; }

        /// <summary>Returns the statement state. </summary>
        /// <value>statement state</value>
        public EPStatementState StatementState { get; private set; }

        /// <summary>Returns the statement name. </summary>
        /// <value>statement name.</value>
        public string StatementName { get; private set; }
    }
}