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

using com.espertech.esper.client;
using com.espertech.esper.codegen.core;
using com.espertech.esper.codegen.model.expression;
using com.espertech.esper.compat.collections;
using com.espertech.esper.events.vaevent;
using com.espertech.esper.util;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events.bean
{
    /// <summary>/// Getter for an enumerable property identified by a given index, using vanilla
    /// reflection.
    /// </summary>
    public class EnumerableMethodPropertyGetter 
        : BaseNativePropertyGetter
        , BeanEventPropertyGetter
        , EventPropertyGetterAndIndexed
    {
        private readonly MethodInfo _method;
        private readonly int _index;
    
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="method">is the method to use to retrieve a value from the object</param>
        /// <param name="index">is tge index within the array to get the property from</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        public EnumerableMethodPropertyGetter(MethodInfo method, int index, EventAdapterService eventAdapterService)
            : base(eventAdapterService, TypeHelper.GetGenericReturnType(method, false), null)
        {
            _index = index;
            _method = method;
    
            if (index < 0)
            {
                throw new ArgumentException("Invalid negative index value");
            }
        }

        /// <summary>
        /// Returns the enumerable at a certain index, or null.
        /// </summary>
        /// <param name="value">the enumerable</param>
        /// <param name="index">index</param>
        /// <returns>
        /// value at index
        /// </returns>
        public static Object GetBeanEventEnumerableValue(Object value, int index)
        {
            var enumerable = value as IEnumerable;
            if (enumerable == null)
                return null;

            return enumerable.AtIndex(index);
        }

        public Object GetBeanProp(Object o)
        {
            return GetBeanPropInternal(o, _index);
        }

        public Object Get(EventBean eventBean, int index)
        {
            return GetBeanPropInternal(eventBean.Underlying, index);
        }

        private Object GetBeanPropInternal(Object o, int index)
        {
            try
            {
                Object value = _method.Invoke(o, null);
                return GetBeanEventEnumerableValue(value, index);
            }
            catch (InvalidCastException e)
            {
                throw PropertyUtility.GetMismatchException(_method, o, e);
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

        internal static String GetBeanPropCodegen(ICodegenContext context, Type beanPropType, Type targetType, MethodInfo method, int index)
        {
            return context.AddMethod(beanPropType, targetType, "object", typeof(EnumerableMethodPropertyGetter))
                .DeclareVar(typeof(object), "value", ExprDotMethod(Ref("object"), method.Name))
                .MethodReturn(Cast(beanPropType, StaticMethod(
                    typeof(EnumerableMethodPropertyGetter), "GetBeanEventEnumerableValue",
                    Ref("value"),
                    Constant(index))));
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
            return "EnumerableMethodPropertyGetter " +
                    " method=" + _method +
                    " index=" + _index;
        }
    
        public override bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public override Type BeanPropType => TypeHelper.GetGenericReturnType(_method, false);
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
            return LocalMethod(GetBeanPropCodegen(context, BeanPropType, TargetType, _method, _index), underlyingExpression);
        }

        public override ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return ConstantTrue();
        }
    }
}
