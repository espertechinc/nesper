///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.pattern;

namespace com.espertech.esper.filter
{
    /// <summary>
    /// A String-typed value as a filter parameter representing a range.
    /// </summary>
    [Serializable]
    public class RangeValueString : FilterSpecParamRangeValue
    {
        private readonly String _theStringValue;
    
        /// <summary>Ctor. </summary>
        /// <param name="theStringValue">is the value of the range endpoint</param>
        public RangeValueString(String theStringValue)
        {
            _theStringValue = theStringValue;
        }

        public object GetFilterValue(MatchedEventMap matchedEvents,
                                     ExprEvaluatorContext exprEvaluatorContext)
        {
            return _theStringValue;
        }

        public int FilterHash
        {
            get { return _theStringValue.GetHashCode(); }
        }

        public override String ToString()
        {
            return _theStringValue;
        }

        public bool Equals(RangeValueString other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._theStringValue, _theStringValue);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (RangeValueString)) return false;
            return Equals((RangeValueString) obj);
        }

        public override int GetHashCode()
        {
            return (_theStringValue != null ? _theStringValue.GetHashCode() : 0);
        }
    }
}
