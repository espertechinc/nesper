///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.util;

namespace com.espertech.esper.view
{
    public class ViewFactoryTimePeriodHelper
    {
        public static ExprTimePeriodEvalDeltaConstFactory ValidateAndEvaluateTimeDeltaFactory(
            string viewName,
            StatementContext statementContext,
            ExprNode expression,
            string expectedMessage,
            int expressionNumber)
        {
            var streamTypeService = new StreamTypeServiceImpl(statementContext.EngineURI, false);
            ExprTimePeriodEvalDeltaConstFactory factory;
            if (expression is ExprTimePeriod)
            {
                var validated = (ExprTimePeriod) ViewFactorySupport.ValidateExpr(
                    viewName, statementContext, expression, streamTypeService, expressionNumber);
                factory = validated.ConstEvaluator(new ExprEvaluatorContextStatement(statementContext, false));
            }
            else
            {
                var validated = ViewFactorySupport.ValidateExpr(
                    viewName, statementContext, expression, streamTypeService, expressionNumber);
                var secondsEvaluator = validated.ExprEvaluator;
                var returnType = secondsEvaluator.ReturnType.GetBoxedType();
                if (!returnType.IsNumeric())
                {
                    throw new ViewParameterException(expectedMessage);
                }
                if (validated.IsConstantResult)
                {
                    var time = ViewFactorySupport.Evaluate(secondsEvaluator, 0, viewName, statementContext);
                    if (!ExprTimePeriodUtil.ValidateTime(time, statementContext.TimeAbacus))
                    {
                        throw new ViewParameterException(ExprTimePeriodUtil.GetTimeInvalidMsg(viewName, "view", time));
                    }
                    var msec = statementContext.TimeAbacus.DeltaForSecondsNumber(time);
                    factory = new ExprTimePeriodEvalDeltaConstGivenDelta(msec);
                }
                else
                {
                    factory = new ExprTimePeriodEvalDeltaConstFactoryMsec(secondsEvaluator, statementContext.TimeAbacus);
                }
            }
            return factory;
        }
    }
} // end of namespace