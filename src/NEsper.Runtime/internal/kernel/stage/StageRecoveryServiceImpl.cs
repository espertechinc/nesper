///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;
using com.espertech.esper.runtime.@internal.filtersvcimpl;
using com.espertech.esper.runtime.@internal.kernel.service;
using com.espertech.esper.runtime.@internal.schedulesvcimpl;

namespace com.espertech.esper.runtime.@internal.kernel.stage
{
	public class StageRecoveryServiceImpl : StageRecoveryServiceBase,
		StageRecoveryService
	{
		public static readonly StageRecoveryServiceImpl INSTANCE = new StageRecoveryServiceImpl();

		private int _currentStageNumber;
		private IDictionary<string, string> _deploymentIdStages;

		private StageRecoveryServiceImpl()
		{
		}

		public override EPStageEventServiceSPI MakeEventService(
			StageSpecificServices stageSpecificServices,
			int stageId,
			string stageUri,
			EPServicesContext servicesContext)
		{
			return new EPStageEventServiceImpl(stageSpecificServices, servicesContext.StageRuntimeServices, stageUri);
		}

		public override string DeploymentGetStage(string deploymentId)
		{
			return _deploymentIdStages?.Get(deploymentId);
		}

		public override IEnumerator<KeyValuePair<string, int>> StagesIterate()
		{
			// no action
			return EnumerationHelper.Empty<KeyValuePair<string, int>>();
		}

		public override int StageAdd(string stageUri)
		{
			return ++_currentStageNumber;
		}

		public override void StageDestroy(
			string stageUri,
			int stageId)
		{
			// no action
		}

		public override void DeploymentSetStage(
			string deploymentId,
			string stageUri)
		{
			InitDeploymentStages();
			_deploymentIdStages.Put(deploymentId, stageUri);
		}

		public override void DeploymentRemoveFromStages(string deploymentId)
		{
			InitDeploymentStages();
			_deploymentIdStages.Remove(deploymentId);
			// no action
		}

		protected override FilterServiceSPI MakeFilterService(
			int stageId,
			EPServicesContext servicesContext)
		{
			return new FilterServiceLockCoarse(servicesContext.Container.RWLockManager(), stageId);
		}

		protected override SchedulingServiceSPI MakeSchedulingService(
			int stageId,
			EPServicesContext servicesContext)
		{
			return new SchedulingServiceImpl(
				stageId,
				new ProxyTimeSourceService(() => servicesContext.SchedulingService.Time + 1));
		}

		private void InitDeploymentStages()
		{
			if (_deploymentIdStages == null) {
				_deploymentIdStages = new Dictionary<string, string>();
			}
		}
	}
} // end of namespace
