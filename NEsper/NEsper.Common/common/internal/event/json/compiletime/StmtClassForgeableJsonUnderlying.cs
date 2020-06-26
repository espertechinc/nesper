///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.common.client.json.minimaljson;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.model.statement;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.common.@internal.@event.json.parser.forge;
using com.espertech.esper.common.@internal.@event.json.write;
using com.espertech.esper.compat.collections;


using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.CodegenRelational; // GT, LT
using static com.espertech.esper.common.@internal.@event.json.compiletime.StmtClassForgeableJsonUtil; // getCasesNumberNtoM, makeNoSuchElementDefault

namespace com.espertech.esper.common.@internal.@event.json.compiletime
{
	public class StmtClassForgeableJsonUnderlying : StmtClassForgeable
	{

		public const string DYNAMIC_PROP_FIELD = "__dyn";

		private readonly string className;
		private readonly CodegenNamespaceScope namespaceScope;
		private readonly StmtClassForgeableJsonDesc desc;

		public StmtClassForgeableJsonUnderlying(
			string className,
			CodegenNamespaceScope namespaceScope,
			StmtClassForgeableJsonDesc desc)
		{
			this.className = className;
			this.namespaceScope = namespaceScope;
			this.desc = desc;
		}

		public CodegenClass Forge(
			bool includeDebugSymbols,
			bool fireAndForget)
		{
			CodegenCtor ctor = new CodegenCtor(typeof(StmtClassForgeableJsonUnderlying), includeDebugSymbols, EmptyList<CodegenTypedParam>.Instance);
			if (NeedDynamic()) {
				ctor.Block.AssignRef(DYNAMIC_PROP_FIELD, NewInstance(typeof(LinkedHashMap<string, object>)));
			}

			CodegenClassProperties properties = new CodegenClassProperties();
			CodegenClassMethods methods = new CodegenClassMethods();
			CodegenClassScope classScope = new CodegenClassScope(includeDebugSymbols, namespaceScope, className);

			IList<CodegenTypedParam> explicitMembers = new List<CodegenTypedParam>(desc.PropertiesThisType.Count);
			if (NeedDynamic()) {
				explicitMembers.Add(new CodegenTypedParam(typeof(IDictionary<string, object>), DYNAMIC_PROP_FIELD, false, true));
			}

			// add members
			foreach (KeyValuePair<string, object> property in desc.PropertiesThisType) {
				JsonUnderlyingField field = desc.FieldDescriptorsInclSupertype.Get(property.Key);
				explicitMembers.Add(new CodegenTypedParam(field.PropertyType, field.FieldName, false, true));
			}

			// getNativeSize
			CodegenMethod getNativeSizeMethod = CodegenMethod.MakeParentNode(typeof(int), this.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope);
			getNativeSizeMethod.Block.MethodReturn(Constant(desc.PropertiesThisType.Count + desc.NumFieldsSupertype));
			CodegenStackGenerator.RecursiveBuildStack(getNativeSizeMethod, "GetNativeSize", methods, properties);

			// getNativeEntry
			CodegenMethod getNativeEntryMethod = CodegenMethod
				.MakeParentNode(typeof(KeyValuePair<object, object>), this.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
				.AddParam(typeof(int), "num");
			MakeGetNativeEntry(getNativeEntryMethod, classScope);
			CodegenStackGenerator.RecursiveBuildStack(getNativeEntryMethod, "GetNativeEntry", methods, properties);

			// getNativeEntry
			CodegenMethod getNativeKey = CodegenMethod.MakeParentNode(typeof(string), this.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
				.AddParam(typeof(int), "num");
			MakeGetNativeKey(getNativeKey);
			CodegenStackGenerator.RecursiveBuildStack(getNativeKey, "GetNativeKey", methods, properties);

			// nativeContainsKey
			CodegenMethod nativeContainsKeyMethod = CodegenMethod.MakeParentNode(typeof(bool), this.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
				.AddParam(typeof(object), "name");
			MakeNativeContainsKey(nativeContainsKeyMethod);
			CodegenStackGenerator.RecursiveBuildStack(nativeContainsKeyMethod, "NativeContainsKey", methods, properties);

			// getNativeValue
			CodegenMethod getNativeValueMethod = CodegenMethod.MakeParentNode(typeof(object), this.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
				.AddParam(typeof(int), "num");
			MakeGetNativeValueMethod(getNativeValueMethod, classScope);
			CodegenStackGenerator.RecursiveBuildStack(getNativeValueMethod, "GetNativeValue", methods, properties);

			// getNativeNum
			CodegenMethod getNativeNumMethod = CodegenMethod.MakeParentNode(typeof(int), this.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
				.AddParam(typeof(string), "name");
			MakeGetNativeNum(getNativeNumMethod, classScope);
			CodegenStackGenerator.RecursiveBuildStack(getNativeNumMethod, "GetNativeNum", methods, properties);

			// nativeWrite
			CodegenMethod nativeWriteMethod = CodegenMethod.MakeParentNode(typeof(void), this.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
				.AddParam(typeof(JsonWriter), "writer")
				.AddThrown(typeof(IOException));
			MakeNativeWrite(nativeWriteMethod, classScope);
			CodegenStackGenerator.RecursiveBuildStack(nativeWriteMethod, "NativeWrite", methods, properties);

			if (!ParentDynamic()) {
				// addJsonValue
				CodegenMethod addJsonValueMethod = CodegenMethod
					.MakeParentNode(typeof(void), this.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
					.AddParam(typeof(string), "name")
					.AddParam(typeof(object), "value");
				if (NeedDynamic()) {
					addJsonValueMethod.Block.ExprDotMethod(Ref(DYNAMIC_PROP_FIELD), "Put", Ref("name"), Ref("value"));
				}

				CodegenStackGenerator.RecursiveBuildStack(addJsonValueMethod, "AddJsonValue", methods, properties);

				// getJsonValues
				CodegenMethod getJsonValuesMethod = CodegenMethod.MakeParentNode(
					typeof(IDictionary<string, object>),
					this.GetType(),
					CodegenSymbolProviderEmpty.INSTANCE,
					classScope);
				getJsonValuesMethod.Block.MethodReturn(desc.IsDynamic ? Ref(DYNAMIC_PROP_FIELD) : PublicConstValue(typeof(Collections), "EMPTY_MAP"));
				CodegenStackGenerator.RecursiveBuildStack(getJsonValuesMethod, "GetJsonValues", methods, properties);
			}

			CodegenClass clazz = new CodegenClass(
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

		private void MakeNativeWrite(
			CodegenMethod method,
			CodegenClassScope classScope)
		{
			bool first = true;
			if (desc.OptionalSupertype != null && !desc.OptionalSupertype.Types.IsEmpty()) {
				method.Block.ExprDotMethod(Ref("super"), "nativeWrite", Ref("writer"));
				first = false;
			}

			foreach (KeyValuePair<string, object> property in desc.PropertiesThisType) {
				JsonUnderlyingField field = desc.FieldDescriptorsInclSupertype.Get(property.Key);
				JsonForgeDesc forge = desc.Forges.Get(property.Key);
				string fieldName = field.FieldName;
				if (!first) {
					method.Block.ExprDotMethod(Ref("writer"), "writeObjectSeparator");
				}

				first = false;
				CodegenExpression write = forge.WriteForge.CodegenWrite(
					new JsonWriteForgeRefs(Ref("writer"), Ref(fieldName), Constant(property.Key)),
					method,
					classScope);
				method.Block
					.ExprDotMethod(Ref("writer"), "writeMemberName", Constant(property.Key))
					.ExprDotMethod(Ref("writer"), "writeMemberSeparator")
					.Expression(write);
			}
		}

		private void MakeGetNativeNum(
			CodegenMethod method,
			CodegenClassScope classScope)
		{
			if (desc.NumFieldsSupertype > 0) {
				method.Block
					.DeclareVar(typeof(int), "parent", ExprDotMethod(Ref("super"), "getNativeNum", Ref("name")))
					.IfCondition(Relational(Ref("parent"), GT, Constant(-1)))
					.BlockReturn(Ref("parent"));
			}

			CodegenExpression[] expressions = new CodegenExpression[desc.PropertiesThisType.Count];
			foreach (KeyValuePair<string, object> property in desc.PropertiesThisType) {
				JsonUnderlyingField field = desc.FieldDescriptorsInclSupertype.Get(property.Key);
				expressions[field.PropertyNumber - desc.NumFieldsSupertype] = Constant(property.Key);
			}

			CodegenStatementSwitch switchStmt = method.Block.SwitchBlockExpressions(Ref("name"), expressions, true, false);
			for (int i = 0; i < switchStmt.Blocks.Length; i++) {
				switchStmt.Blocks[i].BlockReturn(Constant(desc.NumFieldsSupertype + i));
			}

			method.Block.MethodReturn(Constant(-1));
		}

		private void MakeGetNativeValueMethod(
			CodegenMethod method,
			CodegenClassScope classScope)
		{
			if (desc.NumFieldsSupertype > 0) {
				method.Block
					.IfCondition(Relational(Ref("num"), LT, Constant(desc.NumFieldsSupertype)))
					.BlockReturn(ExprDotMethod(Ref("super"), "getNativeValue", Ref("num")));
			}

			CodegenExpression[] cases = GetCasesNumberNtoM(desc);
			CodegenStatementSwitch switchStmt = method.Block.SwitchBlockExpressions(Ref("num"), cases, true, false);
			MakeNoSuchElementDefault(switchStmt, Ref("num"));
			int index = 0;
			foreach (KeyValuePair<string, object> property in desc.PropertiesThisType) {
				JsonUnderlyingField field = desc.FieldDescriptorsInclSupertype.Get(property.Key);
				switchStmt.Blocks[index].BlockReturn(Ref(field.FieldName));
				index++;
			}
		}

		private void MakeGetNativeEntry(
			CodegenMethod method,
			CodegenClassScope classScope)
		{
			CodegenMethod toEntry = method.MakeChild(typeof(KeyValuePair<object, object>), this.GetType(), classScope)
				.AddParam(typeof(string), "name")
				.AddParam(typeof(object), "value");
			toEntry.Block.MethodReturn(NewInstance(typeof(KeyValuePair<object, object>), Ref("name"), Ref("value")));

			if (desc.NumFieldsSupertype > 0) {
				method.Block
					.IfCondition(Relational(Ref("num"), LT, Constant(desc.NumFieldsSupertype)))
					.BlockReturn(ExprDotMethod(Ref("super"), "getNativeEntry", Ref("num")));
			}

			CodegenExpression[] cases = GetCasesNumberNtoM(desc);
			CodegenStatementSwitch switchStmt = method.Block.SwitchBlockExpressions(Ref("num"), cases, true, false);
			MakeNoSuchElementDefault(switchStmt, Ref("num"));

			int index = 0;
			foreach (KeyValuePair<string, object> property in desc.PropertiesThisType) {
				JsonUnderlyingField field = desc.FieldDescriptorsInclSupertype.Get(property.Key);
				switchStmt.Blocks[index].BlockReturn(LocalMethod(toEntry, Constant(property.Key), Ref(field.FieldName)));
				index++;
			}
		}

		private void MakeGetNativeKey(CodegenMethod method)
		{
			if (desc.NumFieldsSupertype > 0) {
				method.Block
					.IfCondition(Relational(Ref("num"), LT, Constant(desc.NumFieldsSupertype)))
					.BlockReturn(ExprDotMethod(Ref("super"), "getNativeKey", Ref("num")));
			}

			CodegenExpression[] cases = GetCasesNumberNtoM(desc);
			CodegenStatementSwitch switchStmt = method.Block.SwitchBlockExpressions(Ref("num"), cases, true, false);
			MakeNoSuchElementDefault(switchStmt, Ref("num"));

			int index = 0;
			foreach (KeyValuePair<string, object> property in desc.PropertiesThisType) {
				switchStmt.Blocks[index].BlockReturn(Constant(property.Key));
				index++;
			}
		}

		private void MakeNativeContainsKey(CodegenMethod method)
		{
			if (desc.OptionalSupertype != null) {
				method.Block
					.DeclareVar(typeof(bool), "parent", ExprDotMethod(Ref("base"), "NativeContainsKey", Ref("name")))
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

			method.Block.MethodReturn(or);
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
