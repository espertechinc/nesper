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
using com.espertech.esper.util;

namespace com.espertech.esper.filter
{
    /// <summary>
    ///     Event property value in a list of values following an in-keyword.
    /// </summary>
    [Serializable]
    public class FilterForEvalContextPropMayCoerce : FilterSpecParamInValue
    {
        [NonSerialized] private readonly EventPropertyGetter _getter;

        [NonSerialized] private readonly Coercer _numberCoercer;

        private readonly string _propertyName;

        [NonSerialized] private readonly Type _returnType;

        public FilterForEvalContextPropMayCoerce(string propertyName, EventPropertyGetter getter, Coercer coercer,
            Type returnType)
        {
            _propertyName = propertyName;
            _getter = getter;
            _numberCoercer = coercer;
            _returnType = returnType;
        }

        public Type ReturnType => _returnType;

        public bool IsConstant => false;

        public object GetFilterValue(MatchedEventMap matchedEvents, ExprEvaluatorContext evaluatorContext)
        {
            if (evaluatorContext.ContextProperties == null) return null;
            var result = _getter.Get(evaluatorContext.ContextProperties);
            if (_numberCoercer == null) return result;
            return _numberCoercer.Invoke(result);
        }
    }
} // end of namespace