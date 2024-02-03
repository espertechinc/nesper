///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.wrap
{
    public class WrapperGetterMapped : EventPropertyGetterMappedSPI
    {
        private readonly EventPropertyGetterMappedSPI undMapped;

        public WrapperGetterMapped(EventPropertyGetterMappedSPI undMapped)
        {
            this.undMapped = undMapped;
        }

        public object Get(
            EventBean @event,
            string key)
        {
            if (!(@event is DecoratingEventBean wrapper)) {
                throw new PropertyAccessException("Mismatched property getter to EventBean type");
            }

            var wrapped = wrapper.UnderlyingEvent;
            if (wrapped == null) {
                return null;
            }

            return undMapped.Get(wrapped, key);
        }

        public CodegenExpression EventBeanGetMappedCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope,
            CodegenExpression beanExpression,
            CodegenExpression key)
        {
            var method = codegenMethodScope.MakeChild(typeof(object), typeof(WrapperGetterMapped), codegenClassScope)
                .AddParam<EventBean>("@event")
                .AddParam<string>("key")
                .Block
                .DeclareVar<DecoratingEventBean>("wrapper", Cast(typeof(DecoratingEventBean), Ref("@event")))
                .DeclareVar<EventBean>("wrapped", ExprDotName(Ref("wrapper"), "UnderlyingEvent"))
                .IfRefNullReturnNull("wrapped")
                .MethodReturn(
                    undMapped.EventBeanGetMappedCodegen(
                        codegenMethodScope,
                        codegenClassScope,
                        Ref("wrapped"),
                        Ref("key")));
            return LocalMethodBuild(method).Pass(beanExpression).Pass(key).Call();
        }
    }
} // end of namespace