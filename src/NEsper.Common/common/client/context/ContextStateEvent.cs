///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.context
{
    /// <summary>
    ///     Context state event.
    /// </summary>
    public abstract class ContextStateEvent
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="runtimeURI">runtime URI</param>
        /// <param name="contextDeploymentId">deployment id of create-context statement</param>
        /// <param name="contextName">context name</param>
        public ContextStateEvent(
            string runtimeURI,
            string contextDeploymentId,
            string contextName)
        {
            RuntimeURI = runtimeURI;
            ContextDeploymentId = contextDeploymentId;
            ContextName = contextName;
        }

        /// <summary>
        ///     Returns the context name
        /// </summary>
        /// <returns>context name</returns>
        public string ContextName { get; }

        /// <summary>
        ///     Returns the deployment id
        /// </summary>
        /// <returns>deployment id</returns>
        public string ContextDeploymentId { get; }

        /// <summary>
        ///     Returns the runtime URI
        /// </summary>
        /// <returns>runtime URI</returns>
        public string RuntimeURI { get; }
    }
} // end of namespace