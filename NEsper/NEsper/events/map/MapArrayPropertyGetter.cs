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

    /// <summary>Getter for Map-entries with well-defined fragment type.</summary>
    public class MapArrayPropertyGetter : MapEventPropertyGetter, MapEventPropertyGetterAndIndexed
    {
        private readonly EventAdapterService _eventAdapterService;
        private readonly EventType _fragmentType;
        private readonly int _index;
        private readonly string _propertyName;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="propertyNameAtomic">property name</param>
        /// <param name="index">array index</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        /// <param name="fragmentType">type of the entry returned</param>
        public MapArrayPropertyGetter(string propertyNameAtomic, int index, EventAdapterService eventAdapterService,
            EventType fragmentType)
        {
            _propertyName = propertyNameAtomic;
            this._index = index;
            this._fragmentType = fragmentType;
            this._eventAdapterService = eventAdapterService;
        }

        public bool IsMapExistsProperty(IDictionary<string, object> map)
        {
            return true;
        }

        public object GetMap(IDictionary<string, object> map)
        {
            return GetMapInternal(map, _index);
        }

        public object Get(EventBean obj)
        {
            var map = BaseNestableEventUtil.CheckedCastUnderlyingMap(obj);
            return GetMap(map);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true;
        }

        public object GetFragment(EventBean obj)
        {
            var value = Get(obj);
            return BaseNestableEventUtil.GetBNFragmentNonPono(value, _fragmentType, _eventAdapterService);
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
            return LocalMethod(GetMapInternalCodegen(context), underlyingExpression);
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

        public object Get(EventBean eventBean, int index)
        {
            var map = BaseNestableEventUtil.CheckedCastUnderlyingMap(eventBean);
            return GetMapInternal(map, index);
        }

        private object GetMapInternal(IDictionary<string, object> map, int index)
        {
            var value = map.Get(_propertyName);
            return BaseNestableEventUtil.GetBNArrayValueAtIndexWithNullCheck(value, index);
        }

        private string GetMapInternalCodegen(ICodegenContext context)
        {
            return context.AddMethod(typeof(object), typeof(Map), "map", GetType())
                .DeclareVar(typeof(object), "value",
                    ExprDotMethod(Ref("map"), "get",
                        Constant(_propertyName)))
                .MethodReturn(StaticMethod(
                    typeof(BaseNestableEventUtil),
                    "GetBNArrayValueAtIndexWithNullCheck",
                    Ref("value"),
                    Constant(_index)));
        }

        private string GetFragmentCodegen(ICodegenContext context)
        {
            var mSvc = context.MakeAddMember(typeof(EventAdapterService), _eventAdapterService);
            var mType = context.MakeAddMember(typeof(EventType), _fragmentType);
            return context.AddMethod(typeof(object), typeof(Map), "map", GetType())
                .DeclareVar(typeof(object), "value", CodegenUnderlyingGet(Ref("map"), context))
                .MethodReturn(StaticMethod(
                    typeof(BaseNestableEventUtil),
                    "GetBNFragmentNonPono",
                    Ref("value"),
                    Ref(mType.MemberName),
                    Ref(mSvc.MemberName)));
        }
    }
} // end of namespace