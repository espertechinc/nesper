///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

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

        ContextDescriptor GetContextDescriptor(string contextName);

        void AddStatement(string contextName, ContextControllerStatementBase statement, bool isRecoveringResilient);
        void StoppedStatement(string contextName, string statementName, int statementId, string epl, ExceptionHandlingService exceptionHandlingService);
        void DestroyedStatement(string contextName, string statementName, int statementId);
    
        void DestroyedContext(String contextName);

        IDictionary<string, ContextManagerEntry> Contexts { get; }

        ContextManager GetContextManager(String contextName);
    }
}
