///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.join.@base
{
    /// <summary>
    ///     Processes a join result set constisting of sets of tuples of events.
    /// </summary>
    public interface JoinSetProcessor
    {
        /// <summary>
        ///     Process join result set.
        /// </summary>
        /// <param name="newEvents">set of event tuples representing new data</param>
        /// <param name="oldEvents">set of event tuples representing old data</param>
        /// <param name="exprEvaluatorContext">expression evaluation context</param>
        void Process(
            ISet<MultiKeyArrayOfKeys<EventBean>> newEvents,
            ISet<MultiKeyArrayOfKeys<EventBean>> oldEvents,
            ExprEvaluatorContext exprEvaluatorContext);
    }
} // end of namespace