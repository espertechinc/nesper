///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;

namespace com.espertech.esper.runtime.client.scopetest
{
    /// <summary>
    /// Listener interface for use in regression testing for asserting receive events.
    /// </summary>
    public interface SupportListener : UpdateListener
    {
        /// <summary>
        /// Returns a pair of insert and remove stream event arrays considering the last invocation only,
        /// asserting that only a single invocation occured, and resetting the listener.
        /// </summary>
        /// <returns>pair of event arrays, the first in the pair is the insert stream data, the second in the pair is the remove stream data</returns>
        UniformPair<EventBean[]> AssertInvokedAndReset();

        /// <summary>
        /// Assert the invoked-flag against the expected value and reset the flag
        /// </summary>
        /// <param name="expected">expected value</param>
        void AssertInvokedFlagAndReset(bool expected);

        /// <summary>
        /// Returns true if the listener was invoked at least once.
        /// </summary>
        /// <value>invoked flag</value>
        bool IsInvoked { get; }

        /// <summary>
        /// Asserts that exactly one insert stream event was received and no remove stream events, resets the listener clearing all state and returns the received event.
        /// </summary>
        /// <returns>single insert-stream event</returns>
        EventBean AssertOneGetNewAndReset();

        /// <summary>
        /// Asserts that exactly one insert stream event and exactly one remove stream event was received, resets the listener clearing all state and returns the received events as a pair.
        /// </summary>
        /// <returns>pair of insert-stream and remove-stream events</returns>
        UniformPair<EventBean> AssertPairGetIRAndReset();

        /// <summary>
        /// Returns true if the listener was invoked at least once and clears the invocation flag.
        /// </summary>
        /// <returns>invoked flag</returns>
        bool IsInvokedAndReset();

        /// <summary>
        /// Returns the last array of events (insert stream) that were received and resets the listener.
        /// </summary>
        /// <returns>insert stream events or null if either a null value was received or when no events have been received since the last reset</returns>
        EventBean[] GetAndResetLastNewData();

        /// <summary>
        /// Returns the last array of events (insert stream) that were received and resets the listener.
        /// </summary>
        /// <returns>insert stream events or null if either a null value was received or when no events have been received since the last reset</returns>
        EventBean[] GetAndResetLastOldData();

        /// <summary>
        /// Get a list of all insert-stream event arrays received.
        /// </summary>
        /// <returns>list of event arrays</returns>
        IList<EventBean[]> NewDataList { get; }

        /// <summary>
        /// Returns the last array of events (insert stream) that were received.
        /// </summary>
        /// <returns>insert stream events or null if either a null value was received or when no events have been received since the last reset</returns>
        EventBean[] LastNewData { get; }

        /// <summary>
        /// Reset listener, clearing all associated state.
        /// </summary>
        void Reset();

        /// <summary>
        /// Returns true if the listener was invoked at least once and clears the invocation flag.
        /// </summary>
        /// <returns>invoked flag</returns>
        bool GetAndClearIsInvoked();

        /// <summary>
        /// Returns the last array of remove-stream events that were received.
        /// </summary>
        /// <returns>remove stream events or null if either a null value was received or when no events have been received since the last reset</returns>
        EventBean[] LastOldData { get; }

        /// <summary>
        /// Returns an event array that represents all insert-stream events received so far.
        /// </summary>
        /// <value>event array</value>
        EventBean[] NewDataListFlattened { get; }

        /// <summary>
        /// Returns an event array that represents all remove-stream events received so far.
        /// </summary>
        /// <returns>event array</returns>
        EventBean[] OldDataListFlattened { get; }

        /// <summary>
        /// Get a list of all remove-stream event arrays received.
        /// </summary>
        /// <returns>list of event arrays</returns>
        IList<EventBean[]> OldDataList { get; }

        /// <summary>
        /// Returns a pair of insert and remove stream event arrays considering the all invocations, and resets the listener.
        /// </summary>
        /// <returns>pair of event arrays, the first in the pair is the insert stream data, the second in the pair is the remove stream data</returns>
        UniformPair<EventBean[]> GetAndResetDataListsFlattened();

        /// <summary>
        /// Asserts name-value pairs of insert and remove stream events
        /// </summary>
        /// <param name="nameAndValuePairsIStream">insert-stream assertions</param>
        /// <param name="nameAndValuePairsRStream">remove-stream assertions</param>
        void AssertNewOldData(object[][] nameAndValuePairsIStream, object[][] nameAndValuePairsRStream);

        /// <summary>
        /// Asserts that exactly one remove stream event was received and no insert stream events, resets the listener clearing all state and returns the received event.
        /// </summary>
        /// <returns>single remove-stream event</returns>
        EventBean AssertOneGetOldAndReset();

        /// <summary>
        /// Asserts that there is exactly one insert-stream event and one remove-stream event available and resets.
        /// </summary>
        /// <returns>pair of insert-stream event and remove-stream event</returns>
        UniformPair<EventBean> AssertGetAndResetIRPair();

        /// <summary>
        /// Returns a pair of last-invocation insert and remove stream events and resets
        /// </summary>
        /// <returns>pair of events</returns>
        UniformPair<EventBean[]> GetAndResetIRPair();

        /// <summary>
        /// Returns a pair of insert and remove stream event arrays considering the all invocations.
        /// </summary>
        /// <returns>pair of event arrays, the first in the pair is the insert stream data, the second in the pair is the remove stream data</returns>
        UniformPair<EventBean[]> DataListsFlattened { get; }

        /// <summary>
        /// Asserts that exactly one insert stream event was received not checking remove stream data, and returns the received event.
        /// </summary>
        /// <returns>single insert-stream event</returns>
        EventBean AssertOneGetNew();

        /// <summary>
        /// Asserts that exactly one remove stream event was received not checking insert stream data, and returns the received event.
        /// </summary>
        /// <returns>single remove-stream event</returns>
        EventBean AssertOneGetOld();

        /// <summary>
        /// Wait for the listener invocation for up to the given number of milliseconds.
        /// </summary>
        /// <param name="numberOfNewEvents">number of events to await</param>
        /// <param name="msecWait">to wait</param>
        /// <throws>RuntimeException when no results were received</throws>
        void WaitForInvocation(long msecWait, int numberOfNewEvents);
    }
} // end of namespace