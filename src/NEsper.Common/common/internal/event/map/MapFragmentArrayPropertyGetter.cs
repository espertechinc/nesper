///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.map
{
    /// <summary>
    ///     Getter for map array.
    /// </summary>
    public class MapFragmentArrayPropertyGetter : MapEventPropertyGetter
    {
        private readonly EventBeanTypedEventFactory eventBeanTypedEventFactory;
        private readonly EventType fragmentEventType;
        private readonly string propertyName;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="propertyNameAtomic">property type</param>
        /// <param name="fragmentEventType">event type of fragment</param>
        /// <param name="eventBeanTypedEventFactory">for creating event instances</param>
        public MapFragmentArrayPropertyGetter(
            string propertyNameAtomic,
            EventType fragmentEventType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            propertyName = propertyNameAtomic;
            this.fragmentEventType = fragmentEventType;
            this.eventBeanTypedEventFactory = eventBeanTypedEventFactory;
        }

        public object GetMap(IDictionary<string, object> map)
        {
            return map.Get(propertyName);
        }

        public bool IsMapExistsProperty(IDictionary<string, object> map)
        {
            return true;
        }

        public object Get(EventBean obj)
        {
            return GetMap(BaseNestableEventUtil.CheckedCastUnderlyingMap(obj));
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true;
        }

        public object GetFragment(EventBean eventBean)
        {
            var value = Get(eventBean);
            if (value is EventBean[]) {
                return value;
            }

            return BaseNestableEventUtil.GetBNFragmentArray(value, fragmentEventType, eventBeanTypedEventFactory);
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
            return ExprDotMethod(underlyingExpression, "Get", Constant(propertyName));
        }

        public CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ConstantTrue();
        }

        public CodegenExpression UnderlyingFragmentCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(GetFragmentCodegen(codegenMethodScope, codegenClassScope), underlyingExpression);
        }

        private CodegenMethod GetFragmentCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var mSvc = codegenClassScope.AddOrGetDefaultFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
            var mType = codegenClassScope.AddDefaultFieldUnshared(
                true,
                typeof(EventType),
                EventTypeUtility.ResolveTypeCodegen(fragmentEventType, EPStatementInitServicesConstants.REF));
            return codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam(typeof(IDictionary<string, object>), "map")
                .Block
                .DeclareVar<object>(
                    "value",
                    UnderlyingGetCodegen(Ref("map"), codegenMethodScope, codegenClassScope))
                .IfInstanceOf("value", typeof(EventBean[]))
                .BlockReturn(Ref("value"))
                .MethodReturn(
                    StaticMethod(typeof(BaseNestableEventUtil), "GetBNFragmentArray", Ref("value"), mType, mSvc));
        }
    }
} // end of namespace