///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.datetime.interval
{
    public class IntervalStartEndParameterPairEval
    {
        public IntervalStartEndParameterPairEval(
            ExprOptionalConstantEval start,
            ExprOptionalConstantEval end)
        {
            Start = start;
            End = end;
        }

        public ExprOptionalConstantEval Start { get; }

        public ExprOptionalConstantEval End { get; }

        public bool IsConstant()
        {
            return Start.OptionalConstant != null && End.OptionalConstant != null;
        }
    }
} // end of namespace