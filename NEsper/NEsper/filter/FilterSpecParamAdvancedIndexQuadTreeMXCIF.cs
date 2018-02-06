///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.core.context.util;
using com.espertech.esper.pattern;
using com.espertech.esper.spatial.quadtree.mxcif;

namespace com.espertech.esper.filter
{
    public sealed class FilterSpecParamAdvancedIndexQuadTreeMXCIF : FilterSpecParam
    {
        private readonly FilterSpecParamFilterForEvalDouble _heightEval;
        private readonly FilterSpecParamFilterForEvalDouble _widthEval;
        private readonly FilterSpecParamFilterForEvalDouble _xEval;
        private readonly FilterSpecParamFilterForEvalDouble _yEval;

        public FilterSpecParamAdvancedIndexQuadTreeMXCIF(FilterSpecLookupable lookupable, FilterOperator filterOperator,
            FilterSpecParamFilterForEvalDouble xEval, FilterSpecParamFilterForEvalDouble yEval,
            FilterSpecParamFilterForEvalDouble widthEval, FilterSpecParamFilterForEvalDouble heightEval)
            : base(lookupable, filterOperator)
        {
            _xEval = xEval;
            _yEval = yEval;
            _widthEval = widthEval;
            _heightEval = heightEval;
        }

        public override object GetFilterValue(MatchedEventMap matchedEvents, AgentInstanceContext agentInstanceContext)
        {
            var x = _xEval.GetFilterValueDouble(matchedEvents, agentInstanceContext);
            var y = _yEval.GetFilterValueDouble(matchedEvents, agentInstanceContext);
            var width = _widthEval.GetFilterValueDouble(matchedEvents, agentInstanceContext);
            var height = _heightEval.GetFilterValueDouble(matchedEvents, agentInstanceContext);
            return new XYWHRectangle(x, y, width, height);
        }

        private bool Equals(FilterSpecParamAdvancedIndexQuadTreeMXCIF other)
        {
            return base.Equals(other)
                   && Equals(_xEval, other._xEval)
                   && Equals(_yEval, other._yEval)
                   && Equals(_widthEval, other._widthEval)
                   && Equals(_heightEval, other._heightEval);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is FilterSpecParamAdvancedIndexQuadTreeMXCIF &&
                   Equals((FilterSpecParamAdvancedIndexQuadTreeMXCIF) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (_xEval != null ? _xEval.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_yEval != null ? _yEval.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_widthEval != null ? _widthEval.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_heightEval != null ? _heightEval.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
} // end of namespace