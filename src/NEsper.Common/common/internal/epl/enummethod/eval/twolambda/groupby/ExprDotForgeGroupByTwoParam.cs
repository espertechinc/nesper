///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.enummethod.eval.twolambda.@base;
using com.espertech.esper.common.@internal.rettype;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.twolambda.groupby
{
    public class ExprDotForgeGroupByTwoParam : ExprDotForgeTwoLambda
    {
        protected override TwoLambdaThreeFormEventPlainFactory.ForgeFunction TwoParamEventPlain()
        {
            return (
                first,
                second,
                streamCountIncoming,
                services) => BuildDesc(
                first,
                second,
                new EnumGroupByTwoParamEventPlain(first.BodyForge, streamCountIncoming, second.BodyForge));
        }

        protected override TwoLambdaThreeFormEventPlusFactory.ForgeFunction TwoParamEventPlus()
        {
            return (
                first,
                second,
                streamCountIncoming,
                firstType,
                secondType,
                numParameters,
                services) => BuildDesc(
                first,
                second,
                new EnumGroupByTwoParamEventPlus(
                    first.BodyForge,
                    streamCountIncoming,
                    firstType,
                    second.BodyForge,
                    numParameters));
        }

        protected override TwoLambdaThreeFormScalarFactory.ForgeFunction TwoParamScalar()
        {
            return (
                    first,
                    second,
                    eventTypeFirst,
                    eventTypeSecond,
                    streamCountIncoming,
                    numParams)
                => BuildDesc(
                    first,
                    second,
                    new EnumGroupByTwoParamScalar(
                        first.BodyForge,
                        streamCountIncoming,
                        second.BodyForge,
                        eventTypeFirst,
                        numParams));
        }

        private EnumForgeDesc BuildDesc(
            ExprDotEvalParamLambda first,
            ExprDotEvalParamLambda second,
            EnumForge forge)
        {
            var key = first.BodyForge.EvaluationType;
            var component = second.BodyForge.EvaluationType;
            var collection = typeof(ICollection<>).MakeGenericType(component);
            var map = typeof(IDictionary<,>).MakeGenericType(key, collection);
            var type = new EPChainableTypeClass(map);
            return new EnumForgeDesc(type, forge);
        }
    }
} // end of namespace