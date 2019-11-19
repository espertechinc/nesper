///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.context;
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.mgr
{
    public interface ContextManagementService
    {
        int ContextCount { get; }

        CopyOnWriteList<ContextStateListener> Listeners { get; }

        void AddContext(
            ContextDefinition contextDefinition,
            EPStatementInitServices services);

        void AddStatement(
            string deploymentIdCreateContext,
            string contextName,
            ContextControllerStatementDesc statement,
            bool recovery);

        void StoppedStatement(
            string deploymentIdCreateContext,
            string contextName,
            ContextControllerStatementDesc statement);

        ContextManager GetContextManager(
            string deploymentIdCreateContext,
            string contextName);

        void DestroyedContext(
            string runtimeURI,
            string deploymentId,
            string contextName);
    }
} // end of namespace