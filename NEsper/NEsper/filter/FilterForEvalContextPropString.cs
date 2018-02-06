///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.pattern;

namespace com.espertech.esper.filter
{
    [Serializable]
    public class FilterForEvalContextPropString : FilterSpecParamFilterForEval
    {
        [NonSerialized] private readonly EventPropertyGetter _getter;

        private readonly string _propertyName;

        public FilterForEvalContextPropString(EventPropertyGetter getter, string propertyName)
        {
            _getter = getter;
            _propertyName = propertyName;
        }

        public object GetFilterValue(MatchedEventMap matchedEvents, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (exprEvaluatorContext.ContextProperties == null) return null;
            return _getter.Get(exprEvaluatorContext.ContextProperties);
        }

        protected bool Equals(FilterForEvalContextPropString other)
        {
            return string.Equals(_propertyName, other._propertyName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((FilterForEvalContextPropString) obj);
        }

        public override int GetHashCode()
        {
            return _propertyName != null ? _propertyName.GetHashCode() : 0;
        }
    }
} // end of namespace