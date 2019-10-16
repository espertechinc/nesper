///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Numerics;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public partial class ExprDotForgeSumOf
    {
        internal class ExprDotEvalSumMethodBigInteger : ExprDotEvalSumMethod
        {
            private long cnt;
            private BigInteger sum;

            public ExprDotEvalSumMethodBigInteger()
            {
                sum = BigInteger.Zero;
            }

            public void Enter(object @object)
            {
                if (@object == null) {
                    return;
                }

                cnt++;
                sum = BigInteger.Add(sum, (BigInteger) @object);
            }

            public object Value {
                get {
                    if (cnt == 0) {
                        return null;
                    }

                    return sum;
                }
            }
        }
    }
}