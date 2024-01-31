///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.expression.time.adder
{
    public class TimePeriodAdderDay : TimePeriodAdder
    {
        public static readonly TimePeriodAdderDay INSTANCE = new TimePeriodAdderDay();

        private const double MULTIPLIER = 24 * 60 * 60;

        private TimePeriodAdderDay()
        {
        }

        public double Compute(double value)
        {
            return value * MULTIPLIER;
        }

        public void Add(
            DateTimeEx dtx,
            int value)
        {
            dtx.AddDays(value);
        }

        public bool IsMicroseconds => false;

        public CodegenExpression ComputeCodegen(CodegenExpression doubleValue)
        {
            return TimePeriodAdderUtil.ComputeCodegenTimesMultiplier(doubleValue, MULTIPLIER);
        }
    }
} // end of namespace