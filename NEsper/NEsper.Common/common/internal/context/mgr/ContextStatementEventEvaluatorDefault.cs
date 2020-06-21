///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.exception;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.mgr
{
    public class ContextStatementEventEvaluatorDefault : ContextStatementEventEvaluator
    {
        public readonly static ContextStatementEventEvaluatorDefault INSTANCE =
            new ContextStatementEventEvaluatorDefault();

        private ContextStatementEventEvaluatorDefault()
        {
        }

        public void EvaluateEventForStatement(
            EventBean theEvent,
            IList<AgentInstance> agentInstances,
            AgentInstanceContext agentInstanceContextCreate)
        {
            // context was created - reevaluate for the given event
            ArrayDeque<FilterHandle> callbacks = new ArrayDeque<FilterHandle>(2);
            agentInstanceContextCreate.FilterService.Evaluate(theEvent, callbacks, agentInstanceContextCreate); // evaluates for ALL statements
            if (callbacks.IsEmpty()) {
                return;
            }

            // there is a single callback and a single context, if they match we are done
            if (agentInstances.Count == 1 && callbacks.Count == 1) {
                AgentInstance agentInstance = agentInstances[0];
                AgentInstanceContext agentInstanceContext = agentInstance.AgentInstanceContext;
                FilterHandle callback = callbacks.First;
                if (agentInstanceContext.StatementId == callback.StatementId &&
                    agentInstanceContext.AgentInstanceId == callback.AgentInstanceId) {
                    Process(agentInstance, callbacks, theEvent);
                }

                return;
            }

            // use the right sorted/unsorted Map keyed by AgentInstance to sort
            bool isPrioritized = agentInstanceContextCreate.RuntimeSettingsService.ConfigurationRuntime.Execution
                .IsPrioritized;
            IDictionary<AgentInstance, object> stmtCallbacks;
            if (!isPrioritized) {
                stmtCallbacks = new Dictionary<AgentInstance, object>();
            }
            else {
                stmtCallbacks = new OrderedDictionary<AgentInstance, object>(AgentInstanceComparator.INSTANCE);
            }

            // process all callbacks
            foreach (FilterHandle filterHandle in callbacks) {
                EPStatementHandleCallbackFilter handleCallback = (EPStatementHandleCallbackFilter) filterHandle;

                // determine if this filter entry applies to any of the affected agent instances
                int statementId = filterHandle.StatementId;
                AgentInstance agentInstanceFound = null;
                foreach (AgentInstance agentInstance in agentInstances) {
                    AgentInstanceContext agentInstanceContext = agentInstance.AgentInstanceContext;
                    if (agentInstanceContext.StatementId == statementId &&
                        agentInstanceContext.AgentInstanceId == handleCallback.AgentInstanceId) {
                        agentInstanceFound = agentInstance;
                        break;
                    }
                }

                if (agentInstanceFound == null) { // when the callback is for some other stmt
                    continue;
                }

                EPStatementAgentInstanceHandle handle = handleCallback.AgentInstanceHandle;

                // Self-joins require that the internal dispatch happens after all streams are evaluated.
                // Priority or preemptive settings also require special ordering.
                if (handle.IsCanSelfJoin || isPrioritized) {
                    var stmtCallback = stmtCallbacks.Get(agentInstanceFound);
                    if (stmtCallback == null) {
                        stmtCallbacks.Put(agentInstanceFound, handleCallback);
                    }
                    else if (stmtCallback is ArrayDeque<FilterHandle> callbackFilterDeque) {
                        if (!callbackFilterDeque.Contains(handleCallback)) {
                            // De-duplicate for Filter OR expression paths
                            callbackFilterDeque.Add(handleCallback);
                        }
                    }
                    else {
                        var filterDeque = new ArrayDeque<FilterHandle>(4);
                        filterDeque.Add((FilterHandle) stmtCallback);
                        if (stmtCallback != handleCallback) { // De-duplicate for Filter OR expression paths
                            filterDeque.Add(handleCallback);
                        }

                        stmtCallbacks.Put(agentInstanceFound, filterDeque);
                    }

                    continue;
                }

                // no need to be sorted, process
                Process(agentInstanceFound, Collections.SingletonList<FilterHandle>(handleCallback), theEvent);
            }

            if (stmtCallbacks.IsEmpty()) {
                return;
            }

            // Process self-join or sorted prioritized callbacks
            foreach (KeyValuePair<AgentInstance, object> entry in stmtCallbacks) {
                AgentInstance agentInstance = entry.Key;
                object callbackList = entry.Value;
                if (callbackList is ICollection<FilterHandle> filterHandleCollection) {
                    Process(agentInstance, filterHandleCollection, theEvent);
                }
                else {
                    Process(
                        agentInstance,
                        Collections.SingletonList<FilterHandle>((FilterHandle) callbackList),
                        theEvent);
                }

                if (agentInstance.AgentInstanceContext.EpStatementAgentInstanceHandle.IsPreemptive) {
                    return;
                }
            }
        }

        private static void Process(
            AgentInstance agentInstance,
            ICollection<FilterHandle> callbacks,
            EventBean theEvent)
        {
            AgentInstanceContext agentInstanceContext = agentInstance.AgentInstanceContext;
            using (agentInstance.AgentInstanceContext.AgentInstanceLock.AcquireWriteLock()) {
                try {
                    agentInstance.AgentInstanceContext.VariableManagementService.SetLocalVersion();

                    // sub-selects always go first
                    foreach (FilterHandle handle in callbacks) {
                        EPStatementHandleCallbackFilter callback = (EPStatementHandleCallbackFilter) handle;
                        if (callback.AgentInstanceHandle != agentInstanceContext.EpStatementAgentInstanceHandle) {
                            continue;
                        }

                        callback.FilterCallback.MatchFound(theEvent, null);
                    }

                    agentInstanceContext.EpStatementAgentInstanceHandle.InternalDispatch();
                }
                catch (EPException) {
                    throw;
                }
                catch (Exception ex) {
                    agentInstanceContext.ExceptionHandlingService.HandleException(
                        ex,
                        agentInstanceContext.EpStatementAgentInstanceHandle,
                        ExceptionHandlerExceptionType.PROCESS,
                        theEvent);
                }
                finally {
                    if (agentInstanceContext.StatementContext.EpStatementHandle.HasTableAccess) {
                        agentInstanceContext.TableExprEvaluatorContext.ReleaseAcquiredLocks();
                    }
                }
            }
        }
    }
} // end of namespace