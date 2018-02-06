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

    /// <summary>
    ///     A getter that works on EventBean events residing within a Map as an event property.
    /// </summary>
    public class MapEventBeanEntryPropertyGetter : MapEventPropertyGetter
    {
        private readonly EventPropertyGetterSPI _eventBeanEntryGetter;
        private readonly string _propertyMap;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="propertyMap">the property to look at</param>
        /// <param name="eventBeanEntryGetter">the getter for the map entry</param>
        public MapEventBeanEntryPropertyGetter(string propertyMap, EventPropertyGetterSPI eventBeanEntryGetter)
        {
            _propertyMap = propertyMap;
            _eventBeanEntryGetter = eventBeanEntryGetter;
        }

        public object GetMap(IDictionary<string, object> map)
        {
            // If the map does not contain the key, this is allowed and represented as null
            var value = map.Get(_propertyMap);

            if (value == null) return null;

            // Object within the map
            var theEvent = (EventBean) value;
            return _eventBeanEntryGetter.Get(theEvent);
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

        public object GetFragment(EventBean obj)
        {
            var map = BaseNestableEventUtil.CheckedCastUnderlyingMap(obj);

            // If the map does not contain the key, this is allowed and represented as null
            var value = map.Get(_propertyMap);

            if (value == null) return null;

            // Object within the map
            var theEvent = (EventBean) value;
            return _eventBeanEntryGetter.GetFragment(theEvent);
        }

        public ICodegenExpression CodegenEventBeanGet(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingGet(CastUnderlying(typeof(Map), beanExpression), context);
        }

        public ICodegenExpression CodegenEventBeanExists(ICodegenExpression beanExpression, ICodegenContext context)
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

        private string GetMapCodegen(ICodegenContext context)
        {
            return context.AddMethod(typeof(object), typeof(Map), "map", GetType())
                .DeclareVar(typeof(object), "value",
                    ExprDotMethod(Ref("map"), "get",
                        Constant(_propertyMap)))
                .IfRefNullReturnNull("value")
                .DeclareVar(typeof(EventBean), "theEvent",
                    Cast(typeof(EventBean), Ref("value")))
                .MethodReturn(_eventBeanEntryGetter.CodegenEventBeanGet(Ref("theEvent"), context));
        }

        private string GetFragmentCodegen(ICodegenContext context)
        {
            return context.AddMethod(typeof(object), typeof(Map), "map", GetType())
                .DeclareVar(typeof(object), "value",
                    ExprDotMethod(Ref("map"), "get",
                        Constant(_propertyMap)))
                .IfRefNullReturnNull("value")
                .DeclareVar(typeof(EventBean), "theEvent",
                    Cast(typeof(EventBean), Ref("value")))
                .MethodReturn(_eventBeanEntryGetter.CodegenEventBeanFragment(Ref("theEvent"), context));
        }
    }
} // end of namespace