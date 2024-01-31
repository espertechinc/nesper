///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.runtime.client
{
    /// <summary>
    /// Uneploy exception to indicate that a precondition is not satisfied
    /// </summary>
    public class EPUndeployPreconditionException : EPUndeployException
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="message">message</param>
        public EPUndeployPreconditionException(string message) :
            base("A precondition is not satisfied: " + message)
        {
        }
    }
} // end of namespace