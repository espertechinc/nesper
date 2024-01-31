///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.common.@internal.view.core;

namespace com.espertech.esper.common.@internal.epl.subselect
{
    public interface SubSelectStrategyFactory
    {
        LookupStrategyDesc LookupStrategyDesc { get; }

        void Ready(
            SubSelectStrategyFactoryContext subselectFactoryContext,
            EventType eventType);

        SubSelectStrategyRealization Instantiate(
            Viewable viewableRoot,
            ExprEvaluatorContext exprEvaluatorContext,
            IList<AgentInstanceMgmtCallback> stopCallbackList,
            int subqueryNumber,
            bool isRecoveringResilient);
    }
} // end of namespace