///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.codegen.core;
using com.espertech.esper.codegen.model.expression;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events.arr
{
    /// <summary>
    /// A getter that works on EventBean events residing within a Map as an event property.
    /// </summary>
    public class ObjectArrayEventBeanEntryPropertyGetter : ObjectArrayEventPropertyGetter
    {
        private readonly int _propertyIndex;
        private readonly EventPropertyGetterSPI _eventBeanEntryGetter;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="propertyIndex">the property to look at</param>
        /// <param name="eventBeanEntryGetter">the getter for the map entry</param>
        public ObjectArrayEventBeanEntryPropertyGetter(int propertyIndex, EventPropertyGetterSPI eventBeanEntryGetter)
        {
            this._propertyIndex = propertyIndex;
            this._eventBeanEntryGetter = eventBeanEntryGetter;
        }

        public Object GetObjectArray(Object[] array)
        {
            // If the map does not contain the key, this is allowed and represented as null
            Object value = array[_propertyIndex];

            if (value == null)
            {
                return null;
            }

            // Object within the map
            EventBean theEvent = (EventBean)value;
            return _eventBeanEntryGetter.Get(theEvent);
        }

        private string GetObjectArrayCodegen(ICodegenContext context)
        {
            return context.AddMethod(typeof(Object), typeof(Object[]), "array", GetType())
                .DeclareVar(typeof(Object), "value", ArrayAtIndex(Ref("array"), Constant(_propertyIndex)))
                .IfRefNullReturnNull("value")
                .DeclareVarWCast(typeof(EventBean), "theEvent", "value")
                .MethodReturn(_eventBeanEntryGetter.CodegenEventBeanGet(Ref("theEvent"), context));
        }

        public bool IsObjectArrayExistsProperty(Object[] array)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public Object Get(EventBean obj)
        {
            return GetObjectArray(BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(obj));
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public Object GetFragment(EventBean obj)
        {
            // If the map does not contain the key, this is allowed and represented as null
            Object value = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(obj)[_propertyIndex];

            if (value == null)
            {
                return null;
            }

            // Object within the map
            EventBean theEvent = (EventBean)value;
            return _eventBeanEntryGetter.GetFragment(theEvent);
        }

        private string GetFragmentCodegen(ICodegenContext context)
        {
            return context.AddMethod(typeof(Object), typeof(Object[]), "array", this.GetType())
                .DeclareVar(typeof(Object), "value", ArrayAtIndex(Ref("array"), Constant(_propertyIndex)))
                .IfRefNullReturnNull("value")
                .DeclareVarWCast(typeof(EventBean), "theEvent", "value")
                .MethodReturn(_eventBeanEntryGetter.CodegenEventBeanFragment(Ref("theEvent"), context));
        }

        public ICodegenExpression CodegenEventBeanGet(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingGet(CastUnderlying(typeof(Object[]), beanExpression), context);
        }

        public ICodegenExpression CodegenEventBeanExists(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return ConstantTrue();
        }

        public ICodegenExpression CodegenEventBeanFragment(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingFragment(
                CastUnderlying(typeof(Object[]), beanExpression), context);
        }

        public ICodegenExpression CodegenUnderlyingGet(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return LocalMethod(GetObjectArrayCodegen(context), underlyingExpression);
        }

        public ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return ConstantTrue();
        }

        public ICodegenExpression CodegenUnderlyingFragment(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return LocalMethod(GetFragmentCodegen(context), underlyingExpression);
        }
    }
} // end of namespace