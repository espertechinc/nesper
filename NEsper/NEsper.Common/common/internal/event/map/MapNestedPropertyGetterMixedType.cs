///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.core;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.map
{
    /// <summary>
    ///     Getter for one or more levels deep nested properties of maps.
    /// </summary>
    public class MapNestedPropertyGetterMixedType : MapEventPropertyGetter
    {
        private readonly EventPropertyGetterSPI[] getterChain;

        public MapNestedPropertyGetterMixedType(IList<EventPropertyGetterSPI> getterChain)
        {
            this.getterChain = getterChain.ToArray();
        }

        public object GetMap(IDictionary<string, object> map)
        {
            var result = ((MapEventPropertyGetter) getterChain[0]).GetMap(map);
            return HandleGetterTrailingChain(result);
        }

        public bool IsMapExistsProperty(IDictionary<string, object> map)
        {
            if (!((MapEventPropertyGetter) getterChain[0]).IsMapExistsProperty(map)) {
                return false;
            }

            var result = ((MapEventPropertyGetter) getterChain[0]).GetMap(map);
            return HandleIsExistsTrailingChain(result);
        }

        public object Get(EventBean eventBean)
        {
            var result = getterChain[0].Get(eventBean);
            return HandleGetterTrailingChain(result);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            if (!getterChain[0].IsExistsProperty(eventBean)) {
                return false;
            }

            var result = getterChain[0].Get(eventBean);
            return HandleIsExistsTrailingChain(result);
        }

        public CodegenExpression EventBeanGetCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingGetCodegen(
                CastUnderlying(typeof(IDictionary<object, object>), beanExpression), codegenMethodScope,
                codegenClassScope);
        }

        public CodegenExpression EventBeanExistsCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingExistsCodegen(
                CastUnderlying(typeof(IDictionary<object, object>), beanExpression), codegenMethodScope,
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
            return LocalMethod(GetMapCodegen(codegenMethodScope, codegenClassScope), underlyingExpression);
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

        public object GetFragment(EventBean eventBean)
        {
            return null;
        }

        private CodegenMethod GetMapCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam(typeof(IDictionary<object, object>), "map").Block
                .DeclareVar(
                    typeof(object), "result",
                    getterChain[0].UnderlyingGetCodegen(Ref("map"), codegenMethodScope, codegenClassScope))
                .MethodReturn(
                    LocalMethod(
                        HandleGetterTrailingChainCodegen(codegenMethodScope, codegenClassScope), Ref("result")));
        }

        private CodegenMethod IsMapExistsPropertyCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(typeof(bool), GetType(), codegenClassScope)
                .AddParam(typeof(IDictionary<object, object>), "map").Block
                .IfConditionReturnConst(
                    getterChain[0].UnderlyingExistsCodegen(Ref("map"), codegenMethodScope, codegenClassScope), false)
                .DeclareVar(
                    typeof(object), "result",
                    getterChain[0].UnderlyingGetCodegen(Ref("map"), codegenMethodScope, codegenClassScope))
                .MethodReturn(
                    LocalMethod(
                        HandleIsExistsTrailingChainCodegen(codegenMethodScope, codegenClassScope), Ref("result")));
        }

        private bool HandleIsExistsTrailingChain(object result)
        {
            for (var i = 1; i < getterChain.Length; i++) {
                if (result == null) {
                    return false;
                }

                EventPropertyGetter getter = getterChain[i];

                if (i == getterChain.Length - 1) {
                    if (getter is BeanEventPropertyGetter) {
                        return ((BeanEventPropertyGetter) getter).IsBeanExistsProperty(result);
                    }

                    if (result is IDictionary<string, object> && getter is MapEventPropertyGetter) {
                        return ((MapEventPropertyGetter) getter).IsMapExistsProperty(
                            (IDictionary<string, object>) result);
                    }

                    if (result is EventBean) {
                        return getter.IsExistsProperty((EventBean) result);
                    }

                    return false;
                }

                if (getter is BeanEventPropertyGetter) {
                    result = ((BeanEventPropertyGetter) getter).GetBeanProp(result);
                }
                else if (result is IDictionary<string, object> && getter is MapEventPropertyGetter) {
                    result = ((MapEventPropertyGetter) getter).GetMap((IDictionary<string, object>) result);
                }
                else if (result is EventBean) {
                    result = getter.Get((EventBean) result);
                }
                else {
                    return false;
                }
            }

            return false;
        }

        private CodegenMethod HandleIsExistsTrailingChainCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var block = codegenMethodScope.MakeChild(typeof(bool), GetType(), codegenClassScope)
                .AddParam(typeof(object), "result").Block;
            for (var i = 1; i < getterChain.Length - 1; i++) {
                block.IfRefNullReturnFalse("result");
                var getter = getterChain[i];
                var blockBean = block.IfInstanceOf("result", typeof(EventBean));
                blockBean.AssignRef(
                    "result",
                    getter.EventBeanGetCodegen(
                        Cast(typeof(EventBean), Ref("result")), codegenMethodScope, codegenClassScope));

                if (getter is BeanEventPropertyGetter) {
                    var type = ((BeanEventPropertyGetter) getter).TargetType;
                    blockBean.IfElse()
                        .AssignRef(
                            "result",
                            getter.UnderlyingGetCodegen(
                                Cast(type, Ref("result")), codegenMethodScope, codegenClassScope))
                        .BlockEnd();
                }
                else if (getter is MapEventPropertyGetter) {
                    blockBean.IfElse()
                        .IfRefNotTypeReturnConst("result", typeof(IDictionary<object, object>), false)
                        .AssignRef(
                            "result",
                            getter.UnderlyingGetCodegen(
                                Cast(typeof(IDictionary<object, object>), Ref("result")), codegenMethodScope,
                                codegenClassScope))
                        .BlockEnd();
                }
                else {
                    blockBean.IfElse().BlockReturn(ConstantFalse());
                }
            }

            var getterAtom = getterChain[getterChain.Length - 1];
            if (getterAtom is BeanEventPropertyGetter) {
                var beanGetter = (BeanEventPropertyGetter) getterAtom;
                return block.MethodReturn(
                    getterAtom.UnderlyingExistsCodegen(
                        Cast(beanGetter.TargetType, Ref("result")), codegenMethodScope, codegenClassScope));
            }

            if (getterAtom is MapEventPropertyGetter) {
                return block.MethodReturn(
                    getterAtom.UnderlyingExistsCodegen(
                        Cast(typeof(IDictionary<object, object>), Ref("result")), codegenMethodScope,
                        codegenClassScope));
            }

            block.IfInstanceOf("result", typeof(EventBean))
                .BlockReturn(
                    getterAtom.EventBeanExistsCodegen(
                        Cast(typeof(EventBean), Ref("result")), codegenMethodScope, codegenClassScope));
            return block.MethodReturn(ConstantFalse());
        }

        private object HandleGetterTrailingChain(object result)
        {
            for (var i = 1; i < getterChain.Length; i++) {
                if (result == null) {
                    return null;
                }

                EventPropertyGetter getter = getterChain[i];
                if (result is EventBean) {
                    result = getter.Get((EventBean) result);
                }
                else if (getter is BeanEventPropertyGetter) {
                    result = ((BeanEventPropertyGetter) getter).GetBeanProp(result);
                }
                else if (result is IDictionary<string, object> && getter is MapEventPropertyGetter) {
                    result = ((MapEventPropertyGetter) getter).GetMap((IDictionary<string, object>) result);
                }
                else {
                    return null;
                }
            }

            return result;
        }

        private CodegenMethod HandleGetterTrailingChainCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var block = codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam(typeof(object), "result").Block;
            for (var i = 1; i < getterChain.Length; i++) {
                block.IfRefNullReturnNull("result");
                var getter = getterChain[i];
                var blockBean = block.IfInstanceOf("result", typeof(EventBean));
                blockBean.AssignRef(
                    "result",
                    getter.EventBeanGetCodegen(
                        Cast(typeof(EventBean), Ref("result")), codegenMethodScope, codegenClassScope));
                if (getter is BeanEventPropertyGetter) {
                    var type = ((BeanEventPropertyGetter) getter).TargetType;
                    blockBean.IfElse()
                        .AssignRef(
                            "result",
                            getter.UnderlyingGetCodegen(
                                Cast(type, Ref("result")), codegenMethodScope, codegenClassScope))
                        .BlockEnd();
                }
                else if (getter is MapEventPropertyGetter) {
                    blockBean.IfElse()
                        .IfRefNotTypeReturnConst("result", typeof(IDictionary<object, object>), null)
                        .AssignRef(
                            "result",
                            getter.UnderlyingGetCodegen(
                                Cast(typeof(IDictionary<object, object>), Ref("result")), codegenMethodScope,
                                codegenClassScope))
                        .BlockEnd();
                }
                else {
                    blockBean.IfElse().BlockReturn(ConstantNull());
                }
            }

            return block.MethodReturn(Ref("result"));
        }
    }
} // end of namespace