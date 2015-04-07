///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.core.context.util;

namespace com.espertech.esper.epl.named
{
    /// <summary>
    /// Holds a unit of dispatch that is a result of a named window processing incoming or timer events.
    /// </summary>
    public class NamedWindowConsumerDispatchUnit
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="deltaData">the insert and remove stream posted by the named window</param>
        /// <param name="dispatchTo">the list of consuming statements, and for each the list of consumer views</param>
        public NamedWindowConsumerDispatchUnit(NamedWindowDeltaData deltaData, IDictionary<EPStatementAgentInstanceHandle, IList<NamedWindowConsumerView>> dispatchTo)
        {
            DeltaData = deltaData;
            DispatchTo = dispatchTo;
        }

        /// <summary>
        /// Returns the data to dispatch.
        /// </summary>
        /// <value>dispatch insert and remove stream events</value>
        public NamedWindowDeltaData DeltaData { get; private set; }

        /// <summary>
        /// Returns the destination of the dispatch: a map of statements and their consuming views (one or multiple)
        /// </summary>
        /// <value>map of statement to consumer views</value>
        public IDictionary<EPStatementAgentInstanceHandle, IList<NamedWindowConsumerView>> DispatchTo { get; private set; }
    }
}
