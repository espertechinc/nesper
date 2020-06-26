///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.@event.map
{
    /// <summary>
    ///     A getter that works on EventBean events residing within a Map as an event property.
    /// </summary>
    public class MapNestedEntryPropertyGetterArrayMap : MapNestedEntryPropertyGetterBase
    {
        private readonly MapEventPropertyGetter getter;
        private readonly int index;

        public MapNestedEntryPropertyGetterArrayMap(
            string propertyMap,
            EventType fragmentType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            int index,
            MapEventPropertyGetter getter)
            : base(propertyMap, fragmentType, eventBeanTypedEventFactory)
        {
            this.index = index;
            this.getter = getter;
        }

        public override object HandleNestedValue(object value)
        {
            return BaseNestableEventUtil.HandleNestedValueArrayWithMap(value, index, getter);
        }

        public override bool HandleNestedValueExists(object value)
        {
            return BaseNestableEventUtil.HandleNestedValueArrayWithMapExists(value, index, getter);
        }

        public override object HandleNestedValueFragment(object value)
        {
            return BaseNestableEventUtil.HandleBNNestedValueArrayWithMapFragment(
                value,
                index,
                getter,
                eventBeanTypedEventFactory,
                fragmentType);
        }

        public override CodegenExpression HandleNestedValueCodegen(
            CodegenExpression name,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return BaseNestableEventUtil.HandleNestedValueArrayWithMapCode(
                index,
                getter,
                name,
                codegenMethodScope,
                codegenClassScope,
                GetType());
        }


        public override CodegenExpression HandleNestedValueExistsCodegen(
            CodegenExpression name,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return BaseNestableEventUtil.HandleNestedValueArrayWithMapExistsCode(
                index,
                getter,
                name,
                codegenMethodScope,
                codegenClassScope,
                GetType());
        }

        public override CodegenExpression HandleNestedValueFragmentCodegen(
            CodegenExpression name,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return BaseNestableEventUtil.HandleBNNestedValueArrayWithMapFragmentCode(
                index,
                getter,
                name,
                codegenMethodScope,
                codegenClassScope,
                eventBeanTypedEventFactory,
                fragmentType,
                GetType());
        }
    }
} // end of namespace