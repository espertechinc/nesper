///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.model.statement;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.common.@internal.@event.json.forge;
using com.espertech.esper.common.@internal.@event.json.parser.core;
using com.espertech.esper.common.@internal.@event.json.parser.forge;
using com.espertech.esper.common.@internal.@event.json.serde;
using com.espertech.esper.common.@internal.@event.json.serializers;
using com.espertech.esper.common.@internal.@event.json.serializers.forge;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.CodegenRelational;
using static com.espertech.esper.common.@internal.@event.json.compiletime.StmtClassForgeableJsonUtil;

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
			// Serialize(Utf8JsonWriter writer, object und);
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
		
		private CodegenMethod ForgeWriteStatic(CodegenClassScope classScope)
		{
			var writeStaticMethod = CodegenMethod
				.MakeParentNode(typeof(void), this.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
				.AddParam(typeof(JsonSerializationContext), "context")
				.AddParam(typeof(object), "underlying")
				.AddThrown(typeof(IOException))
				.WithStatic(true);
			if (_makeWriteMethod) {
				MakeNativeWrite(writeStaticMethod, classScope);
			}
			else {
				writeStaticMethod.Block.MethodThrowUnsupported(); // write method found on underlying class itself
			}

			return writeStaticMethod;
		}

		private CodegenMethod ForgeWrite(
			CodegenClassScope classScope,
			CodegenClassMethods methods,
			CodegenClassProperties properties)
		{
			var writeMethod = CodegenMethod
				.MakeParentNode(typeof(void), this.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
				.AddParam(typeof(JsonSerializationContext), "context")
				.AddParam(typeof(object), "underlying")
				.AddThrown(typeof(IOException));
			writeMethod.Block.StaticMethod(_className, "WriteStatic", Ref("writer"), Ref("underlying"));
			CodegenStackGenerator.RecursiveBuildStack(writeMethod, "Write", methods, properties);
			return writeMethod;
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
