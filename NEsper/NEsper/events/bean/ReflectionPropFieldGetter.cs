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

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events.bean
{
    /// <summary>Property getter for fields using Java's vanilla reflection.</summary>
    public sealed class ReflectionPropFieldGetter
        : BaseNativePropertyGetter
        , BeanEventPropertyGetter
    {
        private readonly FieldInfo _field;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="field">is the regular reflection field to use to obtain values for a property</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        public ReflectionPropFieldGetter(FieldInfo field, EventAdapterService eventAdapterService)
            : base(eventAdapterService, field.FieldType, TypeHelper.GetGenericFieldType(field, true))
        {
            this._field = field;
        }

        public Object GetBeanProp(Object @object)
        {
            try
            {
                return _field.GetValue(@object);
            }
            catch (ArgumentException e)
            {
                throw PropertyUtility.GetIllegalArgumentException(_field, e);
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
            return "ReflectionPropFieldGetter " +
                    "field=" + _field.ToString();
        }

        public override bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public override Type BeanPropType => _field.FieldType;

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
            return ExprDotName(underlyingExpression, _field.Name);
        }

        public override ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return ConstantTrue();
        }
    }
} // end of namespace