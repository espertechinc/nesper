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
using com.espertech.esper.events.bean;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events.map
{
    using Map = IDictionary<string, object>;

    /// <summary>
    ///     A getter for use with Map-based events simply returns the value for the key.
    /// </summary>
    public class MapEntryPropertyGetter : MapEventPropertyGetter
    {
        private readonly EventAdapterService _eventAdapterService;
        private readonly BeanEventType _eventType;
        private readonly string _propertyName;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="propertyName">property to get</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        /// <param name="eventType">type of the entry returned</param>
        public MapEntryPropertyGetter(string propertyName, BeanEventType eventType,
            EventAdapterService eventAdapterService)
        {
            _propertyName = propertyName;
            _eventAdapterService = eventAdapterService;
            _eventType = eventType;
        }

        public object GetMap(IDictionary<string, object> map)
        {
            // If the map does not contain the key, this is allowed and represented as null
            return map.Get(_propertyName);
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
            if (_eventType == null) return null;
            var result = Get(eventBean);
            return BaseNestableEventUtil.GetBNFragmentPono(result, _eventType, _eventAdapterService);
        }

        public ICodegenExpression CodegenEventBeanGet(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return BeanUndCastDotMethodConst(typeof(Map), beanExpression, "get", _propertyName);
        }

        public ICodegenExpression CodegenEventBeanExists(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return ConstantTrue();
        }

        public ICodegenExpression CodegenEventBeanFragment(ICodegenExpression beanExpression, ICodegenContext context)
        {
            if (_eventType == null) return ConstantNull();
            return CodegenUnderlyingFragment(CastUnderlying(typeof(Map), beanExpression), context);
        }

        public ICodegenExpression CodegenUnderlyingGet(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return ExprDotMethod(underlyingExpression, "get", Constant(_propertyName));
        }

        public ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression,
            ICodegenContext context)
        {
            return ExprDotMethod(underlyingExpression, "containsKey",
                Constant(_propertyName));
        }

        public ICodegenExpression CodegenUnderlyingFragment(ICodegenExpression underlyingExpression,
            ICodegenContext context)
        {
            if (_eventType == null) return ConstantNull();
            var mSvc = context.MakeAddMember(typeof(EventAdapterService), _eventAdapterService);
            var mType = context.MakeAddMember(typeof(BeanEventType), _eventType);
            return StaticMethod(typeof(BaseNestableEventUtil), "GetBNFragmentPono",
                CodegenUnderlyingGet(underlyingExpression, context), 
                Ref(mType.MemberName), 
                Ref(mSvc.MemberName));
        }
    }
} // end of namespace