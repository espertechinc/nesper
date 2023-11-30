///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.magic;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.map
{
    /// <summary>
    ///     A getter that interrogates a given property in a map which may itself contain nested maps or indexed entries.
    /// </summary>
    public class MapMapPropertyGetter : MapEventPropertyGetter
    {
        private readonly MapEventPropertyGetter _getter;
        private readonly string _propertyMap;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="propertyMap">is the property returning the map to interrogate</param>
        /// <param name="getter">is the getter to use to interrogate the property in the map</param>
        public MapMapPropertyGetter(
            string propertyMap,
            MapEventPropertyGetter getter)
        {
            if (getter == null) {
                throw new ArgumentException("Getter is a required parameter");
            }

            this._propertyMap = propertyMap;
            this._getter = getter;
        }

        public static IDictionary<string, object> GetStringDictionary(object valueToObj, string propertyKey)
        {
            if (valueToObj == null) {
                return null;
            }
            
            if (valueToObj is IDictionary<string, object> valueToObjDictionary) {
                return valueToObjDictionary;
            }

            var valueType = valueToObj.GetType();
            if (valueType.IsGenericStringDictionary()) {
                return MagicMarker.SingletonInstance
                    .GetStringDictionaryFactory(valueType)
                    .Invoke(valueToObj);
            }

            return null;
        }

        public object GetMap(IDictionary<string, object> map)
        {
            var valueToObj = map.Get(_propertyMap);
            var valueDictionary = GetStringDictionary(valueToObj, _propertyMap);
            if (valueDictionary == null) {
                return null;
            }

            return _getter.GetMap(valueDictionary);
        }

        public bool IsMapExistsProperty(
            IDictionary<string, object> map)
        {
            var valueToObj = map.Get(_propertyMap);
            var valueDictionary = GetStringDictionary(valueToObj, _propertyMap);
            if (valueDictionary == null) {
                return false;
            }

            return _getter.IsMapExistsProperty(valueDictionary);
        }

        public object Get(EventBean eventBean)
        {
            return GetMap(BaseNestableEventUtil.CheckedCastUnderlyingMap(eventBean));
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return IsMapExistsProperty(BaseNestableEventUtil.CheckedCastUnderlyingMap(eventBean));
        }

        public object GetFragment(EventBean eventBean)
        {
            return null;
        }

        public CodegenExpression EventBeanGetCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingGetCodegen(
                CastUnderlying(typeof(IDictionary<string, object>), beanExpression),
                codegenMethodScope,
                codegenClassScope);
        }

        public CodegenExpression EventBeanExistsCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingExistsCodegen(
                CastUnderlying(typeof(IDictionary<string, object>), beanExpression),
                codegenMethodScope,
                codegenClassScope);
        }

        public CodegenExpression EventBeanFragmentCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        public CodegenExpression UnderlyingGetCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(GetMapMethodCodegen(codegenMethodScope, codegenClassScope), underlyingExpression);
        }

        public CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(IsMapExistsPropertyCodegen(codegenMethodScope, codegenClassScope), underlyingExpression);
        }

        public CodegenExpression UnderlyingFragmentCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        private CodegenMethod GetMapMethodCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam(typeof(IDictionary<string, object>), "map")
                .Block
                .DeclareVar<object>(
                    "valueTopObj", ExprDotMethod(Ref("map"), "Get", Constant(_propertyMap)))
                .DeclareVar<IDictionary<string, object>>(
                    "value", StaticMethod(typeof(MapMapPropertyGetter), "GetStringDictionary", Ref("valueTopObj"), Constant(_propertyMap)))
                .IfRefNullReturnNull("value")
                .MethodReturn(_getter.UnderlyingGetCodegen(Ref("value"), codegenMethodScope, codegenClassScope));
        }

        private CodegenMethod IsMapExistsPropertyCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(typeof(bool), GetType(), codegenClassScope)
                .AddParam(typeof(IDictionary<string, object>), "map")
                .Block
                .DeclareVar<object>(
                    "valueTopObj", ExprDotMethod(Ref("map"), "Get", Constant(_propertyMap)))
                .DeclareVar<IDictionary<string, object>>(
                    "value", StaticMethod(typeof(MapMapPropertyGetter), "GetStringDictionary", Ref("valueTopObj"), Constant(_propertyMap)))
                .IfRefNullReturnFalse("value")
                .MethodReturn(_getter.UnderlyingExistsCodegen(Ref("value"), codegenMethodScope, codegenClassScope));
        }
    }
} // end of namespace