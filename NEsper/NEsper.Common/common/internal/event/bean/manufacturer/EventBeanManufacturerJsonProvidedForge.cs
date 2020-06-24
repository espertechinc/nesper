///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.@event.bean.instantiator;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.bean.manufacturer
{
	/// <summary>
	/// Factory for event beans created and populate anew from a set of values.
	/// </summary>
	public class EventBeanManufacturerJsonProvidedForge : EventBeanManufacturerForge
	{
		private readonly BeanInstantiatorForge _beanInstantiator;
		private readonly JsonEventType _jsonEventType;
		private readonly WriteablePropertyDescriptor[] _properties;
		private readonly ImportService _importService;
		private readonly FieldInfo[] _writeFieldReflection;
		private readonly bool[] _primitiveType;

		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="jsonEventType">target type</param>
		/// <param name="properties">written properties</param>
		/// <param name="importService">for resolving write methods</param>
		/// <throws>EventBeanManufactureException if the write method lookup fail</throws>
		public EventBeanManufacturerJsonProvidedForge(
			JsonEventType jsonEventType,
			WriteablePropertyDescriptor[] properties,
			ImportService importService)
		{
			_jsonEventType = jsonEventType;
			_properties = properties;
			_importService = importService;

			_beanInstantiator = new BeanInstantiatorForgeByNewInstanceReflection(jsonEventType.UnderlyingType);

			_writeFieldReflection = new FieldInfo[properties.Length];

			_primitiveType = new bool[properties.Length];
			for (var i = 0; i < properties.Length; i++) {
				var propertyName = properties[i].PropertyName;
				var field = jsonEventType.Detail.FieldDescriptors.Get(propertyName);
				_writeFieldReflection[i] = field.OptionalField;
				_primitiveType[i] = properties[i].PropertyType.IsPrimitive;
			}
		}

		public EventBeanManufacturer GetManufacturer(EventBeanTypedEventFactory eventBeanTypedEventFactory)
		{
			return new EventBeanManufacturerJsonProvided(_jsonEventType, eventBeanTypedEventFactory, _properties, _importService);
		}

		public CodegenExpression Make(
			CodegenMethodScope codegenMethodScope,
			CodegenClassScope codegenClassScope)
		{
			var init = codegenClassScope.NamespaceScope.InitMethod;

			var factory = codegenClassScope.AddOrGetDefaultFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
			var beanType = codegenClassScope.AddDefaultFieldUnshared(
				true,
				typeof(EventType),
				EventTypeUtility.ResolveTypeCodegen(_jsonEventType, EPStatementInitServicesConstants.REF));

			CodegenExpressionNewAnonymousClass manufacturer = NewAnonymousClass(init.Block, typeof(EventBeanManufacturer));

			var makeUndMethod = CodegenMethod
				.MakeParentNode(typeof(object), GetType(), codegenClassScope)
				.AddParam(typeof(object[]), "properties");

			manufacturer.AddMethod("makeUnderlying", makeUndMethod);
			MakeUnderlyingCodegen(makeUndMethod, codegenClassScope);

			var makeMethod = CodegenMethod.MakeParentNode(typeof(EventBean), GetType(), codegenClassScope)
				.AddParam(typeof(object[]), "properties");
			manufacturer.AddMethod("make", makeMethod);
			makeMethod.Block
				.DeclareVar(typeof(object), "und", LocalMethod(makeUndMethod, Ref("properties")))
				.MethodReturn(ExprDotMethod(factory, "AdapterForTypedJson", Ref("und"), beanType));

			return codegenClassScope.AddDefaultFieldUnshared(true, typeof(EventBeanManufacturer), manufacturer);
		}

		private void MakeUnderlyingCodegen(
			CodegenMethod method,
			CodegenClassScope codegenClassScope)
		{
			method.Block
				.DeclareVar(_jsonEventType.UnderlyingType, "und", Cast(_jsonEventType.UnderlyingType, _beanInstantiator.Make(method, codegenClassScope)))
				.DeclareVar(typeof(object), "value", ConstantNull());

			for (var i = 0; i < _writeFieldReflection.Length; i++) {
				method.Block.AssignRef("value", ArrayAtIndex(Ref("properties"), Constant(i)));

				var targetType = _writeFieldReflection[i].FieldType;
				CodegenExpression value;
				if (targetType.IsPrimitive) {
					var caster = SimpleTypeCasterFactory.GetCaster(typeof(object), targetType);
					value = caster.Codegen(Ref("value"), typeof(object), method, codegenClassScope);
				}
				else {
					value = Cast(targetType, Ref("value"));
				}

				var set = Assign(ExprDotName(Ref("und"), _writeFieldReflection[i].Name), value);
				if (_primitiveType[i]) {
					method.Block.IfRefNotNull("value").Expression(set).BlockEnd();
				}
				else {
					method.Block.Expression(set);
				}
			}

			method.Block.MethodReturn(Ref("und"));
		}
	}
} // end of namespace
