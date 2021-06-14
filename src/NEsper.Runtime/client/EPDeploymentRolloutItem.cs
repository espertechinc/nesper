///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.runtime.client
{
	/// <summary>
	///     The result item of a rollout as described in {@link EPDeploymentService#rollout(Collection, RolloutOptions)},
	///     captures the rollout result of a single compilation unit that was deployed as part of a rollout.
	/// </summary>
	public class EPDeploymentRolloutItem
    {
	    /// <summary>
	    ///     Ctor.
	    /// </summary>
	    /// <param name="deployment">deployment</param>
	    public EPDeploymentRolloutItem(EPDeployment deployment)
        {
            Deployment = deployment;
        }

	    /// <summary>
	    ///     Returns the deployment.
	    /// </summary>
	    /// <value>deployment</value>
	    public EPDeployment Deployment { get; }
    }
} // end of namespace