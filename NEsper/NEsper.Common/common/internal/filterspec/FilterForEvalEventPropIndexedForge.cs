///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.filterspec.FilterSpecParam;

namespace com.espertech.esper.common.@internal.filterspec
{
    /// <summary>
    ///     Event property value in a list of values following an in-keyword.
    /// </summary>
    public class FilterForEvalEventPropIndexedForge : FilterSpecParamInValueForge
    {
        private readonly EventType _eventType;
        private readonly bool _isMustCoerce;
        private readonly int _resultEventIndex;

        public FilterForEvalEventPropIndexedForge(
            string resultEventAsName,
            int resultEventindex,
            string resultEventProperty,
            EventType eventType,
            bool isMustCoerce,
            Type coercionType)
        {
            ResultEventAsName = resultEventAsName;
            ResultEventProperty = resultEventProperty;
            _resultEventIndex = resultEventindex;
            ReturnType = coercionType;
            _isMustCoerce = isMustCoerce;
            _eventType = eventType;
        }

        /// <summary>
        ///     Returns the tag used for the event property.
        /// </summary>
        /// <returns>tag</returns>
        public string ResultEventAsName { get; }

        /// <summary>
        ///     Returns the event property name.
        /// </summary>
        /// <returns>property name</returns>
        public string ResultEventProperty { get; }

        public Type ReturnType { get; }

        public CodegenExpression MakeCodegen(
            CodegenClassScope classScope,
            CodegenMethodScope parent)
        {
            var getterSPI = ((EventTypeSPI) _eventType).GetGetterSPI(ResultEventProperty);
            var method = parent.MakeChild(typeof(object), GetType(), classScope).AddParam(GET_FILTER_VALUE_FP);
            method.Block
                .DeclareVar(
                    typeof(EventBean[]), "events",
                    Cast(
                        typeof(EventBean[]),
                        ExprDotMethod(
                            Ref("matchedEvents"), "getMatchingEventAsObjectByTag",
                            CodegenExpressionBuilder.Constant(ResultEventAsName))))
                .DeclareVar(typeof(object), "value", ConstantNull())
                .IfRefNotNull("events")
                .AssignRef(
                    "value",
                    getterSPI.EventBeanGetCodegen(
                        ArrayAtIndex(Ref("events"), CodegenExpressionBuilder.Constant(_resultEventIndex)), method,
                        classScope))
                .BlockEnd();

            if (_isMustCoerce) {
                method.Block.AssignRef(
                    "value", TypeHelper.CoerceNumberToBoxedCodegen(Ref("value"), typeof(object), ReturnType));
            }

            method.Block.MethodReturn(Ref("value"));
            return LocalMethod(method, GET_FILTER_VALUE_REFS);
        }

        public bool IsConstant => false;

        public object GetFilterValue(
            MatchedEventMap matchedEvents,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();
        }

        public bool Constant()
        {
            return false;
        }

        public override string ToString()
        {
            return "resultEventProp=" + ResultEventAsName + '.' + ResultEventProperty;
        }

        public override bool Equals(object obj)
        {
            if (this == obj) {
                return true;
            }

            if (!(obj is FilterForEvalEventPropIndexedForge)) {
                return false;
            }

            var other = (FilterForEvalEventPropIndexedForge) obj;
            if (other.ResultEventAsName.Equals(ResultEventAsName) &&
                other.ResultEventProperty.Equals(ResultEventProperty)) {
                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return ResultEventProperty.GetHashCode();
        }
    }
} // end of namespace