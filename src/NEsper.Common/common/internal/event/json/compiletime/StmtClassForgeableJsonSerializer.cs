///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Text.Json;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.@event.json.serde;
using com.espertech.esper.common.@internal.@event.json.serializers.forge;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.json.compiletime
{
	public class StmtClassForgeableJsonSerializer : StmtClassForgeable
	{
		private readonly CodegenClassType _classType;
		private readonly string _className;
		private readonly bool _makeWriteMethod;
		private readonly CodegenNamespaceScope _namespaceScope;
		private readonly string _underlyingClassName;
		private readonly StmtClassForgeableJsonDesc _desc;

		public StmtClassForgeableJsonSerializer(
			CodegenClassType classType,
			string className,
			bool makeWriteMethod,
			CodegenNamespaceScope namespaceScope,
			string underlyingClassName,
			StmtClassForgeableJsonDesc desc)
		{
			_classType = classType;
			_className = className;
			_makeWriteMethod = makeWriteMethod;
			_namespaceScope = namespaceScope;
			_underlyingClassName = underlyingClassName;
			_desc = desc;
		}

		public CodegenClass Forge(
			bool includeDebugSymbols,
			bool fireAndForget)
		{
			var properties = new CodegenClassProperties();
			var methods = new CodegenClassMethods();
			var classScope = new CodegenClassScope(includeDebugSymbols, _namespaceScope, _className);
			
			// --------------------------------------------------------------------------------
			// void Serialize(JsonSerializationContext context, object underlying)
			// --------------------------------------------------------------------------------

			var serializeMethod = CodegenMethod
				.MakeParentNode(typeof(void), this.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
				.AddParam(typeof(JsonSerializationContext), "context")
				.AddParam(typeof(object), "underlying");
			MakeSerialize(serializeMethod, classScope);
			CodegenStackGenerator.RecursiveBuildStack(serializeMethod, "Serialize", methods, properties);

			var clazz = new CodegenClass(
				_classType,
				_className,
				classScope,
				EmptyList<CodegenTypedParam>.Instance,
				null,
				methods,
				properties,
				EmptyList<CodegenInnerClass>.Instance);

			clazz.BaseList.AssignType(typeof(IJsonSerializer));
			return clazz;
		}

		private void MakeSerialize(
			CodegenMethod method,
			CodegenClassScope classScope)
		{
			if (_makeWriteMethod) {
				MakeNativeWrite(method, classScope);
			}
			else {
				method.Block.MethodThrowUnsupported(); // write method found on underlying class itself
			}
		}

		public string ClassName => _className;

		public StmtClassForgeableType ForgeableType => StmtClassForgeableType.JSON_SERIALIZER;

		private void MakeNativeWrite(
			CodegenMethod method,
			CodegenClassScope classScope)
		{
			method.Block
				.DeclareVar(typeof(Utf8JsonWriter), "writer", ExprDotName(Ref("context"), "Writer"))
				.DeclareVar(_underlyingClassName, "und", Cast(_underlyingClassName, Ref("underlying")))
				.IfRefNull("und")
				.ExprDotMethod(Ref("writer"), "WriteNullValue")
				.BlockReturnNoValue()
				.ExprDotMethod(Ref("writer"), "WriteStartObject");
			
			foreach (var property in _desc.PropertiesThisType) {
				var field = _desc.FieldDescriptorsInclSupertype.Get(property.Key);
				var forge = _desc.Forges.Get(property.Key);
				var fieldName = field.FieldName;
				var write = forge.SerializerForge.CodegenSerialize(
					new JsonSerializerForgeRefs(
						Ref("context"),
						ExprDotName(Ref("und"), fieldName),
						Constant(property.Key)),
					method,
					classScope);

				method.Block
					.ExprDotMethod(Ref("writer"), "WritePropertyName", Constant(property.Key))
					.Expression(write);
			}

			method.Block.ExprDotMethod(Ref("writer"), "WriteEndObject");
		}
	}
} // end of namespace
