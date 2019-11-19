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
    ///     Context partition state event indicating a context partition allocated.
    /// </summary>
    public class ContextStateEventContextPartitionAllocated : ContextStateEventContextPartition
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="runtimeURI">runtime URI</param>
        /// <param name="contextDeploymentId">deployment id of create-context statement</param>
        /// <param name="contextName">context name</param>
        /// <param name="id">context partition id</param>
        /// <param name="identifier">identifier</param>
        public ContextStateEventContextPartitionAllocated(
            string runtimeURI,
            string contextDeploymentId,
            string contextName,
            int id,
            ContextPartitionIdentifier identifier)
            : base(runtimeURI, contextDeploymentId, contextName, id)
        {
            Identifier = identifier;
        }

        /// <summary>
        ///     Returns the identifier; For nested context the identifier is the identifier of the last or innermost context.
        /// </summary>
        /// <value>identifier</value>
        public ContextPartitionIdentifier Identifier { get; }
    }
} // end of namespace