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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.util.CollectionUtil;

namespace com.espertech.esper.common.@internal.@event.bean.getter
{
	/// <summary>
	/// Getter for a dynamic mapped property (syntax field.mapped('key')?), using vanilla reflection.
	/// </summary>
	public class DynamicMappedPropertyGetterByField : DynamicPropertyGetterByFieldBase
	{
		private readonly string _fieldName;
		private readonly string _key;

		public DynamicMappedPropertyGetterByField(
			string fieldName,
			string key,
			EventBeanTypedEventFactory eventBeanTypedEventFactory,
			BeanEventTypeFactory beanEventTypeFactory)
			: base(eventBeanTypedEventFactory, beanEventTypeFactory)
		{
			_fieldName = fieldName;
			_key = key;
		}

		protected override MemberInfo DetermineFieldOrProperty(Type clazz)
		{
			return DynamicMapperPropertyDetermineField(clazz, _fieldName);
		}

		protected override CodegenExpression DetermineFieldOrPropertyCodegen(
			CodegenExpressionRef clazz,
			CodegenMethodScope parent,
			CodegenClassScope codegenClassScope)
		{
			return StaticMethod(typeof(DynamicMappedPropertyGetterByField), "DynamicMapperPropertyDetermineField", clazz, Constant(_fieldName));
		}

		protected override object Call(
			DynamicPropertyDescriptorByField descriptor,
			object underlying)
		{
			return DynamicMappedPropertyGet(descriptor, underlying, _key);
		}

		protected override CodegenExpression CallCodegen(
			CodegenExpressionRef desc,
			CodegenExpressionRef @object,
			CodegenMethodScope parent,
			CodegenClassScope codegenClassScope)
		{
			return StaticMethod(typeof(DynamicMappedPropertyGetterByField), "DynamicMappedPropertyGet", desc, @object, Constant(_key));
		}

		public override CodegenExpression UnderlyingExistsCodegen(
			CodegenExpression underlyingExpression,
			CodegenMethodScope parent,
			CodegenClassScope codegenClassScope)
		{
			CodegenExpression memberCache = codegenClassScope.AddOrGetDefaultFieldSharable(SharableCode);
			var method = parent.MakeChild(typeof(bool), typeof(DynamicPropertyGetterByMethodBase), codegenClassScope)
				.AddParam(typeof(object), "object");
			method.Block
				.DeclareVar(typeof(DynamicPropertyDescriptorByField), "desc", GetPopulateCacheCodegen(memberCache, Ref("object"), method, codegenClassScope))
				.IfCondition(EqualsNull(ExprDotName(Ref("desc"), "Field")))
				.BlockReturn(ConstantFalse())
				.MethodReturn(
					StaticMethod(typeof(DynamicMappedPropertyGetterByField), "DynamicMappedPropertyExists", Ref("desc"), Ref("object"), Constant(_key)));
			return LocalMethod(method, underlyingExpression);

		}

		public override bool IsExistsProperty(EventBean eventBean)
		{
			var desc = GetPopulateCache(Cache, this, eventBean.Underlying, EventBeanTypedEventFactory);
			if (desc.Field == null) {
				return false;
			}

			return DynamicMappedPropertyExists(desc, eventBean.Underlying, _key);
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="clazz">class</param>
		/// <param name="fieldName">method</param>
		/// <returns>value</returns>
		/// <throws>PropertyAccessException for access ex</throws>
		public static FieldInfo DynamicMapperPropertyDetermineField(
			Type clazz,
			string fieldName)
		{
			var field = clazz.GetField(fieldName);
			if (field == null) {
				return null;
			}

			if (field.FieldType != typeof(IDictionary<string, object>)) {
				return null;
			}

			return field;
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="descriptor">descriptor</param>
		/// <param name="underlying">target</param>
		/// <param name="key">key</param>
		/// <returns>value</returns>
		public static object DynamicMappedPropertyGet(
			DynamicPropertyDescriptorByField descriptor,
			object underlying,
			string key)
		{
			try {
				var result = descriptor.Field.GetValue(underlying);
				return GetMapValueChecked(result, key);
			}
			catch (InvalidCastException e) {
				throw PropertyUtility.GetMismatchException(descriptor.Field, underlying, e);
			}
			catch (ArgumentException e) {
				throw PropertyUtility.GetArgumentException(descriptor.Field, e);
			}
			catch (MemberAccessException e) {
				throw PropertyUtility.GetMemberAccessException(descriptor.Field, e);
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
			DynamicPropertyDescriptorByField descriptor,
			object underlying,
			string key)
		{
			try {
				var result = descriptor.Field.GetValue(underlying);
				return GetMapKeyExistsChecked(result, key);
			}
			catch (InvalidCastException e) {
				throw PropertyUtility.GetMismatchException(descriptor.Field, underlying, e);
			}
			catch (ArgumentException e) {
				throw PropertyUtility.GetArgumentException(descriptor.Field, e);
			}
			catch (MemberAccessException e) {
				throw PropertyUtility.GetMemberAccessException(descriptor.Field, e);
			}
		}
	}
} // end of namespace
