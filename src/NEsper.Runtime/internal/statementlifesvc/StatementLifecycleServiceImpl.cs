///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.runtime.@internal.kernel.statement;

namespace com.espertech.esper.runtime.@internal.statementlifesvc
{
    public class StatementLifecycleServiceImpl : StatementLifecycleService
    {
        private readonly IDictionary<int, EPStatementSPI> statementsById = new Dictionary<int, EPStatementSPI>();

        public void AddStatement(EPStatementSPI stmt)
        {
            int statementId = stmt.StatementId;
            if (statementsById.ContainsKey(statementId))
            {
                throw new ArgumentException("Statement id " + stmt.StatementId + " already assigned");
            }
            statementsById.Put(statementId, stmt);
        }

        public StatementContext GetStatementContextById(int statementId)
        {
            EPStatementSPI statement = statementsById.Get(statementId);
            return statement == null ? null : statement.StatementContext;
        }

        public EPStatementSPI GetStatementById(int statementId)
        {
            return statementsById.Get(statementId);
        }

        public void RemoveStatement(int statementId)
        {
            statementsById.Remove(statementId);
        }
    }
} // end of namespace