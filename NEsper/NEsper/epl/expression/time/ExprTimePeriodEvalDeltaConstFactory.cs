///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;

namespace com.espertech.esper.epl.expression.time
{
    public interface ExprTimePeriodEvalDeltaConstFactory {
        ExprTimePeriodEvalDeltaConst Make(string validateMsgName, string validateMsgValue, AgentInstanceContext agentInstanceContext);
    }
} // end of namespace
