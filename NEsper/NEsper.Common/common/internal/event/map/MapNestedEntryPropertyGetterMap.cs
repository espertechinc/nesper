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
            string propertyMap, EventType fragmentType, EventBeanTypedEventFactory eventBeanTypedEventFactory,
            MapEventPropertyGetter mapGetter) : base(propertyMap, fragmentType, eventBeanTypedEventFactory)
        {
            this.mapGetter = mapGetter;
        }

        public override object HandleNestedValue(object value)
        {
            if (!(value is IDictionary<string, object>)) {
                if (value is EventBean) {
                    return mapGetter.Get((EventBean) value);
                }

                return null;
            }

            return mapGetter.GetMap((IDictionary<string, object>) value);
        }

        private CodegenMethod HandleNestedValueCodegen(
            CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam(typeof(object), "value").Block
                .IfNotInstanceOf("value", typeof(IDictionary<object, object>))
                .IfInstanceOf("value", typeof(EventBean))
                .DeclareVarWCast(typeof(EventBean), "bean", "value")
                .BlockReturn(mapGetter.EventBeanGetCodegen(Ref("bean"), codegenMethodScope, codegenClassScope))
                .BlockReturn(ConstantNull())
                .DeclareVarWCast(typeof(IDictionary<object, object>), "map", "value")
                .MethodReturn(mapGetter.UnderlyingGetCodegen(Ref("map"), codegenMethodScope, codegenClassScope));
        }

        public override object HandleNestedValueFragment(object value)
        {
            if (!(value is IDictionary<string, object>)) {
                if (value is EventBean) {
                    return mapGetter.GetFragment((EventBean) value);
                }

                return null;
            }

            // If the map does not contain the key, this is allowed and represented as null
            EventBean eventBean = eventBeanTypedEventFactory.AdapterForTypedMap(
                (IDictionary<string, object>) value, fragmentType);
            return mapGetter.GetFragment(eventBean);
        }

        private CodegenMethod HandleNestedValueFragmentCodegen(
            CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam(typeof(object), "value").Block
                .IfNotInstanceOf("value", typeof(IDictionary<object, object>))
                .IfInstanceOf("value", typeof(EventBean))
                .DeclareVarWCast(typeof(EventBean), "bean", "value")
                .BlockReturn(mapGetter.EventBeanFragmentCodegen(Ref("bean"), codegenMethodScope, codegenClassScope))
                .BlockReturn(ConstantNull())
                .MethodReturn(
                    mapGetter.UnderlyingFragmentCodegen(
                        Cast(typeof(IDictionary<object, object>), Ref("value")), codegenMethodScope,
                        codegenClassScope));
        }

        public override CodegenExpression HandleNestedValueCodegen(
            CodegenExpression name, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope)
        {
            return LocalMethod(HandleNestedValueCodegen(codegenMethodScope, codegenClassScope), name);
        }

        public override CodegenExpression HandleNestedValueFragmentCodegen(
            CodegenExpression name, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope)
        {
            return LocalMethod(HandleNestedValueFragmentCodegen(codegenMethodScope, codegenClassScope), name);
        }
    }
} // end of namespace