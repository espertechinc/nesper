///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.pattern;

namespace com.espertech.esper.filter
{
    /// <summary>
    /// A string-typed value as a filter parameter representing a range.
    /// </summary>
    public class FilterForEvalConstantString : FilterSpecParamFilterForEval
    {
        private readonly string _theStringValue;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="theStringValue">is the value of the range endpoint</param>
        public FilterForEvalConstantString(string theStringValue)
        {
            _theStringValue = theStringValue;
        }

        public object GetFilterValue(MatchedEventMap matchedEvents, ExprEvaluatorContext exprEvaluatorContext)
        {
            return _theStringValue;
        }

        public override String ToString()
        {
            return _theStringValue;
        }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;

            var that = (FilterForEvalConstantString) o;

            if (_theStringValue != null ? !_theStringValue.Equals(that._theStringValue) : that._theStringValue != null)
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            return _theStringValue != null ? _theStringValue.GetHashCode() : 0;
        }
    }
} // end of namespace