///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.collection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.epl.streamtype
{
    public class StreamTypeServiceUtil
    {
        protected internal static Pair<int, string> FindLevMatch(EventType[] eventTypes, string propertyName)
        {
            string bestMatch = null;
            var bestMatchDiff = int.MaxValue;
            for (var i = 0; i < eventTypes.Length; i++) {
                if (eventTypes[i] == null) {
                    continue;
                }

                var props = eventTypes[i].PropertyDescriptors;
                for (var j = 0; j < props.Length; j++) {
                    var diff = LevenshteinDistance.ComputeLevenshteinDistance(propertyName, props[j].PropertyName);
                    if (diff < bestMatchDiff) {
                        bestMatchDiff = diff;
                        bestMatch = props[j].PropertyName;
                    }
                }
            }

            if (bestMatchDiff < int.MaxValue) {
                return new Pair<int, string>(bestMatchDiff, bestMatch);
            }

            return null;
        }

        protected internal static Pair<int, string> FindLevMatch(string propertyName, EventType eventType)
        {
            string bestMatch = null;
            var bestMatchDiff = int.MaxValue;
            var props = eventType.PropertyDescriptors;
            for (var j = 0; j < props.Length; j++) {
                var diff = LevenshteinDistance.ComputeLevenshteinDistance(propertyName, props[j].PropertyName);
                if (diff < bestMatchDiff) {
                    bestMatchDiff = diff;
                    bestMatch = props[j].PropertyName;
                }
            }

            if (bestMatchDiff < int.MaxValue) {
                return new Pair<int, string>(bestMatchDiff, bestMatch);
            }

            return null;
        }
    }
} // end of namespace