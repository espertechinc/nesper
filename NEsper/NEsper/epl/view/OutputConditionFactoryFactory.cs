///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
        /// <returns>
        /// instance for performing output
        /// </returns>
		public static OutputConditionFactory CreateCondition(OutputLimitSpec outputLimitSpec,
											 	  	         StatementContext statementContext,
	                                                         bool isGrouped,
	                                                         bool isWithHavingClause,
	                                                         bool isStartConditionOnCreation)
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
	            if (variableMetaData == null) {
	                throw new ExprValidationException("Variable named '" + outputLimitSpec.VariableName + "' has not been declared");
	            }
	            string message = VariableServiceUtil.CheckVariableContextName(statementContext.ContextDescriptor, variableMetaData);
	            if (message != null) {
	                throw new ExprValidationException(message);
	            }
	        }

	        if (outputLimitSpec.DisplayLimit == OutputLimitLimitType.FIRST)
			{
	            if (isGrouped) {
	                return new OutputConditionNullFactory();
	            }
	            if (!isWithHavingClause) {
	                return new OutputConditionFirstFactory(outputLimitSpec, statementContext, isGrouped, isWithHavingClause);
	            }
			}

	        if (outputLimitSpec.RateType == OutputLimitRateType.CRONTAB)
	        {
	            return new OutputConditionCrontabFactory(outputLimitSpec.CrontabAtSchedule, statementContext, isStartConditionOnCreation);
	        }
	        else if (outputLimitSpec.RateType == OutputLimitRateType.WHEN_EXPRESSION)
	        {
	            return new OutputConditionExpressionFactory(outputLimitSpec.WhenExpressionNode, outputLimitSpec.ThenExpressions, statementContext, outputLimitSpec.AndAfterTerminateExpr, outputLimitSpec.AndAfterTerminateThenExpressions, isStartConditionOnCreation);
	        }
	        else if (outputLimitSpec.RateType == OutputLimitRateType.EVENTS)
			{
	            if (Log.IsDebugEnabled)
	            {
				    Log.Debug(".createCondition creating OutputConditionCount with event rate " + outputLimitSpec);
	            }

	            if ((variableMetaData != null) && (!TypeHelper.IsNumericNonFP(variableMetaData.VariableType)))
	            {
	                throw new ArgumentException("Variable named '" + outputLimitSpec.VariableName + "' must be type integer, long or short");
	            }

	            int rate = -1;
	            if (outputLimitSpec.Rate != null)
	            {
	                rate = outputLimitSpec.Rate.AsInt();
	            }
	            return new OutputConditionCountFactory(rate, variableMetaData);
			}
			else if (outputLimitSpec.RateType == OutputLimitRateType.TERM)
			{
	            if (outputLimitSpec.AndAfterTerminateExpr == null && (outputLimitSpec.AndAfterTerminateThenExpressions == null || outputLimitSpec.AndAfterTerminateThenExpressions.IsEmpty())) {
	                return new OutputConditionTermFactory();
	            }
	            else {
	                return new OutputConditionExpressionFactory(
                        new ExprConstantNodeImpl(false), Collections.GetEmptyList<OnTriggerSetAssignment>(), statementContext, outputLimitSpec.AndAfterTerminateExpr, outputLimitSpec.AndAfterTerminateThenExpressions, isStartConditionOnCreation);
	            }
			}
	        else {
	            if (Log.IsDebugEnabled)
	            {
	                Log.Debug(".createCondition creating OutputConditionTime with interval length " + outputLimitSpec.Rate);
	            }
	            if ((variableMetaData != null) && (!TypeHelper.IsNumeric(variableMetaData.VariableType)))
	            {
	                throw new ArgumentException("Variable named '" + outputLimitSpec.VariableName + "' must be of numeric type");
	            }

	            return new OutputConditionTimeFactory(outputLimitSpec.TimePeriodExpr, isStartConditionOnCreation);
	        }
		}
	}
} // end of namespace
