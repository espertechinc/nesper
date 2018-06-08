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
    /// Getter for a key property identified by a given key value, using vanilla reflection.
    /// </summary>
    public class KeyedMethodPropertyGetter
        : BaseNativePropertyGetter
        , BeanEventPropertyGetter
        , EventPropertyGetterAndMapped
        , EventPropertyGetterAndIndexed
    {
        private readonly MethodInfo _method;
        private readonly Object _key;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="method">is the method to use to retrieve a value from the object.</param>
        /// <param name="key">is the key to supply as parameter to the mapped property getter</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        public KeyedMethodPropertyGetter(MethodInfo method, Object key, EventAdapterService eventAdapterService)
            : base(eventAdapterService, method.ReturnType, null)
        {
            this._key = key;
            this._method = method;
        }

        public Object Get(EventBean eventBean, int index)
        {
            return GetBeanPropInternal(eventBean.Underlying, index);
        }

        public Object Get(EventBean eventBean, string mapKey)
        {
            return GetBeanPropInternal(eventBean.Underlying, mapKey);
        }

        public Object GetBeanProp(Object @object)
        {
            return GetBeanPropInternal(@object, _key);
        }

        private Object GetBeanPropInternal(Object @object, Object key)
        {
            try {
                return _method.Invoke(@object, new object[] {key});
            }
            catch (PropertyAccessException) {
                throw;
            }
            catch (InvalidCastException e) {
                throw PropertyUtility.GetMismatchException(_method, @object, e);
            }
            catch (TargetInvocationException e) {
                throw PropertyUtility.GetInvocationTargetException(_method, e);
            }
            catch (ArgumentException e) {
                throw PropertyUtility.GetIllegalArgumentException(_method, e);
            }
            catch (Exception e) {
                throw PropertyUtility.GetAccessExceptionMethod(_method, e);
            }
        }

        public bool IsBeanExistsProperty(Object @object)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public override Object Get(EventBean obj)
        {
            Object underlying = obj.Underlying;
            return GetBeanProp(underlying);
        }

        public override String ToString()
        {
            return "KeyedMethodPropertyGetter " +
                    " method=" + _method.ToString() +
                    " key=" + _key;
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
            return LocalMethod(KeyedFastPropertyGetter.GetBeanPropInternalCodegen(
                context, TargetType, _method, _key), underlyingExpression);
        }

        public override ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return ConstantTrue();
        }
    }
} // end of namespace