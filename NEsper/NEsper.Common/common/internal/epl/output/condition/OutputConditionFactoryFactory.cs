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
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.output.condition
{
    /// <summary>
    ///     Factory for output condition instances.
    /// </summary>
    public class OutputConditionFactoryFactory
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(OutputConditionFactoryFactory));

        public static OutputConditionFactoryForge CreateCondition(
            OutputLimitSpec outputLimitSpec,
            bool isGrouped,
            bool isWithHavingClause,
            bool isStartConditionOnCreation,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            if (outputLimitSpec == null)
            {
                return OutputConditionNullFactoryForge.INSTANCE;
            }

            // Check if a variable is present
            VariableMetaData variableMetaData = null;
            if (outputLimitSpec.VariableName != null)
            {
                variableMetaData = services.VariableCompileTimeResolver.Resolve(outputLimitSpec.VariableName);
                if (variableMetaData == null)
                {
                    throw new ExprValidationException(
                        "Variable named '" + outputLimitSpec.VariableName + "' has not been declared");
                }

                var message = VariableUtil.CheckVariableContextName(statementRawInfo.ContextName, variableMetaData);
                if (message != null)
                {
                    throw new ExprValidationException(message);
                }
            }

            if (outputLimitSpec.DisplayLimit == OutputLimitLimitType.FIRST && isGrouped)
            {
                return OutputConditionNullFactoryForge.INSTANCE;
            }

            if (outputLimitSpec.RateType == OutputLimitRateType.CRONTAB)
            {
                return new OutputConditionCrontabForge(
                    outputLimitSpec.CrontabAtSchedule, isStartConditionOnCreation, statementRawInfo, services);
            }

            if (outputLimitSpec.RateType == OutputLimitRateType.WHEN_EXPRESSION)
            {
                return new OutputConditionExpressionForge(
                    outputLimitSpec.WhenExpressionNode, outputLimitSpec.ThenExpressions,
                    outputLimitSpec.AndAfterTerminateExpr, outputLimitSpec.AndAfterTerminateThenExpressions,
                    isStartConditionOnCreation, services);
            }

            if (outputLimitSpec.RateType == OutputLimitRateType.EVENTS)
            {
                if (variableMetaData != null && !variableMetaData.Type.IsNumericNonFP())
                {
                    throw new ArgumentException(
                        "Variable named '" + outputLimitSpec.VariableName + "' must be type integer, long or short");
                }

                var rate = -1;
                if (outputLimitSpec.Rate != null)
                {
                    rate = outputLimitSpec.Rate.AsInt();
                }

                return new OutputConditionCountForge(rate, variableMetaData);
            }

            if (outputLimitSpec.RateType == OutputLimitRateType.TERM)
            {
                if (outputLimitSpec.AndAfterTerminateExpr == null &&
                    (outputLimitSpec.AndAfterTerminateThenExpressions == null ||
                     outputLimitSpec.AndAfterTerminateThenExpressions.IsEmpty()))
                {
                    return new OutputConditionTermFactoryForge();
                }

                return new OutputConditionExpressionForge(
                    new ExprConstantNodeImpl(false),
                    Collections.GetEmptyList<OnTriggerSetAssignment>(),
                    outputLimitSpec.AndAfterTerminateExpr,
                    outputLimitSpec.AndAfterTerminateThenExpressions,
                    isStartConditionOnCreation, services);
            }

            if (Log.IsDebugEnabled)
            {
                Log.Debug(".createCondition creating OutputConditionTime with interval length " + outputLimitSpec.Rate);
            }

            if (variableMetaData != null && !variableMetaData.Type.IsNumeric())
            {
                throw new ArgumentException(
                    "Variable named '" + outputLimitSpec.VariableName + "' must be of numeric type");
            }

            return new OutputConditionTimeForge(outputLimitSpec.TimePeriodExpr, isStartConditionOnCreation);
        }
    }
} // end of namespace