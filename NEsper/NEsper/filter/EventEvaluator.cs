///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;

namespace com.espertech.esper.filter
{
    /// <summary>
    /// Interface for matching an event instance based on the event's property values to
    /// filters, specifically filter parameter constants or ranges. 
    /// </summary>
    public interface EventEvaluator
    {
        /// <summary>Perform the matching of an event based on the event property values, adding any callbacks for matches found to the matches list. </summary>
        /// <param name="theTheEvent">is the event object wrapper to obtain event property values from</param>
        /// <param name="matches">accumulates the matching filter callbacks</param>
        void MatchEvent(EventBean theTheEvent, ICollection<FilterHandle> matches);
    }
}
