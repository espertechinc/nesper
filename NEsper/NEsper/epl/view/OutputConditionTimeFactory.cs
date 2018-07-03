///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.expression.time;

namespace com.espertech.esper.epl.view
{
    /// <summary>
    /// Output condition that is satisfied at the end of every time interval of a given length.
    /// </summary>
    public class OutputConditionTimeFactory : OutputConditionFactory
    {
        private readonly ExprTimePeriod _timePeriod;
        private readonly ExprTimePeriodEvalDeltaNonConst _timePeriodDeltaComputation;
        protected readonly bool _isStartConditionOnCreation;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="timePeriod">is the number of minutes or seconds to batch events for, may include variables</param>
        /// <param name="isStartConditionOnCreation">if set to <c>true</c> [is start condition on creation].</param>
        public OutputConditionTimeFactory(ExprTimePeriod timePeriod, bool isStartConditionOnCreation)
        {
            _timePeriod = timePeriod;
            _timePeriodDeltaComputation = timePeriod.NonconstEvaluator();
            _isStartConditionOnCreation = isStartConditionOnCreation;
        }

        public OutputCondition Make(AgentInstanceContext agentInstanceContext, OutputCallback outputCallback)
        {
            return new OutputConditionTime(outputCallback, agentInstanceContext, this, _isStartConditionOnCreation);
        }

        public ExprTimePeriod TimePeriod => _timePeriod;

        public ExprTimePeriodEvalDeltaNonConst TimePeriodDeltaComputation => _timePeriodDeltaComputation;
    }
}
