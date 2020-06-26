///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.util.CollectionUtil;

namespace com.espertech.esper.common.@internal.@event.bean.getter
{
	/// <summary>
	/// Getter for a dynamic mapped property (syntax field.mapped('key')?), using vanilla reflection.
	/// </summary>
	public class DynamicMappedPropertyGetterByMethod : DynamicPropertyGetterByMethodBase
	{
		private readonly string _getterMethodName;
		private readonly object[] _parameters;

		public DynamicMappedPropertyGetterByMethod(
			string fieldName,
			string key,
			EventBeanTypedEventFactory eventBeanTypedEventFactory,
			BeanEventTypeFactory beanEventTypeFactory)
			: base(eventBeanTypedEventFactory, beanEventTypeFactory)
		{
			_getterMethodName = PropertyHelper.GetGetterMethodName(fieldName);
			_parameters = new object[] {key};
		}

		protected override MethodInfo DetermineMethod(Type clazz)
		{
			return DynamicMapperPropertyDetermineMethod(clazz, _getterMethodName);
		}

		protected override CodegenExpression DetermineMethodCodegen(
			CodegenExpressionRef clazz,
			CodegenMethodScope parent,
			CodegenClassScope codegenClassScope)
		{
			return StaticMethod(typeof(DynamicMappedPropertyGetterByMethod), "DynamicMapperPropertyDetermineMethod", clazz, Constant(_getterMethodName));
		}

		protected override object Call(
			DynamicPropertyDescriptorByMethod descriptor,
			object underlying)
		{
			return DynamicMappedPropertyGet(descriptor, underlying, _parameters);
		}

		protected override CodegenExpression CallCodegen(CodegenExpressionRef desc,
			CodegenExpressionRef @object,
			CodegenMethodScope parent,
			CodegenClassScope codegenClassScope)
		{
			var @params = codegenClassScope.AddDefaultFieldUnshared(true, typeof(object[]), Constant(_parameters));
			return StaticMethod(typeof(DynamicMappedPropertyGetterByMethod), "DynamicMappedPropertyGet", desc, @object, @params);
		}

		public CodegenExpression UnderlyingExistsCodegen(
			CodegenExpression underlyingExpression,
			CodegenMethodScope parent,
			CodegenClassScope codegenClassScope)
		{
			CodegenExpression memberCache = codegenClassScope.AddOrGetDefaultFieldSharable(SharableCode);
			var method = parent.MakeChild(typeof(bool), typeof(DynamicPropertyGetterByMethodBase), codegenClassScope)
				.AddParam(typeof(object), "object");
			method.Block
				.DeclareVar(typeof(DynamicPropertyDescriptorByMethod), "desc", GetPopulateCacheCodegen(memberCache, Ref("object"), method, codegenClassScope))
				.IfCondition(EqualsNull(ExprDotName(Ref("desc"), "Method")))
				.BlockReturn(ConstantFalse())
				.MethodReturn(
					StaticMethod(
						typeof(DynamicMappedPropertyGetterByMethod),
						"DynamicMappedPropertyExists",
						Ref("desc"),
						Ref("object"),
						Constant(_parameters[0])));
			return LocalMethod(method, underlyingExpression);

		}

		public bool IsExistsProperty(EventBean eventBean)
		{
			var desc = GetPopulateCache(Cache, this, eventBean.Underlying, EventBeanTypedEventFactory);
			if (desc.Method == null) {
				return false;
			}

			return DynamicMappedPropertyExists(desc, eventBean.Underlying, (string) _parameters[0]);
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="clazz">class</param>
		/// <param name="getterMethodName">method</param>
		/// <returns>value</returns>
		/// <throws>PropertyAccessException for access ex</throws>
		public static MethodInfo DynamicMapperPropertyDetermineMethod(
			Type clazz,
			string getterMethodName)
		{
			try {
				return clazz.GetMethod(getterMethodName, typeof(string));
			}
			catch (NoSuchMethodException ex1) {
				MethodInfo method;
				try {
					method = clazz.GetMethod(getterMethodName);
				}
				catch (NoSuchMethodException e) {
					return null;
				}

				if (method.ReturnType != typeof(IDictionary<string, object>)) {
					return null;
				}

				return method;
			}
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="descriptor">descriptor</param>
		/// <param name="underlying">target</param>
		/// <param name="parameters">params</param>
		/// <returns>value</returns>
		public static object DynamicMappedPropertyGet(
			DynamicPropertyDescriptorByMethod descriptor,
			object underlying,
			object[] parameters)
		{
			try {
				if (descriptor.HasParameters) {
					return descriptor.Method.Invoke(underlying, parameters);
				}
				else {
					var result = descriptor.Method.Invoke(underlying, null);
					return GetMapValueChecked(result, parameters[0]);
				}
			}
			catch (InvalidCastException e) {
				throw PropertyUtility.GetMismatchException(descriptor.Method, underlying, e);
			}
			catch (TargetInvocationException e) {
				throw PropertyUtility.GetTargetException(descriptor.Method, e);
			}
			catch (TargetException e) {
				throw PropertyUtility.GetTargetException(descriptor.Method, e);
			}
			catch (ArgumentException e) {
				throw PropertyUtility.GetArgumentException(descriptor.Method, e);
			}
			catch (MemberAccessException e) {
				throw PropertyUtility.GetMemberAccessException(descriptor.Method, e);
			}
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="descriptor">descriptor</param>
		/// <param name="underlying">target</param>
		/// <param name="key">key</param>
		/// <returns>value</returns>
		public static bool DynamicMappedPropertyExists(
			DynamicPropertyDescriptorByMethod descriptor,
			object underlying,
			string key)
		{
			try {
				if (descriptor.HasParameters) {
					return true;
				}
				else {
					var result = descriptor.Method.Invoke(underlying, null);
					return GetMapKeyExistsChecked(result, key);
				}
			}
			catch (InvalidCastException e) {
				throw PropertyUtility.GetMismatchException(descriptor.Method, underlying, e);
			}
			catch (TargetInvocationException e) {
				throw PropertyUtility.GetTargetException(descriptor.Method, e);
			}
			catch (TargetException e) {
				throw PropertyUtility.GetTargetException(descriptor.Method, e);
			}
			catch (ArgumentException e) {
				throw PropertyUtility.GetArgumentException(descriptor.Method, e);
			}
			catch (MemberAccessException e) {
				throw PropertyUtility.GetMemberAccessException(descriptor.Method, e);
			}
		}
	}
} // end of namespace
