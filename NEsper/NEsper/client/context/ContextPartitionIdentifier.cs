///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client.context
{
    /// <summary>
    /// Context partition identifiers are provided by the API when interrogating context partitions for a given statement.
    /// </summary>
    [Serializable]
    public abstract class ContextPartitionIdentifier
    {
        /// <summary>Compare identifiers returning a bool indicator whether identifier information matches. </summary>
        /// <param name="identifier">to compare to</param>
        /// <returns>true for objects identifying the same context partition (could be different context)</returns>
        public abstract bool CompareTo(ContextPartitionIdentifier identifier);

        /// <summary>Returns the context partition id. </summary>
        /// <value>context partition id</value>
        public int? ContextPartitionId { get; set; }
    }
}
