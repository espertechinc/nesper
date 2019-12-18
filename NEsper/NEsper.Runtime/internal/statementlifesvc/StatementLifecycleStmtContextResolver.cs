///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;

namespace com.espertech.esper.runtime.@internal.statementlifesvc
{
    public interface StatementLifecycleStmtContextResolver
    {
        StatementContext GetStatementContextById(int statementId);
    }
} // end of namespace