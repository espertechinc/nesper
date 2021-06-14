///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.Serialization;

namespace com.espertech.esper.runtime.client
{
	/// <summary>
	///     Deploy exception to indicate that the compiler version mismatches
	/// </summary>
	[Serializable]
	public class EPDeployDeploymentVersionException : EPDeployException
    {
	    /// <summary>
	    ///     Ctor.
	    /// </summary>
	    /// <param name="message">message</param>
	    /// <param name="ex">exception</param>
	    /// <param name="rolloutItemNumber">rollout item number when using rollout</param>
	    public EPDeployDeploymentVersionException(
            string message,
            Exception ex,
            int rolloutItemNumber)
			: base(message, ex, rolloutItemNumber)
        {
        }

	    /// <summary>
	    /// Deserialization constructor.
	    /// </summary>
	    /// <param name="info"></param>
	    /// <param name="context"></param>
	    protected EPDeployDeploymentVersionException(
		    SerializationInfo info,
		    StreamingContext context) 
		    : base(info, context)
	    {
	    }
    }
} // end of namespace