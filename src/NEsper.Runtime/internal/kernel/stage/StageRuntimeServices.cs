///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.hook.expr;
using com.espertech.esper.common.@internal.epl.namedwindow.consume;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.eventtyperepo;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.statement.dispatch;
using com.espertech.esper.container;
using com.espertech.esper.runtime.@internal.kernel.thread;
using com.espertech.esper.runtime.@internal.statementlifesvc;

namespace com.espertech.esper.runtime.@internal.kernel.stage
{
    public class StageRuntimeServices
    {
        public StageRuntimeServices(
            IContainer container,
            ImportServiceRuntime importServiceRuntime,
            Configuration configSnapshot,
            DispatchService dispatchService,
            EventBeanService eventBeanService,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            EventTypeRepository eventTypeRepositoryBus,
            EventTypeResolvingBeanFactory eventTypeResolvingBeanFactory,
            ExceptionHandlingService exceptionHandlingService,
            NamedWindowDispatchService namedWindowDispatchService,
            string runtimeURI,
            RuntimeSettingsService runtimeSettingsService,
            StatementLifecycleService statementLifecycleService,
            TableExprEvaluatorContext tableExprEvaluatorContext,
            ThreadingService threadingService,
            VariableManagementService variableManagementService)
        {
            Container = container;
            ImportServiceRuntime = importServiceRuntime;
            ConfigSnapshot = configSnapshot;
            DispatchService = dispatchService;
            EventBeanService = eventBeanService;
            EventBeanTypedEventFactory = eventBeanTypedEventFactory;
            EventTypeRepositoryBus = eventTypeRepositoryBus;
            EventTypeResolvingBeanFactory = eventTypeResolvingBeanFactory;
            ExceptionHandlingService = exceptionHandlingService;
            NamedWindowDispatchService = namedWindowDispatchService;
            RuntimeURI = runtimeURI;
            RuntimeSettingsService = runtimeSettingsService;
            StatementLifecycleService = statementLifecycleService;
            TableExprEvaluatorContext = tableExprEvaluatorContext;
            ThreadingService = threadingService;
            VariableManagementService = variableManagementService;
        }

        public IContainer Container { get; }
        public ImportServiceRuntime ImportServiceRuntime { get; set; }

        public Configuration ConfigSnapshot { get; set; }

        public DispatchService DispatchService { get; }

        public EventBeanService EventBeanService { get; }

        public EventBeanTypedEventFactory EventBeanTypedEventFactory { get; }

        public EventTypeRepository EventTypeRepositoryBus { get; }

        public EventTypeResolvingBeanFactory EventTypeResolvingBeanFactory { get; }

        public ExceptionHandlingService ExceptionHandlingService { get; }

        public NamedWindowDispatchService NamedWindowDispatchService { get; }

        public RuntimeSettingsService RuntimeSettingsService { get; }

        public string RuntimeURI { get; }

        public StatementLifecycleService StatementLifecycleService { get; }

        public VariableManagementService VariableManagementService { get; }

        public TableExprEvaluatorContext TableExprEvaluatorContext { get; }

        public ThreadingService ThreadingService { get; }
    }
} // end of namespace