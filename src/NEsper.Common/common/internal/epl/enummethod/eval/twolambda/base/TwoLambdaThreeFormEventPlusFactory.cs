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
    public class TwoLambdaThreeFormEventPlusFactory : EnumForgeDescFactory
    {
        private readonly EventType inputEventType;
        private readonly string streamNameFirst;
        private readonly string streamNameSecond;
        private readonly ObjectArrayEventType typeKey;
        private readonly ObjectArrayEventType typeValue;
        private readonly int numParams;
        private readonly ForgeFunction function;

        public TwoLambdaThreeFormEventPlusFactory(
            EventType inputEventType,
            string streamNameFirst,
            string streamNameSecond,
            ObjectArrayEventType typeKey,
            ObjectArrayEventType typeValue,
            int numParams,
            ForgeFunction function)
        {
            this.inputEventType = inputEventType;
            this.streamNameFirst = streamNameFirst;
            this.streamNameSecond = streamNameSecond;
            this.typeKey = typeKey;
            this.typeValue = typeValue;
            this.numParams = numParams;
            this.function = function;
        }

        public EnumForgeLambdaDesc GetLambdaStreamTypesForParameter(int parameterNum)
        {
            return parameterNum == 0 ? MakeDesc(typeKey, streamNameFirst) : MakeDesc(typeValue, streamNameSecond);
        }

        public EnumForgeDesc MakeEnumForgeDesc(
            IList<ExprDotEvalParam> bodiesAndParameters,
            int streamCountIncoming,
            StatementCompileTimeServices services)
        {
            var key = (ExprDotEvalParamLambda)bodiesAndParameters[0];
            var value = (ExprDotEvalParamLambda)bodiesAndParameters[1];
            return function.Invoke(key, value, streamCountIncoming, typeKey, typeValue, numParams, services);
        }

        private EnumForgeLambdaDesc MakeDesc(
            ObjectArrayEventType type,
            string streamName)
        {
            return new EnumForgeLambdaDesc(
                new[] { inputEventType, type },
                new[] { streamName, type.Name });
        }

        public delegate EnumForgeDesc ForgeFunction(
            ExprDotEvalParamLambda first,
            ExprDotEvalParamLambda second,
            int streamCountIncoming,
            ObjectArrayEventType firstType,
            ObjectArrayEventType secondType,
            int numParameters,
            StatementCompileTimeServices services);
    }
} // end of namespace