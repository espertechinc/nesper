///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.prior;

namespace com.espertech.esper.core.context.stmt
{
    public interface AIRegistryPrior : ExprPriorEvalStrategy {
        void AssignService(int num, ExprPriorEvalStrategy value);
        void DeassignService(int num);
        int AgentInstanceCount { get; }
    }
}
