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
using com.espertech.esper.events.vaevent;
using com.espertech.esper.util;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;
//import static com.espertech.esper.codegen.model.expression.CodegenExpressionRelational.CodegenRelational.LE;

namespace com.espertech.esper.events.bean
{
    /// <summary>
    ///     Getter for a list property backed by a field, identified by a given index, using vanilla reflection.
    /// </summary>
    public class ListFieldPropertyGetter
        : BaseNativePropertyGetter
            , BeanEventPropertyGetter
            , EventPropertyGetterAndIndexed
    {
        private readonly FieldInfo _field;
        private readonly int _index;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="field">is the field to use to retrieve a value from the object</param>
        /// <param name="index">is tge index within the array to get the property from</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        public ListFieldPropertyGetter(FieldInfo field, int index, EventAdapterService eventAdapterService)
            : base(eventAdapterService, TypeHelper.GetGenericFieldType(field, false), null)
        {
            _index = index;
            _field = field;

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

        public override Type BeanPropType => TypeHelper.GetGenericFieldType(_field, false);

        public override Type TargetType => _field.DeclaringType;

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
            return LocalMethod(GetBeanPropInternalCodegen(context), underlyingExpression);
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
            try
            {
                var value = _field.GetValue(@object);
                if (!(value is IList<object>)) return null;
                var valueList = (IList<object>) value;
                if (valueList.Count <= index) return null;
                return valueList[index];
            }
            catch (InvalidCastException e)
            {
                throw PropertyUtility.GetMismatchException(_field, @object, e);
            }
            catch (MemberAccessException e)
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
            return context.AddMethod(BeanPropType, TargetType, "object", GetType())
                .DeclareVar(typeof(object), "value",
                    ExprDotName(Ref("object"), _field.Name))
                .IfRefNotTypeReturnConst("value", typeof(IList<object>), null)
                .DeclareVar(typeof(IList<object>), "l",
                    Cast(typeof(IList<object>), Ref("value")))
                .IfConditionReturnConst(
                    Relational(ExprDotMethod(Ref("l"), "size"),
                        CodegenRelational.LE,
                        Constant(_index)), null)
                .MethodReturn(Cast(BeanPropType,
                    ExprDotMethod(Ref("l"), "get", Constant(_index))));
        }

        public override string ToString()
        {
            return "ListFieldPropertyGetter " +
                   " field=" + _field +
                   " index=" + _index;
        }
    }
} // end of namespace