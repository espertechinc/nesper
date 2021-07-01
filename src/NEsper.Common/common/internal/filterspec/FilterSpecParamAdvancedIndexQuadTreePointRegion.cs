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
    public sealed class FilterSpecParamAdvancedIndexQuadTreePointRegion : FilterSpecParam
    {
        public FilterSpecParamAdvancedIndexQuadTreePointRegion(
            ExprFilterSpecLookupable lkupable,
            FilterOperator filterOperator)
            : base(lkupable, filterOperator)
        {
        }

        public FilterSpecParamFilterForEvalDouble XEval { get; set; }

        public FilterSpecParamFilterForEvalDouble YEval { get; set; }

        public override FilterValueSetParam GetFilterValue(
            MatchedEventMap matchedEvents,
            ExprEvaluatorContext exprEvaluatorContext,
            StatementContextFilterEvalEnv filterEvalEnv)
        {
            var x = XEval.GetFilterValueDouble(matchedEvents, exprEvaluatorContext, filterEvalEnv);
            var y = YEval.GetFilterValueDouble(matchedEvents, exprEvaluatorContext, filterEvalEnv);
            var point = new XYPoint(x, y);
            var lookupable = this.lkupable.Make(matchedEvents, exprEvaluatorContext);
            return new FilterValueSetParamImpl(lookupable, FilterOperator, point);
        }

        private bool Equals(FilterSpecParamAdvancedIndexQuadTreePointRegion other)
        {
            return base.Equals(other) && Equals(XEval, other.XEval) && Equals(YEval, other.YEval);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            return obj is FilterSpecParamAdvancedIndexQuadTreePointRegion &&
                   Equals((FilterSpecParamAdvancedIndexQuadTreePointRegion) obj);
        }

        public override int GetHashCode()
        {
            unchecked {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (XEval != null ? XEval.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (YEval != null ? YEval.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
} // end of namespace