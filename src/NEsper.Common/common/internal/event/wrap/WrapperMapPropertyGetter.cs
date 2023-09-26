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
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.wrap
{
    public class WrapperMapPropertyGetter : EventPropertyGetterSPI
    {
        private readonly EventBeanTypedEventFactory eventBeanTypedEventFactory;
        private readonly EventPropertyGetterSPI mapGetter;
        private readonly MapEventType underlyingMapType;
        private readonly WrapperEventType wrapperEventType;

        public WrapperMapPropertyGetter(
            WrapperEventType wrapperEventType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            MapEventType underlyingMapType,
            EventPropertyGetterSPI mapGetter)
        {
            this.wrapperEventType = wrapperEventType;
            this.eventBeanTypedEventFactory = eventBeanTypedEventFactory;
            this.underlyingMapType = underlyingMapType;
            this.mapGetter = mapGetter;
        }

        public object Get(EventBean theEvent)
        {
            if (!(theEvent is DecoratingEventBean wrapperEvent)) {
                throw new PropertyAccessException("Mismatched property getter to EventBean type");
            }

            var map = wrapperEvent.DecoratingProperties;
            return mapGetter.Get(eventBeanTypedEventFactory.AdapterForTypedMap(map, underlyingMapType));
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public object GetFragment(EventBean theEvent)
        {
            if (!(theEvent is DecoratingEventBean wrapperEvent)) {
                throw new PropertyAccessException("Mismatched property getter to EventBean type");
            }

            var map = wrapperEvent.DecoratingProperties;
            return mapGetter.GetFragment(eventBeanTypedEventFactory.AdapterForTypedMap(map, underlyingMapType));
        }

        public CodegenExpression EventBeanGetCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(GetCodegen(codegenMethodScope, codegenClassScope), beanExpression);
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
            return LocalMethod(GetFragmentCodegen(codegenMethodScope, codegenClassScope), beanExpression);
        }

        public CodegenExpression UnderlyingGetCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var method = codegenMethodScope
                .MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam<object>("und");
            // TBD - fix this, this type is not right... below
            if (wrapperEventType.UnderlyingType == typeof(Pair<object, object>)) {
                method
                    .Block
                    .DeclareVarWCast(typeof(Pair<object, object>), "pair", "und")
                    .DeclareVar<IDictionary<string, object>>("wrapped", ExprDotName(Ref("pair"), "Second"))
                    .MethodReturn(
                        mapGetter.UnderlyingGetCodegen(Ref("wrapped"), codegenMethodScope, codegenClassScope));
            }
            else {
                method.Block.MethodReturn(ConstantNull());
            }

            return LocalMethod(method, Ref("und"));
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
            throw ImplementationNotProvided();
        }

        private CodegenMethod GetCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam<EventBean>("theEvent")
                .Block
                .DeclareVarWCast(typeof(DecoratingEventBean), "wrapperEvent", "theEvent")
                .DeclareVar<IDictionary<string, object>>(
                    "map",
                    ExprDotName(Ref("wrapperEvent"), "DecoratingProperties"))
                .MethodReturn(mapGetter.UnderlyingGetCodegen(Ref("map"), codegenMethodScope, codegenClassScope));
        }

        private CodegenMethod GetFragmentCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam<EventBean>("theEvent")
                .Block
                .DeclareVarWCast(typeof(DecoratingEventBean), "wrapperEvent", "theEvent")
                .DeclareVar<IDictionary<object, object>>(
                    "map",
                    ExprDotName(Ref("wrapperEvent"), "DecoratingProperties"))
                .MethodReturn(mapGetter.UnderlyingFragmentCodegen(Ref("map"), codegenMethodScope, codegenClassScope));
        }

        private UnsupportedOperationException ImplementationNotProvided()
        {
            return new UnsupportedOperationException(
                "Wrapper event type does not provide an implementation for underlying get");
        }
    }
} // end of namespace