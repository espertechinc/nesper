///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.runtime.client.stage
{
	/// <summary>
	/// Stage deployment service provides information about deployments staged to a given <seealso cref="EPStage" />.
	/// <para>This API is under development for version 8.4 and newer, and is considered UNSTABLE.</para>
	/// </summary>
	public interface EPStageDeploymentService {

	    /// <summary>
	    /// Returns the staged deployment or null if the deployment is not staged
	    /// </summary>
	    /// <param name="deploymentId">deployment id</param>
	    /// <returns>deployment id</returns>
	    EPDeployment GetDeployment(string deploymentId);

	    /// <summary>
	    /// Returns the deployment ids of all staged deployments.
	    /// </summary>
	    /// <value>deployment ids</value>
	    string[] Deployments { get; }
	}
} // end of namespace
