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
using com.espertech.esper.compat;
using com.espertech.esper.compat.magic;
using com.espertech.esper.events.vaevent;
using com.espertech.esper.util;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events.bean
{
    /// <summary>
    /// Property getter using lambda expressions
    /// </summary>
    public sealed class LambdaPropertyGetter 
        : BaseNativePropertyGetter
        , BeanEventPropertyGetter
    {
        private readonly Func<object, object> _lambdaFunc;
        private readonly MethodInfo _lambdaMethod;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="method">the underlying method</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        public LambdaPropertyGetter(MethodInfo method, EventAdapterService eventAdapterService)
            : base(eventAdapterService, method.ReturnType, TypeHelper.GetGenericReturnType(method, true))
        {
            _lambdaMethod = method;
            _lambdaFunc = MagicType.GetLambdaAccessor(method);
        }
    
        public Object GetBeanProp(Object obj)
        {
            try
            {
                return _lambdaFunc.Invoke(obj);
            }
            catch (InvalidCastException e)
            {
                throw PropertyUtility.GetMismatchException(_lambdaMethod, obj, e);
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

        public bool IsBeanExistsProperty(Object obj)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public override Object Get(EventBean eventBean)
        {
            try
            {
                return _lambdaFunc.Invoke(eventBean.Underlying);
            }
            catch (Exception e)
            {
                if (e is PropertyAccessException)
                    throw;
                if (e is NullReferenceException)
                    throw;
                if (e is InvalidCastException)
                    throw PropertyUtility.GetMismatchException(_lambdaMethod, eventBean.Underlying, (InvalidCastException) e);

                throw new PropertyAccessException(e);
            }
        }

        public override String ToString()
        {
            return "LambdaPropertyGetter " +
                   "lambdaFunc=" + _lambdaFunc;
        }
    
        public override bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }
        public override Type BeanPropType => _lambdaMethod.ReturnType;
        public override Type TargetType => _lambdaMethod.DeclaringType;

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
            throw new UnsupportedOperationException();
        }

        public override ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return ConstantTrue();
        }
    }
}
