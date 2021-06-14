///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.runtime.client.stage
{
	/// <summary>
	///     A stage allows staging and unstageing deployments, allowing independent control over event and time for the deployments.
	///     <para>
	///         This API is under development for version 8.4 and newer, and is considered UNSTABLE.
	///     </para>
	/// </summary>
	public interface EPStage
    {
	    /// <summary>
	    ///     Returns the stage's deployment service that provides information about staged deployments.
	    /// </summary>
	    /// <value>stage deployment service</value>
	    /// <throws>EPStageDestroyedException if the stage is already destroyed</throws>
	    EPStageDeploymentService DeploymentService { get; }

	    /// <summary>
	    ///     Returns the stage's event service that can be used to send events to the stage and to advance time for the stage.
	    /// </summary>
	    /// <value>stage event service</value>
	    /// <throws>EPStageDestroyedException if the stage is already destroyed</throws>
	    EPStageEventService EventService { get; }

	    /// <summary>
	    ///     Returns the stage unique identifier URI.
	    /// </summary>
	    /// <value>uri</value>
	    string URI { get; }

	    /// <summary>
	    ///     Stage deployments.
	    ///     <para>
	    ///         This effectively removes the deployment from the runtime and adds it to the stage's deployments.
	    ///         The deployment can be obtained from <seealso cref="EPStageDeploymentService" /> and can no longer be obtained from {@link
	    ///         EPRuntime#getDeploymentService()}.
	    ///     </para>
	    ///     <para>
	    ///         The staged deployments only receive events that the application sends using the <seealso cref="EPStageEventService" /> for this stage.
	    ///         The staged deployments only advance time according to the application advancing time using the <seealso cref="EPStageEventService" /> for this stage.
	    ///     </para>
	    ///     <para>
	    ///         The staged deployments no longer receive events that the application sends into the runtime {@link EPRuntime#getEventService()}.
	    ///         The staged deployments no longer advance time according to time advancing for the runtime {@link EPRuntime#getEventService()}.
	    ///     </para>
	    ///     <para>
	    ///         Requires that dependent public or protected (not preconfigured) EPL objects are also getting staged.
	    ///     </para>
	    /// </summary>
	    /// <param name="deploymentIds">deployment ids of deployments to stage</param>
	    /// <throws>EPStageException if preconditions validation fails or a deployment does not exist</throws>
	    /// <throws>EPStageDestroyedException if the stage is already destroyed</throws>
	    void Stage(ICollection<string> deploymentIds);

	    /// <summary>
	    ///     Un-stage deployments.
	    ///     <para>
	    ///         This effectively removes the deployment from the stage and adds it to the runtime deployments.
	    ///         The deployment can be obtained from {@link EPRuntime#getDeploymentService()} and can no longer be obtained from
	    ///         <seealso cref="EPStageDeploymentService" />.
	    ///     </para>
	    ///     <para>
	    ///         The un-staged deployments only receive events that the application sends using the runtime {@link EPRuntime#getEventService()}.
	    ///         The un-staged deployments only advance time according to the application advancing time using the runtime {@link EPRuntime#getEventService()}.
	    ///     </para>
	    ///     <para>
	    ///         The staged deployments no longer receive events that the application sends into the <seealso cref="EPStageEventService" /> for this stage.
	    ///         The staged deployments no longer advance time according to time advancing for the <seealso cref="EPStageEventService" /> for this stage.
	    ///     </para>
	    ///     <para>
	    ///         Requires that dependent public or protected (not preconfigured) EPL objects are also getting un-staged.
	    ///     </para>
	    /// </summary>
	    /// <param name="deploymentIds">deployment ids of deployments to un-stage</param>
	    /// <throws>EPStageException if preconditions validation fails or a deployment does not exist</throws>
	    /// <throws>EPStageDestroyedException if the stage is already destroyed</throws>
	    void Unstage(ICollection<string> deploymentIds);

	    /// <summary>
	    ///     Destroy the stage.
	    ///     <para>
	    ///         Requires that any deployments are un-staged.
	    ///     </para>
	    /// </summary>
	    /// <throws>EPException when the destroy operation fails</throws>
	    void Destroy();
    }
} // end of namespace