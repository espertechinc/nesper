///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.expression.time.adder
{
    public class TimePeriodAdderMinute : TimePeriodAdder
    {
        public static readonly TimePeriodAdderMinute INSTANCE = new TimePeriodAdderMinute();

        private const double MULTIPLIER = 60;

        private TimePeriodAdderMinute()
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
            dtx.AddMinutes(value);
        }

        public bool IsMicroseconds => false;

        public CodegenExpression ComputeCodegen(CodegenExpression doubleValue)
        {
            return TimePeriodAdderUtil.ComputeCodegenTimesMultiplier(doubleValue, MULTIPLIER);
        }
    }
} // end of namespace