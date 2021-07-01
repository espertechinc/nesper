///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.runtime.client.util;

namespace com.espertech.esper.runtime.client
{
	/// <summary>
	///     Option holder for use with <seealso cref="EPDeploymentService.Rollout" /> ()}.
	/// </summary>
	public class RolloutOptions
    {
	    /// <summary>
	    ///     Return the rollout lock strategy, the default is <seealso cref="LockStrategyDefault" />
	    /// </summary>
	    /// <value>lock strategy</value>
	    public LockStrategy RolloutLockStrategy { get; set; } = LockStrategyDefault.INSTANCE;
    }
} // end of namespace