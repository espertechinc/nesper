///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;

namespace com.espertech.esper.rowregex
{
    /// <summary>
    /// State for a partial NFA match.
    /// </summary>
    public class RegexNFAStateEntry
    {
        /// <summary>Ctor. </summary>
        /// <param name="matchBeginEventSeqNo">the event number where the match started</param>
        /// <param name="matchBeginEventTime">the time the first match occured</param>
        /// <param name="state">the current match state</param>
        /// <param name="eventsPerStream">events for each single-match variable</param>
        /// <param name="greedyCountPerState">number of greedy matches over all variables</param>
        /// <param name="optionalMultiMatches">matches for multirow-variables</param>
        /// <param name="partitionKey">key of partition</param>
        public RegexNFAStateEntry(int matchBeginEventSeqNo, long matchBeginEventTime, RegexNFAState state, EventBean[] eventsPerStream, int[] greedyCountPerState, MultimatchState[] optionalMultiMatches, Object partitionKey)
        {
            MatchBeginEventSeqNo = matchBeginEventSeqNo;
            MatchBeginEventTime = matchBeginEventTime;
            State = state;
            EventsPerStream = eventsPerStream;
            GreedyCountPerState = greedyCountPerState;
            OptionalMultiMatches = optionalMultiMatches;
            PartitionKey = partitionKey;
        }

        /// <summary>Returns the event number of the first matching event. </summary>
        /// <value>event number</value>
        public int MatchBeginEventSeqNo { get; private set; }

        /// <summary>Returns the time of the first matching event. </summary>
        /// <value>time</value>
        public long MatchBeginEventTime { get; private set; }

        /// <summary>Returns the partial matches. </summary>
        /// <value>state</value>
        public RegexNFAState State { get; set; }

        /// <summary>Returns the single-variable matches. </summary>
        /// <value>match events</value>
        public EventBean[] EventsPerStream { get; private set; }

        /// <summary>Returns the multirow-variable matches, if any. </summary>
        /// <value>matches</value>
        public MultimatchState[] OptionalMultiMatches { get; private set; }

        /// <summary>Returns the count of greedy matches per state. </summary>
        /// <value>greedy counts</value>
        public int[] GreedyCountPerState { get; private set; }

        /// <summary>Returns the match end event number. </summary>
        /// <value>num</value>
        public int MatchEndEventSeqNo { get; set; }

        /// <summary>Returns the partition key. </summary>
        /// <value>key</value>
        public object PartitionKey { get; private set; }

        public override String ToString()
        {
            return "Entry " + State;
        }
    }
}
