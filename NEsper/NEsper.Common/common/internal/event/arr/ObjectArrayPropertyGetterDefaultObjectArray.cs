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
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.@event.core;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.arr
{
    /// <summary>
    ///     Getter for map entry.
    /// </summary>
    public class ObjectArrayPropertyGetterDefaultObjectArray : ObjectArrayPropertyGetterDefaultBase
    {
        public ObjectArrayPropertyGetterDefaultObjectArray(
            int propertyIndex, EventType fragmentEventType, EventBeanTypedEventFactory eventBeanTypedEventFactory) :
            base(propertyIndex, fragmentEventType, eventBeanTypedEventFactory)
        {
        }

        internal override object HandleCreateFragment(object value)
        {
            if (fragmentEventType == null) {
                return null;
            }

            return BaseNestableEventUtil.HandleBNCreateFragmentObjectArray(
                value, fragmentEventType, eventBeanTypedEventFactory);
        }

        internal override CodegenExpression HandleCreateFragmentCodegen(
            CodegenExpression value, CodegenClassScope codegenClassScope)
        {
            if (fragmentEventType == null) {
                return ConstantNull();
            }

            var mSvc = codegenClassScope.AddOrGetFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
            var mType = codegenClassScope.AddFieldUnshared(
                true, typeof(EventType),
                EventTypeUtility.ResolveTypeCodegen(fragmentEventType, EPStatementInitServicesConstants.REF));
            return StaticMethod(typeof(BaseNestableEventUtil), "handleBNCreateFragmentObjectArray", value, mType, mSvc);
        }
    }
} // end of namespace