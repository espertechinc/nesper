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

namespace com.espertech.esper.common.@internal.@event.arr
{
    public class ObjectArrayNestedEntryPropertyGetterArrayObjectArray : ObjectArrayNestedEntryPropertyGetterBase
    {
        private readonly ObjectArrayEventPropertyGetter getter;
        private readonly int index;

        public ObjectArrayNestedEntryPropertyGetterArrayObjectArray(
            int propertyIndex,
            EventType fragmentType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            int index,
            ObjectArrayEventPropertyGetter getter)
            : base(propertyIndex, fragmentType, eventBeanTypedEventFactory)
        {
            this.index = index;
            this.getter = getter;
        }

        public override object HandleNestedValue(object value)
        {
            return BaseNestableEventUtil.HandleNestedValueArrayWithObjectArray(value, index, getter);
        }

        public override object HandleNestedValueFragment(object value)
        {
            return BaseNestableEventUtil.HandleNestedValueArrayWithObjectArrayFragment(
                value,
                index,
                getter,
                FragmentType,
                EventBeanTypedEventFactory);
        }

        public override bool HandleNestedValueExists(object value)
        {
            return BaseNestableEventUtil.HandleNestedValueArrayWithObjectArrayExists(value, index, getter);
        }

        public override CodegenExpression HandleNestedValueCodegen(
            CodegenExpression refName,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return BaseNestableEventUtil.HandleNestedValueArrayWithObjectArrayCodegen(
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
            return BaseNestableEventUtil.HandleNestedValueArrayWithObjectArrayExistsCodegen(
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
            return BaseNestableEventUtil.HandleNestedValueArrayWithObjectArrayFragmentCodegen(
                index,
                getter,
                refName,
                codegenMethodScope,
                codegenClassScope,
                GetType());
        }
    }
} // end of namespace