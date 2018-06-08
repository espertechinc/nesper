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
using com.espertech.esper.compat.collections;
using com.espertech.esper.events.vaevent;
using com.espertech.esper.util;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events.bean
{
    /// <summary>
    /// Getter for a list property identified by a given index, using vanilla reflection.
    /// </summary>
    public class ListMethodPropertyGetter : BaseNativePropertyGetter
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
        public ListMethodPropertyGetter(MethodInfo method, int index, EventAdapterService eventAdapterService)
            : base(eventAdapterService, TypeHelper.GetGenericReturnType(method, false), null)
        {
            _index = index;
            _method = method;

            if (index < 0)
            {
                throw new ArgumentException("Invalid negative index value");
            }
        }

        public object Get(EventBean eventBean, int index)
        {
            return GetBeanPropInternal(eventBean.Underlying, index);
        }

        public object GetBeanProp(object @object)
        {
            return GetBeanPropInternal(@object, _index);
        }

        public object GetBeanPropInternal(object @object, int index)
        {
            try
            {
                var value = _method.Invoke(@object, (object[]) null).AsObjectList();
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
                throw PropertyUtility.GetMismatchException(_method, @object, e);
            }
            catch (TargetInvocationException e)
            {
                throw PropertyUtility.GetInvocationTargetException(_method, e);
            }
            catch (ArgumentException e)
            {
                throw new PropertyAccessException(e);
            }
        }

        internal static string GetBeanPropInternalCodegen(
            ICodegenContext context,
            Type beanPropType, 
            Type targetType, 
            MethodInfo method, 
            int index)
        {
            return context.AddMethod(beanPropType, targetType, "object", typeof(ListMethodPropertyGetter))
                .DeclareVar(typeof(object), "value",
                    ExprDotMethod(Ref("object"), method.Name))
                .IfRefNotTypeReturnConst("value", typeof(IList<object>), null)
                .DeclareVar(typeof(IList<object>), "l",
                    Cast(typeof(IList<object>), Ref("value")))
                .IfConditionReturnConst(Relational(
                    ExprDotMethod(Ref("l"), "get_Count"),
                    CodegenRelational.LE,
                    Constant(index)), null)
                .MethodReturn(Cast(
                    beanPropType,
                    ExprDotMethod(
                        Ref("l"), "get",
                        Constant(index))));
        }

        public bool IsBeanExistsProperty(object @object)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public override object Get(EventBean obj)
        {
            object underlying = obj.Underlying;
            return GetBeanProp(underlying);
        }

        public override String ToString()
        {
            return "ListMethodPropertyGetter " +
                   " method=" + _method +
                   " index=" + _index;
        }

        public override bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public override Type BeanPropType => TypeHelper.GetGenericReturnType(_method, false);

        public override Type TargetType => _method.DeclaringType;

        public override ICodegenExpression CodegenEventBeanGet(ICodegenExpression beanExpression,
            ICodegenContext context)
        {
            return CodegenUnderlyingGet(CastUnderlying(TargetType, beanExpression), context);
        }

        public override ICodegenExpression CodegenEventBeanExists(ICodegenExpression beanExpression,
            ICodegenContext context)
        {
            return ConstantTrue();
        }

        public override ICodegenExpression CodegenUnderlyingGet(ICodegenExpression underlyingExpression,
            ICodegenContext context)
        {
            return LocalMethod(
                GetBeanPropInternalCodegen(context, BeanPropType, TargetType, _method, _index),
                underlyingExpression);
        }

        public override ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression,
            ICodegenContext context)
        {
            return ConstantTrue();
        }
    }
} // end of namespace
