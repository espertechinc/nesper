///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

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
                metadata.EventType, metadata.IsChildBatching, services, metadata.ContextName);
        }

        public string Name {
            get => rootView.EventType.Name;
        }

        public NamedWindowRootView RootView {
            get => rootView;
        }

        public NamedWindowTailView TailView {
            get => tailView;
        }

        public NamedWindowConsumerView AddConsumer(
            NamedWindowConsumerDesc consumerDesc,
            bool isSubselect)
        {
            // handle same-context consumer
            if (rootView.ContextName != null) {
                ContextRuntimeDescriptor contextDescriptor = consumerDesc.AgentInstanceContext.StatementContext.ContextRuntimeDescriptor;
                if (contextDescriptor != null && rootView.ContextName.Equals(contextDescriptor.ContextName)) {
                    StatementResourceHolder holder =
                        statementContext.StatementResourceService.GetPartitioned(consumerDesc.AgentInstanceContext.AgentInstanceId);
                    return holder.NamedWindowInstance.TailViewInstance.AddConsumer(consumerDesc, isSubselect);
                }
                else {
                    // consumer is out-of-context
                    return tailView.AddConsumerNoContext(consumerDesc); // non-context consumers
                }
            }

            // handle no context associated
            StatementResourceService statementResourceService = statementContext.StatementCPCacheService.StatementResourceService;
            return statementResourceService.ResourcesUnpartitioned.NamedWindowInstance.TailViewInstance.AddConsumer(consumerDesc, isSubselect);
        }

        public NamedWindowInstance GetNamedWindowInstance(AgentInstanceContext agentInstanceContext)
        {
            if (rootView.ContextName == null) {
                return NamedWindowInstanceNoContext;
            }

            if (agentInstanceContext.StatementContext.ContextRuntimeDescriptor == null) {
                return null;
            }

            if (this.rootView.ContextName.Equals(agentInstanceContext.StatementContext.ContextRuntimeDescriptor.ContextName)) {
                return GetNamedWindowInstance(agentInstanceContext.AgentInstanceId);
            }

            return null;
        }

        public NamedWindowInstance NamedWindowInstanceNoContext {
            get {
                StatementResourceService statementResourceService =
                    statementContext.StatementCPCacheService.StatementResourceService;
                StatementResourceHolder holder = statementResourceService.Unpartitioned;
                return holder == null ? null : holder.NamedWindowInstance;
            }
        }

        public NamedWindowInstance GetNamedWindowInstance(int agentInstanceId)
        {
            StatementResourceService statementResourceService = statementContext.StatementCPCacheService.StatementResourceService;
            StatementResourceHolder holder = statementResourceService.GetPartitioned(agentInstanceId);
            return holder == null ? null : holder.NamedWindowInstance;
        }

        public EventTableIndexMetadata EventTableIndexMetadata {
            get => eventTableIndexMetadataRepo;
        }

        public void RemoveAllInstanceIndexes(IndexMultiKey index)
        {
            StatementResourceService statementResourceService = statementContext.StatementCPCacheService.StatementResourceService;
            if (rootView.ContextName == null) {
                StatementResourceHolder holder = statementResourceService.Unpartitioned;
                if (holder != null && holder.NamedWindowInstance != null) {
                    holder.NamedWindowInstance.RemoveIndex(index);
                }
            }
            else {
                foreach (KeyValuePair<int, StatementResourceHolder> entry in statementResourceService.ResourcesPartitioned) {
                    if (entry.Value.NamedWindowInstance != null) {
                        entry.Value.NamedWindowInstance.RemoveIndex(index);
                    }
                }
            }
        }

        public void RemoveIndexReferencesStmtMayRemoveIndex(
            IndexMultiKey imk,
            string referringDeploymentId,
            string referringStatementName)
        {
            bool last = eventTableIndexMetadataRepo.RemoveIndexReference(imk, referringDeploymentId);
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
            eventTableIndexMetadataRepo.AddIndexExplicit(false, imk, explicitIndexName, explicitIndexModuleName, explicitIndexDesc, deloymentId);
        }

        public StatementContext StatementContext {
            get => statementContext;
            set { this.statementContext = value; }
        }
    }
} // end of namespace