///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.join.@base
{
    /// <summary>Encapsulates the strategy use to resolve the events for a stream into a tuples of events in a join. </summary>
    public interface QueryStrategy
    {
        /// <summary>Look up events returning tuples of joined events. </summary>
        /// <param name="lookupEvents">events to use to perform the join</param>
        /// <param name="joinSet">result join tuples of events</param>
        /// <param name="exprEvaluatorContext">expression evaluation context</param>
        void Lookup(EventBean[] lookupEvents,
                    ICollection<MultiKey<EventBean>> joinSet,
                    ExprEvaluatorContext exprEvaluatorContext);
    }
}