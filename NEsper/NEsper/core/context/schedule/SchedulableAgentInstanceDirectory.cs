///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.core.context.util;

namespace com.espertech.esper.core.context.schedule
{
    public interface SchedulableAgentInstanceDirectory {
        void Add(EPStatementAgentInstanceHandle handle);
        void Remove(String statementId, int agentInstanceId);
        void RemoveStatement(String statementId);
        EPStatementAgentInstanceHandle Lookup(String statementId, int agentInstanceId);
    }
}
