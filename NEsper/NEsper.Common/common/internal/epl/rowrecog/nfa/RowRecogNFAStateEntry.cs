///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.rowrecog.core;

namespace com.espertech.esper.common.@internal.epl.rowrecog.nfa
{
    /// <summary>
    ///     State for a partial NFA match.
    /// </summary>
    public class RowRecogNFAStateEntry
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="matchBeginEventSeqNo">the event number where the match started</param>
        /// <param name="matchBeginEventTime">the time the first match occured</param>
        /// <param name="state">the current match state</param>
        /// <param name="eventsPerStream">events for each single-match variable</param>
        /// <param name="greedycountPerState">number of greedy matches over all variables</param>
        /// <param name="optionalMultiMatches">matches for multirow-variables</param>
        /// <param name="partitionKey">key of partition</param>
        public RowRecogNFAStateEntry(
            int matchBeginEventSeqNo,
            long matchBeginEventTime,
            RowRecogNFAState state,
            EventBean[] eventsPerStream,
            int[] greedycountPerState,
            RowRecogMultimatchState[] optionalMultiMatches,
            object partitionKey)
        {
            MatchBeginEventSeqNo = matchBeginEventSeqNo;
            MatchBeginEventTime = matchBeginEventTime;
            State = state;
            EventsPerStream = eventsPerStream;
            GreedycountPerState = greedycountPerState;
            OptionalMultiMatches = optionalMultiMatches;
            PartitionKey = partitionKey;
        }

        /// <summary>
        ///     Returns the event number of the first matching event.
        /// </summary>
        /// <returns>event number</returns>
        public int MatchBeginEventSeqNo { get; }

        /// <summary>
        ///     Returns the time of the first matching event.
        /// </summary>
        /// <returns>time</returns>
        public long MatchBeginEventTime { get; }

        /// <summary>
        ///     Returns the partial matches.
        /// </summary>
        /// <returns>state</returns>
        public RowRecogNFAState State { get; set; }

        /// <summary>
        ///     Returns the single-variable matches.
        /// </summary>
        /// <returns>match events</returns>
        public EventBean[] EventsPerStream { get; }

        /// <summary>
        ///     Returns the multirow-variable matches, if any.
        /// </summary>
        /// <returns>matches</returns>
        public RowRecogMultimatchState[] OptionalMultiMatches { get; }

        /// <summary>
        ///     Returns the count of greedy matches per state.
        /// </summary>
        /// <returns>greedy counts</returns>
        public int[] GreedycountPerState { get; }

        /// <summary>
        ///     Returns the match end event number.
        /// </summary>
        /// <returns>num</returns>
        public int MatchEndEventSeqNo { get; set; }

        /// <summary>
        ///     Returns the partition key.
        /// </summary>
        /// <returns>key</returns>
        public object PartitionKey { get; }

        public override string ToString()
        {
            return "Entry " + State;
        }
    }
} // end of namespace