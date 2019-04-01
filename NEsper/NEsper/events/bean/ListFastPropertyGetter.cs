///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.codegen.core;
using com.espertech.esper.codegen.model.expression;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.magic;
using com.espertech.esper.events.vaevent;
using com.espertech.esper.util;

using XLR8.CGLib;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events.bean
{
    /// <summary>
    /// Getter for a list property identified by a given index, using the CGLIB fast method.
    /// </summary>
    public class ListFastPropertyGetter
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
        public ListFastPropertyGetter(MethodInfo method, FastMethod fastMethod, int index, EventAdapterService eventAdapterService)
            : base(eventAdapterService, TypeHelper.GetGenericReturnType(method, false), null)
        {
            this._index = index;
            this._fastMethod = fastMethod;

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

        public Object GetBeanPropInternal(Object @object, int index)
        {
            try
            {
                var value = _fastMethod.Invoke(@object, null).AsObjectList();
                if (value == null)
                {
                    return null;
                }

                if (value.Count <= index)
                {
                    return null;
                }
                return value[index];
            }
            catch (InvalidCastException e)
            {
                throw PropertyUtility.GetMismatchException(_fastMethod.Target, @object, e);
            }
            catch (TargetInvocationException e)
            {
                throw PropertyUtility.GetInvocationTargetException(_fastMethod.Target, e);
            }
        }

        public bool IsBeanExistsProperty(Object @object)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public override Object Get(EventBean obj)
        {
            var underlying = obj.Underlying;
            return GetBeanProp(underlying);
        }

        public override String ToString()
        {
            return "ListFastPropertyGetter " +
                    " fastMethod=" + _fastMethod.ToString() +
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
            return LocalMethod(
                ListMethodPropertyGetter.GetBeanPropInternalCodegen(
                    context, BeanPropType, TargetType, _fastMethod.Target, _index), underlyingExpression);
        }

        public override ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return ConstantTrue();
        }
    }
} // end of namespace