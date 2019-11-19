///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.context.mgr
{
    public class ContextDeployment
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IDictionary<string, ContextManager> contexts = new Dictionary<string, ContextManager>(4);

        public int ContextCount => contexts.Count;

        public void Add(
            ContextDefinition contextDefinition,
            EPStatementInitServices services)
        {
            var contextName = contextDefinition.ContextName;
            var mgr = contexts.Get(contextName);
            if (mgr != null) {
                throw new EPException("Context by name '" + contextDefinition.ContextName + "' already exists");
            }

            var contextManager = new ContextManagerResident(services.DeploymentId, contextDefinition);
            contexts.Put(contextName, contextManager);
        }

        public ContextManager GetContextManager(string contextName)
        {
            return contexts.Get(contextName);
        }

        public void DestroyContext(
            string deploymentIdCreateContext,
            string contextName)
        {
            var entry = contexts.Get(contextName);
            if (entry == null) {
                Log.Warn(
                    "Destroy for context '" +
                    contextName +
                    "' deployment-id '" +
                    deploymentIdCreateContext +
                    "' failed to locate");
                return;
            }

            entry.DestroyContext();
            contexts.Remove(contextName);
        }
    }
} // end of namespace