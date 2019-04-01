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
    ///     Context partition state event.
    /// </summary>
    public abstract class ContextStateEventContextPartition : ContextStateEvent
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="runtimeURI">runtime URI</param>
        /// <param name="contextDeploymentId">deployment id of create-context statement</param>
        /// <param name="contextName">context name</param>
        /// <param name="id">context partition id</param>
        public ContextStateEventContextPartition(
            string runtimeURI, string contextDeploymentId, string contextName, int id)
            : base(runtimeURI, contextDeploymentId, contextName)
        {
            Id = id;
        }

        /// <summary>
        ///     Returns the context partition id
        /// </summary>
        /// <value>id</value>
        public int Id { get; }
    }
} // end of namespace