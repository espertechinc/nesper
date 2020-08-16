///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Text.Json;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.@event.json.parser.core;
using com.espertech.esper.common.@internal.@event.json.parser.deserializers.forge;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.json.compiletime
{
	public class StmtClassForgeableJsonDeserializer : StmtClassForgeable
	{
		private readonly CodegenClassType _classType;
		private readonly string _className;
		private readonly CodegenNamespaceScope _namespaceScope;
		private readonly string _underlyingClassName;
		private readonly StmtClassForgeableJsonDesc _desc;

		public StmtClassForgeableJsonDeserializer(
			CodegenClassType classType,
			string className,
			CodegenNamespaceScope namespaceScope,
			string underlyingClassName,
			StmtClassForgeableJsonDesc desc)
		{
			_classType = classType;
			_className = className;
			_namespaceScope = namespaceScope;
			_underlyingClassName = underlyingClassName;
			_desc = desc;
		}

		public CodegenClass Forge(
			bool includeDebugSymbols,
			bool fireAndForget)
		{
			var classScope = new CodegenClassScope(includeDebugSymbols, _namespaceScope, _className);

			// make members
			var members = new List<CodegenTypedParam>();
			members.Add(new CodegenTypedParam(_underlyingClassName, "_value"));
			
			// --------------------------------------------------------------------------------
			// Constructor
			// --------------------------------------------------------------------------------

			var beanParam = new CodegenTypedParam(_underlyingClassName, "value", false, false);
			var ctorParams = new CodegenTypedParam[] {beanParam};
			var ctor = new CodegenCtor(typeof(StmtClassForgeableRSPFactoryProvider), classScope, ctorParams);

			ctor.Block.SuperCtor();
			ctor.Block.AssignRef(Ref("_value"), Ref("value"));

			// --------------------------------------------------------------------------------
			// Deserialize(JsonElement)
			// --------------------------------------------------------------------------------
			var deserializeMethod = CodegenMethod
				.MakeParentNode(_underlyingClassName, GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
				.AddParam(typeof(JsonElement), "element");

			foreach (var propertyPair in _desc.PropertiesThisType) {
				if (propertyPair.Value == null) { // no assignment for null values
					continue;
				}

				var fieldName = _desc.FieldDescriptorsInclSupertype.Get(propertyPair.Key).FieldName;
				var forge = _desc.Forges.Get(propertyPair.Key);

				// Deserialize a field
				var deserializeFieldExpr = forge.DeserializerForge.CodegenDeserialize(
					JsonDeserializeRefs.INSTANCE,
					deserializeMethod,
					classScope);

				// Assign the field.  If the underlying type is a "value" with properties then this will
				// just use a standard assignment.  If the underlying is dynamic (as defined by IsDynamic)
				// then we believe we need to add this to a general dictionary that masks the data.  We
				// need more information to determine if this is the right thing to do.  Maybe we should
				// just use an IExpando for these kinds of objects?
				
				deserializeMethod.Block.AssignMember(
					"_value." + fieldName,
					deserializeFieldExpr);
			}

			// What the hell is this???
			// if (_desc.IsDynamic) {
			// 	endObjectValueMethod.Block.ExprDotMethod(Ref("this"), "addGeneralJson", Ref("bean." + DYNAMIC_PROP_FIELD), Ref("name"));
			// }

			deserializeMethod.Block.MethodReturn(Ref("bean"));

			// --------------------------------------------------------------------------------
			// GetResult(JsonElement)
			// --------------------------------------------------------------------------------

			var getResultMethod = CodegenMethod.MakeParentNode(_underlyingClassName, GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope);
			getResultMethod.Block.MethodReturn(Ref("_value"));

			var properties = new CodegenClassProperties();

			// walk methods
			var methods = new CodegenClassMethods();
			CodegenStackGenerator.RecursiveBuildStack(getResultMethod, "GetResult", methods, properties);
			CodegenStackGenerator.RecursiveBuildStack(deserializeMethod, "Deserialize", methods, properties);

			var clazz = new CodegenClass(
				_classType,
				_className,
				classScope,
				members,
				ctor,
				methods,
				properties,
				EmptyList<CodegenInnerClass>.Instance);
			
			if (_desc.OptionalSupertype == null) {
				clazz.BaseList.AssignType(typeof(JsonDeserializerBase));
			}
			else {
				clazz.BaseList.AssignBaseType(_desc.OptionalSupertype.Detail.DeserializerClassName);
			}

			return clazz;
		}

		public string ClassName => _className;

		public StmtClassForgeableType ForgeableType => StmtClassForgeableType.JSON_DESERIALIZER;
	}
} // end of namespace
