///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.variable;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.view
{
	/// <summary>
	/// Factory for output condition instances that are polled/queried only.
	/// </summary>
	public class OutputConditionPolledFactoryFactory
	{
	    public static OutputConditionPolledFactory CreateConditionFactory(OutputLimitSpec outputLimitSpec, StatementContext statementContext)
	    {
	        if (outputLimitSpec == null) {
	            throw new ArgumentNullException("Output condition by count requires a non-null callback");
	        }

	        // check variable use
	        VariableMetaData variableMetaData = null;
	        if (outputLimitSpec.VariableName != null) {
	            variableMetaData = statementContext.VariableService.GetVariableMetaData(outputLimitSpec.VariableName);
	            if (variableMetaData == null) {
	                throw new ArgumentException("Variable named '" + outputLimitSpec.VariableName + "' has not been declared");
	            }
	        }

	        if (outputLimitSpec.RateType == OutputLimitRateType.CRONTAB) {
	            return new OutputConditionPolledCrontabFactory(outputLimitSpec.CrontabAtSchedule, statementContext);
	        }
	        else if (outputLimitSpec.RateType == OutputLimitRateType.WHEN_EXPRESSION) {
	            return new OutputConditionPolledExpressionFactory(outputLimitSpec.WhenExpressionNode, outputLimitSpec.ThenExpressions, statementContext);
	        }
	        else if (outputLimitSpec.RateType == OutputLimitRateType.EVENTS) {
	            int rate = -1;
	            if (outputLimitSpec.Rate != null) {
	                rate = outputLimitSpec.Rate.AsInt();
	            }
	            return new OutputConditionPolledCountFactory(rate, statementContext, outputLimitSpec.VariableName);
	        }
	        else {
	            if (variableMetaData != null && (!variableMetaData.VariableType.IsNumeric())) {
	                throw new ArgumentException("Variable named '" + outputLimitSpec.VariableName + "' must be of numeric type");
	            }
	            return new OutputConditionPolledTimeFactory(outputLimitSpec.TimePeriodExpr, statementContext);
	        }
	    }
	}
} // end of namespace
