///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.common.@internal.fabric;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;


namespace com.espertech.esper.common.@internal.epl.output.condition
{
    /// <summary>
    /// Factory for output condition instances.
    /// </summary>
    public class OutputConditionFactoryFactory
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static OutputConditionFactoryForgeResult CreateCondition(
            OutputLimitSpec outputLimitSpec,
            bool isGrouped,
            bool isStartConditionOnCreation,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            var fabricCharge = services.StateMgmtSettingsProvider.NewCharge();
            if (outputLimitSpec == null) {
                return new OutputConditionFactoryForgeResult(OutputConditionNullFactoryForge.INSTANCE, fabricCharge);
            }

            // Check if a variable is present
            VariableMetaData variableMetaData = null;
            if (outputLimitSpec.VariableName != null) {
                variableMetaData = services.VariableCompileTimeResolver.Resolve(outputLimitSpec.VariableName);
                if (variableMetaData == null) {
                    throw new ExprValidationException(
                        "Variable named '" + outputLimitSpec.VariableName + "' has not been declared");
                }

                var message = VariableUtil.CheckVariableContextName(statementRawInfo.ContextName, variableMetaData);
                if (message != null) {
                    throw new ExprValidationException(message);
                }
            }

            if (outputLimitSpec.DisplayLimit == OutputLimitLimitType.FIRST && isGrouped) {
                return new OutputConditionFactoryForgeResult(OutputConditionNullFactoryForge.INSTANCE, fabricCharge);
            }

            if (outputLimitSpec.RateType == OutputLimitRateType.CRONTAB) {
                var forge = new OutputConditionCrontabForge(
                    outputLimitSpec.CrontabAtSchedule,
                    isStartConditionOnCreation,
                    statementRawInfo,
                    services);
                return new OutputConditionFactoryForgeResult(forge, fabricCharge);
            }
            else if (outputLimitSpec.RateType == OutputLimitRateType.WHEN_EXPRESSION) {
                var settings = services.StateMgmtSettingsProvider.ResultSet.OutputExpression(fabricCharge);
                var forge = new OutputConditionExpressionForge(
                    outputLimitSpec.WhenExpressionNode,
                    outputLimitSpec.ThenExpressions,
                    outputLimitSpec.AndAfterTerminateExpr,
                    outputLimitSpec.AndAfterTerminateThenExpressions,
                    isStartConditionOnCreation,
                    settings,
                    statementRawInfo,
                    services);
                return new OutputConditionFactoryForgeResult(forge, fabricCharge);
            }
            else if (outputLimitSpec.RateType == OutputLimitRateType.EVENTS) {
                if (variableMetaData != null && !variableMetaData.Type.IsTypeNumericNonFP()) {
                    throw new ArgumentException(
                        "Variable named '" + outputLimitSpec.VariableName + "' must be type integer, long or short");
                }

                var rate = -1;
                if (outputLimitSpec.Rate != null) {
                    rate = outputLimitSpec.Rate.AsInt32();
                }

                var setting = services.StateMgmtSettingsProvider.ResultSet.OutputCount(fabricCharge);
                var forge = new OutputConditionCountForge(rate, variableMetaData, setting);
                return new OutputConditionFactoryForgeResult(forge, fabricCharge);
            }
            else if (outputLimitSpec.RateType == OutputLimitRateType.TERM) {
                OutputConditionFactoryForge forge;
                if (outputLimitSpec.AndAfterTerminateExpr == null &&
                    (outputLimitSpec.AndAfterTerminateThenExpressions == null ||
                     outputLimitSpec.AndAfterTerminateThenExpressions.IsEmpty())) {
                    forge = new OutputConditionTermFactoryForge();
                }
                else {
                    var setting = services.StateMgmtSettingsProvider.ResultSet.OutputExpression(fabricCharge);
                    forge = new OutputConditionExpressionForge(
                        new ExprConstantNodeImpl(false),
                        EmptyList<OnTriggerSetAssignment>.Instance, 
                        outputLimitSpec.AndAfterTerminateExpr,
                        outputLimitSpec.AndAfterTerminateThenExpressions,
                        isStartConditionOnCreation,
                        setting,
                        statementRawInfo,
                        services);
                }

                return new OutputConditionFactoryForgeResult(forge, fabricCharge);
            }
            else {
                if (Log.IsDebugEnabled) {
                    Log.Debug(
                        ".createCondition creating OutputConditionTime with interval length " + outputLimitSpec.Rate);
                }

                if (variableMetaData != null && !variableMetaData.Type.IsTypeNumeric()) {
                    throw new ArgumentException(
                        "Variable named '" + outputLimitSpec.VariableName + "' must be of numeric type");
                }

                var setting = services.StateMgmtSettingsProvider.ResultSet.OutputTime(fabricCharge);
                var forge = new OutputConditionTimeForge(
                    outputLimitSpec.TimePeriodExpr,
                    isStartConditionOnCreation,
                    setting);
                return new OutputConditionFactoryForgeResult(forge, fabricCharge);
            }
        }
    }
} // end of namespace