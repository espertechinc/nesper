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
    /// Getter for Map-entries with well-defined fragment type.
    /// </summary>
    public class ObjectArrayArrayPropertyGetter : ObjectArrayEventPropertyGetterAndIndexed
    {
        private readonly int _propertyIndex;
        private readonly int _index;
        private readonly EventAdapterService _eventAdapterService;
        private readonly EventType _fragmentType;
    
        /// <summary>Ctor. </summary>
        /// <param name="propertyIndex">property index</param>
        /// <param name="index">array index</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        /// <param name="fragmentType">type of the entry returned</param>
        public ObjectArrayArrayPropertyGetter(int propertyIndex, int index, EventAdapterService eventAdapterService, EventType fragmentType)
        {
            _propertyIndex = propertyIndex;
            _index = index;
            _fragmentType = fragmentType;
            _eventAdapterService = eventAdapterService;
        }
    
        public bool IsObjectArrayExistsProperty(object[] array)
        {
            return true;
        }
    
        public object GetObjectArray(object[] array)
        {
            return GetObjectArrayInternal(array, _index);
        }
    
        public object Get(EventBean eventBean, int index)
        {
            object[] array = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(eventBean);
            return GetObjectArrayInternal(array, index);
        }
    
        public object Get(EventBean eventBean)
        {
            object[] array = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(eventBean);
            return GetObjectArray(array);
        }
    
        private object GetObjectArrayInternal(object[] array, int index)
        {
            object value = array[_propertyIndex];
            return BaseNestableEventUtil.GetBNArrayValueAtIndexWithNullCheck(value, index);
        }
    
        public bool IsExistsProperty(EventBean eventBean)
        {
            return true;
        }
    
        public object GetFragment(EventBean obj)
        {
            object fragmentUnderlying = Get(obj);
            return BaseNestableEventUtil.GetBNFragmentNonPono(fragmentUnderlying, _fragmentType, _eventAdapterService);
        }

        public ICodegenExpression CodegenEventBeanGet(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingGet(CastUnderlying(typeof(object[]), beanExpression), context);
        }

        public ICodegenExpression CodegenEventBeanExists(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return ConstantTrue();
        }

        public ICodegenExpression CodegenEventBeanFragment(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingFragment(CastUnderlying(typeof(object[]), beanExpression), context);
        }

        public ICodegenExpression CodegenUnderlyingGet(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return StaticMethod(typeof(BaseNestableEventUtil), "GetBNArrayValueAtIndexWithNullCheck",
                ArrayAtIndex(underlyingExpression, Constant(_propertyIndex)), Constant(_index));
        }

        public ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return ConstantTrue();
        }

        public ICodegenExpression CodegenUnderlyingFragment(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            var mSvc = context.MakeAddMember(typeof(EventAdapterService), _eventAdapterService);
            var mType = context.MakeAddMember(typeof(EventType), _fragmentType);
            return StaticMethod(typeof(BaseNestableEventUtil), "GetBNFragmentNonPono", 
                CodegenUnderlyingGet(underlyingExpression, context), 
                Ref(mType.MemberName),
                Ref(mSvc.MemberName));
        }
    }
}
