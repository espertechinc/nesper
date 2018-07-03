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

    public abstract class MapNestedEntryPropertyGetterBase : MapEventPropertyGetter
    {
        protected readonly EventAdapterService EventAdapterService;
        protected readonly EventType FragmentType;
        protected readonly string PropertyMap;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="propertyMap">the property to look at</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        /// <param name="fragmentType">type of the entry returned</param>
        protected MapNestedEntryPropertyGetterBase(string propertyMap, EventType fragmentType,
            EventAdapterService eventAdapterService)
        {
            PropertyMap = propertyMap;
            FragmentType = fragmentType;
            EventAdapterService = eventAdapterService;
        }

        public object GetMap(IDictionary<string, object> map)
        {
            var value = map.Get(PropertyMap);
            if (value == null) return null;
            return HandleNestedValue(value);
        }

        public bool IsMapExistsProperty(IDictionary<string, object> map)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public object Get(EventBean obj)
        {
            return GetMap(BaseNestableEventUtil.CheckedCastUnderlyingMap(obj));
        }

        public virtual bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public object GetFragment(EventBean obj)
        {
            var map = BaseNestableEventUtil.CheckedCastUnderlyingMap(obj);
            var value = map.Get(PropertyMap);
            if (value == null) return null;
            return HandleNestedValueFragment(value);
        }

        public ICodegenExpression CodegenEventBeanGet(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingGet(CastUnderlying(typeof(Map), beanExpression), context);
        }

        public virtual ICodegenExpression CodegenEventBeanExists(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return ConstantTrue();
        }

        public ICodegenExpression CodegenEventBeanFragment(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingFragment(CastUnderlying(typeof(Map), beanExpression), context);
        }

        public ICodegenExpression CodegenUnderlyingGet(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return LocalMethod(GetMapCodegen(context), underlyingExpression);
        }

        public virtual ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression,
            ICodegenContext context)
        {
            return ConstantTrue();
        }

        public ICodegenExpression CodegenUnderlyingFragment(ICodegenExpression underlyingExpression,
            ICodegenContext context)
        {
            return LocalMethod(GetFragmentCodegen(context), underlyingExpression);
        }

        public abstract object HandleNestedValue(object value);
        public abstract object HandleNestedValueFragment(object value);
        public abstract ICodegenExpression HandleNestedValueCodegen(ICodegenExpression name, ICodegenContext context);

        public abstract ICodegenExpression HandleNestedValueFragmentCodegen(ICodegenExpression name,
            ICodegenContext context);

        private string GetMapCodegen(ICodegenContext context)
        {
            return context.AddMethod(typeof(object), typeof(Map), "map", GetType())
                .DeclareVar(typeof(object), "value",
                    ExprDotMethod(Ref("map"), "get",
                        Constant(PropertyMap)))
                .IfRefNullReturnNull("value")
                .MethodReturn(HandleNestedValueCodegen(Ref("value"), context));
        }

        private string GetFragmentCodegen(ICodegenContext context)
        {
            return context.AddMethod(typeof(object), typeof(Map), "map", GetType())
                .DeclareVar(typeof(object), "value",
                    ExprDotMethod(Ref("map"), "get",
                        Constant(PropertyMap)))
                .IfRefNullReturnNull("value")
                .MethodReturn(HandleNestedValueFragmentCodegen(Ref("value"), context));
        }
    }
} // end of namespace