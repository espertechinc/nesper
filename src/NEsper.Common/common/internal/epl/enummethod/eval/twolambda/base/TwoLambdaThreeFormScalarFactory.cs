///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.@event.arr;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.twolambda.@base
{
    public class TwoLambdaThreeFormScalarFactory : EnumForgeDescFactory
    {
        private readonly ObjectArrayEventType typeFirst;
        private readonly ObjectArrayEventType typeSecond;
        private readonly int numParams;
        private readonly ForgeFunction function;

        public TwoLambdaThreeFormScalarFactory(
            ObjectArrayEventType typeFirst,
            ObjectArrayEventType typeSecond,
            int numParams,
            ForgeFunction function)
        {
            this.typeFirst = typeFirst;
            this.typeSecond = typeSecond;
            this.numParams = numParams;
            this.function = function;
        }

        public EnumForgeLambdaDesc GetLambdaStreamTypesForParameter(int parameterNum)
        {
            return parameterNum == 0 ? MakeDesc(typeFirst) : MakeDesc(typeSecond);
        }

        public EnumForgeDesc MakeEnumForgeDesc(
            IList<ExprDotEvalParam> bodiesAndParameters,
            int streamCountIncoming,
            StatementCompileTimeServices services)
        {
            var first = (ExprDotEvalParamLambda)bodiesAndParameters[0];
            var second = (ExprDotEvalParamLambda)bodiesAndParameters[1];
            return function.Invoke(first, second, typeFirst, typeSecond, streamCountIncoming, numParams);
        }

        private static EnumForgeLambdaDesc MakeDesc(ObjectArrayEventType type)
        {
            return new EnumForgeLambdaDesc(
                new EventType[] { type },
                new string[] {
                    type.Name
                });
        }

        public delegate EnumForgeDesc ForgeFunction(
            ExprDotEvalParamLambda first,
            ExprDotEvalParamLambda second,
            ObjectArrayEventType eventTypeFirst,
            ObjectArrayEventType eventTypeSecond,
            int streamCountIncoming,
            int numParams);
    }
} // end of namespace