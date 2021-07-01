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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.arr
{
    /// <summary>
    ///     A getter that works on EventBean events residing within a Map as an event property.
    /// </summary>
    public class ObjectArrayEventBeanEntryPropertyGetter : ObjectArrayEventPropertyGetter
    {
        private readonly EventPropertyGetterSPI eventBeanEntryGetter;

        private readonly int propertyIndex;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="propertyIndex">the property to look at</param>
        /// <param name="eventBeanEntryGetter">the getter for the map entry</param>
        public ObjectArrayEventBeanEntryPropertyGetter(
            int propertyIndex,
            EventPropertyGetterSPI eventBeanEntryGetter)
        {
            this.propertyIndex = propertyIndex;
            this.eventBeanEntryGetter = eventBeanEntryGetter;
        }

        public object GetObjectArray(object[] array)
        {
            // If the map does not contain the key, this is allowed and represented as null
            var value = array[propertyIndex];

            if (value == null) {
                return null;
            }

            // Object within the map
            var theEvent = (EventBean) value;
            return eventBeanEntryGetter.Get(theEvent);
        }

        public bool IsObjectArrayExistsProperty(object[] array)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public object Get(EventBean obj)
        {
            return GetObjectArray(BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(obj));
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public object GetFragment(EventBean obj)
        {
            // If the map does not contain the key, this is allowed and represented as null
            var value = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(obj)[propertyIndex];

            if (value == null) {
                return null;
            }

            // Object within the map
            var theEvent = (EventBean) value;
            return eventBeanEntryGetter.GetFragment(theEvent);
        }

        public CodegenExpression EventBeanGetCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingGetCodegen(
                CastUnderlying(typeof(object[]), beanExpression),
                codegenMethodScope,
                codegenClassScope);
        }

        public CodegenExpression EventBeanExistsCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ConstantTrue();
        }

        public CodegenExpression EventBeanFragmentCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingFragmentCodegen(
                CastUnderlying(typeof(object[]), beanExpression),
                codegenMethodScope,
                codegenClassScope);
        }

        public CodegenExpression UnderlyingGetCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(GetObjectArrayCodegen(codegenMethodScope, codegenClassScope), underlyingExpression);
        }

        public CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ConstantTrue();
        }

        public CodegenExpression UnderlyingFragmentCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(GetFragmentCodegen(codegenMethodScope, codegenClassScope), underlyingExpression);
        }

        private CodegenMethod GetObjectArrayCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam(typeof(object[]), "array")
                .Block
                .DeclareVar<object>("value", ArrayAtIndex(Ref("array"), Constant(propertyIndex)))
                .IfRefNullReturnNull("value")
                .DeclareVarWCast(typeof(EventBean), "theEvent", "value")
                .MethodReturn(
                    eventBeanEntryGetter.EventBeanGetCodegen(Ref("theEvent"), codegenMethodScope, codegenClassScope));
        }

        private CodegenMethod GetFragmentCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam(typeof(object[]), "array")
                .Block
                .DeclareVar<object>("value", ArrayAtIndex(Ref("array"), Constant(propertyIndex)))
                .IfRefNullReturnNull("value")
                .DeclareVarWCast(typeof(EventBean), "theEvent", "value")
                .MethodReturn(
                    eventBeanEntryGetter.EventBeanFragmentCodegen(
                        Ref("theEvent"),
                        codegenMethodScope,
                        codegenClassScope));
        }
    }
} // end of namespace