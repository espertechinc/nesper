using System;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.codegen.core;
using com.espertech.esper.codegen.model.expression;
using com.espertech.esper.events.vaevent;

//import static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder.castUnderlying;
//import static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder.constantTrue;
//import static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder.localMethod;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events.bean
{
    /// <summary>
    ///     Getter for an array property identified by a given index, using vanilla reflection.
    /// </summary>
    public class ArrayMethodPropertyGetter : BaseNativePropertyGetter
        , BeanEventPropertyGetter
        , EventPropertyGetterAndIndexed
    {
        private readonly int _index;
        private readonly MethodInfo _method;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="method">is the method to use to retrieve a value from the object</param>
        /// <param name="index">is tge index within the array to get the property from</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        public ArrayMethodPropertyGetter(MethodInfo method, int index, EventAdapterService eventAdapterService)
            : base(eventAdapterService, method.ReturnType.GetElementType(), null)
        {
            _index = index;
            _method = method;

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
            var underlying = obj.Underlying;
            return GetBeanProp(underlying);
        }

        public override bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public override Type BeanPropType => _method.ReturnType.GetElementType();

        public override Type TargetType => _method.DeclaringType;

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
            return LocalMethod(
                ArrayFastPropertyGetter.GetBeanPropInternalCode(context, _method, _index), underlyingExpression);
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

        private object GetBeanPropInternal(object @object, int index)
        {
            try {
                var value = _method.Invoke(@object, null) as Array;
                if (value == null)
                    return null;
                if (value.Length <= index)
                    return null;
                return value.GetValue(index);
            }
            catch (InvalidCastException e) {
                throw PropertyUtility.GetMismatchException(_method, @object, e);
            }
            catch (TargetInvocationException e) {
                throw PropertyUtility.GetInvocationTargetException(_method, e);
            }
            catch (TargetException e) {
                throw PropertyUtility.GetGenericException(_method, e);
            }
            catch (MemberAccessException e) {
                throw PropertyUtility.GetIllegalAccessException(_method, e);
            }
            catch (ArgumentException e) {
                throw PropertyUtility.GetIllegalArgumentException(_method, e);
            }
        }

        public override string ToString()
        {
            return "ArrayMethodPropertyGetter " +
                   " method=" + _method +
                   " index=" + _index;
        }
    }
} // end of namespace