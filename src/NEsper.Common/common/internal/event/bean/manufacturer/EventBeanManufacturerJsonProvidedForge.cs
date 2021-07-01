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
			CodegenBlock codegenBlock,
			CodegenMethodScope codegenMethodScope,
			CodegenClassScope codegenClassScope)
		{
			var init = codegenClassScope.NamespaceScope.InitMethod;
			var factory = codegenClassScope.AddOrGetDefaultFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
			var eventType = codegenClassScope.AddDefaultFieldUnshared(
				true,
				typeof(EventType),
				EventTypeUtility.ResolveTypeCodegen(_jsonEventType, EPStatementInitServicesConstants.REF));

			var makeUndFunc = new CodegenExpressionLambda(codegenBlock)
				.WithParam<object[]>("properties")
				.WithBody(block => MakeUnderlyingCodegen(block, codegenMethodScope, codegenClassScope));

			var manufacturer = NewInstance<ProxyJsonEventBeanManufacturer>(eventType, factory, makeUndFunc);

			// CodegenExpressionNewAnonymousClass manufacturer = newAnonymousClass(init.getBlock(), EventBeanManufacturer.class);
			//
			// CodegenMethod makeUndMethod = CodegenMethod.makeParentNode(Object.class, this.getClass(), codegenClassScope).addParam(Object[].class, "properties");
			// manufacturer.addMethod("makeUnderlying", makeUndMethod);
			// makeUnderlyingCodegen(makeUndMethod, codegenClassScope);
			//
			// CodegenMethod makeMethod = CodegenMethod.makeParentNode(EventBean.class, this.getClass(), codegenClassScope).addParam(Object[].class, "properties");
			// manufacturer.addMethod("make", makeMethod);
			// makeMethod.getBlock()
			// 	.declareVar(Object.class, "und", localMethod(makeUndMethod, ref("properties")))
			// 	.methodReturn(exprDotMethod(factory, "adapterForTypedJson", ref("und"), beanType));

			
			return codegenClassScope.AddDefaultFieldUnshared(true, typeof(EventBeanManufacturer), manufacturer);
		}

		private void MakeUnderlyingCodegen(
			CodegenBlock block,
			CodegenMethodScope method,
			CodegenClassScope codegenClassScope)
		{
			block
				.DeclareVar(_jsonEventType.UnderlyingType, "und", Cast(_jsonEventType.UnderlyingType, _beanInstantiator.Make(method, codegenClassScope)))
				.DeclareVar<object>("value", ConstantNull());

			for (var i = 0; i < _writeFieldReflection.Length; i++) {
				block.AssignRef("value", ArrayAtIndex(Ref("properties"), Constant(i)));

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
					block.IfRefNotNull("value").Expression(set).BlockEnd();
				}
				else {
					block.Expression(set);
				}
			}

			block.BlockReturn(Ref("und"));
		}
	}
} // end of namespace
