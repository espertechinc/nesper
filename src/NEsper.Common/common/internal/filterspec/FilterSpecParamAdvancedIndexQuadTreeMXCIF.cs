///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.type;

namespace com.espertech.esper.common.@internal.filterspec
{
    public class FilterSpecParamAdvancedIndexQuadTreeMXCIF : FilterSpecParam
    {
        public FilterSpecParamAdvancedIndexQuadTreeMXCIF(
            ExprFilterSpecLookupable lkupable,
            FilterOperator filterOperator)
            : base(lkupable, filterOperator)
        {
        }

        public FilterSpecParamFilterForEvalDouble XEval { get; set; }

        public FilterSpecParamFilterForEvalDouble YEval { get; set; }

        public FilterSpecParamFilterForEvalDouble WidthEval { get; set; }

        public FilterSpecParamFilterForEvalDouble HeightEval { get; set; }

        public override FilterValueSetParam GetFilterValue(
            MatchedEventMap matchedEvents,
            ExprEvaluatorContext exprEvaluatorContext,
            StatementContextFilterEvalEnv filterEvalEnv)
        {
            var x = XEval.GetFilterValueDouble(matchedEvents, exprEvaluatorContext, filterEvalEnv);
            var y = YEval.GetFilterValueDouble(matchedEvents, exprEvaluatorContext, filterEvalEnv);
            var width = WidthEval.GetFilterValueDouble(matchedEvents, exprEvaluatorContext, filterEvalEnv);
            var height = HeightEval.GetFilterValueDouble(matchedEvents, exprEvaluatorContext, filterEvalEnv);
            var rectangle = new XYWHRectangle(x, y, width, height);
            var lookupable = lkupable.Make(matchedEvents, exprEvaluatorContext);
            return new FilterValueSetParamImpl(lookupable, FilterOperator, rectangle);
        }

        protected bool Equals(FilterSpecParamAdvancedIndexQuadTreeMXCIF other)
        {
            return Equals(XEval, other.XEval) &&
                   Equals(YEval, other.YEval) &&
                   Equals(WidthEval, other.WidthEval) &&
                   Equals(HeightEval, other.HeightEval);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((FilterSpecParamAdvancedIndexQuadTreeMXCIF) obj);
        }

        public override int GetHashCode()
        {
            unchecked {
                var hashCode = XEval != null ? XEval.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (YEval != null ? YEval.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (WidthEval != null ? WidthEval.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (HeightEval != null ? HeightEval.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
} // end of namespace