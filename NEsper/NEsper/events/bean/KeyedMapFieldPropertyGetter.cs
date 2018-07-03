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
    public class KeyedMapFieldPropertyGetter
        : BaseNativePropertyGetter
        , BeanEventPropertyGetter
        , EventPropertyGetterAndMapped
    {
        private readonly FieldInfo _field;
        private readonly Object _key;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="field">is the field to use to retrieve a value from the object.</param>
        /// <param name="key">is the key to supply as parameter to the mapped property getter</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        public KeyedMapFieldPropertyGetter(FieldInfo field, Object key, EventAdapterService eventAdapterService)
            : base(eventAdapterService, TypeHelper.GetGenericFieldTypeMap(field, false), null)
        {
            this._key = key;
            this._field = field;
        }

        public Object Get(EventBean eventBean, string mapKey)
        {
            return GetBeanPropInternal(eventBean.Underlying, mapKey);
        }

        public Object GetBeanProp(Object @object)
        {
            return GetBeanPropInternal(@object, _key);
        }

        public Object GetBeanPropInternal(Object @object, Object key)
        {
            try {
                var result = _field.GetValue(@object);
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
                throw PropertyUtility.GetMismatchException(_field, @object, e);
            }
            catch (ArgumentException e) {
                throw PropertyUtility.GetIllegalArgumentException(_field, e);
            }
            catch (Exception e) {
                throw PropertyUtility.GetAccessExceptionField(_field, e);
            }
        }

        private string GetBeanPropInternalCodegen(ICodegenContext context)
        {
            return context.AddMethod(BeanPropType, TargetType, "object", this.GetType())
                .DeclareVar(typeof(Object), "result", ExprDotName(Ref("object"), _field.Name))
                .IfRefNotTypeReturnConst("result", typeof(Map), null)
                .DeclareVarWCast(typeof(Map), "map", "result")
                .MethodReturn(Cast(
                    BeanPropType, ExprDotMethod(
                        Ref("map"), "get", Constant(_key))));
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
            return "KeyedMapFieldPropertyGetter " +
                    " field=" + _field.ToString() +
                    " key=" + _key;
        }

        public override bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public override Type BeanPropType => TypeHelper.GetGenericFieldTypeMap(_field, false);

        public override Type TargetType => _field.DeclaringType;

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
            return LocalMethod(GetBeanPropInternalCodegen(context), underlyingExpression);
        }

        public override ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return ConstantTrue();
        }
    }
} // end of namespace