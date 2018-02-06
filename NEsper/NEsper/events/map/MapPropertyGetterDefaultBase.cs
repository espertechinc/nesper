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
using com.espertech.esper.compat.collections;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events.map
{
    using Map = IDictionary<string, object>;

    /// <summary>Getter for map entry.</summary>
    public abstract class MapPropertyGetterDefaultBase : MapEventPropertyGetter
    {
        private readonly string _propertyName;
        protected readonly EventAdapterService EventAdapterService;
        protected readonly EventType FragmentEventType;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="propertyNameAtomic">property name</param>
        /// <param name="fragmentEventType">fragment type</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        protected MapPropertyGetterDefaultBase(string propertyNameAtomic, EventType fragmentEventType,
            EventAdapterService eventAdapterService)
        {
            _propertyName = propertyNameAtomic;
            FragmentEventType = fragmentEventType;
            EventAdapterService = eventAdapterService;
        }

        public object GetMap(IDictionary<string, object> map)
        {
            return map.Get(_propertyName);
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
            return HandleCreateFragment(value);
        }

        public ICodegenExpression CodegenEventBeanGet(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingGet(CastUnderlying(typeof(Map), beanExpression), context);
        }

        public ICodegenExpression CodegenEventBeanExists(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingExists(CastUnderlying(typeof(Map), beanExpression), context);
        }

        public ICodegenExpression CodegenEventBeanFragment(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingFragment(CastUnderlying(typeof(Map), beanExpression), context);
        }

        public ICodegenExpression CodegenUnderlyingGet(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return ExprDotMethod(underlyingExpression, "get", Constant(_propertyName));
        }

        public ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression,
            ICodegenContext context)
        {
            return ConstantTrue();
        }

        public ICodegenExpression CodegenUnderlyingFragment(ICodegenExpression underlyingExpression,
            ICodegenContext context)
        {
            return LocalMethod(GetFragmentCodegen(context), underlyingExpression);
        }

        protected abstract object HandleCreateFragment(object value);

        protected abstract ICodegenExpression HandleCreateFragmentCodegen(ICodegenExpression value,
            ICodegenContext context);

        private string GetFragmentCodegen(ICodegenContext context)
        {
            return context.AddMethod(typeof(object), typeof(Map), "underlying", GetType())
                .DeclareVar(typeof(object), "value", CodegenUnderlyingGet(Ref("underlying"), context))
                .MethodReturn(HandleCreateFragmentCodegen(Ref("value"), context));
        }
    }
} // end of namespace