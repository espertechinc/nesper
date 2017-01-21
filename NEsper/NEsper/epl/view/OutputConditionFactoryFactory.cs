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
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.variable;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.view
{
	/// <summary>
	/// Factory for output condition instances.
	/// </summary>
	public class OutputConditionFactoryFactory
	{
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Creates an output condition instance.
        /// </summary>
        /// <param name="outputLimitSpec">specifies what kind of condition to create</param>
        /// <param name="statementContext">The statement context.</param>
        /// <param name="isGrouped">if set to <c>true</c> [is grouped].</param>
        /// <param name="isWithHavingClause">if set to <c>true</c> [is with having clause].</param>
        /// <param name="isStartConditionOnCreation">if set to <c>true</c> [is start condition on creation].</param>
        /// <param name="resultSetProcessorHelperFactory">The result set processor helper factory.</param>
        /// <returns>
        /// instance for performing output
        /// </returns>
        /// <exception cref="ExprValidationException">
        /// Variable named ' + outputLimitSpec.VariableName + ' has not been declared
        /// or
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Variable named ' + outputLimitSpec.VariableName + ' must be type integer, long or short
        /// or
        /// Variable named ' + outputLimitSpec.VariableName + ' must be of numeric type
        /// </exception>
	    public static OutputConditionFactory CreateCondition(
	        OutputLimitSpec outputLimitSpec,
	        StatementContext statementContext,
	        bool isGrouped,
	        bool isWithHavingClause,
	        bool isStartConditionOnCreation,
	        ResultSetProcessorHelperFactory resultSetProcessorHelperFactory)
	    {
	        if (outputLimitSpec == null)
	        {
	            return new OutputConditionNullFactory();
	        }

	        // Check if a variable is present
	        VariableMetaData variableMetaData = null;
	        if (outputLimitSpec.VariableName != null)
	        {
	            variableMetaData = statementContext.VariableService.GetVariableMetaData(outputLimitSpec.VariableName);
	            if (variableMetaData == null)
	            {
	                throw new ExprValidationException(
	                    "Variable named '" + outputLimitSpec.VariableName + "' has not been declared");
	            }
	            string message = VariableServiceUtil.CheckVariableContextName(
	                statementContext.ContextDescriptor, variableMetaData);
	            if (message != null)
	            {
	                throw new ExprValidationException(message);
	            }
	        }

	        if (outputLimitSpec.DisplayLimit == OutputLimitLimitType.FIRST && isGrouped)
	        {
	            return new OutputConditionNullFactory();
	        }

	        if (outputLimitSpec.RateType == OutputLimitRateType.CRONTAB)
	        {
	            return resultSetProcessorHelperFactory.MakeOutputConditionCrontab(
	                outputLimitSpec.CrontabAtSchedule, statementContext, isStartConditionOnCreation);
	        }
	        else if (outputLimitSpec.RateType == OutputLimitRateType.WHEN_EXPRESSION)
	        {
	            return resultSetProcessorHelperFactory.MakeOutputConditionExpression(
	                outputLimitSpec.WhenExpressionNode, outputLimitSpec.ThenExpressions, statementContext,
	                outputLimitSpec.AndAfterTerminateExpr, outputLimitSpec.AndAfterTerminateThenExpressions,
	                isStartConditionOnCreation);
	        }
	        else if (outputLimitSpec.RateType == OutputLimitRateType.EVENTS)
	        {
	            if (Log.IsDebugEnabled)
	            {
	                Log.Debug(".createCondition creating OutputConditionCount with event rate " + outputLimitSpec);
	            }

	            if ((variableMetaData != null) && (!variableMetaData.VariableType.IsNumericNonFP()))
	            {
	                throw new ArgumentException(
	                    "Variable named '" + outputLimitSpec.VariableName + "' must be type integer, long or short");
	            }

	            int rate = -1;
	            if (outputLimitSpec.Rate != null)
	            {
	                rate = outputLimitSpec.Rate.Value.AsInt();
	            }
	            return resultSetProcessorHelperFactory.MakeOutputConditionCount(rate, variableMetaData, statementContext);
	        }
	        else if (outputLimitSpec.RateType == OutputLimitRateType.TERM)
	        {
	            if (outputLimitSpec.AndAfterTerminateExpr == null &&
	                (outputLimitSpec.AndAfterTerminateThenExpressions == null ||
	                 outputLimitSpec.AndAfterTerminateThenExpressions.IsEmpty()))
	            {
	                return new OutputConditionTermFactory();
	            }
	            else
	            {
	                return
	                    resultSetProcessorHelperFactory.MakeOutputConditionExpression(
	                        new ExprConstantNodeImpl(false),
	                        Collections.GetEmptyList<OnTriggerSetAssignment>(),
	                        statementContext,
	                        outputLimitSpec.AndAfterTerminateExpr,
	                        outputLimitSpec.AndAfterTerminateThenExpressions,
	                        isStartConditionOnCreation);
	            }
	        }
	        else
	        {
	            if (Log.IsDebugEnabled)
	            {
	                Log.Debug(".createCondition creating OutputConditionTime with interval length " + outputLimitSpec.Rate);
	            }
	            if ((variableMetaData != null) && (!variableMetaData.VariableType.IsNumeric()))
	            {
	                throw new ArgumentException(
	                    "Variable named '" + outputLimitSpec.VariableName + "' must be of numeric type");
	            }

	            return resultSetProcessorHelperFactory.MakeOutputConditionTime(
	                outputLimitSpec.TimePeriodExpr, isStartConditionOnCreation);
	        }
	    }
	}
} // end of namespace
