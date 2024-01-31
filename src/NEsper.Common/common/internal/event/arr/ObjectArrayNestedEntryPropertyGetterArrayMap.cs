///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.map;

namespace com.espertech.esper.common.@internal.@event.arr
{
    /// <summary>
    ///     A getter that works on EventBean events residing within a Map as an event property.
    /// </summary>
    public class ObjectArrayNestedEntryPropertyGetterArrayMap : ObjectArrayNestedEntryPropertyGetterBase
    {
        private readonly MapEventPropertyGetter getter;

        private readonly int index;

        public ObjectArrayNestedEntryPropertyGetterArrayMap(
            int propertyIndex,
            EventType fragmentType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            int index,
            MapEventPropertyGetter getter)
            : base(propertyIndex, fragmentType, eventBeanTypedEventFactory)
        {
            this.index = index;
            this.getter = getter;
        }

        public override object HandleNestedValue(object value)
        {
            return BaseNestableEventUtil.HandleNestedValueArrayWithMap(value, index, getter);
        }

        public override object HandleNestedValueFragment(object value)
        {
            return BaseNestableEventUtil.HandleBNNestedValueArrayWithMapFragment(
                value,
                index,
                getter,
                EventBeanTypedEventFactory,
                FragmentType);
        }

        public override bool HandleNestedValueExists(object value)
        {
            return BaseNestableEventUtil.HandleNestedValueArrayWithMapExists(value, index, getter);
        }

        public override CodegenExpression HandleNestedValueCodegen(
            CodegenExpression refName,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return BaseNestableEventUtil.HandleNestedValueArrayWithMapCode(
                index,
                getter,
                refName,
                codegenMethodScope,
                codegenClassScope,
                GetType());
        }

        public override CodegenExpression HandleNestedValueExistsCodegen(
            CodegenExpression refName,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return BaseNestableEventUtil.HandleNestedValueArrayWithMapExistsCode(
                index,
                getter,
                refName,
                codegenMethodScope,
                codegenClassScope,
                GetType());
        }

        public override CodegenExpression HandleNestedValueFragmentCodegen(
            CodegenExpression refName,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return BaseNestableEventUtil.HandleBNNestedValueArrayWithMapFragmentCode(
                index,
                getter,
                refName,
                codegenMethodScope,
                codegenClassScope,
                EventBeanTypedEventFactory,
                FragmentType,
                GetType());
        }
    }
} // end of namespace