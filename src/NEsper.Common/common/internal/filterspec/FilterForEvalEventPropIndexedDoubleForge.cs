///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Text;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.filterspec.FilterSpecParam;

namespace com.espertech.esper.common.@internal.filterspec
{
    /// <summary>
    ///     An event property as a filter parameter representing a range.
    /// </summary>
    public class FilterForEvalEventPropIndexedDoubleForge : FilterSpecParamFilterForEvalDoubleForge
    {
        private readonly EventType _eventType;
        private readonly string _resultEventAsName;
        private readonly int _resultEventIndex;
        private readonly string _resultEventProperty;

        public FilterForEvalEventPropIndexedDoubleForge(
            string resultEventAsName,
            int resultEventIndex,
            string resultEventProperty,
            EventType eventType)
        {
            _resultEventAsName = resultEventAsName;
            _resultEventIndex = resultEventIndex;
            _resultEventProperty = resultEventProperty;
            _eventType = eventType;
        }

        public CodegenExpression MakeCodegen(
            CodegenClassScope classScope,
            CodegenMethodScope parent)
        {
            var getterSPI = ((EventTypeSPI) _eventType).GetGetterSPI(_resultEventProperty);
            var method = parent.MakeChild(typeof(object), GetType(), classScope)
				.AddParam(GET_FILTER_VALUE_FP);
            method.Block
                .DeclareVar<EventBean[]>(
                    "events",
                    Cast(
                        typeof(EventBean[]),
                        ExprDotMethod(
                            Ref("matchedEvents"),
                            "GetMatchingEventAsObjectByTag",
                            Constant(_resultEventAsName))))
                .DeclareVar<object>("value", ConstantNull())
                .IfRefNotNull("events")
                .AssignRef(
                    "value",
                    Cast(
                        typeof(object),
                        getterSPI.EventBeanGetCodegen(
                            ArrayAtIndex(Ref("events"), Constant(_resultEventIndex)),
                            method,
                            classScope)))
                .BlockEnd()
                .IfRefNullReturnNull("value")
                .MethodReturn(ExprDotMethod(Ref("value"), "AsDouble"));
            return LocalMethod(method, GET_FILTER_VALUE_REFS);
        }

        object FilterSpecParamFilterForEvalForge.GetFilterValue(
            MatchedEventMap matchedEvents,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return GetFilterValue(matchedEvents, exprEvaluatorContext);
        }

        public double? GetFilterValue(
            MatchedEventMap matchedEvents,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();
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

            if (!(obj is FilterForEvalEventPropIndexedDoubleForge)) {
                return false;
            }

            var other = (FilterForEvalEventPropIndexedDoubleForge) obj;
            if (other._resultEventAsName.Equals(_resultEventAsName) &&
                other._resultEventProperty.Equals(_resultEventProperty) &&
                other._resultEventIndex == _resultEventIndex) {
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
            @out.Append("indexed event property '")
                .Append(_resultEventProperty)
                .Append("'");
        }
    }
} // end of namespace