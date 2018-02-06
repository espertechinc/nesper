///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using XLR8.CGLib;

using com.espertech.esper.client;
using com.espertech.esper.events.vaevent;
using com.espertech.esper.util;
using com.espertech.esper.codegen.model.expression;
using com.espertech.esper.codegen.core;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events.bean
{
    /// <summary>
    /// Property getter using XLR8.CGLib's FastMethod instance.
    /// </summary>
    public sealed class CGLibPropertyMethodGetter 
        : BaseNativePropertyGetter
        , BeanEventPropertyGetter
    {
        private readonly FastMethod _fastMethod;
    
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="method">the underlying method</param>
        /// <param name="fastMethod">The fast method.</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        public CGLibPropertyMethodGetter(MethodInfo method, FastMethod fastMethod, EventAdapterService eventAdapterService)
            : base(eventAdapterService, fastMethod.ReturnType, TypeHelper.GetGenericReturnType(method, true))
        {
            _fastMethod = fastMethod;
        }

        public Object GetBeanProp(Object obj)
        {
            try
            {
            return _fastMethod.Invoke(obj);
            }
            catch (InvalidCastException e)
            {
                throw PropertyUtility.GetMismatchException(_fastMethod.Target, obj, e);
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
    
        public override Object Get(EventBean obj)
        {
            return GetBeanProp(obj.Underlying);
        }
    
        public override String ToString()
        {
            return "CGLibPropertyGetter " +
                    "fastMethod=" + _fastMethod;
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
