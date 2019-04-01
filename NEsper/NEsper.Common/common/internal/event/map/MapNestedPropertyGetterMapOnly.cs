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
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.map
{
    /// <summary>
    ///     Getter for one or more levels deep nested properties of maps.
    /// </summary>
    public class MapNestedPropertyGetterMapOnly : MapEventPropertyGetter
    {
        private readonly MapEventPropertyGetter[] mapGetterChain;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="getterChain">is the chain of getters to retrieve each nested property</param>
        /// <param name="eventAdaperService">is a factory for PONO bean event types</param>
        public MapNestedPropertyGetterMapOnly(
            IList<EventPropertyGetterSPI> getterChain,
            EventBeanTypedEventFactory eventAdaperService)
        {
            mapGetterChain = new MapEventPropertyGetter[getterChain.Count];
            for (var i = 0; i < getterChain.Count; i++) {
                mapGetterChain[i] = (MapEventPropertyGetter) getterChain[i];
            }
        }

        public object GetMap(IDictionary<string, object> map)
        {
            var result = mapGetterChain[0].GetMap(map);
            return HandleGetterTrailingChain(result);
        }

        public bool IsMapExistsProperty(IDictionary<string, object> map)
        {
            if (!mapGetterChain[0].IsMapExistsProperty(map)) {
                return false;
            }

            var result = mapGetterChain[0].GetMap(map);
            return HandleIsExistsTrailingChain(result);
        }

        public object Get(EventBean eventBean)
        {
            var result = mapGetterChain[0].Get(eventBean);
            return HandleGetterTrailingChain(result);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            if (!mapGetterChain[0].IsExistsProperty(eventBean)) {
                return false;
            }

            var result = mapGetterChain[0].Get(eventBean);
            return HandleIsExistsTrailingChain(result);
        }

        public object GetFragment(EventBean eventBean)
        {
            return null;
        }

        public CodegenExpression EventBeanGetCodegen(
            CodegenExpression beanExpression, CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingGetCodegen(
                CastUnderlying(typeof(IDictionary<object, object>), beanExpression), codegenMethodScope,
                codegenClassScope);
        }

        public CodegenExpression EventBeanExistsCodegen(
            CodegenExpression beanExpression, CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingExistsCodegen(
                CastUnderlying(typeof(IDictionary<object, object>), beanExpression), codegenMethodScope,
                codegenClassScope);
        }

        public CodegenExpression EventBeanFragmentCodegen(
            CodegenExpression beanExpression, CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        public CodegenExpression UnderlyingGetCodegen(
            CodegenExpression underlyingExpression, CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var resultExpression = mapGetterChain[0].UnderlyingGetCodegen(
                underlyingExpression, codegenMethodScope, codegenClassScope);
            return LocalMethod(
                HandleGetterTrailingChainCodegen(codegenMethodScope, codegenClassScope), resultExpression);
        }

        public CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression, CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(IsMapExistsPropertyCodegen(codegenMethodScope, codegenClassScope), underlyingExpression);
        }

        public CodegenExpression UnderlyingFragmentCodegen(
            CodegenExpression underlyingExpression, CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        private CodegenMethod IsMapExistsPropertyCodegen(
            CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(typeof(bool), GetType(), codegenClassScope)
                .AddParam(typeof(IDictionary<object, object>), "map").Block
                .IfConditionReturnConst(
                    Not(mapGetterChain[0].UnderlyingExistsCodegen(Ref("map"), codegenMethodScope, codegenClassScope)),
                    false)
                .DeclareVar(
                    typeof(object), "result",
                    mapGetterChain[0].UnderlyingGetCodegen(Ref("map"), codegenMethodScope, codegenClassScope))
                .MethodReturn(
                    LocalMethod(
                        HandleIsExistsTrailingChainCodegen(codegenMethodScope, codegenClassScope), Ref("result")));
        }

        private bool HandleIsExistsTrailingChain(object result)
        {
            for (var i = 1; i < mapGetterChain.Length; i++) {
                if (result == null) {
                    return false;
                }

                var getter = mapGetterChain[i];

                if (i == mapGetterChain.Length - 1) {
                    if (!(result is IDictionary<string, object>)) {
                        if (result is EventBean) {
                            return getter.IsExistsProperty((EventBean) result);
                        }

                        return false;
                    }

                    return getter.IsMapExistsProperty((IDictionary<string, object>) result);
                }

                if (!(result is IDictionary<string, object>)) {
                    if (result is EventBean) {
                        result = getter.Get((EventBean) result);
                    }
                    else {
                        return false;
                    }
                }
                else {
                    result = getter.GetMap((IDictionary<string, object>) result);
                }
            }

            return true;
        }

        private CodegenMethod HandleIsExistsTrailingChainCodegen(
            CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope)
        {
            var block = codegenMethodScope.MakeChild(typeof(bool), GetType(), codegenClassScope)
                .AddParam(typeof(object), "result").Block;
            for (var i = 1; i < mapGetterChain.Length; i++) {
                block.IfRefNullReturnFalse("result");
                var getter = mapGetterChain[i];

                if (i == mapGetterChain.Length - 1) {
                    block.IfNotInstanceOf("result", typeof(IDictionary<object, object>))
                        .IfInstanceOf("result", typeof(EventBean))
                        .AssignRef(
                            "result",
                            getter.EventBeanExistsCodegen(
                                CastRef(typeof(EventBean), "result"), codegenMethodScope, codegenClassScope))
                        .IfElse()
                        .BlockReturn(ConstantFalse())
                        .IfElse()
                        .BlockReturn(
                            getter.UnderlyingExistsCodegen(
                                CastRef(typeof(IDictionary<object, object>), "result"), codegenMethodScope,
                                codegenClassScope));
                }

                block.IfNotInstanceOf("result", typeof(IDictionary<object, object>))
                    .IfInstanceOf("result", typeof(EventBean))
                    .AssignRef(
                        "result",
                        getter.EventBeanGetCodegen(
                            CastRef(typeof(EventBean), "result"), codegenMethodScope, codegenClassScope))
                    .IfElse()
                    .BlockReturn(ConstantFalse())
                    .IfElse()
                    .AssignRef(
                        "result",
                        getter.UnderlyingGetCodegen(
                            CastRef(typeof(IDictionary<object, object>), "result"), codegenMethodScope,
                            codegenClassScope))
                    .BlockEnd();
            }

            return block.MethodReturn(ConstantTrue());
        }

        private object HandleGetterTrailingChain(object result)
        {
            for (var i = 1; i < mapGetterChain.Length; i++) {
                if (result == null) {
                    return null;
                }

                var getter = mapGetterChain[i];
                if (!(result is IDictionary<string, object>)) {
                    if (result is EventBean) {
                        result = getter.Get((EventBean) result);
                    }
                    else {
                        return null;
                    }
                }
                else {
                    result = getter.GetMap((IDictionary<string, object>) result);
                }
            }

            return result;
        }

        private CodegenMethod HandleGetterTrailingChainCodegen(
            CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope)
        {
            var block = codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam(typeof(object), "result").Block;
            for (var i = 1; i < mapGetterChain.Length; i++) {
                block.IfRefNullReturnNull("result");
                var getter = mapGetterChain[i];
                block.IfNotInstanceOf("result", typeof(IDictionary<object, object>))
                    .IfInstanceOf("result", typeof(EventBean))
                    .AssignRef(
                        "result",
                        getter.EventBeanGetCodegen(
                            CastRef(typeof(EventBean), "result"), codegenMethodScope, codegenClassScope))
                    .IfElse()
                    .BlockReturn(ConstantNull())
                    .IfElse()
                    .AssignRef(
                        "result",
                        getter.UnderlyingGetCodegen(
                            CastRef(typeof(IDictionary<object, object>), "result"), codegenMethodScope,
                            codegenClassScope))
                    .BlockEnd();
            }

            return block.MethodReturn(Ref("result"));
        }
    }
} // end of namespace