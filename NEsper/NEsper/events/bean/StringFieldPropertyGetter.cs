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
using com.espertech.esper.client.soda;
using com.espertech.esper.codegen.core;
using com.espertech.esper.codegen.model.expression;
using com.espertech.esper.events.vaevent;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events.bean
{
    /// <summary>
    /// Getter for a string property backed by a field, identified by a given index, using
    /// vanilla reflection.
    /// </summary>
    public class StringFieldPropertyGetter
        : BaseNativePropertyGetter
            , BeanEventPropertyGetter
            , EventPropertyGetterAndIndexed
    {
        private readonly FieldInfo _field;
        private readonly int _index;

        /// <summary>Constructor. </summary>
        /// <param name="field">is the field to use to retrieve a value from the object</param>
        /// <param name="index">is tge index within the array to get the property from</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        public StringFieldPropertyGetter(FieldInfo field, int index, EventAdapterService eventAdapterService)
            : base(eventAdapterService, typeof(char), null)
        {
            _index = index;
            _field = field;

            if (index < 0)
            {
                throw new ArgumentException("Invalid negative index value");
            }
        }

        public static object GetValueAtIndex(object value, int index)
        {
            if (value is string stringValue && stringValue.Length > index)
            {
                return stringValue[index];
            }

            return null;
        }

        public Object GetBeanProp(Object @object)
        {
            return GetBeanPropInternal(@object, _index);
        }

        private Object GetBeanPropInternal(Object @object, int index)
        {
            return GetValueAtIndex(_field.GetValue(@object), index);
        }

        public bool IsBeanExistsProperty(Object @object)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public override Object Get(EventBean eventBean)
        {
            return GetBeanProp(eventBean.Underlying);
        }

        public Object Get(EventBean eventBean, int index)
        {
            return GetBeanPropInternal(eventBean.Underlying, index);
        }

        public override String ToString()
        {
            return "StringFieldPropertyGetter " +
                   " field=" + _field +
                   " index=" + _index;
        }

        public override bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        private string GetBeanPropInternalCodegen(ICodegenContext context)
        {
            return context
                .AddMethod(BeanPropType, TargetType, "object", GetType())
                .DeclareVar(typeof(object), "value",
                    ExprDotName(Ref("object"), _field.Name))
                .IfRefNotTypeReturnConst("value", typeof(string), null)
                .DeclareVar(typeof(string), "v",
                    Cast(typeof(string), Ref("value")))
                .MethodReturn(Cast(BeanPropType,
                    StaticMethod(GetType(), "GetValueAtIndex",
                        Ref("v"),
                        Constant(_index))));
        }

        public override Type BeanPropType => typeof(char);
        public override Type TargetType => typeof(string);

        public override ICodegenExpression CodegenEventBeanGet(ICodegenExpression beanExpression,
            ICodegenContext context)
        {
            return CodegenUnderlyingGet(
                CastUnderlying(TargetType, beanExpression), context);
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
    }
}
