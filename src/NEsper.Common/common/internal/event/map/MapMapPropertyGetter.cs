///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
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
    ///     A getter that interrogates a given property in a map which may itself contain nested maps or indexed entries.
    /// </summary>
    public class MapMapPropertyGetter : MapEventPropertyGetter
    {
        private readonly MapEventPropertyGetter getter;
        private readonly string propertyMap;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="propertyMap">is the property returning the map to interrogate</param>
        /// <param name="getter">is the getter to use to interrogate the property in the map</param>
        public MapMapPropertyGetter(
            string propertyMap,
            MapEventPropertyGetter getter)
        {
            if (getter == null) {
                throw new ArgumentException("Getter is a required parameter");
            }

            this.propertyMap = propertyMap;
            this.getter = getter;
        }

        public object GetMap(IDictionary<string, object> map)
        {
            var valueTopObj = map.Get(propertyMap);
            if (!(valueTopObj is IDictionary<string, object> obj)) {
                return null;
            }

            return getter.GetMap(obj);
        }

        public bool IsMapExistsProperty(IDictionary<string, object> map)
        {
            var valueTopObj = map.Get(propertyMap);
            if (!(valueTopObj is IDictionary<string, object> obj)) {
                return false;
            }

            return getter.IsMapExistsProperty(obj);
        }

        public object Get(EventBean eventBean)
        {
            return GetMap(BaseNestableEventUtil.CheckedCastUnderlyingMap(eventBean));
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return IsMapExistsProperty(BaseNestableEventUtil.CheckedCastUnderlyingMap(eventBean));
        }

        public object GetFragment(EventBean eventBean)
        {
            return null;
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
            return UnderlyingExistsCodegen(
                CastUnderlying(typeof(IDictionary<string, object>), beanExpression),
                codegenMethodScope,
                codegenClassScope);
        }

        public CodegenExpression EventBeanFragmentCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        public CodegenExpression UnderlyingGetCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(GetMapMethodCodegen(codegenMethodScope, codegenClassScope), underlyingExpression);
        }

        public CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(IsMapExistsPropertyCodegen(codegenMethodScope, codegenClassScope), underlyingExpression);
        }

        public CodegenExpression UnderlyingFragmentCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        private CodegenMethod GetMapMethodCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam(typeof(IDictionary<string, object>), "map")
                .Block
                .DeclareVar<object>(
                    "valueTopObj",
                    ExprDotMethod(Ref("map"), "Get", Constant(propertyMap)))
                .DeclareVar<IDictionary<string, object>>(
                    "value",
                    StaticMethod(typeof(CompatExtensions), "AsStringDictionary", Ref("valueTopObj")))
                .IfRefNullReturnNull("value")
                .MethodReturn(getter.UnderlyingGetCodegen(Ref("value"), codegenMethodScope, codegenClassScope));
        }

        private CodegenMethod IsMapExistsPropertyCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(typeof(bool), GetType(), codegenClassScope)
                .AddParam(typeof(IDictionary<string, object>), "map")
                .Block
                .DeclareVar<object>(
                    "valueTopObj",
                    ExprDotMethod(Ref("map"), "Get", Constant(propertyMap)))
                .DeclareVar<IDictionary<string, object>>(
                    "value",
                    StaticMethod(typeof(CompatExtensions), "AsStringDictionary", Ref("valueTopObj")))
                .IfRefNullReturnFalse("value")
                .MethodReturn(getter.UnderlyingExistsCodegen(Ref("value"), codegenMethodScope, codegenClassScope));
        }
    }
} // end of namespace