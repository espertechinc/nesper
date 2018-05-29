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
    using Map = IDictionary<object, object>;

    /// <summary>
    /// Getter for a key property identified by a given key value of a map, using the CGLIB fast method.
    /// </summary>
    public class KeyedMapFastPropertyGetter
        : BaseNativePropertyGetter
        , BeanEventPropertyGetter
        , EventPropertyGetterAndMapped
    {
        private readonly FastMethod _fastMethod;
        private readonly Object _key;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="method">the underlying method</param>
        /// <param name="fastMethod">is the method to use to retrieve a value from the object.</param>
        /// <param name="key">is the key to supply as parameter to the mapped property getter</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        public KeyedMapFastPropertyGetter(MethodInfo method, FastMethod fastMethod, Object key, EventAdapterService eventAdapterService)
            : base(eventAdapterService, TypeHelper.GetGenericReturnTypeMap(method, false), null)
        {
            this._key = key;
            this._fastMethod = fastMethod;
        }

        public bool IsBeanExistsProperty(Object @object)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public Object GetBeanProp(Object @object)
        {
            return GetBeanPropInternal(@object, _key);
        }

        public Object Get(EventBean eventBean, string mapKey)
        {
            return GetBeanPropInternal(eventBean.Underlying, mapKey);
        }

        public Object GetBeanPropInternal(Object @object, Object key)
        {
            try {
                var result = _fastMethod.Invoke(@object, null);
                if (result == null) {
                    return null;
                }

                if (result is Map) {
                    return ((Map) result).Get(key);
                }

                if (result.GetType().IsGenericDictionary()) {
                    return MagicMarker
                        .GetDictionaryFactory(result.GetType())
                        .Invoke(result)
                        .Get(key);
                }

                return null;
            }
            catch (PropertyAccessException) {
                throw;
            }
            catch (InvalidCastException e) {
                throw PropertyUtility.GetMismatchException(_fastMethod.Target, @object, e);
            }
            catch (TargetInvocationException e) {
                throw PropertyUtility.GetInvocationTargetException(_fastMethod.Target, e);
            }
            catch (Exception e) {
                throw PropertyUtility.GetAccessExceptionMethod(_fastMethod.Target, e);
            }
        }

        public override Object Get(EventBean obj)
        {
            Object underlying = obj.Underlying;
            return GetBeanProp(underlying);
        }

        public override String ToString()
        {
            return "KeyedMapFastPropertyGetter " +
                    " fastMethod=" + _fastMethod.ToString() +
                    " key=" + _key;
        }

        public override bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public override Type BeanPropType => TypeHelper.GetGenericReturnTypeMap(_fastMethod.Target, false);

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
                KeyedMapMethodPropertyGetter.GetBeanPropInternalCodegen(
                    context, BeanPropType, TargetType, _fastMethod.Target, _key), underlyingExpression);
        }

        public override ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return ConstantTrue();
        }
    }
} // end of namespace