///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.compat.logging;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.stage;
using com.espertech.esper.runtime.@internal.kernel.service;
using com.espertech.esper.runtime.@internal.kernel.statement;

using static com.espertech.esper.runtime.@internal.kernel.stage.StageStatementHelper;

namespace com.espertech.esper.runtime.@internal.kernel.stage
{
	public class EPStageServiceImpl : EPStageServiceSPI
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly EPServicesContext services;
		private readonly AtomicBoolean serviceStatusProvider;
		private readonly IDictionary<string, EPStageImpl> stages = new Dictionary<string, EPStageImpl>();

		public EPStageServiceImpl(
			EPServicesContext services,
			AtomicBoolean serviceStatusProvider)
		{
			this.services = services;
			this.serviceStatusProvider = serviceStatusProvider;
		}

		public EPStage GetStage(string stageUri)
		{
			if (stageUri == null) {
				throw new ArgumentException("Stage-URI is null");
			}

			RuntimeDestroyedCheck();
			lock (stages) {
				EPStageImpl stage = stages.Get(stageUri);
				if (stage != null) {
					return stage;
				}

				int stageId = services.StageRecoveryService.StageAdd(stageUri);
				stage = AllocateStage(stageUri, stageId, services.SchedulingService.Time);
				stages.Put(stageUri, stage);

				ISet<EventType> filterServiceTypes = new LinkedHashSet<EventType>(services.EventTypeRepositoryBus.AllTypes);
				Supplier<ICollection<EventType>> availableTypes = () => filterServiceTypes;
				stage.StageSpecificServices.FilterServiceSPI.Init(availableTypes);
				stage.StageSpecificServices.SchedulingServiceSPI.Init();

				return stage;
			}
		}

		public EPStage GetExistingStage(string stageUri)
		{
			if (stageUri == null) {
				throw new ArgumentException("Stage-URI is null");
			}

			RuntimeDestroyedCheck();
			lock (stages) {
				return stages.Get(stageUri);
			}
		}

		public string[] StageURIs {
			get {
				RuntimeDestroyedCheck();
				lock (stages) {
					return stages.Keys.ToArray();
				}
			}
		}

		public void Clear()
		{
			stages.Clear();
		}

		public void RecoverStage(
			string stageURI,
			int stageId,
			long stageCurrentTime)
		{
			EPStageImpl stage = AllocateStage(stageURI, stageId, stageCurrentTime);
			stages.Put(stageURI, stage);
		}

		public void RecoverDeployment(
			string stageUri,
			DeploymentInternal deployment)
		{
			string deploymentId = deployment.DeploymentId;
			services.DeploymentLifecycleService.RemoveDeployment(deploymentId);
			stages.Get(stageUri).StageSpecificServices.DeploymentLifecycleService.AddDeployment(deploymentId, deployment);

			StageSpecificServices stageSpecificServices = stages.Get(stageUri).EventServiceSPI.SpecificServices;
			foreach (EPStatement statement in deployment.Statements) {
				EPStatementSPI spi = (EPStatementSPI) statement;
				UpdateStatement(spi.StatementContext, stageSpecificServices);

				if (Equals(spi.GetProperty(StatementProperty.STATEMENTTYPE), StatementType.UPDATE)) {
					services.InternalEventRouter.MovePreprocessing(spi.StatementContext, stageSpecificServices.InternalEventRouter);
				}
			}
		}

		public void RecoveredStageInitialize(Supplier<ICollection<EventType>> availableTypes)
		{
			foreach (KeyValuePair<string, EPStageImpl> stage in stages) {
				stage.Value.EventServiceSPI.SpecificServices.FilterServiceSPI.Init(availableTypes);
				stage.Value.EventServiceSPI.SpecificServices.SchedulingServiceSPI.Init();
			}
		}

		public bool IsEmpty()
		{
			return stages.IsEmpty();
		}

		public IDictionary<string, EPStageImpl> Stages {
			get { return stages; }
		}

		public void Destroy()
		{
			EPStageImpl[] stageArray = stages.Values.ToArray();
			foreach (EPStageImpl stageEntry in stageArray) {
				try {
					stageEntry.DestroyNoCheck();
				}
				catch (Exception t) {
					log.Error("Failed to destroy stage: " + t.Message, t);
				}
			}
		}

		private EPStageImpl AllocateStage(
			string stageUri,
			int stageId,
			long stageTime)
		{
			StageSpecificServices stageSpecificServices = services.StageRecoveryService.MakeSpecificServices(stageId, stageUri, services);
			EPStageEventServiceSPI eventService = services.StageRecoveryService.MakeEventService(stageSpecificServices, stageId, stageUri, services);
			stageSpecificServices.Initialize(eventService);
			eventService.InternalEventRouter = stageSpecificServices.InternalEventRouter;

			eventService.SpecificServices.SchedulingService.Time = stageTime;
			EPStageDeploymentServiceImpl deploymentService = new EPStageDeploymentServiceImpl(stageUri, services, eventService.SpecificServices);
			DestroyCallback stageDestroyCallback = new ProxyDestroyCallback() {
				ProcDestroy = () => {
					lock (stages) {
						services.StageRecoveryService.StageDestroy(stageUri, stageId);
						stages.Remove(stageUri);
					}
				},
			};
			return new EPStageImpl(stageUri, stageId, services, eventService.SpecificServices, eventService, deploymentService, stageDestroyCallback);
		}

		private void RuntimeDestroyedCheck()
		{
			if (!serviceStatusProvider.Get()) {
				throw new EPRuntimeDestroyedException("Runtime has already been destroyed");
			}
		}
	}
} // end of namespace
