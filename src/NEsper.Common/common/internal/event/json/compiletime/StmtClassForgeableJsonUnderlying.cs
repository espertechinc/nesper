///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.model.statement;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.common.@internal.@event.json.forge;
using com.espertech.esper.common.@internal.@event.json.serde;
using com.espertech.esper.common.@internal.@event.json.serializers.forge;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.CodegenRelational;
using static com.espertech.esper.common.@internal.@event.json.compiletime.StmtClassForgeableJsonUtil;

namespace com.espertech.esper.common.@internal.@event.json.compiletime
{
    public class StmtClassForgeableJsonUnderlying : StmtClassForgeable
    {
        public const string DYNAMIC_PROP_FIELD = "__dyn";
        
        private readonly string _className;
        private readonly string _classNameFull;
        private readonly CodegenNamespaceScope _namespaceScope;
        private readonly StmtClassForgeableJsonDesc _desc;

        public StmtClassForgeableJsonUnderlying(
            string className,
            string classNameFull,
            CodegenNamespaceScope namespaceScope,
            StmtClassForgeableJsonDesc desc)
        {
            _className = className;
			_classNameFull = classNameFull;
            _namespaceScope = namespaceScope;
            _desc = desc;
        }

        public CodegenClass Forge(
            bool includeDebugSymbols,
            bool fireAndForget)
        {
	        var needDynamic = NeedDynamic();
            var ctor = new CodegenCtor(
                typeof(StmtClassForgeableJsonUnderlying),
                includeDebugSymbols,
                EmptyList<CodegenTypedParam>.Instance);
            if (needDynamic) {
                ctor.Block.AssignRef(DYNAMIC_PROP_FIELD, NewInstance(typeof(LinkedHashMap<string, object>)));
            }

            var properties = new CodegenClassProperties();
            var methods = new CodegenClassMethods();
            var classScope = new CodegenClassScope(includeDebugSymbols, _namespaceScope, _className);
            IList<CodegenTypedParam> explicitMembers = new List<CodegenTypedParam>(_desc.PropertiesThisType.Count);
            if (needDynamic) {
                explicitMembers.Add(new CodegenTypedParam(typeof(IDictionary<string, object>), DYNAMIC_PROP_FIELD, false, true));
            }

            // add members
            foreach (var property in _desc.PropertiesThisType) {
                var field = _desc.FieldDescriptorsInclSupertype.Get(property.Key);
                explicitMembers.Add(new CodegenTypedParam(field.PropertyType, field.FieldName, false, true));
            }

            // --------------------------------------------------------------------------------
            // - NativeCount => int
            // --------------------------------------------------------------------------------

			var nativeCountProperty = CodegenProperty
				.MakePropertyNode(typeof(int), GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
				.WithOverride();
			nativeCountProperty.GetterBlock
				.BlockReturn(Constant(_desc.PropertiesThisType.Count + _desc.NumFieldsSupertype));
			CodegenStackGenerator.RecursiveBuildStack(nativeCountProperty, "NativeCount", methods, properties);

			// --------------------------------------------------------------------------------
			// - TryGetNativeEntry(int, out KeyValuePair<string, object>
			// --------------------------------------------------------------------------------

			var tryGetNativeEntryMethod = CodegenMethod
				.MakeParentNode(typeof(bool), GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
				.AddParam(typeof(int), "index")
				.AddParam(new CodegenNamedParam(typeof(KeyValuePair<string, object>), "value").WithOutputModifier())
				.WithOverride();
			MakeTryGetNativeEntry(tryGetNativeEntryMethod, classScope);
			CodegenStackGenerator.RecursiveBuildStack(tryGetNativeEntryMethod, "TryGetNativeEntry", methods, properties);

			// --------------------------------------------------------------------------------
			// - TrySetNativeValue(int, object)
			// --------------------------------------------------------------------------------

			var trySetNativeValueMethod = CodegenMethod
				.MakeParentNode(typeof(bool), GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
				.AddParam(typeof(int), "index")
				.AddParam(typeof(object), "value")
				.WithOverride();
			MakeTrySetNativeValue(trySetNativeValueMethod, classScope);
			CodegenStackGenerator.RecursiveBuildStack(trySetNativeValueMethod, "TrySetNativeValue", methods, properties);

			// --------------------------------------------------------------------------------
			// TryGetNativeKey(string propertyName, out int index)
			// --------------------------------------------------------------------------------

			var tryGetNativeKeyMethod = CodegenMethod
				.MakeParentNode(typeof(bool), GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
				.AddParam(typeof(string), "name")
				.AddParam(new CodegenNamedParam(typeof(int), "index").WithOutputModifier())
				.WithOverride();
			MakeTryGetNativeKey(tryGetNativeKeyMethod, classScope);
			CodegenStackGenerator.RecursiveBuildStack(tryGetNativeKeyMethod, "TryGetNativeKey", methods, properties);
			
			// --------------------------------------------------------------------------------
			// TryGetNativeKeyName(int index, out string propertyName)
			// --------------------------------------------------------------------------------

			var tryGetNativeKeyNameMethod = CodegenMethod
				.MakeParentNode(typeof(bool), GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
				.AddParam(typeof(int), "index")
				.AddParam(new CodegenNamedParam(typeof(string), "name").WithOutputModifier())
				.WithOverride();
			MakeTryGetNativeKeyName(tryGetNativeKeyNameMethod, classScope);
			CodegenStackGenerator.RecursiveBuildStack(tryGetNativeKeyNameMethod, "TryGetNativeKeyName", methods, properties);

			// --------------------------------------------------------------------------------
			// - NativeEnumerable => IEnumerable<KeyValuePair<int, object>>
			// --------------------------------------------------------------------------------

			var nativeEnumerable = CodegenProperty
				.MakePropertyNode(typeof(IEnumerable<KeyValuePair<int, object>>), GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
				.WithOverride();
			MakeNativeEnumerable(nativeEnumerable, classScope);
			CodegenStackGenerator.RecursiveBuildStack(nativeEnumerable, "NativeEnumerable", methods, properties);

			// --------------------------------------------------------------------------------
			// - NativeContainsKey(string)
			// --------------------------------------------------------------------------------

			var nativeContainsKeyMethod = CodegenMethod
				.MakeParentNode(typeof(bool), GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
				.AddParam(typeof(int), "index")
				.WithOverride();
			MakeNativeContainsKey(nativeContainsKeyMethod);
			CodegenStackGenerator.RecursiveBuildStack(nativeContainsKeyMethod, "NativeContainsKey", methods, properties);

			// --------------------------------------------------------------------------------
			// - NativeWrite(Utf8JsonWriter)
			// --------------------------------------------------------------------------------

			var nativeWriteMethod = CodegenMethod
				.MakeParentNode(typeof(void), GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
				.AddParam(typeof(JsonSerializationContext), "context")
				.WithOverride();
			MakeNativeWrite(nativeWriteMethod, classScope);
			CodegenStackGenerator.RecursiveBuildStack(nativeWriteMethod, "NativeWrite", methods, properties);

			if (!ParentDynamic()) {
				// --------------------------------------------------------------------------------
				// AddJsonValue
				// --------------------------------------------------------------------------------

				var addJsonValueMethod = CodegenMethod
					.MakeParentNode(typeof(void), GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
					.AddParam<string>("name")
					.AddParam<object>("value")
					.WithOverride();
                if (needDynamic) {
                    addJsonValueMethod.Block.ExprDotMethod(Ref(DYNAMIC_PROP_FIELD), "Put", Ref("name"), Ref("value"));
                }

                CodegenStackGenerator.RecursiveBuildStack(addJsonValueMethod, "AddJsonValue", methods, properties);

                // - JsonValues => IDictionary<string, object>
                var jsonValuesProperty = CodegenProperty
                    .MakePropertyNode(typeof(IDictionary<string, object>), GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                    .WithOverride();
                MakeJsonValues(needDynamic, jsonValuesProperty);
                CodegenStackGenerator.RecursiveBuildStack(jsonValuesProperty, "JsonValues", methods, properties);
            }

            var clazz = new CodegenClass(
                CodegenClassType.JSONEVENT,
                _className,
                classScope,
                explicitMembers,
                ctor,
                methods,
                properties,
                EmptyList<CodegenInnerClass>.Instance);
            if (_desc.OptionalSupertype == null) {
				clazz.BaseList.AssignType(typeof(JsonEventObjectBase));
            }
            else {
				clazz.BaseList.AssignType(_desc.OptionalSupertype.UnderlyingType);
            }

            return clazz;
        }

        private static void MakeJsonValues(
	        bool needDynamic,
	        CodegenProperty property)
        {
	        property.GetterBlock.BlockReturn(
		        needDynamic ? Ref(DYNAMIC_PROP_FIELD) : EnumValue(typeof(EmptyDictionary<string, object>), "Instance"));
        }

        private void MakeNativeEnumerable(
            CodegenProperty nativeEnumerable,
            CodegenClassScope classScope)
        {
            var getter = nativeEnumerable.GetterBlock;
			
            getter.DeclareVar<IList<KeyValuePair<int, object>>>("result", NewInstance<List<KeyValuePair<int, object>>>());

            var index = 0;
            foreach (var property in _desc.PropertiesThisType) {
                var field = _desc.FieldDescriptorsInclSupertype.Get(property.Key);
                //var name = Constant(property.Key);
                var value = Ref(field.FieldName);
                var entry = NewInstance(typeof(KeyValuePair<int, object>), Constant(index), value);

                getter.ExprDotMethod(Ref("result"), "Add", entry);
                index++;
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
            if (_desc.OptionalSupertype != null && !_desc.OptionalSupertype.Types.IsEmpty()) {
                method.Block
                    .ExprDotMethod(Ref("base"), "NativeWrite", Ref("context"));
            }

            method.Block
                .DeclareVar<Utf8JsonWriter>("writer", ExprDotName(Ref("context"), "Writer"));
			
            foreach (var property in _desc.PropertiesThisType) {
                var field = _desc.FieldDescriptorsInclSupertype.Get(property.Key);
                var forge = _desc.Forges.Get(property.Key);
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
				.MakeChild(typeof(KeyValuePair<string, object>), GetType(), classScope)
				.AddParam(typeof(string), "name")
				.AddParam(typeof(object), "value");
			toEntry.Block.MethodReturn(NewInstance(typeof(KeyValuePair<string, object>), Ref("name"), Ref("value")));
			
			var cases = GetCasesNumberNtoM(_desc);
			var switchStmt = method.Block.SwitchBlockExpressions(Ref("index"), cases, true, false);

			if (method.IsOverride) {
				switchStmt.DefaultBlock
					.BlockReturn(ExprDotMethod(Ref("base"), "TryGetNativeEntry", Ref("index"), OutputVariable("value")));
			}
			else {
				switchStmt.DefaultBlock
					.AssignRef("value", DefaultValue())
					.BlockReturn(ConstantFalse());
			}
			
			var index = 0;
			foreach (var property in _desc.PropertiesThisType) {
				var field = _desc.FieldDescriptorsInclSupertype.Get(property.Key);
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
			var cases = GetCasesNumberNtoM(_desc);
			var switchStmt = method.Block.SwitchBlockExpressions(Ref("index"), cases, true, false);
			
			if (method.IsOverride) {
				switchStmt
					.DefaultBlock
					.BlockReturn(ExprDotMethod(Ref("base"), "TrySetNativeValue", Ref("index"), Ref("value")));
			}
			else {
				switchStmt
					.DefaultBlock
					.BlockReturn(ConstantFalse());
			}

			var index = 0;
			foreach (var property in _desc.PropertiesThisType) {
				var field = _desc.FieldDescriptorsInclSupertype.Get(property.Key);
				var valueExpression = CodegenLegoCast.CastSafeFromObjectType(field.PropertyType, Ref("value"));

				switchStmt
					.Blocks[index++]
					.AssignMember(field.FieldName, valueExpression)
					.BlockReturn(ConstantTrue());
			}
		}

		/// <summary>
		/// TryGetNativeKey(string name, out int index)
		/// </summary>
		/// <param name="method"></param>
		/// <param name="classScope"></param>
		/// <exception cref="NotImplementedException"></exception>
		private void MakeTryGetNativeKey(
			CodegenMethod method,
			CodegenClassScope classScope)
		{
			// convert to an array because we need repeatable unmodifiable values
			var descPropertiesThisType = _desc.PropertiesThisType.ToList();
			
			// create the unique cases
			var cases = descPropertiesThisType
				.Select(_ => Constant(_.Key))
				.ToArray();
			
			var switchStmt = method.Block.SwitchBlockExpressions(Ref("name"), cases, true, false);
			
			if (method.IsOverride) {
				switchStmt
					.DefaultBlock
					.BlockReturn(ExprDotMethod(Ref("base"), "TryGetNativeKey", Ref("name"), OutputVariable("index")));
			}
			else {
				switchStmt
					.DefaultBlock
					.BlockReturn(ConstantFalse());
			}

			for (var ii = 0; ii < descPropertiesThisType.Count; ii++) {
				var propertyName = descPropertiesThisType[ii].Key;
				var propertyIndex = _desc.FieldDescriptorsInclSupertype.Get(propertyName).PropertyNumber;
				switchStmt.Blocks[ii]
					.AssignRef(Ref("index"), Constant(propertyIndex))
					.BlockReturn(ConstantTrue());
			}
		}
		
		/// <summary>
		/// TryGetNativeKeyName(int index, out string name)
		/// </summary>
		/// <param name="method"></param>
		/// <param name="classScope"></param>
		/// <exception cref="NotImplementedException"></exception>
		private void MakeTryGetNativeKeyName(
			CodegenMethod method,
			CodegenClassScope classScope)
		{
			// convert to an array because we need repeatable unmodifiable values
			var descPropertiesThisType = _desc.PropertiesThisType.ToList();
			
			// create the unique cases
			var cases = descPropertiesThisType
				.Select(_ => Constant(_desc.FieldDescriptorsInclSupertype.Get(_.Key).PropertyNumber))
				.ToArray();
			
			var switchStmt = method.Block.SwitchBlockExpressions(Ref("index"), cases, true, false);
			
			if (method.IsOverride) {
				switchStmt
					.DefaultBlock
					.BlockReturn(ExprDotMethod(Ref("base"), "TryGetNativeKeyName", Ref("index"), OutputVariable("name")));
			}
			else {
				switchStmt
					.DefaultBlock
					.BlockReturn(ConstantFalse());
			}

			for (var ii = 0; ii < descPropertiesThisType.Count; ii++) {
				var propertyName = descPropertiesThisType[ii].Key;
				switchStmt.Blocks[ii]
					.AssignRef(Ref("name"), Constant(propertyName))
					.BlockReturn(ConstantTrue());
			}
		}

		private void MakeNativeContainsKey(CodegenMethod method)
		{
			if (_desc.OptionalSupertype != null) {
				method.Block
					.DeclareVar<bool>("parent", ExprDotMethod(Ref("base"), "NativeContainsKey", Ref("index")))
					.IfCondition(Ref("parent"))
					.BlockReturn(ConstantTrue());
			}

			if (_desc.PropertiesThisType.IsEmpty()) {
				method.Block.MethodReturn(ConstantFalse());
				return;
			}

			var numbers = _desc.PropertiesThisType
				.Select(_ => _desc.FieldDescriptorsInclSupertype.Get(_.Key))
				.Select(_ => Op(Ref("index"), "==", Constant(_.PropertyNumber)));

			var or = numbers.Aggregate<CodegenExpression, CodegenExpression>(
				null, (current, expression) => current == null ? expression : Or(current, expression));

			if (method.IsOverride) {
				method.Block.IfCondition(or)
					.BlockReturn(ConstantTrue())
					.MethodReturn(ExprDotMethod(Ref("base"), "NativeContainsKey", Ref("index")));
			}
			else {
				method.Block.MethodReturn(or);
			}
		}

        private bool NeedDynamic()
        {
            return _desc.IsDynamic && !ParentDynamic();
        }

        private bool ParentDynamic()
        {
            return _desc.OptionalSupertype != null && _desc.OptionalSupertype.Detail.IsDynamic;
        }

        public string ClassName => _className;

        public StmtClassForgeableType ForgeableType => StmtClassForgeableType.JSONEVENT;
    }
} // end of namespace