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

using XLR8.CGLib;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events.bean
{
    /// <summary>
    ///     Getter for an array property identified by a given index, using the CGLIB fast method.
    /// </summary>
    public class ArrayFastPropertyGetter
        : BaseNativePropertyGetter
        , BeanEventPropertyGetter
        , EventPropertyGetterAndIndexed
    {
        private readonly FastMethod _fastMethod;
        private readonly int _index;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="fastMethod">is the method to use to retrieve a value from the object</param>
        /// <param name="index">is tge index within the array to get the property from</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        public ArrayFastPropertyGetter(FastMethod fastMethod, int index, EventAdapterService eventAdapterService)
            : base(eventAdapterService, fastMethod.ReturnType.GetElementType(), null)
        {
            _index = index;
            _fastMethod = fastMethod;

            if (index < 0) throw new ArgumentException("Invalid negative index value");
        }

        public object GetBeanProp(object @object)
        {
            return GetBeanPropInternal(@object, _index);
        }

        public bool IsBeanExistsProperty(object @object)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public override object Get(EventBean obj)
        {
            return GetBeanProp(obj.Underlying);
        }

        public override bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public override Type BeanPropType => _fastMethod.ReturnType.GetElementType();
        public override Type TargetType => _fastMethod.ReturnType; // NOTE: should this have been DeclaringType?

        public override ICodegenExpression CodegenEventBeanGet(ICodegenExpression beanExpression,
            ICodegenContext context)
        {
            return CodegenUnderlyingGet(CastUnderlying(TargetType, beanExpression), context);
        }

        public override ICodegenExpression CodegenEventBeanExists(ICodegenExpression beanExpression,
            ICodegenContext context)
        {
            return CodegenUnderlyingExists(CastUnderlying(TargetType, beanExpression), context);
        }

        public override ICodegenExpression CodegenUnderlyingGet(ICodegenExpression underlyingExpression,
            ICodegenContext context)
        {
            return LocalMethod(GetBeanPropInternalCode(context, _fastMethod.Target, _index),
                underlyingExpression);
        }

        public override ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression,
            ICodegenContext context)
        {
            return ConstantTrue();
        }

        public object Get(EventBean eventBean, int index)
        {
            return GetBeanPropInternal(eventBean.Underlying, index);
        }

        public override string ToString()
        {
            return "ArrayFastPropertyGetter " +
                   " fastMethod=" + _fastMethod +
                   " index=" + _index;
        }

        private object GetBeanPropInternal(object @object, int index)
        {
            try
            {
                var value = (Array) _fastMethod.Invoke(@object, null);
                if (value.Length <= index) return null;
                return value.GetValue(index);
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

        internal static string GetBeanPropInternalCode(ICodegenContext context, MethodInfo method, int index)
        {
            return context.AddMethod(method.ReturnType.GetElementType().GetBoxedType(), method.DeclaringType, "obj",
                    typeof(ArrayFastPropertyGetter))
                .DeclareVar(method.ReturnType, "array", ExprDotMethod(Ref("obj"), method.Name))
                .IfConditionReturnConst(
                    Relational(ArrayLength(Ref("array")),
                        CodegenRelational.LE, Constant(index)), null)
                .MethodReturn(ArrayAtIndex(Ref("array"), Constant(index)));
        }
    }
} // end of namespace