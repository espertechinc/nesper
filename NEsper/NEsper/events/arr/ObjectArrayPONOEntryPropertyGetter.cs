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
using com.espertech.esper.events.bean;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events.arr
{
    /// <summary>
    ///     A getter that works on PONO events residing within a Map as an event property.
    /// </summary>
    public class ObjectArrayPonoEntryPropertyGetter
        : BaseNativePropertyGetter
            , ObjectArrayEventPropertyGetter
    {
        private readonly BeanEventPropertyGetter _entryGetter;
        private readonly int _propertyIndex;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="entryGetter">the getter for the map entry</param>
        /// <param name="eventAdapterService">for producing wrappers to objects</param>
        /// <param name="returnType">type of the entry returned</param>
        /// <param name="propertyIndex">index</param>
        /// <param name="nestedComponentType">nested component type</param>
        public ObjectArrayPonoEntryPropertyGetter(int propertyIndex, BeanEventPropertyGetter entryGetter,
            EventAdapterService eventAdapterService, Type returnType, Type nestedComponentType)
            : base(eventAdapterService, returnType, nestedComponentType)
        {
            _propertyIndex = propertyIndex;
            _entryGetter = entryGetter;
        }

        public override Type TargetType => typeof(object[]);
        public override Type BeanPropType => typeof(object);

        public object GetObjectArray(object[] array)
        {
            // If the map does not contain the key, this is allowed and represented as null
            var value = array[_propertyIndex];

            if (value == null) return null;

            // object within the map
            if (value is EventBean) return _entryGetter.Get((EventBean) value);
            return _entryGetter.GetBeanProp(value);
        }

        public bool IsObjectArrayExistsProperty(object[] array)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public override object Get(EventBean obj)
        {
            var array = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(obj);
            return GetObjectArray(array);
        }

        public override bool IsExistsProperty(EventBean eventBean)
        {
            var array = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(eventBean);
            return IsExistsProperty(array);
        }

        public override ICodegenExpression CodegenEventBeanGet(ICodegenExpression beanExpression,
            ICodegenContext context)
        {
            return CodegenUnderlyingGet(CastUnderlying(typeof(object[]), beanExpression), context);
        }

        public override ICodegenExpression CodegenEventBeanExists(ICodegenExpression beanExpression,
            ICodegenContext context)
        {
            return CodegenUnderlyingExists(CastUnderlying(typeof(object[]), beanExpression), context);
        }

        public override ICodegenExpression CodegenUnderlyingGet(ICodegenExpression underlyingExpression,
            ICodegenContext context)
        {
            return LocalMethod(GetObjectArrayCodegen(context), underlyingExpression);
        }

        public override ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression,
            ICodegenContext context)
        {
            return LocalMethod(IsExistsPropertyCodegen(context), underlyingExpression);
        }

        private string GetObjectArrayCodegen(ICodegenContext context)
        {
            return context.AddMethod(typeof(object), typeof(object[]), "array", GetType())
                .DeclareVar(typeof(object), "value",
                    ArrayAtIndex(Ref("array"),
                        Constant(_propertyIndex)))
                .IfRefNullReturnNull("value")
                .IfInstanceOf("value", typeof(EventBean))
                .BlockReturn(_entryGetter.CodegenEventBeanGet(CastRef(typeof(EventBean), "value"),
                    context))
                .MethodReturn(_entryGetter.CodegenUnderlyingGet(
                    Cast(_entryGetter.TargetType, Ref("value")), context));
        }

        private bool IsExistsProperty(object[] array)
        {
            var value = array[_propertyIndex];

            if (value == null) return false;

            // object within the map
            if (value is EventBean) return _entryGetter.IsExistsProperty((EventBean) value);
            return _entryGetter.IsBeanExistsProperty(value);
        }

        private string IsExistsPropertyCodegen(ICodegenContext context)
        {
            return context.AddMethod(typeof(bool), typeof(object[]), "array", GetType())
                .DeclareVar(typeof(object), "value",
                    ArrayAtIndex(Ref("array"),
                        Constant(_propertyIndex)))
                .IfRefNullReturnFalse("value")
                .IfInstanceOf("value", typeof(EventBean))
                .BlockReturn(_entryGetter.CodegenEventBeanExists(CastRef(typeof(EventBean), "value"),
                    context))
                .MethodReturn(_entryGetter.CodegenUnderlyingExists(
                    Cast(_entryGetter.TargetType, Ref("value")), context));
        }
    }
} // end of namespace