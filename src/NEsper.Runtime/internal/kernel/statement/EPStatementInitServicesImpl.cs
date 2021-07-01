///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.context.activator;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.common.@internal.context.mgr;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.namedwindow.consume;
using com.espertech.esper.common.@internal.epl.namedwindow.core;
using com.espertech.esper.common.@internal.epl.namedwindow.path;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.script.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.common.@internal.@event.avro;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.path;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.statement.resource;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;
using com.espertech.esper.runtime.@internal.kernel.service;
using static com.espertech.esper.common.@internal.context.util.StatementCPCacheService;

namespace com.espertech.esper.runtime.@internal.kernel.statement
{
    public class EPStatementInitServicesImpl : EPStatementInitServices
    {
        public EPStatementInitServicesImpl(
            String statementName,
            IDictionary<StatementProperty, Object> statementProperties,
            Attribute[] annotations,
            String deploymentId,
            EventTypeResolver eventTypeResolver,
            FilterSpecActivatableRegistry filterSpecActivatableRegistry,
            FilterSharedBoolExprRegistery filterSharedBoolExprRegistery,
            FilterSharedLookupableRegistery filterSharedLookupableRegistery,
            ModuleIncidentals moduleIncidentals,
            bool recovery,
            StatementResourceService statementResourceService,
            StatementResultService statementResultService,
            EPServicesContext servicesContext)
        {
            StatementName = statementName;
            StatementProperties = statementProperties;
            Annotations = annotations;
            DeploymentId = deploymentId;
            EventTypeResolver = eventTypeResolver;
            FilterSpecActivatableRegistry = filterSpecActivatableRegistry;
            FilterSharedBoolExprRegistery = filterSharedBoolExprRegistery;
            FilterSharedLookupableRegistery = filterSharedLookupableRegistery;
            ModuleIncidentals = moduleIncidentals;
            IsRecovery = recovery;
            StatementResourceService = statementResourceService;
            StatementResultService = statementResultService;
            ServicesContext = servicesContext;
        }

        public IContainer Container => ServicesContext.Container;

        public IObjectCopier ObjectCopier => ServicesContext.Container.Resolve<IObjectCopier>();

        public EPServicesContext ServicesContext { get; }

        public ModuleIncidentals ModuleIncidentals { get; }

        public IList<StatementReadyCallback> ReadyCallbacks { get; } = new List<StatementReadyCallback>();

        public bool IsRecovery { get; }

        public AggregationServiceFactoryService AggregationServiceFactoryService => ServicesContext.AggregationServiceFactoryService;

        public Attribute[] Annotations { get; }

        public ContextServiceFactory ContextServiceFactory => ServicesContext.ContextServiceFactory;

        public string DeploymentId { get; }

        public EventBeanTypedEventFactory EventBeanTypedEventFactory => ServicesContext.EventBeanTypedEventFactory;

        public ImportServiceRuntime ImportServiceRuntime => ServicesContext.ImportServiceRuntime;

        public ScriptCompiler ScriptCompiler => ServicesContext.ScriptCompiler;

        public string RuntimeURI => ServicesContext.RuntimeURI;

        public RuntimeSettingsService RuntimeSettingsService => ServicesContext.RuntimeSettingsService;

        public RuntimeExtensionServices RuntimeExtensionServices => ServicesContext.RuntimeExtensionServices;

        public EventTableIndexService EventTableIndexService => ServicesContext.EventTableIndexService;

        public EventTypeAvroHandler EventTypeAvroHandler => ServicesContext.EventTypeAvroHandler;

        public ExceptionHandlingService ExceptionHandlingService => ServicesContext.ExceptionHandlingService;

        public FilterSharedLookupableRegistery FilterSharedLookupableRegistery { get; }

        public FilterSharedBoolExprRegistery FilterSharedBoolExprRegistery { get; }

        public NamedWindowDispatchService NamedWindowDispatchService => ServicesContext.NamedWindowDispatchService;

        public NamedWindowFactoryService NamedWindowFactoryService => ServicesContext.NamedWindowFactoryService;

        public NamedWindowManagementService NamedWindowManagementService => ServicesContext.NamedWindowManagementService;

        public PatternFactoryService PatternFactoryService => ServicesContext.PatternFactoryService;

        public StatementResultService StatementResultService { get; }
        
        public string StatementName { get; }

        public IDictionary<StatementProperty, object> StatementProperties { get; set; }

        public ContextManagementService ContextManagementService => ServicesContext.ContextManagementService;

        public EventTypeResolver EventTypeResolver { get; }

        public FilterSpecActivatableRegistry FilterSpecActivatableRegistry { get; }

        public FilterBooleanExpressionFactory FilterBooleanExpressionFactory => ServicesContext.FilterBooleanExpressionFactory;

        public InternalEventRouteDest InternalEventRouteDest => ServicesContext.InternalEventRouteDest;

        public void AddReadyCallback(StatementReadyCallback readyCallback)
        {
            ReadyCallbacks.Add(readyCallback);
        }

        public ResultSetProcessorHelperFactory ResultSetProcessorHelperFactory => ServicesContext.ResultSetProcessorHelperFactory;

        public StatementResourceService StatementResourceService { get; }

        public TableManagementService TableManagementService => ServicesContext.TableManagementService;

        public TimeAbacus TimeAbacus => ServicesContext.TimeAbacus;

        public TimeProvider TimeProvider => ServicesContext.SchedulingService;

        public TimeSourceService TimeSourceService => ServicesContext.TimeSourceService;

        public VariableManagementService VariableManagementService => ServicesContext.VariableManagementService;

        public ViewableActivatorFactory ViewableActivatorFactory => ServicesContext.ViewableActivatorFactory;

        public ViewFactoryService ViewFactoryService => ServicesContext.ViewFactoryService;

        public void ActivateNamedWindow(string name)
        {
            // we are checking that all is resolved
            var detail = ModuleIncidentals.NamedWindows.Get(name);
            if (detail == null) {
                throw new ArgumentException("Failed to find named window information for '" + name + "'");
            }

            ServicesContext.NamedWindowManagementService.AddNamedWindow(name, detail, this);
        }

        public void ActivateTable(string name)
        {
            // we are checking that all is resolved
            var detail = ModuleIncidentals.Tables.Get(name);
            if (detail == null) {
                throw new ArgumentException("Failed to find table information for '" + name + "'");
            }

            ServicesContext.TableManagementService.AddTable(name, detail, this);
        }

        public void ActivateContext(
            string name,
            ContextDefinition definition)
        {
            // we are checking that all is resolved
            var detail = ModuleIncidentals.Contexts.Get(name);
            if (detail == null) {
                throw new ArgumentException("Failed to find context information for '" + name + "'");
            }

            ServicesContext.ContextManagementService.AddContext(definition, this);
        }

        public void ActivateVariable(
            string name,
            DataInputOutputSerde serde)
        {
            var variable = ModuleIncidentals.Variables.Get(name);
            if (variable == null) {
                throw new ArgumentException("Failed to find variable information for '" + name + "'");
            }

            string contextDeploymentId = null;
            if (variable.OptionalContextName != null) {
                contextDeploymentId = ContextDeployTimeResolver.ResolveContextDeploymentId(
                    variable.OptionalContextModule,
                    variable.OptionalContextVisibility,
                    variable.OptionalContextName,
                    DeploymentId, ServicesContext.ContextPathRegistry);
            }

            ServicesContext.VariableManagementService.AddVariable(DeploymentId, variable, contextDeploymentId, serde);

            // for non-context variables we allocate the state
            if (contextDeploymentId == null) {
                ServicesContext.VariableManagementService.AllocateVariableState(
                    DeploymentId, name, DEFAULT_AGENT_INSTANCE_ID, IsRecovery, null, ServicesContext.EventBeanTypedEventFactory);
            }
        }

        public void ActivateExpression(string name)
        {
        }

        public PathRegistry<string, ExpressionDeclItem> ExprDeclaredPathRegistry => ServicesContext.ExprDeclaredPathRegistry;

        public PathRegistry<string, NamedWindowMetaData> NamedWindowPathRegistry => ServicesContext.NamedWindowPathRegistry;

        public PathRegistry<string, TableMetaData> TablePathRegistry => ServicesContext.TablePathRegistry;

        public PathRegistry<string, VariableMetaData> VariablePathRegistry => ServicesContext.VariablePathRegistry;
    }
} // end of namespace