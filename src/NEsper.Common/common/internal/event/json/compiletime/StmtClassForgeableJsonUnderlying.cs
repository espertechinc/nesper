///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.common.@internal.@event.json.serde;
using com.espertech.esper.common.@internal.@event.json.serializers.forge;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.@event.json.compiletime.StmtClassForgeableJsonUtil; // getCasesNumberNtoM, makeNoSuchElementDefault

namespace com.espertech.esper.common.@internal.@event.json.compiletime
{
	public class StmtClassForgeableJsonUnderlying : StmtClassForgeable
	{
		public const string DYNAMIC_PROP_FIELD = "__dyn";

		private readonly string className;
		private readonly string classNameFull;
		private readonly CodegenNamespaceScope namespaceScope;
		private readonly StmtClassForgeableJsonDesc desc;

		public StmtClassForgeableJsonUnderlying(
			string className,
			string classNameFull,
			CodegenNamespaceScope namespaceScope,
			StmtClassForgeableJsonDesc desc)
		{
			this.className = className;
			this.classNameFull = classNameFull;
			this.namespaceScope = namespaceScope;
			this.desc = desc;
		}

		public CodegenClass Forge(
			bool includeDebugSymbols,
			bool fireAndForget)
		{
			var dynamic = NeedDynamic();
			var ctor = new CodegenCtor(typeof(StmtClassForgeableJsonUnderlying), includeDebugSymbols, EmptyList<CodegenTypedParam>.Instance);
			if (dynamic) {
				ctor.Block.AssignRef(DYNAMIC_PROP_FIELD, NewInstance(typeof(LinkedHashMap<string, object>)));
			}

			var properties = new CodegenClassProperties();
			var methods = new CodegenClassMethods();
			var classScope = new CodegenClassScope(includeDebugSymbols, namespaceScope, className);

			IList<CodegenTypedParam> explicitMembers = new List<CodegenTypedParam>(desc.PropertiesThisType.Count);
			if (dynamic) {
				explicitMembers.Add(new CodegenTypedParam(typeof(IDictionary<string, object>), DYNAMIC_PROP_FIELD, false, true));
			}

			// add members
			foreach (var property in desc.PropertiesThisType) {
				var field = desc.FieldDescriptorsInclSupertype.Get(property.Key);
				explicitMembers.Add(new CodegenTypedParam(field.PropertyType, field.FieldName, false, true));
			}

			// --------------------------------------------------------------------------------
			// - NativeCount => int
			// --------------------------------------------------------------------------------

			var nativeCountProperty = CodegenProperty
				.MakePropertyNode(typeof(int), this.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
				.WithOverride();
			nativeCountProperty.GetterBlock
				.BlockReturn(Constant(desc.PropertiesThisType.Count + desc.NumFieldsSupertype));
			CodegenStackGenerator.RecursiveBuildStack(nativeCountProperty, "NativeCount", methods, properties);

			// --------------------------------------------------------------------------------
			// - TryGetNativeEntry(string, out KeyValuePair<string, object>
			// --------------------------------------------------------------------------------

			var tryGetNativeEntryMethod = CodegenMethod
				.MakeParentNode(typeof(bool), this.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
				.AddParam(typeof(string), "name")
				.AddParam(new CodegenNamedParam(typeof(KeyValuePair<string, object>), "value").WithOutputModifier())
				.WithOverride();
			MakeTryGetNativeEntry(tryGetNativeEntryMethod, classScope);
			CodegenStackGenerator.RecursiveBuildStack(tryGetNativeEntryMethod, "TryGetNativeEntry", methods, properties);

			// --------------------------------------------------------------------------------
			// - TrySetNativeValue(string, object)
			// --------------------------------------------------------------------------------

			var trySetNativeValueMethod = CodegenMethod
				.MakeParentNode(typeof(bool), this.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
				.AddParam(typeof(string), "name")
				.AddParam(typeof(object), "value")
				.WithOverride();
			MakeTrySetNativeValue(trySetNativeValueMethod, classScope);
			CodegenStackGenerator.RecursiveBuildStack(trySetNativeValueMethod, "TrySetNativeValue", methods, properties);

			// --------------------------------------------------------------------------------
			// - NativeEnumerable => IEnumerable<KeyValuePair<string, object>>
			// --------------------------------------------------------------------------------

			var nativeEnumerable = CodegenProperty
				.MakePropertyNode(typeof(IEnumerable<KeyValuePair<string, object>>), this.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
				.WithOverride();
			MakeNativeEnumerable(nativeEnumerable, classScope);
			CodegenStackGenerator.RecursiveBuildStack(nativeEnumerable, "NativeEnumerable", methods, properties);

			// --------------------------------------------------------------------------------
			// - NativeContainsKey(string)
			// --------------------------------------------------------------------------------

			var nativeContainsKeyMethod = CodegenMethod
				.MakeParentNode(typeof(bool), this.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
				.AddParam(typeof(string), "name")
				.WithOverride();
			MakeNativeContainsKey(nativeContainsKeyMethod);
			CodegenStackGenerator.RecursiveBuildStack(nativeContainsKeyMethod, "NativeContainsKey", methods, properties);

			// --------------------------------------------------------------------------------
			// - NativeWrite(Utf8JsonWriter)
			// --------------------------------------------------------------------------------

			var nativeWriteMethod = CodegenMethod
				.MakeParentNode(typeof(void), this.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
				.AddParam(typeof(JsonSerializationContext), "context")
				.WithOverride();
			MakeNativeWrite(nativeWriteMethod, classScope);
			CodegenStackGenerator.RecursiveBuildStack(nativeWriteMethod, "NativeWrite", methods, properties);

			if (!ParentDynamic() && dynamic) {
				// --------------------------------------------------------------------------------
				// AddJsonValue
				// --------------------------------------------------------------------------------

				var addJsonValueMethod = CodegenMethod
					.MakeParentNode(typeof(void), this.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
					.AddParam(typeof(string), "name")
					.AddParam(typeof(object), "value")
					.WithOverride();
				addJsonValueMethod.Block.ExprDotMethod(Ref(DYNAMIC_PROP_FIELD), "Put", Ref("name"), Ref("value"));

				CodegenStackGenerator.RecursiveBuildStack(addJsonValueMethod, "AddJsonValue", methods, properties);

				// - JsonValues => IDictionary<string, object>
				var jsonValuesProperty = CodegenProperty
					.MakePropertyNode(typeof(IDictionary<string, object>), this.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
					.WithOverride();
				jsonValuesProperty.GetterBlock.BlockReturn(Ref(DYNAMIC_PROP_FIELD));
				CodegenStackGenerator.RecursiveBuildStack(jsonValuesProperty, "JsonValues", methods, properties);
			}

			var clazz = new CodegenClass(
				CodegenClassType.JSONEVENT,
				className,
				classScope,
				explicitMembers,
				ctor,
				methods,
				properties,
				EmptyList<CodegenInnerClass>.Instance);
			
			if (desc.OptionalSupertype == null) {
				clazz.BaseList.AssignType(typeof(JsonEventObjectBase));
			}
			else {
				clazz.BaseList.AssignType(desc.OptionalSupertype.UnderlyingType);
			}

			return clazz;
		}

		public string ClassName => className;

		public StmtClassForgeableType ForgeableType => StmtClassForgeableType.JSONEVENT;

		private void MakeNativeEnumerable(
			CodegenProperty nativeEnumerable,
			CodegenClassScope classScope)
		{
			var getter = nativeEnumerable.GetterBlock;
			
			getter.DeclareVar<IList<KeyValuePair<string, object>>>("result", NewInstance<List<KeyValuePair<string, object>>>());

			foreach (var property in desc.PropertiesThisType) {
				var field = desc.FieldDescriptorsInclSupertype.Get(property.Key);
				var name = Constant(property.Key);
				var value = Ref(field.FieldName);
				var entry = NewInstance(typeof(KeyValuePair<string, object>), name, value);

				getter.ExprDotMethod(Ref("result"), "Add", entry);
			}

			// Enumerable.Concat(result, base.NativeEnumerable);

			if (nativeEnumerable.IsOverride) {
				getter.BlockReturn(
					StaticMethod(
						typeof(Enumerable),
						"Concat",
						Ref("result"),
						ExprDotName(Ref("base"), "NativeEnumerable")));
			}
			else {
				getter.BlockReturn(Ref("result"));
			}
		}

		private void MakeNativeWrite(
			CodegenMethod method,
			CodegenClassScope classScope)
		{
			if (desc.OptionalSupertype != null && !desc.OptionalSupertype.Types.IsEmpty()) {
				method.Block
					.ExprDotMethod(Ref("base"), "NativeWrite", Ref("context"));
			}

			method.Block
				.DeclareVar<Utf8JsonWriter>("writer", ExprDotName(Ref("context"), "Writer"));
			
			foreach (var property in desc.PropertiesThisType) {
				var field = desc.FieldDescriptorsInclSupertype.Get(property.Key);
				var forge = desc.Forges.Get(property.Key);
				var fieldName = field.FieldName;
				var write = forge.SerializerForge.CodegenSerialize(
					new JsonSerializerForgeRefs(Ref("context"), Ref(fieldName), Constant(property.Key)),
					method,
					classScope);
				method.Block
					.ExprDotMethod(Ref("writer"), "WritePropertyName", Constant(property.Key))
					.Expression(write);
			}
		}

		private void MakeTryGetNativeEntry(
			CodegenMethod method,
			CodegenClassScope classScope)
		{
			var toEntry = method
				.MakeChild(typeof(KeyValuePair<string, object>), this.GetType(), classScope)
				.AddParam(typeof(string), "name")
				.AddParam(typeof(object), "value");
			toEntry.Block.MethodReturn(NewInstance(typeof(KeyValuePair<string, object>), Ref("name"), Ref("value")));

			var cases = GetCasesNumberNtoM(desc);
			var switchStmt = method.Block.SwitchBlockExpressions(Ref("name"), cases, true, false);

			if (method.IsOverride) {
				switchStmt.DefaultBlock
					.BlockReturn(ExprDotMethod(Ref("base"), "TryGetNativeEntry", Ref("name"), OutputVariable("value")));
			}
			else {
				switchStmt.DefaultBlock
					.AssignRef("value", DefaultValue())
					.BlockReturn(ConstantFalse());
			}

			var index = 0;
			foreach (var property in desc.PropertiesThisType) {
				var field = desc.FieldDescriptorsInclSupertype.Get(property.Key);
				switchStmt.Blocks[index]
					.AssignRef("value", LocalMethod(toEntry, Constant(property.Key), Ref(field.FieldName)))
					.BlockReturn(ConstantTrue());
				index++;
			}
		}

		private void MakeTrySetNativeValue(
			CodegenMethod method,
			CodegenClassScope classScope)
		{
			var cases = GetCasesNumberNtoM(desc);
			var switchStmt = method.Block.SwitchBlockExpressions(Ref("name"), cases, true, false);
			
			if (method.IsOverride) {
				switchStmt
					.DefaultBlock
					.BlockReturn(ExprDotMethod(Ref("base"), "TrySetNativeValue", Ref("name"), Ref("value")));
			}
			else {
				switchStmt
					.DefaultBlock
					.BlockReturn(ConstantFalse());
			}

			var index = 0;
			foreach (var property in desc.PropertiesThisType) {
				var field = desc.FieldDescriptorsInclSupertype.Get(property.Key);
				var valueExpression = CodegenLegoCast.CastSafeFromObjectType(field.PropertyType, Ref("value"));

				switchStmt
					.Blocks[index++]
					.AssignMember(field.FieldName, valueExpression)
					.BlockReturn(ConstantTrue());
			}
		}

		private void MakeNativeContainsKey(CodegenMethod method)
		{
			if (desc.OptionalSupertype != null) {
				method.Block
					.DeclareVar<bool>("parent", ExprDotMethod(Ref("base"), "NativeContainsKey", Ref("name")))
					.IfCondition(Ref("parent"))
					.BlockReturn(ConstantTrue());
			}

			if (desc.PropertiesThisType.IsEmpty()) {
				method.Block.MethodReturn(ConstantFalse());
				return;
			}

			var names = desc.PropertiesThisType.Keys;
			var enumerator = names.GetEnumerator();
			enumerator.MoveNext();

			var or = ExprDotMethod(Ref("name"), "Equals", Constant(enumerator.Current));
			while (enumerator.MoveNext()) {
				or = Or(or, ExprDotMethod(Ref("name"), "Equals", Constant(enumerator.Current)));
			}

			if (method.IsOverride) {
				method.Block.IfCondition(or)
					.BlockReturn(ConstantTrue())
					.MethodReturn(ExprDotMethod(Ref("base"), "NativeContainsKey", Ref("name")));
			}
			else {
				method.Block.MethodReturn(or);
			}
		}

		private bool NeedDynamic()
		{
			return desc.IsDynamic && !ParentDynamic();
		}

		private bool ParentDynamic()
		{
			return desc.OptionalSupertype != null && desc.OptionalSupertype.Detail.IsDynamic;
		}
	}
} // end of namespace
