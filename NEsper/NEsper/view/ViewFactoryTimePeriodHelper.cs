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
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.util;

namespace com.espertech.esper.view
{
    public class ViewFactoryTimePeriodHelper {
        public static ExprTimePeriodEvalDeltaConstFactory ValidateAndEvaluateTimeDeltaFactory(string viewName,
                                                                                              StatementContext statementContext,
                                                                                              ExprNode expression,
                                                                                              string expectedMessage,
                                                                                              int expressionNumber)
                {
            var streamTypeService = new StreamTypeServiceImpl(statementContext.EngineURI, false);
            ExprTimePeriodEvalDeltaConstFactory factory;
            if (expression is ExprTimePeriod) {
                ExprTimePeriod validated = (ExprTimePeriod) ViewFactorySupport.ValidateExpr(viewName, statementContext, expression, streamTypeService, expressionNumber);
                factory = validated.ConstEvaluator(new ExprEvaluatorContextStatement(statementContext, false));
            } else {
                ExprNode validated = ViewFactorySupport.ValidateExpr(viewName, statementContext, expression, streamTypeService, expressionNumber);
                ExprEvaluator secondsEvaluator = validated.ExprEvaluator;
                Type returnType = TypeHelper.GetBoxedType(secondsEvaluator.Type);
                if (!TypeHelper.IsNumeric(returnType)) {
                    throw new ViewParameterException(expectedMessage);
                }
                if (validated.IsConstantResult) {
                    Number time = (Number) ViewFactorySupport.Evaluate(secondsEvaluator, 0, viewName, statementContext);
                    if (!ExprTimePeriodUtil.ValidateTime(time, statementContext.TimeAbacus)) {
                        throw new ViewParameterException(ExprTimePeriodUtil.GetTimeInvalidMsg(viewName, "view", time));
                    }
                    long msec = statementContext.TimeAbacus.DeltaForSecondsNumber(time);
                    factory = new ExprTimePeriodEvalDeltaConstGivenDelta(msec);
                } else {
                    factory = new ExprTimePeriodEvalDeltaConstFactoryMsec(secondsEvaluator, statementContext.TimeAbacus);
                }
            }
            return factory;
        }
    }
} // end of namespace
