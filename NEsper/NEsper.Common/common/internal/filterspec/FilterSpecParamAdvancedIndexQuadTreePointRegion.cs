///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
        private readonly FilterSpecParamFilterForEvalDouble _xEval;
        private readonly FilterSpecParamFilterForEvalDouble _yEval;

        public FilterSpecParamAdvancedIndexQuadTreePointRegion(
            ExprFilterSpecLookupable lookupable,
            FilterOperator filterOperator) : base(lookupable, filterOperator)
        {
        }

        public override object GetFilterValue(
            MatchedEventMap matchedEvents, 
            ExprEvaluatorContext exprEvaluatorContext,
            StatementContextFilterEvalEnv filterEvalEnv)
        {
            var x = _xEval.GetFilterValueDouble(matchedEvents, exprEvaluatorContext, filterEvalEnv);
            var y = _yEval.GetFilterValueDouble(matchedEvents, exprEvaluatorContext, filterEvalEnv);
            return new XYPoint(x, y);
        }

        private bool Equals(FilterSpecParamAdvancedIndexQuadTreePointRegion other)
        {
            return base.Equals(other) && Equals(_xEval, other._xEval) && Equals(_yEval, other._yEval);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is FilterSpecParamAdvancedIndexQuadTreePointRegion &&
                   Equals((FilterSpecParamAdvancedIndexQuadTreePointRegion) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (_xEval != null ? _xEval.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_yEval != null ? _yEval.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
} // end of namespace