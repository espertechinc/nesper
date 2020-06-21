///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.bean.getter
{
	/// <summary>
	/// Getter for a dynamic indexed property (syntax field.indexed[0]?), using vanilla reflection.
	/// </summary>
	public class DynamicIndexedPropertyGetterByMethod : DynamicPropertyGetterByMethodBase
	{
		private readonly string _getterMethodName;
		private readonly object[] _parameters;
		private readonly int _index;

		public DynamicIndexedPropertyGetterByMethod(
			string fieldName,
			int index,
			EventBeanTypedEventFactory eventBeanTypedEventFactory,
			BeanEventTypeFactory beanEventTypeFactory)
			: base(eventBeanTypedEventFactory, beanEventTypeFactory)
		{
			_getterMethodName = PropertyHelper.GetGetterMethodName(fieldName);
			_parameters = new object[] {index};
			_index = index;
		}

		protected override MethodInfo DetermineMethod(Type clazz)
		{
			return DynamicIndexPropertyDetermineMethod(clazz, _getterMethodName);
		}

		protected override CodegenExpression DetermineMethodCodegen(
			CodegenExpressionRef clazz,
			CodegenMethodScope parent,
			CodegenClassScope codegenClassScope)
		{
			return StaticMethod(typeof(DynamicIndexedPropertyGetterByMethod), "DynamicIndexPropertyDetermineMethod", clazz, Constant(_getterMethodName));
		}

		protected override object Call(
			DynamicPropertyDescriptorByMethod descriptor,
			object underlying)
		{
			return DynamicIndexedPropertyGet(descriptor, underlying, _parameters, _index);
		}

		protected override CodegenExpression CallCodegen(
			CodegenExpressionRef desc,
			CodegenExpressionRef @object,
			CodegenMethodScope parent,
			CodegenClassScope codegenClassScope)
		{
			var @params = codegenClassScope.AddDefaultFieldUnshared(true, typeof(object[]), Constant(_parameters));
			return StaticMethod(typeof(DynamicIndexedPropertyGetterByMethod), "DynamicIndexedPropertyGet", desc, @object, @params, Constant(_index));
		}

		public bool IsExistsProperty(EventBean eventBean)
		{
			var desc = GetPopulateCache(Cache, this, eventBean.Underlying, EventBeanTypedEventFactory);
			if (desc.Method == null) {
				return false;
			}

			return DynamicIndexedPropertyExists(desc, eventBean.Underlying, _index);
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
					StaticMethod(typeof(DynamicIndexedPropertyGetterByMethod), "DynamicIndexedPropertyExists", Ref("desc"), Ref("object"), Constant(_index)));
			return LocalMethod(method, underlyingExpression);
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="clazz">class</param>
		/// <param name="getterMethodName">method</param>
		/// <returns>null or method</returns>
		public static MethodInfo DynamicIndexPropertyDetermineMethod(
			Type clazz,
			string getterMethodName)
		{
			MethodInfo method;

			try {
				return clazz.GetMethod(getterMethodName, typeof(int));
			}
			catch (NoSuchMethodException ex1) {
				try {
					method = clazz.GetMethod(getterMethodName);
				}
				catch (NoSuchMethodException e) {
					return null;
				}

				if (!method.ReturnType.IsArray) {
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
		/// <param name="index">idx</param>
		/// <returns>null or method</returns>
		public static object DynamicIndexedPropertyGet(
			DynamicPropertyDescriptorByMethod descriptor,
			object underlying,
			object[] parameters,
			int index)
		{
			try {
				if (descriptor.HasParameters) {
					return descriptor.Method.Invoke(underlying, parameters);
				}
				else {
					var array = (Array) descriptor.Method.Invoke(underlying, null);
					if (array == null) {
						return null;
					}

					if (array.Length <= index) {
						return null;
					}

					return array.GetValue(index);
				}
			}
			catch (InvalidCastException e) {
				throw PropertyUtility.GetMismatchException(descriptor.Method, underlying, e);
			}
			catch (InvocationTargetException e) {
				throw PropertyUtility.GetInvocationTargetException(descriptor.Method, e);
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
		/// <param name="index">idx</param>
		/// <returns>null or method</returns>
		public static bool DynamicIndexedPropertyExists(
			DynamicPropertyDescriptorByMethod descriptor,
			object underlying,
			int index)
		{
			try {
				if (descriptor.HasParameters) {
					return true;
				}
				else {
					var array = (Array) descriptor.Method.Invoke(underlying, null);
					if (array == null) {
						return false;
					}

					if (array.Length <= index) {
						return false;
					}

					return true;
				}
			}
			catch (InvalidCastException e) {
				throw PropertyUtility.GetMismatchException(descriptor.Method, underlying, e);
			}
			catch (InvocationTargetException e) {
				throw PropertyUtility.GetInvocationTargetException(descriptor.Method, e);
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
