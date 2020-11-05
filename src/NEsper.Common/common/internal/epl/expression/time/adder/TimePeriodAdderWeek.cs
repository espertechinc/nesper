///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.expression.time.adder
{
    public class TimePeriodAdderWeek : TimePeriodAdder
    {
        private const double MULTIPLIER = 7 * 24 * 60 * 60;

        public static readonly TimePeriodAdderWeek INSTANCE = new TimePeriodAdderWeek();

        private TimePeriodAdderWeek()
        {
        }

        public double Compute(Double value)
        {
            return value * MULTIPLIER;
        }

        public void Add(
            DateTimeEx dtx,
            int value)
        {
            dtx.AddDays(7 * value);
        }

        public bool IsMicroseconds {
            get => false;
        }

        public CodegenExpression ComputeCodegen(CodegenExpression doubleValue)
        {
            return TimePeriodAdderUtil.ComputeCodegenTimesMultiplier(doubleValue, MULTIPLIER);
        }
    }
} // end of namespace