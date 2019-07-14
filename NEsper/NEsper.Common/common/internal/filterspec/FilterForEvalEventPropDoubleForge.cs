///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.filterspec.FilterSpecParam;

namespace com.espertech.esper.common.@internal.filterspec
{
    /// <summary>
    ///     An event property as a filter parameter representing a range.
    /// </summary>
    public class FilterForEvalEventPropDoubleForge : FilterSpecParamFilterForEvalDoubleForge
    {
        private readonly ExprIdentNodeEvaluator _exprIdentNodeEvaluator;

        public FilterForEvalEventPropDoubleForge(
            string resultEventAsName,
            string resultEventProperty,
            ExprIdentNodeEvaluator exprIdentNodeEvaluator)
        {
            ResultEventAsName = resultEventAsName;
            ResultEventProperty = resultEventProperty;
            _exprIdentNodeEvaluator = exprIdentNodeEvaluator;
        }

        /// <summary>
        ///     Returns the tag name or stream name to use for the event property.
        /// </summary>
        /// <returns>tag name</returns>
        public string ResultEventAsName { get; }

        /// <summary>
        ///     Returns the name of the event property.
        /// </summary>
        /// <returns>event property name</returns>
        public string ResultEventProperty { get; }

        public CodegenExpression MakeCodegen(
            CodegenClassScope classScope,
            CodegenMethodScope parent)
        {
            var method = parent.MakeChild(typeof(object), GetType(), classScope).AddParam(GET_FILTER_VALUE_FP);
            var get = _exprIdentNodeEvaluator.Getter.EventBeanGetCodegen(Ref("event"), method, classScope);

            method.Block
                .DeclareVar(
                    typeof(EventBean),
                    "event",
                    ExprDotMethod(Ref("matchedEvents"), "getMatchingEventByTag", Constant(ResultEventAsName)))
                .IfRefNull(Ref("event"))
                .BlockThrow(
                    NewInstance<IllegalStateException>(
                        Constant("Matching event named '" + ResultEventAsName + "' not found in event result set")))
                .DeclareVar(typeof(object), "value", Cast(typeof(object), get))
                .IfRefNull("value")
                .BlockReturn(ConstantNull())
                .MethodReturn(ExprDotMethod(Ref("value"), "doubleValue"));

            return LocalMethod(method, GET_FILTER_VALUE_REFS);
        }

        public object GetFilterValue(
            MatchedEventMap matchedEvents,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            throw new IllegalStateException("Cannot evaluate");
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

            if (!(obj is FilterForEvalEventPropDoubleForge)) {
                return false;
            }

            var other = (FilterForEvalEventPropDoubleForge) obj;
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