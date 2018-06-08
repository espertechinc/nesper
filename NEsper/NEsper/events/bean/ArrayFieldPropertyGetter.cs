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
    /// Getter for an array property backed by a field, identified by a given index, using vanilla reflection.
    /// </summary>
    public class ArrayFieldPropertyGetter
        : BaseNativePropertyGetter
        , BeanEventPropertyGetter
        , EventPropertyGetterAndIndexed
    {
        private readonly FieldInfo _field;
        private readonly int _index;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="field">is the field to use to retrieve a value from the object</param>
        /// <param name="index">is tge index within the array to get the property from</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        public ArrayFieldPropertyGetter(FieldInfo field, int index, EventAdapterService eventAdapterService)
            : base(eventAdapterService, field.FieldType.GetElementType(), null)
        {
            this._index = index;
            this._field = field;

            if (index < 0)
            {
                throw new ArgumentException("Invalid negative index value");
            }
        }

        public Object GetBeanProp(Object @object)
        {
            return GetBeanPropInternal(@object, _index);
        }

        private Object GetBeanPropInternal(Object @object, int index)
        {
            try
            {
                var value = (Array)_field.GetValue(@object);
                if (value.Length <= index)
                {
                    return null;
                }
                return value.GetValue(index);
            }
            catch (InvalidCastException e)
            {
                throw PropertyUtility.GetMismatchException(_field, @object, e);
            }
            catch (MethodAccessException e)
            {
                throw PropertyUtility.GetIllegalAccessException(_field, e);
            }
            catch (ArgumentException e)
            {
                throw PropertyUtility.GetIllegalArgumentException(_field, e);
            }
        }

        private string GetBeanPropInternalCodegen(ICodegenContext context)
        {
            return context.AddMethod(BeanPropType, TargetType, "object", this.GetType())
                .DeclareVar(typeof(Object), "value", ExprDotName(Ref("object"), _field.Name))
                .IfConditionReturnConst(
                    Relational(StaticMethod(typeof(Array), "getLength", Ref("value")),
                        CodegenRelational.LE, Constant(_index)), null)
                .MethodReturn(Cast(BeanPropType,
                    StaticMethod(typeof(Array), "get", Ref("value"), Constant(_index))));
        }

        public bool IsBeanExistsProperty(Object @object)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public override Object Get(EventBean obj)
        {
            return GetBeanProp(obj.Underlying);
        }

        public Object Get(EventBean eventBean, int index)
        {
            return GetBeanPropInternal(eventBean.Underlying, index);
        }

        public override Type BeanPropType => _field.FieldType.GetElementType();

        public override String ToString()
        {
            return "ArrayFieldPropertyGetter " +
                    " field=" + _field +
                    " index=" + _index;
        }

        public override bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public override Type TargetType => _field.FieldType;

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
