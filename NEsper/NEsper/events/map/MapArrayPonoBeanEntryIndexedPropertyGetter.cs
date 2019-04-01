///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
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
    ///     A getter that works on POJO events residing within a Map as an event property.
    /// </summary>
    public class MapArrayPonoBeanEntryIndexedPropertyGetter : BaseNativePropertyGetter
        , MapEventPropertyGetter
    {
        private readonly int _index;
        private readonly BeanEventPropertyGetter _nestedGetter;
        private readonly string _propertyMap;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="propertyMap">the property to look at</param>
        /// <param name="nestedGetter">the getter for the map entry</param>
        /// <param name="eventAdapterService">for producing wrappers to objects</param>
        /// <param name="index">the index to fetch the array element for</param>
        /// <param name="returnType">type of the entry returned</param>
        public MapArrayPonoBeanEntryIndexedPropertyGetter(string propertyMap, int index,
            BeanEventPropertyGetter nestedGetter, EventAdapterService eventAdapterService, Type returnType)
            : base(eventAdapterService, returnType, null)
        {
            _propertyMap = propertyMap;
            _index = index;
            _nestedGetter = nestedGetter;
        }

        public override Type TargetType => typeof(Map);

        public override Type BeanPropType => typeof(object);

        public object GetMap(IDictionary<string, object> map)
        {
            // If the map does not contain the key, this is allowed and represented as null
            var value = map.Get(_propertyMap);
            return BaseNestableEventUtil.GetBeanArrayValue(_nestedGetter, value, _index);
        }

        public bool IsMapExistsProperty(IDictionary<string, object> map)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public override object Get(EventBean obj)
        {
            var map = BaseNestableEventUtil.CheckedCastUnderlyingMap(obj);
            return GetMap(map);
        }

        public override bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public override ICodegenExpression CodegenEventBeanGet(ICodegenExpression beanExpression,
            ICodegenContext context)
        {
            return CodegenUnderlyingGet(CastUnderlying(typeof(Map), beanExpression), context);
        }

        public override ICodegenExpression CodegenEventBeanExists(ICodegenExpression beanExpression,
            ICodegenContext context)
        {
            return ConstantTrue();
        }

        public override ICodegenExpression CodegenUnderlyingGet(ICodegenExpression underlyingExpression,
            ICodegenContext context)
        {
            return LocalMethod(GetMapCodegen(context), underlyingExpression);
        }

        public override ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression,
            ICodegenContext context)
        {
            return ConstantTrue();
        }

        private string GetMapCodegen(ICodegenContext context)
        {
            return context.AddMethod(typeof(object), typeof(Map), "map", GetType())
                .DeclareVar(typeof(object), "value",
                    ExprDotMethod(Ref("map"), "get",
                        Constant(_propertyMap)))
                .MethodReturn(LocalMethod(
                    BaseNestableEventUtil.GetBeanArrayValueCodegen(context, _nestedGetter, _index),
                    Ref("value")));
        }
    }
} // end of namespace