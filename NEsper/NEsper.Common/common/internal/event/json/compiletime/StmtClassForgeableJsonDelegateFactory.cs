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
using com.espertech.esper.common.@internal.@event.json.parser.core;
using com.espertech.esper.common.@internal.@event.json.parser.forge;
using com.espertech.esper.common.@internal.@event.json.write;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.CodegenRelational;
using static com.espertech.esper.common.@internal.@event.json.compiletime.StmtClassForgeableJsonUtil;

namespace com.espertech.esper.common.@internal.@event.json.compiletime
{
	public class StmtClassForgeableJsonDelegateFactory : StmtClassForgeable
	{
		private readonly CodegenClassType _classType;
		private readonly string _className;
		private readonly bool _makeWriteMethod;
		private readonly CodegenNamespaceScope _namespaceScope;
		private readonly string _delegateClassName;
		private readonly string _underlyingClassName;
		private readonly StmtClassForgeableJsonDesc _desc;

		public StmtClassForgeableJsonDelegateFactory(
			CodegenClassType classType,
			string className,
			bool makeWriteMethod,
			CodegenNamespaceScope namespaceScope,
			string delegateClassName,
			string underlyingClassName,
			StmtClassForgeableJsonDesc desc)
		{
			this._classType = classType;
			this._className = className;
			this._makeWriteMethod = makeWriteMethod;
			this._namespaceScope = namespaceScope;
			this._delegateClassName = delegateClassName;
			this._underlyingClassName = underlyingClassName;
			this._desc = desc;
		}

		public CodegenClass Forge(
			bool includeDebugSymbols,
			bool fireAndForget)
		{
			CodegenClassProperties properties = new CodegenClassProperties();
			CodegenClassMethods methods = new CodegenClassMethods();
			CodegenClassScope classScope = new CodegenClassScope(includeDebugSymbols, _namespaceScope, _className);

			CodegenMethod makeMethod = CodegenMethod.MakeParentNode(
					typeof(JsonDeserializerBase),
					typeof(StmtClassForgeableJsonDelegateFactory),
					CodegenSymbolProviderEmpty.INSTANCE,
					classScope)
				.AddParam(typeof(JsonHandlerDelegator), "delegator")
				.AddParam(typeof(JsonDeserializerBase), "parent");
			makeMethod.Block.MethodReturn(NewInstance(_delegateClassName, Ref("delegator"), Ref("parent"), NewInstance(_underlyingClassName)));
			CodegenStackGenerator.RecursiveBuildStack(makeMethod, "Make", methods, properties);

			// write-method (applicable for nested classes)
			CodegenMethod writeMethod = CodegenMethod.MakeParentNode(typeof(void), this.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
				.AddParam(typeof(Utf8JsonWriter), "writer")
				.AddParam(typeof(object), "underlying")
				.AddThrown(typeof(IOException));
			writeMethod.Block.StaticMethod(_className, "WriteStatic", Ref("writer"), Ref("underlying"));
			CodegenStackGenerator.RecursiveBuildStack(writeMethod, "Write", methods, properties);

			// write-static-method (applicable for nested classes)
			CodegenMethod writeStaticMethod = CodegenMethod.MakeParentNode(typeof(void), this.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
				.AddParam(typeof(Utf8JsonWriter), "writer")
				.AddParam(typeof(object), "underlying")
				.AddThrown(typeof(IOException));
			writeStaticMethod.IsStatic = true;
			if (_makeWriteMethod) {
				MakeNativeWrite(writeStaticMethod, classScope);
			}
			else {
				writeStaticMethod.Block.MethodThrowUnsupported(); // write method found on underlying class itself
			}

			CodegenStackGenerator.RecursiveBuildStack(writeStaticMethod, "WriteStatic", methods, properties);

			// copy-method
			CodegenMethod copyMethod = CodegenMethod.MakeParentNode(typeof(object), this.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
				.AddParam(typeof(object), "und");
			MakeCopy(copyMethod, classScope);
			CodegenStackGenerator.RecursiveBuildStack(copyMethod, "Copy", methods, properties);

			// get-value-method
			CodegenMethod getValueMethod = CodegenMethod.MakeParentNode(typeof(object), this.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
				.AddParam(typeof(int), "num")
				.AddParam(typeof(object), "und");
			MakeGetValue(getValueMethod, classScope);
			CodegenStackGenerator.RecursiveBuildStack(getValueMethod, "GetValue", methods, properties);

			// set-value-method
			CodegenMethod setValueMethod = CodegenMethod.MakeParentNode(typeof(void), this.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
				.AddParam(typeof(int), "num")
				.AddParam(typeof(object), "value")
				.AddParam(typeof(object), "und");
			MakeSetValue(setValueMethod, classScope);
			CodegenStackGenerator.RecursiveBuildStack(setValueMethod, "SetValue", methods, properties);

			// newUnderlying-method
			CodegenMethod newUnderlyingMethod = CodegenMethod.MakeParentNode(typeof(object), this.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope);
			MakeNewUnderlyingMethod(newUnderlyingMethod);
			CodegenStackGenerator.RecursiveBuildStack(newUnderlyingMethod, "NewUnderlying", methods, properties);

			CodegenClass clazz = new CodegenClass(
				_classType,
				_className,
				classScope,
				EmptyList<CodegenTypedParam>.Instance,
				null,
				methods,
				properties,
				EmptyList<CodegenInnerClass>.Instance);

			clazz.BaseList.AssignType(typeof(JsonDelegateFactory));
			return clazz;
		}

		private void MakeNewUnderlyingMethod(CodegenMethod method)
		{
			// we know this underlying class has a default constructor otherwise it is not json and deep-class eligible
			method.Block.MethodReturn(NewInstance(_underlyingClassName));
		}

		private void MakeGetValue(
			CodegenMethod method,
			CodegenClassScope classScope)
		{
			if (_desc.NumFieldsSupertype > 0) {
				method.Block
					.IfCondition(Relational(Ref("num"), LT, Constant(_desc.NumFieldsSupertype)))
					.BlockReturn(ExprDotMethod(Cast(typeof(JsonEventObjectBase), Ref("und")), "getNativeValue", Ref("num")));
			}

			method.Block
				.DeclareVar(_underlyingClassName, "src", Cast(_underlyingClassName, Ref("und")));
			CodegenExpression[] cases = GetCasesNumberNtoM(_desc);
			CodegenStatementSwitch switchStmt = method.Block.SwitchBlockExpressions(Ref("num"), cases, true, false);
			MakeNoSuchElementDefault(switchStmt, Ref("num"));
			int index = 0;
			foreach (KeyValuePair<string, object> property in _desc.PropertiesThisType) {
				JsonUnderlyingField field = _desc.FieldDescriptorsInclSupertype.Get(property.Key);
				switchStmt.Blocks[index].BlockReturn(Ref("src." + field.FieldName));
				index++;
			}
		}

		private void MakeCopy(
			CodegenMethod method,
			CodegenClassScope classScope)
		{
			method.Block
				.DeclareVar(_underlyingClassName, "copy", NewInstance(_underlyingClassName))
				.DeclareVar(_underlyingClassName, "src", Cast(_underlyingClassName, Ref("und")));
			foreach (KeyValuePair<string, JsonUnderlyingField> field in _desc.FieldDescriptorsInclSupertype) {
				string fieldName = field.Value.FieldName;
				Type fieldType = field.Value.PropertyType;
				CodegenExpression sourceField = Ref("src." + fieldName);
				CodegenExpression rhs;
				if (fieldType.IsArray) {
					CodegenMethod arrayCopy = method.MakeChild(fieldType, this.GetType(), classScope)
						.AddParam(fieldType, "src");
					rhs = LocalMethod(arrayCopy, sourceField);
					arrayCopy.Block
						.IfRefNullReturnNull("src")
						.DeclareVar(fieldType, "copy", NewArrayByLength(fieldType.GetElementType(), ArrayLength(Ref("src"))))
						.StaticMethod(typeof(Array), "Copy", Ref("src"), Constant(0), Ref("copy"), Constant(0), Constant(0))
						.MethodReturn(Ref("copy"));
				}
				else if (fieldType == typeof(IDictionary<string, object>)) {
					CodegenMethod mapCopy = method.MakeChild(typeof(IDictionary<string, object>), this.GetType(), classScope)
						.AddParam(fieldType, "src");
					rhs = LocalMethod(mapCopy, sourceField);
					mapCopy.Block
						.IfRefNullReturnNull("src")
						.MethodReturn(NewInstance(typeof(Dictionary<string, object>), Ref("src")));
				}
				else {
					rhs = sourceField;
				}

				method.Block.AssignRef(Ref("copy." + fieldName), rhs);
			}

			method.Block.MethodReturn(Ref("copy"));
		}

		public string ClassName {
			get { return _className; }
		}

		public StmtClassForgeableType ForgeableType {
			get { return StmtClassForgeableType.JSONDELEGATEFACTORY; }
		}

		private void MakeNativeWrite(
			CodegenMethod method,
			CodegenClassScope classScope)
		{
			method.Block
				.DeclareVar(_underlyingClassName, "und", Cast(_underlyingClassName, Ref("underlying")))
				.IfRefNull("und")
				.ExprDotMethod(Ref("writer"), "writeLiteral", Constant("null"))
				.BlockReturnNoValue()
				.ExprDotMethod(Ref("writer"), "writeObjectOpen");
			bool first = true;
			foreach (KeyValuePair<string, object> property in _desc.PropertiesThisType) {
				JsonUnderlyingField field = _desc.FieldDescriptorsInclSupertype.Get(property.Key);
				JsonForgeDesc forge = _desc.Forges.Get(property.Key);
				string fieldName = field.FieldName;
				if (!first) {
					method.Block.ExprDotMethod(Ref("writer"), "writeObjectSeparator");
				}

				first = false;
				CodegenExpression write = forge.WriteForge.CodegenWrite(
					new JsonWriteForgeRefs(Ref("writer"), Ref("und." + fieldName), Constant(property.Key)),
					method,
					classScope);
				method.Block
					.ExprDotMethod(Ref("writer"), "writeMemberName", Constant(property.Key))
					.ExprDotMethod(Ref("writer"), "writeMemberSeparator")
					.Expression(write);
			}

			method.Block
				.ExprDotMethod(Ref("writer"), "writeObjectClose");
		}

		private void MakeSetValue(
			CodegenMethod method,
			CodegenClassScope classScope)
		{
			method.Block
				.DeclareVar(_underlyingClassName, "bean", Cast(_underlyingClassName, Ref("und")));

			CodegenExpression[] cases = GetCasesNumberNtoM(_desc);
			CodegenStatementSwitch switchStmt = method.Block.SwitchBlockExpressions(Ref("num"), cases, false, false);
			MakeNoSuchElementDefault(switchStmt, Ref("num"));
			CodegenBlock[] blocks = switchStmt.Blocks;

			int index = 0;
			foreach (KeyValuePair<string, object> property in _desc.PropertiesThisType) {
				JsonUnderlyingField field = _desc.FieldDescriptorsInclSupertype.Get(property.Key);
				string fieldName = "bean." + field.FieldName;

				object type = _desc.PropertiesThisType.Get(property.Key);
				if (type == null) {
					// no action
				}
				else if (type is Type) {
					Type classType = (Type) type;
					if (classType.IsPrimitive) {
						blocks[index]
							.IfRefNotNull("value")
							.AssignRef(fieldName, Cast(Boxing.GetBoxedType(classType), Ref("value")));
					}
					else {
						blocks[index].AssignRef(fieldName, Cast(classType, Ref("value")));
					}
				}
				else if (type is TypeBeanOrUnderlying) {
					EventType eventType = ((TypeBeanOrUnderlying) type).EventType;
					if (eventType is JsonEventType) {
						JsonEventType jsonEventType = (JsonEventType) eventType;
						CodegenExpression castAsBean = CastUnderlying(jsonEventType.Detail.UnderlyingClassName, Cast(typeof(EventBean), Ref("value")));
						CodegenExpression castUnd = Cast(jsonEventType.Detail.UnderlyingClassName, Ref("value"));
						blocks[index].AssignRef(fieldName, Conditional(InstanceOf(Ref("value"), typeof(EventBean)), castAsBean, castUnd));
					}
					else {
						CodegenExpression castAsBean = CastUnderlying(typeof(IDictionary<string, object>), Cast(typeof(EventBean), Ref("value")));
						CodegenExpression castUnd = Cast(typeof(IDictionary<string, object>), Ref("value"));
						blocks[index].AssignRef(fieldName, Conditional(InstanceOf(Ref("value"), typeof(EventBean)), castAsBean, castUnd));
					}
				}
				else if (type is TypeBeanOrUnderlying[]) {
					TypeBeanOrUnderlying[] typeDef = (TypeBeanOrUnderlying[]) type;
					EventType eventType = typeDef[0].EventType;
					Type arrayType = TypeHelper.GetArrayType(eventType.UnderlyingType);
					blocks[index]
						.IfRefNull("value")
						.AssignRef(fieldName, ConstantNull())
						.BlockReturnNoValue()
						.IfCondition(InstanceOf(Ref("value"), arrayType))
						.AssignRef(fieldName, Cast(arrayType, Ref("value")))
						.BlockReturnNoValue()
						.DeclareVar(typeof(EventBean[]), "events", Cast(typeof(EventBean[]), Ref("value")))
						.DeclareVar(arrayType, "array", NewArrayByLength(eventType.UnderlyingType, ArrayLength(Ref("events"))))
						.ForLoopIntSimple("i", ArrayLength(Ref("events")))
						.AssignArrayElement("array", Ref("i"), CastUnderlying(eventType.UnderlyingType, ArrayAtIndex(Ref("events"), Ref("i"))))
						.BlockEnd()
						.AssignRef(fieldName, Ref("array"));
				}
				else {
					throw new UnsupportedOperationException("Unrecognized type " + type);
				}

				index++;
			}
		}
	}
} // end of namespace
