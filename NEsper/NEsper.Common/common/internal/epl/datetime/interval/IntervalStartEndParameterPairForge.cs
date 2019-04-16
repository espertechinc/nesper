///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.datetime.interval
{
    public class IntervalStartEndParameterPairForge
    {
        public IntervalStartEndParameterPairForge(
            ExprOptionalConstantForge start,
            ExprOptionalConstantForge end)
        {
            Start = start;
            End = end;
        }

        public ExprOptionalConstantForge Start { get; }

        public ExprOptionalConstantForge End { get; }

        public bool IsConstant => Start.OptionalConstant != null && End.OptionalConstant != null;

        public IntervalStartEndParameterPairEval MakeEval()
        {
            return new IntervalStartEndParameterPairEval(Start.MakeEval(), End.MakeEval());
        }

        public static IntervalStartEndParameterPairForge FromParamsWithSameEnd(ExprOptionalConstantForge[] parameters)
        {
            var start = parameters[0];
            ExprOptionalConstantForge end;
            if (parameters.Length == 1) {
                end = start;
            }
            else {
                end = parameters[1];
            }

            return new IntervalStartEndParameterPairForge(start, end);
        }

        public static IntervalStartEndParameterPairForge FromParamsWithLongMaxEnd(
            ExprOptionalConstantForge[] parameters)
        {
            var start = parameters[0];
            ExprOptionalConstantForge end;
            if (parameters.Length == 1) {
                end = ExprOptionalConstantForge.Make(long.MaxValue);
            }
            else {
                end = parameters[1];
            }

            return new IntervalStartEndParameterPairForge(start, end);
        }
    }
} // end of namespace