///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.rowrecog.nfa;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.rowrecog.core
{
    public class RowRecogPartitionTerminationStateComparator : Comparer<RowRecogNFAStateEntry>
    {
        private readonly int[] multimatchStreamNumToVariable;
        private readonly IDictionary<string, Pair<int, bool>> variableStreams;

        public RowRecogPartitionTerminationStateComparator(
            int[] multimatchStreamNumToVariable,
            IDictionary<string, Pair<int, bool>> variableStreams)
        {
            this.multimatchStreamNumToVariable = multimatchStreamNumToVariable;
            this.variableStreams = variableStreams;
        }

        public override int Compare(
            RowRecogNFAStateEntry o1,
            RowRecogNFAStateEntry o2)
        {
            return CompareTerminationStateToEndState(o1, o2) ? 0 : 1;
        }

        // End-state may have less events then the termination state
        public bool CompareTerminationStateToEndState(
            RowRecogNFAStateEntry terminationState,
            RowRecogNFAStateEntry endState)
        {
            if (terminationState.MatchBeginEventSeqNo != endState.MatchBeginEventSeqNo) {
                return false;
            }

            foreach (var entry in variableStreams) {
                var stream = entry.Value.First;
                var multi = entry.Value.Second;
                if (multi) {
                    var termStreamEvents = RowRecogNFAViewUtil.GetMultimatchArray(
                        multimatchStreamNumToVariable,
                        terminationState,
                        stream);
                    var endStreamEvents = RowRecogNFAViewUtil.GetMultimatchArray(
                        multimatchStreamNumToVariable,
                        endState,
                        stream);
                    if (endStreamEvents != null) {
                        if (termStreamEvents == null) {
                            return false;
                        }

                        for (var i = 0; i < endStreamEvents.Length; i++) {
                            if (termStreamEvents.Length > i &&
                                !EventBeanUtility.EventsAreEqualsAllowNull(
                                    endStreamEvents[i],
                                    termStreamEvents[i])) {
                                return false;
                            }
                        }
                    }
                }
                else {
                    var termStreamEvent = terminationState.EventsPerStream[stream];
                    var endStreamEvent = endState.EventsPerStream[stream];
                    if (!EventBeanUtility.EventsAreEqualsAllowNull(endStreamEvent, termStreamEvent)) {
                        return false;
                    }
                }
            }

            return true;
        }
    }
} // end of namespace