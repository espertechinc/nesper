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
    public class TimePeriodAdderSecond : TimePeriodAdder
    {
        public static readonly TimePeriodAdderSecond INSTANCE = new TimePeriodAdderSecond();

        public double Compute(double value)
        {
            return value;
        }

        private TimePeriodAdderSecond()
        {
        }

        public void Add(
            DateTimeEx dtx,
            int value)
        {
            dtx.AddSeconds(value);
        }

        public bool IsMicroseconds => false;

        public CodegenExpression ComputeCodegen(CodegenExpression doubleValue)
        {
            return doubleValue;
        }
    }
} // end of namespace