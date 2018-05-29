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

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events.bean
{
    using Map = IDictionary<object, object>;

    /// <summary>
    /// Getter for a key property identified by a given key value, using vanilla reflection.
    /// </summary>
    public class KeyedMapMethodPropertyGetter
        : BaseNativePropertyGetter
        , BeanEventPropertyGetter
        , EventPropertyGetterAndMapped
    {
        private readonly MethodInfo _method;
        private readonly Object _key;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="method">is the method to use to retrieve a value from the object.</param>
        /// <param name="key">is the key to supply as parameter to the mapped property getter</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        public KeyedMapMethodPropertyGetter(MethodInfo method, Object key, EventAdapterService eventAdapterService)
            : base(eventAdapterService, TypeHelper.GetGenericReturnTypeMap(method, false), null)
        {
            this._key = key;
            this._method = method;
        }

        public Object Get(EventBean eventBean, string mapKey)
        {
            return GetBeanPropInternal(eventBean.Underlying, mapKey);
        }

        public Object GetBeanProp(Object @object)
        {
            return GetBeanPropInternal(@object, _key);
        }

        public object GetBeanPropInternal(Object @object, Object key)
        {
            try {
                var result = _method.Invoke(@object, (Object[]) null);
                if (result == null) {
                    return null;
                }

                if (result is Map) {
                    return ((Map) result).Get(key);
                }

                var resultType = result.GetType();
                if (resultType.IsGenericDictionary()) {
                    return MagicMarker
                        .GetDictionaryFactory(resultType)
                        .Invoke(result)
                        .Get(key);
                }

                return null;
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

        internal static string GetBeanPropInternalCodegen(ICodegenContext context, Type beanPropType, Type targetType, MethodInfo method, Object key)
        {
            return context.AddMethod(beanPropType, targetType, "object", typeof(KeyedMapMethodPropertyGetter))
                .DeclareVar(method.ReturnType, "result", ExprDotMethod(Ref("object"), method.Name))
                .IfRefNotTypeReturnConst("result", typeof(Map), null)
                .MethodReturn(Cast(
                    beanPropType, ExprDotMethod(
                        Cast(typeof(Map), Ref("result")),
                        "get",
                        Constant(key))));
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
            return "KeyedMapMethodPropertyGetter " +
                    " method=" + _method.ToString() +
                    " key=" + _key;
        }

        public override bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public override Type BeanPropType => TypeHelper.GetGenericReturnTypeMap(_method, false);

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
            return LocalMethod(GetBeanPropInternalCodegen(context, BeanPropType, TargetType, _method, _key), underlyingExpression);
        }

        public override ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return ConstantTrue();
        }
    }
} // end of namespace