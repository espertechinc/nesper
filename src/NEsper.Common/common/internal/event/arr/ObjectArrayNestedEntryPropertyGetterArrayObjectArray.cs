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

namespace com.espertech.esper.common.@internal.@event.arr
{
    public class ObjectArrayNestedEntryPropertyGetterArrayObjectArray : ObjectArrayNestedEntryPropertyGetterBase
    {
        private readonly ObjectArrayEventPropertyGetter _getter;
        private readonly int _index;

        public ObjectArrayNestedEntryPropertyGetterArrayObjectArray(
            int propertyIndex,
            EventType fragmentType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            int index,
            ObjectArrayEventPropertyGetter getter)
            : base(propertyIndex, fragmentType, eventBeanTypedEventFactory)
        {
            this._index = index;
            this._getter = getter;
        }

        public override object HandleNestedValue(object value)
        {
            return BaseNestableEventUtil.HandleNestedValueArrayWithObjectArray(value, _index, _getter);
        }

        public override object HandleNestedValueFragment(object value)
        {
            return BaseNestableEventUtil.HandleNestedValueArrayWithObjectArrayFragment(
                value,
                _index,
                _getter,
                FragmentType,
                EventBeanTypedEventFactory);
        }

        public override bool HandleNestedValueExists(object value)
        {
            return BaseNestableEventUtil.HandleNestedValueArrayWithObjectArrayExists(value, _index, _getter);
        }

        public override CodegenExpression HandleNestedValueCodegen(
            CodegenExpression refName,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return BaseNestableEventUtil.HandleNestedValueArrayWithObjectArrayCodegen(
                _index,
                _getter,
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
                _index,
                _getter,
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
                _index,
                _getter,
                refName,
                codegenMethodScope,
                codegenClassScope,
                GetType());
        }
    }
} // end of namespace