///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.streamtype;

using static com.espertech.esper.common.@internal.epl.expression.core.ExprNodeUtilityEvaluate;

namespace com.espertech.esper.common.@internal.schedule
{
    public class ScheduleExpressionUtil
    {
        public static ExprForge[] CrontabScheduleValidate(
            ExprNodeOrigin origin,
            IList<ExprNode> scheduleSpecExpressionList,
            bool allowBindingConsumption,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            // Validate the expressions
            ExprForge[] expressions = new ExprForge[scheduleSpecExpressionList.Count];
            int count = 0;
            ExprValidationContext validationContext =
                new ExprValidationContextBuilder(new StreamTypeServiceImpl(false), statementRawInfo, services)
                    .WithAllowBindingConsumption(allowBindingConsumption)
                    .Build();
            foreach (ExprNode parameters in scheduleSpecExpressionList) {
                ExprNode node = ExprNodeUtilityValidate.GetValidatedSubtree(origin, parameters, validationContext);
                expressions[count++] = node.Forge;
            }

            if (expressions.Length <= 4 || expressions.Length >= 8) {
                throw new ExprValidationException(
                    "Invalid schedule specification: " +
                    ScheduleSpecUtil.GetExpressionCountException(expressions.Length));
            }

            return expressions;
        }

        public static ScheduleSpec CrontabScheduleBuild(
            ExprEvaluator[] scheduleSpecEvaluators,
            ExprEvaluatorContext context)
        {
            try {
                object[] scheduleSpecParameterList = EvaluateExpressions(scheduleSpecEvaluators, context);
                return ScheduleSpecUtil.ComputeValues(scheduleSpecParameterList);
            }
            catch (ScheduleParameterException e) {
                throw new EPException("Invalid schedule specification: " + e.Message, e);
            }
        }
    }
} // end of namespace