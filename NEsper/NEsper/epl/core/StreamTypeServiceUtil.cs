///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.core
{
	public class StreamTypeServiceUtil
	{
	    internal static Pair<int, string> FindLevMatch(EventType[] eventTypes, string propertyName)
	    {
	        string bestMatch = null;
	        int bestMatchDiff = int.MaxValue;
	        for (int i = 0; i < eventTypes.Length; i++)
	        {
	            if (eventTypes[i] == null)
	            {
	                continue;
	            }
	            var props = eventTypes[i].PropertyDescriptors;
	            for (int j = 0; j < props.Count; j++)
	            {
	                int diff = LevenshteinDistance.ComputeLevenshteinDistance(propertyName, props[j].PropertyName);
	                if (diff < bestMatchDiff)
	                {
	                    bestMatchDiff = diff;
	                    bestMatch = props[j].PropertyName;
	                }
	            }
	        }

	        if (bestMatchDiff < int.MaxValue)
	        {
	            return new Pair<int, string>(bestMatchDiff, bestMatch);
	        }
	        return null;
	    }

        internal static Pair<int, string> FindLevMatch(string propertyName, EventType eventType)
	    {
	        string bestMatch = null;
	        int bestMatchDiff = int.MaxValue;
	        var props = eventType.PropertyDescriptors;
	        for (int j = 0; j < props.Count; j++)
	        {
	            int diff = LevenshteinDistance.ComputeLevenshteinDistance(propertyName, props[j].PropertyName);
	            if (diff < bestMatchDiff)
	            {
	                bestMatchDiff = diff;
	                bestMatch = props[j].PropertyName;
	            }
	        }

	        if (bestMatchDiff < int.MaxValue)
	        {
	            return new Pair<int, string>(bestMatchDiff, bestMatch);
	        }
	        return null;
	    }
	}
} // end of namespace
