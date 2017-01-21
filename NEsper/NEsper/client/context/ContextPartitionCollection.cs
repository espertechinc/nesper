///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.client.context
{
    /// <summary>
    /// A collection of context partitions each uniquely identified by a context partition id (agent instance id).
    /// </summary>
    public class ContextPartitionCollection
    {
        /// <summary>Ctor. </summary>
        /// <param name="descriptors">per agent instance id</param>
        public ContextPartitionCollection(IDictionary<int, ContextPartitionDescriptor> descriptors)
        {
            Descriptors = descriptors;
        }

        /// <summary>Returns the descriptors per agent instance id </summary>
        /// <value>descriptors</value>
        public IDictionary<int, ContextPartitionDescriptor> Descriptors { get; private set; }
    }
}
