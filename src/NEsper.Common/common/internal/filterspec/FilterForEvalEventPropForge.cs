///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.filterspec.FilterSpecParam;

namespace com.espertech.esper.common.@internal.filterspec
{
    /// <summary>
    ///     Event property value in a list of values following an in-keyword.
    /// </summary>
    public class FilterForEvalEventPropForge : FilterSpecParamInValueForge
    {
        private readonly ExprIdentNodeEvaluator _exprIdentNodeEvaluator;
        private readonly bool _isMustCoerce;
        private readonly string _resultEventAsName;
        private readonly string _resultEventProperty;

        public FilterForEvalEventPropForge(
            string resultEventAsName,
            string resultEventProperty,
            ExprIdentNodeEvaluator exprIdentNodeEvaluator,
            bool isMustCoerce,
            Type coercionType)
        {
            _resultEventAsName = resultEventAsName;
            _resultEventProperty = resultEventProperty;
            _exprIdentNodeEvaluator = exprIdentNodeEvaluator;
            ReturnType = coercionType;
            _isMustCoerce = isMustCoerce;
        }

        public Type ReturnType { get; }

        public bool IsConstant => false;

        public object GetFilterValue(
            MatchedEventMap matchedEvents,
            ExprEvaluatorContext evaluatorContext)
        {
            throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();
        }

        public CodegenExpression MakeCodegen(
            CodegenClassScope classScope,
            CodegenMethodScope parent)
        {
            var method = parent.MakeChild(typeof(object), GetType(), classScope)
				.AddParam(GET_FILTER_VALUE_FP);
            var get = _exprIdentNodeEvaluator.Getter.EventBeanGetCodegen(Ref("@event"), method, classScope);

            method.Block
                .DeclareVar<EventBean>(
                    "@event",
                    ExprDotMethod(Ref("matchedEvents"), "GetMatchingEventByTag", Constant(_resultEventAsName)))
                .IfNull(Ref("@event"))
                .BlockThrow(
                    NewInstance<IllegalStateException>(
                        Constant("Matching event named '" + _resultEventAsName + "' not found in event result set")))
                .DeclareVar<object>("value", get);

            if (_isMustCoerce) {
                method.Block.AssignRef(
                    "value",
                    TypeHelper.CoerceNumberBoxedToBoxedCodegen(
                        Cast(typeof(object), Ref("value")),
                        typeof(object),
                        ReturnType));
            }

            method.Block.MethodReturn(Ref("value"));
            return LocalMethod(method, GET_FILTER_VALUE_REFS);
        }

        public override string ToString()
        {
            return "resultEventProp=" + _resultEventAsName + '.' + _resultEventProperty;
        }

        public override bool Equals(object obj)
        {
            if (this == obj) {
                return true;
            }

            if (!(obj is FilterForEvalEventPropForge)) {
                return false;
            }

            var other = (FilterForEvalEventPropForge) obj;
            if (other._resultEventAsName.Equals(_resultEventAsName) &&
                other._resultEventProperty.Equals(_resultEventProperty)) {
                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return _resultEventProperty.GetHashCode();
        }
        
        public void ValueToString(StringBuilder @out)
        {
            @out.Append("property '")
                .Append(_resultEventProperty)
                .Append("'");
            if (_resultEventAsName != null) {
                @out.Append(" of '")
                    .Append(_resultEventAsName)
                    .Append("'");
            }
        }
    }
} // end of namespace