///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;

namespace com.espertech.esper.core.context.mgr
{
    public class ContextManagementServiceImpl : ContextManagementService
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        private readonly IDictionary<String, ContextManagerEntry> _contexts;
        private readonly ICollection<String> _destroyedContexts = new HashSet<String>();
    
        public ContextManagementServiceImpl()
        {
            _contexts = new Dictionary<String, ContextManagerEntry>();
        }
    
        public void AddContextSpec(EPServicesContext servicesContext, AgentInstanceContext agentInstanceContext, CreateContextDesc contextDesc, bool isRecoveringResilient)
        {
            var mgr = _contexts.Get(contextDesc.ContextName);
            if (mgr != null) {
                if (_destroyedContexts.Contains(contextDesc.ContextName)) {
                    throw new ExprValidationException("Context by name '" + contextDesc.ContextName + "' is still referenced by statements and may not be changed");
                }
                throw new ExprValidationException("Context by name '" + contextDesc.ContextName + "' already exists");
            }
    
            var factoryServiceContext = new ContextControllerFactoryServiceContext(contextDesc.ContextName, servicesContext, contextDesc.ContextDetail, agentInstanceContext, isRecoveringResilient);
            ContextManager contextManager;
            if (contextDesc.ContextDetail is ContextDetailNested) {
                contextManager = new ContextManagerNested(factoryServiceContext);
            }
            else {
                contextManager = new ContextManagerImpl(factoryServiceContext);
            }
    
            factoryServiceContext.AgentInstanceContextCreate.EpStatementAgentInstanceHandle.FilterFaultHandler = contextManager;
    
            _contexts.Put(contextDesc.ContextName, new ContextManagerEntry(contextManager));
        }

        public int ContextCount
        {
            get { return _contexts.Count; }
        }

        public ContextDescriptor GetContextDescriptor(String contextName) {
            ContextManagerEntry entry = _contexts.Get(contextName);
            if (entry == null) {
                return null;
            }
            return entry.ContextManager.ContextDescriptor;
        }
    
        public ContextManager GetContextManager(String contextName) {
            ContextManagerEntry entry = _contexts.Get(contextName);
            if (entry == null) {
                return null;
            }
            return entry.ContextManager;
        }
    
        public void AddStatement(String contextName, ContextControllerStatementBase statement, bool isRecoveringResilient) {
            ContextManagerEntry entry = _contexts.Get(contextName);
            if (entry == null) {
                throw new ExprValidationException(GetNotDecaredText(contextName));
            }
            entry.AddStatement(statement.StatementContext.StatementId);
            entry.ContextManager.AddStatement(statement, isRecoveringResilient);
        }
    
        public void DestroyedStatement(String contextName, String statementName, String statementId) {
            ContextManagerEntry entry = _contexts.Get(contextName);
            if (entry == null) {
                Log.Warn("Dispose statement for statement '" + statementName + "' failed to locate corresponding context manager '" + contextName + "'");
                return;
            }
            entry.RemoveStatement(statementId);
            entry.ContextManager.DestroyStatement(statementName, statementId);
            
            if (entry.StatementCount == 0 && _destroyedContexts.Contains(contextName)) {
                DestroyContext(contextName, entry);
            }
        }
    
        public void StoppedStatement(String contextName, String statementName, String statementId) {
            ContextManagerEntry entry = _contexts.Get(contextName);
            if (entry == null) {
                Log.Warn("Stop statement for statement '" + statementName + "' failed to locate corresponding context manager '" + contextName + "'");
                return;
            }
            try {
                entry.ContextManager.StopStatement(statementName, statementId);
            }
            catch (Exception ex) {
                Log.Warn("Failed to stop statement '" + statementName + "' as related to context '" + contextName + "': " + ex.Message, ex);
            }
        }
    
        public void DestroyedContext(String contextName) {
            ContextManagerEntry entry = _contexts.Get(contextName);
            if (entry == null) {
                Log.Warn("Dispose for context '" + contextName + "' failed to locate corresponding context manager '" + contextName + "'");
                return;
            }
            if (entry.StatementCount == 0) {
                DestroyContext(contextName, entry);
            }
            else {
                // some remaining statements have references
                _destroyedContexts.Add(contextName);
            }
        }
    
        private void DestroyContext(String contextName, ContextManagerEntry entry) {
            entry.ContextManager.SafeDestroy();
            _contexts.Remove(contextName);
            _destroyedContexts.Remove(contextName);
        }
    
        private String GetNotDecaredText(String contextName) {
            return "Context by name '" + contextName + "' has not been declared";
        }
    
        public class ContextManagerEntry
        {
            private readonly ICollection<String> _referringStatements;
    
            public ContextManagerEntry(ContextManager contextManager) {
                ContextManager = contextManager;
                _referringStatements = new HashSet<String>();
            }

            public ContextManager ContextManager { get; private set; }

            public void AddStatement(String statementId) {
                _referringStatements.Add(statementId);
            }

            public int StatementCount
            {
                get { return _referringStatements.Count; }
            }

            public void RemoveStatement(String statementId) {
                _referringStatements.Remove(statementId);
            }
        }
    }
}
