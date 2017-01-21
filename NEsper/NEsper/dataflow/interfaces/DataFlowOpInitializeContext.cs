///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.core.service;

namespace com.espertech.esper.dataflow.interfaces
{
    public class DataFlowOpInitializeContext
    {
        public DataFlowOpInitializeContext(StatementContext statementContext)
        {
            StatementContext = statementContext;
        }

        public StatementContext StatementContext { get; private set; }
    }
}
