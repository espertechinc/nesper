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

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events.map
{
    using Map = IDictionary<string, object>;

    /// <summary>
    ///     Getter for one or more levels deep nested properties of maps.
    /// </summary>
    public class MapNestedPropertyGetterMapOnly : MapEventPropertyGetter
    {
        private readonly MapEventPropertyGetter[] _mapGetterChain;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="getterChain">is the chain of getters to retrieve each nested property</param>
        /// <param name="eventAdaperService">is a factory for POJO bean event types</param>
        public MapNestedPropertyGetterMapOnly(List<EventPropertyGetterSPI> getterChain,
            EventAdapterService eventAdaperService)
        {
            _mapGetterChain = new MapEventPropertyGetter[getterChain.Count];
            for (var i = 0; i < getterChain.Count; i++)
                _mapGetterChain[i] = (MapEventPropertyGetter) getterChain[i];
        }

        public object GetMap(IDictionary<string, object> map)
        {
            var result = _mapGetterChain[0].GetMap(map);
            return HandleGetterTrailingChain(result);
        }

        public bool IsMapExistsProperty(IDictionary<string, object> map)
        {
            if (!_mapGetterChain[0].IsMapExistsProperty(map)) return false;
            var result = _mapGetterChain[0].GetMap(map);
            return HandleIsExistsTrailingChain(result);
        }

        public object Get(EventBean eventBean)
        {
            var result = _mapGetterChain[0].Get(eventBean);
            return HandleGetterTrailingChain(result);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            if (!_mapGetterChain[0].IsExistsProperty(eventBean)) return false;
            var result = _mapGetterChain[0].Get(eventBean);
            return HandleIsExistsTrailingChain(result);
        }

        public object GetFragment(EventBean eventBean)
        {
            return null;
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
            var resultExpression = _mapGetterChain[0].CodegenUnderlyingGet(underlyingExpression, context);
            return LocalMethod(HandleGetterTrailingChainCodegen(context), resultExpression);
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

        private string IsMapExistsPropertyCodegen(ICodegenContext context)
        {
            return context.AddMethod(typeof(bool), typeof(Map), "map", GetType())
                .IfConditionReturnConst(
                    Not(_mapGetterChain[0].CodegenUnderlyingExists(Ref("map"), context)),
                    false)
                .DeclareVar(typeof(object), "result",
                    _mapGetterChain[0].CodegenUnderlyingGet(Ref("map"), context))
                .MethodReturn(LocalMethod(HandleIsExistsTrailingChainCodegen(context),
                    Ref("result")));
        }

        private bool HandleIsExistsTrailingChain(object result)
        {
            for (var i = 1; i < _mapGetterChain.Length; i++)
            {
                if (result == null) return false;

                var getter = _mapGetterChain[i];

                if (i == _mapGetterChain.Length - 1)
                    if (!(result is Map))
                    {
                        if (result is EventBean) return getter.IsExistsProperty((EventBean) result);
                        return false;
                    }
                    else
                    {
                        return getter.IsMapExistsProperty((IDictionary<string, object>) result);
                    }

                if (!(result is Map))
                    if (result is EventBean)
                        result = getter.Get((EventBean) result);
                    else
                        return false;
                else
                    result = getter.GetMap((IDictionary<string, object>) result);
            }

            return true;
        }

        private string HandleIsExistsTrailingChainCodegen(ICodegenContext context)
        {
            var block = context.AddMethod(typeof(bool), typeof(object), "result", GetType());
            for (var i = 1; i < _mapGetterChain.Length; i++)
            {
                block.IfRefNullReturnFalse("result");
                var getter = _mapGetterChain[i];

                if (i == _mapGetterChain.Length - 1)
                    block.IfNotInstanceOf("result", typeof(Map))
                        .IfInstanceOf("result", typeof(EventBean))
                        .AssignRef("result",
                            getter.CodegenEventBeanExists(CastRef(typeof(EventBean), "result"),
                                context))
                        .BlockElse()
                        .BlockReturn(ConstantFalse())
                        .BlockElse()
                        .BlockReturn(getter.CodegenUnderlyingExists(CastRef(typeof(Map), "result"),
                            context));

                block.IfNotInstanceOf("result", typeof(Map))
                    .IfInstanceOf("result", typeof(EventBean))
                    .AssignRef("result",
                        getter.CodegenEventBeanGet(CastRef(typeof(EventBean), "result"), context))
                    .BlockElse()
                    .BlockReturn(ConstantFalse())
                    .BlockElse()
                    .AssignRef("result",
                        getter.CodegenUnderlyingGet(CastRef(typeof(Map), "result"), context))
                    .BlockEnd();
            }

            return block.MethodReturn(ConstantTrue());
        }

        private object HandleGetterTrailingChain(object result)
        {
            for (var i = 1; i < _mapGetterChain.Length; i++)
            {
                if (result == null) return null;

                var getter = _mapGetterChain[i];
                if (!(result is Map))
                    if (result is EventBean)
                        result = getter.Get((EventBean) result);
                    else
                        return null;
                else
                    result = getter.GetMap((IDictionary<string, object>) result);
            }

            return result;
        }

        private string HandleGetterTrailingChainCodegen(ICodegenContext context)
        {
            var block = context.AddMethod(typeof(object), typeof(object), "result", GetType());
            for (var i = 1; i < _mapGetterChain.Length; i++)
            {
                block.IfRefNullReturnNull("result");
                var getter = _mapGetterChain[i];
                block.IfNotInstanceOf("result", typeof(Map))
                    .IfInstanceOf("result", typeof(EventBean))
                    .AssignRef("result",
                        getter.CodegenEventBeanGet(CastRef(typeof(EventBean), "result"), context))
                    .BlockElse()
                    .BlockReturn(ConstantNull())
                    .BlockElse()
                    .AssignRef("result",
                        getter.CodegenUnderlyingGet(CastRef(typeof(Map), "result"), context))
                    .BlockEnd();
            }

            return block.MethodReturn(Ref("result"));
        }
    }
} // end of namespace