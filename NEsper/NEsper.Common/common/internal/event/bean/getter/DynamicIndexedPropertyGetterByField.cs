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
	public class DynamicIndexedPropertyGetterByField : DynamicPropertyGetterByFieldBase
	{
		private readonly string _fieldName;
		private readonly int _index;

		public DynamicIndexedPropertyGetterByField(
			string fieldName,
			int index,
			EventBeanTypedEventFactory eventBeanTypedEventFactory,
			BeanEventTypeFactory beanEventTypeFactory)
			: base(eventBeanTypedEventFactory, beanEventTypeFactory)
		{
			_fieldName = fieldName;
			_index = index;
		}

		protected override FieldInfo DetermineField(Type clazz)
		{
			return DynamicIndexPropertyDetermineField(clazz, _fieldName);
		}

		protected override CodegenExpression DetermineFieldCodegen(
			CodegenExpressionRef clazz,
			CodegenMethodScope parent,
			CodegenClassScope codegenClassScope)
		{
			return StaticMethod(typeof(DynamicIndexedPropertyGetterByField), "DynamicIndexPropertyDetermineField", clazz, Constant(_fieldName));
		}

		protected override object Call(
			DynamicPropertyDescriptorByField descriptor,
			object underlying)
		{
			return DynamicIndexedPropertyGet(descriptor, underlying, _index);
		}

		protected override CodegenExpression CallCodegen(
			CodegenExpressionRef desc,
			CodegenExpressionRef @object,
			CodegenMethodScope parent,
			CodegenClassScope codegenClassScope)
		{
			return StaticMethod(typeof(DynamicIndexedPropertyGetterByField), "DynamicIndexedPropertyGet", desc, @object, Constant(_index));
		}

		public override bool IsExistsProperty(EventBean eventBean)
		{
			var desc = GetPopulateCache(Cache, this, eventBean.Underlying, EventBeanTypedEventFactory);
			if (desc.Field == null) {
				return false;
			}

			return DynamicIndexedPropertyExists(desc, eventBean.Underlying, _index);
		}

		public override CodegenExpression UnderlyingExistsCodegen(
			CodegenExpression underlyingExpression,
			CodegenMethodScope parent,
			CodegenClassScope codegenClassScope)
		{
			CodegenExpression memberCache = codegenClassScope.AddOrGetDefaultFieldSharable(SharableCode);
			var method = parent.MakeChild(typeof(bool), typeof(DynamicPropertyGetterByMethodOrPropertyBase), codegenClassScope)
				.AddParam(typeof(object), "@object");
			method.Block
				.DeclareVar<DynamicPropertyDescriptorByField>("desc", GetPopulateCacheCodegen(memberCache, Ref("@object"), method, codegenClassScope))
				.IfCondition(EqualsNull(ExprDotName(Ref("desc"), "Field")))
				.BlockReturn(ConstantFalse())
				.MethodReturn(
					StaticMethod(typeof(DynamicIndexedPropertyGetterByField), "DynamicIndexedPropertyExists", Ref("desc"), Ref("@object"), Constant(_index)));
			return LocalMethod(method, underlyingExpression);
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="clazz">class</param>
		/// <param name="fieldName">field</param>
		/// <returns>null or field</returns>
		public static FieldInfo DynamicIndexPropertyDetermineField(
			Type clazz,
			string fieldName)
		{
			var field = clazz.GetField(fieldName);
			if (field == null) {
				return null;
			}

			return field.FieldType.IsArray ? field : null;
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="descriptor">descriptor</param>
		/// <param name="underlying">target</param>
		/// <param name="index">idx</param>
		/// <returns>null or method</returns>
		public static object DynamicIndexedPropertyGet(
			DynamicPropertyDescriptorByField descriptor,
			object underlying,
			int index)
		{
			try {
				var array = (Array) descriptor.Field.GetValue(underlying);
				if (array == null) {
					return null;
				}

				if (array.Length <= index) {
					return null;
				}

				return array.GetValue(index);
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
		/// <param name="index">idx</param>
		/// <returns>null or method</returns>
		public static bool DynamicIndexedPropertyExists(
			DynamicPropertyDescriptorByField descriptor,
			object underlying,
			int index)
		{
			try {
				var array = (Array) descriptor.Field.GetValue(underlying);
				if (array == null) {
					return false;
				}

				if (array.Length <= index) {
					return false;
				}

				return true;
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
