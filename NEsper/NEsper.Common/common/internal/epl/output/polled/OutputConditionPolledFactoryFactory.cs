///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.output.polled
{
    /// <summary>
    /// Factory for output condition instances that are polled/queried only.
    /// </summary>
    public class OutputConditionPolledFactoryFactory
    {
        public static OutputConditionPolledFactoryForge CreateConditionFactory(
            OutputLimitSpec outputLimitSpec,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            if (outputLimitSpec == null) {
                throw new NullPointerException("Output condition requires a non-null callback");
            }

            // check variable use
            VariableMetaData variableMetaData = null;
            if (outputLimitSpec.VariableName != null) {
                variableMetaData = compileTimeServices.VariableCompileTimeResolver.Resolve(outputLimitSpec.VariableName);
                if (variableMetaData == null) {
                    throw new ArgumentException("Variable named '" + outputLimitSpec.VariableName + "' has not been declared");
                }
            }

            if (outputLimitSpec.RateType == OutputLimitRateType.CRONTAB) {
                return new OutputConditionPolledCrontabFactoryForge(outputLimitSpec.CrontabAtSchedule, statementRawInfo, compileTimeServices);
            }
            else if (outputLimitSpec.RateType == OutputLimitRateType.WHEN_EXPRESSION) {
                return new OutputConditionPolledExpressionFactoryForge(
                    outputLimitSpec.WhenExpressionNode, outputLimitSpec.ThenExpressions, compileTimeServices);
            }
            else if (outputLimitSpec.RateType == OutputLimitRateType.EVENTS) {
                int rate = -1;
                if (outputLimitSpec.Rate != null) {
                    rate = outputLimitSpec.Rate.IntValue();
                }

                return new OutputConditionPolledCountFactoryForge(rate, variableMetaData);
            }
            else {
                if (variableMetaData != null && (!TypeHelper.IsNumeric(variableMetaData.Type))) {
                    throw new ArgumentException("Variable named '" + outputLimitSpec.VariableName + "' must be of numeric type");
                }

                return new OutputConditionPolledTimeFactoryForge(outputLimitSpec.TimePeriodExpr);
            }
        }
    }
} // end of namespace