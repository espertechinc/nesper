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
    ///     A getter that works on EventBean events residing within a Map as an event property.
    /// </summary>
    public class MapNestedEntryPropertyGetterMap : MapNestedEntryPropertyGetterBase
    {
        private readonly MapEventPropertyGetter mapGetter;

        public MapNestedEntryPropertyGetterMap(
            string propertyMap,
            EventType fragmentType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            MapEventPropertyGetter mapGetter)
            : base(propertyMap, fragmentType, eventBeanTypedEventFactory)
        {
            this.mapGetter = mapGetter;
        }

        public override object HandleNestedValue(object value)
        {
            if (value is IDictionary<string, object> mapValue) {
                return mapGetter.GetMap(mapValue);
            }

            if (value is EventBean eventBean) {
                return mapGetter.Get(eventBean);
            }

            return null;

        }

        public override bool HandleNestedValueExists(object value) {
            if (value is IDictionary<string, object> mapValue) {
                return mapGetter.IsMapExistsProperty(mapValue);
            }

            if (value is MappedEventBean mappedEventBean) {
                return mapGetter.IsMapExistsProperty(mappedEventBean.Properties);
            }
            
            return false;
        }
        
        private CodegenMethod HandleNestedValueCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam(typeof(object), "value")
                .Block

                .IfInstanceOf("value", typeof(EventBean))
                .DeclareVarWCast(typeof(EventBean), "bean", "value")
                .BlockReturn(mapGetter.EventBeanGetCodegen(Ref("bean"), codegenMethodScope, codegenClassScope))

                .IfInstanceOf("value", typeof(IDictionary<string, object>))
                .DeclareVarWCast(typeof(IDictionary<string, object>), "map", "value")
                .BlockReturn(mapGetter.UnderlyingGetCodegen(Ref("map"), codegenMethodScope, codegenClassScope))

                .MethodReturn(ConstantNull());
        }
        
        private CodegenMethod HandleNestedValueExistsCodegen(CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope
                .MakeChild(typeof(bool), GetType(), codegenClassScope)
                .AddParam(typeof(object), "value")
                .Block

                .IfInstanceOf("value", typeof(IDictionary<string, object>))
                .DeclareVarWCast(typeof(IDictionary<string, object>), "map", "value")
                .BlockReturn(mapGetter.UnderlyingExistsCodegen(Ref("map"), codegenMethodScope, codegenClassScope))

                .IfInstanceOf("value", typeof(EventBean))
                .DeclareVarWCast(typeof(EventBean), "bean", "value")
                .BlockReturn(mapGetter.EventBeanExistsCodegen(Ref("bean"), codegenMethodScope, codegenClassScope))

                .MethodReturn(ConstantFalse());
        }

        public override object HandleNestedValueFragment(object value)
        {
            if (value is IDictionary<string, object> valueAsMap) {
                var valueEventBean = eventBeanTypedEventFactory.AdapterForTypedMap(
                    valueAsMap, fragmentType);
                return mapGetter.GetFragment(valueEventBean);
            }

            // If the map does not contain the key, this is allowed and represented as null
            if (value is EventBean eventBean) {
                return mapGetter.GetFragment(eventBean);
            }

            return null;
        }

        private CodegenMethod HandleNestedValueFragmentCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam(typeof(object), "value")
                .Block

                .IfInstanceOf("value", typeof(IDictionary<string, object>))
                .DeclareVarWCast(typeof(IDictionary<string, object>), "valueAsMap", "value")
                .BlockReturn(
                    mapGetter.UnderlyingFragmentCodegen(
                        Ref("valueAsMap"),
                        codegenMethodScope,
                        codegenClassScope))

                .IfInstanceOf("value", typeof(EventBean))
                .DeclareVarWCast(typeof(EventBean), "valueAsBean", "value")
                .BlockReturn(
                    mapGetter.EventBeanFragmentCodegen(
                        Ref("valueAsBean"),
                        codegenMethodScope,
                        codegenClassScope))

                .MethodReturn(ConstantNull());
        }

        public override CodegenExpression HandleNestedValueCodegen(
            CodegenExpression name,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(HandleNestedValueCodegen(codegenMethodScope, codegenClassScope), name);
        }
        
        
        public override CodegenExpression HandleNestedValueExistsCodegen(
            CodegenExpression name, 
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(HandleNestedValueExistsCodegen(codegenMethodScope, codegenClassScope), name);
        }

        public override CodegenExpression HandleNestedValueFragmentCodegen(
            CodegenExpression name,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(HandleNestedValueFragmentCodegen(codegenMethodScope, codegenClassScope), name);
        }
    }
} // end of namespace