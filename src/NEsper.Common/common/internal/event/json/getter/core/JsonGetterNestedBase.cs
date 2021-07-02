///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.json.getter.core
{
	public abstract class JsonGetterNestedBase : JsonEventPropertyGetter
	{
		public JsonEventPropertyGetter InnerGetter { get; }
		public string UnderlyingClassName { get; }

		public abstract string FieldName { get; }
		public abstract Type FieldType { get; }

		public JsonGetterNestedBase(
			JsonEventPropertyGetter innerGetter,
			string underlyingClassName)
		{
			InnerGetter = innerGetter;
			UnderlyingClassName = underlyingClassName;
		}

		public abstract object GetJsonProp(object @object);
		public abstract bool GetJsonExists(object @object);
		public abstract object GetJsonFragment(object @object);

		public object Get(EventBean eventBean)
		{
			return GetJsonProp(eventBean.Underlying);
		}

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
			CodegenMethod method = codegenMethodScope
				.MakeChild(typeof(object), GetType(), codegenClassScope)
				.AddParam(UnderlyingClassName, "und");
			method.Block
				.DeclareVar(FieldType, "inner", ExprDotName(Ref("und"), FieldName))
				.IfRefNullReturnNull("inner")
				.MethodReturn(InnerGetter.UnderlyingGetCodegen(Ref("inner"), method, codegenClassScope));
			return LocalMethod(method, underlyingExpression);
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
			CodegenMethod method = codegenMethodScope.MakeChild(typeof(bool), GetType(), codegenClassScope)
				.AddParam(UnderlyingClassName, "und");
			method.Block
				.DeclareVar(FieldType, "inner", ExprDotName(Ref("und"), FieldName))
				.IfRefNull("inner")
				.BlockReturn(ConstantFalse())
				.MethodReturn(InnerGetter.UnderlyingExistsCodegen(Ref("inner"), method, codegenClassScope));
			return LocalMethod(method, underlyingExpression);
		}

		public CodegenExpression EventBeanFragmentCodegen(
			CodegenExpression beanExpression,
			CodegenMethodScope codegenMethodScope,
			CodegenClassScope codegenClassScope)
		{
			return UnderlyingFragmentCodegen(CastUnderlying(UnderlyingClassName, beanExpression), codegenMethodScope, codegenClassScope);
		}

		public CodegenExpression UnderlyingFragmentCodegen(
			CodegenExpression underlyingExpression,
			CodegenMethodScope codegenMethodScope,
			CodegenClassScope codegenClassScope)
		{
			CodegenMethod method = codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope)
				.AddParam(UnderlyingClassName, "und");
			method.Block
				.DeclareVar(FieldType, "inner", ExprDotName(Ref("und"), FieldName))
				.IfRefNull("inner")
				.BlockReturn(ConstantNull())
				.MethodReturn(InnerGetter.UnderlyingFragmentCodegen(Ref("inner"), method, codegenClassScope));
			return LocalMethod(method, underlyingExpression);
		}

		public bool IsExistsProperty(EventBean eventBean)
		{
			return GetJsonExists(eventBean.Underlying);
		}

		public object GetFragment(EventBean eventBean)
		{
			return GetJsonFragment(eventBean.Underlying);
		}
	}
} // end of namespace
