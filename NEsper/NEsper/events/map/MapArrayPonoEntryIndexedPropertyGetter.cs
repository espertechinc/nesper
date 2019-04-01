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
    /// A getter that works on arrays residing within a Map as an event property.
    /// </summary>
    public class MapArrayPonoEntryIndexedPropertyGetter 
        : BaseNativePropertyGetter
        , MapEventPropertyGetter
        , MapEventPropertyGetterAndIndexed
    {
        private readonly string _propertyMap;
        private readonly int _index;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="propertyMap">the property to use for the map lookup</param>
        /// <param name="index">the index to fetch the array element for</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        /// <param name="returnType">type of the entry returned</param>
        public MapArrayPonoEntryIndexedPropertyGetter(string propertyMap, int index, EventAdapterService eventAdapterService, Type returnType)
            : base(eventAdapterService, returnType, null)
        {
            this._propertyMap = propertyMap;
            this._index = index;
        }
    
        public Object GetMap(IDictionary<string, Object> map) {
            return GetMapInternal(map, _index);
        }
    
        private string GetMapCodegen(ICodegenContext context) {
            return context.AddMethod(typeof(Object), typeof(Map), "map", this.GetType())
                .DeclareVar(typeof(Object), "value", ExprDotMethod(Ref("map"), "get", Constant(_propertyMap)))
                .MethodReturn(StaticMethod(typeof(BaseNestableEventUtil), "GetBNArrayValueAtIndexWithNullCheck", Ref("value"), Constant(_index)));
        }
    
        private Object GetMapInternal(IDictionary<string, Object> map, int index) {
            // If the map does not contain the key, this is allowed and represented as null
            Object value = map.Get(_propertyMap);
            return BaseNestableEventUtil.GetBNArrayValueAtIndexWithNullCheck(value, index);
        }
    
        public bool IsMapExistsProperty(IDictionary<string, Object> map) {
            return map.ContainsKey(_propertyMap);
        }
    
        public Object Get(EventBean eventBean, int index) {
            IDictionary<string, Object> map = BaseNestableEventUtil.CheckedCastUnderlyingMap(eventBean);
            return GetMapInternal(map, index);
        }
    
        public override Object Get(EventBean obj) {
            return GetMap(BaseNestableEventUtil.CheckedCastUnderlyingMap(obj));
        }
    
        public override bool IsExistsProperty(EventBean eventBean) {
            Map map = BaseNestableEventUtil.CheckedCastUnderlyingMap(eventBean);
            return map.ContainsKey(_propertyMap);
        }
    
        public override ICodegenExpression CodegenEventBeanGet(ICodegenExpression beanExpression, ICodegenContext context) {
            return CodegenUnderlyingGet(CastUnderlying(typeof(Map), beanExpression), context);
        }
    
        public override ICodegenExpression CodegenEventBeanExists(ICodegenExpression beanExpression, ICodegenContext context) {
            return CodegenUnderlyingExists(CastUnderlying(typeof(Map), beanExpression), context);
        }
    
        public override ICodegenExpression CodegenUnderlyingGet(ICodegenExpression underlyingExpression, ICodegenContext context) {
            return LocalMethod(GetMapCodegen(context), underlyingExpression);
        }
    
        public override ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression, ICodegenContext context) {
            return ExprDotMethod(underlyingExpression, "containsKey", Constant(_propertyMap));
        }

        public override Type TargetType => typeof(Map);

        public override Type BeanPropType => typeof(Object);
    }
} // end of namespace
