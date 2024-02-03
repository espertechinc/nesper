///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.context
{
    /// <summary>
    ///     Context state event indicating a context activated.
    /// </summary>
    public class ContextStateEventContextActivated : ContextStateEvent
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="runtimeURI">runtime URI</param>
        /// <param name="contextDeploymentId">deployment id of create-context statement</param>
        /// <param name="contextName">context name</param>
        public ContextStateEventContextActivated(
            string runtimeURI,
            string contextDeploymentId,
            string contextName)
            :
            base(runtimeURI, contextDeploymentId, contextName)
        {
        }
    }
} // end of namespace