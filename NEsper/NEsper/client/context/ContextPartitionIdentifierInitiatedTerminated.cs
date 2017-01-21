///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.core.context.mgr;

namespace com.espertech.esper.client.context
{
    /// <summary>Context partition identifier for overlapping and non-overlapping contexts. </summary>
    public class ContextPartitionIdentifierInitiatedTerminated : ContextPartitionIdentifier
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContextPartitionIdentifierInitiatedTerminated"/> class.
        /// </summary>
        public ContextPartitionIdentifierInitiatedTerminated()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextPartitionIdentifierInitiatedTerminated"/> class.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <param name="startTime">The start time.</param>
        /// <param name="endTime">The end time.</param>
        public ContextPartitionIdentifierInitiatedTerminated(IDictionary<string, object> properties, long startTime, long? endTime)
        {
            Properties = properties;
            StartTime = startTime;
            EndTime = endTime;
        }

        /// <summary>Event or pattern information. </summary>
        /// <value>starting or initiating information</value>
        public IDictionary<string, object> Properties { get; set; }

        /// <summary>Returns the start time of the context partition. </summary>
        /// <value>start time</value>
        public long StartTime { get; set; }

        /// <summary>Returns the end time of the context partition, if it can be computed </summary>
        /// <value>end time</value>
        public long? EndTime { get; set; }

        public override bool CompareTo(ContextPartitionIdentifier other)
        {
            if (!(other is ContextPartitionIdentifierInitiatedTerminated))
            {
                return false;
            }
            var ito = (ContextPartitionIdentifierInitiatedTerminated) other;
            return ContextControllerInitTerm.Compare(
                StartTime, Properties, EndTime, ito.StartTime, ito.Properties, ito.EndTime);
        }

        public override string ToString()
        {
            return "ContextPartitionIdentifierInitiatedTerminated{" +
                            "properties=" + Properties +
                            ", startTime=" + StartTime +
                            ", endTime=" + EndTime +
                            '}';
        }
    }
}