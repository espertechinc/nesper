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
    ///     Getter for Map-entries with well-defined fragment type.
    /// </summary>
    public class MapArrayPropertyGetter : MapEventPropertyGetter,
        MapEventPropertyGetterAndIndexed
    {
        private readonly EventBeanTypedEventFactory eventBeanTypedEventFactory;
        private readonly EventType fragmentType;
        private readonly int index;
        private readonly string propertyName;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="propertyNameAtomic">property name</param>
        /// <param name="index">array index</param>
        /// <param name="eventBeanTypedEventFactory">factory for event beans and event types</param>
        /// <param name="fragmentType">type of the entry returned</param>
        public MapArrayPropertyGetter(
            string propertyNameAtomic,
            int index,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            EventType fragmentType)
        {
            propertyName = propertyNameAtomic;
            this.index = index;
            this.fragmentType = fragmentType;
            this.eventBeanTypedEventFactory = eventBeanTypedEventFactory;
        }

        public bool IsMapExistsProperty(IDictionary<string, object> map)
        {
            return true;
        }

        public object GetMap(IDictionary<string, object> map)
        {
            return GetMapInternal(map, index);
        }

        public object Get(EventBean obj)
        {
            var map = BaseNestableEventUtil.CheckedCastUnderlyingMap(obj);
            return GetMap(map);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true;
        }

        public object GetFragment(EventBean obj)
        {
            var value = Get(obj);
            return BaseNestableEventUtil.GetBNFragmentNonPono(value, fragmentType, eventBeanTypedEventFactory);
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
            return LocalMethod(
                GetMapInternalCodegen(codegenMethodScope, codegenClassScope),
                underlyingExpression,
                Constant(index));
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
            var value = map.Get(propertyName);
            return BaseNestableEventUtil.GetBNArrayValueAtIndexWithNullCheck(value, index);
        }

        private CodegenMethod GetMapInternalCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam(typeof(IDictionary<string, object>), "map")
                .AddParam<int>("index")
                .Block
                .DeclareVar<object>("value", ExprDotMethod(Ref("map"), "Get", Constant(propertyName)))
                .MethodReturn(
                    StaticMethod(
                        typeof(BaseNestableEventUtil),
                        "GetBNArrayValueAtIndexWithNullCheck",
                        Ref("value"),
                        Ref("index")));
        }

        private CodegenMethod GetFragmentCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var factory =
                codegenClassScope.AddOrGetDefaultFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
            var eventType = codegenClassScope.AddDefaultFieldUnshared(
                true,
                typeof(EventType),
                EventTypeUtility.ResolveTypeCodegen(fragmentType, EPStatementInitServicesConstants.REF));
            return codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam(typeof(IDictionary<string, object>), "map")
                .Block
                .DeclareVar<object>("value", UnderlyingGetCodegen(Ref("map"), codegenMethodScope, codegenClassScope))
                .MethodReturn(
                    StaticMethod(
                        typeof(BaseNestableEventUtil),
                        "GetBNFragmentNonPono",
                        Ref("value"),
                        eventType,
                        factory));
        }
    }
} // end of namespace