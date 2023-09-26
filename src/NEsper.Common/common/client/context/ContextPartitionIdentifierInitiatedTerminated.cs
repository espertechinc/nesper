///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.context
{
    /// <summary>
    ///     Context partition identifier for overlapping and non-overlapping contexts.
    /// </summary>
    [Serializable]
    public class ContextPartitionIdentifierInitiatedTerminated : ContextPartitionIdentifier
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        public ContextPartitionIdentifierInitiatedTerminated()
        {
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="properties">of triggering object</param>
        /// <param name="startTime">start time</param>
        /// <param name="endTime">optional end time</param>
        public ContextPartitionIdentifierInitiatedTerminated(
            IDictionary<string, object> properties,
            long startTime,
            long? endTime)
        {
            Properties = properties;
            StartTime = startTime;
            EndTime = endTime;
        }

        /// <summary>
        ///     Returns the start time of the context partition.
        /// </summary>
        /// <returns>start time</returns>
        public long StartTime { get; set; }

        /// <summary>
        ///     Event or pattern information.
        /// </summary>
        /// <value>starting or initiating information</value>
        public IDictionary<string, object> Properties { get; set; }

        /// <summary>
        ///     Returns the end time of the context partition, if it can be computed
        /// </summary>
        /// <value>end time</value>
        public long? EndTime { get; set; }

        public override bool CompareTo(ContextPartitionIdentifier other)
        {
            if (!(other is ContextPartitionIdentifierInitiatedTerminated ito)) {
                return false;
            }

            return Compare(StartTime, Properties, EndTime, ito.StartTime, ito.Properties, ito.EndTime);
        }

        public override string ToString()
        {
            return "ContextPartitionIdentifierInitiatedTerminated{" +
                   "properties=" +
                   Properties +
                   ", startTime=" +
                   StartTime +
                   ", endTime=" +
                   EndTime +
                   '}';
        }

        private static bool Compare(
            long savedStartTime,
            IDictionary<string, object> savedProperties,
            long? savedEndTime,
            long existingStartTime,
            IDictionary<string, object> existingProperties,
            long? existingEndTime)
        {
            if (savedStartTime != existingStartTime) {
                return false;
            }

            if (savedEndTime != null && existingEndTime != null && !savedEndTime.Equals(existingEndTime)) {
                return false;
            }

            foreach (var savedEntry in savedProperties) {
                var existingValue = existingProperties.Get(savedEntry.Key);
                var savedValue = savedEntry.Value;
                if (savedValue == null && existingValue == null) {
                    continue;
                }

                if (savedValue == null || existingValue == null) {
                    return false;
                }

                if (existingValue.Equals(savedValue)) {
                    continue;
                }

                if (existingValue is EventBean bean && savedValue is EventBean eventBean) {
                    if (bean.Underlying.Equals(eventBean.Underlying)) {
                        continue;
                    }
                }

                return false;
            }

            return true;
        }
    }
} // end of namespace