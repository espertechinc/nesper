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
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.map
{
    /// <summary>
    ///     Getter for an array of event bean using a nested getter.
    /// </summary>
    public class MapEventBeanArrayIndexedElementPropertyGetter : MapEventPropertyGetter
    {
        private readonly int index;
        private readonly EventPropertyGetterSPI nestedGetter;
        private readonly string propertyName;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="propertyName">property name</param>
        /// <param name="index">array index</param>
        /// <param name="nestedGetter">nested getter</param>
        public MapEventBeanArrayIndexedElementPropertyGetter(
            string propertyName,
            int index,
            EventPropertyGetterSPI nestedGetter)
        {
            this.propertyName = propertyName;
            this.index = index;
            this.nestedGetter = nestedGetter;
        }

        public object GetMap(IDictionary<string, object> map)
        {
            // If the map does not contain the key, this is allowed and represented as null
            var wrapper = (EventBean[]) map.Get(propertyName);
            return BaseNestableEventUtil.GetArrayPropertyValue(wrapper, index, nestedGetter);
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
            var wrapper = (EventBean[]) map.Get(propertyName);
            return BaseNestableEventUtil.GetArrayPropertyFragment(wrapper, index, nestedGetter);
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
            return LocalMethod(GetMapCodegen(codegenMethodScope, codegenClassScope), underlyingExpression);
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

        private CodegenMethod GetMapCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam(typeof(IDictionary<string, object>), "map")
                .Block
                .DeclareVar<EventBean[]>(
                    "wrapper",
                    Cast(typeof(EventBean[]), ExprDotMethod(Ref("map"), "Get", Constant(propertyName))))
                .MethodReturn(
                    LocalMethod(
                        BaseNestableEventUtil.GetArrayPropertyValueCodegen(
                            codegenMethodScope,
                            codegenClassScope,
                            index,
                            nestedGetter),
                        Ref("wrapper")));
        }

        private CodegenMethod GetFragmentCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam(typeof(IDictionary<string, object>), "map")
                .Block
                .DeclareVar<EventBean[]>(
                    "wrapper",
                    Cast(typeof(EventBean[]), ExprDotMethod(Ref("map"), "Get", Constant(propertyName))))
                .MethodReturn(
                    LocalMethod(
                        BaseNestableEventUtil.GetArrayPropertyFragmentCodegen(
                            codegenMethodScope,
                            codegenClassScope,
                            index,
                            nestedGetter),
                        Ref("wrapper")));
        }
    }
} // end of namespace