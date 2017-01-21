///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.epl.datetime.interval
{
    public class IntervalStartEndParameterPair {
        private readonly ExprOptionalConstant _start;
        private readonly ExprOptionalConstant _end;
    
        private IntervalStartEndParameterPair(ExprOptionalConstant start, ExprOptionalConstant end) {
            _start = start;
            _end = end;
        }
    
        public static IntervalStartEndParameterPair FromParamsWithSameEnd(ExprOptionalConstant[] paramList) {
            ExprOptionalConstant start = paramList[0];
            ExprOptionalConstant end = paramList.Length == 1 ? start : paramList[1];
            return new IntervalStartEndParameterPair(start, end);
        }
    
        public static IntervalStartEndParameterPair FromParamsWithLongMaxEnd(ExprOptionalConstant[] paramList) {
            ExprOptionalConstant start = paramList[0];
            ExprOptionalConstant end = paramList.Length == 1 ? ExprOptionalConstant.Make(long.MaxValue) : paramList[1];
            return new IntervalStartEndParameterPair(start, end);
        }

        public ExprOptionalConstant Start
        {
            get { return _start; }
        }

        public ExprOptionalConstant End
        {
            get { return _end; }
        }

        public bool IsConstant
        {
            get { return _start.OptionalConstant != null && _end.OptionalConstant != null; }
        }
    }
}
