///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading.locks;
using com.espertech.esper.container;
using com.espertech.esper.runtime.@internal.deploymentlifesvc;
using com.espertech.esper.runtime.@internal.filtersvcimpl;
using com.espertech.esper.runtime.@internal.kernel.service;
using com.espertech.esper.runtime.@internal.kernel.thread;
using com.espertech.esper.runtime.@internal.metrics.stmtmetrics;
using com.espertech.esper.runtime.@internal.schedulesvcimpl;

namespace com.espertech.esper.runtime.@internal.kernel.stage
{
	public abstract class StageRecoveryServiceBase : StageRecoveryService
	{
		protected abstract FilterServiceSPI MakeFilterService(
			int stageId,
			EPServicesContext servicesContext);

		protected abstract SchedulingServiceSPI MakeSchedulingService(
			int stageId,
			EPServicesContext servicesContext);

		public abstract int StageAdd(string stageUri);
		public abstract IEnumerator<KeyValuePair<string, int>> StagesIterate();

		public abstract void StageDestroy(
			string stageUri,
			int stageId);

		public abstract string DeploymentGetStage(string deploymentId);

		public abstract void DeploymentSetStage(
			string deploymentId,
			string stageUri);

		public abstract void DeploymentRemoveFromStages(string deploymentId);

		public abstract EPStageEventServiceSPI MakeEventService(
			StageSpecificServices stageSpecificServices,
			int stageId,
			string stageUri,
			EPServicesContext servicesContext);

		public StageSpecificServices MakeSpecificServices(
			int stageId,
			string stageUri,
			EPServicesContext servicesContext)
		{
			var rwLockManager = servicesContext.Container.RWLockManager();
			
			IReaderWriterLock eventProcessingRWLock;
			if (servicesContext.ConfigSnapshot.Runtime.Threading.IsRuntimeFairlock) {
				eventProcessingRWLock = new FairReaderWriterLock();
			}
			else {
				eventProcessingRWLock = rwLockManager.CreateLock(GetType());
			}
			
			var filterService = MakeFilterService(stageId, servicesContext);
			var schedulingService = MakeSchedulingService(stageId, servicesContext);
			var deploymentLifecycleService = new DeploymentLifecycleServiceImpl(stageId);
			var threadingService = new ThreadingServiceImpl(servicesContext.ConfigSnapshot.Runtime.Threading);
			var metricsReporting = new MetricReportingServiceImpl(servicesContext.ConfigSnapshot.Runtime.MetricsReporting, stageUri, rwLockManager);
			var internalEventRouter = new InternalEventRouterImpl(servicesContext.EventBeanTypedEventFactory);

			return new StageSpecificServices(
				deploymentLifecycleService,
				eventProcessingRWLock,
				filterService,
				internalEventRouter,
				metricsReporting,
				schedulingService,
				servicesContext.StageRuntimeServices,
				threadingService);
		}
	}
} // end of namespace
