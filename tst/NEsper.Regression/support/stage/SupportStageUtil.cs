///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client.stage;

namespace com.espertech.esper.regressionlib.support.stage
{
	public class SupportStageUtil
	{
		public static void StageIt(
			RegressionEnvironment env,
			string stageUri,
			params string[] deploymentIds)
		{
			var stage = CheckStage(env, stageUri);
			try {
				stage.Stage(Arrays.AsList(deploymentIds));
			}
			catch (EPStageException ex) {
				throw new EPRuntimeException(ex);
			}
		}

		public static void UnstageIt(
			RegressionEnvironment env,
			string stageUri,
			params string[] deploymentIds)
		{
			var stage = CheckStage(env, stageUri);
			try {
				stage.Unstage(Arrays.AsList(deploymentIds));
			}
			catch (EPStageException ex) {
				throw new EPRuntimeException(ex);
			}
		}

		private static EPStage CheckStage(
			RegressionEnvironment env,
			string stageUri)
		{
			var stage = env.StageService.GetExistingStage(stageUri);
			if (stage == null) {
				throw new EPRuntimeException("Failed to find stage '" + stageUri + "'");
			}

			return stage;
		}
	}
} // end of namespace
