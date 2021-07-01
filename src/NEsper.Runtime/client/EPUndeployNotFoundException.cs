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
    /// Undeploy exception to indicate that the deployment was not found
    /// </summary>
    public class EPUndeployNotFoundException : EPUndeployException
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="message">message</param>
        public EPUndeployNotFoundException(string message) :
            base(message)
        {
        }
    }
} // end of namespace