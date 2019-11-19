///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.magic;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.filterspec
{
    /// <summary>
    ///     This class represents a 'in' filter parameter in an <seealso cref="FilterSpecActivatable" /> filter specification.
    ///     <para />
    ///     The 'in' checks for a list of values.
    /// </summary>
    public sealed class FilterSpecParamInForge : FilterSpecParamForge
    {
        private readonly FilterSpecParamInAdder[] _adders;
        private readonly bool _hasCollMapOrArray;
        private readonly object[] _inListConstantsOnly;
        private readonly IList<FilterSpecParamInValueForge> _listOfValues;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="lookupable">is the event property or function</param>
        /// <param name="filterOperator">is expected to be the IN-list operator</param>
        /// <param name="listofValues">is a list of constants and event property names</param>
        /// <throws>ArgumentException for illegal args</throws>
        public FilterSpecParamInForge(
            ExprFilterSpecLookupableForge lookupable,
            FilterOperator filterOperator,
            IList<FilterSpecParamInValueForge> listofValues)
            : base(lookupable, filterOperator)
        {
            _listOfValues = listofValues;

            foreach (var value in listofValues) {
                var returnType = value.ReturnType;
                if (returnType.IsCollectionMapOrArray()) {
                    _hasCollMapOrArray = true;
                    break;
                }
            }

            if (_hasCollMapOrArray) {
                _adders = new FilterSpecParamInAdder[listofValues.Count];
                for (var i = 0; i < listofValues.Count; i++) {
                    var returnType = listofValues[i].ReturnType;
                    if (returnType == null) {
                        _adders[i] = InValueAdderPlain.INSTANCE;
                    }
                    else if (returnType.IsArray) {
                        _adders[i] = InValueAdderArray.INSTANCE;
                    }
                    else if (returnType.IsGenericDictionary()) {
                        _adders[i] = InValueAdderMap.INSTANCE;
                    }
                    else if (returnType.IsGenericCollection()) {
                        _adders[i] = InValueAdderColl.INSTANCE;
                    }
                    else {
                        _adders[i] = InValueAdderPlain.INSTANCE;
                    }
                }
            }

            var isAllConstants = true;
            foreach (var value in listofValues) {
                if (!value.IsConstant) {
                    isAllConstants = false;
                    break;
                }
            }

            if (isAllConstants) {
                _inListConstantsOnly = GetFilterValues(null, null);
            }

            if (filterOperator != FilterOperator.IN_LIST_OF_VALUES &&
                filterOperator != FilterOperator.NOT_IN_LIST_OF_VALUES) {
                throw new ArgumentException(
                    "Illegal filter operator " +
                    filterOperator +
                    " supplied to " +
                    "in-values filter parameter");
            }
        }

        public object GetFilterValue(
            MatchedEventMap matchedEvents,
            ExprEvaluatorContext exprEvaluatorContext,
            ImportServiceRuntime importService,
            Attribute[] annotations)
        {
            // If the list of values consists of all-constants and no event properties, then use cached version
            if (_inListConstantsOnly != null) {
                return _inListConstantsOnly;
            }

            return GetFilterValues(matchedEvents, exprEvaluatorContext);
        }

        public override string ToString()
        {
            return base.ToString() + "  in=(listOfValues=" + _listOfValues + ')';
        }

        public override bool Equals(object obj)
        {
            if (this == obj) {
                return true;
            }

            if (!(obj is FilterSpecParamInForge)) {
                return false;
            }

            var other = (FilterSpecParamInForge) obj;
            if (!base.Equals(other)) {
                return false;
            }

            if (_listOfValues.Count != other._listOfValues.Count) {
                return false;
            }

            if (!_listOfValues.ToArray().DeepEquals(other._listOfValues.ToArray())) {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            var result = base.GetHashCode();
            result = 31 * result + (_listOfValues != null ? _listOfValues.GetHashCode() : 0);
            return result;
        }

        public override CodegenMethod MakeCodegen(
            CodegenClassScope classScope,
            CodegenMethodScope parent,
            SAIFFInitializeSymbolWEventType symbols)
        {
            var method = parent.MakeChild(typeof(FilterSpecParam), GetType(), classScope);
            method.Block
                .DeclareVar<ExprFilterSpecLookupable>(
                    "lookupable",
                    LocalMethod(lookupable.MakeCodegen(method, symbols, classScope)))
                .DeclareVar<FilterOperator>(
                    "op",
                    EnumValue(typeof(FilterOperator), filterOperator.GetName()));

            var getFilterValue = new CodegenExpressionLambda(method.Block)
                .WithParams(FilterSpecParam.GET_FILTER_VALUE_FP);
            var param = NewInstance<ProxyFilterSpecParam>(
                Ref("lookupable"),
                Ref("op"),
                getFilterValue);

            //var param = NewAnonymousClass(
            //    method.Block,
            //    typeof(FilterSpecParam),
            //    +++Arrays.AsList<CodegenExpression>(Ref("lookupable"), Ref("op")));
            //var getFilterValue = CodegenMethod.MakeParentNode(typeof(object), GetType(), classScope)
            //    .AddParam(FilterSpecParam.GET_FILTER_VALUE_FP);
            //param.AddMethod("GetFilterValue", getFilterValue);

            if (_inListConstantsOnly != null) {
                getFilterValue.Block.BlockReturn(NewInstance<HashableMultiKey>(Constant(_inListConstantsOnly)));
            }
            else if (!_hasCollMapOrArray) {
                getFilterValue.Block.DeclareVar<object[]>(
                    "values",
                    NewArrayByLength(typeof(object), Constant(_listOfValues.Count)));
                for (var i = 0; i < _listOfValues.Count; i++) {
                    var forge = _listOfValues[i];
                    getFilterValue.Block.AssignArrayElement(
                        Ref("values"),
                        Constant(i),
                        forge.MakeCodegen(classScope, method));
                }

                getFilterValue.Block.BlockReturn(NewInstance<HashableMultiKey>(Ref("values")));
            }
            else {
                getFilterValue.Block.DeclareVar<ArrayDeque<object>>(
                    "values",
                    NewInstance<ArrayDeque<object>>(Constant(_listOfValues.Count)));
                for (var i = 0; i < _listOfValues.Count; i++) {
                    var valueName = "value" + i;
                    var adderName = "adder" + i;
                    getFilterValue.Block
                        .DeclareVar<object>(valueName, _listOfValues[i].MakeCodegen(classScope, parent))
                        .IfRefNotNull(valueName)
                        .DeclareVar(_adders[i].GetType(), adderName, EnumValue(_adders[i].GetType(), "INSTANCE"))
                        .ExprDotMethod(Ref(adderName), "Add", Ref("values"), Ref(valueName))
                        .BlockEnd();
                }

                getFilterValue.Block.BlockReturn(
                    NewInstance<HashableMultiKey>(ExprDotMethod(Ref("values"), "ToArray")));
            }

            method.Block.MethodReturn(param);
            return method;
        }

        private object[] GetFilterValues(
            MatchedEventMap matchedEvents,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (!_hasCollMapOrArray) {
                var constantsX = new object[_listOfValues.Count];
                var countX = 0;
                foreach (var valuePlaceholder in _listOfValues) {
                    constantsX[countX++] = valuePlaceholder.GetFilterValue(matchedEvents, exprEvaluatorContext);
                }

                return constantsX;
            }

            var constants = new ArrayDeque<object>(_listOfValues.Count);
            var count = 0;
            foreach (var valuePlaceholder in _listOfValues) {
                var value = valuePlaceholder.GetFilterValue(matchedEvents, exprEvaluatorContext);
                if (value != null) {
                    _adders[count].Add(constants, value);
                }

                count++;
            }

            return constants.ToArray();
        }

        public class InValueAdderArray : FilterSpecParamInAdder
        {
            public static readonly InValueAdderArray INSTANCE = new InValueAdderArray();

            private InValueAdderArray()
            {
            }

            public void Add(
                ICollection<object> constants,
                object value)
            {
                var array = (Array) value;
                var len = array.Length;
                for (var i = 0; i < len; i++) {
                    constants.Add(array.GetValue(i));
                }
            }
        }

        public class InValueAdderMap : FilterSpecParamInAdder
        {
            public static readonly InValueAdderMap INSTANCE = new InValueAdderMap();

            private InValueAdderMap()
            {
            }

            public void Add(
                ICollection<object> constants,
                object value)
            {
                var map = value.AsObjectDictionary(MagicMarker.SingletonInstance);
                constants.AddAll(map.Keys);
            }
        }

        public class InValueAdderColl : FilterSpecParamInAdder
        {
            public static readonly InValueAdderColl INSTANCE = new InValueAdderColl();

            private InValueAdderColl()
            {
            }

            public void Add(
                ICollection<object> constants,
                object value)
            {
                var coll = value.UnwrapEnumerable<object>();
                constants.AddAll(coll);
            }
        }

        public class InValueAdderPlain : FilterSpecParamInAdder
        {
            public static readonly InValueAdderPlain INSTANCE = new InValueAdderPlain();

            private InValueAdderPlain()
            {
            }

            public void Add(
                ICollection<object> constants,
                object value)
            {
                constants.Add(value);
            }
        }
    }
} // end of namespace