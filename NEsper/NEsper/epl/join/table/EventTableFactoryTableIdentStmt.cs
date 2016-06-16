///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.core.service;

namespace com.espertech.esper.epl.join.table
{
    public class EventTableFactoryTableIdentStmt : EventTableFactoryTableIdent
    {
        public EventTableFactoryTableIdentStmt(StatementContext statementContext)
        {
            StatementContext = statementContext;
        }

        public StatementContext StatementContext { get; private set; }
    }
} // end of namespace