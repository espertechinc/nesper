///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.time.adder
{
    public class TimePeriodAdderUtil
    {
        public static CodegenExpression ComputeCodegenTimesMultiplier(
            CodegenExpression doubleValue,
            double multiplier)
        {
            return Op(doubleValue, "*", Constant(multiplier));
        }

        public static CodegenExpression AddCodegenCalendar(
            CodegenExpression dtx,
            CodegenExpression value,
            int unit)
        {
            return ExprDotMethod(dtx, "Add", Constant(unit), value);
        }

        public static CodegenExpression MakeArray(
            TimePeriodAdder[] adders,
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            var expressions = new CodegenExpression[adders.Length];
            for (var i = 0; i < adders.Length; i++) {
                expressions[i] = PublicConstValue(adders[i].GetType(), "INSTANCE");
            }

            return NewArrayWithInit(typeof(TimePeriodAdder), expressions);
        }
    }
} // end of namespace