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
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.bean.manufacturer
{
	/// <summary>
	/// Factory for Map-underlying events.
	/// </summary>
	public class EventBeanManufacturerJsonForge : EventBeanManufacturerForge
	{
		private readonly JsonEventType _jsonEventType;
		private readonly WriteablePropertyDescriptor[] _writables;

		public EventBeanManufacturerJsonForge(
			JsonEventType jsonEventType,
			WriteablePropertyDescriptor[] writables)
		{
			this._jsonEventType = jsonEventType;
			this._writables = writables;
		}

		public CodegenExpression Make(
			CodegenMethodScope codegenMethodScope,
			CodegenClassScope codegenClassScope)
		{
			var init = codegenClassScope.NamespaceScope.InitMethod;
			var factory = codegenClassScope.AddOrGetDefaultFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
			var eventType = codegenClassScope.AddDefaultFieldUnshared(
				true,
				typeof(EventType),
				EventTypeUtility.ResolveTypeCodegen(_jsonEventType, EPStatementInitServicesConstants.REF));

			CodegenExpressionNewAnonymousClass manufacturer = NewAnonymousClass(init.Block, typeof(EventBeanManufacturer));

			CodegenMethod makeUndMethod = CodegenMethod
				.MakeParentNode(typeof(object), this.GetType(), codegenClassScope)
				.AddParam(typeof(object[]), "properties");
			manufacturer.AddMethod("MakeUnderlying", makeUndMethod);
			MakeUnderlyingCodegen(makeUndMethod, codegenClassScope);

			CodegenMethod makeMethod = CodegenMethod
				.MakeParentNode(typeof(EventBean), this.GetType(), codegenClassScope)
				.AddParam(typeof(object[]), "properties");
			manufacturer.AddMethod("make", makeMethod);
			makeMethod.Block
				.DeclareVar(typeof(object), "und", LocalMethod(makeUndMethod, Ref("properties")))
				.MethodReturn(ExprDotMethod(factory, "AdapterForTypedJson", Ref("und"), eventType));

			return codegenClassScope.AddDefaultFieldUnshared(true, typeof(EventBeanManufacturer), manufacturer);
		}

		public EventBeanManufacturer GetManufacturer(EventBeanTypedEventFactory eventBeanTypedEventFactory)
		{
			int[] nativeNums = EventBeanManufacturerJson.FindPropertyIndexes(_jsonEventType, _writables);
			return new EventBeanManufacturerJson(_jsonEventType, eventBeanTypedEventFactory, nativeNums);
		}

		private void MakeUnderlyingCodegen(
			CodegenMethod method,
			CodegenClassScope codegenClassScope)
		{
			method.Block.DeclareVar(_jsonEventType.UnderlyingType, "und", NewInstance(_jsonEventType.UnderlyingType));
			for (int i = 0; i < _writables.Length; i++) {
				var field = _jsonEventType.Detail.FieldDescriptors.Get(_writables[i].PropertyName);
				var rhs = ArrayAtIndex(Ref("properties"), Constant(i));
				var fieldTypeBoxed = field.PropertyType.GetBoxedType();
				if (field.PropertyType.IsPrimitive) {
					method.Block
						.IfCondition(NotEqualsNull(rhs))
						.AssignRef(Ref("und." + field.FieldName), Cast(fieldTypeBoxed, rhs));
				}
				else {
					method.Block.AssignRef(Ref("und." + field.FieldName), Cast(fieldTypeBoxed, rhs));
				}
			}

			method.Block.MethodReturn(Ref("und"));
		}
	}
} // end of namespace
