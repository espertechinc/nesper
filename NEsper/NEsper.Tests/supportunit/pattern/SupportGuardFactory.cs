///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.pattern;
using com.espertech.esper.pattern.guard;

namespace com.espertech.esper.supportunit.pattern
{
    public class SupportGuardFactory : GuardFactory
    {
        public void SetGuardParameters(IList<ExprNode> guardParameters, MatchedEventConvertor convertor)
        {
        }
    
        public Guard MakeGuard(PatternAgentInstanceContext context, MatchedEventMap beginState, Quitable quitable, EvalStateNodeNumber stateNodeId, Object guardState) {
            return null;
        }
    }
}
