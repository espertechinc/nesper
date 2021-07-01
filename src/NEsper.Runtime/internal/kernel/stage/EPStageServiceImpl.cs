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
		private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly EPServicesContext _services;
		private readonly AtomicBoolean _serviceStatusProvider;
		private readonly IDictionary<string, EPStageImpl> _stages = new Dictionary<string, EPStageImpl>();

		public EPStageServiceImpl(
			EPServicesContext services,
			AtomicBoolean serviceStatusProvider)
		{
			_services = services;
			_serviceStatusProvider = serviceStatusProvider;
		}

		public EPStage GetStage(string stageUri)
		{
			if (stageUri == null) {
				throw new ArgumentException("Stage-URI is null");
			}

			RuntimeDestroyedCheck();
			lock (_stages) {
				EPStageImpl stage = _stages.Get(stageUri);
				if (stage != null) {
					return stage;
				}

				int stageId = _services.StageRecoveryService.StageAdd(stageUri);
				stage = AllocateStage(stageUri, stageId, _services.SchedulingService.Time);
				_stages.Put(stageUri, stage);

				ISet<EventType> filterServiceTypes = new LinkedHashSet<EventType>(_services.EventTypeRepositoryBus.AllTypes);
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
			lock (_stages) {
				return _stages.Get(stageUri);
			}
		}

		public string[] StageURIs {
			get {
				RuntimeDestroyedCheck();
				lock (_stages) {
					return _stages.Keys.ToArray();
				}
			}
		}

		public void Clear()
		{
			_stages.Clear();
		}

		public void RecoverStage(
			string stageURI,
			int stageId,
			long stageCurrentTime)
		{
			EPStageImpl stage = AllocateStage(stageURI, stageId, stageCurrentTime);
			_stages.Put(stageURI, stage);
		}

		public void RecoverDeployment(
			string stageUri,
			DeploymentInternal deployment)
		{
			string deploymentId = deployment.DeploymentId;
			_services.DeploymentLifecycleService.RemoveDeployment(deploymentId);
			_stages.Get(stageUri).StageSpecificServices.DeploymentLifecycleService.AddDeployment(deploymentId, deployment);

			StageSpecificServices stageSpecificServices = _stages.Get(stageUri).EventServiceSPI.SpecificServices;
			foreach (EPStatement statement in deployment.Statements) {
				EPStatementSPI spi = (EPStatementSPI) statement;
				UpdateStatement(spi.StatementContext, stageSpecificServices);

				if (Equals(spi.GetProperty(StatementProperty.STATEMENTTYPE), StatementType.UPDATE)) {
					_services.InternalEventRouter.MovePreprocessing(spi.StatementContext, stageSpecificServices.InternalEventRouter);
				}
			}
		}

		public void RecoveredStageInitialize(Supplier<ICollection<EventType>> availableTypes)
		{
			foreach (KeyValuePair<string, EPStageImpl> stage in _stages) {
				stage.Value.EventServiceSPI.SpecificServices.FilterServiceSPI.Init(availableTypes);
				stage.Value.EventServiceSPI.SpecificServices.SchedulingServiceSPI.Init();
			}
		}

		public bool IsEmpty()
		{
			return _stages.IsEmpty();
		}

		public IDictionary<string, EPStageImpl> Stages {
			get { return _stages; }
		}

		public void Destroy()
		{
			EPStageImpl[] stageArray = _stages.Values.ToArray();
			foreach (EPStageImpl stageEntry in stageArray) {
				try {
					stageEntry.DestroyNoCheck();
				}
				catch (Exception t) {
					Log.Error("Failed to destroy stage: " + t.Message, t);
				}
			}
		}

		private EPStageImpl AllocateStage(
			string stageUri,
			int stageId,
			long stageTime)
		{
			StageSpecificServices stageSpecificServices = _services.StageRecoveryService.MakeSpecificServices(stageId, stageUri, _services);
			EPStageEventServiceSPI eventService = _services.StageRecoveryService.MakeEventService(stageSpecificServices, stageId, stageUri, _services);
			stageSpecificServices.Initialize(eventService);
			eventService.InternalEventRouter = stageSpecificServices.InternalEventRouter;

			eventService.SpecificServices.SchedulingService.Time = stageTime;
			EPStageDeploymentServiceImpl deploymentService = new EPStageDeploymentServiceImpl(stageUri, _services, eventService.SpecificServices);
			DestroyCallback stageDestroyCallback = new ProxyDestroyCallback() {
				ProcDestroy = () => {
					lock (_stages) {
						_services.StageRecoveryService.StageDestroy(stageUri, stageId);
						_stages.Remove(stageUri);
					}
				},
			};
			return new EPStageImpl(stageUri, stageId, _services, eventService.SpecificServices, eventService, deploymentService, stageDestroyCallback);
		}

		private void RuntimeDestroyedCheck()
		{
			if (!_serviceStatusProvider.Get()) {
				throw new EPRuntimeDestroyedException("Runtime has already been destroyed");
			}
		}
	}
} // end of namespace
