///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.variable;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.view
{
    /// <summary>Factory for output condition instances.</summary>
    public class OutputConditionFactoryFactory {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        public static OutputConditionFactory CreateCondition(OutputLimitSpec outputLimitSpec,
                                                             StatementContext statementContext,
                                                             bool isGrouped,
                                                             bool isWithHavingClause,
                                                             bool isStartConditionOnCreation,
                                                             ResultSetProcessorHelperFactory resultSetProcessorHelperFactory)
                {
            if (outputLimitSpec == null) {
                return new OutputConditionNullFactory();
            }
    
            // Check if a variable is present
            VariableMetaData variableMetaData = null;
            if (outputLimitSpec.VariableName != null) {
                variableMetaData = statementContext.VariableService.GetVariableMetaData(outputLimitSpec.VariableName);
                if (variableMetaData == null) {
                    throw new ExprValidationException("Variable named '" + outputLimitSpec.VariableName + "' has not been declared");
                }
                string message = VariableServiceUtil.CheckVariableContextName(statementContext.ContextDescriptor, variableMetaData);
                if (message != null) {
                    throw new ExprValidationException(message);
                }
            }
    
            if (outputLimitSpec.DisplayLimit == OutputLimitLimitType.FIRST && isGrouped) {
                return new OutputConditionNullFactory();
            }
    
            if (outputLimitSpec.RateType == OutputLimitRateType.CRONTAB) {
                return ResultSetProcessorHelperFactory.MakeOutputConditionCrontab(outputLimitSpec.CrontabAtSchedule, statementContext, isStartConditionOnCreation);
            } else if (outputLimitSpec.RateType == OutputLimitRateType.WHEN_EXPRESSION) {
                return ResultSetProcessorHelperFactory.MakeOutputConditionExpression(outputLimitSpec.WhenExpressionNode, outputLimitSpec.ThenExpressions, statementContext, outputLimitSpec.AndAfterTerminateExpr, outputLimitSpec.AndAfterTerminateThenExpressions, isStartConditionOnCreation);
            } else if (outputLimitSpec.RateType == OutputLimitRateType.EVENTS) {
                if (Log.IsDebugEnabled) {
                    Log.Debug(".createCondition creating OutputConditionCount with event rate " + outputLimitSpec);
                }
    
                if ((variableMetaData != null) && (!TypeHelper.IsNumericNonFP(variableMetaData.Type))) {
                    throw new ArgumentException("Variable named '" + outputLimitSpec.VariableName + "' must be type integer, long or short");
                }
    
                int rate = -1;
                if (outputLimitSpec.Rate != null) {
                    rate = outputLimitSpec.Rate.IntValue();
                }
                return ResultSetProcessorHelperFactory.MakeOutputConditionCount(rate, variableMetaData, statementContext);
            } else if (outputLimitSpec.RateType == OutputLimitRateType.TERM) {
                if (outputLimitSpec.AndAfterTerminateExpr == null && (outputLimitSpec.AndAfterTerminateThenExpressions == null || outputLimitSpec.AndAfterTerminateThenExpressions.IsEmpty())) {
                    return new OutputConditionTermFactory();
                } else {
                    return ResultSetProcessorHelperFactory.MakeOutputConditionExpression(new ExprConstantNodeImpl(false), Collections.<OnTriggerSetAssignment>EmptyList(), statementContext, outputLimitSpec.AndAfterTerminateExpr, outputLimitSpec.AndAfterTerminateThenExpressions, isStartConditionOnCreation);
                }
            } else {
                if (Log.IsDebugEnabled) {
                    Log.Debug(".createCondition creating OutputConditionTime with interval length " + outputLimitSpec.Rate);
                }
                if ((variableMetaData != null) && (!TypeHelper.IsNumeric(variableMetaData.Type))) {
                    throw new ArgumentException("Variable named '" + outputLimitSpec.VariableName + "' must be of numeric type");
                }
    
                return ResultSetProcessorHelperFactory.MakeOutputConditionTime(outputLimitSpec.TimePeriodExpr, isStartConditionOnCreation);
            }
        }
    }
} // end of namespace
