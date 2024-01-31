///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.join.lookup;
using com.espertech.esper.common.@internal.epl.join.queryplan;
using com.espertech.esper.common.@internal.epl.lookupplansubord;
using com.espertech.esper.common.@internal.epl.namedwindow.consume;
using com.espertech.esper.common.@internal.epl.namedwindow.path;
using com.espertech.esper.common.@internal.statement.resource;

namespace com.espertech.esper.common.@internal.epl.namedwindow.core
{
    public class NamedWindowImpl : NamedWindowWDirectConsume
    {
        private readonly NamedWindowRootView rootView;
        private readonly NamedWindowTailView tailView;
        private readonly EventTableIndexMetadata eventTableIndexMetadataRepo;
        private StatementContext statementContext;

        public NamedWindowImpl(
            NamedWindowMetaData metadata,
            EPStatementInitServices services)
        {
            rootView = new NamedWindowRootView(metadata);
            eventTableIndexMetadataRepo = metadata.IndexMetadata;
            tailView = services.NamedWindowFactoryService.CreateNamedWindowTailView(
                metadata.EventType,
                metadata.IsChildBatching,
                services,
                metadata.ContextName);
        }

        public string Name => rootView.EventType.Name;

        public NamedWindowRootView RootView => rootView;

        public NamedWindowTailView TailView => tailView;

        public NamedWindowConsumerView AddConsumer(
            NamedWindowConsumerDesc consumerDesc,
            bool isSubselect)
        {
            // handle same-context consumer
            if (rootView.ContextName != null) {
                var contextDescriptor =
                    consumerDesc.AgentInstanceContext.StatementContext.ContextRuntimeDescriptor;
                if (contextDescriptor != null && rootView.ContextName.Equals(contextDescriptor.ContextName)) {
                    var holder =
                        statementContext.StatementResourceService.GetPartitioned(
                            consumerDesc.AgentInstanceContext.AgentInstanceId);
                    return holder.NamedWindowInstance.TailViewInstance.AddConsumer(consumerDesc, isSubselect);
                }
                else {
                    // consumer is out-of-context
                    return tailView.AddConsumerNoContext(consumerDesc); // non-context consumers
                }
            }

            // handle no context associated
            var statementResourceService =
                statementContext.StatementCPCacheService.StatementResourceService;
            return statementResourceService.ResourcesUnpartitioned.NamedWindowInstance.TailViewInstance.AddConsumer(
                consumerDesc,
                isSubselect);
        }

        public NamedWindowInstance GetNamedWindowInstance(ExprEvaluatorContext exprEvaluatorContext)
        {
            if (rootView.ContextName == null) {
                return NamedWindowInstanceNoContext;
            }

            if (exprEvaluatorContext.ContextName == null) {
                return null;
            }

            if (rootView.ContextName == exprEvaluatorContext.ContextName) {
                return GetNamedWindowInstance(exprEvaluatorContext.AgentInstanceId);
            }

            return null;
        }

        public NamedWindowInstance NamedWindowInstanceNoContext {
            get {
                var statementResourceService =
                    statementContext.StatementCPCacheService.StatementResourceService;
                var holder = statementResourceService.Unpartitioned;
                return holder?.NamedWindowInstance;
            }
        }

        public NamedWindowInstance GetNamedWindowInstance(int agentInstanceId)
        {
            var statementResourceService =
                statementContext.StatementCPCacheService.StatementResourceService;
            var holder = statementResourceService.GetPartitioned(agentInstanceId);
            return holder?.NamedWindowInstance;
        }

        public EventTableIndexMetadata EventTableIndexMetadata => eventTableIndexMetadataRepo;

        public void RemoveAllInstanceIndexes(IndexMultiKey index)
        {
            var statementResourceService =
                statementContext.StatementCPCacheService.StatementResourceService;
            if (rootView.ContextName == null) {
                var holder = statementResourceService.Unpartitioned;
                holder?.NamedWindowInstance?.RemoveIndex(index);
            }
            else {
                foreach (var entry in statementResourceService
                             .ResourcesPartitioned) {
                    entry.Value.NamedWindowInstance?.RemoveIndex(index);
                }
            }
        }

        public void RemoveIndexInstance(
            IndexMultiKey indexMultiKey,
            int agentInstanceId)
        {
            var statementResourceService = statementContext.StatementCPCacheService.StatementResourceService;
            if (statementResourceService.ResourcesPartitioned.TryGetValue(agentInstanceId, out var holder)) {
                holder.NamedWindowInstance?.RemoveIndex(indexMultiKey);
            }
        }

        public void RemoveIndexReferencesStmtMayRemoveIndex(
            IndexMultiKey imk,
            string referringDeploymentId,
            string referringStatementName)
        {
            var last = eventTableIndexMetadataRepo.RemoveIndexReference(imk, referringDeploymentId);
            if (last) {
                eventTableIndexMetadataRepo.RemoveIndex(imk);
                RemoveAllInstanceIndexes(imk);
            }
        }

        public void ValidateAddIndex(
            string deloymentId,
            string statementName,
            string explicitIndexName,
            string explicitIndexModuleName,
            QueryPlanIndexItem explicitIndexDesc,
            IndexMultiKey imk)
        {
            eventTableIndexMetadataRepo.AddIndexExplicit(
                false,
                imk,
                explicitIndexName,
                explicitIndexModuleName,
                explicitIndexDesc,
                deloymentId);
        }

        public StatementContext StatementContext {
            get => statementContext;
            set => statementContext = value;
        }
    }
} // end of namespace