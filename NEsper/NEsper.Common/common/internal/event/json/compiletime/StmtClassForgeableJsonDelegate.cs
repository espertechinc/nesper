///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text.Json;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.@event.json.parser.core;
using com.espertech.esper.common.@internal.@event.json.parser.delegates.endvalue;
using com.espertech.esper.common.@internal.@event.json.parser.forge;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;


using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.@event.json.compiletime.StmtClassForgeableJsonUnderlying; // DYNAMIC_PROP_FIELD

namespace com.espertech.esper.common.@internal.@event.json.compiletime
{
	public class StmtClassForgeableJsonDelegate : StmtClassForgeable
	{
		private readonly CodegenClassType classType;
		private readonly string className;
		private readonly CodegenNamespaceScope namespaceScope;
		private readonly string underlyingClassName;
		private readonly StmtClassForgeableJsonDesc desc;

		public StmtClassForgeableJsonDelegate(
			CodegenClassType classType,
			string className,
			CodegenNamespaceScope namespaceScope,
			string underlyingClassName,
			StmtClassForgeableJsonDesc desc)
		{
			this.classType = classType;
			this.className = className;
			this.namespaceScope = namespaceScope;
			this.underlyingClassName = underlyingClassName;
			this.desc = desc;
		}

		public CodegenClass Forge(
			bool includeDebugSymbols,
			bool fireAndForget)
		{
			CodegenClassScope classScope = new CodegenClassScope(includeDebugSymbols, namespaceScope, className);

			// make members
			IList<CodegenTypedParam> members = new List<CodegenTypedParam>(2);
			members.Add(new CodegenTypedParam(underlyingClassName, "bean"));

			// make ctor
			CodegenTypedParam delegatorParam = new CodegenTypedParam(typeof(JsonHandlerDelegator), "delegator", false, false);
			CodegenTypedParam parentParam = new CodegenTypedParam(typeof(JsonDeserializerBase), "parent", false, false);
			CodegenTypedParam beanParam = new CodegenTypedParam(underlyingClassName, "bean", false, false);
			IList<CodegenTypedParam> ctorParams = Arrays.AsList(delegatorParam, parentParam, beanParam);
			CodegenCtor ctor = new CodegenCtor(typeof(StmtClassForgeableRSPFactoryProvider), classScope, ctorParams);
			if (desc.OptionalSupertype != null) {
				ctor.Block.SuperCtor(Ref("delegator"), Ref("parent"), Ref("bean"));
			}
			else {
				ctor.Block.SuperCtor(Ref("delegator"), Ref("parent"));
			}

			ctor.Block.AssignRef(Ref("this.bean"), Ref("bean"));

			// startObject
			CodegenMethod startObjectMethod = CodegenMethod
				.MakeParentNode(typeof(JsonDeserializerBase), this.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
				.AddParam(typeof(string), "name");
			if (desc.OptionalSupertype != null) {
				startObjectMethod.Block
					.DeclareVar(typeof(JsonDeserializerBase), "delegate", ExprDotMethod(Ref("super"), "startObject", Ref("name")))
					.IfCondition(NotEqualsNull(Ref("delegate")))
					.BlockReturn(Ref("delegate"));
			}

			foreach (string property in desc.PropertiesThisType.Keys) {
				JsonForgeDesc forge = desc.Forges.Get(property);
				if (forge.OptionalStartObjectForge != null) {
					startObjectMethod.Block
						.IfCondition(ExprDotMethod(Ref("name"), "equals", Constant(property)))
						.BlockReturn(forge.OptionalStartObjectForge.NewDelegate(JsonDelegateRefs.INSTANCE, startObjectMethod, classScope));
				}
			}

			CodegenExpression resultStartObject = desc.IsDynamic
				? NewInstance(typeof(JsonDeserializerGenericObject), JsonDelegateRefs.INSTANCE.BaseHandler, JsonDelegateRefs.INSTANCE.This)
				: ConstantNull();
			startObjectMethod.Block.MethodReturn(resultStartObject);

			// startArray
			CodegenMethod startArrayMethod = CodegenMethod
				.MakeParentNode(typeof(JsonDeserializerBase), this.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
				.AddParam(typeof(string), "name");
			if (desc.OptionalSupertype != null) {
				startArrayMethod.Block
					.DeclareVar(typeof(JsonDeserializerBase), "delegate", ExprDotMethod(Ref("super"), "startArray", Ref("name")))
					.IfCondition(NotEqualsNull(Ref("delegate")))
					.BlockReturn(Ref("delegate"));
			}

			foreach (string property in desc.PropertiesThisType.Keys) {
				JsonForgeDesc forge = desc.Forges.Get(property);
				if (forge.OptionalStartArrayForge != null) {
					startArrayMethod.Block
						.IfCondition(ExprDotMethod(Ref("name"), "equals", Constant(property)))
						.BlockReturn(forge.OptionalStartArrayForge.NewDelegate(JsonDelegateRefs.INSTANCE, startArrayMethod, classScope));
				}
			}

			CodegenExpression resultStartArray = desc.IsDynamic
				? NewInstance(typeof(JsonDeserializerGenericArray), JsonDelegateRefs.INSTANCE.BaseHandler, JsonDelegateRefs.INSTANCE.This)
				: ConstantNull();
			startArrayMethod.Block.MethodReturn(resultStartArray);

			// endObjectValue
			CodegenMethod endObjectValueMethod = CodegenMethod.MakeParentNode(typeof(bool), this.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
				.AddParam(typeof(string), "name");
			if (desc.OptionalSupertype != null) {
				endObjectValueMethod.Block
					.DeclareVar(typeof(bool), "handled", ExprDotMethod(Ref("super"), "endObjectValue", Ref("name")))
					.IfCondition(Ref("handled"))
					.BlockReturn(ConstantTrue());
			}

			foreach (KeyValuePair<string, object> propertyPair in desc.PropertiesThisType) {
				if (propertyPair.Value == null) { // no assignment for null values
					continue;
				}

				var fieldName = desc.FieldDescriptorsInclSupertype.Get(propertyPair.Key).FieldName;
				JsonForgeDesc forge = desc.Forges.Get(propertyPair.Key);
				CodegenExpression value = forge.EndValueForge.CaptureValue(JsonEndValueRefs.INSTANCE, endObjectValueMethod, classScope);
				endObjectValueMethod.Block
					.IfCondition(ExprDotMethod(Ref("name"), "equals", Constant(propertyPair.Key)))
					.AssignRef(Ref("bean." + fieldName), value)
					.BlockReturn(ConstantTrue());
			}

			if (desc.IsDynamic) {
				endObjectValueMethod.Block.ExprDotMethod(Ref("this"), "addGeneralJson", Ref("bean." + DYNAMIC_PROP_FIELD), Ref("name"));
			}

			endObjectValueMethod.Block.MethodReturn(ConstantFalse());

			// make get-bean method
			var getResultMethod = CodegenMethod.MakeParentNode(underlyingClassName, this.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope);
			getResultMethod.Block.MethodReturn(Ref("bean"));

			// Make Deserialize(JsonElement) method
			var deserializeMethod = CodegenMethod
				.MakeParentNode(underlyingClassName, this.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
				.AddParam(typeof(JsonElement), "element");
			deserializeMethod.Block.MethodReturn(Ref("bean"));

			CodegenClassProperties properties = new CodegenClassProperties();

			// walk methods
			CodegenClassMethods methods = new CodegenClassMethods();
			CodegenStackGenerator.RecursiveBuildStack(getResultMethod, "GetResult", methods, properties);
			CodegenStackGenerator.RecursiveBuildStack(deserializeMethod, "Deserialize", methods, properties);

			CodegenClass clazz = new CodegenClass(
				classType,
				className,
				classScope,
				members,
				ctor,
				methods,
				properties,
				EmptyList<CodegenInnerClass>.Instance);
			
			if (desc.OptionalSupertype == null) {
				clazz.BaseList.AssignType(typeof(JsonDeserializerBase));
			}
			else {
				clazz.BaseList.AssignBaseType(desc.OptionalSupertype.Detail.DelegateClassName);
			}

			return clazz;
		}

		public string ClassName => className;

		public StmtClassForgeableType ForgeableType => StmtClassForgeableType.JSONDELEGATE;
	}
} // end of namespace
