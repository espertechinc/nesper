///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.codegen.core;
using com.espertech.esper.codegen.model.expression;
using com.espertech.esper.compat;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events.wrap
{
    public class WrapperUnderlyingPropertyGetter : EventPropertyGetterSPI
    {
        private readonly EventPropertyGetterSPI _underlyingGetter;

        public WrapperUnderlyingPropertyGetter(EventPropertyGetterSPI underlyingGetter)
        {
            _underlyingGetter = underlyingGetter;
        }

        public object Get(EventBean theEvent)
        {
            if (!(theEvent is DecoratingEventBean))
                throw new PropertyAccessException("Mismatched property getter to EventBean type");
            var wrapperEvent = (DecoratingEventBean) theEvent;
            var wrappedEvent = wrapperEvent.UnderlyingEvent;
            if (wrappedEvent == null) return null;
            return _underlyingGetter.Get(wrappedEvent);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public object GetFragment(EventBean theEvent)
        {
            if (!(theEvent is DecoratingEventBean))
                throw new PropertyAccessException("Mismatched property getter to EventBean type");
            var wrapperEvent = (DecoratingEventBean) theEvent;
            var wrappedEvent = wrapperEvent.UnderlyingEvent;
            if (wrappedEvent == null) return null;
            return _underlyingGetter.GetFragment(wrappedEvent);
        }

        public ICodegenExpression CodegenEventBeanGet(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return LocalMethod(GetCodegen(context), beanExpression);
        }

        public ICodegenExpression CodegenEventBeanExists(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return ConstantTrue();
        }

        public ICodegenExpression CodegenEventBeanFragment(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return LocalMethod(GetFragmentCodegen(context), beanExpression);
        }

        public ICodegenExpression CodegenUnderlyingGet(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            throw ImplementationNotProvided();
        }

        public ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression,
            ICodegenContext context)
        {
            return ConstantTrue();
        }

        public ICodegenExpression CodegenUnderlyingFragment(ICodegenExpression underlyingExpression,
            ICodegenContext context)
        {
            throw ImplementationNotProvided();
        }

        private string GetCodegen(ICodegenContext context)
        {
            return context.AddMethod(typeof(object), typeof(EventBean), "theEvent", GetType())
                .DeclareVarWCast(typeof(DecoratingEventBean), "wrapperEvent", "theEvent")
                .DeclareVar(typeof(EventBean), "wrappedEvent",
                    ExprDotMethod(Ref("wrapperEvent"), "getUnderlyingEvent"))
                .IfRefNullReturnNull("wrappedEvent")
                .MethodReturn(_underlyingGetter.CodegenEventBeanGet(Ref("wrappedEvent"), context));
        }

        private string GetFragmentCodegen(ICodegenContext context)
        {
            return context.AddMethod(typeof(object), typeof(EventBean), "theEvent", GetType())
                .DeclareVarWCast(typeof(DecoratingEventBean), "wrapperEvent", "theEvent")
                .DeclareVar(typeof(EventBean), "wrappedEvent",
                    ExprDotMethod(Ref("wrapperEvent"), "getUnderlyingEvent"))
                .IfRefNullReturnNull("wrappedEvent")
                .MethodReturn(_underlyingGetter.CodegenEventBeanFragment(Ref("wrappedEvent"), context));
        }

        private UnsupportedOperationException ImplementationNotProvided()
        {
            return new UnsupportedOperationException(
                "Wrapper event type does not provide an implementation for underlying get");
        }
    }
} // end of namespace