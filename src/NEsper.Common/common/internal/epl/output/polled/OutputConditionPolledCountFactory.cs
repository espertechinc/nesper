///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.variable.core;

namespace com.espertech.esper.common.@internal.epl.output.polled
{
    /// <summary>
    ///     Output limit condition that is satisfied when either
    ///     the total number of new events arrived or the total number
    ///     of old events arrived is greater than a preset value.
    /// </summary>
    public class OutputConditionPolledCountFactory : OutputConditionPolledFactory
    {
        public int EventRate { get; set; }

        public Variable Variable { get; set; }

        public OutputConditionPolled MakeNew(ExprEvaluatorContext exprEvaluatorContext)
        {
            var state = new OutputConditionPolledCountState(
                EventRate,
                EventRate,
                EventRate,
                true);
            return new OutputConditionPolledCount(state, GetVariableReader(exprEvaluatorContext));
        }

        public OutputConditionPolled MakeFromState(
            ExprEvaluatorContext exprEvaluatorContext,
            OutputConditionPolledState state)
        {
            return new OutputConditionPolledCount(
                (OutputConditionPolledCountState)state,
                GetVariableReader(exprEvaluatorContext));
        }

        private VariableReader GetVariableReader(ExprEvaluatorContext exprEvaluatorContext)
        {
            if (Variable == null) {
                return null;
            }

            return exprEvaluatorContext.VariableManagementService.GetReader(
                Variable.DeploymentId,
                Variable.MetaData.VariableName,
                exprEvaluatorContext.AgentInstanceId);
        }
    }
} // end of namespace