///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    /// This class represents a filter parameter containing a reference to another event's property
    /// in the event pattern result, for use to describe a filter parameter in a <seealso cref = "FilterSpecActivatable"/> filter specification.
    /// </summary>
    public class FilterSpecParamEventPropForge : FilterSpecParamForge
    {
        private readonly string _resultEventAsName;
        private readonly string _resultEventProperty;
        private readonly ExprIdentNodeEvaluator _exprIdentNodeEvaluator;
        private readonly bool _isMustCoerce;
        [NonSerialized] private readonly Coercer _numberCoercer;
        private readonly Type _coercionType;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name = "lookupable">is the property or function to get a lookup value</param>
        /// <param name = "filterOperator">is the type of compare</param>
        /// <param name = "resultEventAsName">is the name of the result event from which to get a property value to compare</param>
        /// <param name = "resultEventProperty">is the name of the property to get from the named result event</param>
        /// <param name = "exprIdentNodeEvaluator">evaluator</param>
        /// <param name = "isMustCoerce">indicates on whether numeric coercion must be performed</param>
        /// <param name = "numberCoercer">interface to use to perform coercion</param>
        /// <param name = "coercionType">indicates the numeric coercion type to use</param>
        /// <throws>IllegalArgumentException if an operator was supplied that does not take a single constant value</throws>
        public FilterSpecParamEventPropForge(
            ExprFilterSpecLookupableForge lookupable,
            FilterOperator filterOperator,
            string resultEventAsName,
            string resultEventProperty,
            ExprIdentNodeEvaluator exprIdentNodeEvaluator,
            bool isMustCoerce,
            Coercer numberCoercer,
            Type coercionType) : base(lookupable, filterOperator)
        {
            this._resultEventAsName = resultEventAsName;
            this._resultEventProperty = resultEventProperty;
            this._exprIdentNodeEvaluator = exprIdentNodeEvaluator;
            this._isMustCoerce = isMustCoerce;
            this._numberCoercer = numberCoercer;
            this._coercionType = coercionType;
            if (filterOperator.IsRangeOperator()) {
                throw new ArgumentException(
                    "Illegal filter operator " + filterOperator + " supplied to " + "event property filter parameter");
            }
        }

        /// <summary>
        /// Returns true if numeric coercion is required, or false if not
        /// </summary>
        /// <value>true to coerce at runtime</value>
        public bool IsMustCoerce => _isMustCoerce;

        /// <summary>
        /// Returns the numeric coercion type.
        /// </summary>
        /// <value>type to coerce to</value>
        public Type CoercionType => _coercionType;

        /// <summary>
        /// Returns tag for result event.
        /// </summary>
        /// <value>tag</value>
        public string ResultEventAsName => _resultEventAsName;

        /// <summary>
        /// Returns the property of the result event.
        /// </summary>
        /// <value>property name</value>
        public string ResultEventProperty => _resultEventProperty;

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
                   " resultEventAsName=" +
                   _resultEventAsName +
                   " resultEventProperty=" +
                   _resultEventProperty;
        }

        public override bool Equals(object obj)
        {
            if (this == obj) {
                return true;
            }

            if (!(obj is FilterSpecParamEventPropForge other)) {
                return false;
            }

            if (!base.Equals(other)) {
                return false;
            }

            if (!_resultEventAsName.Equals(other._resultEventAsName) ||
                !_resultEventProperty.Equals(other._resultEventProperty)) {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            var result = base.GetHashCode();
            result = 31 * result + _resultEventProperty.GetHashCode();
            return result;
        }

        public override CodegenExpression MakeCodegen(
            CodegenClassScope classScope,
            CodegenMethodScope parent,
            SAIFFInitializeSymbolWEventType symbols)
        {
            var method = parent.MakeChild(typeof(FilterSpecParam), GetType(), classScope);
            var get = _exprIdentNodeEvaluator.Getter.EventBeanGetCodegen(Ref("@event"), method, classScope);
            
            method.Block
                .DeclareVar<ExprFilterSpecLookupable>("lookupable",
                    LocalMethod(lookupable.MakeCodegen(method, symbols, classScope)))
                .DeclareVar<FilterOperator>("filterOperator", EnumValue(typeof(FilterOperator), filterOperator.GetName()));
            
            // CodegenExpressionNewAnonymousClass param = NewAnonymousClass(
            //     method.Block,
            //     typeof(FilterSpecParam),
            //     Arrays.AsList(Ref("lookupable"), Ref("op")));
            
            var getFilterValue = new CodegenExpressionLambda(method.Block)
                .WithParams(FilterSpecParam.GET_FILTER_VALUE_FP);
            
            // var getFilterValue = CodegenMethod
            //     .MakeParentNode(typeof(FilterValueSetParam), GetType(), classScope)
            //     .AddParam(FilterSpecParam.GET_FILTER_VALUE_FP);

            var param = NewInstance<ProxyFilterSpecParam>(
                Ref("lookupable"),
                Ref("filterOperator"),
                getFilterValue);
            
            getFilterValue.Block
                .DeclareVar<EventBean>("@event",
                    ExprDotMethod(Ref("matchedEvents"), "GetMatchingEventByTag", Constant(_resultEventAsName)))
                .DeclareVar<object>("value", ConstantNull())
                .IfRefNotNull("@event")
                .AssignRef("value", get)
                .BlockEnd();
            
            if (_isMustCoerce) {
                getFilterValue.Block.AssignRef(
                    "value",
                    _numberCoercer.CoerceCodegenMayNullBoxed(
                        Ref("value"),
                        typeof(object),
                        method,
                        classScope));
            }

            getFilterValue.Block.BlockReturn(FilterValueSetParamImpl.CodegenNew(Ref("value")));

            method.Block.MethodReturn(param);
            return LocalMethod(method);
        }

        public override void ValueExprToString(
            StringBuilder @out,
            int i)
        {
            @out.Append("event property '")
                .Append(_resultEventProperty)
                .Append("'");
        }

        public ExprIdentNodeEvaluator ExprIdentNodeEvaluator => _exprIdentNodeEvaluator;
    }
} // end of namespace