///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.codegen.core;
using com.espertech.esper.codegen.model.expression;
using com.espertech.esper.compat;
using com.espertech.esper.events.map;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events.wrap
{
    using Map = IDictionary<string, object>;

    public class WrapperMapPropertyGetter : EventPropertyGetterSPI
    {
        private readonly EventAdapterService _eventAdapterService;
        private readonly EventPropertyGetterSPI _mapGetter;
        private readonly MapEventType _underlyingMapType;
        private readonly WrapperEventType _wrapperEventType;

        public WrapperMapPropertyGetter(WrapperEventType wrapperEventType, EventAdapterService eventAdapterService,
            MapEventType underlyingMapType, EventPropertyGetterSPI mapGetter)
        {
            _wrapperEventType = wrapperEventType;
            _eventAdapterService = eventAdapterService;
            _underlyingMapType = underlyingMapType;
            _mapGetter = mapGetter;
        }

        public object Get(EventBean theEvent)
        {
            if (!(theEvent is DecoratingEventBean))
                throw new PropertyAccessException("Mismatched property getter to EventBean type");
            var wrapperEvent = (DecoratingEventBean) theEvent;
            var map = wrapperEvent.DecoratingProperties;
            return _mapGetter.Get(_eventAdapterService.AdapterForTypedMap(map, _underlyingMapType));
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
            var map = wrapperEvent.DecoratingProperties;
            return _mapGetter.GetFragment(_eventAdapterService.AdapterForTypedMap(map, _underlyingMapType));
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
                .DeclareVar(typeof(Map), "map",
                    ExprDotMethod(Ref("wrapperEvent"), "getDecoratingProperties"))
                .MethodReturn(_mapGetter.CodegenUnderlyingGet(Ref("map"), context));
        }

        private string GetFragmentCodegen(ICodegenContext context)
        {
            return context.AddMethod(typeof(object), typeof(EventBean), "theEvent", GetType())
                .DeclareVarWCast(typeof(DecoratingEventBean), "wrapperEvent", "theEvent")
                .DeclareVar(typeof(Map), "map",
                    ExprDotMethod(Ref("wrapperEvent"), "getDecoratingProperties"))
                .MethodReturn(_mapGetter.CodegenUnderlyingFragment(Ref("map"), context));
        }

        private UnsupportedOperationException ImplementationNotProvided()
        {
            return new UnsupportedOperationException(
                "Wrapper event type does not provide an implementation for underlying get");
        }
    }
} // end of namespace