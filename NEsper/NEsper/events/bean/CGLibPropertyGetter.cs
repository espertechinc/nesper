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
using com.espertech.esper.codegen.core;
using com.espertech.esper.codegen.model.expression;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events.bean
{
    /// <summary>
    /// Property getter using XLR8.CGLib's FastMethod instance.
    /// </summary>
    public sealed class CGLibPropertyGetter
        : BaseNativePropertyGetter
        , BeanEventPropertyGetter
    {
        private readonly Getter _fastGetter;
        private readonly PropertyInfo _propInfo;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="fastProp">is the method to use to retrieve a value from the object.</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        public CGLibPropertyGetter(PropertyInfo property, FastProperty fastProp, EventAdapterService eventAdapterService)
            : base(eventAdapterService, fastProp.PropertyType, TypeHelper.GetGenericPropertyType(property, true))
        {
            _fastGetter = fastProp.GetGetInvoker();
            _propInfo = property;
        }

        public Object GetBeanProp(Object obj)
        {
            try
            {
                return _fastGetter(obj);
            }

#if false
            catch (InvalidCastException e)
            {
                throw PropertyUtility.GetMismatchException(_propInfo.GetGetMethod(), obj, e);
            }
            catch (PropertyAccessException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new PropertyAccessException(e);
            }
#else
            catch (Exception e)
            {
                if (e is InvalidCastException)
                    throw PropertyUtility.GetMismatchException(_propInfo.GetGetMethod(), obj, (InvalidCastException)e);
                if (e is PropertyAccessException)
                    throw;

                throw new PropertyAccessException(e);
            }
#endif
        }

        public bool IsBeanExistsProperty(Object obj)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public override Object Get(EventBean obj)
        {
            try
            {
                return _fastGetter(obj.Underlying);
            }
            catch (InvalidCastException e)
            {
                throw PropertyUtility.GetMismatchException(_propInfo.GetGetMethod(), obj.Underlying, e);
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

        public override String ToString()
        {
            return "CGLibPropertyGetter " +
                    "fastProp =" + _fastGetter;
        }

        public override bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public override Type BeanPropType => _propInfo.PropertyType;
        public override Type TargetType => _propInfo.DeclaringType;

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
            return ExprDotMethod(underlyingExpression, _propInfo.GetGetMethod().Name);
        }

        public override ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return ConstantTrue();
        }
    }
}
