///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.context;
using com.espertech.esper.common.@internal.context.mgr;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.statement.resource;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.stage;
using com.espertech.esper.runtime.@internal.deploymentlifesvc;
using com.espertech.esper.runtime.@internal.kernel.service;
using com.espertech.esper.runtime.@internal.kernel.statement;

using static com.espertech.esper.runtime.@internal.kernel.stage.StageDeploymentHelper;
using static com.espertech.esper.runtime.@internal.kernel.stage.StagePreconditionHelper;
using static com.espertech.esper.runtime.@internal.kernel.stage.StageStatementHelper;

namespace com.espertech.esper.runtime.@internal.kernel.stage
{
	public class EPStageImpl : EPStageSPI
	{
		private readonly string _stageUri;
		private readonly int _stageId;
		private readonly EPServicesContext _servicesContext;
		private readonly StageSpecificServices _stageSpecificServices;
		private readonly EPStageEventServiceSPI _eventServiceStage;
		private readonly EPStageDeploymentServiceImpl _deploymentServiceStage;
		private readonly DestroyCallback _stageDestroyCallback;
		private bool _destroyed;

		public EPStageImpl(
			string stageUri,
			int stageId,
			EPServicesContext servicesContext,
			StageSpecificServices stageSpecificServices,
			EPStageEventServiceSPI eventServiceStage,
			EPStageDeploymentServiceImpl deploymentServiceStage,
			DestroyCallback stageDestroyCallback)
		{
			_stageUri = stageUri;
			_stageId = stageId;
			_servicesContext = servicesContext;
			_stageSpecificServices = stageSpecificServices;
			_eventServiceStage = eventServiceStage;
			_deploymentServiceStage = deploymentServiceStage;
			_stageDestroyCallback = stageDestroyCallback;
		}

		public void Stage(ICollection<string> deploymentIdsProvided)
		{
			lock (this) {
				CheckDestroyed();
				ISet<string> deploymentIds = CheckArgument(deploymentIdsProvided);
				if (deploymentIds.IsEmpty()) {
					return;
				}

				CheckDeploymentIdsExist(deploymentIds, _servicesContext.DeploymentLifecycleService);
				ValidateDependencyPreconditions(deploymentIds, _servicesContext, _servicesContext.DeploymentLifecycleService);

				ISet<int> statementIds = new HashSet<int>();
				foreach (string deploymentId in deploymentIds) {
					DeploymentInternal deployment = _servicesContext.DeploymentLifecycleService.GetDeploymentById(deploymentId);
					TraverseContextPartitions(deployment, ProcessStage);
					TraverseStatements(deployment, statementContext => UpdateStatement(statementContext, _stageSpecificServices), statementIds);
					MovePath(deployment, _servicesContext, _stageSpecificServices);
					_servicesContext.DeploymentLifecycleService.RemoveDeployment(deploymentId);
					_stageSpecificServices.DeploymentLifecycleService.AddDeployment(deploymentId, deployment);
					_servicesContext.StageRecoveryService.DeploymentSetStage(deploymentId, _stageUri);
				}

				_servicesContext.SchedulingServiceSPI.Transfer(statementIds, _stageSpecificServices.SchedulingServiceSPI);
			}
		}

		public void Unstage(ICollection<string> deploymentIdsProvided)
		{
			lock (this) {
				CheckDestroyed();
				ISet<string> deploymentIds = CheckArgument(deploymentIdsProvided);
				if (deploymentIds.IsEmpty()) {
					return;
				}

				CheckDeploymentIdsExist(deploymentIds, _stageSpecificServices.DeploymentLifecycleService);
				ValidateDependencyPreconditions(deploymentIds, _stageSpecificServices, _stageSpecificServices.DeploymentLifecycleService);

				ISet<int> statementIds = new HashSet<int>();
				foreach (string deploymentId in deploymentIds) {
					var deployment = _stageSpecificServices.DeploymentLifecycleService.GetDeploymentById(deploymentId);
					TraverseContextPartitions(deployment, ProcessUnstage);
					TraverseStatements(deployment, statementContext => UpdateStatement(statementContext, _servicesContext), statementIds);
					MovePath(deployment, _stageSpecificServices, _servicesContext);
					_stageSpecificServices.DeploymentLifecycleService.RemoveDeployment(deploymentId);
					_servicesContext.DeploymentLifecycleService.AddDeployment(deploymentId, deployment);
					_servicesContext.StageRecoveryService.DeploymentRemoveFromStages(deploymentId);
				}

				_stageSpecificServices.SchedulingServiceSPI.Transfer(statementIds, _servicesContext.SchedulingServiceSPI);
			}
		}

		public EPStageDeploymentService DeploymentService {
			get {
				CheckDestroyed();
				return _deploymentServiceStage;
			}
		}

		public EPStageEventService EventService {
			get {
				CheckDestroyed();
				return _eventServiceStage;
			}
		}

		public EPStageEventServiceSPI EventServiceSPI {
			get {
				CheckDestroyed();
				return _eventServiceStage;
			}
		}

		public StageSpecificServices StageSpecificServices => _stageSpecificServices;

		public void Destroy()
		{
			lock (this) {
				if (_destroyed) {
					return;
				}

				if (!_stageSpecificServices.DeploymentLifecycleService.DeploymentMap.IsEmpty()) {
					throw new EPException("Failed to destroy stage '" + _stageUri + "': The stage has existing deployments");
				}

				DestroyNoCheck();
			}
		}

		public void DestroyNoCheck()
		{
			lock (this) {
				if (_destroyed) {
					return;
				}

				_stageSpecificServices.Destroy();
				_stageDestroyCallback.Destroy();
				_destroyed = true;
			}
		}

		public string URI => _stageUri;

		private void TraverseContextPartitions(
			DeploymentInternal deployment,
			Consumer<StatementResourceHolder> consumer)
		{
			foreach (EPStatement statement in deployment.Statements) {
				EPStatementSPI spi = (EPStatementSPI) statement;
				if (spi.StatementContext.ContextName == null) {
					StatementResourceHolder holder = spi.StatementContext.StatementCPCacheService.MakeOrGetEntryCanNull(-1, spi.StatementContext);
					consumer.Invoke(holder);
				}
				else {
					ContextRuntimeDescriptor contextDesc = spi.StatementContext.ContextRuntimeDescriptor;
					ContextManager contextManager = _servicesContext.ContextManagementService.GetContextManager(
						contextDesc.ContextDeploymentId,
						contextDesc.ContextName);
					ICollection<int> agentInstanceIds = contextManager.Realization.GetAgentInstanceIds(ContextPartitionSelectorAll.INSTANCE);
					foreach (int agentInstanceId in agentInstanceIds) {
						StatementResourceHolder holder =
							spi.StatementContext.StatementCPCacheService.MakeOrGetEntryCanNull(agentInstanceId, spi.StatementContext);
						consumer.Invoke(holder);
					}
				}
			}
		}

		private ISet<string> CheckArgument(ICollection<string> deploymentIds)
		{
			if (deploymentIds == null) {
				throw new ArgumentException("Null or empty deployment ids");
			}

			foreach (string deploymentId in deploymentIds) {
				if (deploymentId == null) {
					throw new ArgumentException("Null or empty deployment id");
				}
			}

			return new LinkedHashSet<string>(deploymentIds);
		}

		private void TraverseStatements(
			DeploymentInternal deployment,
			Consumer<StatementContext> consumer,
			ISet<int> statementIds)
		{
			foreach (EPStatement statement in deployment.Statements) {
				EPStatementSPI spi = (EPStatementSPI) statement;
				consumer.Invoke(spi.StatementContext);
				statementIds.Add(spi.StatementId);
			}
		}

		private void ProcessStage(StatementResourceHolder holder)
		{
			AgentInstanceTransferServices xfer = new AgentInstanceTransferServices(
				holder.AgentInstanceContext,
				_stageSpecificServices.FilterService,
				_stageSpecificServices.SchedulingService,
				_stageSpecificServices.InternalEventRouter);
			holder.AgentInstanceStopCallback.Transfer(xfer);
			if (holder.ContextManagerRealization != null) {
				holder.ContextManagerRealization.Transfer(xfer);
			}

			holder.AgentInstanceContext.EpStatementAgentInstanceHandle.StatementFilterVersion.StmtFilterVersion =
				_stageSpecificServices.FilterService.FiltersVersion;
		}

		private void ProcessUnstage(StatementResourceHolder holder)
		{
			AgentInstanceTransferServices xfer = new AgentInstanceTransferServices(
				holder.AgentInstanceContext,
				_servicesContext.FilterService,
				_servicesContext.SchedulingService,
				_servicesContext.InternalEventRouter);
			holder.AgentInstanceStopCallback.Transfer(xfer);
			if (holder.ContextManagerRealization != null) {
				holder.ContextManagerRealization.Transfer(xfer);
			}

			holder.AgentInstanceContext.EpStatementAgentInstanceHandle.StatementFilterVersion.StmtFilterVersion = _servicesContext.FilterService.FiltersVersion;
		}

		private void CheckDeploymentIdsExist(
			ISet<string> deploymentIds,
			DeploymentLifecycleService deploymentLifecycleService)
		{
			foreach (string deploymentId in deploymentIds) {
				DeploymentInternal deployment = deploymentLifecycleService.GetDeploymentById(deploymentId);
				if (deployment == null) {
					throw new EPStageException("Deployment '" + deploymentId + "' was not found");
				}
			}
		}

		private void CheckDestroyed()
		{
			if (_destroyed) {
				throw new EPStageDestroyedException("Stage '" + _stageUri + "' is destroyed");
			}
		}

		public int StageId => _stageId;
	}
} // end of namespace
