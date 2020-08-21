///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.compile.multikey.MultiKeyPlanner;

namespace com.espertech.esper.common.@internal.compile.multikey
{
	public class StmtClassForgeableMultiKey : StmtClassForgeable
	{
		private readonly string className;
		private readonly CodegenNamespaceScope _namespaceScope;
		private readonly Type[] types;
		private readonly bool lenientEquals;

		public StmtClassForgeableMultiKey(
			string className,
			CodegenNamespaceScope namespaceScope,
			Type[] types,
			bool lenientEquals)
		{
			this.className = className;
			this._namespaceScope = namespaceScope;
			this.types = types;
			this.lenientEquals = lenientEquals;
		}

		public CodegenClass Forge(
			bool includeDebugSymbols,
			bool fireAndForget)
		{
			IList<CodegenTypedParam> @params = new List<CodegenTypedParam>();
			for (int i = 0; i < types.Length; i++) {
				@params.Add(new CodegenTypedParam(types[i].GetBoxedType(), "k" + i));
			}

			CodegenCtor ctor = new CodegenCtor(typeof(StmtClassForgeableMultiKey), ClassName, includeDebugSymbols, @params);

			CodegenClassProperties properties = new CodegenClassProperties();
			CodegenClassMethods methods = new CodegenClassMethods();
			CodegenClassScope classScope = new CodegenClassScope(includeDebugSymbols, _namespaceScope, className);

			CodegenMethod hashMethod = CodegenMethod
				.MakeParentNode(typeof(int), typeof(StmtClassForgeableMultiKey), CodegenSymbolProviderEmpty.INSTANCE, classScope)
				.WithOverride();
			MakeHashMethod(types.Length, hashMethod);
			CodegenStackGenerator.RecursiveBuildStack(hashMethod, "GetHashCode", methods, properties);

			CodegenMethod equalsMethod = CodegenMethod
				.MakeParentNode(typeof(bool), typeof(StmtClassForgeableMultiKey), CodegenSymbolProviderEmpty.INSTANCE, classScope)
				.WithOverride()
				.AddParam(typeof(object), "o");
			MakeEqualsMethod(types.Length, equalsMethod);
			CodegenStackGenerator.RecursiveBuildStack(equalsMethod, "Equals", methods, properties);

			CodegenProperty numKeysProperty = CodegenProperty
				.MakePropertyNode(typeof(int), typeof(StmtClassForgeableMultiKey), CodegenSymbolProviderEmpty.INSTANCE, classScope);
			numKeysProperty.GetterBlock.BlockReturn(Constant(types.Length));
			CodegenStackGenerator.RecursiveBuildStack(numKeysProperty, "NumKeys", methods, properties);

			CodegenMethod getKeyMethod = CodegenMethod
				.MakeParentNode(typeof(object), typeof(StmtClassForgeableMultiKey), CodegenSymbolProviderEmpty.INSTANCE, classScope)
				.AddParam(typeof(int), "num");
			MakeGetKeyMethod(types.Length, getKeyMethod);
			CodegenStackGenerator.RecursiveBuildStack(getKeyMethod, "GetKey", methods, properties);

			CodegenMethod toStringMethod = CodegenMethod
				.MakeParentNode(typeof(string), typeof(StmtClassForgeableMultiKey), CodegenSymbolProviderEmpty.INSTANCE, classScope)
				.WithOverride();
			MakeToStringMethod(toStringMethod);
			CodegenStackGenerator.RecursiveBuildStack(toStringMethod, "ToString", methods, properties);

			return new CodegenClass(
				CodegenClassType.KEYPROVISIONING,
				typeof(MultiKey),
				className,
				classScope,
				EmptyList<CodegenTypedParam>.Instance, 
				ctor,
				methods,
				properties,
				EmptyList<CodegenInnerClass>.Instance);
		}

		public string ClassName {
			get { return className; }
		}

		public StmtClassForgeableType ForgeableType {
			get { return StmtClassForgeableType.MULTIKEY; }
		}

		private void MakeEqualsMethod(
			int length,
			CodegenMethod equalsMethod)
		{
			/*
			public boolean equals(Object o) {
			    if (this == o) return true;
			    if (o == null || getClass() != o.getClass()) return false;

			    SampleKey sampleKey = (SampleKey) o;

			    if (a != null ? !a.equals(sampleKey.a) : sampleKey.a != null) return false; // or Arrays.equals or deepEquals
			    return b != null ? b.equals(sampleKey.b) : sampleKey.b == null; // or Arrays.equals or deepEquals
			}
			*/

			equalsMethod.Block
				.IfCondition(EqualsIdentity(Ref("this"), Ref("o")))
				.BlockReturn(Constant(true));

			if (!lenientEquals) {
				equalsMethod.Block
					.IfCondition(
						Or(
							EqualsNull(Ref("o")),
							Not(
								EqualsIdentity(
									ExprDotMethod(Ref("this"), "GetType"),
									ExprDotMethod(Ref("o"), "GetType")))))
					.BlockReturn(Constant(false))
					.DeclareVar(className, "k", Cast(className, Ref("o")));

				for (int i = 0; i < length; i++) {
					CodegenExpressionRef self = Ref("k" + i);
					CodegenExpressionRef other = Ref("k.k" + i);
					if (i < length - 1) {
						CodegenExpression notEquals = GetNotEqualsExpression(types[i], self, other);
						equalsMethod.Block.IfCondition(notEquals).BlockReturn(ConstantFalse());
					}
					else {
						CodegenExpression equals = GetEqualsExpression(types[i], self, other);
						equalsMethod.Block.MethodReturn(equals);
					}
				}

				return;
			}

			// Lenient-equals:
			// - does not check the class
			// - pull the key value from the "getKey" method of KeyProvisioning
			// - may cast the key in case of Array.equals
			equalsMethod.Block
				.IfCondition(Not(InstanceOf(Ref("o"), typeof(MultiKeyArrayOfKeys<object>))))
				.BlockReturn(Constant(false))
				.DeclareVar<MultiKeyArrayOfKeys<object>>("k", Cast(typeof(MultiKeyArrayOfKeys<object>), Ref("o")));

			for (int i = 0; i < length; i++) {
				CodegenExpressionRef self = Ref("k" + i);
				CodegenExpression other = ExprDotMethod(Ref("k"), "Get", Constant(i));
				if (types[i].IsArray) {
					other = Cast(types[i], other);
				}

				if (i < length - 1) {
					CodegenExpression notEquals = GetNotEqualsExpression(types[i], self, other);
					equalsMethod.Block.IfCondition(notEquals).BlockReturn(ConstantFalse());
				}
				else {
					CodegenExpression equals = GetEqualsExpression(types[i], self, other);
					equalsMethod.Block.MethodReturn(equals);
				}
			}
		}

		private void MakeGetKeyMethod(
			int length,
			CodegenMethod method)
		{
			CodegenBlock[] blocks = method.Block.SwitchBlockOfLength(Ref("num"), length, true);
			for (int i = 0; i < length; i++) {
				blocks[i].BlockReturn(Ref("k" + i));
			}
		}

		private static CodegenExpression GetEqualsExpression(
			Type type,
			CodegenExpressionRef self,
			CodegenExpression other)
		{
			if (!type.IsArray) {
				CodegenExpression cond = NotEqualsNull(self);
				CodegenExpression condTrue = ExprDotMethod(self, "Equals", other);
				CodegenExpression condFalse = EqualsNull(other);
				return Conditional(cond, condTrue, condFalse);
			}

			if (RequiresDeepEquals(type.GetElementType())) {
				return StaticMethod(typeof(Arrays), "DeepEquals", self, other);
			}
			else {
				return StaticMethod(typeof(Arrays), "Equals", self, other);
			}
		}

		private static CodegenExpression GetNotEqualsExpression(
			Type type,
			CodegenExpressionRef self,
			CodegenExpression other)
		{
			if (!type.IsArray) {
				CodegenExpression cond = NotEqualsNull(self);
				CodegenExpression condTrue = Not(ExprDotMethod(self, "Equals", other));
				CodegenExpression condFalse = NotEqualsNull(other);
				return Conditional(cond, condTrue, condFalse);
			}

			if (RequiresDeepEquals(type.GetElementType())) {
				return Not(StaticMethod(typeof(Arrays), "DeepEquals", self, other));
			}
			else {
				return Not(StaticMethod(typeof(Arrays), "Equals", self, other));
			}
		}

		private void MakeHashMethod(
			int length,
			CodegenMethod hashMethod)
		{
			// <code>
			// public int GetHashCode() {
			//    int result = a != null ? a.hashCode() : 0; // (or Arrays.equals and Arrays.deepEquals)
			//    result = 31 * result + (b != null ? b.hashCode() : 0); // (or Arrays.equals and Arrays.deepEquals)
			//    return result;
			// <code>

			CodegenExpression computeHash = GetHashExpression(Ref("k0"), types[0]);
			hashMethod.Block.DeclareVar<int>("h", computeHash);

			for (int i = 1; i < length; i++) {
				computeHash = GetHashExpression(Ref("k" + i), types[i]);
				hashMethod.Block.AssignRef("h", Op(Op(Constant(31), "*", Ref("h")), "+", computeHash));
			}

			hashMethod.Block.MethodReturn(Ref("h"));
		}

		private static CodegenExpression GetHashExpression(
			CodegenExpressionRef key,
			Type type)
		{
			if (!type.IsArray) {
				return Conditional(NotEqualsNull(key), ExprDotMethod(key, "GetHashCode"), Constant(0));
			}

			if (RequiresDeepEquals(type.GetElementType())) {
				return StaticMethod(typeof(Arrays), "DeepHashCode", key);
			}
			else {
				return StaticMethod(typeof(Arrays), "HashCode", key);
			}
		}

		private void MakeToStringMethod(CodegenMethod toStringMethod)
		{
			toStringMethod.Block
				.DeclareVar<StringBuilder>("b", NewInstance(typeof(StringBuilder)))
				.ExprDotMethod(Ref("b"), "Append", Constant(typeof(MultiKeyArrayOfKeys<object>).Name + "["));
			for (int i = 0; i < types.Length; i++) {
				if (i > 0) {
					toStringMethod.Block.ExprDotMethod(Ref("b"), "Append", Constant(","));
				}

				CodegenExpressionRef self = Ref("k" + i);
				CodegenExpression text = self;
				if (types[i].IsArray) {
					text = StaticMethod(typeof(Arrays), "ToString", self);
				}

				toStringMethod.Block.ExprDotMethod(Ref("b"), "Append", text);
			}

			toStringMethod.Block
				.ExprDotMethod(Ref("b"), "Append", Constant("]"))
				.MethodReturn(ExprDotMethod(Ref("b"), "ToString"));
		}
	}
} // end of namespace
