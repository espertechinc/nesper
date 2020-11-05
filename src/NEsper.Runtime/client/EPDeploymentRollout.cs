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
	///     Contains the result of a rollout as described in {@link EPDeploymentService#rollout(Collection, RolloutOptions)},
	///     captures the rollout result wherein the deployment result of each compilation unit is provided by
	///     <seealso cref="EPDeploymentRolloutItem" />.
	/// </summary>
	public class EPDeploymentRollout
    {
	    /// <summary>
	    ///     Ctor.
	    /// </summary>
	    /// <param name="items">deployment items</param>
	    public EPDeploymentRollout(EPDeploymentRolloutItem[] items)
        {
            Items = items;
        }

	    /// <summary>
	    ///     Returns the deployment items
	    /// </summary>
	    /// <value>deployment items</value>
	    public EPDeploymentRolloutItem[] Items { get; }
    }
} // end of namespace