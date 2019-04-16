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
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.filterspec
{
    /// <summary>
    ///     This class represents a filter parameter containing a reference to another event's property
    ///     in the event pattern result, for use to describe a filter parameter in a <seealso cref="FilterSpecActivatable" />
    ///     filter specification.
    /// </summary>
    public class FilterSpecParamEventPropForge : FilterSpecParamForge
    {
        [NonSerialized] private readonly SimpleNumberCoercer numberCoercer;
        private readonly string statementName;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="lookupable">is the property or function to get a lookup value</param>
        /// <param name="filterOperator">is the type of compare</param>
        /// <param name="resultEventAsName">is the name of the result event from which to get a property value to compare</param>
        /// <param name="resultEventProperty">is the name of the property to get from the named result event</param>
        /// <param name="isMustCoerce">indicates on whether numeric coercion must be performed</param>
        /// <param name="coercionType">indicates the numeric coercion type to use</param>
        /// <param name="numberCoercer">interface to use to perform coercion</param>
        /// <param name="statementName">statement name</param>
        /// <param name="exprIdentNodeEvaluator">evaluator</param>
        /// <throws>ArgumentException if an operator was supplied that does not take a single constant value</throws>
        public FilterSpecParamEventPropForge(
            ExprFilterSpecLookupableForge lookupable,
            FilterOperator filterOperator,
            string resultEventAsName,
            string resultEventProperty,
            ExprIdentNodeEvaluator exprIdentNodeEvaluator,
            bool isMustCoerce,
            SimpleNumberCoercer numberCoercer,
            Type coercionType,
            string statementName)
            : base(lookupable, filterOperator)
        {
            ResultEventAsName = resultEventAsName;
            ResultEventProperty = resultEventProperty;
            ExprIdentNodeEvaluator = exprIdentNodeEvaluator;
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

        public ExprIdentNodeEvaluator ExprIdentNodeEvaluator { get; }

        public object GetFilterValue(
            MatchedEventMap matchedEvents,
            ExprEvaluatorContext exprEvaluatorContext,
            ImportServiceRuntime importService,
            Attribute[] annotations)
        {
            throw new IllegalStateException("Not possible to evaluate");
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

            if (!(obj is FilterSpecParamEventPropForge)) {
                return false;
            }

            var other = (FilterSpecParamEventPropForge) obj;
            if (!base.Equals(other)) {
                return false;
            }

            if (!ResultEventAsName.Equals(other.ResultEventAsName) ||
                !ResultEventProperty.Equals(other.ResultEventProperty)) {
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

        public override CodegenMethod MakeCodegen(
            CodegenClassScope classScope,
            CodegenMethodScope parent,
            SAIFFInitializeSymbolWEventType symbols)
        {
            var method = parent.MakeChild(typeof(FilterSpecParam), GetType(), classScope);
            var get = ExprIdentNodeEvaluator.Getter.EventBeanGetCodegen(Ref("event"), method, classScope);

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
                    typeof(EventBean), "event",
                    ExprDotMethod(Ref("matchedEvents"), "getMatchingEventByTag", Constant(ResultEventAsName)))
                .DeclareVar(typeof(object), "value", ConstantNull())
                .IfRefNotNull("event")
                .AssignRef("value", get)
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
    }
} // end of namespace