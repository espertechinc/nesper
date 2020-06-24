///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.bytecodemodel.@base
{
	public class CodegenSetterBuilder
	{
		private readonly Type _originator;
		private readonly string _refName;
		private readonly CodegenClassScope _classScope;

		private CodegenMethod _method;
		private bool _closed;

		public CodegenSetterBuilder(
			Type returnType,
			Type originator,
			string refName,
			CodegenMethodScope parent,
			CodegenClassScope classScope)
		{
			_originator = originator;
			_refName = refName;
			_classScope = classScope;

			_method = parent.MakeChild(returnType, originator, classScope);
			_method.Block.DeclareVar(returnType, refName, NewInstance(returnType));
		}

		public CodegenSetterBuilder Constant(
			string name,
			object value)
		{
			if (value is CodegenExpression) {
				throw new ArgumentException("Expected a non-expression value, received " + value);
			}

			return SetValue(name, value == null ? ConstantNull() : CodegenExpressionBuilder.Constant(value));
		}

		public CodegenSetterBuilder Expression(
			string name,
			CodegenExpression expression)
		{
			return SetValue(name, expression);
		}

		public CodegenSetterBuilder Method(
			string name,
			Func<CodegenMethod, CodegenExpression> expressionFunc)
		{
			CodegenExpression expression = expressionFunc.Invoke(_method);
			return SetValue(name, expression == null ? ConstantNull() : expression);
		}

		public CodegenSetterBuilder MapOfConstants<T>(
			string name,
			IDictionary<string, T> values)
		{
			CodegenSetterBuilderItemConsumer<T> consumer = (
				o,
				parent,
				scope) => CodegenExpressionBuilder.Constant(o);
			return SetValue(name, BuildMap(values, consumer, _originator, _method, _classScope));
		}

		public CodegenSetterBuilder Map<TI>(
			string name,
			IDictionary<string, TI> values,
			CodegenSetterBuilderItemConsumer<TI> consumer)
		{
			return SetValue(name, BuildMap(values, consumer, _originator, _method, _classScope));
		}

		public CodegenExpression Build()
		{
			if (_closed) {
				throw new IllegalStateException("Builder already completed build");
			}

			_closed = true;
			_method.Block.MethodReturn(Ref(_refName));
			return LocalMethod(_method);
		}

		public CodegenMethod GetMethod()
		{
			return _method;
		}

		private static CodegenExpression BuildMap<TV>(
			IDictionary<string, TV> map,
			CodegenSetterBuilderItemConsumer<TV> valueConsumer,
			Type originator,
			CodegenMethod method,
			CodegenClassScope classScope)
		{
			if (map == null) {
				return ConstantNull();
			}

			if (map.IsEmpty()) {
				return EnumValue(typeof(EmptyDictionary<string, TV>), "Instance");
			}

			CodegenMethod child = method.MakeChild(typeof(IDictionary<string, TV>), originator, classScope);
			if (map.Count == 1) {
				KeyValuePair<string, TV> single = map.First();
				CodegenExpression value = BuildMapValue(single.Value, valueConsumer, originator, child, classScope);
				child.Block.MethodReturn(
					StaticMethod(
						typeof(Collections),
						"SingletonMap",
						new[] {typeof(string), typeof(TV)},
						CodegenExpressionBuilder.Constant(single.Key),
						value));
			}
			else {
				child.Block.DeclareVar(
					typeof(IDictionary<string, TV>),
					"map",
					NewInstance(typeof(LinkedHashMap<string, TV>)));
				foreach (KeyValuePair<string, TV> entry in map) {
					CodegenExpression value = BuildMapValue(entry.Value, valueConsumer, originator, child, classScope);
					child.Block.ExprDotMethod(Ref("map"), "Put", CodegenExpressionBuilder.Constant(entry.Key), value);
				}

				child.Block.MethodReturn(Ref("map"));
			}

			return LocalMethod(child);
		}

		private static CodegenExpression BuildMapValue<TV>(
			TV value,
			CodegenSetterBuilderItemConsumer<TV> valueConsumer,
			Type originator,
			CodegenMethod method,
			CodegenClassScope classScope)
		{
			if (value is IDictionary<string, TV> valueMap) {
				return BuildMap(valueMap, valueConsumer, originator, method, classScope);
			}

			return valueConsumer.Invoke(value, method, classScope);
		}

		private CodegenSetterBuilder SetValue(
			string name,
			CodegenExpression expression)
		{
			_method.Block.ExprDotMethod(Ref(_refName), "Set" + GetBeanCap(name), expression);
			return this;
		}

		private string GetBeanCap(string name)
		{
			return name.Substring(0, 1).ToUpper() + name.Substring(1);
		}
	}
} // end of namespace
