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
    ///     A getter that works on PONO events residing within a Map as an event property.
    /// </summary>
    public class MapPonoEntryPropertyGetter : BaseNativePropertyGetter
        , MapEventPropertyGetter
    {
        private readonly BeanEventPropertyGetter _mapEntryGetter;
        private readonly string _propertyMap;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="propertyMap">the property to look at</param>
        /// <param name="mapEntryGetter">the getter for the map entry</param>
        /// <param name="eventAdapterService">for producing wrappers to objects</param>
        /// <param name="returnType">type of the entry returned</param>
        /// <param name="nestedComponentType">nested type</param>
        public MapPonoEntryPropertyGetter(string propertyMap, BeanEventPropertyGetter mapEntryGetter,
            EventAdapterService eventAdapterService, Type returnType, Type nestedComponentType)
            : base(eventAdapterService, returnType, nestedComponentType)
        {
            _propertyMap = propertyMap;
            _mapEntryGetter = mapEntryGetter;
        }

        public override Type TargetType => typeof(Map);

        public override Type BeanPropType => typeof(object);

        public object GetMap(IDictionary<string, object> map)
        {
            // If the map does not contain the key, this is allowed and represented as null
            var value = map.Get(_propertyMap);
            if (value == null) return null;
            // Object within the map
            if (value is EventBean) return _mapEntryGetter.Get((EventBean) value);
            return _mapEntryGetter.GetBeanProp(value);
        }

        public bool IsMapExistsProperty(IDictionary<string, object> map)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public override object Get(EventBean obj)
        {
            return GetMap(BaseNestableEventUtil.CheckedCastUnderlyingMap(obj));
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
                .IfRefNullReturnNull("value")
                .IfInstanceOf("value", typeof(EventBean))
                .BlockReturn(_mapEntryGetter.CodegenEventBeanGet(CastRef(typeof(EventBean), "value"),
                    context))
                .MethodReturn(
                    _mapEntryGetter.CodegenUnderlyingGet(CastRef(_mapEntryGetter.TargetType, "value"),
                        context));
        }
    }
} // end of namespace