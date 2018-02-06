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
using com.espertech.esper.util;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events.bean
{
    /// <summary>
    /// Property getter for methods using vanilla reflection.
    /// </summary>
    public sealed class ReflectionPropMethodGetter
        : BaseNativePropertyGetter
        , BeanEventPropertyGetter
    {
        private readonly MethodInfo _method;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="method">is the regular reflection method to use to obtain values for a field.</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        public ReflectionPropMethodGetter(MethodInfo method, EventAdapterService eventAdapterService)
            : base(eventAdapterService, method.ReturnType, TypeHelper.GetGenericReturnType(method, false))
        {
            this._method = method;
        }

        public Object GetBeanProp(Object @object)
        {
            try
            {
                return _method.Invoke(@object, (Object[])null);
            }
            catch (ArgumentException e)
            {
                throw PropertyUtility.GetIllegalArgumentException(_method, e);
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
            return true;
        }

        public override Object Get(EventBean obj)
        {
            Object underlying = obj.Underlying;
            return GetBeanProp(underlying);
        }

        public override String ToString()
        {
            return "ReflectionPropMethodGetter " +
                    "method=" + _method.ToString();
        }

        public override bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public override Type BeanPropType => _method.ReturnType;

        public override Type TargetType => _method.DeclaringType;

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
            return ExprDotMethod(underlyingExpression, _method.Name);
        }

        public override ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return ConstantTrue();
        }
    }
} // end of namespace