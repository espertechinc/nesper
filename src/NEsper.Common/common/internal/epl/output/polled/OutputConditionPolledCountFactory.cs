///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.variable.core;

namespace com.espertech.esper.common.@internal.epl.output.polled
{
    /// <summary>
    /// Output limit condition that is satisfied when either
    /// the total number of new events arrived or the total number
    /// of old events arrived is greater than a preset value.
    /// </summary>
    public sealed class OutputConditionPolledCountFactory : OutputConditionPolledFactory
    {
        private int _eventRate;
        private Variable _variable;

        public int EventRate {
            get => _eventRate;
            set => _eventRate = value;
        }

        public Variable Variable {
            get => _variable;
            set => _variable = value;
        }

        public OutputConditionPolledCountFactory SetEventRate(int eventRate)
        {
            _eventRate = eventRate;
            return this;
        }

        public OutputConditionPolledCountFactory SetVariable(Variable variable)
        {
            _variable = variable;
            return this;
        }

        public OutputConditionPolled MakeNew(ExprEvaluatorContext exprEvaluatorContext)
        {
            OutputConditionPolledCountState state = new OutputConditionPolledCountState(
                _eventRate,
                _eventRate,
                _eventRate,
                true);
            return new OutputConditionPolledCount(state, GetVariableReader(exprEvaluatorContext));
        }

        public OutputConditionPolled MakeFromState(
            ExprEvaluatorContext exprEvaluatorContext,
            OutputConditionPolledState state)
        {
            return new OutputConditionPolledCount(
                (OutputConditionPolledCountState) state,
                GetVariableReader(exprEvaluatorContext));
        }

        private VariableReader GetVariableReader(ExprEvaluatorContext exprEvaluatorContext)
        {
            if (_variable == null) {
                return null;
            }

            return exprEvaluatorContext.VariableManagementService.GetReader(
                _variable.DeploymentId,
                _variable.MetaData.VariableName,
                exprEvaluatorContext.AgentInstanceId);
        }
    }
} // end of namespace