///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.expr;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.enummethod.cache;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.common.@internal.epl.prior;
using com.espertech.esper.common.@internal.epl.script.core;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.metrics.audit;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.statement.helper;
using com.espertech.esper.common.@internal.view.access;
using com.espertech.esper.common.@internal.view.previous;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.common.@internal.view.core
{
    public class AgentInstanceViewFactoryChainContext : ExprEvaluatorContext
    {
        public AgentInstanceViewFactoryChainContext(
            AgentInstanceContext agentInstanceContext,
            bool isRemoveStream,
            PreviousGetterStrategy previousNodeGetter,
            ViewUpdatedCollection priorViewUpdatedCollection)
        {
            AgentInstanceContext = agentInstanceContext;
            IsRemoveStream = isRemoveStream;
            PreviousNodeGetter = previousNodeGetter;
            PriorViewUpdatedCollection = priorViewUpdatedCollection;
        }

        public AgentInstanceContext AgentInstanceContext { get; }

        public PreviousGetterStrategy PreviousNodeGetter { get; }

        public ViewUpdatedCollection PriorViewUpdatedCollection { get; }

        public StatementContext StatementContext => AgentInstanceContext.StatementContext;

        public RuntimeSettingsService RuntimeSettingsService => AgentInstanceContext.StatementContext.RuntimeSettingsService;

        public Attribute[] Annotations => AgentInstanceContext.StatementContext.Annotations;

        public EPStatementAgentInstanceHandle EpStatementAgentInstanceHandle => AgentInstanceContext.EpStatementAgentInstanceHandle;

        public bool IsRemoveStream { get; set; }

        public SchedulingService SchedulingService => AgentInstanceContext.SchedulingService;

        public EventBeanTypedEventFactory EventBeanTypedEventFactory => AgentInstanceContext.EventBeanTypedEventFactory;

        public RuntimeExtensionServices RuntimeExtensionServices => AgentInstanceContext.RuntimeExtensionServicesContext;

        public ImportServiceRuntime ImportService => AgentInstanceContext.ImportServiceRuntime;

        public string StatementName => AgentInstanceContext.StatementName;

        public object UserObjectCompileTime => AgentInstanceContext.UserObjectCompileTime;

        public int StatementId => AgentInstanceContext.StatementId;

        public string DeploymentId => AgentInstanceContext.DeploymentId;

        public int AgentInstanceId => AgentInstanceContext.AgentInstanceId;

        public string RuntimeURI => AgentInstanceContext.RuntimeURI;

        public EventBeanService EventBeanService => AgentInstanceContext.EventBeanService;

        public TimeProvider TimeProvider => AgentInstanceContext.TimeProvider;

        public IReaderWriterLock AgentInstanceLock => AgentInstanceContext.AgentInstanceLock;

        public EventBean ContextProperties => AgentInstanceContext.ContextProperties;

        public TableExprEvaluatorContext TableExprEvaluatorContext => AgentInstanceContext.TableExprEvaluatorContext;

        public ExpressionResultCacheService ExpressionResultCacheService => AgentInstanceContext.ExpressionResultCacheService;

        public AgentInstanceScriptContext AllocateAgentInstanceScriptContext => AgentInstanceContext.AllocateAgentInstanceScriptContext;

        public AuditProvider AuditProvider => AgentInstanceContext.AuditProvider;

        public InstrumentationCommon InstrumentationProvider => AgentInstanceContext.InstrumentationProvider;

        public ExceptionHandlingService ExceptionHandlingService => AgentInstanceContext.ExceptionHandlingService;

        public object FilterReboolConstant {
            get => null;
            set { }
        }
        
        public string ContextName => AgentInstanceContext.ContextName;

        public string EPLWhenAvailable => AgentInstanceContext.EPLWhenAvailable;

        public TimeZoneInfo TimeZone => AgentInstanceContext.TimeZone;

        public TimeAbacus TimeAbacus => AgentInstanceContext.TimeAbacus;

        public VariableManagementService VariableManagementService => AgentInstanceContext.VariableManagementService;
        public string ModuleName => AgentInstanceContext.ModuleName;

        public bool IsWritesToTables => AgentInstanceContext.IsWritesToTables;

        public static AgentInstanceViewFactoryChainContext Create(
            ViewFactory[] viewFactoryChain,
            AgentInstanceContext agentInstanceContext,
            ViewResourceDelegateDesc viewResourceDelegate)
        {
            PreviousGetterStrategy previousNodeGetter = null;
            if (viewResourceDelegate.HasPrevious) {
                var factoryFound = EPStatementStartMethodHelperPrevious.FindPreviousViewFactory(viewFactoryChain);
                previousNodeGetter = factoryFound.MakePreviousGetter();
            }

            ViewUpdatedCollection priorViewUpdatedCollection = null;
            if (viewResourceDelegate.PriorRequests != null && !viewResourceDelegate.PriorRequests.IsEmpty()) {
                var priorEventViewFactory = PriorHelper.FindPriorViewFactory(viewFactoryChain);
                priorViewUpdatedCollection = priorEventViewFactory.MakeViewUpdatedCollection(
                    viewResourceDelegate.PriorRequests,
                    agentInstanceContext);
            }

            var removedStream = false;
            if (viewFactoryChain.Length > 1) {
                var countDataWindow = 0;
                foreach (var viewFactory in viewFactoryChain) {
                    if (viewFactory is DataWindowViewFactory) {
                        countDataWindow++;
                    }
                }

                removedStream = countDataWindow > 1;
            }

            return new AgentInstanceViewFactoryChainContext(
                agentInstanceContext,
                removedStream,
                previousNodeGetter,
                priorViewUpdatedCollection);
        }
    }
} // end of namespace