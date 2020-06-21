///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.json.getter.core
{
    public abstract class JsonGetterIndexedBase : JsonEventPropertyGetter
    {
        public EventBeanTypedEventFactory EventBeanTypedEventFactory { get; }
        public int Index { get; }
        public EventType OptionalInnerType { get; }
        public string UnderlyingClassName { get; }

        public JsonGetterIndexedBase(
            int index,
            string underlyingClassName,
            EventType optionalInnerType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            this.Index = index;
            this.UnderlyingClassName = underlyingClassName;
            this.OptionalInnerType = optionalInnerType;
            this.EventBeanTypedEventFactory = eventBeanTypedEventFactory;
        }

        public abstract CodegenExpression UnderlyingFragmentCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope);

        public abstract object GetJsonProp(object @object);
        public abstract bool GetJsonExists(object @object);
        public abstract object GetJsonFragment(object @object);

        public CodegenExpression EventBeanGetCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingGetCodegen(CastUnderlying(UnderlyingClassName, beanExpression), codegenMethodScope, codegenClassScope);
        }

        public CodegenExpression UnderlyingGetCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return StaticMethod(typeof(CollectionUtil), "ArrayValueAtIndex", ExprDotName(underlyingExpression, FieldName), Constant(Index));
        }

        public CodegenExpression EventBeanExistsCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingExistsCodegen(CastUnderlying(UnderlyingClassName, beanExpression), codegenMethodScope, codegenClassScope);
        }

        public CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return StaticMethod(typeof(CollectionUtil), "ArrayExistsAtIndex", ExprDotName(underlyingExpression, FieldName), Constant(Index));
        }

        public CodegenExpression EventBeanFragmentCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingFragmentCodegen(CastUnderlying(UnderlyingClassName, beanExpression), codegenMethodScope, codegenClassScope);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return GetJsonExists(eventBean.Underlying);
        }

        public object GetFragment(EventBean eventBean)
        {
            return GetJsonFragment(eventBean.Underlying);
        }

        public object Get(EventBean eventBean)
        {
            return GetJsonProp(eventBean.Underlying);
        }

        public abstract string FieldName { get; }
    }
} // end of namespace