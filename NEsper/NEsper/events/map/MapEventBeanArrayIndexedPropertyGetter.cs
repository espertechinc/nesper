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
    ///     Getter for array events.
    /// </summary>
    /// <seealso cref="com.espertech.esper.events.map.MapEventPropertyGetter" />
    public class MapEventBeanArrayIndexedPropertyGetter : MapEventPropertyGetter
    {
        private readonly int _index;
        private readonly string _propertyName;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="propertyName">property name</param>
        /// <param name="index">array index</param>
        public MapEventBeanArrayIndexedPropertyGetter(string propertyName, int index)
        {
            _propertyName = propertyName;
            _index = index;
        }

        public object GetMap(IDictionary<string, object> map)
        {
            // If the map does not contain the key, this is allowed and represented as null
            var wrapper = (EventBean[]) map.Get(_propertyName);
            return BaseNestableEventUtil.GetBNArrayPropertyUnderlying(wrapper, _index);
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
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public object GetFragment(EventBean obj)
        {
            var map = BaseNestableEventUtil.CheckedCastUnderlyingMap(obj);
            var wrapper = (EventBean[]) map.Get(_propertyName);
            return BaseNestableEventUtil.GetBNArrayPropertyBean(wrapper, _index);
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
                .DeclareVar(typeof(EventBean[]), "wrapper",
                    Cast(typeof(EventBean[]),
                        ExprDotMethod(Ref("map"), "get",
                            Constant(_propertyName))))
                .MethodReturn(StaticMethod(typeof(BaseNestableEventUtil),
                    "GetBNArrayPropertyUnderlying", Ref("wrapper"), Constant(_index)));
        }

        private string GetFragmentCodegen(ICodegenContext context)
        {
            return context.AddMethod(typeof(object), typeof(Map), "map", GetType())
                .DeclareVar(typeof(EventBean[]), "wrapper",
                    Cast(typeof(EventBean[]),
                        ExprDotMethod(Ref("map"), "get",
                            Constant(_propertyName))))
                .MethodReturn(StaticMethod(typeof(BaseNestableEventUtil), "GetBNArrayPropertyBean",
                    Ref("wrapper"), Constant(_index)));
        }
    }
} // end of namespace