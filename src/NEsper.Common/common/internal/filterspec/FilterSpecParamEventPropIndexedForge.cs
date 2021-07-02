///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Text;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.logging;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.filterspec
{
    /// <summary>
    ///     This class represents a filter parameter containing a reference to another event's property
    ///     in the event pattern result, for use to describe a filter parameter in a filter specification.
    /// </summary>
    public class FilterSpecParamEventPropIndexedForge : FilterSpecParamForge
    {
        [NonSerialized] private readonly Coercer _numberCoercer;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="lookupable">is the lookupable</param>
        /// <param name="filterOperator">is the type of compare</param>
        /// <param name="resultEventAsName">is the name of the result event from which to get a property value to compare</param>
        /// <param name="resultEventProperty">is the name of the property to get from the named result event</param>
        /// <param name="isMustCoerce">indicates on whether numeric coercion must be performed</param>
        /// <param name="coercionType">indicates the numeric coercion type to use</param>
        /// <param name="numberCoercer">interface to use to perform coercion</param>
        /// <param name="resultEventIndex">index</param>
        /// <param name="eventType">event type</param>
        /// <throws>ArgumentException if an operator was supplied that does not take a single constant value</throws>
        public FilterSpecParamEventPropIndexedForge(
            ExprFilterSpecLookupableForge lookupable,
            FilterOperator filterOperator,
            string resultEventAsName,
            int resultEventIndex,
            string resultEventProperty,
            EventType eventType,
            bool isMustCoerce,
            Coercer numberCoercer,
            Type coercionType)
            : base(lookupable, filterOperator)
        {
            ResultEventAsName = resultEventAsName;
            ResultEventIndex = resultEventIndex;
            ResultEventProperty = resultEventProperty;
            EventType = eventType;
            IsMustCoerce = isMustCoerce;
            _numberCoercer = numberCoercer;
            CoercionType = coercionType;

            if (filterOperator.IsRangeOperator()) {
                throw new ArgumentException(
                    "Illegal filter operator " +
                    filterOperator +
                    " supplied to " +
                    "event property filter parameter");
            }
        }

        /// <summary>
        ///     Returns true if numeric coercion is required, or false if not
        /// </summary>
        /// <returns>true to coerce at runtime</returns>
        public bool IsMustCoerce { get; }

        /// <summary>
        ///     Returns the numeric coercion type.
        /// </summary>
        /// <returns>type to coerce to</returns>
        public Type CoercionType { get; }

        /// <summary>
        ///     Returns tag for result event.
        /// </summary>
        /// <returns>tag</returns>
        public string ResultEventAsName { get; }

        /// <summary>
        ///     Returns the property of the result event.
        /// </summary>
        /// <returns>property name</returns>
        public string ResultEventProperty { get; }

        public EventType EventType { get; }

        /// <summary>
        ///     Returns the index.
        /// </summary>
        /// <returns>index</returns>
        public int ResultEventIndex { get; }

        public override CodegenExpression MakeCodegen(
            CodegenClassScope classScope,
            CodegenMethodScope parent,
            SAIFFInitializeSymbolWEventType symbols)
        {
            var getterSPI = ((EventTypeSPI) EventType).GetGetterSPI(ResultEventProperty);
            var method = parent.MakeChild(typeof(FilterSpecParam), typeof(FilterSpecParamConstantForge), classScope);

            method.Block
                .DeclareVar<ExprFilterSpecLookupable>(
                    "lookupable",
                    LocalMethod(lookupable.MakeCodegen(method, symbols, classScope)))
                .DeclareVar<FilterOperator>("filterOperator", EnumValue(filterOperator));

            var getFilterValue = new CodegenExpressionLambda(method.Block)
                .WithParams(FilterSpecParam.GET_FILTER_VALUE_FP);
            var param = NewInstance<ProxyFilterSpecParam>(
                Ref("lookupable"),
                Ref("filterOperator"),
                getFilterValue);

            //var param = NewAnonymousClass(
            //    method.Block,
            //    typeof(FilterSpecParam),
            //    Arrays.AsList<CodegenExpression>(Ref("lookupable"), Ref("filterOperator")));
            //var getFilterValue = CodegenMethod.MakeParentNode(typeof(object), GetType(), classScope)
            //    .AddParam(FilterSpecParam.GET_FILTER_VALUE_FP);
            //param.AddMethod("GetFilterValue", getFilterValue);

            getFilterValue.Block
                .DeclareVar<EventBean[]>(
                    "events",
                    Cast(
                        typeof(EventBean[]),
                        ExprDotMethod(
                            Ref("matchedEvents"),
                            "GetMatchingEventAsObjectByTag",
                            Constant(ResultEventAsName))))
                .DeclareVar<object>("value", ConstantNull())
                .IfRefNotNull("events")
                .AssignRef(
                    "value",
                    getterSPI.EventBeanGetCodegen(
                        ArrayAtIndex(Ref("events"), Constant(ResultEventIndex)),
                        method,
                        classScope))
                .BlockEnd();

            if (IsMustCoerce) {
                getFilterValue.Block.AssignRef(
                    "value",
                    _numberCoercer.CoerceCodegenMayNullBoxed(
                        Cast(typeof(object), Ref("value")),
                        typeof(object),
                        method,
                        classScope));
            }

            getFilterValue.Block.BlockReturn(FilterValueSetParamImpl.CodegenNew(Ref("value")));

            method.Block.MethodReturn(param);
            return LocalMethod(method);
        }

        public override string ToString()
        {
            return base.ToString() +
                   " resultEventAsName=" +
                   ResultEventAsName +
                   " resultEventProperty=" +
                   ResultEventProperty;
        }

        public override bool Equals(object obj)
        {
            if (this == obj) {
                return true;
            }

            if (!(obj is FilterSpecParamEventPropIndexedForge)) {
                return false;
            }

            var other = (FilterSpecParamEventPropIndexedForge) obj;
            if (!base.Equals(other)) {
                return false;
            }

            if (!ResultEventAsName.Equals(other.ResultEventAsName) ||
                !ResultEventProperty.Equals(other.ResultEventProperty) ||
                ResultEventIndex != other.ResultEventIndex) {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            var result = base.GetHashCode();
            result = 31 * result + ResultEventProperty.GetHashCode();
            return result;
        }
        
        public override void ValueExprToString(StringBuilder @out, int i)
        {
            @out.Append("indexed event property '")
                .Append(ResultEventProperty)
                .Append("'");
        }
    }
} // end of namespace