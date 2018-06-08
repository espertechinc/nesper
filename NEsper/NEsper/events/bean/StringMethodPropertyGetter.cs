///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.codegen.core;
using com.espertech.esper.codegen.model.expression;
using com.espertech.esper.events.vaevent;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events.bean
{
    /// <summary>
    /// Getter for a string property identified by a given index, using vanilla reflection.
    /// </summary>
    public class StringMethodPropertyGetter 
        : BaseNativePropertyGetter
        , BeanEventPropertyGetter
        , EventPropertyGetterAndIndexed
    {
        private readonly MethodInfo _method;
        private readonly int _index;
    
        /// <summary>Constructor. </summary>
        /// <param name="method">is the method to use to retrieve a value from the object</param>
        /// <param name="index">is tge index within the array to get the property from</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        public StringMethodPropertyGetter(MethodInfo method, int index, EventAdapterService eventAdapterService)
            : base(eventAdapterService, typeof(char), null)
        {
            _index = index;
            _method = method;
    
            if (index < 0)
            {
                throw new ArgumentException("Invalid negative index value");
            }
        }
    
        public Object Get(EventBean eventBean, int index)
        {
            return GetBeanPropInternal(eventBean.Underlying, index);
        }
    
        public Object GetBeanProp(Object @object)
        {
            return GetBeanPropInternal(@object, _index);
        }
    
        private Object GetBeanPropInternal(Object @object, int index)
        {
            try
            {
                var value = (String)_method.Invoke(@object, null);
                if (value.Length <= index)
                {
                    return null;
                }

                return value[index];
            }
            catch (InvalidCastException e)
            {
                throw PropertyUtility.GetMismatchException(_method, @object, e);
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
            Object underlying = eventBean.Underlying;
            return GetBeanProp(underlying);
        }
    
        public override String ToString()
        {
            return "StringMethodPropertyGetter " +
                    " method=" + _method +
                    " index=" + _index;
        }
    
        public override bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public override Type BeanPropType => _method.ReturnType;
        public override Type TargetType => _method.DeclaringType;

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
            return ExprDotMethod(underlyingExpression, _method.Name);
        }

        public override ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return ConstantTrue();
        }
    }
}
