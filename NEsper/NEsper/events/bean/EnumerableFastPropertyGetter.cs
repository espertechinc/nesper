///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Reflection;

using XLR8.CGLib;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.events.vaevent;
using com.espertech.esper.util;
using com.espertech.esper.codegen.model.expression;
using com.espertech.esper.codegen.core;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events.bean
{
    /// <summary>
    /// Getter for a enumerable property identified by a given index, using the CGLIB fast
    /// method.
    /// </summary>
    public class EnumerableFastPropertyGetter 
        : BaseNativePropertyGetter
        , BeanEventPropertyGetter
        , EventPropertyGetterAndIndexed
    {
        private readonly FastMethod _fastMethod;
        private readonly int _index;
    
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="method">the underlying method</param>
        /// <param name="fastMethod">is the method to use to retrieve a value from the object</param>
        /// <param name="index">is tge index within the array to get the property from</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        public EnumerableFastPropertyGetter(MethodInfo method, FastMethod fastMethod, int index, EventAdapterService eventAdapterService)
            : base(eventAdapterService, TypeHelper.GetGenericReturnType(method, false), null)
        {
            _index = index;
            _fastMethod = fastMethod;
    
            if (index < 0)
            {
                throw new ArgumentException("Invalid negative index value");
            }
        }
    
        public Object GetBeanProp(Object o)
        {
            try
            {
                Object value = _fastMethod.Invoke(o);
                return EnumerableMethodPropertyGetter.GetBeanEventEnumerableValue(value, _index);
            }
            catch (InvalidCastException e)
            {
                throw PropertyUtility.GetMismatchException(_fastMethod.Target, o, e);
            }
            catch (PropertyAccessException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new PropertyAccessException(e);
            }
        }
    
        public Object Get(EventBean eventBean, int index) 
        {
            return EnumerableMethodPropertyGetter.GetBeanEventEnumerableValue(eventBean.Underlying, index);
        }
    
        public bool IsBeanExistsProperty(Object o)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }
    
        public override Object Get(EventBean eventBean)
        {
            Object underlying = eventBean.Underlying;
            return GetBeanProp(underlying);
        }
    
        public override String ToString()
        {
            return "EnumerableFastPropertyGetter " +
                    " fastMethod=" + _fastMethod +
                    " index=" + _index;
        }
    
        public override bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public override Type BeanPropType => TypeHelper.GetGenericReturnType(_fastMethod.Target, false);
        public override Type TargetType => _fastMethod.DeclaringType.TargetType;

        public override ICodegenExpression CodegenEventBeanGet(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingGet(CastUnderlying(TargetType, beanExpression), context);
        }

        public override ICodegenExpression CodegenEventBeanExists(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return ConstantTrue();
        }

        public override ICodegenExpression CodegenUnderlyingGet(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return LocalMethod(EnumerableMethodPropertyGetter.GetBeanPropCodegen(
                context, BeanPropType, TargetType, _fastMethod.Target, _index), underlyingExpression);
        }

        public override ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return ConstantTrue();
        }
    }
}
