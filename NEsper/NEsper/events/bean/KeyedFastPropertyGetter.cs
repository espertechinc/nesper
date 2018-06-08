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
using com.espertech.esper.codegen.core;
using com.espertech.esper.codegen.model.expression;
using com.espertech.esper.events.vaevent;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events.bean
{
    /// <summary>
    /// Getter for a key property identified by a given key value, using the CGLIB fast method.
    /// </summary>
    public class KeyedFastPropertyGetter
        : BaseNativePropertyGetter
        , BeanEventPropertyGetter
        , EventPropertyGetterAndMapped
        , EventPropertyGetterAndIndexed
    {
        private readonly FastMethod _fastMethod;
        private readonly Object _key;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="fastMethod">is the method to use to retrieve a value from the object.</param>
        /// <param name="key">is the key to supply as parameter to the mapped property getter</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        public KeyedFastPropertyGetter(FastMethod fastMethod, Object key, EventAdapterService eventAdapterService)
            : base(eventAdapterService, fastMethod.ReturnType, null)
        {
            this._key = key;
            this._fastMethod = fastMethod;
        }

        public bool IsBeanExistsProperty(Object @object)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public override Object Get(EventBean obj)
        {
            return GetBeanProp(obj.Underlying);
        }

        public Object GetBeanProp(Object @object)
        {
            return GetBeanPropInternal(@object, _key);
        }

        public Object Get(EventBean eventBean, string mapKey)
        {
            return GetBeanPropInternal(eventBean.Underlying, mapKey);
        }

        public Object Get(EventBean eventBean, int index)
        {
            return GetBeanPropInternal(eventBean.Underlying, index);
        }

        public Object GetBeanPropInternal(Object @object, Object key)
        {
            try {
                return _fastMethod.Invoke(@object, new Object[] {key});
            }
            catch (PropertyAccessException) {
                throw;
            }
            catch (InvalidCastException e) {
                throw PropertyUtility.GetMismatchException(_fastMethod.Target, @object, e);
            }
            catch (Exception e) {
                throw PropertyUtility.GetAccessExceptionMethod(_fastMethod.Target, e);
            }
        }

        internal static string GetBeanPropInternalCodegen(ICodegenContext context, Type targetType, MethodInfo method, Object key)
        {
            return context.AddMethod(method.ReturnType, targetType, "object", typeof(KeyedFastPropertyGetter))
                    .MethodReturn(ExprDotMethod(
                        Ref("object"), method.Name,
                        Constant(key)));
        }

        public override String ToString()
        {
            return "KeyedFastPropertyGetter " +
                    " fastMethod=" + _fastMethod.ToString() +
                    " key=" + _key;
        }

        public override bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public override Type BeanPropType => _fastMethod.ReturnType;

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
            return LocalMethod(GetBeanPropInternalCodegen(context, TargetType, _fastMethod.Target, _key), underlyingExpression);
        }

        public override ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return ConstantTrue();
        }
    }
} // end of namespace