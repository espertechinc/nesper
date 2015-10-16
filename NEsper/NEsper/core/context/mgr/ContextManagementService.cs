///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.spec;

namespace com.espertech.esper.core.context.mgr
{
    public interface ContextManagementService
    {
        void AddContextSpec(EPServicesContext servicesContext, AgentInstanceContext agentInstanceContext, CreateContextDesc contextDesc, bool isRecoveringResilient, EventType statementResultEventType);
        int ContextCount { get; }

        ContextDescriptor GetContextDescriptor(String contextName);

        void AddStatement(String contextName, ContextControllerStatementBase statement, bool isRecoveringResilient);
        void StoppedStatement(String contextName, String statementName, String statementId);
        void DestroyedStatement(String contextName, String statementName, String statementId);
    
        void DestroyedContext(String contextName);

        ContextManager GetContextManager(String contextName);
    }
}
