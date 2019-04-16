///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
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
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        [NonSerialized] private readonly SimpleNumberCoercer numberCoercer;
        private readonly string statementName;

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
        /// <param name="statementName">statement name</param>
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
            SimpleNumberCoercer numberCoercer,
            Type coercionType,
            string statementName)
            : base(lookupable, filterOperator)
        {
            ResultEventAsName = resultEventAsName;
            ResultEventIndex = resultEventIndex;
            ResultEventProperty = resultEventProperty;
            EventType = eventType;
            IsMustCoerce = isMustCoerce;
            this.numberCoercer = numberCoercer;
            CoercionType = coercionType;
            this.statementName = statementName;

            if (filterOperator.IsRangeOperator()) {
                throw new ArgumentException(
                    "Illegal filter operator " + filterOperator + " supplied to " +
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

        public override CodegenMethod MakeCodegen(
            CodegenClassScope classScope,
            CodegenMethodScope parent,
            SAIFFInitializeSymbolWEventType symbols)
        {
            var getterSPI = ((EventTypeSPI) EventType).GetGetterSPI(ResultEventProperty);
            var method = parent.MakeChild(typeof(FilterSpecParam), typeof(FilterSpecParamConstantForge), classScope);

            method.Block
                .DeclareVar(
                    typeof(ExprFilterSpecLookupable), "lookupable",
                    LocalMethod(lookupable.MakeCodegen(method, symbols, classScope)))
                .DeclareVar(typeof(FilterOperator), "op", EnumValue(filterOperator));

            var param = NewAnonymousClass(
                method.Block, typeof(FilterSpecParam), CompatExtensions.AsList<CodegenExpression>(Ref("lookupable"), Ref("op")));
            var getFilterValue = CodegenMethod.MakeParentNode(typeof(object), GetType(), classScope)
                .AddParam(FilterSpecParam.GET_FILTER_VALUE_FP);
            param.AddMethod("getFilterValue", getFilterValue);
            getFilterValue.Block
                .DeclareVar(
                    typeof(EventBean[]), "events",
                    Cast(
                        typeof(EventBean[]),
                        ExprDotMethod(
                            Ref("matchedEvents"), "getMatchingEventAsObjectByTag", Constant(ResultEventAsName))))
                .DeclareVar(typeof(object), "value", ConstantNull())
                .IfRefNotNull("events")
                .AssignRef(
                    "value",
                    getterSPI.EventBeanGetCodegen(
                        ArrayAtIndex(Ref("events"), Constant(ResultEventIndex)), method, classScope))
                .BlockEnd();

            if (IsMustCoerce) {
                getFilterValue.Block.AssignRef(
                    "value",
                    numberCoercer.CoerceCodegenMayNullBoxed(
                        Cast(typeof(object), Ref("value")), typeof(object), method, classScope));
            }

            getFilterValue.Block.MethodReturn(Ref("value"));

            method.Block.MethodReturn(param);
            return method;
        }

        public override string ToString()
        {
            return base.ToString() +
                   " resultEventAsName=" + ResultEventAsName +
                   " resultEventProperty=" + ResultEventProperty;
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
    }
} // end of namespace