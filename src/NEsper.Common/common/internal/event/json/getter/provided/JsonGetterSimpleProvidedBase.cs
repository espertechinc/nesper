///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.json.getter.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.json.getter.provided
{
    /// <summary>
    ///     Property getter for Json underlying fields.
    /// </summary>
    public abstract class JsonGetterSimpleProvidedBase : JsonEventPropertyGetter
    {
        public JsonGetterSimpleProvidedBase(
            FieldInfo field,
            EventType fragmentType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            Field = field;
            FragmentType = fragmentType;
            EventBeanTypedEventFactory = eventBeanTypedEventFactory;
        }

        public EventBeanTypedEventFactory EventBeanTypedEventFactory { get; }
        public FieldInfo Field { get; }
        public EventType FragmentType { get; }

        public abstract CodegenExpression EventBeanFragmentCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope);

        public abstract CodegenExpression UnderlyingFragmentCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope);

        public abstract object GetJsonFragment(object @object);

        public object Get(EventBean eventBean)
        {
            return GetJsonProp(eventBean.Underlying);
        }

        public object GetJsonProp(object @object)
        {
            return JsonFieldGetterHelperProvided.GetJsonProvidedSimpleProp(@object, Field);
        }

        public CodegenExpression EventBeanGetCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingGetCodegen(
                CastUnderlying(Field.DeclaringType, beanExpression),
                codegenMethodScope,
                codegenClassScope);
        }

        public CodegenExpression UnderlyingGetCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ExprDotName(underlyingExpression, Field.Name);
        }

        public CodegenExpression EventBeanExistsCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ConstantTrue();
        }

        public CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ConstantTrue();
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true;
        }

        public object GetFragment(EventBean eventBean)
        {
            return GetJsonFragment(eventBean.Underlying);
        }

        public bool GetJsonExists(object @object)
        {
            return true;
        }
    }
} // end of namespace