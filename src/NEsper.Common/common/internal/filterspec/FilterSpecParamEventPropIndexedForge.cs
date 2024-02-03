///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text;
using System.Text.Json.Serialization;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.filterspec
{
    /// <summary>
    /// This class represents a filter parameter containing a reference to another event's property
    /// in the event pattern result, for use to describe a filter parameter in a filter specification.
    /// </summary>
    public class FilterSpecParamEventPropIndexedForge : FilterSpecParamForge
    {
        private readonly string _resultEventAsName;
        private readonly int _resultEventIndex;
        private readonly string _resultEventProperty;
        private readonly EventType _eventType;
        private readonly bool _isMustCoerce;
        [JsonIgnore]
        [NonSerialized]
        private readonly Coercer _numberCoercer;
        private readonly Type _coercionType;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="lookupable">is the lookupable</param>
        /// <param name="filterOperator">is the type of compare</param>
        /// <param name="resultEventAsName">is the name of the result event from which to get a property value to compare</param>
        /// <param name="resultEventIndex">index</param>
        /// <param name="resultEventProperty">is the name of the property to get from the named result event</param>
        /// <param name="eventType">event type</param>
        /// <param name="isMustCoerce">indicates on whether numeric coercion must be performed</param>
        /// <param name="numberCoercer">interface to use to perform coercion</param>
        /// <param name="coercionType">indicates the numeric coercion type to use</param>
        /// <throws>IllegalArgumentException if an operator was supplied that does not take a single constant value</throws>
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
            _resultEventAsName = resultEventAsName;
            _resultEventIndex = resultEventIndex;
            _resultEventProperty = resultEventProperty;
            _eventType = eventType;
            _isMustCoerce = isMustCoerce;
            _numberCoercer = numberCoercer;
            _coercionType = coercionType;

            if (filterOperator.IsRangeOperator()) {
                throw new ArgumentException(
                    "Illegal filter operator " +
                    filterOperator +
                    " supplied to " +
                    "event property filter parameter");
            }
        }

        public override CodegenExpression MakeCodegen(
            CodegenClassScope classScope,
            CodegenMethodScope parent,
            SAIFFInitializeSymbolWEventType symbols)
        {
            var getterSPI = ((EventTypeSPI)_eventType).GetGetterSPI(_resultEventProperty);
            var method = parent.MakeChild(typeof(FilterSpecParam), typeof(FilterSpecParamConstantForge), classScope);

            method.Block
                .DeclareVar<ExprFilterSpecLookupable>(
                    "lookupable",
                    LocalMethod(lookupable.MakeCodegen(method, symbols, classScope)))
                .DeclareVar<FilterOperator>("filterOperator", EnumValue(typeof(FilterOperator), filterOperator.GetName()));

            // var getFilterValue = CodegenMethod.MakeParentNode(typeof(FilterValueSetParam), GetType(), classScope)
            //     .AddParam(FilterSpecParam.GET_FILTER_VALUE_FP);

            var getFilterValue = new CodegenExpressionLambda(method.Block)
                .WithParams(FilterSpecParam.GET_FILTER_VALUE_FP);
            
            // var param = NewAnonymousClass(
            //     method.Block,
            //     typeof(FilterSpecParam),
            //     Arrays.AsList(Ref("lookupable"), Ref("op")));
            
            var param = NewInstance<ProxyFilterSpecParam>(
                Ref("lookupable"),
                Ref("filterOperator"),
                getFilterValue);
            
            getFilterValue.Block
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
                    getterSPI.EventBeanGetCodegen(
                        ArrayAtIndex(Ref("events"), Constant(_resultEventIndex)),
                        method,
                        classScope))
                .BlockEnd();

            if (_isMustCoerce) {
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

        public EventType EventType => _eventType;

        /// <summary>
        /// Returns the index.
        /// </summary>
        /// <value>index</value>
        public int ResultEventIndex => _resultEventIndex;

        public override string ToString()
        {
            return base.ToString() +
                   " resultEventAsName=" +
                   _resultEventAsName +
                   " resultEventProperty=" +
                   _resultEventProperty;
        }


        protected bool Equals(FilterSpecParamEventPropIndexedForge other)
        {
            return _resultEventIndex == other._resultEventIndex && _resultEventProperty == other._resultEventProperty;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((FilterSpecParamEventPropIndexedForge)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_resultEventIndex, _resultEventProperty);
        }


        public override void ValueExprToString(
            StringBuilder @out,
            int i)
        {
            @out.Append("indexed event property '").Append(_resultEventProperty).Append("'");
        }
    }
} // end of namespace