///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.events;

namespace com.espertech.esper.rowregex
{
	public class RegexPartitionTerminationStateComparator : IComparer<RegexNFAStateEntry>
	{
	    private readonly int[] _multimatchStreamNumToVariable;
	    private readonly LinkedHashMap<string, Pair<int, bool>> _variableStreams;

	    public RegexPartitionTerminationStateComparator(int[] multimatchStreamNumToVariable, LinkedHashMap<string, Pair<int, bool>> variableStreams)
        {
	        _multimatchStreamNumToVariable = multimatchStreamNumToVariable;
	        _variableStreams = variableStreams;
	    }

	    public int Compare(RegexNFAStateEntry o1, RegexNFAStateEntry o2)
        {
	        return CompareTerminationStateToEndState(o1, o2) ? 0 : 1;
	    }

	    // End-state may have less events then the termination state
	    public bool CompareTerminationStateToEndState(RegexNFAStateEntry terminationState, RegexNFAStateEntry endState)
        {
	        if (terminationState.MatchBeginEventSeqNo != endState.MatchBeginEventSeqNo)
            {
	            return false;
	        }
	        foreach (var entry in _variableStreams)
            {
	            int stream = entry.Value.First;
	            bool multi = entry.Value.Second;
	            if (multi)
                {
	                EventBean[] termStreamEvents = EventRowRegexNFAViewUtil.GetMultimatchArray(_multimatchStreamNumToVariable, terminationState, stream);
	                EventBean[] endStreamEvents = EventRowRegexNFAViewUtil.GetMultimatchArray(_multimatchStreamNumToVariable, endState, stream);
	                if (endStreamEvents != null)
                    {
	                    if (termStreamEvents == null)
                        {
	                        return false;
	                    }
	                    for (int i = 0; i < endStreamEvents.Length; i++)
                        {
	                        if (termStreamEvents.Length > i && !EventBeanUtility.EventsAreEqualsAllowNull(endStreamEvents[i], termStreamEvents[i]))
                            {
	                            return false;
	                        }
	                    }
	                }
	            }
	            else
                {
	                EventBean termStreamEvent = terminationState.EventsPerStream[stream];
	                EventBean endStreamEvent = endState.EventsPerStream[stream];
	                if (!EventBeanUtility.EventsAreEqualsAllowNull(endStreamEvent, termStreamEvent))
                    {
	                    return false;
	                }
	            }
	        }
	        return true;
	    }
	}
} // end of namespace
