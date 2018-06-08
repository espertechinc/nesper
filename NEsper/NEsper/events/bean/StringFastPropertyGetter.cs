///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using XLR8.CGLib;

using com.espertech.esper.client;
using com.espertech.esper.codegen.core;
using com.espertech.esper.codegen.model.expression;
using com.espertech.esper.events.vaevent;
using com.espertech.esper.util;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events.bean
{
    /// <summary>
    /// Getter for a string property identified by a given index, using the CGLIB fast method.
    /// </summary>
    public class StringFastPropertyGetter : BaseNativePropertyGetter
        , BeanEventPropertyGetter
        , EventPropertyGetterAndIndexed
    {
        private readonly FastMethod _fastMethod;
        private readonly int _index;
    
        /// <summary>Constructor. </summary>
        /// <param name="fastMethod">is the method to use to retrieve a value from the object</param>
        /// <param name="index">is tge index within the array to get the property from</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        public StringFastPropertyGetter(FastMethod fastMethod, int index, EventAdapterService eventAdapterService)
            : base(eventAdapterService, typeof(char), null)
        {
            _index = index;
            _fastMethod = fastMethod;
    
            if (index < 0)
            {
                throw new ArgumentException("Invalid negative index value");
            }
        }
    
        public Object GetBeanProp(Object @object)
        {
            return GetBeanPropInternal(@object, _index);
        }
    
        private Object GetBeanPropInternal(Object @object, int index)
        {
            try
            {
                String value = (String)_fastMethod.Invoke(@object);
                if (value.Length <= index)
                {
                    return null;
                }

                return value[index];
            }
            catch (InvalidCastException e)
            {
                throw PropertyUtility.GetMismatchException(_fastMethod.Target, @object, e);
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
    
        public bool IsBeanExistsProperty(Object @object)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }
    
        public override Object Get(EventBean eventBean)
        {
            return GetBeanProp(eventBean.Underlying);
        }
    
        public Object Get(EventBean eventBean, int index)
        {
            return GetBeanPropInternal(eventBean.Underlying, index);
        }
    
        public override String ToString()
        {
            return "StringFastPropertyGetter " +
                    " fastMethod=" + _fastMethod +
                    " index=" + _index;
        }
    
        public override bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public override Type BeanPropType => _fastMethod.ReturnType;
        public override Type TargetType => _fastMethod.DeclaringType.TargetType;

        public override ICodegenExpression CodegenEventBeanGet(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingGet(
                CastUnderlying(TargetType, beanExpression), context);
        }

        public override ICodegenExpression CodegenEventBeanExists(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return ConstantTrue();
        }

        public override ICodegenExpression CodegenUnderlyingGet(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return ExprDotMethod(underlyingExpression, _fastMethod.Name);
        }

        public override ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return ConstantTrue();
        }
    }
}
