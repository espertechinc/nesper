///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.time.eval;
using com.espertech.esper.common.@internal.epl.expression.time.node;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.view.util
{
    public class ViewFactoryTimePeriodHelper
    {
        public static TimePeriodComputeForge ValidateAndEvaluateTimeDeltaFactory(
            string viewName,
            ExprNode expression,
            string expectedMessage,
            int expressionNumber,
            ViewForgeEnv viewForgeEnv)
        {
            StreamTypeService streamTypeService = new StreamTypeServiceImpl(false);
            TimePeriodComputeForge forge;
            if (expression is ExprTimePeriod) {
                var validated = (ExprTimePeriod)ViewForgeSupport.ValidateExpr(
                    viewName,
                    expression,
                    streamTypeService,
                    viewForgeEnv,
                    expressionNumber);
                forge = validated.TimePeriodComputeForge;
            }
            else {
                var validated = ViewForgeSupport.ValidateExpr(
                    viewName,
                    expression,
                    streamTypeService,
                    viewForgeEnv,
                    expressionNumber);
                var returnType = validated.Forge.EvaluationType.GetBoxedType();
                if (!returnType.IsTypeNumeric()) {
                    throw new ViewParameterException(expectedMessage);
                }

                if (validated.Forge.ForgeConstantType.IsCompileTimeConstant) {
                    var timeAbacus = viewForgeEnv.ImportServiceCompileTime.TimeAbacus;
                    var secondsEvaluator = validated.Forge.ExprEvaluator;
                    var time = ViewForgeSupport.Evaluate(secondsEvaluator, 0, viewName);
                    if (!time.IsNumber()) {
                        throw new IllegalStateException(nameof(time) + " is not a number");
                    }

                    if (!ExprTimePeriodUtil.ValidateTime(time, timeAbacus)) {
                        throw new ViewParameterException(ExprTimePeriodUtil.GetTimeInvalidMsg(viewName, "view", time));
                    }

                    var msec = timeAbacus.DeltaForSecondsNumber(time);
                    forge = new TimePeriodComputeConstGivenDeltaForge(msec);
                }
                else {
                    forge = new TimePeriodComputeNCGivenExprForge(
                        validated.Forge,
                        viewForgeEnv.ImportServiceCompileTime.TimeAbacus);
                }
            }

            return forge;
        }
    }
} // end of namespace