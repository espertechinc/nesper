///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.map
{
    /// <summary>
    ///     A getter for use with Map-based events simply returns the value for the key.
    /// </summary>
    public class MapEntryPropertyGetter : MapEventPropertyGetter
    {
        private readonly EventBeanTypedEventFactory eventBeanTypedEventFactory;
        private readonly BeanEventType eventType;
        private readonly string propertyName;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="propertyName">property to get</param>
        /// <param name="eventBeanTypedEventFactory">factory for event beans and event types</param>
        /// <param name="eventType">type of the entry returned</param>
        public MapEntryPropertyGetter(
            string propertyName,
            BeanEventType eventType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            this.propertyName = propertyName;
            this.eventBeanTypedEventFactory = eventBeanTypedEventFactory;
            this.eventType = eventType;
        }

        public object GetMap(IDictionary<string, object> map)
        {
            // If the map does not contain the key, this is allowed and represented as null
            var value = map.Get(propertyName);
            if (value is EventBean) {
                return ((EventBean) value).Underlying;
            }

            return value;
        }

        public bool IsMapExistsProperty(IDictionary<string, object> map)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public object Get(EventBean obj)
        {
            return GetMap(BaseNestableEventUtil.CheckedCastUnderlyingMap(obj));
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public object GetFragment(EventBean eventBean)
        {
            if (eventType == null) {
                return null;
            }

            var result = Get(eventBean);
            return BaseNestableEventUtil.GetBNFragmentPono(result, eventType, eventBeanTypedEventFactory);
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
            return ConstantTrue();
        }

        public CodegenExpression EventBeanFragmentCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            if (eventType == null) {
                return ConstantNull();
            }

            return UnderlyingFragmentCodegen(
                CastUnderlying(typeof(IDictionary<string, object>), beanExpression),
                codegenMethodScope,
                codegenClassScope);
        }

        public CodegenExpression UnderlyingGetCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return GetMapCodegen(underlyingExpression, codegenMethodScope, codegenClassScope);
        }

        public CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ExprDotMethod(underlyingExpression, "CheckedContainsKey", Constant(propertyName));
        }

        public CodegenExpression UnderlyingFragmentCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            if (eventType == null) {
                return ConstantNull();
            }

            var mSvc = codegenClassScope.AddOrGetDefaultFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
            var mType = codegenClassScope.AddDefaultFieldUnshared(
                true,
                typeof(BeanEventType),
                Cast(
                    typeof(BeanEventType),
                    EventTypeUtility.ResolveTypeCodegen(eventType, EPStatementInitServicesConstants.REF)));
            return StaticMethod(
                typeof(BaseNestableEventUtil),
                "GetBNFragmentPono",
                UnderlyingGetCodegen(underlyingExpression, codegenMethodScope, codegenClassScope),
                mType,
                mSvc);
        }

        private CodegenExpression GetMapCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var method = codegenMethodScope.MakeChild(typeof(object), typeof(MapEntryPropertyGetter), codegenClassScope)
                .AddParam(typeof(IDictionary<string, object>), "map")
                .Block
                .DeclareVar<object>("value", ExprDotMethod(Ref("map"), "Get", Constant(propertyName)))
                .IfInstanceOf("value", typeof(EventBean))
                .BlockReturn(ExprDotUnderlying(Cast(typeof(EventBean), Ref("value"))))
                .MethodReturn(Ref("value"));
            return LocalMethod(method, underlyingExpression);
        }
    }
} // end of namespace