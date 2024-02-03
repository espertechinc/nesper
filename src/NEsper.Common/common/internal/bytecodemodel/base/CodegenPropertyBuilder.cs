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

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.bytecodemodel.@base
{
    public class CodegenPropertyBuilder
    {
        private readonly Type _originator;
        private readonly string _refName;
        private readonly CodegenClassScope _classScope;
        private readonly bool _methodProvided;
        private CodegenMethod _method;
        private bool _closed;

        public string RefName => _refName;

        public CodegenPropertyBuilder(
            Type returnType,
            Type originator,
            string refName,
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            _originator = originator;
            _refName = refName;
            _classScope = classScope;

            _methodProvided = false;
            _method = parent.MakeChild(returnType, originator, classScope);
            _method.Block.DeclareVarNewInstance(returnType, refName);
        }

        public CodegenPropertyBuilder(
            Type returnType,
            Type originator,
            string refName,
            CodegenClassScope classScope,
            CodegenMethod method)
        {
            _originator = originator;
            _refName = refName;
            _classScope = classScope;

            _methodProvided = true;
            _method = method;
            _method.Block.DeclareVarNewInstance(returnType, refName);
        }

        public CodegenPropertyBuilder(
            Type returnType,
            Type originator,
            string refName,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenExpression initializer)
        {
            _originator = originator;
            _refName = refName;
            _classScope = classScope;
            _method = method;
            _methodProvided = true;
            method.Block.DeclareVar(returnType, refName, initializer);
        }

        public CodegenPropertyBuilder ConstantExplicit(
            string name,
            object value)
        {
            if (value is CodegenExpression) {
                throw new ArgumentException("Expected a non-expression value, received " + value);
            }

            return SetValue(name, value == null ? ConstantNull() : Constant(value));
        }

        public CodegenPropertyBuilder ConstantDefaultChecked(
            string name,
            bool value)
        {
            if (!value) {
                return this;
            }

            return SetValue(name, Constant(value));
        }

        public CodegenPropertyBuilder ConstantDefaultChecked(
            string name,
            int value)
        {
            if (value == 0) {
                return this;
            }

            return SetValue(name, Constant(value));
        }

        public CodegenPropertyBuilder ConstantDefaultChecked(
            string name,
            bool? value)
        {
            return ConstantDefaultCheckedObj(name, value);
        }

        public CodegenPropertyBuilder ConstantDefaultChecked(
            string name,
            int? value)
        {
            return ConstantDefaultCheckedObj(name, value);
        }

        public CodegenPropertyBuilder ConstantDefaultCheckedObj(
            string name,
            object value)
        {
            if (value == null) {
                return this;
            }

            return SetValue(name, Constant(value));
        }

        public CodegenPropertyBuilder ExpressionDefaultChecked(
            string name,
            CodegenExpression expression)
        {
            if (expression.Equals(ConstantNull())) {
                return this;
            }

            return SetValue(name, expression);
        }

        public CodegenPropertyBuilder Expression(
            string name,
            CodegenExpression expression)
        {
            return SetValue(name, expression);
        }

        public CodegenPropertyBuilder Method(
            string name,
            Func<CodegenMethod, CodegenExpression> expressionFunc)
        {
            var expression = expressionFunc.Invoke(_method);
            return SetValue(name, expression ?? ConstantNull());
        }

        public CodegenPropertyBuilder MapOfConstants<T>(
            string name,
            IDictionary<string, T> values)
        {
            Func<T, CodegenMethod, CodegenClassScope, CodegenExpression> consumer = (
                value,
                method,
                classScope) => Constant(value);
            
            return SetValue(name, BuildMap(values, consumer, _originator, _method, _classScope));
        }

        public CodegenPropertyBuilder Map<T>(
            string name,
            IDictionary<string, T> values,
            Func<T, CodegenMethod, CodegenClassScope, CodegenExpression> consumer)
        {
            return SetValue(name, BuildMap(values, consumer, _originator, _method, _classScope));
        }

        public CodegenExpression Build()
        {
            if (_methodProvided) {
                throw new IllegalStateException(
                    "Builder build is reserved for the case when the method is not already provided");
            }

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
            Func<TV, CodegenMethod, CodegenClassScope, CodegenExpression> valueConsumer,
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

            var child = method.MakeChild(typeof(IDictionary<string, TV>), originator, classScope);
            if (map.Count == 1) {
                var single = map.First();
                var value = BuildMapValue(single.Value, valueConsumer, originator, child, classScope);
                child.Block.MethodReturn(
                    StaticMethod(
                        typeof(Collections),
                        "SingletonMap",
                        new[] { typeof(string), typeof(TV) },
                        Constant(single.Key),
                        value));
            }
            else {
                child.Block.DeclareVar(
                    typeof(IDictionary<string, TV>),
                    "map",
                    NewInstance(typeof(LinkedHashMap<string, TV>)));
                foreach (var entry in map) {
                    var value = BuildMapValue(entry.Value, valueConsumer, originator, child, classScope);
                    child.Block.ExprDotMethod(Ref("map"), "Put", Constant(entry.Key), value);
                }

                child.Block.MethodReturn(Ref("map"));
            }

            return LocalMethod(child);
        }

        private static CodegenExpression BuildMapValue<TV>(
            TV value,
            Func<TV, CodegenMethod, CodegenClassScope, CodegenExpression> valueConsumer,
            Type originator,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            if (value is IDictionary<string, TV> valueMap) {
                return BuildMap(valueMap, valueConsumer, originator, method, classScope);
            }

            return valueConsumer.Invoke(value, method, classScope);
        }

        private CodegenPropertyBuilder SetValue(
            string name,
            CodegenExpression expression)
        {
            _method.Block.SetProperty(Ref(_refName), GetBeanCap(name), expression);
            return this;
        }

        private string GetBeanCap(string name)
        {
            return name.Substring(0, 1).ToUpper() + name.Substring(1);
        }
    }
} // end of namespace