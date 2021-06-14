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
	///     Deploy exception to indicate that a deployment by the same deployment id already exists
	/// </summary>
	public class EPDeployDeploymentExistsException : EPDeployException
	{
		/// <summary>
		///     Ctor.
		/// </summary>
		/// <param name="message">message</param>
		/// <param name="rolloutItemNumber">rollout item number when using rollout</param>
		public EPDeployDeploymentExistsException(
			string message,
			int rolloutItemNumber)
			: base(message, rolloutItemNumber)
		{
		}
	}
} // end of namespace