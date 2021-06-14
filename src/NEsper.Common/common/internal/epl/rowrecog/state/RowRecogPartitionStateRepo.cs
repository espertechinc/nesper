///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.rowrecog.core;

namespace com.espertech.esper.common.@internal.epl.rowrecog.state
{
    /// <summary>
    ///     Service for holding partition state.
    /// </summary>
    public interface RowRecogPartitionStateRepo
    {
        int StateCount { get; }

        RowRecogPartitionStateRepoScheduleState ScheduleState { get; }

        /// <summary>
        ///     Return state for key or create state if not found.
        /// </summary>
        /// <param name="key">to look up</param>
        /// <returns>state</returns>
        RowRecogPartitionState GetState(object key);

        /// <summary>
        ///     Return state for event or create state if not found.
        /// </summary>
        /// <param name="theEvent">to look up</param>
        /// <param name="isCollect">true if a collection of unused state can occur</param>
        /// <returns>state</returns>
        RowRecogPartitionState GetState(
            EventBean theEvent,
            bool isCollect);

        /// <summary>
        ///     Remove old events from the state, applicable for "prev" function and partial NFA state.
        /// </summary>
        /// <param name="events">to remove</param>
        /// <param name="IsEmpty">indicator if there are not matches</param>
        /// <param name="isEmpty"></param>
        /// <param name="found">indicator if any partial matches exist to be deleted</param>
        /// <returns>number removed</returns>
        int RemoveOld(
            EventBean[] events,
            bool isEmpty,
            bool[] found);

        /// <summary>
        ///     Copy state for iteration.
        /// </summary>
        /// <param name="forOutOfOrderReprocessing">indicator whether we are processing out-of-order events</param>
        /// <returns>copied state</returns>
        RowRecogPartitionStateRepo CopyForIterate(bool forOutOfOrderReprocessing);

        void RemoveState(object partitionKey);

        void Accept(RowRecogNFAViewServiceVisitor visitor);

        bool IsPartitioned { get; }

        int IncrementAndGetEventSequenceNum();

        int EventSequenceNum { set; }

        void Destroy();
    }
} // end of namespace