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
using com.espertech.esper.common.@internal.@event.bean.getter;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.map
{
    /// <summary>
    ///     A getter that works on arrays residing within a Map as an event property.
    /// </summary>
    public class MapArrayPONOEntryIndexedPropertyGetter : BaseNativePropertyGetter,
        MapEventPropertyGetter,
        MapEventPropertyGetterAndIndexed
    {
        private readonly string _propertyMap;
        private readonly int _index;

        public MapArrayPONOEntryIndexedPropertyGetter(
            string propertyMap,
            int index,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory,
            Type returnType)
            : base(eventBeanTypedEventFactory, beanEventTypeFactory, returnType)
        {
            _propertyMap = propertyMap;
            _index = index;
        }

        public object GetMap(IDictionary<string, object> map)
        {
            return GetMapInternal(map, _index);
        }

        private object GetMapInternal(
            IDictionary<string, object> map,
            int index)
        {
            // If the map does not contain the key, this is allowed and represented as null
            var value = map.Get(_propertyMap);
            return BaseNestableEventUtil.GetBNArrayValueAtIndexWithNullCheck(value, index);
        }

        private CodegenMethod GetMapInternalCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope
                .MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam(typeof(IDictionary<string, object>), "map")
                .AddParam<int>("index")
                .Block
                .DeclareVar<object>("value", ExprDotMethod(Ref("map"), "Get", Constant(_propertyMap)))
                .MethodReturn(
                    StaticMethod(
                        typeof(BaseNestableEventUtil),
                        "GetBNArrayValueAtIndexWithNullCheck",
                        Ref("value"),
                        Ref("index")));
        }

        private CodegenMethod ExistsMapInternalCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope
                .MakeChild(typeof(bool), GetType(), codegenClassScope)
                .AddParam(typeof(IDictionary<string, object>), "map")
                .AddParam<int>("index")
                .Block
                .DeclareVar<Array>("value", Cast<Array>(ExprDotMethod(Ref("map"), "Get", Constant(_propertyMap))))
                .MethodReturn(StaticMethod(typeof(CollectionUtil), "ArrayExistsAtIndex", Ref("value"), Ref("index")));
        }

        public bool IsMapExistsProperty(IDictionary<string, object> map)
        {
            return map.ContainsKey(_propertyMap);
        }

        public object Get(
            EventBean eventBean,
            int index)
        {
            var map = BaseNestableEventUtil.CheckedCastUnderlyingMap(eventBean);
            return GetMapInternal(map, index);
        }

        public override object Get(EventBean obj)
        {
            return GetMap(BaseNestableEventUtil.CheckedCastUnderlyingMap(obj));
        }

        public override bool IsExistsProperty(EventBean eventBean)
        {
            var map = BaseNestableEventUtil.CheckedCastUnderlyingMap(eventBean);
            var array = (Array)map.Get(_propertyMap);
            return CollectionUtil.ArrayExistsAtIndex(array, _index);
        }

        public override CodegenExpression EventBeanGetCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingGetCodegen(
                CastUnderlying(typeof(IDictionary<string, object>), beanExpression),
                codegenMethodScope,
                codegenClassScope);
        }

        public override CodegenExpression EventBeanExistsCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingExistsCodegen(
                CastUnderlying(typeof(IDictionary<string, object>), beanExpression),
                codegenMethodScope,
                codegenClassScope);
        }

        public override CodegenExpression UnderlyingGetCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(
                GetMapInternalCodegen(codegenMethodScope, codegenClassScope),
                underlyingExpression,
                Constant(_index));
        }

        public override CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(
                ExistsMapInternalCodegen(codegenMethodScope, codegenClassScope),
                underlyingExpression,
                Constant(_index));
        }

        public CodegenExpression EventBeanGetIndexedCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope,
            CodegenExpression beanExpression,
            CodegenExpression key)
        {
            return LocalMethod(
                GetMapInternalCodegen(codegenMethodScope, codegenClassScope),
                CastUnderlying(typeof(IDictionary<string, object>), beanExpression),
                key);
        }

        public override Type TargetType => typeof(IDictionary<string, object>);

        // public override Type BeanPropType => typeof(object);
    }
} // end of namespace