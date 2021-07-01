///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.classprovided.core;
using com.espertech.esper.common.@internal.epl.namedwindow.path;
using com.espertech.esper.common.@internal.epl.script.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.common.@internal.metrics.stmtmetrics;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat.threading.locks;
using com.espertech.esper.runtime.@internal.deploymentlifesvc;
using com.espertech.esper.runtime.@internal.filtersvcimpl;
using com.espertech.esper.runtime.@internal.kernel.service;
using com.espertech.esper.runtime.@internal.kernel.thread;
using com.espertech.esper.runtime.@internal.schedulesvcimpl;

namespace com.espertech.esper.runtime.@internal.kernel.stage
{
	public class StageSpecificServices : EPServicesEvaluation,
		EPServicesPath
	{
		private readonly DeploymentLifecycleService deploymentLifecycleService;
		private readonly IReaderWriterLock eventProcessingRWLock;
		private readonly FilterServiceSPI filterService;
		private readonly InternalEventRouter internalEventRouter;
		private readonly MetricReportingService metricReportingService;
		private readonly SchedulingServiceSPI schedulingService;
		private readonly StageRuntimeServices stageRuntimeServices;
		private readonly ThreadingService threadingService;

		private readonly PathRegistry<string, NamedWindowMetaData> namedWindowPathRegistry =
			new PathRegistry<string, NamedWindowMetaData>(PathRegistryObjectType.NAMEDWINDOW);

		private readonly PathRegistry<string, ContextMetaData> contextPathRegistry = new PathRegistry<string, ContextMetaData>(PathRegistryObjectType.CONTEXT);
		private readonly PathRegistry<string, EventType> eventTypesPathRegistry = new PathRegistry<string, EventType>(PathRegistryObjectType.EVENTTYPE);
		private readonly PathRegistry<string, TableMetaData> tablesPathRegistry = new PathRegistry<string, TableMetaData>(PathRegistryObjectType.TABLE);

		private readonly PathRegistry<string, VariableMetaData> variablesPathRegistry =
			new PathRegistry<string, VariableMetaData>(PathRegistryObjectType.VARIABLE);

		private readonly PathRegistry<string, ExpressionDeclItem> exprDeclaredPathRegistry =
			new PathRegistry<string, ExpressionDeclItem>(PathRegistryObjectType.EXPRDECL);

		private readonly PathRegistry<NameAndParamNum, ExpressionScriptProvided> scriptPathRegistry =
			new PathRegistry<NameAndParamNum, ExpressionScriptProvided>(PathRegistryObjectType.SCRIPT);

		private readonly PathRegistry<string, ClassProvided> classProvidedPathRegistry =
			new PathRegistry<string, ClassProvided>(PathRegistryObjectType.CLASSPROVIDED);

		private InternalEventRouteDest internalEventRouteDest;

		public StageSpecificServices(
			DeploymentLifecycleService deploymentLifecycleService,
			IReaderWriterLock eventProcessingRWLock,
			FilterServiceSPI filterService,
			InternalEventRouter internalEventRouter,
			MetricReportingService metricReportingService,
			SchedulingServiceSPI schedulingService,
			StageRuntimeServices stageRuntimeServices,
			ThreadingService threadingService)
		{
			this.deploymentLifecycleService = deploymentLifecycleService;
			this.eventProcessingRWLock = eventProcessingRWLock;
			this.filterService = filterService;
			this.internalEventRouter = internalEventRouter;
			this.metricReportingService = metricReportingService;
			this.schedulingService = schedulingService;
			this.stageRuntimeServices = stageRuntimeServices;
			this.threadingService = threadingService;
		}

		public void Initialize(EPStageEventServiceSPI eventService)
		{
			this.internalEventRouteDest = eventService;
			this.metricReportingService.SetContext(filterService, schedulingService, eventService);
		}

		public IReaderWriterLock EventProcessingRWLock => eventProcessingRWLock;

		public FilterServiceSPI FilterServiceSPI => filterService;

		public FilterService FilterService => filterService;

		public DeploymentLifecycleService DeploymentLifecycleService => deploymentLifecycleService;

		public MetricReportingService MetricReportingService => metricReportingService;

		public SchedulingServiceSPI SchedulingServiceSPI => schedulingService;

		public SchedulingService SchedulingService => schedulingService;

		public VariableManagementService VariableManagementService => stageRuntimeServices.VariableManagementService;

		public ExceptionHandlingService ExceptionHandlingService => stageRuntimeServices.ExceptionHandlingService;

		public TableExprEvaluatorContext TableExprEvaluatorContext => stageRuntimeServices.TableExprEvaluatorContext;

		public PathRegistry<string, NamedWindowMetaData> NamedWindowPathRegistry => namedWindowPathRegistry;

		public InternalEventRouteDest InternalEventRouteDest => internalEventRouteDest;

		public PathRegistry<string, ContextMetaData> ContextPathRegistry => contextPathRegistry;

		public EventTypeResolvingBeanFactory EventTypeResolvingBeanFactory => stageRuntimeServices.EventTypeResolvingBeanFactory;

		public ThreadingService ThreadingService => threadingService;

		public PathRegistry<string, ExpressionDeclItem> ExprDeclaredPathRegistry => exprDeclaredPathRegistry;

		public PathRegistry<string, EventType> EventTypePathRegistry => eventTypesPathRegistry;

		public PathRegistry<NameAndParamNum, ExpressionScriptProvided> ScriptPathRegistry => scriptPathRegistry;

		public PathRegistry<string, TableMetaData> TablePathRegistry => tablesPathRegistry;

		public PathRegistry<string, VariableMetaData> VariablePathRegistry => variablesPathRegistry;

		public InternalEventRouter InternalEventRouter => internalEventRouter;

		public PathRegistry<string, ClassProvided> ClassProvidedPathRegistry => classProvidedPathRegistry;

		public void Destroy()
		{
			filterService.Destroy();
			schedulingService.Dispose();
			threadingService.Dispose();
			metricReportingService.Dispose();
		}
	}
} // end of namespace
