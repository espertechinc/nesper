///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.runtime.@internal.kernel.service;

namespace com.espertech.esper.runtime.@internal.kernel.stage
{
	public interface StageRecoveryService
	{
		int StageAdd(string stageUri);
		IEnumerator<KeyValuePair<string, int>> StagesIterate();

		void StageDestroy(
			string stageUri,
			int stageId);

		string DeploymentGetStage(string deploymentId);

		void DeploymentSetStage(
			string deploymentId,
			string stageUri);

		void DeploymentRemoveFromStages(string deploymentId);

		StageSpecificServices MakeSpecificServices(
			int stageId,
			string stageUri,
			EPServicesContext servicesContext);

		EPStageEventServiceSPI MakeEventService(
			StageSpecificServices stageSpecificServices,
			int stageId,
			string stageUri,
			EPServicesContext servicesContext);
	}
} // end of namespace
