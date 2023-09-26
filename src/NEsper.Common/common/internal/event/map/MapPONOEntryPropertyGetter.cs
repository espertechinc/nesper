///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.bean.getter;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.map
{
    /// <summary>
    /// A getter that works on PONO events residing within a Map as an event property.
    /// </summary>
    public class MapPONOEntryPropertyGetter : BaseNativePropertyGetter,
        MapEventPropertyGetter
    {
        private readonly string propertyMap;
        private readonly BeanEventPropertyGetter mapEntryGetter;

        public MapPONOEntryPropertyGetter(
            string propertyMap,
            BeanEventPropertyGetter mapEntryGetter,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            Type returnType,
            BeanEventTypeFactory beanEventTypeFactory) : base(
            eventBeanTypedEventFactory,
            beanEventTypeFactory,
            returnType)
        {
            this.propertyMap = propertyMap;
            this.mapEntryGetter = mapEntryGetter;
        }

        public object GetMap(IDictionary<string, object> map)
        {
            // If the map does not contain the key, this is allowed and represented as null
            var value = map.Get(propertyMap);
            if (value == null) {
                return null;
            }

            // Object within the map
            if (value is EventBean bean) {
                return mapEntryGetter.Get(bean);
            }

            return mapEntryGetter.GetBeanProp(value);
        }

        private CodegenMethod GetMapCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam(typeof(IDictionary<string, object>), "map")
                .Block
                .DeclareVar<object>("value", ExprDotMethod(Ref("map"), "Get", Constant(propertyMap)))
                .IfRefNullReturnNull("value")
                .IfInstanceOf("value", typeof(EventBean))
                .BlockReturn(
                    mapEntryGetter.EventBeanGetCodegen(
                        CastRef(typeof(EventBean), "value"),
                        codegenMethodScope,
                        codegenClassScope))
                .MethodReturn(
                    mapEntryGetter.UnderlyingGetCodegen(
                        CastRef(mapEntryGetter.TargetType, "value"),
                        codegenMethodScope,
                        codegenClassScope));
        }

        public bool IsMapExistsProperty(IDictionary<string, object> map)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public override object Get(EventBean obj)
        {
            return GetMap(BaseNestableEventUtil.CheckedCastUnderlyingMap(obj));
        }

        public override bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
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
            return ConstantTrue();
        }

        public override CodegenExpression UnderlyingGetCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(GetMapCodegen(codegenMethodScope, codegenClassScope), underlyingExpression);
        }

        public override CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ConstantTrue();
        }

        public override Type TargetType => typeof(IDictionary<string, object>);

        public override Type BeanPropType => typeof(object);
    }
} // end of namespace