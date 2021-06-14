///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;

namespace com.espertech.esper.common.@internal.epl.dataflow.interfaces
{
    public class DataFlowOpFactoryInitializeContext
    {
        public DataFlowOpFactoryInitializeContext(
            string dataFlowName,
            int operatorNumber,
            StatementContext statementContext)
        {
            DataFlowName = dataFlowName;
            OperatorNumber = operatorNumber;
            StatementContext = statementContext;
        }

        public StatementContext StatementContext { get; }

        public int OperatorNumber { get; }

        public string DataFlowName { get; }
    }
} // end of namespace