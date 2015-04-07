///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.util;

namespace com.espertech.esper.view
{
    public class ViewFactoryTimePeriodHelper
    {
        public static ExprTimePeriodEvalDeltaConst ValidateAndEvaluateTimeDelta(String viewName,
                                                                               StatementContext statementContext,
                                                                               ExprNode expression,
                                                                               String expectedMessage,
                                                                               int expressionNumber)
        {
            StreamTypeService streamTypeService = new StreamTypeServiceImpl(statementContext.EngineURI, false);
            ExprTimePeriodEvalDeltaConst timeDelta;
            if (expression is ExprTimePeriod) {
                ExprTimePeriod validated = (ExprTimePeriod) ViewFactorySupport.ValidateExpr(viewName, statementContext, expression, streamTypeService, expressionNumber);
                timeDelta = validated.ConstEvaluator(new ExprEvaluatorContextStatement(statementContext, false));
            }
            else {
                var result = ViewFactorySupport.ValidateAndEvaluateExpr(viewName, statementContext, expression, streamTypeService, expressionNumber);
                if (!result.IsNumber()) {
                    throw new ViewParameterException(expectedMessage);
                }

                long millisecondsBeforeExpiry;
                if (TypeHelper.IsFloatingPointNumber(result)) {
                    millisecondsBeforeExpiry = (long) Math.Round(1000d * result.AsDouble());
                }
                else {
                    millisecondsBeforeExpiry = 1000 * result.AsLong();
                }
                timeDelta = new ExprTimePeriodEvalDeltaConstMsec(millisecondsBeforeExpiry);
            }
            if (timeDelta.DeltaMillisecondsAdd(0) < 1) {
                throw new ViewParameterException(viewName + " view requires a size of at least 1 msec");
            }
            return timeDelta;
        }
    }
}
