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
        private readonly int index;
        private readonly string propertyMap;

        public MapArrayPONOEntryIndexedPropertyGetter(
            string propertyMap,
            int index,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory,
            Type returnType)
            : base(eventBeanTypedEventFactory, beanEventTypeFactory, returnType, null)
        {
            this.propertyMap = propertyMap;
            this.index = index;
        }

        public override Type TargetType => typeof(IDictionary<string, object>);

        public override Type BeanPropType => typeof(object);

        public object GetMap(IDictionary<string, object> map)
        {
            return GetMapInternal(map, index);
        }

        public bool IsMapExistsProperty(IDictionary<string, object> map)
        {
            return map.ContainsKey(propertyMap);
        }

        public override object Get(EventBean obj)
        {
            return GetMap(BaseNestableEventUtil.CheckedCastUnderlyingMap(obj));
        }

        public override bool IsExistsProperty(EventBean eventBean)
        {
            var map = BaseNestableEventUtil.CheckedCastUnderlyingMap(eventBean);
            return map.ContainsKey(propertyMap);
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
                Constant(index));
        }

        public override CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ExprDotMethod(underlyingExpression, "ContainsKey", Constant(propertyMap));
        }

        public object Get(
            EventBean eventBean,
            int index)
        {
            var map = BaseNestableEventUtil.CheckedCastUnderlyingMap(eventBean);
            return GetMapInternal(map, index);
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

        private object GetMapInternal(
            IDictionary<string, object> map,
            int index)
        {
            // If the map does not contain the key, this is allowed and represented as null
            var value = map.Get(propertyMap);
            return BaseNestableEventUtil.GetBNArrayValueAtIndexWithNullCheck(value, index);
        }

        private CodegenMethod GetMapInternalCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam(typeof(IDictionary<string, object>), "map")
                .AddParam(typeof(int), "index")
                .Block
                .DeclareVar<object>("value", ExprDotMethod(Ref("map"), "Get", Constant(propertyMap)))
                .MethodReturn(
                    StaticMethod(
                        typeof(BaseNestableEventUtil),
                        "GetBNArrayValueAtIndexWithNullCheck",
                        Ref("value"),
                        Ref("index")));
        }
    }
} // end of namespace