///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;

namespace com.espertech.esper.dataflow.core
{
    public class DataFlowServiceEntry
    {
        public DataFlowServiceEntry(DataFlowStmtDesc dataFlowDesc, EPStatementState state)
        {
            DataFlowDesc = dataFlowDesc;
            State = state;
        }

        public DataFlowStmtDesc DataFlowDesc { get; private set; }

        public EPStatementState State { get; set; }
    }
}