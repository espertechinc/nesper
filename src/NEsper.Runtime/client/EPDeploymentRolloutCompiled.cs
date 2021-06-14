///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;

namespace com.espertech.esper.runtime.client
{
	/// <summary>
	///     For use with rollout as described in {@link EPDeploymentService#rollout(Collection, RolloutOptions)},
	///     for passing a compilation unit and the deployment options for the compilation unit.
	/// </summary>
	public class EPDeploymentRolloutCompiled
    {
	    /// <summary>
	    ///     Ctor, assumes default deployment options
	    /// </summary>
	    /// <param name="compiled">compiled module to deploy</param>
	    public EPDeploymentRolloutCompiled(EPCompiled compiled)
        {
            Compiled = compiled;
            Options = new DeploymentOptions();
        }

	    /// <summary>
	    ///     Ctor.
	    /// </summary>
	    /// <param name="compiled">compiled module to deploy</param>
	    /// <param name="options">deployment options</param>
	    public EPDeploymentRolloutCompiled(
            EPCompiled compiled,
            DeploymentOptions options)
        {
            Compiled = compiled;
            Options = options ?? new DeploymentOptions();
        }

	    /// <summary>
	    ///     Returns the compiled module.
	    /// </summary>
	    /// <value>compiled module</value>
	    public EPCompiled Compiled { get; }

	    /// <summary>
	    ///     Returns the deployment options
	    /// </summary>
	    /// <value>deployment options</value>
	    public DeploymentOptions Options { get; }
    }
} // end of namespace