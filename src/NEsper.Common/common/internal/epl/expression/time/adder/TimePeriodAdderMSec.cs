///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.time.adder
{
    public class TimePeriodAdderMSec : TimePeriodAdder
    {
        public static readonly TimePeriodAdderMSec INSTANCE = new TimePeriodAdderMSec();

        public double Compute(double value)
        {
            return value / 1000d;
        }

        private TimePeriodAdderMSec()
        {
        }

        public void Add(
            DateTimeEx dtx,
            int value)
        {
            dtx.AddMilliseconds(value);
        }

        public bool IsMicroseconds {
            get => false;
        }

        public CodegenExpression ComputeCodegen(CodegenExpression doubleValue)
        {
            return Op(doubleValue, "/", Constant(1000d));
        }
    }
} // end of namespace