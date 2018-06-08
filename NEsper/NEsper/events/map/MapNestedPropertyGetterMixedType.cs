///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.codegen.core;
using com.espertech.esper.codegen.model.expression;
using com.espertech.esper.events.bean;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events.map
{
    using Map = IDictionary<string, object>;

    /// <summary>
    ///     Getter for one or more levels deep nested properties of maps.
    /// </summary>
    public class MapNestedPropertyGetterMixedType : MapEventPropertyGetter
    {
        private readonly EventPropertyGetterSPI[] _getterChain;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="getterChain">is the chain of getters to retrieve each nested property</param>
        /// <param name="eventAdaperService">is a factory for POJO bean event types</param>
        public MapNestedPropertyGetterMixedType(IList<EventPropertyGetterSPI> getterChain,
            EventAdapterService eventAdaperService)
        {
            _getterChain = getterChain.ToArray();
        }

        public object GetMap(IDictionary<string, object> map)
        {
            var result = ((MapEventPropertyGetter) _getterChain[0]).GetMap(map);
            return HandleGetterTrailingChain(result);
        }

        public bool IsMapExistsProperty(IDictionary<string, object> map)
        {
            if (!((MapEventPropertyGetter) _getterChain[0]).IsMapExistsProperty(map)) return false;
            var result = ((MapEventPropertyGetter) _getterChain[0]).GetMap(map);
            return HandleIsExistsTrailingChain(result);
        }

        public object Get(EventBean eventBean)
        {
            var result = _getterChain[0].Get(eventBean);
            return HandleGetterTrailingChain(result);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            if (!_getterChain[0].IsExistsProperty(eventBean)) return false;
            var result = _getterChain[0].Get(eventBean);
            return HandleIsExistsTrailingChain(result);
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
            return ConstantNull();
        }

        public ICodegenExpression CodegenUnderlyingGet(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return LocalMethod(GetMapCodegen(context), underlyingExpression);
        }

        public ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression,
            ICodegenContext context)
        {
            return LocalMethod(IsMapExistsPropertyCodegen(context), underlyingExpression);
        }

        public ICodegenExpression CodegenUnderlyingFragment(ICodegenExpression underlyingExpression,
            ICodegenContext context)
        {
            return ConstantNull();
        }

        public object GetFragment(EventBean eventBean)
        {
            return null;
        }

        private string GetMapCodegen(ICodegenContext context)
        {
            return context.AddMethod(typeof(object), typeof(Map), "map", GetType())
                .DeclareVar(typeof(object), "result",
                    _getterChain[0].CodegenUnderlyingGet(Ref("map"), context))
                .MethodReturn(LocalMethod(HandleGetterTrailingChainCodegen(context),
                    Ref("result")));
        }

        private string IsMapExistsPropertyCodegen(ICodegenContext context)
        {
            return context.AddMethod(typeof(bool), typeof(Map), "map", GetType())
                .IfConditionReturnConst(_getterChain[0].CodegenUnderlyingExists(Ref("map"), context),
                    false)
                .DeclareVar(typeof(object), "result",
                    _getterChain[0].CodegenUnderlyingGet(Ref("map"), context))
                .MethodReturn(LocalMethod(HandleIsExistsTrailingChainCodegen(context),
                    Ref("result")));
        }

        private bool HandleIsExistsTrailingChain(object result)
        {
            for (var i = 1; i < _getterChain.Length; i++)
            {
                if (result == null) return false;

                EventPropertyGetter getter = _getterChain[i];

                if (i == _getterChain.Length - 1)
                    if (getter is BeanEventPropertyGetter)
                        return ((BeanEventPropertyGetter) getter).IsBeanExistsProperty(result);
                    else if (result is Map && getter is MapEventPropertyGetter)
                        return ((MapEventPropertyGetter) getter).IsMapExistsProperty((Map) result);
                    else if (result is EventBean)
                        return getter.IsExistsProperty((EventBean) result);
                    else
                        return false;

                if (getter is BeanEventPropertyGetter)
                    result = ((BeanEventPropertyGetter) getter).GetBeanProp(result);
                else if (result is Map && getter is MapEventPropertyGetter)
                    result = ((MapEventPropertyGetter) getter).GetMap((Map) result);
                else if (result is EventBean)
                    result = getter.Get((EventBean) result);
                else
                    return false;
            }

            return false;
        }

        private string HandleIsExistsTrailingChainCodegen(ICodegenContext context)
        {
            var block = context.AddMethod(typeof(bool), typeof(object), "result", GetType());
            for (var i = 1; i < _getterChain.Length - 1; i++)
            {
                block.IfRefNullReturnFalse("result");
                var getterX = _getterChain[i];
                var blockBean = block.IfInstanceOf("result", typeof(EventBean));
                blockBean.AssignRef("result",
                    getterX.CodegenEventBeanGet(Cast(typeof(EventBean), Ref("result")), context));

                if (getterX is BeanEventPropertyGetter)
                {
                    var type = ((BeanEventPropertyGetter) getterX).TargetType;
                    blockBean.BlockElse()
                        .AssignRef("result",
                            getterX.CodegenUnderlyingGet(Cast(type, Ref("result")), context))
                        .BlockEnd();
                }
                else if (getterX is MapEventPropertyGetter)
                {
                    blockBean.BlockElse()
                        .IfRefNotTypeReturnConst("result", typeof(Map), false)
                        .AssignRef("result",
                            getterX.CodegenUnderlyingGet(Cast(typeof(Map), Ref("result")), context))
                        .BlockEnd();
                }
                else
                {
                    blockBean.BlockElse().BlockReturn(ConstantFalse());
                }
            }

            var getter = _getterChain[_getterChain.Length - 1];
            if (getter is BeanEventPropertyGetter)
            {
                var beanGetter = (BeanEventPropertyGetter) getter;
                return block.MethodReturn(
                    getter.CodegenUnderlyingExists(Cast(beanGetter.TargetType, Ref("result")),
                        context));
            }

            if (getter is MapEventPropertyGetter)
            {
                return block.MethodReturn(
                    getter.CodegenUnderlyingExists(Cast(typeof(Map), Ref("result")), context));
            }

            block.IfInstanceOf("result", typeof(EventBean))
                .BlockReturn(getter.CodegenEventBeanExists(Cast(typeof(EventBean), Ref("result")),
                    context));
            return block.MethodReturn(ConstantFalse());
        }

        private object HandleGetterTrailingChain(object result)
        {
            for (var i = 1; i < _getterChain.Length; i++)
            {
                if (result == null) return null;
                EventPropertyGetter getter = _getterChain[i];
                if (result is EventBean)
                    result = getter.Get((EventBean) result);
                else if (getter is BeanEventPropertyGetter)
                    result = ((BeanEventPropertyGetter) getter).GetBeanProp(result);
                else if (result is Map && getter is MapEventPropertyGetter)
                    result = ((MapEventPropertyGetter) getter).GetMap((Map) result);
                else
                    return null;
            }

            return result;
        }

        private string HandleGetterTrailingChainCodegen(ICodegenContext context)
        {
            var block = context.AddMethod(typeof(object), typeof(object), "result", GetType());
            for (var i = 1; i < _getterChain.Length; i++)
            {
                block.IfRefNullReturnNull("result");
                var getter = _getterChain[i];
                var blockBean = block.IfInstanceOf("result", typeof(EventBean));
                blockBean.AssignRef("result",
                    getter.CodegenEventBeanGet(Cast(typeof(EventBean), Ref("result")), context));
                if (getter is BeanEventPropertyGetter)
                {
                    var type = ((BeanEventPropertyGetter) getter).TargetType;
                    blockBean.BlockElse()
                        .AssignRef("result",
                            getter.CodegenUnderlyingGet(Cast(type, Ref("result")), context))
                        .BlockEnd();
                }
                else if (getter is MapEventPropertyGetter)
                {
                    blockBean.BlockElse()
                        .IfRefNotTypeReturnConst("result", typeof(Map), null)
                        .AssignRef("result",
                            getter.CodegenUnderlyingGet(Cast(typeof(Map), Ref("result")), context))
                        .BlockEnd();
                }
                else
                {
                    blockBean.BlockElse().BlockReturn(ConstantNull());
                }
            }

            return block.MethodReturn(Ref("result"));
        }
    }
} // end of namespace