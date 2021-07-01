///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;

namespace com.espertech.esper.common.@internal.epl.output.condition
{
    public class OutputConditionNullFactory : OutputConditionFactory
    {
        public static readonly OutputConditionNullFactory INSTANCE = new OutputConditionNullFactory();

        private OutputConditionNullFactory()
        {
        }

        public OutputCondition InstantiateOutputCondition(
            AgentInstanceContext agentInstanceContext,
            OutputCallback outputCallback)
        {
            return new OutputConditionNull(outputCallback);
        }
    }
} // end of namespace