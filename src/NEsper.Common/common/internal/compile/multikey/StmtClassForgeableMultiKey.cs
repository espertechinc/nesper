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
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.compile.multikey
{
	public class StmtClassForgeableMultiKey : StmtClassForgeable
	{
		private readonly string _className;
		private readonly CodegenNamespaceScope _namespaceScope;
		private readonly Type[] _types;
		private readonly bool _lenientEquals;

		public StmtClassForgeableMultiKey(
			string className,
			CodegenNamespaceScope namespaceScope,
			Type[] types,
			bool lenientEquals)
		{
			_className = className;
			_namespaceScope = namespaceScope;
			_types = types;
			_lenientEquals = lenientEquals;
		}

		public CodegenClass Forge(
			bool includeDebugSymbols,
			bool fireAndForget)
		{
			IList<CodegenTypedParam> @params = new List<CodegenTypedParam>();
			for (var i = 0; i < _types.Length; i++) {
				@params.Add(new CodegenTypedParam(_types[i].GetBoxedType(), "k" + i));
			}

			var ctor = new CodegenCtor(typeof(StmtClassForgeableMultiKey), ClassName, includeDebugSymbols, @params);

			var properties = new CodegenClassProperties();
			var methods = new CodegenClassMethods();
			var classScope = new CodegenClassScope(includeDebugSymbols, _namespaceScope, _className);

			var hashMethod = CodegenMethod
				.MakeParentNode(typeof(int), typeof(StmtClassForgeableMultiKey), CodegenSymbolProviderEmpty.INSTANCE, classScope)
				.WithOverride();
			MakeHashMethod(_types.Length, hashMethod);
			CodegenStackGenerator.RecursiveBuildStack(hashMethod, "GetHashCode", methods, properties);

			var equalsMethod = CodegenMethod
				.MakeParentNode(typeof(bool), typeof(StmtClassForgeableMultiKey), CodegenSymbolProviderEmpty.INSTANCE, classScope)
				.WithOverride()
				.AddParam(typeof(object), "o");
			MakeEqualsMethod(_types.Length, equalsMethod);
			CodegenStackGenerator.RecursiveBuildStack(equalsMethod, "Equals", methods, properties);

			var numKeysProperty = CodegenProperty
				.MakePropertyNode(typeof(int), typeof(StmtClassForgeableMultiKey), CodegenSymbolProviderEmpty.INSTANCE, classScope);
			numKeysProperty.GetterBlock.BlockReturn(Constant(_types.Length));
			CodegenStackGenerator.RecursiveBuildStack(numKeysProperty, "NumKeys", methods, properties);

			var getKeyMethod = CodegenMethod
				.MakeParentNode(typeof(object), typeof(StmtClassForgeableMultiKey), CodegenSymbolProviderEmpty.INSTANCE, classScope)
				.AddParam(typeof(int), "num");
			MakeGetKeyMethod(_types.Length, getKeyMethod);
			CodegenStackGenerator.RecursiveBuildStack(getKeyMethod, "GetKey", methods, properties);

			var toStringMethod = CodegenMethod
				.MakeParentNode(typeof(string), typeof(StmtClassForgeableMultiKey), CodegenSymbolProviderEmpty.INSTANCE, classScope)
				.WithOverride();
			MakeToStringMethod(toStringMethod);
			CodegenStackGenerator.RecursiveBuildStack(toStringMethod, "ToString", methods, properties);

			return new CodegenClass(
				CodegenClassType.KEYPROVISIONING,
				typeof(MultiKey),
				_className,
				classScope,
				EmptyList<CodegenTypedParam>.Instance, 
				ctor,
				methods,
				properties,
				EmptyList<CodegenInnerClass>.Instance);
		}

		public string ClassName {
			get { return _className; }
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

			if (!_lenientEquals) {
				equalsMethod.Block
					.IfCondition(
						Or(
							EqualsNull(Ref("o")),
							Not(
								EqualsIdentity(
									ExprDotMethod(Ref("this"), "GetType"),
									ExprDotMethod(Ref("o"), "GetType")))))
					.BlockReturn(Constant(false))
					.DeclareVar(_className, "k", Cast(_className, Ref("o")));

				for (var i = 0; i < length; i++) {
					var self = Ref("k" + i);
					var other = Ref("k.k" + i);
					if (i < length - 1) {
						var notEquals = GetNotEqualsExpression(_types[i], self, other);
						equalsMethod.Block.IfCondition(notEquals).BlockReturn(ConstantFalse());
					}
					else {
						var equals = GetEqualsExpression(_types[i], self, other);
						equalsMethod.Block.MethodReturn(equals);
					}
				}

				return;
			}

			// Lenient-equals:
			// - does not check the class
			// - pull the key value from the "GetKey" method of KeyProvisioning
			// - may cast the key in case of Array.equals
			equalsMethod.Block
				.IfCondition(Not(InstanceOf(Ref("o"), typeof(MultiKey))))
				.BlockReturn(Constant(false))
				.DeclareVar<MultiKey>("k", Cast(typeof(MultiKey), Ref("o")));

			for (var i = 0; i < length; i++) {
				var self = Ref("k" + i);
				var other = ExprDotMethod(Ref("k"), "GetKey", Constant(i));
				if (_types[i].IsArray) {
					other = Cast(_types[i], other);
				}

				if (i < length - 1) {
					var notEquals = GetNotEqualsExpression(_types[i], self, other);
					equalsMethod.Block
						.IfCondition(notEquals)
						.BlockReturn(ConstantFalse());
				}
				else {
					var equals = GetEqualsExpression(_types[i], self, other);
					equalsMethod.Block
						.MethodReturn(equals);
				}
			}
		}

		private void MakeGetKeyMethod(
			int length,
			CodegenMethod method)
		{
			var blocks = method.Block.SwitchBlockOfLength(Ref("num"), length, true);
			for (var i = 0; i < length; i++) {
				blocks[i].BlockReturn(Ref("k" + i));
			}
		}

		private static CodegenExpression GetEqualsExpression(
			Type type,
			CodegenExpressionRef self,
			CodegenExpression other)
		{
			if (!type.IsArray) {
				var cond = NotEqualsNull(self);
				var condTrue = StaticMethod(typeof(CompatExtensions), "DeepEquals", self, other);
				var condFalse = EqualsNull(other);
				return Conditional(cond, condTrue, condFalse);
			}

			return StaticMethod(typeof(Arrays), "DeepEquals", self, other);
		}

		private static CodegenExpression GetNotEqualsExpression(
			Type type,
			CodegenExpressionRef self,
			CodegenExpression other)
		{
			if (!type.IsArray) {
				var cond = NotEqualsNull(self);
				var condTrue = Not(StaticMethod(typeof(CompatExtensions), "DeepEquals", self, other));
				var condFalse = NotEqualsNull(other);
				return Conditional(cond, condTrue, condFalse);
			}

			return Not(StaticMethod(typeof(Arrays), "DeepEquals", self, other));
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

			var computeHash = GetHashExpression(Ref("k0"), _types[0]);
			hashMethod.Block.DeclareVar<int>("h", computeHash);

			for (var i = 1; i < length; i++) {
				computeHash = GetHashExpression(Ref("k" + i), _types[i]);
				hashMethod.Block.AssignRef("h", Op(Op(Constant(31), "*", Ref("h")), "+", computeHash));
			}

			hashMethod.Block.MethodReturn(Ref("h"));
		}

		private static CodegenExpression GetHashExpression(
			CodegenExpressionRef key,
			Type type)
		{
			// if (!type.IsArray) {
			// 	return Conditional(NotEqualsNull(key), ExprDotMethod(key, "GetHashCode"), Constant(0));
			// }

			return StaticMethod(typeof(CompatExtensions), "DeepHash", key);
		}

		private void MakeToStringMethod(CodegenMethod toStringMethod)
		{
			toStringMethod.Block
				.DeclareVar<StringBuilder>("b", NewInstance(typeof(StringBuilder)))
				.ExprDotMethod(Ref("b"), "Append", Constant(nameof(MultiKey)))
				.ExprDotMethod(Ref("b"), "Append", Constant("<"));

			for (var i = 0; i < _types.Length; i++) {
				if (i > 0) {
					toStringMethod.Block.ExprDotMethod(Ref("b"), "Append", Constant(","));
				}

				var self = Ref("k" + i);
				CodegenExpression text = self;
				if (_types[i].IsArray) {
					text = StaticMethod(typeof(CompatExtensions), "RenderAny", self);
				}

				toStringMethod.Block.ExprDotMethod(Ref("b"), "Append", text);
			}

			toStringMethod.Block
				.ExprDotMethod(Ref("b"), "Append", Constant(">"))
				.MethodReturn(ExprDotMethod(Ref("b"), "ToString"));
		}
	}
} // end of namespace
