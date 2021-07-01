///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.common.client.context
{
    /// <summary>
    ///     A collection of context partitions each uniquely identified by a context partition id (agent instance id).
    /// </summary>
    public class ContextPartitionCollection
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="identifiers">per agent instance id</param>
        public ContextPartitionCollection(IDictionary<int, ContextPartitionIdentifier> identifiers)
        {
            Identifiers = identifiers;
        }

        /// <summary>
        ///     Returns the identifiers per agent instance id
        /// </summary>
        /// <value>descriptors</value>
        public IDictionary<int, ContextPartitionIdentifier> Identifiers { get; }
    }
} // end of namespace